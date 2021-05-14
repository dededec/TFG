using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.UI;

public class ClientAddPlayer : NetworkBehaviour
{
    private NetworkManagerTFG networkManager;
    public Button Boton;

    private void Start() 
    {
        Boton.onClick.AddListener(delegate() { ClientScene.AddPlayer(connectionToServer); });
    }

}