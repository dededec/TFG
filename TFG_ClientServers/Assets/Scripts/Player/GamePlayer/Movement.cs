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
    [SerializeField] private Vector3 limite;
    public Collider2D cc;

    [ClientCallback]
    private void FixedUpdate()
    {
        if (!isClient) return;
        
        if (!hasAuthority) return;

        float vertical = Input.GetAxis("Vertical");
        float horizontal = Input.GetAxis("Horizontal");


        animator.SetBool("IsWalking", Mathf.Abs(vertical) > 0.1f || Mathf.Abs(horizontal) > 0.1f);

        cmd_Move(vertical, horizontal);
    }

    /*
    Al usar position en el comando y RPC, lo que hacemos es que pasamos la posición donde deberia estar en los
    otros clientes, evitando el problema de que _playerRigidbody.position sea 0 porque en el servidor es así.
    */
    [Command]
    private void cmd_Move(float v, float h)
    {
        
        Move(v, h); // Con NetworkTransform
    }

    private void Move(float vertical, float horizontal)
    {
        // Animacion
        Vector3 scale = Vector3.one;
        scale.x = Mathf.Sign(horizontal);
        playerTransform.localScale = scale;


        // Movimiento
        _playerRigidbody.MovePosition(_playerRigidbody.position
                                    + (Vector2.up * vertical * _playerSpeed * Time.deltaTime)
                                    + (Vector2.right * horizontal * _playerSpeed * Time.deltaTime)
                                    );

    }


}
