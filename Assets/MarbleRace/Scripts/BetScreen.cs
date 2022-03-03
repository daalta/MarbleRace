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
        [Header("Settings")]
        [SerializeField, Tooltip("Money earned by guessing correctly. Default to -1$ if no value is given.")]
        private int[] payouts = { 3, 2, 1};
            
        [Header("References")]
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

        private uint bettingTime;

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

        private uint bettingTimer;

        private void _StartBettingTimer()
        {
            if (bettingTimer > 0)
            {
                Debug.LogWarning("Marble Race: Betting timer was started while it was already running!");
                return;
            }

            bettingTimer = bettingTime;
            _UpdateBettingTimer();
        }
        
        public void _UpdateBettingTimer()
        {
            UpdateStatusText();
            if (bettingTimer == 0)
            {
                if (Networking.IsMaster) State = 3;
                return;
            }
            
            bettingTimer--;
            
            SendCustomEventDelayedSeconds(nameof(_UpdateBettingTimer), 1f);
        }

        private RaceManager raceManager;
        
        /// <summary>
        /// Index of the marble the local player has bet on.
        /// </summary>
        private sbyte betOnMarbleIndex = -1;

        public void _Setup(RaceManager manager, uint timeToBet)
        {
            raceManager = manager;
            bettingTime = timeToBet;
        }
        
        public void _SetupButton(int buttonIndex, string marbleName, Color marbleColor)
        {
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

            if (betOnMarbleIndex != -1) // Undo previous bet, if betting is still unlocked
            {
                betButtons[betOnMarbleIndex].HasPlacedBet = false;
            }
            
            betOnMarbleIndex = index;
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

        public void _SetPlacement(sbyte marbleIndex, sbyte placement)
        {
            betButtons[marbleIndex]._SetPlacement(placement, GetPayout(placement));
        }

        private void UpdateStatusText()
        {
            if (State == 0) statusText.text = "Not<br>started";
            if (State == 1) statusText.text = "Click<br>to bet!";
            else statusText.text = State == 3 ? "<i>Bets<br>closed</i>" : $"{bettingTimer-1}s<br>to bet";
        }

        public void _Reset()
        {
            State = 0;
            betOnMarbleIndex = (sbyte) -1;
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

        public int _GetPayout(sbyte[] placements)
        {
            var placement = placements[betOnMarbleIndex];
            return GetPayout(placement);
        }

        private int GetPayout(sbyte placement)
        {
            if (placement == 0 || placement < 0 ||placement > payouts.Length) return -1;
            return payouts[placement];
        }
    }
}
