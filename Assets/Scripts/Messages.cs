using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace MessagesTFG
{
    // Formato para el mensaje donde se informa al servidor partida
    // del resultado del mensaje
    public struct CombatResultMessage : NetworkMessage
    {
        public string[] PlayerNames; // Jugadores en orden de muerte ([0] es el ultimo, [1] es el segundo [2] es el primero)
    }

    public struct ChangePortMessage : NetworkMessage
    {
        public string Argument; // Argumento adicional
    }
}

