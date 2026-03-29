using UnityEngine;
using UnityEngine.Localization.Settings;

public class LocalizationDebug : MonoBehaviour
{
    void Start()
    {
        Debug.Log("=== LOCALIZATION DEBUG ===");

        // Step 1: Is the localization system initialized?
        var initOp = LocalizationSettings.InitializationOperation;
        Debug.Log($"[1] InitializationOperation.IsDone: {initOp.IsDone}");
        Debug.Log($"[1] InitializationOperation.Status: {initOp.Status}");

        // Step 2: What locale is selected?
        var locale = LocalizationSettings.SelectedLocale;
        Debug.Log($"[2] SelectedLocale: {(locale != null ? locale.Identifier.Code : "NULL")}");

        // Step 3: List available locales
        var locales = LocalizationSettings.AvailableLocales.Locales;
        Debug.Log($"[3] Available locales count: {locales.Count}");
        foreach (var l in locales)
            Debug.Log($"[3]   - {l.Identifier.Code} ({l.name})");

        // Step 4: Try to get the MainScene table
        var tableOp = LocalizationSettings.StringDatabase.GetTableAsync("MainScene");
        Debug.Log($"[4] GetTableAsync('MainScene').IsDone: {tableOp.IsDone}");
        if (tableOp.IsDone && tableOp.Result != null)
        {
            Debug.Log($"[4] Table found: {tableOp.Result.TableCollectionName}, entries: {tableOp.Result.Count}");
        }
        else if (tableOp.IsDone)
        {
            Debug.Log("[4] Table operation done but Result is NULL");
        }
        else
        {
            Debug.Log("[4] Table not yet loaded (async pending)");
            tableOp.Completed += (op) =>
            {
                if (op.Result != null)
                    Debug.Log($"[4-async] Table loaded: {op.Result.TableCollectionName}, entries: {op.Result.Count}");
                else
                    Debug.Log("[4-async] Table loaded but Result is NULL");
            };
        }

        // Step 5: Try direct key lookup
        string[] testKeys = { "narrative_round_0", "end_round_button", "rules_button" };
        foreach (var key in testKeys)
        {
            string result = LocalizationSettings.StringDatabase.GetLocalizedString("MainScene", key);
            Debug.Log($"[5] GetLocalizedString('MainScene', '{key}'): \"{result}\"");
        }

        Debug.Log("=== END LOCALIZATION DEBUG ===");
    }
}
