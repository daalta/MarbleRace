using System;
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

        private bool isRotationSyncQueued = false;

        void Start()
        {
            rigidbody2d = GetComponent<Rigidbody2D>();
            rigidbody2d.angularVelocity = spinSpeed;
            RotationFix();
        }

        /// <summary>
        /// If this isn't done, the rigidbody freaks out. Very likely a Unity bug.
        /// </summary>
        private void RotationFix()
        {
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
        }

        private void OnEnable()
        {
            _UpdateRotation();
        }

        public override void OnDeserialization()
        {
            SyncRotation();
        }

        private void QueueRotationSync()
        {
            if (isRotationSyncQueued) return;
            isRotationSyncQueued = true;
            SendCustomEventDelayedSeconds(nameof(_UpdateRotation), 30f);
        }

        public void _UpdateRotation()
        {
            if (rigidbody2d == null) return;
            
            if (Networking.IsMaster)
            {
                syncedRotation = rigidbody2d.rotation;
                RequestSerialization();
            } else SyncRotation();

            QueueRotationSync();
        }

        private void SyncRotation()
        {
            rigidbody2d.SetRotation(syncedRotation);
        }
    }
}
