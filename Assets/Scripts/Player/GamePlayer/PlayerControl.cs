using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Assertions;

/*
Podríamos hacer este script como el de inicio
Y de aquí manejar cámaras y movimiento
Y este se queda como el script activo ya que 
tenemos los datos del jugador

*/
public class PlayerControl : NetworkBehaviour
{
    private int _maxHealth = 100;
    [SerializeField] private int _health = 100;
    private int _maxMana = 100;
    [SerializeField] private int _mana = 100;
    private int _manaIncrease = 10;

    private ClasificationManager _clasification = null;
    private NetworkManagerTFG room;
    private NetworkManagerTFG Room
    {
        get
        {
            if (room != null) { return room; }
            return room = NetworkManager.singleton as NetworkManagerTFG;
        }
    }

    [SerializeField] private GameObject _cam;
    [SerializeField] private Movement _movement;
    [SerializeField] private ClientUI _clientUI;
    [SerializeField] private SpellManager _spellManager;

    // Start is called before the first frame update
    void Start()
    {
        // Fuente: https://forum.unity.com/threads/multiple-cameras.342006/
        // Basicamente, desde el jugador, desactivamos las cámaras que no sean las nuestras
        // Cuando entras, en tu jugador se ejecuta esto y se sale
        // Y luego se ejecuta una vez por cada jugador, y las desactiva en tu cliente.

        // IF I'M THE PLAYER, STOP HERE (DON'T TURN MY OWN CAMERA OFF)
        if (isLocalPlayer){
            StartCoroutine(RegenerateMana());
            return;
        }

 
        // DISABLE CAMERA AND CONTROLS HERE (BECAUSE THEY ARE NOT ME)
        // Pongo un GameObject para encargarme de los AudioListeners también
        _cam.SetActive(false);

        // Esto no sería necesario en un principio por el hasAuthority de FixedUpdate
        // Pero lo dejo porque es seguridad de más
        _movement.enabled = false;
        _clientUI.DisableAll();
        _clientUI.enabled = false;
        _spellManager.enabled=false;
    }


    // Devuelve la salud actual del jugador
    public int GetHealth(){ return _health; }

    // Devuelve la salud máxima del jugador
    public int GetMaxHealth(){ return _maxHealth; }

    public void DamagePlayer(int damage) {
        _health -= damage;
        if(_health < 0) _health = 0;
        _clientUI.UpdateHealthUI();
    }
    public void HealPlayer(int amount) {
        _health += amount;
        if(_health > _maxHealth) _health = _maxHealth;
        _clientUI.UpdateHealthUI();
    }


    // Devuelve el maná actual del jugador
    public int GetMana() { return _mana; }

    // Devuelve el maná máximo del jugador
    public int GetMaxMana() {return _maxMana; }

    public void ReduceMana(int amount) {
        _mana -= amount;
        if(_mana < 0) _mana = 0;
        _clientUI.UpdateManaUI();
    }
    public void IncreaseMana(int amount) {
        _mana += amount;
        if(_mana > _maxMana) _mana = _maxMana;
        _clientUI.UpdateManaUI();
    }
    
    [Client]
    private IEnumerator RegenerateMana() {
        while(true) {
            yield return new WaitForSeconds(1f);
            cmd_RegenerateMana();
        }
    }

    [Command]
    private void cmd_RegenerateMana() {
        //...
        rpc_RegenerateMana();
    }

    [ClientRpc]
    private void rpc_RegenerateMana() {
        if(isLocalPlayer) {
            IncreaseMana(_manaIncrease);
        }
    }


    public void ActivateCam() => _cam.SetActive(true);

    public void PlayerDead(int playerID) {
        if(!hasAuthority) return;
        
        foreach(var player in GameObject.FindGameObjectsWithTag("Player")){
            if(player.gameObject.GetInstanceID() == playerID) {
                player.GetComponent<PlayerControl>().ActivateCam();
                break;
            }
        }

        cmd_PlayerDead();
    }

    [Command]
    private void cmd_PlayerDead() {

        // Aqui hacemos lo de la clasificacion para que se cambie en el server y no en el cliente

        rpc_PlayerDead();
    }

    [ClientRpc]
    private void rpc_PlayerDead() {
        this.gameObject.SetActive(false);
    }

}
