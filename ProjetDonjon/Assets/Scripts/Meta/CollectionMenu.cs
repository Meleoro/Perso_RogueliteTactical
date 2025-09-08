using System;
using System.Collections;
using UnityEngine;
using Utilities;

public class CollectionMenu : MonoBehaviour
{


    [Header("Actions")]
    public Action OnStartTransition;
    public Action OnEndTransition;
    public Action OnShow;
    public Action OnHide;

    [Header("Private Infos")]
    private bool[] possessedRelics;

    [Header("References")]
    [SerializeField] private RectTransform _mainRectTr;
    [SerializeField] private CollectionRelic[] _collectionRelics;



    private void Start()
    {
        
    }


    public void LoadPage(int index)
    {
        int startRelicIndex = index * 12;

        for(int i = startRelicIndex; i < startRelicIndex + 12; i++)
        {
            if (i >= RelicsManager.Instance.AllRelics.Length) _collectionRelics[i - startRelicIndex].Setup(null, false);
            else _collectionRelics[i - startRelicIndex].Setup(RelicsManager.Instance.AllRelics[i], possessedRelics[i]);
        }
    }


    #region Open / Close

    public void Show()
    {
        OnStartTransition.Invoke();
        OnShow.Invoke();

        possessedRelics = RelicsManager.Instance.PossessedRelicIndexes;
        LoadPage(0);

        StartCoroutine(ShowCoroutine());
    }

    private IEnumerator ShowCoroutine()
    {
        yield return new WaitForSeconds(0.2f);

        _mainRectTr.UChangeScale(0.2f, Vector3.one * 1.2f);

        yield return new WaitForSeconds(0.2f);

        _mainRectTr.UChangeScale(0.2f, Vector3.one * 1f);

        OnEndTransition.Invoke();
    }


    public void Hide()
    {
        OnStartTransition.Invoke();
        OnHide.Invoke();

        StartCoroutine(HideCoroutine());
    }

    private IEnumerator HideCoroutine()
    {
        yield return new WaitForSeconds(0.2f);

        _mainRectTr.UChangeScale(0.2f, Vector3.one * 1.2f);

        yield return new WaitForSeconds(0.2f);

        _mainRectTr.UChangeScale(0.2f, Vector3.one * 0f);

        OnEndTransition.Invoke();
    }

    #endregion
}
