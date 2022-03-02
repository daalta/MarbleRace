using UdonSharp;
using UnityEngine;
using UnityEngine.UI;

namespace MarbleRace.Scripts
{
    public class BetScreen : UdonSharpBehaviour
    {
        [SerializeField] private BetButton[] betButtons;

        /// <summary>
        /// Whether bets can be placed.
        /// </summary>
        [UdonSynced, FieldChangeCallback(nameof(IsLocked))] private bool isLocked = false;

        private bool IsLocked
        {
            get => isLocked;
            set
            {
                if (isLocked == value) return;
                isLocked = value;
                LockAllButtons(isLocked);
            }
        }
        
        /// <summary>
        /// Index of the marble the local player has bet on.
        /// </summary>
        private sbyte bet = -1;

        public void _Setup(int buttonIndex, string marbleName, Color marbleColor)
        {
            betButtons[buttonIndex]._Setup(
                this,
                (sbyte) buttonIndex,
                marbleName,
                marbleColor);
        }

        public void _Press(sbyte index)
        {
            if (isLocked)
            {
                Debug.Log("Marble Race: Betting is locked, but player tried to bet.");
                return;
            }

            if (bet != -1) // Undo previous bet, if betting is still unlocked
            {
                betButtons[index]._HasPlacedBet(false);
            }

            bet = index;
            betButtons[index]._HasPlacedBet(true);
        }

        private void LockAllButtons(bool b)
        {
            foreach (var button in betButtons)
            {
                button._SetIsLocked(b);
            }
        }
    }
}
