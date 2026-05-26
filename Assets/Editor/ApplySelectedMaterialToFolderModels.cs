using UnityEngine;
using UnityEditor;

public class ApplySelectedMaterialToFolderModels
{
    [MenuItem("Tools/Pixel 3D RPG/Apply Selected Material To Selected Folder Models")]
    static void Apply()
    {
        Material selectedMaterial = null;
        string selectedFolder = null;

        foreach (Object obj in Selection.objects)
        {
            string path = AssetDatabase.GetAssetPath(obj);

            if (obj is Material material)
            {
                selectedMaterial = material;
            }

            if (AssetDatabase.IsValidFolder(path))
            {
                selectedFolder = path;
            }
        }

        if (selectedMaterial == null)
        {
            Debug.LogError("適用したいMaterialを1つ選択してください。");
            return;
        }

        if (string.IsNullOrEmpty(selectedFolder))
        {
            Debug.LogError("対象のモデルフォルダを1つ選択してください。");
            return;
        }

        string[] modelGuids = AssetDatabase.FindAssets("t:Model", new[] { selectedFolder });

        int rendererCount = 0;
        int modelCount = 0;

        foreach (string guid in modelGuids)
        {
            string modelPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);

            if (model == null) continue;

            Renderer[] renderers = model.GetComponentsInChildren<Renderer>(true);

            foreach (Renderer renderer in renderers)
            {
                SerializedObject so = new SerializedObject(renderer);
                SerializedProperty materials = so.FindProperty("m_Materials");

                for (int i = 0; i < materials.arraySize; i++)
                {
                    materials.GetArrayElementAtIndex(i).objectReferenceValue = selectedMaterial;
                }

                so.ApplyModifiedProperties();
                rendererCount++;
            }

            modelCount++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"完了: {modelCount}モデル / {rendererCount}Renderer に {selectedMaterial.name} を適用しました。");
    }
}