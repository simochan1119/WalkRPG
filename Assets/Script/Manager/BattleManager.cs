using UnityEngine;
using System.Threading.Tasks;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    [Header("Default Prefabs")]
    public GameObject defaultZakoPrefab; // Fallback if enemyPrefab is null in EnemyData

    [Header("Spawn Position")]
    public Transform enemySpawnPoint; // Optional custom spawn point in battle

    private DungeonManager activeDungeonManager;
    private EnemyData activeEnemyData;
    private GameObject spawnedEnemyInstance;
    private EnemyController spawnedEnemyController;
    private bool isBattleActive = false;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Pre-spawns an enemy at a fixed world position (off-screen) before combat starts
    /// to avoid pop-in and allow the enemy to scroll naturally into view.
    /// </summary>
    public void PreSpawnEnemy(EnemyData enemyData, Vector3 position)
    {
        if (enemyData == null) return;
        if (spawnedEnemyInstance != null) return; // Already spawned

        activeEnemyData = enemyData;

        GameObject prefabToSpawn = enemyData.enemyPrefab != null ? enemyData.enemyPrefab : defaultZakoPrefab;

        if (prefabToSpawn == null)
        {
            Debug.LogWarning("Cannot pre-spawn enemy: Both enemyPrefab and defaultZakoPrefab are null!");
            return;
        }

        // Spawn at absolute event world coordinate
        spawnedEnemyInstance = Instantiate(prefabToSpawn, position, Quaternion.identity);

        // Get or add EnemyController
        spawnedEnemyController = spawnedEnemyInstance.GetComponent<EnemyController>();
        if (spawnedEnemyController == null)
        {
            spawnedEnemyController = spawnedEnemyInstance.AddComponent<EnemyController>();
        }

        // Initialize enemy with data
        spawnedEnemyController.Initialize(enemyData);
        
        Debug.Log($"Pre-spawned {enemyData.enemyName} at position {position}");
    }

    /// <summary>
    /// Starts the battle. Seamlessly uses the pre-spawned enemy instance if it exists.
    /// </summary>
    public void StartBattle(EnemyData enemyData, DungeonManager dungeonManager)
    {
        activeDungeonManager = dungeonManager;
        isBattleActive = true;

        if (spawnedEnemyInstance == null)
        {
            // Fallback: If not pre-spawned, spawn it right now
            activeEnemyData = enemyData;
            GameObject prefabToSpawn = enemyData.enemyPrefab != null ? enemyData.enemyPrefab : defaultZakoPrefab;

            if (prefabToSpawn == null)
            {
                Debug.LogError("Cannot spawn enemy: Both enemyPrefab and defaultZakoPrefab are null!");
                return;
            }

            Vector3 spawnPos = Vector3.zero;
            if (enemySpawnPoint != null)
            {
                spawnPos = enemySpawnPoint.position;
            }
            else if (dungeonManager != null && dungeonManager.player != null)
            {
                spawnPos = dungeonManager.player.position + Vector3.right * 3f;
            }

            spawnedEnemyInstance = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
            spawnedEnemyController = spawnedEnemyInstance.GetComponent<EnemyController>();
            if (spawnedEnemyController == null)
            {
                spawnedEnemyController = spawnedEnemyInstance.AddComponent<EnemyController>();
            }
            spawnedEnemyController.Initialize(enemyData);
        }

        Debug.Log($"Battle active against {activeEnemyData.enemyName}!");
    }

    public async void OnClickAttack()
    {
        if (!isBattleActive || spawnedEnemyController == null)
            return;

        // Player attacks enemy
        int playerDamage = 10; // Default flat damage
        if (FirebaseManager.Instance != null && FirebaseManager.Instance.CurrentPlayer != null)
        {
            // Simple damage scaling: level * 5
            playerDamage = FirebaseManager.Instance.CurrentPlayer.level * 5;
        }

        spawnedEnemyController.TakeDamage(playerDamage);

        if (activeDungeonManager != null)
        {
            activeDungeonManager.ShowBattleText($"{activeEnemyData.enemyName}に {playerDamage} ダメージを与えた！");
        }

        if (spawnedEnemyController.IsDead())
        {
            isBattleActive = false;
            await HandleBattleWin();
        }
        else
        {
            await HandleEnemyTurn();
        }
    }

    private async Task HandleEnemyTurn()
    {
        if (spawnedEnemyController == null) return;

        int enemyDamage = activeEnemyData.attack;
        
        // Deal damage to player
        if (FirebaseManager.Instance != null)
        {
            await FirebaseManager.Instance.DamagePlayer(enemyDamage);
        }

        if (activeDungeonManager != null)
        {
            activeDungeonManager.ShowBattleText(
                $"{activeEnemyData.enemyName}の反撃！\\n" +
                $"{enemyDamage} ダメージを受けた！"
            );
            activeDungeonManager.RefreshBattleUI();
        }

        // Check if player is dead
        if (FirebaseManager.Instance != null && FirebaseManager.Instance.CurrentPlayer.hp <= 0)
        {
            isBattleActive = false;
            if (spawnedEnemyInstance != null)
            {
                Destroy(spawnedEnemyInstance);
            }
            if (activeDungeonManager != null)
            {
                activeDungeonManager.HandlePlayerDeath();
            }
        }
    }

    private async Task HandleBattleWin()
    {
        int goldReward = activeEnemyData.rewardGold;
        
        // Add gold to player
        if (FirebaseManager.Instance != null)
        {
            await FirebaseManager.Instance.AddGold(goldReward);
        }

        if (activeDungeonManager != null)
        {
            activeDungeonManager.ShowBattleText(
                $"{activeEnemyData.enemyName}を倒した！\\n" +
                $"{goldReward} G を獲得した！"
            );
        }

        // Give a short delay to let the player read the text
        await Task.Delay(1500);

        if (spawnedEnemyInstance != null)
        {
            Destroy(spawnedEnemyInstance);
        }

        isBattleActive = false;

        if (activeDungeonManager != null)
        {
            activeDungeonManager.NotifyBattleFinished();
        }
    }
}
