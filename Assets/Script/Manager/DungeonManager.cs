using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Threading.Tasks;

public enum DungeonEventType
{
    Enemy,
    Treasure,
    Boss,
    Heal,
    None
}

[System.Serializable]
public class DungeonEventPoint
{
    public int distance;
    public DungeonEventType eventType;
    public string message;
    public EnemyData enemyData; // Added for Enemy/Boss events
    public int minGold;         // Added for Treasure events
    public int maxGold;         // Added for Treasure events
}

public class DungeonManager : GameSceneManager
{
    public static DungeonManager Instance { get; private set; }

    [Header("Player")]
    public Transform player;
    public Animator animator;
    public float moveSpeed = 3f;

    [Header("Dungeon UI")]
    public TMP_Text areaNameText;
    public TMP_Text distanceText;
    public TMP_Text eventText;

    [Header("Stage Data")]
    public DungeonStageData stageData;

    [Header("VFX")]
    public GameObject bikkuriVFX;

    private Sprite[] pickupSprites;

    [Header("Event Points")]
    public DungeonEventPoint[] eventPoints;

    private int currentDistance = 0;
    private int totalDistance = 300;
    private int nextEventIndex = 0;

    private bool isRunning = false;
    private bool isWaitingEvent = false;
    private bool isCleared = false;
    private bool hasPreSpawnedCurrent = false; // Tracks if current event enemy has been pre-spawned

    private Vector3 startPosition;
    private Vector3 targetPosition;

    protected override void OnSceneReady()
    {
        Instance = this;

        // Setup looping terrain if not present in the scene
        WorldChunkSpawner spawner = FindObjectOfType<WorldChunkSpawner>();
        if (spawner == null)
        {
            Debug.Log("[DungeonManager] WorldChunkSpawner not found in scene. Creating one dynamically...");
            GameObject staticField = GameObject.Find("Field");
            Vector3 spawnPos = new Vector3(24.776865f, 0f, -1.5517247f);
            if (staticField != null)
            {
                spawnPos = staticField.transform.position;
                Destroy(staticField);
            }
            
            GameObject spawnerObj = new GameObject("WorldChunkSpawner_Dynamic");
            spawner = spawnerObj.AddComponent<WorldChunkSpawner>();
            GameObject fieldPrefab = Resources.Load<GameObject>("Prefab/Field");
            if (fieldPrefab == null)
            {
                Debug.LogError("[DungeonManager] Failed to load Prefab/Field from Resources!");
            }
            else
            {
                spawner.player = player;
                spawner.chunkA = fieldPrefab;
                spawner.chunkB = fieldPrefab;
                spawner.fixedChunkWidth = 60f;
                spawner.startX = spawnPos.x;
                spawner.chunkY = spawnPos.y;
                spawner.chunkZ = spawnPos.z;
                spawner.showDebugLog = true;
                Debug.Log($"[DungeonManager] Successfully initialized dynamic WorldChunkSpawner at X={spawnPos.x}, Y={spawnPos.y}, Z={spawnPos.z}");
            }
        }

        // Load and sort pickup sprites
        pickupSprites = Resources.LoadAll<Sprite>("Heroine_pickup");
        System.Array.Sort(pickupSprites, (a, b) => {
            int aNum = GetSpriteIndexFromName(a.name);
            int bNum = GetSpriteIndexFromName(b.name);
            return aNum.CompareTo(bNum);
        });

        if (!DungeonSession.HasSession)
        {
            SceneManager.LoadScene("02_Village");
            return;
        }

        // Load stage configuration from ScriptableObject if assigned
        if (stageData != null)
        {
            totalDistance = stageData.totalDistance;
            eventPoints = stageData.eventPoints;
        }
        else
        {
            totalDistance = DungeonSession.totalDistance;
        }

        if (player != null)
        {
            startPosition = player.position;
            targetPosition = startPosition;
        }

        if (animator == null && player != null)
            animator = player.GetComponent<Animator>();

        currentDistance = 0;
        nextEventIndex = 0;
        isCleared = false;
        hasPreSpawnedCurrent = false;

        RefreshUI();
        SetEventText(DungeonSession.areaName + " の探索開始！");

        StartRunToNextEvent();
    }

