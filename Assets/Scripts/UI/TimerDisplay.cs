using UnityEngine;
using TMPro;

public class TimerDisplay : MonoBehaviour
{
    private TextMeshProUGUI timerText;

    void Start()
    {
        // Auto-find TextMeshProUGUI named "InGameTimer" in the scene
        GameObject timerObject = GameObject.Find("InGameTimer");
        if (timerObject != null)
        {
            timerText = timerObject.GetComponent<TextMeshProUGUI>();
        }
    }

    void Update()
    {
        // Update timer display every frame
        if (timerText == null)
        {
            // Try to find it again if it wasn't found in Start
            GameObject timerObject = GameObject.Find("InGameTimer");
            if (timerObject != null)
            {
                timerText = timerObject.GetComponent<TextMeshProUGUI>();
            }
            return;
        }

        if (GameTimer.Instance == null)
        {
            return;
        }

        string timeString = GameTimer.Instance.GetFormattedTime();
        
        // Only update if text actually changed (avoid unnecessary updates)
        if (timerText.text != timeString)
        {
            timerText.text = timeString;
        }
    }
}

