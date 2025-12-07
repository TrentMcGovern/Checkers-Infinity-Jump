using UnityEngine;
using UnityEngine.SceneManagement;

public class EndScreen : MonoBehaviour
{
    public void endGame(bool quit)
    {
        if (quit) { Application.Quit();return; }
        
        SceneManager.LoadScene(0);
    }
}
