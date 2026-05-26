using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour
{
    public StatusUI statusUI;

    protected virtual void Start()
    {
        if (!CheckPlayerData())
            return;

        InitializeCommonUI();
        OnSceneReady();
    }

    protected bool CheckPlayerData()
    {
        if (FirebaseManager.Instance == null)
        {
            SceneManager.LoadScene("00_Boot");
            return false;
        }

        if (FirebaseManager.Instance.CurrentPlayer == null)
        {
            SceneManager.LoadScene("01_Title");
            return false;
        }

        return true;
    }

    protected void InitializeCommonUI()
    {
        if (statusUI != null)
        {
            statusUI.Refresh();
        }
    }

    protected virtual void OnSceneReady()
    {
        // 各シーン固有処理
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}