using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitUI : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private float appearEffectSpeed;

    [Header("References")]
    [SerializeField] private TextMeshProUGUI _healthText;
    [SerializeField] private Image _healthFillableImage;
    [SerializeField] private TextMeshProUGUI _turnCounterText;
    private Animator _animator;


    private void Start()
    {
        HideUnitUI();
        _animator = GetComponent<Animator>();   
    }

    public void HideUnitUI()
    {
        _animator.SetBool("IsShowned", false);
    }

    public void ShowUnitUI()
    {
        _animator.SetBool("IsShowned", true);
    }
}
