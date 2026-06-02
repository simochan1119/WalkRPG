using UnityEngine;
using System.Collections;
using System.Threading.Tasks;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    [Header("Default Prefabs")]
    public GameObject defaultZakoPrefab; // Fallback if enemyPrefab is null in EnemyData

    [Header("Spawn Position")]
    public Transform enemySpawnPoint; // Optional custom spawn point in battle

    [Header("Auto Battle Settings")]
    public float playerAttackInterval = 1.2f;
    public float enemyAttackInterval = 1.5f;
    [Range(0f, 1f)] public float knockbackChance = 0.2f; // 20% chance to trigger knockback/stun on hit

    // Potion stats (temporary in-memory for this battle/dungeon session)
    [Header("Potion Settings")]
    public int maxPotionCount = 3;
    public int potionHealAmount = 50;
    public float potionCooldownDuration = 5f;
    private int currentPotionCount = 3;

    // Skill stats
    [Header("Skill Settings")]
    public float slashCooldownDuration = 5f;

    // Runtime variables
    private DungeonManager activeDungeonManager;
    private EnemyData activeEnemyData;
    private GameObject spawnedEnemyInstance;
    private EnemyController spawnedEnemyController;
    private bool isBattleActive = false;

    // Battle Timers
    private float playerAttackTimer = 0f;
    private float enemyAttackTimer = 0f;
    private float enemyStunTimer = 0f; // Stun duration remaining for the enemy

    // Cooldown variables
    private float slashCooldownTimer = 0f;
    private float potionCooldownTimer = 0f;

    // Public properties for UI bindings
    public int CurrentPotionCount => currentPotionCount;
    public float SlashCooldownRatio => slashCooldownTimer > 0 ? slashCooldownTimer / slashCooldownDuration : 0f;
    public float PotionCooldownRatio => potionCooldownTimer > 0 ? potionCooldownTimer / potionCooldownDuration : 0f;
    public float SlashCooldownRemaining => slashCooldownTimer;
    public float PotionCooldownRemaining => potionCooldownTimer;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Update()
    {
        if (!isBattleActive) return;

        // 1. Manage Cooldowns
        if (slashCooldownTimer > 0f) slashCooldownTimer -= Time.deltaTime;
        if (potionCooldownTimer > 0f) potionCooldownTimer -= Time.deltaTime;

        // 2. Manage Enemy Stun / Knockback State
        if (enemyStunTimer > 0f)
        {
            enemyStunTimer -= Time.deltaTime;
            // Enemy is stunned, do not tick their attack timer
        }
        else
        {
            // Enemy ticks attack timer
            enemyAttackTimer += Time.deltaTime;
            if (enemyAttackTimer >= enemyAttackInterval)
            {
                enemyAttackTimer = 0f;
                ExecuteEnemyAttack();
            }
        }

        // 3. Player ticks automatic attack timer
        playerAttackTimer += Time.deltaTime;
        if (playerAttackTimer >= playerAttackInterval)
        {
            playerAttackTimer = 0f;
            ExecutePlayerAutoAttack();
        }
    }

    /// <summary>
    /// Pre-spawns an enemy at a fixed world position (off-screen) before combat starts.
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
        activeEnemyData = enemyData;

        // Reset timers and resources for a new battle
        playerAttackTimer = 0f;
        enemyAttackTimer = 0f;
        enemyStunTimer = 0f;
        
        // Potion reset (simulate inventory replenishment per battle for now)
        currentPotionCount = maxPotionCount;

        if (spawnedEnemyInstance == null)
        {
            // Fallback: If not pre-spawned, spawn it right now
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

        isBattleActive = true;
        Debug.Log($"Battle active against {activeEnemyData.enemyName}! Autoplay active.");

        if (activeDungeonManager != null)
        {
            activeDungeonManager.ShowBattleText($"{activeEnemyData.enemyName}が現れた！\n自動戦闘を開始します！");
        }
    }

    /// <summary>
    /// Automatic regular attack from the Player
    /// </summary>
    private void ExecutePlayerAutoAttack()
    {
        if (!isBattleActive || spawnedEnemyController == null) return;

        int playerDamage = 10;
        if (FirebaseManager.Instance != null && FirebaseManager.Instance.CurrentPlayer != null)
        {
            playerDamage = FirebaseManager.Instance.CurrentPlayer.level * 4;
        }

        spawnedEnemyController.TakeDamage(playerDamage);

        // Try triggering a Knockback (stun) effect
        bool didKnockback = TryTriggerKnockback();
        string kbText = didKnockback ? " (ノックバック！)" : "";

        if (activeDungeonManager != null)
        {
            activeDungeonManager.ShowBattleText($"{activeEnemyData.enemyName}に {playerDamage} の自動攻撃ダメージ！{kbText}");
        }

        CheckBattleStatus();
    }

    /// <summary>
    /// Automatic attack from the Enemy
    /// </summary>
    private void ExecuteEnemyAttack()
    {
        if (!isBattleActive || spawnedEnemyController == null) return;

        int enemyDamage = activeEnemyData.attack;
        
        // Apply damage to player
        if (FirebaseManager.Instance != null)
        {
            // Execute as background async task safely
            _ = FirebaseManager.Instance.DamagePlayer(enemyDamage);
        }

        if (activeDungeonManager != null)
        {
            activeDungeonManager.ShowBattleText($"{activeEnemyData.enemyName}の攻撃！\n{enemyDamage} ダメージを受けた！");
            activeDungeonManager.RefreshBattleUI();
        }

        // Check if player is dead
        if (FirebaseManager.Instance != null && FirebaseManager.Instance.CurrentPlayer.hp <= 0)
        {
            HandlePlayerLoss();
        }
    }

    /// <summary>
    /// Hook to trigger a knockback / stun state.
    /// </summary>
    private bool TryTriggerKnockback()
    {
        if (Random.value <= knockbackChance)
        {
            // Trigger 1 second stun on the enemy
            enemyStunTimer = 1.0f;
            Debug.Log("[BattleManager] Knockback triggered! Enemy is stunned for 1 second.");
            return true;
        }
        return false;
    }

    /// <summary>
    /// Active Skill: Slash (CD 5s)
    /// </summary>
    public void UseSlash()
    {
        if (!isBattleActive || slashCooldownTimer > 0f || spawnedEnemyController == null) return;

        int playerLevel = 1;
        if (FirebaseManager.Instance != null && FirebaseManager.Instance.CurrentPlayer != null)
        {
            playerLevel = FirebaseManager.Instance.CurrentPlayer.level;
        }

        int skillDamage = playerLevel * 15;
        slashCooldownTimer = slashCooldownDuration; // Set cooldown

        spawnedEnemyController.TakeDamage(skillDamage);

        // Slash also has a slightly higher chance to knockback (e.g., 40%)
        bool didKnockback = Random.value <= 0.4f;
        if (didKnockback) enemyStunTimer = 1.2f;
        string kbText = didKnockback ? " (強力ノックバック！)" : "";

        if (activeDungeonManager != null)
        {
            activeDungeonManager.ShowBattleText($"強撃（スラッシュ）を発動！\n{activeEnemyData.enemyName}に {skillDamage} ダメージ！{kbText}");
        }

        CheckBattleStatus();
    }

    /// <summary>
    /// Consumable Item: Potion (CD 5s, restores 50 HP)
    /// </summary>
    public void UsePotion()
    {
        if (!isBattleActive || potionCooldownTimer > 0f || currentPotionCount <= 0) return;

        if (FirebaseManager.Instance == null || FirebaseManager.Instance.CurrentPlayer == null) return;

        PlayerData player = FirebaseManager.Instance.CurrentPlayer;
        if (player.hp >= player.maxHp)
        {
            if (activeDungeonManager != null)
            {
                activeDungeonManager.ShowBattleText("HPが満タンのためポーションは不要です！");
            }
            return;
        }

        currentPotionCount--;
        potionCooldownTimer = potionCooldownDuration;

        // Perform healing logic
        int healVal = Mathf.Min(potionHealAmount, player.maxHp - player.hp);
        
        // Call firebase heal wrapper or do manual update
        _ = HealPlayerAmount(healVal);

        if (activeDungeonManager != null)
        {
            activeDungeonManager.ShowBattleText($"ポーションを使用した！\nHPが {healVal} 回復した！（残り: {currentPotionCount}個）");
            activeDungeonManager.RefreshBattleUI();
        }
    }

    private async Task HealPlayerAmount(int amount)
    {
        if (FirebaseManager.Instance == null) return;
        // In the database, let's damage player with negative amount to heal them
        await FirebaseManager.Instance.DamagePlayer(-amount);
    }

    private void CheckBattleStatus()
    {
        if (spawnedEnemyController != null && spawnedEnemyController.IsDead())
        {
            isBattleActive = false;
            _ = HandleBattleWin();
        }
    }

    private void HandlePlayerLoss()
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

    private async Task HandleBattleWin()
    {
        isBattleActive = false;

        // Process reward using RewardManager encapsulating class
        if (RewardManager.Instance != null)
        {
            await RewardManager.Instance.ProcessBattleReward(activeEnemyData);
        }
        else
        {
            // Fallback direct payout
            if (FirebaseManager.Instance != null)
            {
                await FirebaseManager.Instance.AddGold(activeEnemyData.rewardGold);
            }
            if (activeDungeonManager != null)
            {
                activeDungeonManager.ShowBattleText($"{activeEnemyData.enemyName}を倒した！\n{activeEnemyData.rewardGold} G 獲得！");
            }
        }

        // Wait 1.5 seconds so the player can read the reward text
        await Task.Delay(1500);

        if (spawnedEnemyInstance != null)
        {
            Destroy(spawnedEnemyInstance);
        }

        // Notify finished
        if (activeDungeonManager != null)
        {
            activeDungeonManager.NotifyBattleFinished();
            
            // AUTOMATIC PROGRESSION: Auto-continue exploration without manual click
            activeDungeonManager.OnClickContinue();
        }
    }
}

