using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace MarbleRace.Scripts
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual), RequireComponent(typeof(Rigidbody2D))]
    public class SpinObstacle : UdonSharpBehaviour
    {
        [SerializeField] private float spinSpeed = 1f;

        [UdonSynced] private float syncedRotation;
        
        private Rigidbody2D rigidbody2d;
        
        void Start()
        {
            rigidbody2d = GetComponent<Rigidbody2D>();
            rigidbody2d.angularVelocity = -spinSpeed;
            transform.rotation = Quaternion.identity;
            _SyncRotation();
        }

        public override void OnDeserialization()
        {
            _SyncRotation();
        }

        public void _SyncRotation()
        {
            if (rigidbody2d == null) return;
            if (Networking.IsMaster)
            {
                syncedRotation = rigidbody2d.rotation;
                RequestSerialization();
            }
            else
            {
                rigidbody2d.SetRotation(syncedRotation);
            }
            
            SendCustomEventDelayedSeconds(nameof(_SyncRotation), UnityEngine.Random.Range(10f, 30f));
        }
    }
}
