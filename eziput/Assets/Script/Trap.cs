using UnityEngine;

public class Trap : MonoBehaviour
{
    public int damage = 20;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"⚠️ Player hit trap! -{damage} HP");
            // PlayerHealthなどにアクセスしてHP減少を実装可
        }
    }
}
