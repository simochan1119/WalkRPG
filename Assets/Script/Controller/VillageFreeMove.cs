using UnityEngine;

public class VillageFreeMove : MonoBehaviour
{
    public enum FacingDirection
    {
        East,
        West,
        North,
        South
    }

    public float moveSpeed = 3f;
    public Animator animator;
    
    private SpriteRenderer spriteRenderer;
    public VirtualJoystick joystick;

    [Header("Turn Animation Settings")]
    public float turnDuration = 0.12f; // Time in seconds to complete the turn squash/stretch

    [Header("Sprite Sheets")]
    public Sprite[] eastSprites;
    public Sprite[] westSprites;
    public Sprite[] frontSprites;
    public Sprite[] backSprites;

    private Vector3 originalScale;
    private Coroutine turnCoroutine;
    private bool isTurning = false;
    private FacingDirection currentDirection = FacingDirection.East;

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;

        SortSprites();
    }

    private void SortSprites()
    {
        SortSpriteArray(eastSprites);
        SortSpriteArray(westSprites);
        SortSpriteArray(frontSprites);
        SortSpriteArray(backSprites);
    }

    private void SortSpriteArray(Sprite[] array)
    {
        if (array != null && array.Length > 0)
        {
            System.Array.Sort(array, (a, b) => GetSpriteIndex(a.name).CompareTo(GetSpriteIndex(b.name)));
        }
    }

    private int GetSpriteIndex(string spriteName)
    {
        string[] parts = spriteName.Split('_');
        int val;
        if (parts.Length > 0 && int.TryParse(parts[parts.Length - 1], out val))
            return val;
        return 0;
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
        }

        if (animator != null)
        {
            animator.SetBool("Walk", move.magnitude > 0.05f);
        }

        if (spriteRenderer != null && move.magnitude > 0.05f)
        {
            FacingDirection wantsDirection = currentDirection;

            // Determine primary direction based on movement vector
            if (Mathf.Abs(move.x) >= Mathf.Abs(move.z))
            {
                wantsDirection = move.x > 0f ? FacingDirection.East : FacingDirection.West;
            }
            else
            {
                wantsDirection = move.z > 0f ? FacingDirection.North : FacingDirection.South;
            }
            
            // Only trigger turn if the desired facing direction changes and we aren't already turning
            if (currentDirection != wantsDirection && !isTurning)
            {
                if (turnCoroutine != null)
                    StopCoroutine(turnCoroutine);
                turnCoroutine = StartCoroutine(TurnRoutine(wantsDirection));
            }
        }
    }

    void LateUpdate()
    {
        if (spriteRenderer == null) return;

        Sprite[] targetSheet = null;
        bool flip = false;

        switch (currentDirection)
        {
            case FacingDirection.East:
                targetSheet = eastSprites;
                flip = false;
                break;
            case FacingDirection.West:
                targetSheet = westSprites;
                flip = true;
                break;
            case FacingDirection.North:
                targetSheet = backSprites;
                flip = false;
                break;
            case FacingDirection.South:
                targetSheet = frontSprites;
                flip = false;
                break;
        }

        if (targetSheet != null && targetSheet.Length > 0 && eastSprites != null && eastSprites.Length > 0)
        {
            var currentSprite = spriteRenderer.sprite;
            if (currentSprite != null)
            {
                // Find the index of currentSprite in eastSprites (animator standard)
                int idx = -1;
                for (int i = 0; i < eastSprites.Length; i++)
                {
                    if (eastSprites[i] == currentSprite)
                    {
                        idx = i;
                        break;
                    }
                }

                if (idx != -1)
                {
                    // Map index to targetSheet (clamp to prevent out of bounds)
                    int targetIdx = Mathf.Clamp(idx, 0, targetSheet.Length - 1);
                    spriteRenderer.sprite = targetSheet[targetIdx];
                }
            }
        }
        
        spriteRenderer.flipX = flip;
    }

    System.Collections.IEnumerator TurnRoutine(FacingDirection targetDir)
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

        // 2. Midpoint: Update direction
        currentDirection = targetDir;

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