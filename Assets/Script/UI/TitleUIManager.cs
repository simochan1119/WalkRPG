using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class TitleUIManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_InputField nameInput;
    public TMP_Text infoText;
    [Header("Popup")]
    public GameObject activityPopup;
    public GameObject notificationPopup;

    private const string VillageSceneName = "02_Village";

    void Start()
    {
        var player = FirebaseManager.Instance.CurrentPlayer;

        if (player != null)
        {
            infoText.text = $"ようこそ、{player.name} さん";
            nameInput.text = player.name;
        }
        else
        {
            infoText.text = "ユーザー名を入力してください";
        }

        if (!PermissionManager.Instance.HasActivityPermission)
        {
            activityPopup.SetActive(true);
        }

        // 通知権限
        if (!PermissionManager.Instance.HasNotificationPermission)
        {
            notificationPopup.SetActive(true);
        }
    }

    public async void OnClickGameStart()
    {
        infoText.text = "保存中...";

        await FirebaseManager.Instance.CreateOrUpdatePlayer(nameInput.text);

        SceneManager.LoadScene(VillageSceneName);
    }
    public void OnClickNotificationAllow()
    {
        PermissionManager.Instance.RequestNotificationPermission();

        notificationPopup.SetActive(false);
    }
    public void OnClickNotificationLater()
    {
        notificationPopup.SetActive(false);
    }
    public void OnClickActivityAllow()
    {
        PermissionManager.Instance.RequestActivityPermission();

        activityPopup.SetActive(false);
    }
    public void OnClickActivityLater()
    {
        activityPopup.SetActive(false);
    }
}