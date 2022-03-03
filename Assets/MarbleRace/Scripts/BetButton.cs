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
            if (place < 0)
            {
                _ClearPlacement();
                return;
            }
            var colorPrefix = !hasPlacedBet ? "<color=grey>" : payout > 0 ? "<color=yellow>": "<color=red>" ;
            textPlacement.text = colorPrefix + GetPlacementString(place);
            textPayout.text = colorPrefix + payout + "$";
        }

        public void _ClearPlacement()
        {
            textPlacement.text = "";
            textPayout.text = "";
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
