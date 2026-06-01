using UnityEngine;
using UnityEditor;
using System.IO;

public static class CreateTownPrefabsWithMaterial
{
    [MenuItem("Tools/Ultimate End/Create Town Prefabs With Material")]
    public static void CreatePrefabs()
    {
        string modelRoot = "Assets/Resources/Pixel_3D_RPG_Town_2.0";
        string materialPath = "Assets/Resources/Pixel_3D_RPG_Town_2.0/Generated Materials/M_Town_Atlas.mat";
        string prefabFolder = "Assets/Resources/Pixel_3D_RPG_Town_2.0/Generated Prefabs";

        Material mat = AssetDatabase.LoadAssetAtPath<Material>(materialPath);

        if (mat == null)
        {
            Debug.LogError("MaterialÇ™å©Ç¬Ç©ÇËÇ‹ÇπÇÒ: " + materialPath);
            return;
        }

        if (!AssetDatabase.IsValidFolder(prefabFolder))
        {
            AssetDatabase.CreateFolder("Assets/Resources/Pixel_3D_RPG_Town_2.0", "Generated Prefabs");
        }

        string[] modelPaths = Directory.GetFiles(modelRoot, "*.*", SearchOption.AllDirectories);

        int count = 0;

        foreach (string rawPath in modelPaths)
        {
            string path = rawPath.Replace("\\", "/");

            if (!path.EndsWith(".obj") && !path.EndsWith(".fbx"))
                continue;

            // Generated Prefabsì‡ÇçƒèàóùÇµÇ»Ç¢
            if (path.Contains("/Generated Prefabs/"))
                continue;

            GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (modelAsset == null)
            {
                Debug.LogWarning("ÉÇÉfÉãÇ∆ÇµÇƒì«Ç›çûÇﬂÇ‹ÇπÇÒÇ≈ÇµÇΩ: " + path);
                continue;
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(modelAsset) as GameObject;

            if (instance == null)
            {
                instance = Object.Instantiate(modelAsset);
            }

            instance.name = Path.GetFileNameWithoutExtension(path);

            Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(true);

            foreach (Renderer renderer in renderers)
            {
                Material[] mats = renderer.sharedMaterials;

                if (mats == null || mats.Length == 0)
                {
                    mats = new Material[1];
                }

                for (int i = 0; i < mats.Length; i++)
                {
                    mats[i] = mat;
                }

                renderer.sharedMaterials = mats;
            }

            string prefabPath = prefabFolder + "/" + instance.name + ".prefab";
            prefabPath = AssetDatabase.GenerateUniqueAssetPath(prefabPath);

            PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
            Object.DestroyImmediate(instance);

            Debug.Log("PrefabçÏê¨: " + prefabPath);
            count++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"äÆóπ: {count}å¬ÇÃPrefabÇçÏê¨ÇµÇ‹ÇµÇΩÅB");
    }
}