    void Update()
    {
        if (!isRunning || player == null)
            return;

        player.position = Vector3.MoveTowards(
            player.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        currentDistance = Mathf.RoundToInt(
            player.position.x - startPosition.x
        );

        RefreshUI();

        // Check if we need to pre-spawn the upcoming enemy off-screen
        CheckPreSpawnEnemy();

        if (Vector3.Distance(player.position, targetPosition) <= 0.01f)
        {
            isRunning = false;

            if (animator != null)
                animator.SetBool("Walk", false);

            TriggerCurrentEvent();
        }
    }

    private void StartRunToNextEvent()
    {
        if (isCleared)
            return;

        isWaitingEvent = false;
        hasPreSpawnedCurrent = false; // Reset pre-spawn tracker for the new event

        int nextDistance = GetNextTargetDistance();

        targetPosition = startPosition + Vector3.right * nextDistance;

        isRunning = true;

        if (animator != null)
            animator.SetBool("Walk", true);

        SetEventText("探索中...");
    }

    private int GetNextTargetDistance()
    {
        if (eventPoints != null && nextEventIndex < eventPoints.Length)
        {
            int eventDistance = eventPoints[nextEventIndex].distance;
            int detectOffset = stageData != null ? stageData.eventDetectDistance : 10;
            return Mathf.Clamp(eventDistance - detectOffset, 0, totalDistance);
        }

        return totalDistance;
    }

    private async void TriggerCurrentEvent()
    {
        isWaitingEvent = true;

        if (eventPoints != null && nextEventIndex < eventPoints.Length)
        {
            DungeonEventPoint point = eventPoints[nextEventIndex];
            nextEventIndex++;

            // 1. Play Exclamation/Surprise VFX if set
            if (bikkuriVFX != null)
            {
                bikkuriVFX.SetActive(true);
                await System.Threading.Tasks.Task.Delay(1000);
                bikkuriVFX.SetActive(false);
            }

            // 2. Process event type
            switch (point.eventType)
            {
                case DungeonEventType.Enemy:
                    EnemyEvent(point);
                    break;

                case DungeonEventType.Treasure:
                    TreasureEvent(point);
                    break;

                case DungeonEventType.Heal:
                    HealEvent(point);
                    break;

                case DungeonEventType.Boss:
                    BossEvent(point);
                    break;

                default:
                    SetEventText(point.message);
                    break;
            }

            return;
        }

        BossEvent(new DungeonEventPoint
        {
            distance = totalDistance,
            eventType = DungeonEventType.Boss,
            message = "最終地点に到達した！"
        });
    }

    private void CheckPreSpawnEnemy()
    {
        if (hasPreSpawnedCurrent) return;

        if (eventPoints != null && nextEventIndex < eventPoints.Length)
        {
            DungeonEventPoint point = eventPoints[nextEventIndex];
            if (point.eventType == DungeonEventType.Enemy || point.eventType == DungeonEventType.Boss)
            {
                int eventDistance = point.distance;
                int distanceToEvent = eventDistance - currentDistance;

                // Pre-spawn enemy when player is within 25m of the event
                if (distanceToEvent <= 25 && distanceToEvent > 0)
                {
                    if (BattleManager.Instance != null && point.enemyData != null)
                    {
                        Vector3 spawnPos = startPosition + Vector3.right * eventDistance;
                        BattleManager.Instance.PreSpawnEnemy(point.enemyData, spawnPos);
                        hasPreSpawnedCurrent = true;
                    }
                }
            }
        }
    }

    private void EnemyEvent(DungeonEventPoint point)
    {
        SetEventText(point.message);

        if (BattleManager.Instance != null && point.enemyData != null)
        {
            // Trigger combat logic (seamlessly uses pre-spawned instance)
            BattleManager.Instance.StartBattle(point.enemyData, this);
        }
        else
        {
            Debug.LogWarning("BattleManager or EnemyData is missing. Skipping combat.");
            NotifyBattleFinished();
        }
    }

    private async void TreasureEvent(DungeonEventPoint point)
    {
        await PerformPickupAnimationAsync(); // Play Pickup pose

        int minG = point.minGold > 0 ? point.minGold : 10;
        int maxG = point.maxGold >= minG ? point.maxGold : 30;
        int gold = Random.Range(minG, maxG + 1);

        await FirebaseManager.Instance.AddGold(gold);

        SetEventText(
            point.distance + "m 地点\n" +
            "宝箱をみつけた！\n" +
            gold + " G 手に入れた。"
        );

        RefreshUI();
    }

    private async void HealEvent(DungeonEventPoint point)
    {
        await PerformPickupAnimationAsync(); // Play Pickup pose

        await FirebaseManager.Instance.HealPlayerFull();

        SetEventText(
            point.distance + "m 地点\n" +
            "回復ポイントをみつけた！\n" +
            "HPが全回復した。"
        );

        RefreshUI();
    }

    private void BossEvent(DungeonEventPoint point)
    {
        SetEventText(point.message);

        if (BattleManager.Instance != null && point.enemyData != null)
        {
            // Trigger combat logic for Boss (seamlessly uses pre-spawned instance)
            BattleManager.Instance.StartBattle(point.enemyData, this);
        }
        else
        {
            Debug.LogWarning("BattleManager or EnemyData is missing. Skipping boss battle.");
            NotifyBattleFinished();
        }
    }

    public void OnClickContinue()
    {
        if (!isWaitingEvent || isCleared)
            return;

        if (currentDistance >= totalDistance)
        {
            OnClickClearDungeon();
            return;
        }

        StartRunToNextEvent();
    }

    public async void OnClickClearDungeon()
    {
        if (isCleared)
            return;

        isCleared = true;

        await FirebaseManager.Instance.UnlockTown(DungeonSession.nextTownIndex);

        string clearScene = DungeonSession.clearSceneName;

        DungeonSession.Clear();

        SceneManager.LoadScene(clearScene);
    }

    public void OnClickGiveUp()
    {
        ReturnToTown();
    }

    public void OnPlayerDead()
    {
        ReturnToTown();
    }

    private void ReturnToTown()
    {
        string returnScene = DungeonSession.returnSceneName;

        DungeonSession.Clear();

        SceneManager.LoadScene(returnScene);
    }

    private void RefreshUI()
    {
        if (areaNameText != null)
            areaNameText.text = DungeonSession.areaName;

        if (distanceText != null)
            distanceText.text = currentDistance + " / " + totalDistance + " m";

        if (statusUI != null)
            statusUI.Refresh();
    }

    private void SetEventText(string text)
    {
        Debug.Log(text);

        if (eventText != null)
            eventText.text = text;
    }

    private int GetSpriteIndexFromName(string name)
    {
        var match = System.Text.RegularExpressions.Regex.Match(name, @"_(\d+)$");
        if (match.Success)
        {
            return int.Parse(match.Groups[1].Value);
        }
        return 0;
    }

    private async Task PerformPickupAnimationAsync()
    {
        if (player == null || pickupSprites == null || pickupSprites.Length == 0) return;

        SpriteRenderer playerRenderer = player.GetComponentInChildren<SpriteRenderer>();
        if (animator == null) animator = player.GetComponentInChildren<Animator>();

        if (playerRenderer != null)
        {
            if (animator != null) animator.enabled = false;

            float frameDelay = 0.015f; 
            for (int i = 0; i < pickupSprites.Length; i++)
            {
                playerRenderer.sprite = pickupSprites[i];
                await Task.Delay((int)(frameDelay * 1000));
            }
        }

        if (animator != null) animator.enabled = true;
    }

    // --- BattleManager Interaction Support Methods ---

    public void ShowBattleText(string text)
    {
        SetEventText(text);
    }

    public void RefreshBattleUI()
    {
        RefreshUI();
    }

    public void HandlePlayerDeath()
    {
        OnPlayerDead();
    }

    public void NotifyBattleFinished()
    {
        RefreshUI();
    }
}