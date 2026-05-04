using UnityEngine;

public class StepManager : MonoBehaviour
{
    public static StepManager Instance;
    public int CurrentSteps { get; private set; }
    public bool IsWalking { get; private set; }

    private float lastMagnitude = 0f;
    private static readonly float THRESHOLD = 1.5f;
    private float lastStepTime = 0f;
    public float stopDelay = 0.5f; // ~‚Ü‚Á‚½‚Æ”»’f‚·‚é‚Ü‚Å‚Ì•b”

    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        Vector3 accel = Input.acceleration;
        float magnitude = accel.magnitude;
        float delta = magnitude - lastMagnitude;

        if (delta > THRESHOLD)
        {
            CurrentSteps++;
            lastStepTime = Time.time;
        }

        // ÅŒã‚ÌŒŸo‚©‚çstopDelay•bˆÈ“à‚È‚ç•à‚¢‚Ä‚é”»’è
        IsWalking = (Time.time - lastStepTime) < stopDelay;

        lastMagnitude = magnitude;
    }
}