using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;

public class Timer : NetworkBehaviour
{
    public float timeRemaining = 10f; // Dos minutos
    public TMP_Text textoTiempo = null;

    private void Update()
    {
        timeRemaining -= Time.deltaTime;

        if(isServer)
        {
            if (timeRemaining <= 0f)
            {
                (NetworkManager.singleton as NetworkManagerTFG).EndCombat();
            }
        }
        else if (textoTiempo != null)
        {
            string sec = "";

            int minutos = ((int)timeRemaining) / 60;
            int segundos = ((int)timeRemaining) % 60;
            
            if (segundos < 10) sec = $"0{segundos}";
            else sec = $"{segundos}";

            textoTiempo.text = $"{minutos}:{sec}";
        }
    }
    

}
