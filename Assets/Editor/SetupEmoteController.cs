using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class SetupEmoteController : EditorWindow
{
    [MenuItem("Tools/Setup Emote Controller")]
    public static void Setup()
    {
        // 1. Configure Prefab Asset
        string prefabPath = "Assets/Prefab/MainCharacter.prefab";
        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);

        if (prefabRoot != null)
        {
            ConfigureGameObject(prefabRoot);
            PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefabRoot);
            Debug.Log("[SetupEmoteController] MainCharacter prefab updated and saved successfully!");
        }
        else
        {
            Debug.LogError($"[SetupEmoteController] Failed to load prefab at {prefabPath}");
        }

        // 2. Configure Active Scene Instance (for cases where the player is a non-prefab local GameObject)
        GameObject scenePlayer = GameObject.Find("MainCharacter");
        if (scenePlayer != null)
        {
            ConfigureGameObject(scenePlayer);
            EditorUtility.SetDirty(scenePlayer);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            Debug.Log($"[SetupEmoteController] MainCharacter instance in active scene '{UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}' updated successfully!");
        }
    }

    private static void ConfigureGameObject(GameObject root)
    {
        // 1. Get or Add EmoteController
        EmoteController emoteController = root.GetComponent<EmoteController>();
        if (emoteController == null)
        {
            emoteController = root.AddComponent<EmoteController>();
            Debug.Log($"[SetupEmoteController] Added EmoteController to {root.name}");
        }

        // 2. Setup EmoteBubble child
        Transform bubbleTrans = root.transform.Find("EmoteBubble");
        SpriteRenderer bubbleRenderer = null;
        if (bubbleTrans == null)
        {
            GameObject bubbleObj = new GameObject("EmoteBubble");
            bubbleObj.transform.SetParent(root.transform, false);
            bubbleRenderer = bubbleObj.AddComponent<SpriteRenderer>();
            Debug.Log($"[SetupEmoteController] Created child GameObject 'EmoteBubble' with SpriteRenderer under {root.name}");
        }
        else
        {
            bubbleRenderer = bubbleTrans.GetComponent<SpriteRenderer>();
            if (bubbleRenderer == null)
            {
                bubbleRenderer = bubbleTrans.gameObject.AddComponent<SpriteRenderer>();
            }
        }

        // Configure child renderer
        bubbleRenderer.gameObject.SetActive(false);
        bubbleRenderer.transform.localPosition = new Vector3(0.5f, 2.2f, 0f);
        bubbleRenderer.transform.localScale = Vector3.zero;

        // Assign emoteRenderer and baseScale reference using SerializedObject
        SerializedObject so = new SerializedObject(emoteController);
        SerializedProperty rendererProperty = so.FindProperty("emoteRenderer");
        if (rendererProperty != null)
        {
            rendererProperty.objectReferenceValue = bubbleRenderer;
        }
        SerializedProperty localOffsetProperty = so.FindProperty("localOffset");
        if (localOffsetProperty != null)
        {
            localOffsetProperty.vector3Value = new Vector3(0.5f, 2.2f, 0f);
        }
        SerializedProperty baseScaleProperty = so.FindProperty("baseScale");
        if (baseScaleProperty != null)
        {
            baseScaleProperty.vector3Value = new Vector3(6f, 6f, 1f);
        }
        
        // 3. Load Exclamation Sprites
        List<Sprite> sprites = new List<Sprite>();
        string singlesFolder = "Assets/Admurin's Pixel Items/PixelItems/Emoticons/Singles";
        for (int i = 43; i <= 48; i++)
        {
            string spritePath = $"{singlesFolder}/{i}_ExclamationMark.png";
            Sprite spriteAsset = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (spriteAsset != null)
            {
                sprites.Add(spriteAsset);
            }
        }

        if (sprites.Count > 0)
        {
            SerializedProperty spritesProp = so.FindProperty("exclamationSprites");
            if (spritesProp != null)
            {
                spritesProp.ClearArray();
                spritesProp.arraySize = sprites.Count;
                for (int i = 0; i < sprites.Count; i++)
                {
                    spritesProp.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
                }
            }
            Debug.Log($"[SetupEmoteController] Assigned {sprites.Count} Exclamation Sprites to {root.name} via SerializedProperty");
        }
        else
        {
            Debug.LogWarning($"[SetupEmoteController] Could not find any exclamation sprites for {root.name} in {singlesFolder}");
        }

        // Apply properties
        so.ApplyModifiedProperties();
    }
}
