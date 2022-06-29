using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using MessagesTFG;
using TMPro;

public class SendRegister : MonoBehaviour
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

    [SerializeField] private Button botonRegister;
    [SerializeField] private TMP_InputField usernameField;
    [SerializeField] private TMP_InputField passwordField;
    [SerializeField] private TMP_InputField repeatPasswordField;



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

    public void StartRegister()
    {
        if (usernameField.text.Length == 0 || passwordField.text.Length == 0)
        {
            Room.SetDebugText("Falta username o password", disappear: true);
            return;
        }
        else if (passwordField.text != repeatPasswordField.text)
        {
            Room.SetDebugText("Las contraseñas no coinciden", disappear: true);
            return;
        }

        botonRegister.interactable = false;
        Room.Register(usernameField.text, passwordField.text);
    }

    public void BackButton() => Room.ChangeUIActual("UI_Menu");

    private void HandleClientDisconnected()
    {
        botonRegister.interactable = true;
    }

    private void HandleClientConnected()
    {
        botonRegister.interactable = true;
    }
}
