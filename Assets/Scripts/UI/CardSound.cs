using UnityEngine;
using UnityEngine.EventSystems;
using CardGame.Managers;

namespace CardGame.UI
{
    public class CardSound : MonoBehaviour,
        IPointerEnterHandler,
        IPointerClickHandler,
        IBeginDragHandler,
        IEndDragHandler
    {
        [Header("Card Sounds")]
        [SerializeField] private AudioClip cardHoverEnterSound;
        [SerializeField] private AudioClip cardClickSound;
        [SerializeField] private AudioClip cardPickupSound;
        [SerializeField] private AudioClip cardDropSound;

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(cardHoverEnterSound);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(cardClickSound);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(cardPickupSound);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(cardDropSound);
        }
    }
}
