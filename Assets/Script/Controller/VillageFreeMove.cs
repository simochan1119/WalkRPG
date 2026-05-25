using UnityEngine;

public class VillageFreeMove : MonoBehaviour
{
    public float moveSpeed = 3f;
    public Animator animator;
    public VirtualJoystick joystick;

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (joystick == null)
        {
            Debug.LogWarning("Joystick‚ªÝ’è‚³‚ê‚Ä‚¢‚Ü‚¹‚ñ");
            return;
        }

        Vector2 input = joystick.InputDirection;

        Vector3 move = new Vector3(input.x, 0f, input.y);

        if (move.magnitude > 1f)
            move.Normalize();

        if (move.magnitude > 0.05f)
        {
            transform.position += move * moveSpeed * Time.deltaTime;
           // transform.rotation = Quaternion.LookRotation(move);
        }

        if (animator != null)
        {
            animator.SetBool("Walk", move.magnitude > 0.05f);
        }
    }
}