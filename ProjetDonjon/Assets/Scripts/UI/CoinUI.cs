using TMPro;
using UnityEngine;

public class CoinUI : MonoBehaviour
{
    [Header("Private Infos")]
    private int currentCoins;

    [Header("Public Infos")]
    public RectTransform CoinAimedTr { get { return _coinAimedTr; } }

    [Header("References")]
    [SerializeField] private TextMeshProUGUI _coinText;
    [SerializeField] private RectTransform _coinAimedTr;

    public void AddCoin(int value)
    {
        currentCoins += value;
        _coinText.text = currentCoins.ToString();
    }
}
