using UdonSharp;
using UnityEngine;

namespace MarbleRace.Scripts
{
    /// <summary>
    /// Overrides some Physics 2D settings.
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class Physics2DSettings : UdonSharpBehaviour
    {
        [Header("VRChat ignores Physics 2D project settings. This script will allow you to set them anyway..")]
        [SerializeField] private float velocityThreshold = 0.15f;

        private void Start()
        {
            Physics2D.velocityThreshold = velocityThreshold;
        }
    }
}
