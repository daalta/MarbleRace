using System;
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;

namespace MarbleRace.Scripts
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class BetScreen : UdonSharpBehaviour
    {
        [SerializeField] private BetButton[] betButtons;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private Animator animator;

        /// <summary>
        /// 0 = Hidden
        /// 1 = Betting open (no timer)
        /// 2 = Betting open (timer running)
        /// 3 = Betting over (timer ran out), show payout if available
        /// </summary>
        [UdonSynced, FieldChangeCallback(nameof(State))]private int state;

        public int State
        {
            get => state;
            set
            {
                if (State == value) return;
                state = value;
                OnStateChanged();
            }
        }

        private void OnStateChanged()
        {
            animator.SetInteger("State", state);
            var isBettingClosed = state == 0 || state == 3; 
            LockAllButtons(isBettingClosed);
            UpdateStatusText();
            if (state == 2) _StartBettingTimer();
            if (Networking.IsMaster) RequestSerialization();
        }

        private int bettingTimer;

        private void _StartBettingTimer()
        {
            if (bettingTimer > 0)
            {
                Debug.LogWarning("Marble Race: Betting timer was started while it was already running!");
                return;
            }

            bettingTimer = 10;
            _UpdateBettingTimer();
        }
        
        public void _UpdateBettingTimer()
        {

            bettingTimer--;
            UpdateStatusText();
            if (bettingTimer == 0)
            {
                if (Networking.IsMaster) State = 3;
                return;
            }
            SendCustomEventDelayedSeconds(nameof(_UpdateBettingTimer), 1f);
        }

        private RaceManager raceManager;
        
        /// <summary>
        /// Index of the marble the local player has bet on.
        /// </summary>
        private sbyte bet = -1;

        public void _Setup(RaceManager manager, int buttonIndex, string marbleName, Color marbleColor)
        {
            raceManager = manager;
            betButtons[buttonIndex]._Setup(
                this,
                (sbyte) buttonIndex,
                marbleName,
                marbleColor);
            UpdateStatusText();
        }

        public void _Press(sbyte index)
        {
            if (State == 0)
            {
                Debug.Log("Marble Race: Bets have not yet started, but player tried to bet.");
                return;
            }
            
            if (State == 3)
            {
                Debug.Log("Marble Race: Betting is locked, but player tried to bet.");
                return;
            }

            if (bet != -1) // Undo previous bet, if betting is still unlocked
            {
                betButtons[bet].HasPlacedBet = false;
            }
            
            bet = index;
            betButtons[index].HasPlacedBet = true;

            raceManager.OnBetPlaced();
        }

        private void LockAllButtons(bool b)
        {
            foreach (var button in betButtons)
            {
                button._SetIsLocked(b);
            }
        }

        public void _SetPlacement(sbyte marbleIndex, sbyte placement, int payout)
        {
            betButtons[marbleIndex]._SetPlacement(placement, payout);
        }

        private void UpdateStatusText()
        {
            if (State == 0) statusText.text = "Not<br>started";
            else statusText.text = State == 3 ? "<i>Bets<br>closed</i>" : $"{bettingTimer-1}<br>Bet!!";
        }

        public void _Reset()
        {
            State = 0;
            bet = (sbyte) -1;
            foreach (var button in betButtons)
            {
                button._ClearPlacement();
                button.HasPlacedBet = false;
            }
            RequestSerialization();
        }

        public void _StartBettingWithoutTimer()
        {
            if (State != 0)
            {
                Debug.LogWarning("Marble Race: Bet screen was not properly reset!");
            }

            State = 1;
        }

        public void _StartBetting()
        {
            if (state >= 2)
            {
                Debug.LogWarning("Marble Race: Bet screen was in an improper state when betting was started.");
            }
            
            State = 2;
        }
    }
}
