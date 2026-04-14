#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// Converts selected textures to Sprite type so they can be used in UI Image fields.
/// Select your icon PNGs in the Project window, then run:
/// Tools > Zombies > Convert Selected Textures to Sprites
/// </summary>
public static class TextureToSprite
{
    [MenuItem("Tools/Zombies/Convert Selected Textures to Sprites")]
    static void ConvertSelected()
    {
        Object[] selected = Selection.objects;

        if (selected.Length == 0)
        {
            EditorUtility.DisplayDialog("No Selection",
                "Select one or more texture files in the Project window first.", "OK");
            return;
        }

        int converted = 0;

        foreach (Object obj in selected)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

            if (importer == null) continue;   // not a texture
            if (importer.textureType == TextureImporterType.Sprite)
            {
                Debug.Log($"Already a sprite: {path}");
                continue;
            }

            importer.textureType      = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();

            Debug.Log($"Converted to sprite: {path}");
            converted++;
        }

        EditorUtility.DisplayDialog("Done",
            $"Converted {converted} texture(s) to Sprite.", "OK");

        AssetDatabase.Refresh();
    }
}
#endif
