using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace CardGame.UI
{
    public class LocalizedText : MonoBehaviour
    {
        [SerializeField] private string tableName;
        [SerializeField] private string keyName;

        void Start()
        {
            var tmp = GetComponent<TMP_Text>();
            if (tmp != null)
                tmp.text = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, keyName);
        }
    }
}
