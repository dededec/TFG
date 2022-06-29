using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PruebaNTDos : NetworkBehaviour
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


    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Movimiento());
    }

    private IEnumerator Movimiento()
    {
        yield return new WaitForSeconds(5.0f);
        float timeElapsed = 0.0f;
        //Moverse durante 5 segundos
        while(timeElapsed < 5.0f)
        {
            //Muevete
            timeElapsed += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

    }


}
