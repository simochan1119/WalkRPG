using UnityEngine;

[CreateAssetMenu(fileName = "DungeonStageData", menuName = "ScriptableObjects/DungeonStageData", order = 1)]
public class DungeonStageData : ScriptableObject
{
    public string stageName;
    public int totalDistance = 300;
    public int eventDetectDistance = 10;
    public DungeonEventPoint[] eventPoints;
}
