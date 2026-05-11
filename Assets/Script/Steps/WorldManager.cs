using UnityEngine;

public class WorldManager : MonoBehaviour
{
    public Transform player;
    public Transform[] worlds; // 3つ入れる

    private float width;

    void Start()
    {
        // 1個目の幅を取得
        Renderer r = worlds[0].GetComponentInChildren<Renderer>();
        width = r.bounds.size.x;
    }

    void Update()
    {
        Transform left = worlds[0];
        Transform center = worlds[1];
        Transform right = worlds[2];

        // プレイヤーがcenterを超えたら
        if (player.position.x > center.position.x)
        {
            // leftを右に移動
            left.position = right.position + new Vector3(width, 0, 0);

            // 配列を回す
            worlds[0] = center;
            worlds[1] = right;
            worlds[2] = left;
        }
    }
}