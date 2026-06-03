using UnityEngine;

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

        // Snap to ground Y (Terrain height or Y=0)
        SnapToGround();

        Debug.Log($"Initialized enemy {enemyData.enemyName} with HP {currentHp}/{enemyData.maxHp}");
    }

    private void SnapToGround()
    {
        Vector3 pos = transform.position;
        if (Terrain.activeTerrain != null)
        {
            pos.y = Terrain.activeTerrain.SampleHeight(pos) + Terrain.activeTerrain.transform.position.y;
        }
        else
        {
            pos.y = 0f;
        }
        transform.position = pos;
    }

    void Update()
    {
        if (knockbackTimer > 0f)
        {
            knockbackTimer -= Time.deltaTime;
            float progress = Mathf.Clamp01(knockbackTimer / knockbackDuration); // 1 -> 0
            transform.position = originalPosition + knockbackOffset * progress;
        }
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
