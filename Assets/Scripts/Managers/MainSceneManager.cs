using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[System.Serializable]
public class CharacterTexture
{
    public Texture Main;
    public Texture Metalic;
    public Texture Normal;
}

public class MainSceneManager : MonoBehaviour
{
    public Text txtLevel;
    public Image imgLevelProgress;

    public Renderer character_renderer;
    public List<CharacterTexture> character_textures;
    public List<Toggle> character_toggles;

    public GameObject ExitMessage;

    void Awake()
    {
        int skin_id = PlayerPrefs.GetInt("Char_SKIN_ID", 0);

        character_toggles[skin_id].isOn = true;
    }

    public void OnBackClicked()
    {
        ExitMessage.SetActive(true);
    }

    public void OnPlayClicked()
    {
        SceneManager.LoadScene("LevelScene");
    }

    public void OnCharSkin(int index)
    {
        if (index < character_textures.Count && character_toggles[index].isOn == true)
        {
            character_renderer.material.SetTexture("_MainTex", character_textures[index].Main);
            //character_renderer.material.SetTexture("_MetallicGlossMap", character_textures[index].Metalic);
            //character_renderer.material.SetTexture("_BumpMap", character_textures[index].Normal);

            PlayerPrefs.SetInt("Char_SKIN_ID", index);
        }
    }

    public void OnMessageYes()
    {
        Application.Quit();
    }

    public void OnMessageNo()
    {
        ExitMessage.SetActive(false);
    }
}
