using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    COSAS QUE NECESITA UN SPELL:
    BASICO:
    - nombre YES
    - nivel YES
    - coste YES
    - daño YES
    - velocidad YES

    - Cosas si 1er puesto YES
    - Cosas si 2o puesto MEH Falta probarlo
    - Cosas si 3er puesto MEH Falta probarlo
    
    UI:
    - Icono YES

    ANIMACION:
    - Animación movimiento
    - Efecto de impacto
    - Efecto de salida
*/

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

    // Averiguar lo de si está en primer, segundo o tercer lugar.
    // Primer lugar: sprite
    // Segundo lugar: elemento
    // Tercer lugar: efecto especial

    /*
    Para el efecto especial, he pensado guardar los posibles efectos (funciones)
    en un script aparte y que el efecto especial sea un número o algo, que nos
    permita acceder a una función del script.
    Ej.: EfectoEspecial = 3 -> FuncionScript = AumentarDaño(Spell)
    */

    /*
    PREGUNTAS:
    - ¿Qué velocidad tiene un hechizo conjunto?
        Esto sí o sí lo dicta el primero.
    
    - ¿Qué daño tiene un hechizo conjunto?
        Esto tal vez podemos hacer como con el coste
        o que el daño lo dicte el segundo (lo cual me tiene sentido)
        Pero entonces el coste del hechizo debería de dictarse por el
        segundo y no por el primero. -> El coste es una mezcla de ambos.

        Como el primero dicta velocidad y el segundo daño pos un cálculo con
        ambas cosas sería lo lógico y razonable.
    
    - ¿Qué coste tiene un hechizo conjunto?
        El coste irá asociado al número de hechizos en el conjunto.
        Lo más lógico y simple por ahora es que el coste de un hechizo
        sea el del primero del grupo multiplicado por algo
        (Ej.: Si son 2 hechizos pues coste * 1.5 y si es tres * 2)

        De todas formas esto se haría en la función de disparar y no aquí.
        (Aunque si hace falta sería meter nuevas variables).


        Por tanto, tanto daño como coste se calculan en la función de disparar
        y no aquí.
    */

    public AnimationClip spritePrimerPuesto;
    public Color elementoSegundoPuesto;  
    public int funcionTercerPuesto;

    // public delegate void funcionTercerPuesto(); // void funcion() {...}
    // public funcionTercerPuesto funcion; // Esto es lo que usamos para el tercer puesto, aquí metemos funciones sin parámetros y void.

    /*
    Creo que en verdad esto me parece matar moscas a cañonazos
    Si lo hago con un int, en el mismo script donde haga lo de disparar,
    podría hacer un switch antes de disparar (si hay 3 hechizos) y con eso ya
    hacer los cambios pertinentes.
    Algo así medio complejo sería por ejemplo disparar 3 hechizos en vez de 1,
    pero se puede solucionar haciendo que se dispare desde la función "especial" o algo así.
    */
}
