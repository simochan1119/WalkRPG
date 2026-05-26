using Firebase.Firestore;

[FirestoreData]
public class PlayerData
{
    [FirestoreProperty] public string uid { get; set; }
    [FirestoreProperty] public string name { get; set; }

    [FirestoreProperty] public int level { get; set; }
    [FirestoreProperty] public int hp { get; set; }
    [FirestoreProperty] public int maxHp { get; set; }
    [FirestoreProperty] public int gold { get; set; }

    // 旧steps互換用：総歩数として扱う
    [FirestoreProperty] public int steps { get; set; }

    // 新しい歩数管理
    [FirestoreProperty] public int totalSteps { get; set; }
    [FirestoreProperty] public int usableSteps { get; set; }
    [FirestoreProperty] public int todaySteps { get; set; }

    // Android歩数センサー管理用
    [FirestoreProperty] public int lastSensorSteps { get; set; }
    [FirestoreProperty] public int todayBaseSensorSteps { get; set; }
    [FirestoreProperty] public string lastStepDate { get; set; }
}