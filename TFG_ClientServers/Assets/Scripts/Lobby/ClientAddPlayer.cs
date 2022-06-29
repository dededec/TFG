using System.Collections;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using MessagesTFG;

public class ClientAddPlayer : MonoBehaviour
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

    public Button Boton;
    public Text debugText;

    private void Start()
    {
        Boton.onClick.AddListener(delegate ()
        {
            if (NetworkClient.AddPlayer())
            {
                Room.SetDebugText("Esperando al resto de jugadores...", disappear: false);
                Boton.interactable = false;
            }
            else
            {
                Room.SetDebugText("Error al enviar mensaje al servidor", disappear: true);
                Boton.interactable = true;
            }
        }
        );
    }

    public void func()
    {
        if (NetworkClient.AddPlayer())
        {
            // Room.SetDebugText("Esperando al resto de jugadores...", disappear: false);
            Boton.interactable = false;
        }
        else
        {
            // Room.SetDebugText("Error al enviar mensaje al servidor", disappear: true);
            Boton.interactable = true;
        }
    }

}