using UnityEngine;

public class Goal : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("🎉 Goal reached! ダンジョンクリア！");
            // クリア演出やシーン遷移をここで処理
        }
    }
}
