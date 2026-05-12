using UnityEngine;

#if UNITY_ANDROID
using UnityEngine.Android;
#endif

public class PermissionManager : MonoBehaviour
{
    public static PermissionManager Instance;

    public bool HasActivityPermission { get; private set; }
    public bool HasNotificationPermission { get; private set; }

    void Awake()
    {
        // Singleton
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        CheckPermissions();
    }

    // 現在の権限状態を確認
    public void CheckPermissions()
    {
#if UNITY_ANDROID && !UNITY_EDITOR

        // 身体活動
        HasActivityPermission =
            Permission.HasUserAuthorizedPermission(
                "android.permission.ACTIVITY_RECOGNITION"
            );

        // 通知(Android13以降)
        HasNotificationPermission =
            Permission.HasUserAuthorizedPermission(
                "android.permission.POST_NOTIFICATIONS"
            );

#else

        // Editorでは常にtrue扱い
        HasActivityPermission = true;
        HasNotificationPermission = true;

#endif

        Debug.Log("身体活動許可: " + HasActivityPermission);
        Debug.Log("通知許可: " + HasNotificationPermission);
    }

    // 身体活動権限要求
    public void RequestActivityPermission()
    {
#if UNITY_ANDROID && !UNITY_EDITOR

        Permission.RequestUserPermission(
            "android.permission.ACTIVITY_RECOGNITION"
        );

#endif
    }

    // 通知権限要求
    public void RequestNotificationPermission()
    {
#if UNITY_ANDROID && !UNITY_EDITOR

        Permission.RequestUserPermission(
            "android.permission.POST_NOTIFICATIONS"
        );

#endif
    }

    // 権限要求後に再確認
    public void RefreshPermissions()
    {
        CheckPermissions();
    }
}