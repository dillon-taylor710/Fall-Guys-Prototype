using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallenGuysNetworkManager : NetworkManager
{
    public Transform[] spawnPoints;

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        //base.OnServerAddPlayer(conn);

        if (numPlayers >= maxConnections)
            return;

        Transform start = spawnPoints[numPlayers];
        GameObject player = Instantiate(playerPrefab, start.position, start.rotation);
        NetworkServer.AddPlayerForConnection(conn, player);
    }
    //public override void OnServerAddPlayer(NetworkConnection conn)
    //{
    //    // add player at correct spawn position
    //    Transform start = numPlayers == 0 ? spawnPointOne : spawnPointTwo;
    //    GameObject player = Instantiate(playerPrefab, start.position, start.rotation);
    //    NetworkServer.AddPlayerForConnection(conn, player);
    //}
}
