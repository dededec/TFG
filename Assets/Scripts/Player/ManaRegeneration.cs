using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ManaRegeneration : NetworkBehaviour
{

    // Start is called before the first frame update
    [Server]
    void Start() {
        StartCoroutine(RegenerateMana());
    }
    
    [Server]
    private IEnumerator RegenerateMana() {
        while(true) {
            yield return new WaitForSeconds(0f);
            rpc_ManaRegeneration();
        }
    }

    [Command]
    private void cmd_RegenerateMana() {
        //...
        rpc_ManaRegeneration();
    }

    [ClientRpc]
    private void rpc_ManaRegeneration() {
        if(isLocalPlayer) {
            
        }
    }
}
