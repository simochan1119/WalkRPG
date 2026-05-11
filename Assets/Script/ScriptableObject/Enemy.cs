using UnityEngine;

[CreateAssetMenu(fileName = "Enemy", menuName = "Scriptable Objects/Enemy")]
public class Enemy : ScriptableObject
{
    public int EID;
    public string enemyName;
    public int HP;
    public int Attack;
    public int Deffence;
}
