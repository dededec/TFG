using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Mirror;

public class JoinLobby : MonoBehaviour
{
    [SerializeField] private NetworkManagerTFG networkManager = null;
    

    [Header("UI")]
    [SerializeField] private Button joinButton = null;
    [SerializeField] private GameObject botones = null;

    private void Start()
    {
        networkManager = GameObject.FindGameObjectWithTag("NetworkManager").GetComponent<NetworkManagerTFG>();
        if(networkManager == null)
        {
            Debug.LogError("JoinLobby could not find networkManager.");
        }
    }

    public void Join()
    {
        networkManager.networkAddress = GlobalVariables.matchmakingServerIP;
        networkManager.StartClient();
        joinButton.interactable = false;
    }

    private void OnEnable()
    {
        NetworkManagerTFG.OnClientConnected += HandleClientConnected;
        NetworkManagerTFG.OnClientDisconnected += HandleClientDisconnected;
    }
    
    private void OnDisable()
    {
        NetworkManagerTFG.OnClientConnected -= HandleClientConnected;
        NetworkManagerTFG.OnClientDisconnected -= HandleClientDisconnected;
    }
    
    private void HandleClientConnected()
    {
        joinButton.interactable = true; // Esto es por si vuelves luego a la parte de unirse, taría feo que el botón no estuviese activado
        botones.SetActive(false);
    }
    
    private void HandleClientDisconnected()
    {
        botones.SetActive(true);
        joinButton.interactable = true;
    }
}
