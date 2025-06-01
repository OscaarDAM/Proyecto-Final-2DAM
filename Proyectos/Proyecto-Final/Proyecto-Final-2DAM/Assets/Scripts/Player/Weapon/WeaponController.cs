using UnityEngine;

public class WeaponController : MonoBehaviour
{
    public GameObject projectilePrefab; // Prefab del proyectil
    public Transform firePoint; // Lugar desde donde se dispara
    public float fireRate = 0.5f; // Tiempo entre disparos

    private float nextFireTime;

    public SpriteRenderer weaponSprite; // Sprite del arma para poder hacer flip vertical

    // Rota el arma hacia la dirección apuntada
    public void Aim(Vector2 direction)
    {
        if (direction.sqrMagnitude > 0.1f)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            // Si el sprite del arma apunta hacia la derecha, hacemos flip si apunta hacia la izquierda
            if (weaponSprite != null)
            {
                weaponSprite.flipY = angle > 90 || angle < -90;
            }
        }
    }

    // Intenta disparar (revisa si ha pasado suficiente tiempo desde el último disparo)
    public void TryShoot()
    {
        if (Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    // Instancia el proyectil en el punto de disparo
    private void Shoot()
    {
        if (projectilePrefab != null && firePoint != null)
        {
            Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        }
    }
}
