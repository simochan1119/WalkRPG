using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "ScriptableObjects/EnemyData", order = 2)]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public int maxHp = 20;
    public int attack = 5;
    public int rewardGold = 10;
    public int rewardExp = 5;
    public Sprite enemySprite;
    public GameObject enemyPrefab; // Fallback to ZakoPrefab if null
    public RuntimeAnimatorController animatorController;

    [Tooltip("Size ratio relative to player height. Default is 0.6 (60% of player).")]
    public float heightRatio = 0.6f;
}
