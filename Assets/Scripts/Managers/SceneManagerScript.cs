using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SCENE_STATE
{
    SCENE_WAITING,
    SCENE_READY,
    SCENE_GAME
}

public class SceneManagerScript : MonoBehaviour
{
    public GameObject WaitingObj;
    public GameObject ReadyObj;
    public GameObject GameObj;

    public SCENE_STATE state;

    void Start()
    {
        SetState(SCENE_STATE.SCENE_WAITING);
    }

    public void SetState(SCENE_STATE st)
    {
        state = st;

        switch (state)
        {
            case SCENE_STATE.SCENE_WAITING:
                WaitingObj.SetActive(true);
                ReadyObj.SetActive(false);
                GameObj.SetActive(false);
                break;
            case SCENE_STATE.SCENE_READY:
                WaitingObj.SetActive(false);
                ReadyObj.SetActive(true);
                GameObj.SetActive(false);
                break;
            case SCENE_STATE.SCENE_GAME:
                WaitingObj.SetActive(false);
                ReadyObj.SetActive(false);
                GameObj.SetActive(true);

                break;
        }
    }
}
