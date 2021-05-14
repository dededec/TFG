using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using MessagesTFG;

public class RoomPlayer : NetworkBehaviour
{
    private NetworkManagerTFG room;
    private NetworkManagerTFG Room
    {
        get
        {
            if (room != null) { return room; }
            return room = NetworkManager.singleton as NetworkManagerTFG;
        }
    }

    [ClientRpc]
    public void ChangeConnection(string port)
    {
        if (!hasAuthority) return;
        string puerto = port;
        Room.StartChangeConnection(puerto);
    }

    public override void OnStartClient()
    {
        // DontDestroyOnLoad(transform.root.gameObject);
        Room.RoomPlayers.Add(this);
        NetworkClient.RegisterHandler<ChangePortMessage>(OnChangePortMessage);
    }

    public override void OnStartServer()
    {
        // DontDestroyOnLoad(transform.root.gameObject);
        base.OnStartServer();
    }

    public override void OnStopClient()
    {
        Room.RoomPlayers.Remove(this);
    }

    private void OnChangePortMessage(NetworkConnection conn, ChangePortMessage msg)
    {
        string port = msg.Argument;
        ChangeConnection(port);
    }

}
