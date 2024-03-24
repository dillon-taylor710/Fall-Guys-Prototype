using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine.UI;
using Mirror;

public class WaitingSceneManager : MonoBehaviour
{
    public SceneManagerScript Manager;
    public FallenGuysNetworkManager networkManager;

    public Text status;
    public Text lobby;
    public Renderer character_renderer;
    public List<CharacterTexture> character_textures;
    
    // Start is called before the first frame update
    void Start()
    {
        int skin_id = PlayerPrefs.GetInt("Char_SKIN_ID", 0);
        if (skin_id < character_textures.Count)
        {
            character_renderer.material.SetTexture("_MainTex", character_textures[skin_id].Main);
            //character_renderer.material.SetTexture("_MetallicGlossMap", character_textures[index].Metalic);
            //character_renderer.material.SetTexture("_BumpMap", character_textures[index].Normal);
        }

        string[] game_info = File.ReadAllLines(Path.Combine(Directory.GetCurrentDirectory(), "Game.ini"));

        for (int i = 0; i < game_info.Length; i++)
        {
            if (game_info[i].StartsWith("ServerIP"))
            {
                string[] server = game_info[i].Split(':');
                if (server.Length > 1)
                    networkManager.networkAddress = server[1].Trim();
            }
            else if (game_info[i].StartsWith("Players"))
            {
                string[] player = game_info[i].Split(':');
                if (player.Length > 1)
                {
                    networkManager.maxConnections = int.Parse(player[1].Trim());
                    if (networkManager.maxConnections > networkManager.spawnPoints.Length)
                        networkManager.maxConnections = networkManager.spawnPoints.Length;
                }
            }
        }

        StartCoroutine(LoadScene()); 
    }

    public void OnBackClicked()
    {
        SceneManager.LoadScene("LevelScene");
    }

    IEnumerator LoadScene()
    {
        yield return new WaitForSeconds(3f);

        if (Constants.isServer)
        {
            status.text = "CREATING GAME...";
            networkManager.StartHost();
        }
        else
        {
            status.text = "CONNECTING TO SERVER...";

            networkManager.StartClient();
        }
    }

    void Update()
    {
        if (GameManager.singleton.isServer)
            GameManager.singleton.numPlayers = networkManager.numPlayers;
        if (GameManager.singleton.numPlayers > 0)
        {
            status.text = "WAITING FOR PLAYERS...";
            lobby.text = GameManager.singleton.numPlayers + "/" + networkManager.maxConnections + "\nPOPULATING";

            if (GameManager.singleton.numPlayers == networkManager.maxConnections)
                Manager.SetState(SCENE_STATE.SCENE_READY);
        }
    }
}
