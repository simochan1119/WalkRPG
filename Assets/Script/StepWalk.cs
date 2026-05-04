using UnityEngine;

public class StepWalk : MonoBehaviour
{
    public float speed = 2f;
    private Animator animator;

    int lastSteps = 0;
    int currentSteps = 0;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // 仮の歩数（まずはテスト）
        currentSteps = Mathf.FloorToInt(Time.time);

        if (currentSteps > lastSteps)
        {
            transform.position += Vector3.right * speed * Time.deltaTime;
            animator.SetBool("Walk", true);
        }
        else
        {
            animator.SetBool("Walk", false);
        }

        lastSteps = currentSteps;
    }
}