using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    // Start is called before the first frame update

    public void SceneSwap(string myScene)
    {
        SceneManager.LoadScene(myScene);
    }

    public void QuitApp()
    {
        Application.Quit();
    }

}