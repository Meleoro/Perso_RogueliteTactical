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
    private StatModificatorStruct currentData;
    private bool isDisplayed;

    [Header("References")]
    [SerializeField] private Image _image;


    private void Setup(StatModificatorStruct info)
    {
        currentData = info;

        switch (info.modificatorType)
        {
            case SkillEffectType.Damage:
                _image.sprite = attackSprite;
                break;

            case SkillEffectType.ModifyStrength:
                _image.sprite = attackSprite;
                break;

            case SkillEffectType.ModifySpeed:
                if (info.modificatorAdditiveStrength > 0 || info.modificatorMultipliedStrength > 0)
                    _image.sprite = buffSprite;
                else
                    _image.sprite = debuffSprite;
                break;

            case SkillEffectType.ModifyLuck:
                if (info.modificatorAdditiveStrength > 0 || info.modificatorMultipliedStrength > 0)
                    _image.sprite = buffSprite;
                else
                    _image.sprite = debuffSprite;
                break;

            case SkillEffectType.ModifyMove:
                _image.sprite = speedSprite;
                break;

            case SkillEffectType.AddShield:
                _image.sprite = shieldSprite;
                break;

            case SkillEffectType.Provoke:
                _image.sprite = provocationSprite;
                break;
        }

        if(info.modificatorAdditiveStrength > 0 || info.modificatorMultipliedStrength > 0)
        {
            _image.color = buffColor;
        }
        else
        {
            _image.color = debuffColor;
        }
    }


    #region Appear / Disappear

    public void Appear(StatModificatorStruct info)
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
