using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

public enum MenuType
{
    BaseActions,
    Move,
    Skills,
    UseItems
}


public class PlayerActionsMenu : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private Vector3 positionOffset;
    [SerializeField] private Color impossibleActionRemovedColor;
    [SerializeField] private Color overlayActionAddedColor;
    [SerializeField] private Sprite filledActionPointSprite;
    [SerializeField] private Sprite emptyActionPointSprite;
    [SerializeField] private Sprite filledSkillPointSprite;
    [SerializeField] private Sprite emptySkillPointSprite;

    [Header("Private Infos")]
    private Hero currentHero;
    private Color[] colorSaves;
    private bool movedUnit;
    private bool usedSkill;
    private bool isOpened;
    private MenuType currentMenu;

    [Header("Public Infos")]
    public MenuType CurrentMenu { get { return currentMenu; } }

    [Header("References")]
    [SerializeField] private Animator _animator;
    [SerializeField] private Image[] _buttonImages;
    [SerializeField] private SkillsPanel _skillsPanel;
    [SerializeField] private Image[] _skillPointImages;
    [SerializeField] private Image[] _actionPointImages;


    private void Start()
    {
        colorSaves = new Color[_buttonImages.Length];
        for (int i = 0; i < _buttonImages.Length; i++)
        {
            colorSaves[i] = _buttonImages[i].color;
        }
    }

    private void Update()
    {
        if (!BattleManager.Instance.IsInBattle) return;
        if (BattleManager.Instance.CurrentUnit is null) return;
        if (BattleManager.Instance.CurrentUnit.GetType() != typeof(Hero)) return;

        if (InputManager.wantsToReturn)
        {
            ReturnPreviousBattleMenu();
        }
    }


    public void SetupHeroActionsUI(Hero currentHero)
    {
        this.currentHero = currentHero;
        movedUnit = false;
        usedSkill = false;

        OpenActionsMenu();
    }

    public void OpenActionsMenu()
    {
        if (currentHero.CurrentActionPoints == 0) return;
        if (!BattleManager.Instance.IsInBattle) return;

        CameraManager.Instance.OnCameraMouseInput += CloseActionsMenu;
        currentHero.OnClickUnit -= OpenActionsMenu;

        currentMenu = MenuType.BaseActions;
        isOpened = true;

        CameraManager.Instance.FocusOnTr(currentHero.transform, 5f);

        transform.position = currentHero.transform.position + positionOffset;
        _animator.SetBool("IsOpenned", true);

        if(currentHero.IsHindered) _buttonImages[0].ULerpImageColor(0.1f, colorSaves[0] - impossibleActionRemovedColor);

        for (int i = 0; i < 4; i++)
        {
            QuitOverlayButtonInstant(i);
        }

        for(int i = 0; i < _skillPointImages.Length; i++)
        {
            if (i >= currentHero.CurrentMaxSkillPoints)
            {
                _skillPointImages[i].gameObject.SetActive(false);
                continue;
            }

            _skillPointImages[i].gameObject.SetActive(true);
            _skillPointImages[i].sprite = currentHero.CurrentSkillPoints > i ? filledSkillPointSprite : emptySkillPointSprite;
        }

        for (int i = 0; i < _actionPointImages.Length; i++)
        {
            _actionPointImages[i].sprite = currentHero.CurrentActionPoints > i ? filledActionPointSprite : emptyActionPointSprite;
        }
    }

    public void CloseActionsMenu()
    {
        CameraManager.Instance.OnCameraMouseInput -= CloseActionsMenu;
        currentHero.OnClickUnit += OpenActionsMenu;

        _animator.SetBool("IsOpenned", false);
        isOpened = false;
    }


    #region Button Effects

    public void StartMoveAction()
    {
        if (movedUnit || currentHero.IsHindered) return;

        currentMenu = MenuType.Move;
        BattleManager.Instance.DisplayPossibleMoveTiles(currentHero);

        CloseActionsMenu();
    }

    public void StartSkillsAction()
    {
        if(usedSkill) return;

        currentMenu = MenuType.Skills;
        _skillsPanel.OpenSkillsPanel(currentHero);

        CloseActionsMenu();
    }

    public void StartUseObjectAction()
    {

    }

    public void EndTurnAction()
    {
        currentHero.EndTurn(0.5f);

        CloseActionsMenu();
    }

    #endregion


    #region Overlay / Click Effects

    public void OverlayButton(int buttonIndex)
    {
        if (buttonIndex == 0 && (movedUnit || currentHero.IsHindered)) return;

        _buttonImages[buttonIndex].ULerpImageColor(0.1f, colorSaves[buttonIndex] + overlayActionAddedColor);
        _buttonImages[buttonIndex].rectTransform.UChangeScale(0.1f, Vector3.one * 1.05f);
    }

    public void QuitOverlayButton(int buttonIndex)
    {
        if (buttonIndex == 0 && movedUnit || currentHero.IsHindered) return;

        _buttonImages[buttonIndex].rectTransform.UStopChangeScale();

        _buttonImages[buttonIndex].ULerpImageColor(0.15f, colorSaves[buttonIndex]);
        _buttonImages[buttonIndex].rectTransform.UChangeScale(0.15f, Vector3.one);
    }

    public void QuitOverlayButtonInstant(int buttonIndex)
    {
        if (buttonIndex == 0 && movedUnit || currentHero.IsHindered) return;

        _buttonImages[buttonIndex].color = colorSaves[buttonIndex];
        _buttonImages[buttonIndex].rectTransform.localScale = Vector3.one;
    }

    #endregion


    #region Others

    private void MovedUnit()
    {
        movedUnit = true;
        _buttonImages[0].ULerpImageColor(0.1f, colorSaves[0] - impossibleActionRemovedColor);

        OpenActionsMenu();
    }

    private void ReturnPreviousBattleMenu()
    {
        switch (currentMenu)
        {
            case MenuType.Move:
                OpenActionsMenu();
                CameraManager.Instance.FocusOnTr(currentHero.transform, 5f);
                BattleManager.Instance.ResetTiles();
                break;

            case MenuType.Skills:
                OpenActionsMenu();
                CameraManager.Instance.FocusOnTr(currentHero.transform, 5f);
                BattleManager.Instance.ResetTiles();
                _skillsPanel.CloseSkillsPanel();
                break;

            case MenuType.BaseActions:
                if (!isOpened)
                {
                    OpenActionsMenu();
                }
                break;
        }
    }

    #endregion
}
