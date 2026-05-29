public static class DungeonSession
{
    public static bool HasSession { get; private set; }

    public static string areaName;
    public static int requiredSteps;
    public static int totalDistance;
    public static int eventInterval;
    public static int nextTownIndex;

    public static string returnSceneName;
    public static string clearSceneName;

    public static void StartSession(
        string areaNameValue,
        int requiredStepsValue,
        int totalDistanceValue,
        int eventIntervalValue,
        int nextTownIndexValue,
        string returnSceneNameValue,
        string clearSceneNameValue
    )
    {
        HasSession = true;

        areaName = areaNameValue;
        requiredSteps = requiredStepsValue;
        totalDistance = totalDistanceValue;
        eventInterval = eventIntervalValue;
        nextTownIndex = nextTownIndexValue;

        returnSceneName = returnSceneNameValue;
        clearSceneName = clearSceneNameValue;
    }

    public static void Clear()
    {
        HasSession = false;

        areaName = "";
        requiredSteps = 0;
        totalDistance = 0;
        eventInterval = 100;
        nextTownIndex = 0;

        returnSceneName = "";
        clearSceneName = "";
    }
}