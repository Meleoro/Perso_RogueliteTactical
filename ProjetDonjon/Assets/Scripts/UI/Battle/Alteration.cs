using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Alteration : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private Color buffColor;
    [SerializeField] private Color debuffColor;
    [SerializeField] private Sprite attackSprite;
    [SerializeField] private Sprite speedSprite;
    [SerializeField] private Sprite shieldSprite;
    [SerializeField] private Sprite provocationSprite;
    [SerializeField] private Sprite buffSprite;
    [SerializeField] private Sprite debuffSprite;

    [Header("Private Infos")]
    private AlterationStruct currentData;
    private bool isDisplayed;

    [Header("References")]
    [SerializeField] private Image _image;


    private void Setup(AlterationStruct info)
    {
        currentData = info;

        _image.sprite = info.alteration.alterationIcon;

        if (info.alteration.isPositive)
        {
            _image.color = buffColor;
        }
        else
        {
            _image.color = debuffColor;
        }
    }


    #region Appear / Disappear

    public void Appear(AlterationStruct info)
    {
        Setup(info);
        _image.enabled = true;

        if (!isDisplayed)
        {
            isDisplayed = true;

        }
    }

    public void Disappear()
    {
        _image.enabled = false;

        if (isDisplayed)
        {
            isDisplayed = false;

        }
    }

    #endregion


    #region Mouse Inputs

    public void OverlayAlteration()
    {
        UIManager.Instance.AlterationDetailsPanel.OpenDetails(transform.position, currentData);
    }

    public void QuitOverlayAlteration()
    {
        UIManager.Instance.AlterationDetailsPanel.CloseDetails();
    }

    #endregion
}
