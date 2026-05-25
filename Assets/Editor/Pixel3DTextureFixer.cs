using UnityEngine;
using UnityEditor;
using System.IO;

public class Pixel3DTextureFixer : EditorWindow
{
    [MenuItem("Tools/Pixel 3D RPG/Fix Texture Import Settings")]
    static void Fix()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets" });
        int count = 0;
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string name = Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
            if (!(path.Contains("Pixel_3D_RPG") || path.Contains("Pixel_3D_Caverns"))) continue;

            TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti == null) continue;

            ti.filterMode = FilterMode.Point;
            ti.mipmapEnabled = true;
            ti.textureCompression = TextureImporterCompression.Uncompressed;

            if (name.EndsWith("_n") || name.Contains("_normal"))
            {
                ti.textureType = TextureImporterType.NormalMap;
                ti.sRGBTexture = false;
            }
            else if (name.EndsWith("_m") || name.EndsWith("_r") || name.EndsWith("_rm"))
            {
                ti.textureType = TextureImporterType.Default;
                ti.sRGBTexture = false;
            }
            else
            {
                ti.textureType = TextureImporterType.Default;
                ti.sRGBTexture = true;
            }

            ti.SaveAndReimport();
            count++;
        }
        Debug.Log($"Pixel 3D RPG: texture settings fixed: {count} textures.");
    }
}