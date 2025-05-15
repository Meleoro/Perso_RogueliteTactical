using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeroInfoPanel : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image _heroImage;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private SkillPoint[] _skillPoints;
    [SerializeField] private TextMeshProUGUI _healthText;
    [SerializeField] private Image _healthFillableImage;


    public void InitialisePanel(Hero hero)
    {
        _heroImage.sprite = hero.HeroData.unitImage;
        _nameText.text = hero.HeroData.unitName;

        ActualisePanel(hero);
    }

    public void ActualisePanel(Hero hero)
    {
        _levelText.text = "LVL." + hero.CurrentLevel;

        for(int i = 0; i < hero.CurrentSkillPoints; i++)
        {
            _skillPoints[i].gameObject.SetActive(true);
            _skillPoints[i].ActivateSkillPoint();
        }

        for (int i = hero.CurrentSkillPoints; i < hero.CurrentMaxSkillPoints; i++)
        {
            _skillPoints[i].gameObject.SetActive(true);
            _skillPoints[i].DeactivateSkillPoint();
        }

        for(int i = hero.CurrentMaxSkillPoints; i < _skillPoints.Length; i++)
        {
            _skillPoints[i].gameObject.SetActive(false);
        }

        _healthText.text = hero.CurrentHealth + "/" + hero.CurrentMaxHealth;
        _healthFillableImage.fillAmount = (float)hero.CurrentHealth / hero.CurrentMaxHealth;
    }
}
