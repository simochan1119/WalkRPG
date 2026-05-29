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

    [FirestoreProperty] public int steps { get; set; }

    [FirestoreProperty] public int totalSteps { get; set; }
    [FirestoreProperty] public int usableSteps { get; set; }
    [FirestoreProperty] public int todaySteps { get; set; }

    [FirestoreProperty] public int lastSensorSteps { get; set; }
    [FirestoreProperty] public int todayBaseSensorSteps { get; set; }
    [FirestoreProperty] public string lastStepDate { get; set; }

    [FirestoreProperty] public int currentTownIndex { get; set; }
    [FirestoreProperty] public int maxUnlockedTownIndex { get; set; }
}