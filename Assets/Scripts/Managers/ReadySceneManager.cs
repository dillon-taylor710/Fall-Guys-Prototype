using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ReadySceneManager : MonoBehaviour
{
    public SceneManagerScript Manager;
    public GameManager GameManagerObj;

    // Start is called before the first frame update
    void Start()
    {
        Invoke("OnRace", 5f);
    }

    public void OnBackClicked()
    {
        SceneManager.LoadScene("LevelScene");
    }

    public void OnRace()
    {
        GameManagerObj.StartNavigation();
        Manager.SetState(SCENE_STATE.SCENE_GAME);
    }
}
