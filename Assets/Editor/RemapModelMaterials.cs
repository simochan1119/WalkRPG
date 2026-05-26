using UnityEngine;
using UnityEditor;

public class RemapModelMaterials
{
    [MenuItem("Tools/Pixel 3D RPG/Remap Selected Material To Selected Folder Models")]
    static void Remap()
    {
        Material targetMaterial = null;
        string targetFolder = null;

        foreach (Object obj in Selection.objects)
        {
            string path = AssetDatabase.GetAssetPath(obj);

            if (obj is Material mat)
                targetMaterial = mat;

            if (AssetDatabase.IsValidFolder(path))
                targetFolder = path;
        }

        if (targetMaterial == null)
        {
            Debug.LogError("適用したいMaterialを1つ選択してください。");
            return;
        }

        if (string.IsNullOrEmpty(targetFolder))
        {
            Debug.LogError("対象モデルが入っているFolderを1つ選択してください。");
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:Model", new[] { targetFolder });

        int count = 0;

        foreach (string guid in guids)
        {
            string modelPath = AssetDatabase.GUIDToAssetPath(guid);

            ModelImporter importer = AssetImporter.GetAtPath(modelPath) as ModelImporter;
            if (importer == null)
                continue;

            var source = new AssetImporter.SourceAssetIdentifier(
                typeof(Material),
                "defaultMat"
            );

            importer.AddRemap(source, targetMaterial);
            importer.SaveAndReimport();

            count++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Remap完了: {count} 個のモデルに {targetMaterial.name} を割り当てました。");
    }
}