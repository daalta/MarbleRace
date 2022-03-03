using System;
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace MarbleRace.Scripts
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None), RequireComponent(typeof(BoxCollider2D))]
    public class Finish : UdonSharpBehaviour
    {
        [SerializeField, Tooltip("Shows how much money the player has.")] private TextMeshProUGUI textMoney;
        
        private RaceManager raceManager;

        private void Start()
        {
            _SetMoney(0);
        }

        public void Setup(RaceManager r)
        {
            raceManager = r;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!Networking.IsMaster) return;
            var marble = other.GetComponent<Marble>();
            if (marble == null)
            {
                Debug.Log("Marble Race: Finish was entered by something that's not a marble? Weird.");
                return;
            }

            raceManager._Finish(marble);
        }

        public void _SetMoney(int money)
        {
            textMoney.text = "<size=50%>You<br>have</size><br>" + money.ToString() + "$";
        }
    }
}
