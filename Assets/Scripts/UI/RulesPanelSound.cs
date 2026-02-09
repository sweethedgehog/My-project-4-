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

        private RulesCords currentState = RulesCords.Closed;

        public void PlaySoundForState(RulesCords targetState)
        {
            if (targetState == RulesCords.Open)
                PlayOpenSound();
            else
                PlayCloseSound();
        }

        private void PlayOpenSound()
        {
            if (panelOpenSound != null && currentState != RulesCords.Open && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(panelOpenSound);
                currentState = RulesCords.Open;
            }
        }

        private void PlayCloseSound()
        {
            if (panelCloseSound != null && currentState != RulesCords.Closed && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(panelCloseSound);
                currentState = RulesCords.Closed;
            }
        }
    }
}
