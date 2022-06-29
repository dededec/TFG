using Mirror;
using UnityEngine;

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

    public override void OnStartClient() => Room.RoomPlayers.Add(this);

    public override void OnStopClient() => Room.RoomPlayers.Remove(this);

}
