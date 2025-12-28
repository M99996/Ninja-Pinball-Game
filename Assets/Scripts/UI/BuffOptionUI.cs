using UnityEngine;
using UnityEngine.UI;

public class BuffOptionUI : MonoBehaviour
{
    private BuffType buffType;
    private Button button;

    void Start()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnClick);
        }
    }

    public void SetBuff(BuffType type)
    {
        buffType = type;
    }

    private void OnClick()
    {
        if (BuffManager.Instance != null)
        {
            BuffManager.Instance.SelectBuff(buffType);
        }
    }
}

