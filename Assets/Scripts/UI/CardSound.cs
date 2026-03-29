using UnityEngine;
using CardGame.Managers;

namespace CardGame.UI
{
    public class CardSound : MonoBehaviour
    {
        [Header("Card Sounds")]
        [SerializeField] private AudioClip cardHoverEnterSound;
        [SerializeField] private AudioClip cardClickSound;
        [SerializeField] private AudioClip cardPickupSound;
        [SerializeField] private AudioClip cardDropSound;

        void OnMouseEnter()
        {
            AudioManager.Instance?.PlaySFX(cardHoverEnterSound);
        }

        void OnMouseDown()
        {
            AudioManager.Instance?.PlaySFX(cardClickSound);
        }

        /// <summary>
        /// Called explicitly from SimpleDraggableWithBoard on drag start
        /// </summary>
        public void PlayPickup()
        {
            AudioManager.Instance?.PlaySFX(cardPickupSound);
        }

        /// <summary>
        /// Called explicitly from SimpleDraggableWithBoard on drag end
        /// </summary>
        public void PlayDrop()
        {
            AudioManager.Instance?.PlaySFX(cardDropSound);
        }
    }
}
