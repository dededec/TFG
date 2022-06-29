using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace MessagesTFG
{
    // Formato para el mensaje donde se informa al servidor partida
    // del resultado del mensaje
    public struct CombatResultMessage : NetworkMessage
    {
        public string[] PlayerTokens; // Jugadores en orden de muerte ([0] es el ultimo, [1] es el segundo [2] es el primero)
    }

    public struct ResultReceivedMessage : NetworkMessage
    {
        
    }

    public struct ChangeConnectionMessage : NetworkMessage
    {
        public string Ip;
        public string Port;
    }

    public struct LoginStartMessage : NetworkMessage
    {

    }

    public struct LoginMessage : NetworkMessage
    {
        public string Username;
        public string Password;
    }

    public struct RegisterMessage : NetworkMessage
    {
        public string Username;
        public string Password;
    }

    public struct LoginResultMessage : NetworkMessage
    {
        public string ResultCode;
        public string Token;
    }

    public struct RegisterResultMessage : NetworkMessage
    {
        public string ResultCode;
    }

    public struct NonceMessage : NetworkMessage
    {
        public int Nonce;
    }

    public struct TokenMessage : NetworkMessage
    {
        public string Token;
    }

    public struct GameEndedMessage : NetworkMessage
    {
        public int Puesto;
    }

    public struct PlayerColorMessage : NetworkMessage
    {
        public Color Color;
    }

    public struct TablaClasificacionMessage : NetworkMessage
    {
        public List<string> Tokens;
        public List<int[]> Resultados;
    }

    public struct ChangeUIMessage : NetworkMessage
    {
        public string Tag;

        public ChangeUIMessage(string tag)
        {
            Tag = tag;
        }
    }

    public struct CombatUsersMessage : NetworkMessage
    {
        public List<string> Users;
    }

    public struct BuscarPartidaMessage : NetworkMessage
    {
        public string Token;
    }

    public struct DisconnectMessage : NetworkMessage
    {
        public string Token;
    }

    public struct DisconnectSuccessMessage : NetworkMessage
    {

    }

}

