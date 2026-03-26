using System.Collections;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace CardGame.UI
{
    /// <summary>
    /// Hides selected CanvasGroups until localization is fully initialized,
    /// then reveals them. Prevents text flicker on scene load.
    /// </summary>
    public class LocalizationReadyRevealer : MonoBehaviour
    {
        [Header("Groups to hide until localization is ready")]
        [SerializeField] private CanvasGroup[] groups;

        [Header("Reveal Settings")]
        [SerializeField] private float fadeDuration = 0.3f;

        private void Awake()
        {
            foreach (var group in groups)
            {
                if (group == null) continue;
                group.alpha = 0f;
                group.interactable = false;
                group.blocksRaycasts = false;
            }
        }

        private IEnumerator Start()
        {
            yield return LocalizationSettings.InitializationOperation;

            if (fadeDuration <= 0f)
            {
                RevealAll();
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float alpha = Mathf.Clamp01(elapsed / fadeDuration);
                foreach (var group in groups)
                {
                    if (group != null)
                        group.alpha = alpha;
                }
                yield return null;
            }

            RevealAll();
        }

        private void RevealAll()
        {
            foreach (var group in groups)
            {
                if (group == null) continue;
                group.alpha = 1f;
                group.interactable = true;
                group.blocksRaycasts = true;
            }
        }
    }
}
