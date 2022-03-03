using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace MarbleRace.Scripts
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual), RequireComponent(typeof(Rigidbody2D))]
    public class SpinObstacle : UdonSharpBehaviour
    {
        [SerializeField] private float spinSpeed = 1f;
        
        private Rigidbody2D rigidbody2d;

        [UdonSynced] private float syncedRotation;

        void Start()
        {
            rigidbody2d = GetComponent<Rigidbody2D>();
            rigidbody2d.angularVelocity = spinSpeed;
            transform.rotation = Quaternion.identity;
            _SendSyncedRotation();
        }

        public override void OnDeserialization()
        {
            SyncRotation();
        }

        public void _SendSyncedRotation()
        {
            if (rigidbody2d == null) return;
            
            if (Networking.IsMaster)
            {
                syncedRotation = rigidbody2d.rotation;
                RequestSerialization();
            } else SyncRotation();
            
            SendCustomEventDelayedSeconds(nameof(_SendSyncedRotation), 30f);
        }

        private void SyncRotation()
        {
            rigidbody2d.SetRotation(syncedRotation);
        }
    }
}
