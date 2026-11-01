using UnityEngine;
using UnityEngine.Serialization;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class AlienCounter : MonoBehaviour
{
    [Header("Dynamic")]
    [FormerlySerializedAs("score")]
    [SerializeField] private int remainingAliens = 0;
    public int RemainingAliens => remainingAliens;
    private TextMeshProUGUI uiText;
    void Awake()
    {
        uiText = GetComponent<TextMeshProUGUI>();
        RefreshText();
    }

    public void SetCount(int count)
    {
        if (remainingAliens == count) return;
        remainingAliens = count;
        RefreshText();
    }

    private void RefreshText()
    {
        uiText.text = "Remaining Aliens: " + remainingAliens.ToString("#,0");
    }
}
