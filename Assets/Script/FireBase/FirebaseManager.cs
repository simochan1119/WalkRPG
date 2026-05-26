using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using System.Threading.Tasks;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance { get; private set; }

    public FirebaseAuth Auth { get; private set; }
    public FirebaseFirestore DB { get; private set; }
    public FirebaseUser User { get; private set; }
    public PlayerData CurrentPlayer { get; private set; }

    private const string TitleSceneName = "01_Title";

    async void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        await InitializeFirebase();
    }

    private async Task InitializeFirebase()
    {
        var status = await FirebaseApp.CheckAndFixDependenciesAsync();

        if (status != DependencyStatus.Available)
        {
            Debug.LogError("Firebase初期化失敗: " + status);
            return;
        }

        Auth = FirebaseAuth.DefaultInstance;
        DB = FirebaseFirestore.DefaultInstance;

        if (Auth.CurrentUser == null)
        {
            var result = await Auth.SignInAnonymouslyAsync();
            User = result.User;
        }
        else
        {
            User = Auth.CurrentUser;
        }

        Debug.Log("ログイン成功 UID: " + User.UserId);

        await LoadPlayerData();

        SceneManager.LoadScene(TitleSceneName);
    }

    public async Task LoadPlayerData()
    {
        DocumentReference docRef = DB.Collection("users").Document(User.UserId);
        DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

        if (snapshot.Exists)
        {
            CurrentPlayer = snapshot.ConvertTo<PlayerData>();
            Debug.Log("既存ユーザーデータ読込: " + CurrentPlayer.name);
        }
        else
        {
            CurrentPlayer = null;
            Debug.Log("ユーザーデータ未作成");
        }
    }

    public async Task CreateOrUpdatePlayer(string playerName)
    {
        if (string.IsNullOrWhiteSpace(playerName))
        {
            playerName = "ななし勇者";
        }

        DocumentReference docRef = DB.Collection("users").Document(User.UserId);

        PlayerData data = new PlayerData
        {
            uid = User.UserId,
            name = playerName,
            level = CurrentPlayer != null ? CurrentPlayer.level : 1,
            hp = CurrentPlayer != null ? CurrentPlayer.hp : 30,
            gold = CurrentPlayer != null ? CurrentPlayer.gold : 0,
            steps = CurrentPlayer != null ? CurrentPlayer.steps : 0,
            maxHp = CurrentPlayer != null ? CurrentPlayer.maxHp : 30
        };

        await docRef.SetAsync(data, SetOptions.MergeAll);
        CurrentPlayer = data;

        Debug.Log("ユーザーデータ保存完了: " + data.name);
    }
    public void HealPlayerFull()
    {
        if (CurrentPlayer == null) return;

        if (CurrentPlayer.maxHp <= 0)
            CurrentPlayer.maxHp = 100;

        CurrentPlayer.hp = CurrentPlayer.maxHp;
    }
}