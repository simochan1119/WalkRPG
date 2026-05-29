using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class DungeonManager : GameSceneManager
{
    [Header("Dungeon UI")]
    public TMP_Text areaNameText;
    public TMP_Text distanceText;
    public TMP_Text eventText;

    [Header("Test Buttons")]
    public GameObject advanceButton;
    public GameObject battleButton;
    public GameObject treasureButton;

    private int currentDistance = 0;
    private int totalDistance = 300;
    private int eventInterval = 100;
    private bool isCleared = false;

    protected override void OnSceneReady()
    {
        if (!DungeonSession.HasSession)
        {
            Debug.LogWarning("DungeonSession がありません。村へ戻ります。");
            SceneManager.LoadScene("02_Village");
            return;
        }

        totalDistance = DungeonSession.totalDistance;
        eventInterval = DungeonSession.eventInterval;

        if (eventInterval <= 0)
            eventInterval = 100;

        currentDistance = 0;
        isCleared = false;

        RefreshUI();

        SetEventText(
            DungeonSession.areaName +
            " の探索を開始しました\n" +
            totalDistance + "m 先を目指そう！"
        );
    }

    public void OnClickAdvance()
    {
        if (isCleared)
            return;

        currentDistance += eventInterval;

        if (currentDistance >= totalDistance)
        {
            currentDistance = totalDistance;
            BossEvent();
            RefreshUI();
            return;
        }

        RandomEvent();
        RefreshUI();
    }

    private void RandomEvent()
    {
        int r = Random.Range(0, 100);

        if (r < 60)
        {
            EnemyEvent();
        }
        else
        {
            TreasureEvent();
        }
    }

    private void EnemyEvent()
    {
        int damage = Random.Range(3, 8);

        SetEventText(
            currentDistance + "m 地点\n" +
            "敵が現れた！\n" +
            "戦闘で " + damage + " ダメージを受けた。"
        );

        _ = FirebaseManager.Instance.DamagePlayer(damage);

        if (FirebaseManager.Instance.CurrentPlayer.hp <= 0)
        {
            OnPlayerDead();
        }
        else
        {
            RefreshStatus();
        }
    }

    private async void TreasureEvent()
    {
        int gold = Random.Range(10, 31);

        await FirebaseManager.Instance.AddGold(gold);

        SetEventText(
            currentDistance + "m 地点\n" +
            "宝箱を見つけた！\n" +
            gold + " G を手に入れた。"
        );

        RefreshStatus();
    }

    private void BossEvent()
    {
        SetEventText(
            totalDistance + "m 地点\n" +
            "ボスが現れた！\n" +
            "突破ボタンでクリア扱いにします。"
        );
    }

    public async void OnClickClearDungeon()
    {
        if (isCleared)
            return;

        isCleared = true;

        await FirebaseManager.Instance.UnlockTown(DungeonSession.nextTownIndex);

        SetEventText(
            DungeonSession.areaName +
            " を突破した！\n" +
            "次の町が解放されました。"
        );

        string clearScene = DungeonSession.clearSceneName;

        DungeonSession.Clear();

        SceneManager.LoadScene(clearScene);
    }

    public void OnClickGiveUp()
    {
        ReturnToTown("探索を中断しました。町へ戻ります。");
    }

    private void OnPlayerDead()
    {
        ReturnToTown("HPが0になりました。町へ戻ります。");
    }

    private void ReturnToTown(string message)
    {
        Debug.Log(message);

        string returnScene = DungeonSession.returnSceneName;

        DungeonSession.Clear();

        SceneManager.LoadScene(returnScene);
    }

    private void RefreshUI()
    {
        if (areaNameText != null)
            areaNameText.text = DungeonSession.areaName;

        if (distanceText != null)
            distanceText.text = currentDistance + " / " + totalDistance + " m";

        RefreshStatus();
    }

    private void RefreshStatus()
    {
        if (statusUI != null)
            statusUI.Refresh();
    }

    private void SetEventText(string text)
    {
        Debug.Log(text);

        if (eventText != null)
            eventText.text = text;
    }
}