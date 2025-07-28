using System.Collections;
using UnityEngine;

public class SpriteLayererManager : MonoBehaviour
{
    public void InitialiseAll()
    {
        SpriteLayerer[] spriteLayerers = FindObjectsByType<SpriteLayerer>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        for(int i = 0; i < spriteLayerers.Length; i++)
        {
            spriteLayerers[i].Initialise(HeroesManager.Instance.Heroes[HeroesManager.Instance.CurrentHeroIndex].transform);
        }
    }

    public IEnumerator InitialiseAllCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        InitialiseAll();
    }
}
