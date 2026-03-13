using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitchInput : MonoBehaviour
{
    public string sceneA;
    public string sceneB;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            string currentScene = SceneManager.GetActiveScene().name;

            if (currentScene == sceneA)
                TransitionManager.Instance.TransitionTo(sceneB);
            else
                TransitionManager.Instance.TransitionTo(sceneA);
        }
    }
}