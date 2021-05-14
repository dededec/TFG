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
                (NetworkManager.singleton as NetworkManagerTFG).EndGame();
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

    // ---------------- USANDO SYNCVAR ----------------

    // [SyncVar(hook = nameof(SyncTimeRemaining))]
    // public float timeRemaining = 120f; // Dos minutos

    // public TMP_Text textoTiempo = null;

    // private void Update()
    // {
    //     if(isServer)
    //     {
    //         ServerUpdateTime();
    //     }
    // }

    // public void ServerUpdateTime()
    // {
    //     SyncTimeRemaining(this.timeRemaining, timeRemaining - Time.deltaTime);
    // }

    // public override void OnStartClient()
    // {
    //     SyncTimeRemaining(this.timeRemaining, this.timeRemaining);
    //     base.OnStartClient();
    // }

    // private void SyncTimeRemaining(float oldTime, float newTime)
    // {
    //     timeRemaining = newTime;
    //     if (textoTiempo != null)
    //     {
    //         int minutos = ((int)timeRemaining) / 60;
    //         int segundos = ((int)timeRemaining) % 60;
    //         if (minutos == 0 && segundos < 10) textoTiempo.text = $"0:0{segundos}";
    //         else textoTiempo.text = $"{minutos}:{segundos}";
    //     }
    // }
