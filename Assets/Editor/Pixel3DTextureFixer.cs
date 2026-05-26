using UnityEngine;
using UnityEditor;
using System.IO;

public class Pixel3DMaterialAssigner
{
    [MenuItem("Tools/Pixel 3D RPG/Assign Grasslands Atlas Material")]
    static void AssignGrasslandsAtlasMaterial()
    {
        string colorTexPath = FindAssetPath("GrassLands_NEW_C", "t:Texture2D");
        string normalTexPath = FindAssetPath("GrassLands_NEW_N", "t:Texture2D");

        if (string.IsNullOrEmpty(colorTexPath))
        {
            Debug.LogError("GrassLands_NEW_C Ç™å©Ç¬Ç©ÇËÇ‹ÇπÇÒÅB");
            return;
        }

        Texture2D colorTex = AssetDatabase.LoadAssetAtPath<Texture2D>(colorTexPath);
        Texture2D normalTex = string.IsNullOrEmpty(normalTexPath)
            ? null
            : AssetDatabase.LoadAssetAtPath<Texture2D>(normalTexPath);

        string matFolder = "Assets/GeneratedMaterials";
        if (!AssetDatabase.IsValidFolder(matFolder))
        {
            AssetDatabase.CreateFolder("Assets", "GeneratedMaterials");
        }

        string matPath = matFolder + "/Grasslands_Atlas_Material.mat";

        Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(mat, matPath);
        }

        mat.SetTexture("_BaseMap", colorTex);
        mat.SetColor("_BaseColor", Color.white);

        if (normalTex != null)
        {
            mat.SetTexture("_BumpMap", normalTex);
            mat.EnableKeyword("_NORMALMAP");
        }

        EditorUtility.SetDirty(mat);

        int count = 0;

        string[] modelGuids = AssetDatabase.FindAssets("t:Model", new[] { "Assets" });

        foreach (string guid in modelGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            if (!path.Contains("Pixel_3D_RPG_Grasslands_2.0"))
                continue;

            if (!path.Contains("/OBJ/"))
                continue;

            GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (model == null) continue;

            Renderer[] renderers = model.GetComponentsInChildren<Renderer>(true);

            foreach (Renderer r in renderers)
            {
                SerializedObject so = new SerializedObject(r);
                SerializedProperty mats = so.FindProperty("m_Materials");

                for (int i = 0; i < mats.arraySize; i++)
                {
                    mats.GetArrayElementAtIndex(i).objectReferenceValue = mat;
                }

                so.ApplyModifiedProperties();
                count++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Grasslands material assigned to renderers: " + count);
    }

    static string FindAssetPath(string assetName, string filter)
    {
        string[] guids = AssetDatabase.FindAssets(assetName + " " + filter, new[] { "Assets" });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (Path.GetFileNameWithoutExtension(path) == assetName)
                return path;
        }

        return null;
    }
}