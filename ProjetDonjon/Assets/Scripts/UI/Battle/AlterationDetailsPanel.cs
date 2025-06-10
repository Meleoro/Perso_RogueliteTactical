using TMPro;
using UnityEngine;

public class AlterationDetailsPanel : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private Vector2 offset;
    [SerializeField] private string shieldAlterationName;
    [SerializeField] private string shieldAlterationDescr;
    [SerializeField] private string provocationAlterationName;
    [SerializeField] private string provocationAlterationDescr;
    [SerializeField] private string hinderedAlterationName;
    [SerializeField] private string hinderedAlterationDescr;
    [SerializeField] private string weakenedAlterationName;
    [SerializeField] private string weakenedAlterationDescr;

    [Header("References")]
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _descriptionText;
    [SerializeField] private TextMeshProUGUI _counterText;
    [SerializeField] private Animator _animator;
    [SerializeField] private RectTransform _parentRectTr;


    public void OpenDetails(Vector2 position, StatModificatorStruct infos)
    {
        _animator.SetBool("IsOpened", true);
        transform.position = position + offset;

        switch (infos.modificatorType)
        {
            case SkillEffectType.AddShield:
                _nameText.text = shieldAlterationName;
                _descriptionText.text = shieldAlterationDescr;
                _counterText.text = infos.modificatorAdditiveStrength.ToString();
                break;

            case SkillEffectType.Provoke:
                _nameText.text = provocationAlterationName;
                _descriptionText.text = provocationAlterationDescr;
                _counterText.text = infos.modificatorTurnsLeft.ToString();
                break;

            case SkillEffectType.ModifyStrength:
                if(infos.modificatorAdditiveStrength < 0)
                {
                    _nameText.text = weakenedAlterationName;
                    _descriptionText.text = weakenedAlterationDescr;
                }
                _counterText.text = infos.modificatorTurnsLeft.ToString();
                break;

            case SkillEffectType.Hinder:
                _nameText.text = hinderedAlterationName;
                _descriptionText.text = hinderedAlterationDescr;
                _counterText.text = infos.modificatorTurnsLeft.ToString();
                break;
        }
    }

    public void CloseDetails()
    {
        _animator.SetBool("IsOpened", false);


    }
}
