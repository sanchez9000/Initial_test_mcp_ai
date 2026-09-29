using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Ensures slot PNGs import as Sprites. Menu: Tools / Arcade Sample / Configure Slot Sprites
/// </summary>
public static class ConfigureSlotSprites
{
    [MenuItem("Tools/Arcade Sample/Configure Slot Sprites")]
    public static void Configure()
    {
        string[] folders =
        {
            "Assets/Resources/Slots/Symbols",
            "Assets/Resources/Slots/UI",
            "Assets/Art/Slots/Symbols",
            "Assets/Art/Slots/UI",
        };

        int count = 0;
        foreach (var folder in folders)
        {
            if (!AssetDatabase.IsValidFolder(folder)) continue;
            var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folder });
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;
                bool dirty = false;
                if (importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    dirty = true;
                }
                if (importer.spritePixelsPerUnit != 100f)
                {
                    importer.spritePixelsPerUnit = 100f;
                    dirty = true;
                }
                if (importer.mipmapEnabled)
                {
                    importer.mipmapEnabled = false;
                    dirty = true;
                }
                if (dirty)
                {
                    importer.SaveAndReimport();
                    count++;
                }
            }
        }

        Debug.Log($"[Slots] Configured {count} textures as Sprites.");
    }

    [InitializeOnLoadMethod]
    static void AutoConfigureOnce()
    {
        const string key = "Arcade_SlotSpritesConfigured_v1";
        if (SessionState.GetBool(key, false)) return;
        if (!Directory.Exists("Assets/Resources/Slots/Symbols")) return;
        EditorApplication.delayCall += () =>
        {
            if (SessionState.GetBool(key, false)) return;
            Configure();
            SessionState.SetBool(key, true);
        };
    }
}
