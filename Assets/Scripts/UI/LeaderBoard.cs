using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LeaderboardController : MonoBehaviour
{
    public TextMeshProUGUI leaderboardText;
    public Button exitButton;

    void Awake()
    {
        if (exitButton) exitButton.onClick.AddListener(() => { gameObject.SetActive(false); });
    }

    void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (!leaderboardText) return;

        var list = LeaderboardManager.GetTop(10);

        leaderboardText.alignment = TextAlignmentOptions.TopLeft;
        leaderboardText.enableWordWrapping = false;

        var sb = new StringBuilder();
        sb.AppendLine("<mspace=0.6em>");
        sb.AppendLine($"{"#",2}  {"NAME",-12} {"TIME",9}");

        for (int i = 0; i < 10; i++)
        {
            string name, time;
            if (i < list.Count)
            {
                var e = list[i];
                name = Truncate(e.name, 12);
                time = LeaderboardManager.FormatTime(e.time);
            }
            else
            {
                name = "---";
                time = "--:--.--";
            }

            sb.AppendLine($"{i + 1,2}. {name,-12} {time,9}");
        }

        sb.Append("</mspace>");

        leaderboardText.text = sb.ToString();
    }

    private static string Truncate(string s, int max)
        => string.IsNullOrEmpty(s) ? s : (s.Length <= max ? s : s.Substring(0, max));
}