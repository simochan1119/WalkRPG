using UnityEngine;

public class StepWalk3D : MonoBehaviour
{
    public float speed = 2f;
    public Animator animator;

    void Update()
    {
        int currentSteps = StepManager.Instance.CurrentSteps;

        bool walking = currentSteps > 0 &&
                       StepManager.Instance.IsWalking;

        animator.SetBool("Walk", walking);

        if (walking)
        {
            transform.position += Vector3.right * speed * Time.deltaTime;
        }
    }
}