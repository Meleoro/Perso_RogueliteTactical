using UnityEngine;
using UnityEngine.UI;

public class SkillPoint : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private Sprite activeSprite;
    [SerializeField] private Sprite unactiveSprite;

    [Header("Private Infos")]
    private bool isActive;

    [Header("Public Infos")]
    public bool IsActive { get { return isActive; } }

    [Header("Parameters")]
    [SerializeField] private Image _image;


    public void ActivateSkillPoint()
    {
        isActive = true;
        _image.sprite = activeSprite;
    }

    public void DeactivateSkillPoint()
    {
        isActive = false;
        _image.sprite = unactiveSprite;
    }
}
