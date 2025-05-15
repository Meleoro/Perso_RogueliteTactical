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

    [Header("Private Infos")]
    private Hero currentHero;
    private Color[] colorSaves;
    private bool movedUnit;
    private bool usedSkill;
    private MenuType currentMenu;

    [Header("Public Infos")]
    public MenuType CurrentMenu { get { return currentMenu; } }

    [Header("References")]
    [SerializeField] private Animator _animator;
    [SerializeField] private Image[] _buttonImages;
    [SerializeField] private SkillsPanel _skillsPanel;


    private void Start()
    {
        colorSaves = new Color[_buttonImages.Length];
        for (int i = 0; i < _buttonImages.Length; i++)
        {
            colorSaves[i] = _buttonImages[i].color;
        }

        BattleManager.Instance.OnMoveUnit += MovedUnit;
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
        currentMenu = MenuType.BaseActions;

        transform.position = currentHero.transform.position + positionOffset;
        _animator.SetBool("IsOpenned", true);
    }

    public void CloseActionsMenu()
    {
        _animator.SetBool("IsOpenned", false);
    }


    #region Button Effects

    public void StartMoveAction()
    {
        if (movedUnit) return;

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
        currentHero.EndTurn();

        CloseActionsMenu();
    }

    #endregion


    #region Overlay / Click Effects

    public void OverlayButton(int buttonIndex)
    {
        if (buttonIndex == 0 && movedUnit) return;

        _buttonImages[buttonIndex].ULerpImageColor(0.1f, colorSaves[buttonIndex] + overlayActionAddedColor);
        _buttonImages[buttonIndex].rectTransform.UChangeScale(0.1f, Vector3.one * 1.05f);
    }

    public void QuitOverlayButton(int buttonIndex)
    {
        if (buttonIndex == 0 && movedUnit) return;

        _buttonImages[buttonIndex].ULerpImageColor(0.15f, colorSaves[buttonIndex]);
        _buttonImages[buttonIndex].rectTransform.UChangeScale(0.15f, Vector3.one);
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
        }
    }

    #endregion
}
