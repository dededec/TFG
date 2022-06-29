using Mirror;
using UnityEngine;

public class PlayerControl : NetworkBehaviour
{
    [SyncVar(hook = nameof(SetTokenSyncvar))]
    [SerializeField] private string token = null; // Aqui un syncvar lo mismo pega
    public string Token => token;
    public void SetTokenSyncvar(string oldToken, string newToken) => token = newToken;

    private int _maxHealth = 100;
    [SerializeField] private int _health = 100;

    // -------------------------------------------------

    private int _maxMana = 100;
    [SerializeField] private int _mana = 100;
    private int _manaIncrease = 10;

    // -------------------------------------------------

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
    public override void OnStartClient()
    {
        if (isLocalPlayer)
        {
            GameObject.FindGameObjectWithTag("MainTimer").GetComponent<Timer>().textoTiempo = _clientUI.textoTiempo;
            return;
        }

        // Pongo un GameObject para encargarme de los AudioListeners también
        _cam.SetActive(false);

        // Esto no sería necesario en un principio por el hasAuthority de FixedUpdate
        // Pero lo dejo porque es seguridad de más
        _movement.enabled = false;
        _clientUI.DisableAll();
        _clientUI.enabled = false;
        _spellManager.enabled = false;
    }

    public int GetHealth() => _health;
    
    public int GetMaxHealth() => _maxHealth;

    [Server]
    public void DamagePlayer(int damage)
    {
        _health -= damage;
        if (_health < 0)
        {
            _health = 0;
        }

        _clientUI.UpdateHealthUITargetRPC(connectionToClient, _health);
    }

    [Server]
    public void HealPlayer(int amount)
    {
        _health += amount;
        if (_health > _maxHealth) _health = _maxHealth;

        _clientUI.UpdateHealthUITargetRPC(connectionToClient, _health);
    }

    public int GetMana() => _mana;

    public int GetMaxMana() => _maxMana;

    [Server]
    public void ReduceMana(int amount)
    {
        _mana -= amount;
        if (_mana < 0) _mana = 0;

        _clientUI.UpdateManaUITargetRPC(connectionToClient, _mana);
    }

    [Server]
    public void IncreaseMana(int amount)
    {
        _mana += amount;
        if (_mana > _maxMana) _mana = _maxMana;

        _clientUI.UpdateManaUITargetRPC(connectionToClient, _mana);
    }

    public void ActivateCam() => _cam.SetActive(true);

    public void PlayerDead(string enemyToken)
    {
        print("Muere un jugador");
        if (string.IsNullOrEmpty(enemyToken))
        {
            print("enemyToken es null o vacio");
            return;
        }

        // ActivateEnemyCam(connectionToClient, enemyToken); // Esto daba problemas porque chocaba con el rpc que desactiva el jugador.
        Room.PlayerDead();

        // Aqui desactivarias cositas (SetChildren(false)??)
        rpc_PlayerDead(enemyToken, Room.GetDeadPlayers(), Room.maxPlayers);
    }

    private void ActivateEnemyCam(string enemyToken)
    {
        foreach (var player in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (player.gameObject.GetComponent<PlayerControl>().Token == enemyToken)
            {
                player.GetComponent<PlayerControl>().ActivateCam();
                break;
            }
        }
    }

    [ClientRpc]
    private void rpc_PlayerDead(string enemyToken, int deadPlayers, int maxPlayers)
    {
        if (hasAuthority && deadPlayers < maxPlayers-1) // Para que no lo haga si se va a acabar la pelea (no ha dado problemas sin esto, pero por asegurarse)
        {
            ActivateEnemyCam(enemyToken);
        }
        this.gameObject.SetActive(false);
    }
}
