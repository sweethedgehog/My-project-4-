using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization.Tables;

/// <summary>
/// One-shot Editor tool: reads claude_workspace/DiitC - localization.csv
/// and updates all StringTable assets (en + ru) with writer-edited text.
/// Delete this file after use.
/// </summary>
public static class LocalizationCSVImporter
{
    private const string CsvPath = "claude_workspace/DiitC - localization.csv";

    [MenuItem("Tools/Import Localization CSV")]
    public static void Import()
    {
        string fullPath = Path.Combine(Application.dataPath, "..", CsvPath);
        if (!File.Exists(fullPath))
        {
            Debug.LogError($"[LocalizationCSVImporter] CSV not found at: {fullPath}");
            return;
        }

        string csvText = File.ReadAllText(fullPath, Encoding.UTF8);
        List<string[]> rows = ParseCSV(csvText);

        // Skip header row
        if (rows.Count > 0 && rows[0].Length >= 1 && rows[0][0] == "Table")
            rows.RemoveAt(0);

        // Cache: table name -> StringTableCollection
        var collectionCache = new Dictionary<string, StringTableCollection>();

        int updated = 0;
        int skipped = 0;

        foreach (string[] row in rows)
        {
            if (row.Length < 4)
            {
                Debug.LogWarning($"[LocalizationCSVImporter] Skipping malformed row: {string.Join(" | ", row)}");
                skipped++;
                continue;
            }

            string tableName = row[0].Trim();
            string key = row[1].Trim();
            string english = row[2];
            string russian = row[3];

            if (string.IsNullOrEmpty(tableName) || string.IsNullOrEmpty(key))
            {
                skipped++;
                continue;
            }

            // Get or cache the StringTableCollection
            if (!collectionCache.TryGetValue(tableName, out StringTableCollection collection))
            {
                collection = FindCollection(tableName);
                if (collection == null)
                {
                    Debug.LogWarning($"[LocalizationCSVImporter] Table collection not found: {tableName}");
                    skipped++;
                    continue;
                }
                collectionCache[tableName] = collection;
            }

            // Find the key ID from SharedTableData
            SharedTableData sharedData = collection.SharedData;
            SharedTableData.SharedTableEntry entry = sharedData.GetEntry(key);
            if (entry == null)
            {
                Debug.LogWarning($"[LocalizationCSVImporter] Key not found in {tableName}: {key}");
                skipped++;
                continue;
            }

            long keyId = entry.Id;

            // Update EN table
            StringTable enTable = FindLocaleTable(collection, "en");
            if (enTable != null)
            {
                enTable.AddEntry(keyId, english);
                EditorUtility.SetDirty(enTable);
            }

            // Update RU table
            StringTable ruTable = FindLocaleTable(collection, "ru");
            if (ruTable != null)
            {
                ruTable.AddEntry(keyId, russian);
                EditorUtility.SetDirty(ruTable);
            }

            updated++;
            Debug.Log($"[LocalizationCSVImporter] Updated {tableName}/{key}");
        }

        // Save all modified assets
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[LocalizationCSVImporter] Done! Updated: {updated}, Skipped: {skipped}");
    }

    private static StringTableCollection FindCollection(string tableName)
    {
        var collections = LocalizationEditorSettings.GetStringTableCollections();
        foreach (var col in collections)
        {
            if (col.TableCollectionName == tableName)
                return col;
        }
        return null;
    }

    private static StringTable FindLocaleTable(StringTableCollection collection, string localeCode)
    {
        foreach (var table in collection.StringTables)
        {
            if (table != null && table.LocaleIdentifier.Code == localeCode)
                return table;
        }
        return null;
    }

    /// <summary>
    /// RFC 4180 CSV parser — handles quoted fields with newlines and escaped quotes.
    /// </summary>
    private static List<string[]> ParseCSV(string text)
    {
        var rows = new List<string[]>();
        var fields = new List<string>();
        var field = new StringBuilder();
        bool inQuotes = false;
        int i = 0;

        while (i < text.Length)
        {
            char c = text[i];

            if (inQuotes)
            {
                if (c == '"')
                {
                    // Check for escaped quote ""
                    if (i + 1 < text.Length && text[i + 1] == '"')
                    {
                        field.Append('"');
                        i += 2;
                    }
                    else
                    {
                        // End of quoted field
                        inQuotes = false;
                        i++;
                    }
                }
                else
                {
                    field.Append(c);
                    i++;
                }
            }
            else
            {
                if (c == '"' && field.Length == 0)
                {
                    // Start of quoted field
                    inQuotes = true;
                    i++;
                }
                else if (c == ',')
                {
                    // Field separator
                    fields.Add(field.ToString());
                    field.Clear();
                    i++;
                }
                else if (c == '\n' || (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n'))
                {
                    // End of row
                    fields.Add(field.ToString());
                    field.Clear();

                    if (fields.Count > 1 || !string.IsNullOrEmpty(fields[0]))
                        rows.Add(fields.ToArray());

                    fields = new List<string>();

                    i += (c == '\r') ? 2 : 1;
                }
                else if (c == '\r')
                {
                    // Bare \r as row end
                    fields.Add(field.ToString());
                    field.Clear();

                    if (fields.Count > 1 || !string.IsNullOrEmpty(fields[0]))
                        rows.Add(fields.ToArray());

                    fields = new List<string>();
                    i++;
                }
                else
                {
                    field.Append(c);
                    i++;
                }
            }
        }

        // Handle last field/row if file doesn't end with newline
        if (field.Length > 0 || fields.Count > 0)
        {
            fields.Add(field.ToString());
            if (fields.Count > 1 || !string.IsNullOrEmpty(fields[0]))
                rows.Add(fields.ToArray());
        }

        return rows;
    }
}
