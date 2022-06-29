using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

//Todo lo referente a la lógica del hechizo
public class SpellBehaviour : NetworkBehaviour
{
    [SerializeField] private Spell _spell;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Vector2 _direction;
    [SerializeField] private string token; // Esto identifica al SpellManager que instancia el hechizo (y por tanto a su lanzador)

    [SerializeField] private float _speed;
    [SerializeField] private int _damage;

    private float _destroyTime = 5f;


    public override void OnStartServer()
    {
        if (_spell != null)
        {
            _speed = _spell.speed;
            _damage = _spell.damage;
        }
        rb.velocity = _direction * _speed;

        StartCoroutine(NetworkServerDestroy(this.gameObject, _destroyTime));
    }   

    public override void OnStartClient()
    {
        rb.isKinematic = true;
    }


    private IEnumerator NetworkServerDestroy(GameObject obj, float destroyTime)
    {
        yield return new WaitForSeconds(destroyTime);
        NetworkServer.Destroy(obj);
    }

    public void SetSpell(Spell spell) => _spell = spell;

    public void SetDirection(Vector2 dir) => _direction = dir;

    public void SetToken(string tk) => token = tk;

    public string GetToken() => token;

    public int GetDamage() => _damage;

    public void SetDamage(int newDamage) => _damage = newDamage;

    public float GetSpeed() => _speed;

    public void SetSpeed(float newSpeed) => _speed = newSpeed;

    [Server]
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isClient) return; // Esto lo hace el server

        if (other.tag == "Player") // Si es un jugador -> El Collider2D está en un HIJO y no en la raíz
        {
            if (other.transform.parent.GetComponent<PlayerControl>().Token != token)
            {
                DestroySpell(); // Si no es el que lo mandó
            }
            else
            {
                print("Los token del jugador y spell coinciden: " + token);
                return;
            }
        }
        else if (other.tag == "Spell") // Aquí el Collider si está en la raiz, no hay problema
        {
            if (other.gameObject.GetComponent<SpellBehaviour>().GetToken() != token)
            {
                DestroySpell(); // Si no es del que lo mandó
            }
            else
            {
                print("Los token de los spell coinciden: " + token);
                return;
            }
        }
        else DestroySpell();
    }

    private void DestroySpell()
    {
        //Parafernalia del impacto
        NetworkServer.Destroy(this.gameObject);
    }

}


