using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using MessagesTFG;

public class LobbyServer : MonoBehaviour {

    [SerializeField] private string _lobbyScene = "Lobby";
    [SerializeField] private NetworkManagerTFG networkManager = null;
    
    [Header("UI")]
    [SerializeField] private GameObject landingPagePanel = null;

    private void Start()
    {
        networkManager = GameObject.FindGameObjectWithTag("NetworkManager").GetComponent<NetworkManagerTFG>();
        if(networkManager == null)
        {
            Debug.LogError("LobbyServer could not find networkManager.");
        }
    }
    
    public void BecomeServer()
    {
        networkManager.StartServer();
        networkManager.ServerChangeScene(_lobbyScene);
    }
}
