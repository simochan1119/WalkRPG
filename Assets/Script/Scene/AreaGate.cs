using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class AreaGate : MonoBehaviour
{
    [Header("Area Settings")]
    public string areaName = "草原";
    public int requiredSteps = 300;
    public int totalDistance = 300;
    public int eventInterval = 100;

    [Header("Scene Settings")]
    public string dungeonSceneName = "03_Field1";
    public string returnSceneName = "02_Village";
    public string clearSceneName = "02_Village";

    [Header("Progress")]
    public int nextTownIndex = 1;

    [Header("UI")]
    public GameObject messagePanel;
    public TMP_Text messageText;

    private bool isEntering = false;

    private async void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (isEntering)
            return;

        isEntering = true;

        if (FirebaseManager.Instance == null ||
            FirebaseManager.Instance.CurrentPlayer == null)
        {
            ShowMessage("プレイヤーデータがありません");
            isEntering = false;
            return;
        }

        var player = FirebaseManager.Instance.CurrentPlayer;

        if (player.usableSteps < requiredSteps)
        {
            ShowMessage(
                areaName + "に入るには " +
                requiredSteps + "歩 必要です\n" +
                "現在の行動力 : " + player.usableSteps
            );

            isEntering = false;
            return;
        }

        bool success =
            await FirebaseManager.Instance.TryUseSteps(requiredSteps);

        if (!success)
        {
            ShowMessage("行動力が足りません");
            isEntering = false;
            return;
        }

        DungeonSession.StartSession(
            areaName,
            requiredSteps,
            totalDistance,
            eventInterval,
            nextTownIndex,
            returnSceneName,
            clearSceneName
        );

        SceneManager.LoadScene(dungeonSceneName);
    }

    private void ShowMessage(string message)
    {
        Debug.Log(message);

        if (messagePanel != null)
            messagePanel.SetActive(true);

        if (messageText != null)
            messageText.text = message;
    }
}