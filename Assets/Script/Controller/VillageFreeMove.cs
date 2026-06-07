using UnityEngine;

public class VillageFreeMove : MonoBehaviour
{
    public float moveSpeed = 3f;
    public Animator animator;
    
    private SpriteRenderer spriteRenderer;
    public VirtualJoystick joystick;

    [Header("Turn Animation Settings")]
    public float turnDuration = 0.12f; // Time in seconds to complete the turn squash/stretch

    private Vector3 originalScale;
    private Coroutine turnCoroutine;
    private bool isTurning = false;

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
            bool wantsFaceLeft = move.x < 0f;
            
            // Only trigger turn if the desired facing direction changes and we aren't already turning
            if (spriteRenderer.flipX != wantsFaceLeft && !isTurning)
            {
                if (turnCoroutine != null)
                    StopCoroutine(turnCoroutine);
                turnCoroutine = StartCoroutine(TurnRoutine(wantsFaceLeft));
            }
        }
    }

    System.Collections.IEnumerator TurnRoutine(bool faceLeft)
    {
        isTurning = true;
        float elapsed = 0f;
        float halfDuration = turnDuration / 2f;

        // 1. Squash to 0
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            transform.localScale = new Vector3(Mathf.Lerp(originalScale.x, 0f, t), originalScale.y, originalScale.z);
            yield return null;
        }

        // 2. Midpoint: Flip the sprite
        spriteRenderer.flipX = faceLeft;

        // 3. Stretch back to original
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            transform.localScale = new Vector3(Mathf.Lerp(0f, originalScale.x, t), originalScale.y, originalScale.z);
            yield return null;
        }

        transform.localScale = originalScale;
        isTurning = false;
        turnCoroutine = null;
    }
}