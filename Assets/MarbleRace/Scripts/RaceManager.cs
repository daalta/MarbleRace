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
        private Vector2[] spawnLocations;

        private void Start()
        {
            SaveMarbleSpawns();
        }

        private void SaveMarbleSpawns()
        {
            spawnLocations = new Vector2[marbles.Length];
            for (var i = 0; i < marbles.Length; i++)
            {
                spawnLocations[i] = marbles[i].transform.position;
            }
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
                marble._Respawn(spawnLocations[i]);
                marble._SetSimulatePhysics(true);
                marble._SerializeRigidbodyData();
            }
        }

        public void _Finish(Marble marble)
        {
            Debug.Log($"Marble Race: {marble.gameObject.name} has finished!");
        }
    }
}
