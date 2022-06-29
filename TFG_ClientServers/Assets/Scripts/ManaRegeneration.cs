using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ManaRegeneration : NetworkBehaviour
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

    public float IntervaloRegeneracion = 1.5f;
    public int ManaRegenerado = 10;

    private void Start()
    {
        StartCoroutine(ManaRegenerationCoroutine());
    }

    [Server]
    private IEnumerator ManaRegenerationCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(IntervaloRegeneracion);
            foreach (var player in Room.GamePlayers)
            {
                player.playerControl.IncreaseMana(ManaRegenerado);
            }
        }
    }
}
