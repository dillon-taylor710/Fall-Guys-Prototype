using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SplashSceneManager : MonoBehaviour
{
    public Image imgProgress;

    // Start is called before the first frame update
    IEnumerator Start()
    {
        yield return new WaitForSeconds(2f);
        AsyncOperation loading = SceneManager.LoadSceneAsync("MainScene");

        while (loading.isDone == false)
        {
            imgProgress.fillAmount = loading.progress;
            yield return null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
