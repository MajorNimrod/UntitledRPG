using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlotView : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI qty;
    public Button button;

    int index;
    System.Action<int> onClick;

    public void Bind(int slotIndex, System.Action<int> onClickHandler)
    {
        index = slotIndex;
        onClick = onClickHandler;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick?.Invoke(index));
    }

    public void ShowEmpty()
    {
        icon.enabled = false;
        qty.text = "";
    }

    public void Show(Sprite sprite, int amount)
    {
        icon.enabled = true;
        icon.sprite = sprite;
        qty.text = amount > 1 ? $"x{amount}" : "";
    }
}
