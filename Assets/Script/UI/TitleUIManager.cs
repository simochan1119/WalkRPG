using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class TitleUIManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_InputField nameInput;
    public TMP_Text infoText;

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
    }

    public async void OnClickGameStart()
    {
        infoText.text = "保存中...";

        await FirebaseManager.Instance.CreateOrUpdatePlayer(nameInput.text);

        SceneManager.LoadScene(VillageSceneName);
    }
}