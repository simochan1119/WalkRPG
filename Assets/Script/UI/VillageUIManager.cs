using UnityEngine;
using TMPro;

public class VillageUIManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text statusText;
    public TMP_Text characterName;
    public TMP_Text characterHp;
    public TMP_Text characterMp;
    public TMP_Text characterSteps;
    public TMP_Text characterMoney;

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

        characterName.text = player.name;
        //characterHp.text = player.hp; あとからじっそう
        //characterMp.text = player.mp; あとからじっそう
        //characterSteps.text = player.steps.ToString; あとからじっそう
        //characterMoney.text = player.gold;




    }
}