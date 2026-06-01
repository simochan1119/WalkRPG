using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public enum DungeonEventType
{
    Enemy,
    Treasure,
    Boss,
    Heal,
    None
}

[System.Serializable]
public class DungeonEventPoint
{
    public int distance;
    public DungeonEventType eventType;
    public string message;
}

public class DungeonManager : GameSceneManager
{
    [Header("Player")]
    public Transform player;
    public Animator animator;
    public float moveSpeed = 3f;

    [Header("Dungeon UI")]
    public TMP_Text areaNameText;
    public TMP_Text distanceText;
    public TMP_Text eventText;

    [Header("Event Points")]
    public DungeonEventPoint[] eventPoints;

    [Header("Event Detection")]
    [Tooltip("イベント地点の何m手前で反応するか")]
    public int eventDetectDistance = 10;

    [Header("Event VFX")]
    [Tooltip("びっくりマークなどのVFX Prefab")]
    public GameObject surpriseVfxPrefab;

    [Tooltip("VFXを出す位置。未設定ならPlayerの頭上に出します")]
    public Transform surpriseVfxPoint;

    [Tooltip("VFXを表示してからイベント処理に入るまでの時間")]
    public float surpriseVfxTime = 0.8f;

    [Tooltip("surpriseVfxPoint未設定時、Playerからどれだけ上に出すか")]
    public Vector3 defaultVfxOffset = new Vector3(0f, 2f, 0f);

    private int currentDistance = 0;
    private int totalDistance = 300;
    private int nextEventIndex = 0;

    private bool isRunning = false;
    private bool isWaitingEvent = false;
    private bool isCleared = false;
    private bool isEventStarting = false;

    private Vector3 startPosition;
    private Vector3 targetPosition;

    protected override void OnSceneReady()
    {
        if (!DungeonSession.HasSession)
        {
            SceneManager.LoadScene("02_Village");
            return;
        }

        totalDistance = DungeonSession.totalDistance;

        if (player != null)
        {
            startPosition = player.position;
            targetPosition = startPosition;
        }

        if (animator == null && player != null)
        {
            animator = player.GetComponent<Animator>();
        }

        currentDistance = 0;
        nextEventIndex = 0;
        isCleared = false;
        isRunning = false;
        isWaitingEvent = false;
        isEventStarting = false;

        RefreshUI();
        SetEventText(DungeonSession.areaName + " の探索開始！");

        StartRunToNextEvent();
    }

    private void Update()
    {
        if (!isRunning || player == null)
            return;

        player.position = Vector3.MoveTowards(
            player.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        currentDistance = Mathf.RoundToInt(
            player.position.x - startPosition.x
        );

        RefreshUI();

        if (Vector3.Distance(player.position, targetPosition) <= 0.01f)
        {
            isRunning = false;

            if (animator != null)
                animator.SetBool("Walk", false);

            if (!isEventStarting)
            {
                StartCoroutine(TriggerCurrentEventWithVfx());
            }
        }
    }

    private void StartRunToNextEvent()
    {
        if (isCleared)
            return;

        isWaitingEvent = false;
        isEventStarting = false;

        int nextDistance = GetNextTargetDistance();

        targetPosition = startPosition + Vector3.right * nextDistance;

        isRunning = true;

        if (animator != null)
            animator.SetBool("Walk", true);

        SetEventText("探索中...");
    }

    private int GetNextTargetDistance()
    {
        if (eventPoints != null && nextEventIndex < eventPoints.Length)
        {
            int eventDistance = eventPoints[nextEventIndex].distance;

            // 例：100m地点のイベントなら、10m手前の90mで停止する
            int detectStartDistance = eventDistance - eventDetectDistance;

            return Mathf.Clamp(detectStartDistance, 0, totalDistance);
        }

        return totalDistance;
    }

    private IEnumerator TriggerCurrentEventWithVfx()
    {
        isEventStarting = true;

        GameObject vfx = null;

        if (surpriseVfxPrefab != null && player != null)
        {
            Vector3 spawnPosition = surpriseVfxPoint != null
                ? surpriseVfxPoint.position
                : player.position + defaultVfxOffset;

            vfx = Instantiate(surpriseVfxPrefab, spawnPosition, Quaternion.identity);
        }

        if (surpriseVfxTime > 0f)
        {
            yield return new WaitForSeconds(surpriseVfxTime);
        }

        if (vfx != null)
        {
            Destroy(vfx);
        }

        TriggerCurrentEvent();

        isEventStarting = false;
    }

    private void TriggerCurrentEvent()
    {
        isWaitingEvent = true;

        if (eventPoints != null && nextEventIndex < eventPoints.Length)
        {
            DungeonEventPoint point = eventPoints[nextEventIndex];
            nextEventIndex++;

            switch (point.eventType)
            {
                case DungeonEventType.Enemy:
                    EnemyEvent(point);
                    break;

                case DungeonEventType.Treasure:
                    TreasureEvent(point);
                    break;

                case DungeonEventType.Heal:
                    HealEvent(point);
                    break;

                case DungeonEventType.Boss:
                    BossEvent(point);
                    break;

                default:
                    SetEventText(point.message);
                    break;
            }

            return;
        }

        BossEvent(new DungeonEventPoint
        {
            distance = totalDistance,
            eventType = DungeonEventType.Boss,
            message = "最奥に到達した！"
        });
    }

    private async void EnemyEvent(DungeonEventPoint point)
    {
        int damage = Random.Range(3, 8);

        await FirebaseManager.Instance.DamagePlayer(damage);

        SetEventText(
            point.distance + "m 地点\n" +
            "敵が現れた！\n" +
            damage + " ダメージを受けた。"
        );

        RefreshUI();

        if (FirebaseManager.Instance.CurrentPlayer.hp <= 0)
        {
            OnPlayerDead();
        }
    }

    private async void TreasureEvent(DungeonEventPoint point)
    {
        int gold = Random.Range(10, 31);

        await FirebaseManager.Instance.AddGold(gold);

        SetEventText(
            point.distance + "m 地点\n" +
            "宝箱を見つけた！\n" +
            gold + " G を手に入れた。"
        );

        RefreshUI();
    }

    private async void HealEvent(DungeonEventPoint point)
    {
        await FirebaseManager.Instance.HealPlayerFull();

        SetEventText(
            point.distance + "m 地点\n" +
            "休憩ポイントを見つけた！\n" +
            "HPが全回復した。"
        );

        RefreshUI();
    }

    private void BossEvent(DungeonEventPoint point)
    {
        SetEventText(
            point.distance + "m 地点\n" +
            "ボスが現れた！\n" +
            "勝利すると次の町へ進めます。"
        );
    }

    public void OnClickContinue()
    {
        if (!isWaitingEvent || isCleared)
            return;

        if (currentDistance >= totalDistance)
        {
            OnClickClearDungeon();
            return;
        }

        StartRunToNextEvent();
    }

    public async void OnClickClearDungeon()
    {
        if (isCleared)
            return;

        isCleared = true;

        await FirebaseManager.Instance.UnlockTown(DungeonSession.nextTownIndex);

        string clearScene = DungeonSession.clearSceneName;

        DungeonSession.Clear();

        SceneManager.LoadScene(clearScene);
    }

    public void OnClickGiveUp()
    {
        ReturnToTown();
    }

    private void OnPlayerDead()
    {
        ReturnToTown();
    }

    private void ReturnToTown()
    {
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