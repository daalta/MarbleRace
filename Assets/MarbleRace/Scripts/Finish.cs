using System;
using UdonSharp;
using UnityEngine;

namespace MarbleRace.Scripts
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None), RequireComponent(typeof(BoxCollider2D))]
    public class Finish : UdonSharpBehaviour
    {
        private RaceManager raceManager;

        private void Start()
        {
            raceManager = GetComponentInParent<RaceManager>();
            if (raceManager == null)
            {
                Debug.LogError("Marble Race: Error! Finish couldn't find RaceManager. " +
                               "RaceManager script should be on the parent GameObject of the Finish script.");
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var marble = other.GetComponent<Marble>();
            if (marble == null)
            {
                Debug.Log("Marble Race: Finish was entered by something that's not a marble? Weird.");
                return;
            }

            raceManager._Finish(marble);
        }
    }
}
