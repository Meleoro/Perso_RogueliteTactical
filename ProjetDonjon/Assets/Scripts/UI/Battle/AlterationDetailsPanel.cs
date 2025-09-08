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


    public void OpenDetails(Vector2 position, AlterationStruct infos, Unit attachedUnit)
    {
        _animator.SetBool("IsOpened", true);
        transform.position = position + offset;

        _nameText.text = infos.alteration.alterationName;
        _descriptionText.text = infos.alteration.alterationDescription;

        switch (infos.alteration.alterationType)
        {
            case AlterationType.Shield:
                _counterText.text = attachedUnit.CurrentShield.ToString();
                break;

            case AlterationType.Strength:
                _counterText.text = infos.duration.ToString();
                break;

            case AlterationType.Weakened:
                _counterText.text = infos.duration.ToString();
                break;

            case AlterationType.Hindered:
                _counterText.text = infos.duration.ToString();
                break;

            case AlterationType.Provocked:
                _counterText.text = infos.duration.ToString();
                break;

            case AlterationType.Vulnerable:
                _counterText.text = infos.duration.ToString();
                break;
        }
    }

    public void CloseDetails()
    {
        _animator.SetBool("IsOpened", false);


    }
}
