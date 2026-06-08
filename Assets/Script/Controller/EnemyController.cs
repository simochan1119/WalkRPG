using UnityEngine;
using System.Threading.Tasks;

public class EnemyController : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    
    private EnemyData enemyData;
    private int currentHp;

    private Vector3 originalPosition;
    private float knockbackTimer = 0f;
    private float knockbackDuration = 0.25f;
    private Vector3 knockbackOffset;

    public void Initialize(EnemyData data)
    {
        enemyData = data;
        currentHp = data.maxHp;

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null && data.enemySprite != null)
        {
            spriteRenderer.sprite = data.enemySprite;
        }

        // Setup animator controller dynamically if provided
        Animator animator = GetComponent<Animator>();
        if (animator != null && data.animatorController != null)
        {
            animator.runtimeAnimatorController = data.animatorController;
        }

        // Immediately adjust on initialization
        AdjustScaleAndPosition();

        Debug.Log($"Initialized enemy {enemyData.enemyName} with HP {currentHp}/{enemyData.maxHp}");
    }

    private void AdjustScaleAndPosition()
    {
        if (spriteRenderer == null || spriteRenderer.sprite == null) return;

        // 1. Get player bottom Y and height
        float playerHeight = 2.0f;
        float playerBottomY = 0f;

        if (DungeonManager.Instance != null && DungeonManager.Instance.player != null)
        {
            Transform playerTrans = DungeonManager.Instance.player;
            SpriteRenderer playerRenderer = playerTrans.GetComponentInChildren<SpriteRenderer>();
            
            if (playerRenderer != null && playerRenderer.sprite != null)
            {
                playerHeight = (playerRenderer.sprite.rect.height / playerRenderer.sprite.pixelsPerUnit) * playerTrans.localScale.y;
                float playerDistFromPivotToBottom = (playerRenderer.sprite.pivot.y / playerRenderer.sprite.pixelsPerUnit) * playerTrans.localScale.y;
                playerBottomY = playerTrans.position.y - playerDistFromPivotToBottom;
            }
            else
            {
                playerBottomY = playerTrans.position.y;
            }
        }
        else
        {
            // Terrain fallback if DungeonManager player is not available
            float groundY = 0f;
            if (Terrain.activeTerrain != null)
            {
                groundY = Terrain.activeTerrain.SampleHeight(transform.position) + Terrain.activeTerrain.transform.position.y;
            }
            playerBottomY = groundY;
        }

        // 2. Calculate target uniform scale based on actual current sprite
        float ratio = (enemyData != null && enemyData.heightRatio > 0f) ? enemyData.heightRatio : 0.6f;
        float enemySpriteHeight = spriteRenderer.sprite.rect.height / spriteRenderer.sprite.pixelsPerUnit;
        float targetHeight = playerHeight * ratio;
        
        float scaleFactor = targetHeight / enemySpriteHeight;

        // Apply uniform scale (proportional width & height)
        transform.localScale = new Vector3(scaleFactor, scaleFactor, 1f);

        // 3. Snap bottom to ground Y reference
        float distFromPivotToBottom = (spriteRenderer.sprite.pivot.y / spriteRenderer.sprite.pixelsPerUnit) * scaleFactor;
        
        Vector3 pos = transform.position;
        pos.y = playerBottomY + distFromPivotToBottom;
        transform.position = pos;
    }

    void Update()
    {
        if (knockbackTimer > 0f)
        {
            knockbackTimer -= Time.deltaTime;
            float progress = Mathf.Clamp01(knockbackTimer / knockbackDuration); // 1 -> 0
            
            // Only slide horizontally to prevent Y conflict with ground snapping
            Vector3 pos = transform.position;
            pos.x = originalPosition.x + knockbackOffset.x * progress;
            transform.position = pos;
        }
    }

    void LateUpdate()
    {
        // Keep scale and Y ground position perfectly aligned to the actual current sprite frame
        AdjustScaleAndPosition();
    }

    public int GetCurrentHp() => currentHp;
    public int GetMaxHp() => enemyData != null ? enemyData.maxHp : 0;
    public EnemyData GetEnemyData() => enemyData;

    public void PlayAttack()
    {
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
    }

    public void ApplyKnockback(float distance)
    {
        if (knockbackTimer <= 0f)
        {
            originalPosition = transform.position;
        }
        knockbackOffset = Vector3.right * distance;
        knockbackTimer = knockbackDuration;
    }

    public void TakeDamage(int damage)
    {
        currentHp = Mathf.Max(0, currentHp - damage);
        Debug.Log($"{enemyData.enemyName} took {damage} damage. Current HP: {currentHp}");
    }

    public bool IsDead()
    {
        return currentHp <= 0;
    }
}