using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class StartMenu : MonoBehaviour
{
    [Header("Scene")]
    public string sceneName = "Level_1";

    [Header("UI")]
    public TMP_InputField nameInput; 
    public GameObject leaderboardPanel;
    public Button startButton;
    public Button leaderboardButton;
    public Button exitButton;

    [Header("Visuals")]
    public int flashCount = 3;
    public float flashSpeed = 0.15f;
    private Color normalColor = Color.white;
    private Color errorColor = new(1f, 0.5f, 0.5f);

    private void Awake()
    {
        if (nameInput) nameInput.text = "";
        Time.timeScale = 1f;

        if (startButton) startButton.onClick.AddListener(OnStartClicked);
        if (leaderboardButton) leaderboardButton.onClick.AddListener(OnLeaderboardClicked);
        if (exitButton) exitButton.onClick.AddListener(OnExitClicked);

        // Hide leaderboard panel at start
        if (leaderboardPanel) leaderboardPanel.SetActive(false);

        if (nameInput) normalColor = nameInput.image.color;
    }

    public void OnStartClicked()
    {
        string nickname = nameInput.text.Trim();

        if (string.IsNullOrEmpty(nickname) || nickname.Length < 3 || nickname.Length > 12)
        {
            StartCoroutine(FlashInputBox(nameInput.image));

            Debug.Log("[NameInput] field is empty OR [NameInput] content length is not between 3-12.");
            return;
        }

        nameInput.image.color = normalColor;

        PlayerPrefs.SetString("player_nickname", nickname);
        PlayerPrefs.Save();

        // Reset and start timer before loading game scene
        if (GameTimer.Instance != null)
        {
            GameTimer.Instance.ResetTimer();
            GameTimer.Instance.StartTimer();
        }
        else
        {
            Debug.LogWarning("StartMenu: GameTimer.Instance is null. Make sure GameTimer GameObject exists in the scene.");
        }

        // Load game scene
        SceneManager.LoadScene(sceneName);
    }

    public void OnLeaderboardClicked()
    {
        if (!leaderboardPanel) return;
        leaderboardPanel.SetActive(!leaderboardPanel.activeSelf);
    }

    public void OnExitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
    
    private IEnumerator FlashInputBox(Graphic target)
    {
        if (target == null) yield break;

        for (int i = 0; i < flashCount; i++)
        {
            target.color = errorColor;
            yield return new WaitForSeconds(flashSpeed);
            target.color = normalColor;
            yield return new WaitForSeconds(flashSpeed);
        }
    }
}
