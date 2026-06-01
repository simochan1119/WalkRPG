using UnityEngine;
using UnityEditor;
using System.IO;

public static class CreateUltimateEndPrefabsWithMaterials
{
    [MenuItem("Tools/Ultimate End/Create All Prefabs With Materials")]
    public static void CreateAllPrefabs()
    {
        CreatePrefabsForPack(
            packName: "Town",
            modelRoot: "Assets/Resources/Pixel_3D_RPG_Town_2.0",
            materialPath: "Assets/Resources/Pixel_3D_RPG_Town_2.0/Generated Materials/M_Town_Atlas.mat",
            prefabFolder: "Assets/Resources/Pixel_3D_RPG_Town_2.0/Generated Prefabs"
        );

        CreatePrefabsForPack(
            packName: "Grasslands",
            modelRoot: "Assets/Resources/Pixel_3D_RPG_Grasslands_2.0",
            materialPath: "Assets/Resources/Pixel_3D_RPG_Grasslands_2.0/Generated Materials/M_Grasslands_Atlas.mat",
            prefabFolder: "Assets/Resources/Pixel_3D_RPG_Grasslands_2.0/Generated Prefabs"
        );

        CreatePrefabsForPack(
            packName: "Caverns",
            modelRoot: "Assets/Resources/Pixel_3D_Caverns",
            materialPath: "Assets/Resources/Pixel_3D_Caverns/Generated Materials/M_Caverns_Atlas.mat",
            prefabFolder: "Assets/Resources/Pixel_3D_Caverns/Generated Prefabs"
        );

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Ultimate End 全Prefab生成完了！");
    }

    [MenuItem("Tools/Ultimate End/Create Town Prefabs With Material")]
    public static void CreateTownPrefabs()
    {
        CreatePrefabsForPack(
            packName: "Town",
            modelRoot: "Assets/Resources/Pixel_3D_RPG_Town_2.0",
            materialPath: "Assets/Resources/Pixel_3D_RPG_Town_2.0/Generated Materials/M_Town_Atlas.mat",
            prefabFolder: "Assets/Resources/Pixel_3D_RPG_Town_2.0/Generated Prefabs"
        );
    }

    [MenuItem("Tools/Ultimate End/Create Grasslands Prefabs With Material")]
    public static void CreateGrasslandsPrefabs()
    {
        CreatePrefabsForPack(
            packName: "Grasslands",
            modelRoot: "Assets/Resources/Pixel_3D_RPG_Grasslands_2.0",
            materialPath: "Assets/Resources/Pixel_3D_RPG_Grasslands_2.0/Generated Materials/M_Grasslands_Atlas.mat",
            prefabFolder: "Assets/Resources/Pixel_3D_RPG_Grasslands_2.0/Generated Prefabs"
        );
    }

    [MenuItem("Tools/Ultimate End/Create Caverns Prefabs With Material")]
    public static void CreateCavernsPrefabs()
    {
        CreatePrefabsForPack(
            packName: "Caverns",
            modelRoot: "Assets/Resources/Pixel_3D_Caverns",
            materialPath: "Assets/Resources/Pixel_3D_Caverns/Generated Materials/M_Caverns_Atlas.mat",
            prefabFolder: "Assets/Resources/Pixel_3D_Caverns/Generated Prefabs"
        );
    }

    private static void CreatePrefabsForPack(
        string packName,
        string modelRoot,
        string materialPath,
        string prefabFolder
    )
    {
        if (!AssetDatabase.IsValidFolder(modelRoot))
        {
            Debug.LogWarning($"{packName}: modelRoot が見つかりません: {modelRoot}");
            return;
        }

        Material mat = AssetDatabase.LoadAssetAtPath<Material>(materialPath);

        if (mat == null)
        {
            Debug.LogError($"{packName}: Material が見つかりません: {materialPath}");
            return;
        }

        EnsureFolder(prefabFolder);

        string[] modelPaths = Directory.GetFiles(modelRoot, "*.*", SearchOption.AllDirectories);

        int count = 0;

        foreach (string rawPath in modelPaths)
        {
            string path = rawPath.Replace("\\", "/");

            if (!path.EndsWith(".obj") && !path.EndsWith(".fbx"))
                continue;

            // 自動生成フォルダ内を再処理しない
            if (path.Contains("/Generated Prefabs/"))
                continue;

            if (path.Contains("/Generated Materials/"))
                continue;

            GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (modelAsset == null)
            {
                Debug.LogWarning($"{packName}: モデルとして読み込めませんでした: {path}");
                continue;
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(modelAsset) as GameObject;

            if (instance == null)
            {
                instance = Object.Instantiate(modelAsset);
            }

            instance.name = Path.GetFileNameWithoutExtension(path);

            ApplyMaterialToInstance(instance, mat);

            string prefabPath = prefabFolder + "/" + instance.name + ".prefab";

            // 同名Prefabがあれば上書きしたいので、既存がある場合はそのまま保存
            PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
            Object.DestroyImmediate(instance);

            Debug.Log($"{packName}: Prefab作成/更新: {prefabPath}");
            count++;
        }

        Debug.Log($"{packName}: 完了。{count}個のPrefabを作成/更新しました。");
    }

    private static void ApplyMaterialToInstance(GameObject instance, Material mat)
    {
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
            EditorUtility.SetDirty(renderer);
        }
    }

    private static void EnsureFolder(string fullPath)
    {
        if (AssetDatabase.IsValidFolder(fullPath))
            return;

        string[] parts = fullPath.Split('/');
        string current = parts[0];

        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];

            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, parts[i]);
            }

            current = next;
        }
    }
}