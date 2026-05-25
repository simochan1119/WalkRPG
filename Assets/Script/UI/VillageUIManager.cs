using UnityEngine;
using TMPro;

public class VillageUIManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text statusText;
    public TMP_Text characterName;
    public TMP_Text characterHp;
    public TMP_Text characterLevel;
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
        characterHp.text = "HP : " + player.hp.ToString();
        characterLevel.text = "LEVEL : " + player.level.ToString();
        characterSteps.text = "STEPS : " + player.steps.ToString();
        characterMoney.text = "GOLD : " +player.gold.ToString();




    }
}