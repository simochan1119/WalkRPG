using UnityEngine;

public class StepWalk3D : MonoBehaviour
{
    public float stepDistance = 1f;
    public Animator animator;

    private int lastSteps = 0;

    void Update()
    {
        if (StepManager.Instance == null)
            return;

        int currentSteps =
            StepManager.Instance.CurrentSteps;

        // •à”‚ª‘‚¦‚½uŠÔ‚¾‚¯
        if (currentSteps > lastSteps)
        {
            int diff = currentSteps - lastSteps;

            // •à”•ªi‚Ş
            transform.position +=
                Vector3.right * stepDistance * diff;

            animator.SetTrigger("Step");

            Debug.Log("•à‚¢‚½I");
        }

        lastSteps = currentSteps;
    }
}