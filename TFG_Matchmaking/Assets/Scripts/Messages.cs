using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace MessagesTFG
{
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

    public struct ChangeUIMessage : NetworkMessage
    {
        public string Tag;

        public ChangeUIMessage(string tag)
        {
            Tag = tag;
        }
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

    public struct TablaClasificacionMessage : NetworkMessage
    {
        public List<string> Tokens;
        public List<int[]> Resultados;
    }

    public struct ResultReceivedMessage : NetworkMessage
    {
        
    }

}

