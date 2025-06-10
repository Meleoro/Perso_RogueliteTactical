using UnityEngine;

public class Arrow : MonoBehaviour
{
    [Header("Private Infos")]
    private float speed;
    private int damages;


    public void InitialiseArrow(float arrowSpeed, int arrowDamages)
    {
        speed = arrowSpeed;
        damages = arrowDamages;
    }

    void Update()
    {
        transform.position += transform.right * speed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Hero"))
        {
            HeroesManager.Instance.TakeDamage(damages);
            Destroy(gameObject);
        } 
        else if (collision.CompareTag("Walls"))
        {
            Destroy(gameObject);
        }
    }
}
