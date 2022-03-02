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
        [SerializeField] private Animator startGameButton;
        

        /// <summary>
        /// Placement of each marble at the end of the race.
        /// Length equals amount of marbles.
        /// Each index corresponds to a marble in the marbles. 
        /// -1 indicates that a marble has not yet finished the race.
        /// </summary>
        [UdonSynced, FieldChangeCallback(nameof(RacePlacement))] private sbyte[] racePlacement;

        [UdonSynced, FieldChangeCallback(nameof(IsGameRunning))] private bool isGameRunning;

        private bool IsGameRunning
        {
            get => isGameRunning;
            set
            {
                if (IsGameRunning == value) return;
                isGameRunning = value;
                startGameButton.SetBool("IsGameRunning", IsGameRunning);
            }
        }

        public sbyte[] RacePlacement
        {
            get => racePlacement;
            set
            {
                racePlacement = value;
                OnRacePlacementChanged();
            }
        }

        private void OnRacePlacementChanged()
        {
            for (sbyte marbleIndex = 0; marbleIndex < RacePlacement.Length; marbleIndex++)
            {
                var placement = RacePlacement[marbleIndex];
                var payout = GetPayout(placement);
                foreach (var betScreen in betScreens)
                {
                    betScreen._SetPlacement(marbleIndex, placement, payout);
                }
            }
        }

        private void Start()
        {
            CheckReferences();
            if (Networking.IsMaster) InitPlacement();
            SetupUI();
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
            RacePlacement = new sbyte[marbles.Length];
            for (var i = 0; i < RacePlacement.Length; i++)
            {
                RacePlacement[i] = (sbyte) -1;
            }
            RequestSerialization();
            OnRacePlacementChanged();
        }

        
        /// <summary>
        /// Start the initial betting before the race begins.
        /// After betting is concluded, the race begins.
        /// </summary>
        [PublicAPI]
        public void StartPreRace()
        {
            if (IsGameRunning)
            {
                Debug.LogWarning("Marble Race: Already running!");
                return;
            }

            if (!Networking.IsMaster)
            {
                SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(StartPreRace));
                return;
            }
            FreezeMarbles(true);
            RespawnMarbles();
            ResetBetScreens();
            betScreens[0]._StartBettingWithoutTimer();
            IsGameRunning = true;
            RequestSerialization();
        }

        private void ResetBetScreens()
        {
            foreach (var betScreen in betScreens)
            {
                betScreen._Reset();
            }
        }

        public void _StartRace()
        {
            // if (isRaceRunning) return; TODO For debugging this is disabled.
            if (!Networking.IsMaster) return;
            FreezeMarbles(false);
            InitPlacement();
        }
        
        private void RespawnMarbles()
        {
            Debug.Log("Respawning marbles");
            for (var i = 0; i < marbles.Length; i++)
            {
                var marble = marbles[i];
                marble._Respawn(spawn._GetMarbleSpawn(i));
                marble._SerializeRigidbodyData();
            }
        }

        private void FreezeMarbles(bool b)
        {
            for (var i = 0; i < marbles.Length; i++)
            {
                var marble = marbles[i];
                marble._SetSimulatePhysics(!b);
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
            if (RacePlacement[marbleIndex] != -1) return; // Already finished
            
            var placement = GetNextPlacement();
            Debug.Log(marbleIndex + " finished at " + placement);
            RacePlacement[marbleIndex] = placement;

            if (placement > 2 && IsGameRunning) EndRace();
            
            RequestSerialization();
            OnRacePlacementChanged();
        }

        /// <summary>
        /// Returns the next placement a marble reaching the finish would receive.
        /// </summary>
        /// <returns>Placement of the next marble to reach the finish.</returns>
        private sbyte GetNextPlacement()
        {
            var highestPlacement = -1;
            foreach (var placement in RacePlacement)
            {
                if (highestPlacement < placement) highestPlacement = placement;
            }

            return (sbyte) (highestPlacement + 1);
        }

        private void EndRace()
        {
            Debug.Log("Marble Race: Top 3 have finished, race is over!");
            IsGameRunning = false;
            RequestSerialization();
            // TODO End betting if it hasn't already concluded
        }

        private int GetPayout(sbyte placement)
        {
            switch (placement)
            {
                case 0: return 3;
                case 1: return 2;
                case 2: return 1;
                default: return -1;
            }
        }

        public void OnBetPlaced()
        {
            Debug.Log("Marble Race: A player has placed a bet.");
            SendCustomEventDelayedSeconds(nameof(_StartRace), 10);
            if (!Networking.IsMaster) SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(OnBetPlaced));
            else
            {
                betScreens[0]._StartBetting();
            }
        }
    }
}
