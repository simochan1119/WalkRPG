using UnityEngine;

public class StepWalk3D : MonoBehaviour
{
    [Header("Step Move")]
    public float stepDistance = 1f;
    public float moveSpeed = 2f;

    [Header("Animation")]
    public Animator animator;

    private int lastSteps = 0;
    private int pendingSteps = 0;

    private Vector3 targetPosition;

    void Start()
    {
        targetPosition = transform.position;

        if (animator == null)
            animator = GetComponent<Animator>();

        if (StepManager.Instance != null)
        {
            lastSteps = StepManager.Instance.CurrentSteps;
        }
    }

    void Update()
    {
        if (StepManager.Instance == null)
            return;

        int currentSteps = StepManager.Instance.CurrentSteps;

        if (currentSteps > lastSteps)
        {
            int diff = currentSteps - lastSteps;

            pendingSteps += diff;
            lastSteps = currentSteps;

            Debug.Log("–¢Á‰»•à”: " + pendingSteps);
        }

        bool moving =
            Vector3.Distance(
                transform.position,
                targetPosition
            ) > 0.01f;

        if (!moving && pendingSteps > 0)
        {
            pendingSteps--;

            targetPosition +=
                Vector3.right * stepDistance;
        }

        transform.position =
            Vector3.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );

        if (animator != null)
        {
            animator.SetBool(
                "Walk",
                moving || pendingSteps > 0
            );
        }
    }
}