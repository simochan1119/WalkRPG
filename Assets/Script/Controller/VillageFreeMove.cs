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

    [Header("Sprite Sheets")]
    public Sprite[] eastSprites;
    public Sprite[] westSprites;
    public Sprite[] frontSprites;
    public Sprite[] backSprites;

    private FacingDirection currentDirection = FacingDirection.East;

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

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
            
            // Instantly change direction
            if (currentDirection != wantsDirection)
            {
                currentDirection = wantsDirection;
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
                targetSheet = eastSprites; // Reuse East sprites for absolute size symmetry
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

        // Apply scale compensation based on direction to keep visual thickness consistent
        // Make the character slightly taller (Y = 1.1f) as requested
        Vector3 targetScale = new Vector3(1f, 1.1f, 1f);
        if (currentDirection == FacingDirection.North)
        {
            targetScale.x = 0.9f; // Slightly thin the width when facing North (backwards)
        }
        transform.localScale = targetScale;
    }
}