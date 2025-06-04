using UnityEngine;

public class SimpleEnemy2D : MonoBehaviour
{
    public GameObject projectilePrefab;
    public float shootCooldown = 2f;
    public float projectileSpeed = 5f;

    private Transform player;
    private float shootTimer = 0f;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogError("No se encontró el jugador. Asegúrate de que tenga el tag 'Player'.");
        }
    }

    void Update()
    {
        if (player == null) return;

        shootTimer += Time.deltaTime;

        if (shootTimer >= shootCooldown)
        {
            ShootAtPlayer();
            shootTimer = 0f;
        }
    }

    void ShootAtPlayer()
    {
        if (projectilePrefab == null) return;

        Vector2 direction = (player.position - transform.position).normalized;

        GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = direction * projectileSpeed;
        }
    }
}
