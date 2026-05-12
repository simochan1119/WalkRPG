using UnityEngine;

public class StepWalk3D : MonoBehaviour
{
    public float stepDistance = 0.5f;
    public float moveSpeed = 2f;

    public Animator animator;

    private int lastSteps = 0;

    private int pendingSteps = 0;

    private Vector3 targetPosition;

    void Start()
    {
        targetPosition = transform.position;

        if (StepManager.Instance != null)
        {
            lastSteps = StepManager.Instance.CurrentSteps;
        }
    }

    void Update()
    {
        if (StepManager.Instance == null)
            return;

        int currentSteps =
            StepManager.Instance.CurrentSteps;

        // 新しい歩数検知
        if (currentSteps > lastSteps)
        {
            int diff = currentSteps - lastSteps;

            pendingSteps += diff;

            lastSteps = currentSteps;

            Debug.Log("pending: " + pendingSteps);
        }

        bool moving =
            Vector3.Distance(
                transform.position,
                targetPosition
            ) > 0.01f;

        // 移動完了していて、
        // まだ未消化歩数がある
        if (!moving && pendingSteps > 0)
        {
            pendingSteps--;

            targetPosition +=
                Vector3.right * stepDistance;
        }

        // スムーズ移動
        transform.position =
            Vector3.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );

        animator.SetBool(
            "Walk",
            moving || pendingSteps > 0
        );
    }
}