using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSceneManager : MonoBehaviour
{
    public string LevelName;
    public GameObject LoadingObj;
    public Image ImgFront;

    public void OnServerClicked()
    {
        Constants.isServer = true;
        StartCoroutine(LoadScene());
    }
    public void OnPlayClicked()
    {
        Constants.isServer = false;
        StartCoroutine(LoadScene());
    }

    public void OnBackClicked()
    {
        SceneManager.LoadScene("MainScene");
    }
    IEnumerator LoadScene()
    {
        AsyncOperation loading = SceneManager.LoadSceneAsync(LevelName);
        LoadingObj.SetActive(true);

        while (loading.isDone == false)
        {
            ImgFront.fillAmount = loading.progress;
            yield return null;
        }
    }

}
