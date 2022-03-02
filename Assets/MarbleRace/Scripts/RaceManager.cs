using System;
using JetBrains.Annotations;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

namespace MarbleRace.Scripts
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class RaceManager : UdonSharpBehaviour
    {
        [SerializeField] private Marble[] marbles;
        [SerializeField] private Spawn spawn;
        [SerializeField] private Finish finish;
        [SerializeField] private BetScreen[] betScreens;

        /// <summary>
        /// Placement of each marble at the end of the race.
        /// Length equals amount of marbles.
        /// First index indicates 1st place, and so on.
        /// -1 indicates that a marble has not yet finished the race.
        /// </summary>
        [UdonSynced] private sbyte[] racePlacement;

        private void Start()
        {
            CheckReferences();
            SetupUI();
            if (Networking.IsMaster) InitPlacement();
        }

        /// <summary>
        /// Gets the marble names and colors, and applies them to the in-game racing UI.
        /// </summary>
        private void SetupUI()
        {
            for (var marbleIndex = 0; marbleIndex < marbles.Length; marbleIndex++)
            {
                var marble = marbles[marbleIndex];
                foreach (var betScreen in betScreens)
                {
                    betScreen._Setup(this, marbleIndex, marble.name, marble._GetColor());
                }
            }
        }

        private void CheckReferences()
        {
            if (marbles == null || marbles.Length == 0)
                Debug.LogError("Marble Race: Marble array in RaceManager is null");
            if (spawn == null)
                Debug.LogError("Marble Race: Spawn reference in RaceManager is null");
            if (finish == null)
                Debug.LogError("Marble Race: Finish reference in RaceManager is null");
        }

        private void InitPlacement()
        {
            racePlacement = new sbyte[marbles.Length];
            for (var i = 0; i < racePlacement.Length; i++)
            {
                racePlacement[i] = (sbyte) -1;
            }
            RequestSerialization();
        }

        [PublicAPI]
        public void StartRace()
        {
            // if (isRaceRunning) return; TODO For debugging this is disabled.
            if (!Networking.IsMaster) SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(StartRace));
            else
            {
                RespawnMarbles();
            }
        }
        
        private void RespawnMarbles()
        {
            Debug.Log("Respawning marbles");
            for (var i = 0; i < marbles.Length; i++)
            {
                var marble = marbles[i];
                marble._Respawn(spawn._GetMarbleSpawn(i));
                marble._SetSimulatePhysics(true);
                marble._SerializeRigidbodyData();
            }
        }
        
        private sbyte GetMarbleIndex(Marble marble)
        {
            for (var i = 0; i < marbles.Length; i++)
            {
                if (marbles[i] == marble) return (sbyte) i;
            }
            return 0;
        }

        public void _Finish(Marble marble)
        {
            var marbleIndex = GetMarbleIndex(marble);

            var placement = (sbyte) -1;
            for (var i = 0; i < racePlacement.Length; i++)
            {
                if (racePlacement[i] == marbleIndex) return; // Marble already finished the race
                if (racePlacement[i] != -1) continue;
                placement = (sbyte) i;
                racePlacement[placement] = (sbyte) marbleIndex;
                break;
            }

            var payout = GetPayout(placement);

            foreach (var betScreen in betScreens)
            {
                betScreen._Finish(marbleIndex, placement, payout);
            }
            
            Debug.Log($"Marble Race: {marble.gameObject.name} has finished in place {placement}!");
        }

        private int GetPayout(sbyte placement)
        {
            switch (placement)
            {
                case 0: return 4;
                case 1: return 3;
                case 2: return 2;
                default: return 0;
            }
        }

        public void _OnBetPlaced()
        {
            Debug.Log("Marble Race: A player has placed a bet. Starting race because betting ain't done yet.");
            StartRace();
        }
    }
}
