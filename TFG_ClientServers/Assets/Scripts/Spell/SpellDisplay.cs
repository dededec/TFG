using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Mirror;

// Todo lo referente a lo visual en los hechizos
public class SpellDisplay : NetworkBehaviour
{
    private Spell _spell;

    // Ya tendrá esto más cosas de animación y efectos y cosas.
    public SpriteRenderer _renderer;
    public Sprite DefaultSprite;
    public Animation _animation;
    public AnimationClip _clip;

    void Start()
    {
        if (_spell != null)
        {
            _renderer.sprite = _spell.thumbnail;
        }

    }

    public void SetSpell(Spell spell)
    {
        _spell = spell;
    }

    [ClientRpc]
    public void SetSpriteRPC(int tipo, string codigo)
    {
        // _renderer.sprite = sprite;
        switch (tipo)
        {
            case 0: // Es un color
                _renderer.sprite = DefaultSprite;
                Color aux;
                if (ColorUtility.TryParseHtmlString(codigo, out aux))
                {
                    aux.a = 1; // Ponemos el alpha a 1 (Siempre está a 0 y no se ve el sprite en ese caso)
                    _renderer.color = aux;
                }
                else
                {
                    _renderer.color = Color.white;
                }
                break;

            default:
                _renderer.sprite = DefaultSprite;
                _renderer.color = Color.white;
                break;
        }
    }
}
