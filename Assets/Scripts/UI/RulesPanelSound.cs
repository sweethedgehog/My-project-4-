using UnityEngine;
using CardGame.Managers;

namespace CardGame.UI
{
    [RequireComponent(typeof(RulesPanel))]
    public class RulesPanelSound : MonoBehaviour
    {
        [Header("Panel Sounds")]
        [SerializeField] private AudioClip panelOpenSound;
        [SerializeField] private AudioClip panelCloseSound;

        private RulesCoords currentState = RulesCoords.Closed;

        public void PlaySoundForState(RulesCoords targetState)
        {
            if (targetState == RulesCoords.Open)
                PlayOpenSound();
            else
                PlayCloseSound();
        }

        private void PlayOpenSound()
        {
            if (panelOpenSound != null && currentState != RulesCoords.Open && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(panelOpenSound);
                currentState = RulesCoords.Open;
            }
        }

        private void PlayCloseSound()
        {
            if (panelCloseSound != null && currentState != RulesCoords.Closed && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(panelCloseSound);
                currentState = RulesCoords.Closed;
            }
        }
    }
}
