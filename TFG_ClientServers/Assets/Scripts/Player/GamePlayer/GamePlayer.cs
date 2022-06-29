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

    public PlayerControl playerControl = null;

    private void Start() => DontDestroyOnLoad(transform.root.gameObject);

    public override void OnStopClient() => Room.GamePlayers.Remove(this);

    public override void OnStartClient()
    {
        DontDestroyOnLoad(transform.root.gameObject);
        Room.GamePlayers.Add(this);
        playerControl = gameObject.GetComponent<PlayerControl>();
        cmd_ChangeColor(Room.colorJugador);
    }

    public override void OnStartServer() // Añadirlo al servidor
    {
        DontDestroyOnLoad(transform.root.gameObject);
        Room.GamePlayers.Add(this);
        playerControl = gameObject.GetComponent<PlayerControl>();
        base.OnStartServer();
    }

    [Command]
    private void cmd_ChangeColor(Color color) => ChangeColor(color);

    [ClientRpc]
    public void ChangeColor(Color color) => GetComponentInChildren<SpriteRenderer>().color = color;
}
