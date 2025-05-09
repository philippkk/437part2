using UnityEngine;

public class swordController : MonoBehaviour
{
    public int damage = 1;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("enemy"))
        {
            followEnemy enemy = other.GetComponent<followEnemy>();
            if (enemy != null)
            {
                enemy.HitEnemy(damage);
            }
        }
    }
}
