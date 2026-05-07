using UnityEngine;
using TMPro;

public class VillageUIManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text statusText;

    void Start()
    {
        var player = FirebaseManager.Instance.CurrentPlayer;

        if (player == null)
        {
            statusText.text = "プレイヤーデータがありません";
            return;
        }

        statusText.text =
            $"名前：{player.name}\n" +
            $"UID：{player.uid}\n" +
            $"Lv：{player.level}\n" +
            $"Gold：{player.gold}\n" +
            $"歩数：{player.steps}";
    }
}