using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameSceneManager : MonoBehaviour
{
    public StatusUI statusUI;

    protected virtual void Start()
    {
        if (!CheckPlayerData())
            return;

        ConfigureCanvasScaler();
        InitializeCommonUI();
        OnSceneReady();
    }

    private void ConfigureCanvasScaler()
    {
        CanvasScaler[] scalers = FindObjectsOfType<CanvasScaler>();
        foreach (var scaler in scalers)
        {
            if (scaler == null) continue;

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            Debug.Log("[GameSceneManager] Automatically configured UI CanvasScaler (" + scaler.gameObject.name + ") for responsive screen layout (1920x1080, Match 0.5)");
        }
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
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}