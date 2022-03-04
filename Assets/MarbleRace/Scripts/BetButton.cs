using System;
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
                UpdateAnimator();
            }
        }

        private void OnEnable()
        {
            UpdateAnimator();
        }
        
        private void UpdateAnimator()
        {
            animator.SetBool("HasPlacedBet", hasPlacedBet);
        }

        public void _Setup(BetScreen screen, sbyte index, string marbleName, Color marbleColor)
        {
            textName.text = marbleName;
            backgroundImage.color = marbleColor;
            marbleIndex = index;
            betScreen = screen;
            SetTextColor(GetTextColor(marbleColor));
        }

        private void SetTextColor(Color textColor)
        {
            textName.color = textColor;
            textPlacement.color = textColor;
            textPayout.color = textColor;
        }

        private Color GetTextColor(Color marbleColor)
        {
            return marbleColor.grayscale > 0.5f ? Color.black : Color.white;
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
            if (n < 0) return "";
            if (betScreen == null) return "";
            return betScreen._GetPlacementString(n);
        }
    }
}
