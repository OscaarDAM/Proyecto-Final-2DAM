using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 10f;
    public float lifeTime = 2f;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // El proyectil se mueve hacia la derecha (porque es como está rotado por defecto)
        transform.position += transform.right * speed * Time.deltaTime;
    }
}
