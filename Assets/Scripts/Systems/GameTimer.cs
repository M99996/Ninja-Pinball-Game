using UnityEngine;

public class GameTimer : MonoBehaviour
{
    public static GameTimer Instance { get; private set; }

    private float elapsedTime = 0f;
    private bool isRunning = false;
    private bool isPaused = false;

    public float ElapsedTime { get { return elapsedTime; } }

    void Awake()
    {
        // Singleton pattern with DontDestroyOnLoad
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        // Only update if running and not paused
        // Use unscaledDeltaTime so timer continues even when game is paused (Time.timeScale = 0)
        // Timer will only pause when PauseTimer() is explicitly called (e.g., during buff selection)
        if (isRunning && !isPaused)
        {
            elapsedTime += Time.unscaledDeltaTime;
        }
    }

    public void StartTimer()
    {
        isRunning = true;
        isPaused = false;
    }

    public void StopTimer()
    {
        isRunning = false;
        isPaused = false;
    }

    public void PauseTimer()
    {
        isPaused = true;
    }

    public void ResumeTimer()
    {
        isPaused = false;
    }

    public void ResetTimer()
    {
        elapsedTime = 0f;
        isRunning = false;
        isPaused = false;
    }

    public string GetFormattedTime()
    {
        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);
        int milliseconds = Mathf.FloorToInt((elapsedTime * 100f) % 100f);
        
        return string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, milliseconds);
    }
}

