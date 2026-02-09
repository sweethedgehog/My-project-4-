using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using CardGame.Managers;

namespace CardGame.UI
{
    [RequireComponent(typeof(Selectable))]
    public class UISound : MonoBehaviour,
        IPointerEnterHandler,
        IPointerClickHandler
    {
        [Header("Sound Settings")]
        [SerializeField] private AudioClip clickSound;
        [SerializeField] private AudioClip hoverEnterSound;

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(hoverEnterSound);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(clickSound);
        }
    }
}
