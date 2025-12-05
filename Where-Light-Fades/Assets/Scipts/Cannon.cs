using UnityEngine;

public class Cannon : MonoBehaviour
{
    public enum ShootDirection { Up, Down, Left, Right }
    public ShootDirection direction = ShootDirection.Right;

    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireCooldown = 1f;
    public float bulletSpeed = 6f;

    float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= fireCooldown)
        {
            Shoot();
            timer = 0f;
        }
    }

    void Shoot()
    {
        GameObject b = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Rigidbody2D rb = b.GetComponent<Rigidbody2D>();

        Vector2 dir = Vector2.right;  // default

        switch (direction)
        {
            case ShootDirection.Up:    dir = Vector2.up; break;
            case ShootDirection.Down:  dir = Vector2.down; break;
            case ShootDirection.Left:  dir = Vector2.left; break;
            case ShootDirection.Right: dir = Vector2.right; break;
        }

        rb.velocity = dir * bulletSpeed;
    }
}
