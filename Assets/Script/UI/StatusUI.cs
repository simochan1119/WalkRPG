using UnityEngine;
using TMPro;

public class StatusUI : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text characterName;
    public TMP_Text characterHp;
    public TMP_Text characterLevel;
    public TMP_Text characterSteps;
    public TMP_Text characterMoney;

    public void Refresh()
    {
        // Firebase確認
        if (FirebaseManager.Instance == null)
            return;

        // Player確認
        var player = FirebaseManager.Instance.CurrentPlayer;

        if (player == null)
            return;

        // UI更新
        characterName.text = player.name;

        characterHp.text =
            "HP : " +
            player.hp.ToString() +
            "/" +
            player.maxHp.ToString();

        characterLevel.text =
            "LEVEL : " +
            player.level.ToString();

        characterSteps.text =
            "STEPS : " +
            player.steps.ToString();

        characterMoney.text =
            "GOLD : " +
            player.gold.ToString();
    }
}