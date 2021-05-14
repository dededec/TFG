using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Movement : NetworkBehaviour
{
    [SerializeField] private float _playerSpeed = 10f;
    [SerializeField] private Rigidbody2D _playerRigidbody = null;
    [SerializeField] private Transform playerTransform = null;
    [SerializeField] private Animator animator = null;

    [Client]
    private void FixedUpdate()
    {

        if (!hasAuthority) return;

        float vertical = Input.GetAxis("Vertical");
        float horizontal = Input.GetAxis("Horizontal");

        
        animator.SetBool("IsWalking", Mathf.Abs(vertical) > 0.1f || Mathf.Abs(horizontal) > 0.1f);

        /*
        Esto lo quito, porque de normal los jugadores se van a mover constantemente, con lo que se hace un poco inútil.
        Además, quita un caso especial en el que si un jugador entra, los otros jugadores le aparecerán en el 0,0 hasta que
        se muevan.

        if(vertical + horizontal == 0f) return; //Si no lo estamos moviendo no llamamos a nada (menos tráfico, aunque tampoco mucho)
        */

        cmd_Move(vertical, horizontal);
    }

    /*
    Al usar position en el comando y RPC, lo que hacemos es que pasamos la posición donde deberia estar en los
    otros clientes, evitando el problema de que _playerRigidbody.position sea 0 porque en el servidor es así.
    */
    [Command]
    private void cmd_Move(float v, float h)
    {
        // Aqui validas las cositas y tal
        // Deberias validar aqui que la persona es quien debe de ser y no otra
        // 10:50 - https://www.youtube.com/watch?v=oBRt9OifJvE&list=PLS6sInD7ThM1aUDj8lZrF4b4lpvejB2uB&index=2&ab_channel=DapperDino
        
        Move(v, h); // Con NetworkTransform
    }

    private void Move(float vertical, float horizontal)
    {
        // Animacion
        Vector3 scale = Vector3.one;
        scale.x = Mathf.Sign(horizontal);
        playerTransform.localScale = scale;

        // Movimiento
        _playerRigidbody.MovePosition( _playerRigidbody.position 
                                    + (Vector2.up * vertical * _playerSpeed * Time.deltaTime)
                                    + (Vector2.right * horizontal * _playerSpeed * Time.deltaTime)
                                    );

    }


}
