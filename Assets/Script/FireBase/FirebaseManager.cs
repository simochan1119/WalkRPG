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

            // 古いデータ対策
            NormalizePlayerData();

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

        PlayerData data = new PlayerData
        {
            uid = User.UserId,
            name = playerName,

            level = CurrentPlayer != null ? CurrentPlayer.level : 1,
            hp = CurrentPlayer != null ? CurrentPlayer.hp : 30,
            maxHp = CurrentPlayer != null ? CurrentPlayer.maxHp : 30,
            gold = CurrentPlayer != null ? CurrentPlayer.gold : 0,

            steps = CurrentPlayer != null ? CurrentPlayer.steps : 0,

            totalSteps = CurrentPlayer != null ? CurrentPlayer.totalSteps : 0,
            usableSteps = CurrentPlayer != null ? CurrentPlayer.usableSteps : 0,
            todaySteps = CurrentPlayer != null ? CurrentPlayer.todaySteps : 0,

            lastSensorSteps = CurrentPlayer != null ? CurrentPlayer.lastSensorSteps : 0,
            todayBaseSensorSteps = CurrentPlayer != null ? CurrentPlayer.todayBaseSensorSteps : 0,
            lastStepDate = CurrentPlayer != null ? CurrentPlayer.lastStepDate : ""
        };

        CurrentPlayer = data;

        NormalizePlayerData();

        await SavePlayer();

        Debug.Log("ユーザーデータ保存完了: " + CurrentPlayer.name);
    }

    private void NormalizePlayerData()
    {
        if (CurrentPlayer == null)
            return;

        if (string.IsNullOrWhiteSpace(CurrentPlayer.uid))
            CurrentPlayer.uid = User.UserId;

        if (string.IsNullOrWhiteSpace(CurrentPlayer.name))
            CurrentPlayer.name = "ななし勇者";

        if (CurrentPlayer.level <= 0)
            CurrentPlayer.level = 1;

        if (CurrentPlayer.maxHp <= 0)
            CurrentPlayer.maxHp = 30;

        if (CurrentPlayer.hp <= 0)
            CurrentPlayer.hp = CurrentPlayer.maxHp;

        if (CurrentPlayer.hp > CurrentPlayer.maxHp)
            CurrentPlayer.hp = CurrentPlayer.maxHp;

        // 旧stepsしかないデータへの対応
        if (CurrentPlayer.totalSteps <= 0 && CurrentPlayer.steps > 0)
            CurrentPlayer.totalSteps = CurrentPlayer.steps;

        // UI互換用
        CurrentPlayer.steps = CurrentPlayer.totalSteps;

        if (CurrentPlayer.usableSteps < 0)
            CurrentPlayer.usableSteps = 0;

        if (CurrentPlayer.todaySteps < 0)
            CurrentPlayer.todaySteps = 0;

        if (CurrentPlayer.lastStepDate == null)
            CurrentPlayer.lastStepDate = "";
    }

    public async Task SavePlayer()
    {
        if (CurrentPlayer == null)
        {
            Debug.LogWarning("保存失敗: CurrentPlayer が null");
            return;
        }

        if (DB == null || User == null)
        {
            Debug.LogWarning("保存失敗: Firebase未初期化");
            return;
        }

        NormalizePlayerData();

        DocumentReference docRef =
            DB.Collection("users")
            .Document(User.UserId);

        await docRef.SetAsync(CurrentPlayer, SetOptions.MergeAll);

        Debug.Log("プレイヤーデータ保存");
    }

    public async Task AddGold(int amount)
    {
        if (CurrentPlayer == null)
            return;

        CurrentPlayer.gold += amount;

        if (CurrentPlayer.gold < 0)
            CurrentPlayer.gold = 0;

        await SavePlayer();
    }

    public async Task AddSteps(int amount)
    {
        if (CurrentPlayer == null)
            return;

        if (amount <= 0)
            return;

        CurrentPlayer.totalSteps += amount;
        CurrentPlayer.usableSteps += amount;

        CurrentPlayer.steps = CurrentPlayer.totalSteps;

        await SavePlayer();
    }

    public async Task UseSteps(int amount)
    {
        if (CurrentPlayer == null)
            return;

        if (amount <= 0)
            return;

        CurrentPlayer.usableSteps -= amount;

        if (CurrentPlayer.usableSteps < 0)
            CurrentPlayer.usableSteps = 0;

        await SavePlayer();
    }

    public async Task HealPlayerFull()
    {
        if (CurrentPlayer == null)
            return;

        if (CurrentPlayer.maxHp <= 0)
            CurrentPlayer.maxHp = 30;

        CurrentPlayer.hp = CurrentPlayer.maxHp;

        await SavePlayer();

        Debug.Log("HP全回復");
    }

    public async Task DamagePlayer(int damage)
    {
        if (CurrentPlayer == null)
            return;

        if (damage <= 0)
            return;

        CurrentPlayer.hp -= damage;

        if (CurrentPlayer.hp < 0)
            CurrentPlayer.hp = 0;

        await SavePlayer();
    }

    public async Task SetHp(int hp)
    {
        if (CurrentPlayer == null)
            return;

        CurrentPlayer.hp = Mathf.Clamp(hp, 0, CurrentPlayer.maxHp);

        await SavePlayer();
    }

    public async Task AddLevel(int amount)
    {
        if (CurrentPlayer == null)
            return;

        if (amount <= 0)
            return;

        CurrentPlayer.level += amount;

        await SavePlayer();
    }
}