using UnityEngine;


public class FieldManager : GameSceneManager
{
    protected override void OnSceneReady()
    {
        FirebaseManager.Instance.HealPlayerFull();

        if (statusUI != null)
            statusUI.Refresh();

        Debug.Log("FieldŠJŽn");
    }
}