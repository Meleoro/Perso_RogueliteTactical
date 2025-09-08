using DG.Tweening;
using DG.Tweening.Core.Easing;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Utilities;

public class MainMetaMenu : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private float hoverRotationPower = 5f;

    [Header("Private Infos")]
    private Coroutine hoverCoroutine;
    private Vector2[] saveLocalPosShadows;
    private int unhoveredIndex = -1;

    [Header("References")]
    [SerializeField] private RectTransform[] _buttonsRectTr;
    [SerializeField] private RectTransform[] _shadowsRectTr;
    [SerializeField] private ExpeditionsMenu _expeditionsMenu;
    [SerializeField] private ChestMenu _chestMenu;
    [SerializeField] private CollectionMenu _collectionMenu;
    [SerializeField] private RectTransform _mainRectTr;
    [SerializeField] private Transform _globalRectTr;
    [SerializeField] private RectTransform _shownPosRef;
    [SerializeField] private RectTransform _bottomHiddenPos;
    [SerializeField] private RectTransform _leftHiddenPos;
    [SerializeField] private RectTransform _rightHiddenPos;



    private void Start()
    {
        saveLocalPosShadows = new Vector2[_shadowsRectTr.Length];

        for (int i = 0; i < _shadowsRectTr.Length; i++)
        {
            saveLocalPosShadows[i] = _shadowsRectTr[i].localPosition;
        }
    }



    #region Show / Hide

    public void Hide(Vector3 hidePos)
    {
        StartCoroutine(HideCoroutine(hidePos));
    }

    private IEnumerator HideCoroutine(Vector3 hidePos)
    {
        Vector3 dir = _mainRectTr.position - hidePos;

        _mainRectTr.DOMove(_mainRectTr.position + dir.normalized * 0.4f, 0.3f, false).SetEase(Ease.InOutSine);
        _globalRectTr.DOScale(Vector3.one * 0.9f, 0.4f).SetEase(Ease.InOutSine);

        yield return new WaitForSeconds(0.3f);

        _mainRectTr.DOMove(hidePos - dir.normalized * 0.4f, 0.3f, false).SetEase(Ease.InOutSine);

        yield return new WaitForSeconds(0.15f);

        _globalRectTr.DOScale(Vector3.one * 1f, 0.4f).SetEase(Ease.InOutSine);

        yield return new WaitForSeconds(0.15f);

        _mainRectTr.DOMove(hidePos, 0.3f, false).SetEase(Ease.InOutSine);
    }


    public void Show()
    {
        StartCoroutine(ShowCoroutine());
    }

    private IEnumerator ShowCoroutine()
    {
        Vector3 dir = _mainRectTr.position - _shownPosRef.position;

        _mainRectTr.DOMove(_mainRectTr.position + dir.normalized * 0.4f, 0.3f, false).SetEase(Ease.InOutSine);
        _globalRectTr.DOScale(Vector3.one * 0.9f, 0.4f).SetEase(Ease.InOutSine);

        yield return new WaitForSeconds(0.3f);

        _mainRectTr.DOLocalMove(Vector3.zero - _mainRectTr.parent.InverseTransformVector(dir.normalized * 0.4f), 0.3f, false).SetEase(Ease.InOutSine);

        yield return new WaitForSeconds(0.15f);

        _globalRectTr.DOScale(Vector3.one * 1f, 0.4f).SetEase(Ease.InOutSine);

        yield return new WaitForSeconds(0.15f);

        _mainRectTr.DOLocalMove(Vector3.zero, 0.3f, false).SetEase(Ease.InOutSine);
    }

    #endregion


    #region Open Menus

    public void ClickExpeditions()
    {
        if (UIMetaManager.Instance.IsInTransition) return;

        Hide(_bottomHiddenPos.position);
        _expeditionsMenu.Show();
    }

    public void ClickChest()
    {
        if (UIMetaManager.Instance.IsInTransition) return;

        Hide(_leftHiddenPos.position);
        _chestMenu.Show();
    }

    public void ClickTreasures()
    {
        if (UIMetaManager.Instance.IsInTransition) return;

        _collectionMenu.Show();

    }

    public void ClickShop()
    {
        if (UIMetaManager.Instance.IsInTransition) return;

    }

    public void ClickSmith()
    {
        if (UIMetaManager.Instance.IsInTransition) return;

    }

    #endregion


    #region Buttons Hover

    public void HoverButton(int index)
    {
        if(hoverCoroutine != null) { 
            StopCoroutine(hoverCoroutine);
        }

        hoverCoroutine = StartCoroutine(HoverButtonCoroutine(index));
    }

    private IEnumerator HoverButtonCoroutine(int index)
    {
        _buttonsRectTr[index].DOScale(Vector3.one * 1.3f, 0.1f).SetEase(Ease.InOutSine);

        yield return new WaitForSeconds(0.1f);

        _buttonsRectTr[index].DOScale(Vector3.one * 1.15f, 0.2f).SetEase(Ease.InOutSine);

        yield return new WaitForSeconds(0.5f);

        while(true)
        {
            _buttonsRectTr[index].DOScale(Vector3.one * 1.2f, 0.85f).SetEase(Ease.InOutSine);

            yield return new WaitForSeconds(1f);

            _buttonsRectTr[index].DOScale(Vector3.one * 1.15f, 0.85f).SetEase(Ease.InOutSine);

            yield return new WaitForSeconds(1f);
        }
    }

    public void UnHoverButton(int index)
    {
        if (unhoveredIndex == index) return;

        if (hoverCoroutine != null)
        {
            StopCoroutine(hoverCoroutine);
        }

        _buttonsRectTr[index].DOComplete();

        _buttonsRectTr[index].DOScale(Vector3.one * 1f, 0.2f).SetEase(Ease.InOutSine);
    }

    #endregion
}
