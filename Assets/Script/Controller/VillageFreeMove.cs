using UnityEngine;

public class VillageFreeMove : MonoBehaviour
{
    public float moveSpeed = 3f;
    public Animator animator;
    
    private SpriteRenderer spriteRenderer;
    public VirtualJoystick joystick;

    [Header("West (Left-Facing) Adjustments")]
    public float westWidthMultiplier = 1.062f;
    public float westAnimSpeedMultiplier = 0.85f; // Adjust this in Inspector to tune the walk speed/pitch

    private Vector3 originalScale;

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
    }

    void Update()
    {
        if (joystick == null)
        {
            Debug.LogWarning("Joystickが設定されていません");
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

        if (spriteRenderer != null && Mathf.Abs(move.x) > 0.05f)
        {
            bool isMovingLeft = move.x < 0f;
            spriteRenderer.flipX = isMovingLeft;
            if (animator != null)
            {
                animator.SetBool("FacingWest", isMovingLeft);
            }

            // Adjust scale width for West
            transform.localScale = isMovingLeft 
                ? new Vector3(originalScale.x * westWidthMultiplier, originalScale.y, originalScale.z)
                : originalScale;
        }

        // Apply animation speed (pitch) correction dynamically based on current facing direction
        if (animator != null && spriteRenderer != null)
        {
            animator.speed = spriteRenderer.flipX ? westAnimSpeedMultiplier : 1.0f;
        }
    }
}