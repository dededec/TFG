using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using MessagesTFG;
using TMPro;

public class SendLogin : MonoBehaviour
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

    [SerializeField] private Button botonLogin;
    [SerializeField] private TMP_InputField usernameField;
    [SerializeField] private TMP_InputField passwordField;
    [SerializeField] private Text debugText;

    private void OnEnable()
    {
        NetworkManagerTFG.OnClientDisconnected += HandleClientDisconnected;
        NetworkManagerTFG.OnClientConnected += HandleClientConnected;
    }

    private void OnDisable()
    {
        NetworkManagerTFG.OnClientConnected -= HandleClientConnected;
        NetworkManagerTFG.OnClientDisconnected -= HandleClientDisconnected;
    }

    public void StartLogin()
    {
        if (usernameField.text.Length <= 0 || passwordField.text.Length <= 0)
        {
            debugText.text = "Error: usuario o contraseña no introducidos";
            return;
        }
        
        botonLogin.interactable = false;
        // debugText.text = "Logeando...";
        Room.ChangeUIActual("UI_Cargando");
        Room.Login(usernameField.text, passwordField.text, debugText);
    }

    public void BackButton() => Room.ChangeUIActual("UI_Menu");

    private void HandleClientDisconnected() 
    {
        botonLogin.interactable = true;
    }
    private void HandleClientConnected() => botonLogin.interactable = true;

}
