using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Spell", menuName = "Spell", order = 0)]
public class Spell : ScriptableObject 
{
    // Defines aquí todo lo que define a un hechizo
    public new string name;
    public string level;

    public float speed;
    public int damage; 
    public int manaCost;

    // Esto es simplemente cómo se ve en las opciones del jugador
    public Sprite thumbnail;

    // Aquí falta una animación del hechizo moviéndose
    // y alome un efecto visual al impactar
    public AnimationClip animationMovimiento;

    // Primer lugar: sprite
    // Segundo lugar: elemento
    // Tercer lugar: efecto especial


    public AnimationClip spritePrimerPuesto;
    
    public Color colorElemental;  

    public Color elementoSegundoPuesto;  
    public int funcionTercerPuesto;
}
