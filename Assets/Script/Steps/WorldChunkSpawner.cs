using System.Collections.Generic;
using UnityEngine;

public class WorldChunkSpawner : MonoBehaviour
{
    [Header("Player")]
    public Transform player;

    [Header("Chunk Prefabs")]
    public GameObject chunkA;
    public GameObject chunkB;

    [Header("Spawn Settings")]
    public int initialChunkCount = 4;
    public float spawnAheadDistance = 120f;
    public float despawnBehindDistance = 120f;

    [Header("Position")]
    public float startX = 0f;
    public float chunkY = 0f;
    public float chunkZ = 0f;

    [Header("Debug")]
    public bool showDebugLog = true;

    private readonly Queue<ChunkInfo> activeChunks = new Queue<ChunkInfo>();

    private int nextIndex = 0;
    private float nextSpawnX;

    private class ChunkInfo
    {
        public GameObject obj;
        public float startX;
        public float width;
        public float endX => startX + width;
    }

    void Start()
    {
        if (player == null)
        {
            Debug.LogError("WorldChunkSpawner: player Ç™ñ¢ê›íËÇ≈Ç∑ÅB");
            enabled = false;
            return;
        }

        if (chunkA == null || chunkB == null)
        {
            Debug.LogError("WorldChunkSpawner: chunkA / chunkB Ç™ñ¢ê›íËÇ≈Ç∑ÅB");
            enabled = false;
            return;
        }

        nextSpawnX = startX;

        for (int i = 0; i < initialChunkCount; i++)
        {
            SpawnNextChunk();
        }
    }

    void LateUpdate()
    {
        SpawnAheadIfNeeded();
        DespawnBehindIfNeeded();
    }

    private void SpawnAheadIfNeeded()
    {
        while (nextSpawnX < player.position.x + spawnAheadDistance)
        {
            SpawnNextChunk();
        }
    }

    private void DespawnBehindIfNeeded()
    {
        while (activeChunks.Count > 0)
        {
            ChunkInfo oldest = activeChunks.Peek();

            if (oldest == null || oldest.obj == null)
            {
                activeChunks.Dequeue();
                continue;
            }

            if (oldest.endX < player.position.x - despawnBehindDistance)
            {
                activeChunks.Dequeue();

                if (showDebugLog)
                    Debug.Log("Destroy Chunk: " + oldest.obj.name);

                Destroy(oldest.obj);
            }
            else
            {
                break;
            }
        }
    }

    private void SpawnNextChunk()
    {
        GameObject prefab = nextIndex % 2 == 0 ? chunkA : chunkB;

        Vector3 spawnPos = new Vector3(nextSpawnX, chunkY, chunkZ);

        GameObject chunk = Instantiate(prefab, spawnPos, Quaternion.identity, transform);
        chunk.name = prefab.name + "_" + nextIndex;

        float width = GetChunkWidth(chunk);

        if (width <= 0f)
        {
            Debug.LogError("Chunk width ÇéÊìæÇ≈Ç´Ç‹ÇπÇÒÇ≈ÇµÇΩ: " + chunk.name);
            Destroy(chunk);
            return;
        }

        ChunkInfo info = new ChunkInfo
        {
            obj = chunk,
            startX = nextSpawnX,
            width = width
        };

        activeChunks.Enqueue(info);

        if (showDebugLog)
        {
            Debug.Log(
                "Spawn Chunk: " + chunk.name +
                " / StartX: " + info.startX +
                " / Width: " + info.width +
                " / EndX: " + info.endX
            );
        }

        nextSpawnX += width;
        nextIndex++;
    }

    private float GetChunkWidth(GameObject chunk)
    {
        Renderer[] renderers = chunk.GetComponentsInChildren<Renderer>();

        if (renderers == null || renderers.Length == 0)
        {
            Debug.LogWarning("Renderer Ç™å©Ç¬Ç©ÇËÇ‹ÇπÇÒ: " + chunk.name);
            return 0f;
        }

        Bounds bounds = renderers[0].bounds;

        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        return bounds.size.x;
    }
}