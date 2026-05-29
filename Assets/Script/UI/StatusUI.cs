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
        if (FirebaseManager.Instance == null)
            return;

        var player = FirebaseManager.Instance.CurrentPlayer;

        if (player == null)
            return;

        if (characterName != null)
            characterName.text = player.name;

        if (characterHp != null)
            characterHp.text =
                "HP : " +
                player.hp.ToString() +
                "/" +
                player.maxHp.ToString();

        if (characterLevel != null)
            characterLevel.text =
                "LEVEL : " +
                player.level.ToString();

        if (characterSteps != null)
            characterSteps.text =
                "çsìÆóÕ : " + player.usableSteps +
                "\nç°ì˙ : " + player.todaySteps +
                "\nó›åv : " + player.totalSteps;

        if (characterMoney != null)
            characterMoney.text =
                "GOLD : " +
                player.gold.ToString();
    }
}