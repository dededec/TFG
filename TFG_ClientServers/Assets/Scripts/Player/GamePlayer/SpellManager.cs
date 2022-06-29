using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEditor;

public class SpellManager : NetworkBehaviour
{
    [SerializeField] private PlayerControl _playerData;
    //----------------------------------------------------------------------
    [SerializeField] private ClientUI _clientUI;
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private Camera _cam;
    //----------------------------------------------------------------------
    [SerializeField] private List<Spell> _spells = new List<Spell>();
    [SerializeField] private GameObject _spellPrefab;
    //----------------------------------------------------------------------
    private Spell[] chosenSpells = new Spell[3];
    int selected = 0;
    int numSpellsChosen = 0;

    public Sprite groupSpell = null;
    //----------------------------------------------------------------------
    [SerializeField] int _mouseScrollSensibility = 0;
    int numUpScrolls = 0;
    int numDownScrolls = 0;
    //----------------------------------------------------------------------

    // Start is called before the first frame update
    void Start()
    {
        _spellPrefab.gameObject.SetActive(false);
    }

    public override void OnStartClient() => _clientUI.InitializeSpellDisplay(_spells);

    // Update is called once per frame
    [ClientCallback]
    void Update()
    {
        if (!isClient) return;

        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            ScrollDownSpell();
        }

        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            ScrollUpSpell();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            SelectSpell();
        }


        if (Input.GetMouseButtonDown(1)) // Hechizo simple
        {
            ShootSingle(_spells[selected].name);
        }
        else if (Input.GetMouseButtonDown(0)) // Hechizo complejo
        {
            ShootMix();
        }
    }

    [Client]
    private void ScrollDownSpell()
    {
        numDownScrolls++;
        if (numUpScrolls != 0) numUpScrolls = 0; // Para un cambio de sentido en el scroll
        if (numDownScrolls >= _mouseScrollSensibility) // Si superas la sensibilidad, cambias de hechizo selected
        {
            selected++;
            if (selected >= _spells.Count) selected = _spells.Count - 1;
            // selected = (selected + 1) % _spells.Count; // Esto es para que dé la vuelta
            numDownScrolls = 0;
        }

        _clientUI.UpdateSelectedSpell(selected);
    }

    [Client]
    private void ScrollUpSpell()
    {
        numUpScrolls++;
        if (numDownScrolls != 0) numDownScrolls = 0; // Para un cambio de sentido en el scroll
        if (numUpScrolls >= _mouseScrollSensibility) // Si superas la sensibilidad, cambias de hechizo selected
        {
            selected = (selected - 1) % _spells.Count;
            numUpScrolls = 0;
            // if(selected < 0) selected = _spells.Count -1; // Si quieres que dé la vuelta
            if (selected < 0) selected = 0;
        }

        _clientUI.UpdateSelectedSpell(selected);
    }

    [Client]
    private void SelectSpell()
    {
        if (numSpellsChosen >= 3) return;

        chosenSpells[numSpellsChosen] = _spells[selected];
        numSpellsChosen++;
        _clientUI.SelectSpell(selected);
    }

    [Client]
    private void ShootSingle(string spellName) // Si esto tira bien, cambiar Spell spell a string spellName
    {
        Vector3 direction = Input.mousePosition;
        direction = _cam.ScreenToWorldPoint(direction);
        direction.z = 0.0f; // La función de antes pone z a -10 (z de la cámara)
        direction = direction - _playerTransform.position;
        direction.Normalize();

        cmd_ShootSingle(spellName, _playerTransform.position, direction);
    }

    private Spell FindSpell(string name)
    {
        foreach (var spell in _spells)
        {
            if (spell.name == name)
            {
                return spell;
            }
        }
        return null;
    }

    [Command]
    private void cmd_ShootSingle(string spellName, Vector3 position, Vector3 direction)
    {
        // Validar input ...

        Spell spell = FindSpell(spellName);
        if (spell == null) return;
        if (spell.manaCost > _playerData.GetMana()) return;

        GameObject spellInstance = InstantiateSpell(spell, position, direction);
        _playerData.ReduceMana(spell.manaCost);

        NetworkServer.Spawn(spellInstance); // Spawnea un spellPrefab pero sin nada más
        spellInstance.GetComponent<SpellDisplay>().SetSpriteRPC(0, '#' + ColorUtility.ToHtmlStringRGBA(spell.colorElemental));
        // spellInstance.GetComponent<SpellBehaviour>().SetBehaviourRPC(spell.speed, spell.damage, direction); // No hace falta
    }

    private GameObject InstantiateSpell(Spell spellType, Vector3 position, Vector3 direction)
    {
        // Obviamente hay que averiguar que dispare según el hechizo y tal
        GameObject spell = (GameObject)Instantiate(_spellPrefab, position + direction * 1.15f, Quaternion.identity);

        spell.GetComponent<SpellDisplay>().SetSpell(spellType);
        spell.GetComponent<SpellBehaviour>().SetSpell(spellType);
        spell.GetComponent<SpellBehaviour>().SetDirection(direction);

        spell.GetComponent<SpellBehaviour>().SetToken(_playerData.Token);

        spell.SetActive(true);

        return spell;
    }

    [Client]
    private void ShootMix()
    {
        if (numSpellsChosen <= 0) return;

        if (numSpellsChosen == 1)
        {
            ShootSingle(chosenSpells[0].name);
            // Vacíamos todo para poder hacer grupo de nuevo 
            for (int i = 0; i < numSpellsChosen; ++i)
            {
                chosenSpells[i] = null;
            }
            _clientUI.EmptySlots();
            numSpellsChosen = 0;
        }
        else
        {
            Vector3 direction = Input.mousePosition;
            direction = _cam.ScreenToWorldPoint(direction);
            direction.z = 0.0f; // La función de antes pone z a -10 (z de la cámara)
            direction = direction - _playerTransform.position;
            direction.Normalize();

            string[] group = new string[numSpellsChosen];
            for (int i = 0; i < numSpellsChosen; ++i)
            {
                group[i] = chosenSpells[i].name;
            }

            cmd_ShootMix(group, _playerTransform.position, direction);
        }
    }

    [Command]
    private void cmd_ShootMix(string[] group, Vector3 position, Vector3 direction)
    {
        // Validar input ...
        Spell[] spellTypes = new Spell[group.Length];
        for (int i = 0; i < group.Length; ++i)
        {
            spellTypes[i] = FindSpell(group[i]);
        }

        int coste = spellTypes[0].manaCost + spellTypes[1].manaCost; // Calculamos coste antes
        if (group.Length == 3)
        {
            coste += (spellTypes[2].manaCost / 2);
        }

        if (coste > _playerData.GetMana()) return;

        GameObject spellInstance = InstantiateSpellGroup(spellTypes, position, direction);

        _playerData.ReduceMana(coste);
        _clientUI.EmptySlotsTargetRPC(connectionToClient);
        ResetChosenSpells(connectionToClient);

        NetworkServer.Spawn(spellInstance);
        spellInstance.GetComponent<SpellDisplay>().SetSpriteRPC(0, '#' + ColorUtility.ToHtmlStringRGBA(spellTypes[1].elementoSegundoPuesto));
    }

    [TargetRpc]
    private void ResetChosenSpells(NetworkConnection connection)
    {
        for (int i = 0; i < numSpellsChosen; ++i)
        {
            chosenSpells[i] = null;
        }
        numSpellsChosen = 0;
    }

    private GameObject InstantiateSpellGroup(Spell[] spellTypes, Vector3 position, Vector3 direction)
    {
        GameObject spell = (GameObject)Instantiate(_spellPrefab, position + direction * 1.15f, Quaternion.identity);

        SpellBehaviour sbehaviour = spell.GetComponent<SpellBehaviour>();
        SpellDisplay sdisplay = spell.GetComponent<SpellDisplay>();

        sbehaviour.SetDirection(direction);
        sbehaviour.SetSpeed((spellTypes[0].speed + spellTypes[1].speed) / 2);
        sbehaviour.SetDamage(spellTypes[0].damage + (spellTypes[1].damage / 2));
        sbehaviour.SetToken(_playerData.Token);

        /*
        FALTA:
        - La animación del primer puesto
        */
        sdisplay._renderer.sprite = groupSpell;
        sdisplay._renderer.color = spellTypes[1].elementoSegundoPuesto;

        if (spellTypes.Length == 3)
        {
            switch (spellTypes[2].funcionTercerPuesto)
            {
                case 0:
                    fasterSpell(sbehaviour);
                    break;
                case 1:
                    strongerSpell(sbehaviour);
                    break;
                default:
                    defaultEffect(sbehaviour);
                    break;
            }
        }

        spell.SetActive(true);

        return spell;
    }

    /***********************************************************************/
    /*
    FUNCIONES DE HECHIZO EN TERCER PUESTO 
    */
    /***********************************************************************/

    // TercerPuesto == 0
    private void fasterSpell(SpellBehaviour spellBehaviour)
    {
        spellBehaviour.SetSpeed((int)Mathf.Ceil((float)(spellBehaviour.GetSpeed()) * 1.25f));
    }

    // TercerPuesto == 1
    private void strongerSpell(SpellBehaviour spellBehaviour)
    {
        spellBehaviour.SetDamage((int)Mathf.Ceil((float)(spellBehaviour.GetDamage()) * 1.25f));
    }

    private void defaultEffect(SpellBehaviour spellBehaviour)
    {
        Debug.Log("No hay efecto");
    }
}
