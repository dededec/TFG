using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerCollision : MonoBehaviour
{

    [SerializeField] private PlayerControl _playerData;
    private string token;

    public void Start()
    {
        token = _playerData.Token;
    }

    [Server]
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.transform.tag == "Spell")
        {
            SpellBehaviour spell = other.gameObject.GetComponent<SpellBehaviour>();

            if (token != spell.GetToken()) // Comparamos token del jugador que lo lanzó con el token de este
            { 
                _playerData.DamagePlayer(spell.GetDamage());
                if (_playerData.GetHealth() <= 0) 
                {
                    _playerData.PlayerDead(spell.GetToken());
                }
            }
        }
    }
}
