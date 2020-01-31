using UnityEngine;

namespace Generation
{
    public class Despawner : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Planet") || other.CompareTag("Projectile"))
            {
                Destroy(other.gameObject);
            }
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.CompareTag("Planet") || other.gameObject.CompareTag("Projectile"))
            {
                Destroy(other.gameObject);
            }
        }
    }
}
