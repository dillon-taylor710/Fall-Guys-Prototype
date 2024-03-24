using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Security.Policy;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : NetworkBehaviour
{
    public SceneManagerScript Manager;
    
    // WTF SINGLETONS HOW DARE Y-
    #region Singleton
    public static GameManager singleton;
    private void Awake()
    {
        if (singleton != this)
            singleton = this;
    }
    #endregion

    [Header("Game Settings")]
    public int minPlayers = 1;
    public float timeTrialLength = 30.0f;

    private static List<NetworkIdentity> readyPlayers = new List<NetworkIdentity>();

    [Header("UI Elements")]
    public GameObject waitingForHost;
    public GameObject startGameButton, winnerText;
    public GameObject WinnerParticle;

    public GameObject Navigation;
    public Animator CountAnim;

    public GameObject objPos;
    public Text txtPos;

    [SyncVar]
    public int numPlayers = 0;

    /// <summary>
    /// Tell the server the Client is ready to play.
    /// </summary>
    /// <param name="netId">The NetID of the ready Client.</param>
    //[Command(ignoreAuthority = true)]
    public void CmdReadyPlayer(NetworkIdentity netId)
    {
        readyPlayers.Add(netId);

        // Reached playable number of connections
        if (NetworkServer.connections.Count >= minPlayers)
        {
            RpcHostReadyUp();
        }
    }

    /// <summary>
    /// Tells Host/Client to toggle respective ready UI
    /// </summary>
    [ClientRpc]
    public void RpcHostReadyUp()
    {
        //StartCoroutine(StartNavigation());
    }

    public void OnBackPressed()
    {
        SceneManager.LoadScene("LevelScene");
    }

    public void StartNavigation()
    {
        StartCoroutine(SetNavigation());
    }

    IEnumerator SetNavigation()
    {
        yield return null;

        if (isServer)
        {
            // Host: Start game button
            //startGameButton.SetActive(true);
            Navigation.SetActive(true);
        }
        else
        {
            // Client: Waiting for Host
            //waitingForHost.SetActive(true);
            Navigation.SetActive(true);
        }
    }

    public void OnNavigationEnd(ORNavigation navigation)
    {
        navigation.gameObject.SetActive(false);

        StartCoroutine(StartGame(navigation));
    }

    IEnumerator StartGame(ORNavigation navigation)
    {
        yield return new WaitForSeconds(1f);

        NetworkClient.localPlayer.GetComponent<Player>().enabled = true;
        yield return new WaitForEndOfFrame();
        NetworkClient.localPlayer.GetComponent<Player>().enabled = false;
        yield return new WaitForSeconds(1f);

        CountAnim.enabled = true;

        yield return new WaitForSeconds(4f);

        if (isServer)
            RpcOnStartGame();

        yield return new WaitForSeconds(20f);
        objPos.SetActive(true);
    }


    /// <summary>
    /// Starts the Game
    /// </summary>
    [ClientRpc]
    public void RpcOnStartGame()
    {
        // Remove Client/Host UI
        if (isServer)
        {
            startGameButton.SetActive(false);
        }
        else
            waitingForHost.SetActive(false);


        //ClientScene.localPlayer.GetComponent<Player>().enabled = true;
        NetworkClient.localPlayer.GetComponent<Player>().enabled = true;
        //CursorManager.ToggleCursor(false);
    }

    /// <summary>
    /// Ends the current game session.
    /// </summary>
    [ClientRpc]
    public void RpcEndGame(NetworkIdentity winner)
    {
        StartCoroutine(Celebrate(winner));
    }

    [Client]
    IEnumerator Celebrate(NetworkIdentity winner)
    {
        if (winner == NetworkClient.localPlayer) // me
        {
            WinnerParticle.SetActive(true);

            yield return new WaitForSeconds(3f);
            winnerText.SetActive(true);

            yield return new WaitForSeconds(3f);
            //SceneManager.LoadScene(1);

            NetworkClient.localPlayer.gameObject.SetActive(false);
        }
        else
        {
            yield return new WaitForSeconds(3f);
            winner.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (NetworkClient.localPlayer == null)
            return;

        int pos = 1;
        float z = NetworkClient.localPlayer.transform.position.z;

        foreach (NetworkIdentity player in NetworkClient.spawned.Values)
        {
            if (player != NetworkClient.localPlayer && player.GetComponent<Player>() != null)
            {
                if (z < player.transform.position.z)
                    pos++;
            }
        }

        if (pos == 1)
            txtPos.text = pos + "st";
        else if (pos == 2)
            txtPos.text = pos + "nd";
        else if (pos == 3)
            txtPos.text = pos + "rd";
        else
            txtPos.text = pos + "th";
    }
}