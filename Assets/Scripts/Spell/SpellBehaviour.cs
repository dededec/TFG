using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Todo lo referente a la lógica del hechizo
public class SpellBehaviour : MonoBehaviour
{
    private Spell _spell;

    [SerializeField] private Rigidbody2D rb;
    private Vector2 _direction;
    private int _playerID; // Esto identifica al SpellManager que instancia el hechizo (y por tanto a su lanzador)

    [SerializeField] private float _speed;
    [SerializeField] private int _damage;

    private float _destroyTime = 5f;


    void Start() {
        if(_spell != null) {
            _speed = _spell.speed;
            _damage = _spell.damage;
        }
        
        rb.velocity = _direction * _speed;
        
        Destroy(this.gameObject, _destroyTime);
    }

    public void SetSpell(Spell spell) {
        _spell = spell;
    }

    public void SetDirection(Vector2 dir) {
        _direction = dir;
    }

    public void SetPlayerID(int id) {
        _playerID = id;
    }

    public int GetPlayerID() {
        return _playerID;
    }

    public int GetDamage() {
        return _damage;
    }

    public void SetDamage(int newDamage) {
        _damage = newDamage;
    }

    public float GetSpeed() {
        return _speed;
    }

    public void SetSpeed(float newSpeed) {
        _speed = newSpeed;
    }


    private void OnTriggerEnter2D(Collider2D other) {
        if(other.tag == "Player") { //Si es un jugador
            if(other.gameObject.transform.parent.gameObject.GetInstanceID() != _playerID) DestroySpell(); // Si no es el que lo mandó
        }
        else if(other.tag == "Spell") {
            if(other.gameObject.GetComponent<SpellBehaviour>().GetPlayerID() != _playerID) DestroySpell(); // Si no es el que lo mandó
        }
        else DestroySpell();
    }

    private void DestroySpell(){
        //Parafernalia del impacto
        Destroy(this.gameObject);
    }

}


