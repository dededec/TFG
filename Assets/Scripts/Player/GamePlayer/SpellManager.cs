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
    [Client]
    void Start()
    {
        _spellPrefab.gameObject.SetActive(false);
        _clientUI.InitializeSpellDisplay(_spells);  
    }

    // Update is called once per frame
    [Client]
    void Update()
    {
        if(Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            ScrollDownSpell();
        }

        if(Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            ScrollUpSpell();
        }

        if(Input.GetKeyDown(KeyCode.Space))
        {
            SelectSpell();
        }

        
        if(Input.GetMouseButtonDown(1)) { // Hechizo simple
            ShootSingle(_spells[selected]);
        }
        else if(Input.GetMouseButtonDown(0)) { // Hechizo complejo
            ShootMix();
        }
    }

    [Client]
    private void ScrollDownSpell()
    {
        numDownScrolls++;
        if(numUpScrolls != 0) numUpScrolls = 0; // Para un cambio de sentido en el scroll
        if(numDownScrolls >= _mouseScrollSensibility) // Si superas la sensibilidad, cambias de hechizo selected
        {
            selected++;
            if(selected >= _spells.Count) selected = _spells.Count -1;
            // selected = (selected + 1) % _spells.Count; // Esto es para que dé la vuelta
            numDownScrolls = 0;
        }
        
        _clientUI.UpdateSelectedSpell(selected);
    }

    [Client]
    private void ScrollUpSpell()
    {
        numUpScrolls++;
        if(numDownScrolls != 0) numDownScrolls = 0; // Para un cambio de sentido en el scroll
        if(numUpScrolls >= _mouseScrollSensibility) // Si superas la sensibilidad, cambias de hechizo selected
        {
            selected = (selected - 1) % _spells.Count;
            numUpScrolls = 0;
            // if(selected < 0) selected = _spells.Count -1; // Si quieres que dé la vuelta
            if(selected < 0) selected = 0;
        }

        _clientUI.UpdateSelectedSpell(selected);
    }

    [Client]
    private void SelectSpell()
    {
        if(numSpellsChosen >= 3) return;

        chosenSpells[numSpellsChosen] = _spells[selected];
        numSpellsChosen++;
        _clientUI.SelectSpell(selected);
    }

    private string SpellPath(Spell spell) {
        return "Spells/spell_" + spell.name;
        
    }

    [Client]
    private void ShootSingle(Spell spell) {
        if(spell.manaCost > _playerData.GetMana()) return;

        Vector3 direction = Input.mousePosition;
        direction = _cam.ScreenToWorldPoint(direction);
        direction.z = 0.0f; // La función de antes pone z a -10 (z de la cámara)
        direction = direction - _playerTransform.position;
        direction.Normalize();

        cmd_ShootSingle(SpellPath(spell), _playerTransform.position, direction);
    }

    [Command]
    private void cmd_ShootSingle(string spellPath, Vector3 position, Vector3 direction){
        // Validar input ...
        rpc_ShootSingle(spellPath, position, direction);
    }

    [ClientRpc]
    private void rpc_ShootSingle(string spellPath, Vector3 position, Vector3 direction) {
        Spell spell = Resources.Load<Spell>(spellPath); // Warro y tal vez muy costoso a la larga
        InstantiateSpell(spell, position, direction); //_spellToInstantiate es null en los otros clientes y los echa
        _playerData.ReduceMana(spell.manaCost);
    }

    /*
    ESTO NO PUEDE SER [SERVER]
    FORMA PARTE DE UNA LLAMADA RPC
    ES DECIR, QUE EL SERVIDOR LE DICE A CADA CLIENTE:
    "EH TU, EJECUTA ESTO" Y LO OTROS LO HACEN
    EL SERVIDOR NO LO EJECUTA, SOLO PASA LA ORDEN
    */
    private void InstantiateSpell(Spell spellType, Vector3 position, Vector3 direction)
    {
        // Obviamente hay que averiguar que dispare según el hechizo y tal
        GameObject spell = (GameObject) Instantiate(_spellPrefab, position + direction * 1.15f, Quaternion.identity);
        
        spell.GetComponent<SpellDisplay>().SetSpell(spellType);
        spell.GetComponent<SpellBehaviour>().SetSpell(spellType);
        spell.GetComponent<SpellBehaviour>().SetDirection(direction);

        spell.GetComponent<SpellBehaviour>().SetPlayerID(this.gameObject.GetInstanceID());

        spell.SetActive(true);
    }

    private void ShootMix()
    {

        if(numSpellsChosen <= 0) return;

        if(numSpellsChosen == 1) {
            ShootSingle(chosenSpells[0]);
            // Vacíamos todo para poder hacer grupo de nuevo 
            for(int i=0; i<numSpellsChosen; ++i)
            {
                chosenSpells[i] = null;
            }
            _clientUI.EmptySlots();
            numSpellsChosen = 0;
        }
        else if (numSpellsChosen == 2) { 
            if(chosenSpells[0].manaCost + chosenSpells[1].manaCost > _playerData.GetMana()) return;

            Vector3 direction = Input.mousePosition;
            direction = _cam.ScreenToWorldPoint(direction);
            direction.z = 0.0f; // La función de antes pone z a -10 (z de la cámara)
            direction = direction - _playerTransform.position;
            direction.Normalize();

            string[] group = new string[] {SpellPath(chosenSpells[0]), SpellPath(chosenSpells[1])};

            cmd_ShootMix(group, _playerTransform.position, direction);
        }
        else {
            if(chosenSpells[0].manaCost + chosenSpells[1].manaCost + chosenSpells[2].manaCost > _playerData.GetMana()) return;

            Vector3 direction = Input.mousePosition;
            direction = _cam.ScreenToWorldPoint(direction);
            direction.z = 0.0f; // La función de antes pone z a -10 (z de la cámara)
            direction = direction - _playerTransform.position;
            direction.Normalize();

            string[] group = new string[] {SpellPath(chosenSpells[0]), SpellPath(chosenSpells[1]), SpellPath(chosenSpells[2])};

            cmd_ShootMix(group, _playerTransform.position, direction);
        }
    }

    [Command]
    private void cmd_ShootMix(string[] group, Vector3 position, Vector3 direction){
        // Validar input ...
        rpc_ShootMix(group, position, direction);
    }

    [ClientRpc]
    private void rpc_ShootMix(string[] group, Vector3 position, Vector3 direction) {
        InstantiateSpellGroup(group, position, direction);
    }

    private void InstantiateSpellGroup(string[] group, Vector3 position, Vector3 direction)
    {
        // Si entras aquí, ya sabes que hay MINIMO 2 hechizos en el grupo
        Spell[] spellTypes = new Spell[group.Length];
        for(int i=0; i<group.Length; ++i) {
            spellTypes[i] = Resources.Load<Spell>(group[i]);
        }

        // Obviamente hay que averiguar que dispare según el hechizo y tal
        GameObject spell = (GameObject) Instantiate(_spellPrefab, position + direction * 1.15f, Quaternion.identity);
        
        SpellBehaviour sbehaviour = spell.GetComponent<SpellBehaviour>();
        SpellDisplay sdisplay = spell.GetComponent<SpellDisplay>();

        // Esto lo hago por los jajas, es para que no de error
        // No hago SetSpell porque no me deja cambiar cosas luegos, lo pongo todo aquí manualmente
        
        sbehaviour.SetDirection(direction);
        sbehaviour.SetSpeed(spellTypes[1].speed);
        sbehaviour.SetDamage(spellTypes[1].damage);
        sbehaviour.SetPlayerID(this.gameObject.GetInstanceID());

        /*
        FALTA:
        - La animación del primer puesto
        */
        sdisplay._renderer.sprite = groupSpell;
        sdisplay._renderer.color = spellTypes[1].elementoSegundoPuesto; // funciona


        if(spellTypes[2] != null) {
            switch (spellTypes[2].funcionTercerPuesto)
            {
                case 0:
                    fasterSpell(sbehaviour);
                    break;
                default:
                    fasterSpell(sbehaviour);
                    break;
            }

            _playerData.ReduceMana(spellTypes[0].manaCost + spellTypes[1].manaCost + spellTypes[2].manaCost/2);
        }
        else _playerData.ReduceMana(spellTypes[0].manaCost + spellTypes[1].manaCost);


        spell.SetActive(true);

    
        // Vacíamos todo para poder hacer grupo de nuevo 
        for(int i=0; i<numSpellsChosen; ++i)
        {
            chosenSpells[i] = null;
        }
        _clientUI.EmptySlots();
        numSpellsChosen = 0;
        
    }

    /***********************************************************************/
    /*
    FUNCIONES DE HECHIZO EN TERCER PUESTO 
    */
    /***********************************************************************/

    // TercerPuesto == 0
    private void fasterSpell(SpellBehaviour spellBehaviour) {
        spellBehaviour.SetSpeed((int) Mathf.Ceil((float)(spellBehaviour.GetSpeed()) * 2.5f));
    }
}
