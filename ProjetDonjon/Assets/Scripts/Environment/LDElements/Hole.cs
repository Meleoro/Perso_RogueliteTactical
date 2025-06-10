using UnityEngine;

public class Hole : MonoBehaviour
{


    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!collision.CompareTag("Hero")) return;
        if (collision.GetComponent<HeroController>().CurrentControllerState == ControllerState.Jump) return;
        if (collision.GetComponent<HeroController>().CurrentControllerState == ControllerState.Fall) return;
        if (BattleManager.Instance.IsInBattle) return;

        StartCoroutine(collision.GetComponent<HeroController>().FallCoroutine());
    }
}
