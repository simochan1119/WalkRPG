using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    
    private EnemyData enemyData;
    private int currentHp;

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

        Debug.Log($"Initialized enemy {enemyData.enemyName} with HP {currentHp}/{enemyData.maxHp}");
    }

    public int GetCurrentHp() => currentHp;
    public int GetMaxHp() => enemyData != null ? enemyData.maxHp : 0;
    public EnemyData GetEnemyData() => enemyData;

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
