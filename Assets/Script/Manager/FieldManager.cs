using UnityEngine;

public class FieldManager : GameSceneManager
{
    protected override async void OnSceneReady()
    {
        await FirebaseManager.Instance.HealPlayerFull();

        if (statusUI != null)
            statusUI.Refresh();

        Debug.Log("FieldŠJŽn");
    }
}