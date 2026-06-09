using UnityEngine;

public class WorldChunkSpawner : MonoBehaviour
{
    [Header("Player")]
    public Transform player;

    [Header("Chunk Prefabs")]
    public GameObject chunkA;
    public GameObject chunkB;

    [Header("Loop Settings")]
    [Tooltip("Fallback width of a single chunk in Unity units if dynamic calculation fails.")]
    public float fixedChunkWidth = 200f;

    [Header("Position")]
    public float startX = 0f;
    public float chunkY = 0f;
    public float chunkZ = 0f;

    [Header("Debug")]
    public bool showDebugLog = false;

    private GameObject[] pooledChunks = new GameObject[3];
    private float calculatedChunkWidth;

    void Start()
    {
        if (player == null)
        {
            Debug.LogError("WorldChunkSpawner: Player transform is not assigned!");
            enabled = false;
            return;
        }

        if (chunkA == null || chunkB == null)
        {
            Debug.LogError("WorldChunkSpawner: chunkA or chunkB is not assigned!");
            enabled = false;
            return;
        }

        // Calculate chunk width dynamically from chunkA
        calculatedChunkWidth = GetPrefabWidth(chunkA);
        if (calculatedChunkWidth <= 0.1f)
        {
            calculatedChunkWidth = fixedChunkWidth; // Fallback
            if (showDebugLog)
            {
                Debug.LogWarning($"[WorldChunkSpawner] Could not calculate chunk width dynamically. Falling back to inspector value: {calculatedChunkWidth}");
            }
        }
        else
        {
            if (showDebugLog)
            {
                Debug.Log($"[WorldChunkSpawner] Dynamically calculated chunk width: {calculatedChunkWidth} units.");
            }
        }

        // Instantiate exactly 3 chunks (A, B, A) to cover enough view distance for the camera
        GameObject[] prefabs = new GameObject[] { chunkA, chunkB, chunkA };
        for (int i = 0; i < pooledChunks.Length; i++)
        {
            float spawnX = startX + (i * calculatedChunkWidth);
            Vector3 pos = new Vector3(spawnX, chunkY, chunkZ);
            pooledChunks[i] = Instantiate(prefabs[i], pos, Quaternion.identity, transform);
            pooledChunks[i].name = prefabs[i].name + "_Loop_" + i;
        }

        if (showDebugLog)
        {
            Debug.Log($"[WorldChunkSpawner] Initialized 3-chunk pool. Total width covered: {calculatedChunkWidth * 3} units.");
        }
    }

    private float GetPrefabWidth(GameObject prefab)
    {
        // 1. Try Terrain component first
        var terrain = prefab.GetComponentInChildren<Terrain>();
        if (terrain != null && terrain.terrainData != null)
        {
            return terrain.terrainData.size.x;
        }

        // 2. Instantiate temporary copy to measure bounds
        GameObject tempObj = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        tempObj.SetActive(false);
        var renderers = tempObj.GetComponentsInChildren<Renderer>();
        
        float width = 0f;
        if (renderers != null && renderers.Length > 0)
        {
            float minX = float.MaxValue;
            float maxX = float.MinValue;
            bool hasValidBounds = false;
            foreach (var r in renderers)
            {
                if (r is ParticleSystemRenderer) continue;
                minX = Mathf.Min(minX, r.bounds.min.x);
                maxX = Mathf.Max(maxX, r.bounds.max.x);
                hasValidBounds = true;
            }
            if (hasValidBounds)
            {
                width = maxX - minX;
            }
        }
        
        Destroy(tempObj);
        return width;
    }

    void LateUpdate()
    {
        if (player == null) return;

        float playerX = player.position.x;

        // Warp forward (Moving Right)
        // If a chunk is left behind the player by more than 1.1x chunk width,
        // it means it's completely out of the camera's left view. Warp it to the right of the furthest forward chunk.
        foreach (var chunk in pooledChunks)
        {
            if (chunk == null) continue;

            float chunkX = chunk.transform.position.x;

            if (playerX - chunkX > calculatedChunkWidth * 1.1f)
            {
                // Find the current furthest forward chunk's X position
                float maxForwardX = -999999f;
                foreach (var c in pooledChunks)
                {
                    if (c != null && c.transform.position.x > maxForwardX)
                    {
                        maxForwardX = c.transform.position.x;
                    }
                }

                float newX = maxForwardX + calculatedChunkWidth;
                chunk.transform.position = new Vector3(newX, chunkY, chunkZ);

                if (showDebugLog)
                {
                    Debug.Log($"[WorldChunkSpawner] Warped {chunk.name} forward to X={newX} (Player at X={playerX:F2})");
                }
            }
            // Warp backward (Moving Left - Village fallback)
            else if (chunkX - playerX > calculatedChunkWidth * 1.9f)
            {
                // Find the current furthest backward chunk's X position
                float minBackwardX = 999999f;
                foreach (var c in pooledChunks)
                {
                    if (c != null && c.transform.position.x < minBackwardX)
                    {
                        minBackwardX = c.transform.position.x;
                    }
                }

                float newX = minBackwardX - calculatedChunkWidth;
                chunk.transform.position = new Vector3(newX, chunkY, chunkZ);

                if (showDebugLog)
                {
                    Debug.Log($"[WorldChunkSpawner] Warped {chunk.name} backward to X={newX} (Player at X={playerX:F2})");
                }
            }
        }
    }
}