using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace MarbleRace.Scripts
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class Marble : UdonSharpBehaviour
    {
        [SerializeField] private new Rigidbody2D rigidbody;
        [UdonSynced] private Vector2 position;
        [UdonSynced] private Vector2 velocity;
        [UdonSynced, FieldChangeCallback(nameof(SimulatePhysics))] private bool simulatePhysics;

        private bool SimulatePhysics
        {
            get => simulatePhysics;
            set
            {
                if (value == simulatePhysics) return;
                simulatePhysics = value;
                rigidbody.simulated = value;
            }
        }
        
        private void OnCollisionEnter2D(Collision2D other)
        {
            SendRigidbody();
        }

        private void SendRigidbody()
        {
            if (!Networking.IsMaster)
            {
                Debug.LogWarning("Marble race: Non-master tried to send rigidbody variables.");
            }

            _SerializeRigidbodyData();
        }

        public void _SerializeRigidbodyData()
        {
            position = rigidbody.position;
            velocity = rigidbody.velocity;
            RequestSerialization();
        }

        public override void OnDeserialization()
        {
            if (Networking.IsMaster) return; // Master already has the values
            rigidbody.position = position;
            rigidbody.velocity = velocity;
        }

        public void _Respawn(Vector2 spawnLocation)
        {
            if (!Networking.IsMaster) return;
            rigidbody.position = spawnLocation;
            rigidbody.velocity = Vector2.zero;
        }

        public void _SetSimulatePhysics(bool b)
        {
            if (!Networking.IsMaster) return;
            SimulatePhysics = b;
        }
    }
}
