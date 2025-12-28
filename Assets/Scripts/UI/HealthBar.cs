using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBar : MonoBehaviour
{
    [Header("Refs")]
    public PlayerCharacter player;
    public Image fillImage;
    public TextMeshProUGUI ballCountText;

    [Header("Visual")]
    public float lerpSpeed = 8f;

    void Start()
    {
        if (!player) player = FindObjectOfType<PlayerCharacter>();
    }

    void Update()
    {
        if (!player || !fillImage) return;

        float target = Mathf.Clamp01((float)player.currentHP / Mathf.Max(1, player.maxHP));

        fillImage.fillAmount = Mathf.MoveTowards(fillImage.fillAmount, target, lerpSpeed * Time.deltaTime);
        
        if (ballCountText != null)
        {
            int displayBalls = player.GetDisplayedBallCount();
            ballCountText.text = $"x {displayBalls}";
        }
    }
}
