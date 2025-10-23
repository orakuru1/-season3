using UnityEngine;

public class DebugTester : MonoBehaviour
{
    void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LogStatus();
        }
        else
        {
            Debug.Log("GameManager not found!");
        }
    }
}
