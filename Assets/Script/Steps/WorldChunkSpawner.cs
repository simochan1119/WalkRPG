using UnityEngine;

public class WorldChunkSpawner : MonoBehaviour
{
    [Header("Player")]
    public Transform player;

    [Header("Chunk Prefabs")]
    public GameObject chunkA;
    public GameObject chunkB;

    [Header("Loop Settings")]
    [Tooltip("The actual width of a single chunk in Unity units. Used to align and warp chunks seamlessly.")]
    public float fixedChunkWidth = 60f;

    [Header("Position")]
    public float startX = 0f;
    public float chunkY = 0f;
    public float chunkZ = 0f;

    [Header("Debug")]
    public bool showDebugLog = false;

    private GameObject[] pooledChunks = new GameObject[3];

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

        // Instantiate exactly 3 chunks (A, B, A) to cover enough view distance for the camera
        GameObject[] prefabs = new GameObject[] { chunkA, chunkB, chunkA };
        for (int i = 0; i < pooledChunks.Length; i++)
        {
            float spawnX = startX + (i * fixedChunkWidth);
            Vector3 pos = new Vector3(spawnX, chunkY, chunkZ);
            pooledChunks[i] = Instantiate(prefabs[i], pos, Quaternion.identity, transform);
            pooledChunks[i].name = prefabs[i].name + "_Loop_" + i;
        }

        if (showDebugLog)
        {
            Debug.Log($"[WorldChunkSpawner] Initialized 3-chunk pool. Total width covered: {fixedChunkWidth * 3} units.");
        }
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

            if (playerX - chunkX > fixedChunkWidth * 1.1f)
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

                float newX = maxForwardX + fixedChunkWidth;
                chunk.transform.position = new Vector3(newX, chunkY, chunkZ);

                if (showDebugLog)
                {
                    Debug.Log($"[WorldChunkSpawner] Warped {chunk.name} forward to X={newX} (Player at X={playerX:F2})");
                }
            }
            // Warp backward (Moving Left - Village fallback)
            else if (chunkX - playerX > fixedChunkWidth * 1.9f)
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

                float newX = minBackwardX - fixedChunkWidth;
                chunk.transform.position = new Vector3(newX, chunkY, chunkZ);

                if (showDebugLog)
                {
                    Debug.Log($"[WorldChunkSpawner] Warped {chunk.name} backward to X={newX} (Player at X={playerX:F2})");
                }
            }
        }
    }
}