using System.Collections;
using UnityEngine;
using Utilities;

public class GameManager : GenericSingletonClass<GameManager>
{
    [Header("Parameters")]
    [SerializeField] private bool startGameInExplo;
    [SerializeField] private EnviroData startEnviroData;


    [Header("Public Infos")]
    public bool IsInExplo { get; private set; }


    private void Start()
    {
        if (startGameInExplo)
        {
            StartExploration(startEnviroData);
        }
        else
        {
            UIMetaManager.Instance.Show();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            StartCoroutine(EndExplorationCoroutine());
        }
    }


    #region Start / End Exploration

    public void StartExploration(EnviroData enviroData)
    {
        UIMetaManager.Instance.Hide();
        IsInExplo = true;

        ProceduralGenerationManager.Instance.StartExploration(enviroData);
    }


    public IEnumerator EndExplorationCoroutine()
    {
        UIManager.Instance.FadeScreen(0.8f, 1);

        yield return new WaitForSeconds(1);

        UIManager.Instance.FadeScreen(0.8f, 0);

        ProceduralGenerationManager.Instance.EndExploration();
        HeroesManager.Instance.EndExploration();

        IsInExplo = false;

        UIMetaManager.Instance.Show();
    }


    #endregion
}
