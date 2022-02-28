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
    }
}
