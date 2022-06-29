using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CameraBehaviour : NetworkBehaviour
{
    public Camera playerCamera;
    public Transform playerTransform;
    private float radioCircunferencia = 6.0f;
    private float lerpT = 0.25f;

    public bool basic = true;

    // Update is called once per frame
    [ClientCallback]
    void Update()
    {
        if(!isClient) return;
        
        if (basic) playerCamera.transform.position = playerTransform.position + Vector3.back * 10f;
        else
        {
            playerCamera.transform.position = Vector3.Lerp(playerCamera.transform.position, playerTransform.position, 0.75f);

            Vector3 mouse = Input.mousePosition;
            mouse = playerCamera.ScreenToWorldPoint(mouse);
            mouse = mouse - playerTransform.position;

            mouse.Normalize();
            mouse *= radioCircunferencia;
            mouse.z = -10.0f; // La función de antes pone z a -10 (z de la cámara)
            playerCamera.transform.position = Vector3.Lerp(playerCamera.transform.position, mouse, lerpT);
        }
    }
}






















