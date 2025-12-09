using UnityEngine;
using UnityEngine.SceneManagement;

public class StartButton : MonoBehaviour
{
    public void OnStartButton()
    {
        SceneManager.LoadScene("Scene_Select");
    }
}
