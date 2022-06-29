using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public abstract class Matchmaker : MonoBehaviour
{
    protected List<PlayerData> Players = new List<PlayerData>();
    public int PlayersPerMatch = 4;
    protected NetworkManagerTFG room;
    protected NetworkManagerTFG Room
    {
        get
        {
            if (room != null) { return room; }
            return room = NetworkManager.singleton as NetworkManagerTFG;
        }
    }


    public abstract void Process();
    public virtual void Add(PlayerData player) => Players.Add(player);
    public virtual void Remove(PlayerData player) => Players.Remove(player);
    public virtual void Remove(string username)
    {
        foreach(var player in Players.ToArray())
        {
            if(player.player_tag == username)
            {
                Players.Remove(player);
                break;
            }
        }
    }

}
