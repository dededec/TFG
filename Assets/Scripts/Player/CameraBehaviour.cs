using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Lo de la cámara se hace todo en el cliente
// por eso es MonoBehaviour y no usa nada de Mirror
// (Cuidado con trampas -> Mirar a otros jugadores?)

/*
Creo que como la camara es hija de un objeto NetworkBehaviour,
obligatoriamente el servidor se encarga de ella, por lo que
actua como un objeto del servidor y no de cada cliente que es
lo que buscamos.

Para solucionarlo, la pondré con NetworkBehaviour en vez de MonoBehaviour
y especificaré que el control de cámaras se realiza en el cliente.
*/

/*
En PlayerPrefab, la cámara no es hija del jugador, por lo que no hay
problemas con el servidor.
Además, desde el jugador nos encargamos de la activación de cámaras en el
cliente, con lo que este script solo se encarga del comportamiento en sí y nada más.
*/

public class CameraBehaviour : MonoBehaviour
{
    public Transform playerTransform;
    private float radioCircunferencia = 6.0f;
    private float lerpT = 0.25f;

    public bool basic = true;

    // Update is called once per frame
    void Update()
    {
        if(basic) transform.position = playerTransform.position + Vector3.back*10f;
        else {
            transform.position = Vector3.Lerp(transform.position, playerTransform.position, 0.75f);

            Vector3 mouse = Input.mousePosition;
            mouse = this.GetComponent<Camera>().ScreenToWorldPoint(mouse);
            mouse = mouse - playerTransform.position;

            mouse.Normalize();
            mouse *= radioCircunferencia;
            mouse.z = -10.0f; // La función de antes pone z a -10 (z de la cámara)
            transform.position = Vector3.Lerp(transform.position, mouse, lerpT);
        }
    }
}






















