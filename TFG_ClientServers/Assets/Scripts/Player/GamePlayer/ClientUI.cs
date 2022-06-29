using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using Mirror;

public class ClientUI : NetworkBehaviour
{
    [SerializeField] private PlayerControl _playerData = null;
    [SerializeField] private Camera _cam = null;

    public TMP_Text textoTiempo = null;

    [Header("Vida y Mana")]
    [SerializeField] private Image _contenedorVida = null; // Esto es simplemente el rectángulo dentro del cual va la barra
    [SerializeField] private TMP_Text _textoVida = null;
    [SerializeField] private Image _contenedorMana = null;
    [SerializeField] private TMP_Text _textoMana = null;

    // UI de hechizos agrupados
    [Header("Hechizos Agrupados")]
    [SerializeField] Image[] spellSlots = null;
    [SerializeField] Sprite emptySlotSprite = null;
    int filledSlots = 0;

    // UI de seleccionar hechizos
    [Header("Seleccionar Hechizos")]
    [SerializeField] private Canvas _canvas = null;
    [SerializeField] private Transform _playerTransform = null;
    [SerializeField] private RectTransform _selectorTransform = null;
    [SerializeField] private GameObject _UISpellList = null;
    [SerializeField] private List<GameObject> _spellDisplay = new List<GameObject>();
    private int selected = 0;


    public override void OnStartClient()
    {
        _UISpellList.transform.position = worldToUISpace(_canvas, _playerTransform.position); // Esto no se si hace falta hacerlo en el Update
        _textoVida.text = _playerData.GetHealth().ToString() + "/" + _playerData.GetMaxHealth().ToString();
        _textoMana.text = _playerData.GetMana().ToString(); // + "\n/\n" + _playerData.GetMaxMana().ToString();
    }

    public void UpdateHealthUI()
    {
        _textoVida.text = _playerData.GetHealth().ToString() + "/" + _playerData.GetMaxHealth().ToString();
    }

    [TargetRpc]
    public void UpdateHealthUITargetRPC(NetworkConnection connection, float health)
    {
        _textoVida.text = health.ToString() + "/" + _playerData.GetMaxHealth().ToString();
    }

    public void UpdateManaUI()
    {
        _textoMana.text = _playerData.GetMana().ToString(); //+ "\n/\n" + _playerData.GetMaxMana().ToString();
    }

    [TargetRpc]
    public void UpdateManaUITargetRPC(NetworkConnection connection, float mana)
    {
        _textoMana.text = mana.ToString();
    }

    public void DisableAll()
    {
        _canvas.enabled = false;
        // _contenedorVida.enabled = false;
        // _textoVida.enabled = false;
        // // _contenedorMana.enabled = false;
        // _textoMana.enabled = false;
    }

    public void InitializeSpellDisplay(List<Spell> _spells)
    {
        // Siempre vas a tener al menos un hechizo (por diseño del juego)
        int[] positions = new int[_spells.Count];

        int initial = (int)(-100 + 50 * Mathf.Round(_spells.Count / 2.0f + 0.1f)); // El + 0.1f es porque Round, si tienes numPar.5 (0.5, 2.5, 4.5, etc) redondea al nº par
        for (int i = 0; i < _spells.Count; ++i) positions[i] = initial - 50 * i;

        // Creamos el vector de display según los spells que tengamos
        for (int i = 0; i < _spells.Count; ++i)
        {
            _spellDisplay.Add(new GameObject());
            _spellDisplay[i].transform.SetParent(_UISpellList.transform);

            RectTransform rt = _spellDisplay[i].AddComponent<RectTransform>();
            rt.sizeDelta = Vector2.one * 32; // (1,1) * 32 = (32,32)
            rt.anchorMin = Vector2.up; // up = (0,1) que es el vector que buscamos
            rt.anchorMax = Vector2.up;
            rt.localScale = Vector3.one;
            rt.anchoredPosition = new Vector3(0, positions[i], 0);

            Image im = _spellDisplay[i].AddComponent<Image>();
            im.sprite = _spells[i].thumbnail;
        }

        // Ponemos el selector al principio de la lista  
        _selectorTransform.position = _spellDisplay[0].transform.position;
        selected = 0;
    }

    public void UpdateSelectedSpell(int selectedParam)
    {
        selected = selectedParam;
        // _selectorTransform.position = _spellDisplay[selected].transform.position; // Lo ponemos al principio de la lista    
    }

    public void SelectSpell(int selected)
    {
        Image selectedImage = _spellDisplay[selected].GetComponent<Image>();
        spellSlots[filledSlots].sprite = selectedImage.sprite;
        //spellSlots[filledSlots].color = selectedImage.color; Esto no hace falta porque 1 hechizo = 1 imagen

        filledSlots++;
    }

    public void EmptySlots()
    {
        for (int i = 0; i < filledSlots; ++i)
        {
            spellSlots[i].sprite = emptySlotSprite;
            spellSlots[i].color = Color.white;
        }

        filledSlots = 0;
    }

    [TargetRpc]
    public void EmptySlotsTargetRPC(NetworkConnection conn)
    {
        for (int i = 0; i < filledSlots; ++i)
        {
            spellSlots[i].sprite = emptySlotSprite;
            spellSlots[i].color = Color.white;
        }

        filledSlots = 0;
    }

    private Vector3 worldToUISpace(Canvas parentCanvas, Vector3 worldPos)
    {
        //Convert the world for screen point so that it can be used with ScreenPointToLocalPointInRectangle function
        Vector3 screenPos = _cam.WorldToScreenPoint(worldPos);
        Vector2 movePos;

        //Convert the screenpoint to ui rectangle local point
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentCanvas.transform as RectTransform, screenPos, parentCanvas.worldCamera, out movePos);
        //Convert the local point to world point
        return parentCanvas.transform.TransformPoint(movePos);
    }

    private float lastSign = 1f;

    [ClientCallback]
    private void Update()
    {
        if (!isClient) return; 
        // _UISpellList.transform.position = worldToUISpace(_canvas, _playerTransform.position); // Esto no se si hace falta hacerlo en el Update
        _selectorTransform.position = _spellDisplay[selected].transform.position;

        Vector3 mouse = Input.mousePosition;
        mouse = _cam.ScreenToWorldPoint(mouse);
        mouse = mouse - _playerTransform.position;

        if (Mathf.Sign(mouse.x) != lastSign)
        {
            Vector3 lScale = _UISpellList.transform.localScale;
            lScale.x *= -1;
            _UISpellList.transform.localScale = lScale;
            lastSign = Mathf.Sign(mouse.x);
        }

    }
}
