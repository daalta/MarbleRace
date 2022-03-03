using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace MarbleRace.Scripts
{
    [RequireComponent(typeof(Collider2D)), UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class DisappearingObstacle : UdonSharpBehaviour
    {
        [SerializeField][Tooltip("Time it takes for the obstacle to disappear.")] private float disappearDelay = 0.5f;
        [SerializeField][Tooltip("Time it takes for the obstacle to re-appear.")] private float respawnDelay = 60f;
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private new Collider2D collider;

        [UdonSynced, FieldChangeCallback(nameof(IsHidden))] private bool isHidden = false;

        private bool IsHidden
        {
            get => isHidden;
            set
            {
                if (IsHidden == value) return;
                isHidden = value;
                if (IsHidden) SendCustomEventDelayedSeconds(nameof(_Reset), respawnDelay);
                meshRenderer.enabled = !IsHidden;
                collider.enabled = !IsHidden;
            }
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (!Networking.IsMaster) return;
            SendCustomEventDelayedSeconds(nameof(_Disappear), disappearDelay);
        }

        public void _Disappear()
        {
            SetIsHidden(true);
        }

        public void _Reset()
        {
            SetIsHidden(false);
        }
        
        private void SetIsHidden(bool value)
        {
            if (!Networking.IsMaster) return;
            IsHidden = value;
            RequestSerialization();
        }
    }
}
