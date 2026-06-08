using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class SetupWorldLoop
{
    static SetupWorldLoop()
    {
        EditorApplication.delayCall += RunSetup;
    }

    [MenuItem("Tools/Setup World Loop")]
    public static void RunSetup()
    {
        if (EditorApplication.isPlaying) return;

        string scenePath = "Assets/Scenes/03_Field1.unity";
        var activeScene = EditorSceneManager.GetActiveScene();
        
        if (activeScene.path != scenePath)
        {
            // Open the scene
            activeScene = EditorSceneManager.OpenScene(scenePath);
        }

        GameObject spawnerObj = GameObject.Find("WorldChunkSpawner");
        GameObject fieldObj = GameObject.Find("Field");
        GameObject playerObj = GameObject.Find("MainCharacter");

        if (playerObj == null)
        {
            Debug.LogWarning("[SetupWorldLoop] MainCharacter not found in scene.");
            return;
        }

        if (spawnerObj != null)
        {
            Debug.Log("[SetupWorldLoop] WorldChunkSpawner already exists in scene. Skipping setup.");
            return;
        }

        if (fieldObj == null)
        {
            Debug.LogWarning("[SetupWorldLoop] Static Field object not found in scene. Cannot extract position.");
            return;
        }

        Vector3 pos = fieldObj.transform.position;
        Debug.Log($"[SetupWorldLoop] Found static Field at X={pos.x}, Y={pos.y}, Z={pos.z}. Setting up loop...");

        // Create the spawner GameObject
        spawnerObj = new GameObject("WorldChunkSpawner");
        var spawner = spawnerObj.AddComponent<WorldChunkSpawner>();

        // Load Field prefab
        GameObject fieldPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Field.prefab");
        if (fieldPrefab == null)
        {
            Debug.LogError("[SetupWorldLoop] Field.prefab not found at Assets/Prefab/Field.prefab!");
            return;
        }

        // Set properties
        spawner.player = playerObj.transform;
        spawner.chunkA = fieldPrefab;
        spawner.chunkB = fieldPrefab;
        spawner.fixedChunkWidth = 60f;
        spawner.startX = pos.x;
        spawner.chunkY = pos.y;
        spawner.chunkZ = pos.z;
        spawner.showDebugLog = true;

        // Delete the static field from the scene
        Undo.DestroyObjectImmediate(fieldObj);

        // Mark scene dirty and save
        EditorUtility.SetDirty(spawnerObj);
        EditorSceneManager.MarkSceneDirty(activeScene);
        EditorSceneManager.SaveScene(activeScene);

        Debug.Log("[SetupWorldLoop] WorldChunkSpawner successfully created, static Field deleted, and scene saved!");
    }
}
