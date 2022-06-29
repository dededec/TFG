using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class MenuButtons : MonoBehaviour
{
    public Button LoginButton;
    public Button RegisterButton;

    private NetworkManagerTFG room;
    private NetworkManagerTFG Room
    {
        get
        {
            if (room != null) { return room; }
            return room = NetworkManager.singleton as NetworkManagerTFG;
        }
    }

    public void OnClickLoginButton() => Room.ChangeUIActual("UI_Login");
    public void OnClickRegisterButton() => Room.ChangeUIActual("UI_Register");
    
    // Start is called before the first frame update
    void Start()
    {
        
    }
}
