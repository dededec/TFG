using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

// Todo lo referente a lo visual en los hechizos
public class SpellDisplay : MonoBehaviour
{
    private Spell _spell;

    // Ya tendrá esto más cosas de animación y efectos y cosas.
    public SpriteRenderer _renderer;
    public Animation _animation;
    public AnimationClip _clip;

    void Start()
    {
        if(_spell != null) {
            _renderer.sprite = _spell.thumbnail;
        
            // "Default clip could not be found in attached animations list", y no es problema de que la animacion sea legacy porque ya está puesto
            /*
            _animation = this.GetComponent<Animation>();
            if(_animation == null)
            {
                Debug.LogError("No Animation Component found.");
            }
            else
            {
                _clip = _spell.idleAnimation;
                _animation.clip = _clip;
                _animation.Play();
            }
            */
        }
        
    }

    public void SetSpell(Spell spell)
    {
        _spell = spell;
    }

    public void SetSprite(Sprite sprite) {
        _renderer.sprite = sprite;
    }
}
