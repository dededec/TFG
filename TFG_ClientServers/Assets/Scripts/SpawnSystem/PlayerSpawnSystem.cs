using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class PlayerSpawnSystem : NetworkBehaviour
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

    [SerializeField] private GameObject playerPrefab = null;
    private static List<Transform> spawnPoints = new List<Transform>(); //static porque es la misma lista para todos los playerSpawnSystem
    private int nextIndex = 0; // No necesita ser static porque SpawnPlayer se hace en el server (y ya se spawnea en los clientes)

    public static void AddSpawnPoint(Transform transform)
    {
        spawnPoints.Add(transform);
        spawnPoints = spawnPoints.OrderBy(x => x.GetSiblingIndex()).ToList();
    }

    public static void RemoveSpawnPoint(Transform transform) => spawnPoints.Remove(transform);
    public override void OnStartServer() => NetworkManagerTFG.OnServerReadied += SpawnPlayer;

    [ServerCallback]
    private void OnDestroy() => NetworkManagerTFG.OnServerReadied -= SpawnPlayer;

    [Server]
    public void SpawnPlayer(NetworkConnection conn)
    {
        Transform sp = spawnPoints.ElementAtOrDefault(nextIndex);
        if (sp == null)
        {
            Debug.LogError($"Missing spawn point for player {nextIndex}");
            return;
        }

        GameObject playerInstance = Instantiate(playerPrefab, spawnPoints[nextIndex].position, Quaternion.identity);
        PlayerControl aux = playerInstance.GetComponent<PlayerControl>();

        NetworkServer.AddPlayerForConnection(conn, playerInstance);
        aux.SetTokenSyncvar("", Room.GetToken(conn));
        
        nextIndex++;
    }
}
