using System;
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;

namespace MarbleRace.Scripts
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class BetScreen : UdonSharpBehaviour
    {
        [SerializeField] private BetButton[] betButtons;
        [SerializeField] private TextMeshProUGUI statusText;
        //[SerializeField] private Animator animator;
        
        /// <summary>
        /// Whether bets can be placed.
        /// </summary>
        [UdonSynced, FieldChangeCallback(nameof(IsLocked))] private bool isLocked = false;

        public bool IsLocked
        {
            get => isLocked;
            set
            {
                if (IsLocked == value) return;
                isLocked = value;
                LockAllButtons(IsLocked);
                UpdateStatusText();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(HasBettingStarted))] private bool hasBettingStarted = false;
        
        public bool HasBettingStarted
        {
            get => hasBettingStarted;
            set
            {
                if (HasBettingStarted == value) return;
                hasBettingStarted = value;
                _StartBettingTimer();
                //animator.SetBool("HasBettingStarted", HasBettingStarted);
            }
        }

        private int bettingTimer;

        public void _StartBettingTimer()
        {
            if (bettingTimer == 0) bettingTimer = 10;
            UpdateStatusText();
            bettingTimer--;
            if (bettingTimer == 0)
            {
                IsLocked = true;
                return;
            }
            SendCustomEventDelayedSeconds(nameof(_StartBettingTimer), 1f);
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
            if (!HasBettingStarted)
            {
                Debug.Log("Marble Race: Bets have not yet started, but player tried to bet.");
                return;
            }
            
            if (IsLocked)
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

            raceManager._OnBetPlaced();
        }

        private void LockAllButtons(bool b)
        {
            foreach (var button in betButtons)
            {
                button._SetIsLocked(b);
            }
        }

        public void _Finish(sbyte marbleIndex, sbyte placement, int payout)
        {
            betButtons[marbleIndex]._SetPlacement(placement, payout);
        }

        private void UpdateStatusText()
        {
            if (!HasBettingStarted) statusText.text = "Not<br>started";
            else statusText.text = IsLocked ? "<i>Bets<br>closed</i>" : $"{bettingTimer}<br>Bet!!";
        }
    }
}
