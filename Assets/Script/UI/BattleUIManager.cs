using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleUIManager : MonoBehaviour
{
    public static BattleUIManager Instance { get; private set; }

    [Header("Slash Skill UI")]
    public Button slashButton;
    public Image slashCooldownImage; // Image Type must be set to 'Filled' with Fill Method 'Radial 360'
    public TMP_Text slashCooldownText;

    [Header("Potion Item UI")]
    public Button potionButton;
    public Image potionCooldownImage; // Image Type must be set to 'Filled' with Fill Method 'Radial 360'
    public TMP_Text potionCooldownText;
    public TMP_Text potionCountText;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // Bind UI button click events to BattleManager actions
        if (slashButton != null)
        {
            slashButton.onClick.AddListener(OnSlashClicked);
        }

        if (potionButton != null)
        {
            potionButton.onClick.AddListener(OnPotionClicked);
        }
    }

    void Update()
    {
        if (BattleManager.Instance == null) return;

        UpdateSlashUI();
        UpdatePotionUI();
    }

    private void UpdateSlashUI()
    {
        float remaining = BattleManager.Instance.SlashCooldownRemaining;
        float ratio = BattleManager.Instance.SlashCooldownRatio;

        if (slashCooldownImage != null)
        {
            slashCooldownImage.fillAmount = ratio;
            slashCooldownImage.gameObject.SetActive(ratio > 0f);
        }

        if (slashCooldownText != null)
        {
            if (remaining > 0f)
            {
                slashCooldownText.text = remaining.ToString("F1") + "s";
                slashCooldownText.gameObject.SetActive(true);
            }
            else
            {
                slashCooldownText.gameObject.SetActive(false);
            }
        }

        if (slashButton != null)
        {
            // Disable click interactions while on cooldown
            slashButton.interactable = (remaining <= 0f);
        }
    }

    private void UpdatePotionUI()
    {
        float remaining = BattleManager.Instance.PotionCooldownRemaining;
        float ratio = BattleManager.Instance.PotionCooldownRatio;
        int count = BattleManager.Instance.CurrentPotionCount;

        if (potionCooldownImage != null)
        {
            potionCooldownImage.fillAmount = ratio;
            potionCooldownImage.gameObject.SetActive(ratio > 0f);
        }

        if (potionCooldownText != null)
        {
            if (remaining > 0f)
            {
                potionCooldownText.text = remaining.ToString("F1") + "s";
                potionCooldownText.gameObject.SetActive(true);
            }
            else
            {
                potionCooldownText.gameObject.SetActive(false);
            }
        }

        if (potionCountText != null)
        {
            potionCountText.text = "x" + count;
        }

        if (potionButton != null)
        {
            // Enable button only if we have potions left and cooldown is finished
            potionButton.interactable = (remaining <= 0f && count > 0);
        }
    }

    private void OnSlashClicked()
    {
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.UseSlash();
        }
    }

    private void OnPotionClicked()
    {
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.UsePotion();
        }
    }
}
