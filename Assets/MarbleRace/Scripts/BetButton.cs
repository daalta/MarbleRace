using JetBrains.Annotations;
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;

namespace MarbleRace.Scripts
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class BetButton : UdonSharpBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private Image backgroundImage;
        
        private BetScreen betScreen;
        private sbyte marbleIndex;

        public void _Setup(BetScreen screen, sbyte index, string marbleName, Color marbleColor)
        {
            text.text = marbleName;
            backgroundImage.color = marbleColor;
            marbleIndex = index;
            betScreen = screen;
        }

        [PublicAPI]
        public void _Press()
        {
            betScreen._Press(marbleIndex);
        }

        public void _HasPlacedBet(bool b)
        {
            animator.SetBool("HasPlacedBet", b);
        }

        public void _SetIsLocked(bool b)
        {
            animator.SetBool("IsLocked", b);
        }
    }
}
