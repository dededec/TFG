using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollision : MonoBehaviour
{

    [SerializeField] private PlayerControl _playerData;
    private int _parentID;

    void Start() {
        _parentID = this.transform.parent.gameObject.GetInstanceID();
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if(other.transform.tag == "Spell") {
            SpellBehaviour sp = other.gameObject.GetComponent<SpellBehaviour>();
            
            if(_parentID != sp.GetPlayerID()) { // Comparamos ID del transform del jugador que lo lanzó con el ID del transform de este
                _playerData.DamagePlayer(sp.GetDamage());
                if(_playerData.GetHealth() <= 0) _playerData.PlayerDead(sp.GetPlayerID());
            }
        }
    }
}
