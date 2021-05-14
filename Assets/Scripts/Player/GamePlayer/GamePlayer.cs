using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

// Desde TFG_ServidorPartida no se manejará esto, esto se peleará con el ServidorCombate
public class GamePlayer : NetworkBehaviour
{
    // public Color ColorSprite = Color.blue;
    private NetworkManagerTFG room;
    private NetworkManagerTFG Room
    {
        get
        {
            if (room != null) { return room; }
            return room = NetworkManager.singleton as NetworkManagerTFG;
        }
    }

    private void Start()
    {
        DontDestroyOnLoad(transform.root.gameObject);
    }

    public override void OnStartClient()
    {
        DontDestroyOnLoad(transform.root.gameObject);
        Room.GamePlayers.Add(this);
        // cmd_ChangeColor();
    }

    public override void OnStartServer() // Añadirlo al servidor
    {
        DontDestroyOnLoad(transform.root.gameObject);
        Room.GamePlayers.Add(this);
        base.OnStartServer();
    }

    public override void OnStopClient()
    {
        Room.GamePlayers.Remove(this);
    }

    [ClientRpc]
    public void ChangeConnection(string port)
    {
        if (!hasAuthority) return;
        string puerto = port;
        Room.StartChangeConnection(puerto);
    }

    // [Command]
    // private void cmd_ChangeColor() 
    // {
    //     // this.ChangeColor(Room.colores[Room.GetIndex()]);
    //     this.ChangeColor(ColorSprite);
    //     // Room.AumentarIndex();
    // }

    // [ClientRpc]
    // public void ChangeColor(Color color) 
    // {
    //     GetComponentInChildren<SpriteRenderer>().color = color;
    // }
}
