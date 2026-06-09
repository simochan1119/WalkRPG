using UnityEngine;
using System.Collections;
using System.Threading.Tasks;

public class EmoteController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer emoteRenderer;

    [Header("Settings")]
    [SerializeField] private Vector3 localOffset = new Vector3(0.5f, 2.2f, 0f);
    [SerializeField] private Vector3 baseScale = new Vector3(6f, 6f, 1f);

    [Header("Emote Presets")]
    public Sprite[] exclamationSprites;

    private Coroutine activeCoroutine;

    private void Awake()
    {
        InitializeRenderer();
    }

    private void InitializeRenderer()
    {
        if (emoteRenderer == null)
        {
            Transform bubbleTrans = transform.Find("EmoteBubble");
            if (bubbleTrans != null)
            {
                emoteRenderer = bubbleTrans.GetComponent<SpriteRenderer>();
            }
            else
            {
                GameObject bubbleObj = new GameObject("EmoteBubble");
                bubbleObj.transform.SetParent(transform, false);
                emoteRenderer = bubbleObj.AddComponent<SpriteRenderer>();
            }
        }

        if (emoteRenderer != null)
        {
            emoteRenderer.gameObject.SetActive(false);
            emoteRenderer.transform.localPosition = localOffset;
        }
    }

    public void ShowEmote(Sprite[] frames, float duration = 1.0f, float frameRate = 12f)
    {
        if (emoteRenderer == null)
        {
            InitializeRenderer();
        }

        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
        }
        activeCoroutine = StartCoroutine(PlayEmoteCoroutine(frames, duration, frameRate));
    }

    public async Task ShowEmoteAsync(Sprite[] frames, float duration = 1.0f, float frameRate = 12f)
    {
        ShowEmote(frames, duration, frameRate);
        
        // Wait for duration to complete (in milliseconds)
        await Task.Delay((int)(duration * 1000));
    }

    public async Task PlayExclamationAsync(float duration = 1.0f)
    {
        if (exclamationSprites != null && exclamationSprites.Length > 0)
        {
            await ShowEmoteAsync(exclamationSprites, duration, 12f);
        }
        else
        {
            Debug.LogWarning("[EmoteController] Exclamation sprites are not assigned!");
            await Task.Delay((int)(duration * 1000));
        }
    }

    private IEnumerator PlayEmoteCoroutine(Sprite[] frames, float duration, float frameRate)
    {
        if (emoteRenderer == null || frames == null || frames.Length == 0)
        {
            Debug.LogWarning($"[EmoteController] Coroutine abort. Renderer: {emoteRenderer != null}, Frames: {(frames != null ? frames.Length : 0)}");
            yield break;
        }

        Debug.Log($"[EmoteController] PlayEmoteCoroutine started. Frames: {frames.Length}, Duration: {duration}");
        emoteRenderer.gameObject.SetActive(true);
        emoteRenderer.transform.localPosition = localOffset;
        emoteRenderer.transform.localScale = Vector3.zero;

        // Ensure it is rendered in front of the character sprite
        SpriteRenderer parentRenderer = GetComponent<SpriteRenderer>();
        if (parentRenderer != null)
        {
            emoteRenderer.sortingLayerID = parentRenderer.sortingLayerID;
            emoteRenderer.sortingOrder = parentRenderer.sortingOrder + 10;
        }

        float elapsed = 0f;
        float popDuration = 0.15f;
        float fadeDuration = 0.15f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            // 1. Animate sprite frames
            int frameIndex = Mathf.Clamp((int)(elapsed * frameRate), 0, frames.Length - 1);
            emoteRenderer.sprite = frames[frameIndex];

            // 2. Animate scale bounce
            float scale = 1.0f;
            if (elapsed < popDuration)
            {
                float t = elapsed / popDuration;
                // Pop-in bounce: 0 -> 1.2 -> 1.0
                if (t < 0.6f)
                {
                    scale = Mathf.Lerp(0f, 1.2f, t / 0.6f);
                }
                else
                {
                    scale = Mathf.Lerp(1.2f, 1.0f, (t - 0.6f) / 0.4f);
                }
            }
            else if (elapsed > duration - fadeDuration)
            {
                float t = (elapsed - (duration - fadeDuration)) / fadeDuration;
                // Shrink: 1.0 -> 0.0
                scale = Mathf.Lerp(1.0f, 0f, t);
            }

            emoteRenderer.transform.localScale = baseScale * scale;
            Debug.Log($"[EmoteController] PlayEmoteCoroutine running. Elapsed: {elapsed:F2}, Frame: {frameIndex} ({frames[frameIndex].name}), Scale: {emoteRenderer.transform.localScale}");

            yield return null;
        }

        emoteRenderer.gameObject.SetActive(false);
        Debug.Log("[EmoteController] PlayEmoteCoroutine finished and deactivated.");
        activeCoroutine = null;
    }
}
