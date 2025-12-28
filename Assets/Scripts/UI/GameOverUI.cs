using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject rootPanel;
    public TMP_Text totalTimeText;
    public Button mainMenuButton;

    private bool isShown = false;

    void Awake()
    {
        if (rootPanel != null)
        {
            rootPanel.SetActive(false);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }
    }

    public void Show()
    {
        if (isShown) return;
        isShown = true;

        if (rootPanel != null)
        {
            rootPanel.SetActive(true);
        }

        Time.timeScale = 0f;

        float finalTime = 0f;
        string formattedTime = "--:--.--";

        if (GameTimer.Instance != null)
        {
            GameTimer.Instance.StopTimer();
            finalTime = GameTimer.Instance.ElapsedTime;
            formattedTime = GameTimer.Instance.GetFormattedTime();
        }

        if (totalTimeText != null)
        {
            totalTimeText.text = formattedTime;
        }

        string playerName = PlayerPrefs.GetString("player_nickname", "PLAYER");
        LeaderboardManager.AddEntry(playerName, finalTime);
    }

    private void OnMainMenuClicked()
    {
        Time.timeScale = 1f;

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.ReturnToMainMenu();
        }
        else
        {
            SceneManager.LoadScene("StartMenu");
        }
    }
}

