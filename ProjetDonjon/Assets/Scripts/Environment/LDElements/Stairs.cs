using UnityEngine;

public class Stairs : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        HeroesManager.Instance.TakeStairs();
    }
}
