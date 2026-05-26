using UnityEngine;

public class WorldManager : MonoBehaviour
{
    public Transform player;

    public Transform worldA;
    public Transform worldB;

    private float width;

    void Start()
    {
        Renderer r = worldA.GetComponentInChildren<Renderer>();
        width = r.bounds.size.x;
    }

    void Update()
    {
        // Aが左、Bが右
        Transform left = worldA.position.x < worldB.position.x ? worldA : worldB;
        Transform right = worldA.position.x > worldB.position.x ? worldA : worldB;

        // プレイヤーが右ワールド中央を超えたら
        if (player.position.x > right.position.x)
        {
            left.position = right.position + new Vector3(width, 0, 0);
        }
    }
}