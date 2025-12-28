using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BuffManager : MonoBehaviour
{
    public static BuffManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject buffContainer;
    public UnityEngine.UI.Image option1;
    public UnityEngine.UI.Image option2;
    public UnityEngine.UI.Image option3;

    private List<BuffType> activeBuffs = new List<BuffType>();
    private bool isSelectingBuff = false;

    // Buff values
    public int ballDamageBonus { get; private set; } = 0;
    public int healthBonus { get; private set; } = 0;
    public int maxHealthBonus { get; private set; } = 0;
    public int ballCountBonus { get; private set; } = 0;
    public int ballCollisionBonus { get; private set; } = 0;
    public float ballSpeedMultiplier { get; private set; } = 1f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Hide buff selection UI at start
        // But don't hide if TurnManager already called ShowBuffSelection
        // (This handles execution order issues)
        if (buffContainer != null && !isSelectingBuff)
        {
            buffContainer.SetActive(false);
        }
    }

    public void ShowBuffSelection()
    {
        if (isSelectingBuff)
        {
            return;
        }

        isSelectingBuff = true;
        
        // Pause timer when selecting buff
        if (GameTimer.Instance != null)
        {
            GameTimer.Instance.PauseTimer();
        }
        
        // Get 3 random unique buffs
        List<BuffType> availableBuffs = GetRandomBuffs(3);
        
        // Assign buffs to UI options
        SetBuffOption(option1, availableBuffs[0], 1);
        SetBuffOption(option2, availableBuffs[1], 2);
        SetBuffOption(option3, availableBuffs[2], 3);

        // Show buff container
        if (buffContainer != null)
        {
            buffContainer.SetActive(true);
        }
    }

    private List<BuffType> GetRandomBuffs(int count)
    {
        List<BuffType> allBuffs = new List<BuffType>
        {
            BuffType.BallDamagePlus,
            BuffType.HealthPlus,
            BuffType.BallCountPlus,
            BuffType.BallCollisionPlus,
            BuffType.BallSpeedPlus
        };

        // Shuffle list
        for (int i = 0; i < allBuffs.Count; i++)
        {
            BuffType temp = allBuffs[i];
            int randomIndex = Random.Range(i, allBuffs.Count);
            allBuffs[i] = allBuffs[randomIndex];
            allBuffs[randomIndex] = temp;
        }

        // Return first 'count' buffs
        return allBuffs.GetRange(0, count);
    }

    private void SetBuffOption(UnityEngine.UI.Image option, BuffType buffType, int optionNumber)
    {
        if (option != null)
        {
            option.gameObject.SetActive(true);
            option.GetComponent<BuffOptionUI>()?.SetBuff(buffType);
            
            // Set text if exists (show buff description)
            // Try TextMeshPro first, then fall back to regular Text
            TextMeshProUGUI tmpText = option.GetComponentInChildren<TextMeshProUGUI>();
            if (tmpText != null)
            {
                tmpText.text = GetBuffDescription(buffType);
            }
            else
            {
                // Fall back to regular Text component
                UnityEngine.UI.Text text = option.GetComponentInChildren<UnityEngine.UI.Text>();
                if (text != null)
                {
                    text.text = GetBuffDescription(buffType);
                }
            }
        }
    }

    private string GetBuffDescription(BuffType buffType)
    {
        switch (buffType)
        {
            case BuffType.BallDamagePlus:
                return "Ball Damage +2";
            case BuffType.HealthPlus:
                return "Health & Max Health +3";
            case BuffType.BallCountPlus:
                return "Ball Count +1";
            case BuffType.BallCollisionPlus:
                return "Ball Max Collisions +2";
            case BuffType.BallSpeedPlus:
                return "Ball Speed +10%";
            default:
                return "Unknown";
        }
    }

    public void SelectBuff(BuffType buffType)
    {
        if (!isSelectingBuff) return;

        // Apply buff effect
        ApplyBuff(buffType);
        
        // Get player reference once
        PlayerCharacter player = FindObjectOfType<PlayerCharacter>();
        
        // Update player's ball count if BallCountPlus was selected
        if (buffType == BuffType.BallCountPlus && player != null)
        {
            player.UpdateBallCount();
        }
        
        // Hide buff selection UI
        HideBuffSelection();
        
        isSelectingBuff = false;
        
        // Resume timer after selecting buff
        if (GameTimer.Instance != null)
        {
            GameTimer.Instance.ResumeTimer();
        }
        
        // Check if player has no balls left after buff selection
        if (player != null)
        {
            int remainingBalls = player.GetRemainingBalls();
            if (remainingBalls <= 0)
            {
                // Player dies if no balls available
                player.Die();
            }
        }
    }

    private void ApplyBuff(BuffType buffType)
    {
        activeBuffs.Add(buffType);

        switch (buffType)
        {
            case BuffType.BallDamagePlus:
                ballDamageBonus += 2;
                break;
                
            case BuffType.HealthPlus:
            {
                healthBonus += 3;
                maxHealthBonus += 3;
                PlayerCharacter player = FindObjectOfType<PlayerCharacter>();
                if (player != null)
                {
                    player.AddMaxHealth(3);
                    player.AddHealth(3);
                }
                break;
            }
            case BuffType.BallCountPlus:
                ballCountBonus++;
                break;
            case BuffType.BallCollisionPlus:
                ballCollisionBonus += 2;
                break;
            case BuffType.BallSpeedPlus:
            {
                ballSpeedMultiplier *= 1.1f;
                PlayerCharacter player = FindObjectOfType<PlayerCharacter>();
                if (player != null)
                {
                    player.ApplyBallSpeedMultiplier(1.1f);
                }
                break;
            }
        }
    }

    private void HideBuffSelection()
    {
        if (option1 != null) option1.gameObject.SetActive(false);
        if (option2 != null) option2.gameObject.SetActive(false);
        if (option3 != null) option3.gameObject.SetActive(false);
        
        if (buffContainer != null)
        {
            buffContainer.SetActive(false);
        }
    }

    public bool IsSelectingBuff()
    {
        return isSelectingBuff;
    }

    public void ResetBuffs()
    {
        activeBuffs.Clear();
        ballDamageBonus = 0;
        healthBonus = 0;
        maxHealthBonus = 0;
        ballCountBonus = 0;
        ballCollisionBonus = 0;
        ballSpeedMultiplier = 1f;
    }
}

