using JetBrains.Annotations;
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;

namespace MarbleRace.Scripts
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None), RequireComponent(typeof(Button))]
    public class BetButton : UdonSharpBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private TextMeshProUGUI textName;
        [SerializeField] private TextMeshProUGUI textPlacement;
        [SerializeField] private TextMeshProUGUI textPayout;
        [SerializeField] private Image backgroundImage;
        
        private BetScreen betScreen;
        private sbyte marbleIndex;
        
        /// <summary>
        /// Placement of this ball. 0 is the first ball, 1 is the second, etc.
        /// -1 means the ball has not finished yet.
        /// </summary>
        private sbyte placement = -1;

        private bool hasPlacedBet;

        public bool HasPlacedBet
        {
            get => hasPlacedBet;
            set
            {
                if (hasPlacedBet == value) return;
                hasPlacedBet = value;
                animator.SetBool("HasPlacedBet", hasPlacedBet);
            }
        }

        public void _Setup(BetScreen screen, sbyte index, string marbleName, Color marbleColor)
        {
            textName.text = marbleName;
            backgroundImage.color = marbleColor;
            marbleIndex = index;
            betScreen = screen;
        }

        [PublicAPI]
        public void _Press()
        {
            betScreen._Press(marbleIndex);
        }

        public void _SetIsLocked(bool b)
        {
            GetComponent<Button>().interactable = !b;
        }
        
        public void _SetPlacement(sbyte place, int payout)
        {
            placement = place;
            var colorPrefix = hasPlacedBet ? "<color=white>" : "<color=grey>";
            textPlacement.text = colorPrefix + GetPlacementString(place);
            textPayout.text = colorPrefix + payout + "$";
        }

        private string GetPlacementString(sbyte n)
        {
            var result = (n + 1).ToString();
            switch (n)
            {
                case 0:
                    return result + "st";
                case 1:
                    return result + "nd";
                case 2:
                    return result + "rd";
                default:
                    return result + "th";
            }
        }
    }
}
