using UnityEngine;

public class BladeTrapCollider : MonoBehaviour
{
    [Header("Public Infos")]
    public bool canCollide;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!collision.CompareTag("Hero")) return;
        if (!canCollide) return;

        HeroesManager.Instance.TakeDamage(1);
        canCollide = false;
    }
}
