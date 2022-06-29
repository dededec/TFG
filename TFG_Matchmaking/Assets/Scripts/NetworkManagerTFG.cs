using System;
using System.Text;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

using Mirror;
using MessagesTFG;

public class NetworkManagerTFG : NetworkManager
{

    [Header("Prueba para uno")]
    public bool PruebaParaUno = false;

    [Header("Escalabilidad")]
    public List<string> MatchServerIPs = new List<string>();
    private int matchServerIndex = 0;

    public List<string> CombatServerIPs = new List<string>();
    private int combatServerIndex = 0;


    [Header("Matchmaker")]
    public Matchmaker matchmaker;

    [Header("Puertos")]
    [SerializeField] private int puertoInicial = 7778;
    [SerializeField] private int puertoActual = 7778;
    [SerializeField] private int puertoLimite = 7778 + 10 * 2000; // A cada partida le damos 2000 puertos (Lo mismo no hace falta tanto)
    private void IncrementPort()
    {
        puertoActual += 2000;
        if (puertoActual > puertoLimite)
        {
            puertoActual = puertoInicial;
        }
    }

    //---------------------------------------------------------------------

    private List<string> usuariosLogeados = new List<string>(); // Tiene que ser username por OnServerLoginMessageHandler

    private Dictionary<string, NetworkConnection> usernameConnection = new Dictionary<string, NetworkConnection>();
    private Dictionary<NetworkConnection, string> connectionUsername = new Dictionary<NetworkConnection, string>();
    private Dictionary<NetworkConnection, int> connectionNonce = new Dictionary<NetworkConnection, int>();
    private List<PlayerData> datosUsuariosLogeados = new List<PlayerData>();
    private Dictionary<string, string> usernameToken = new Dictionary<string, string>();
    private Dictionary<string, Coroutine> usernameCoroutine = new Dictionary<string, Coroutine>();
    private float loggedTime = 15 * 60;

    //---------------------------------------------------------------------

    /**********************************************
    ****                 LOGIN                *****
    ***********************************************/
    private void OnServerLoginStartHandler(NetworkConnection conn, LoginStartMessage request)
    {
        print("Recibida peticion de Login. Enviamos Nonce");
        //Enviamos Nonce
        NonceMessage msg = new NonceMessage();
        msg.Nonce = UnityEngine.Random.Range(13000, 50000);

        connectionNonce.Add(conn, msg.Nonce);
        conn.Send<NonceMessage>(msg);
    }
    private void OnServerLoginMessageHandler(NetworkConnection connection, LoginMessage msg)
    {
        if (usuariosLogeados.Contains(msg.Username))
        {
            print("Usuario ya logeado");
            connection.Disconnect();
        }
        else
        {
            StartCoroutine(MongoDBRequests.GetPlayerRequest(msg, connection, ProccessLoginGetRequestResult));
        }
    }

    private void ProccessLoginGetRequestResult(string password, PlayerData data, NetworkConnection connection)
    {
        if (data == null) // No lo ha encontrado
        {
            print("No se encontró el usuario");
            connection.Disconnect();
        }
        else // Existe el usuario
        {
            if (!connectionNonce.ContainsKey(connection)) // No hay nonce, debe haber algún error
            {
                print("No hay nonce");
                connection.Disconnect();
            }
            else
            {
                using (SHA256 sha = SHA256.Create())
                {
                    string dbpassword = GetHashString(data.password + connectionNonce[connection].ToString()); // h( h(password) + nonce); data.password = h(password)
                    if (password == dbpassword) // Contraseña bien introducida
                    {
                        print("Contraseñas correcta");
                        StartCoroutine(MongoDBRequests.GetPlayerTokenRequest(data, connection, ProccessGetPlayerTokenRequestResult));

                    }
                    else // Contraseña mal introducida
                    {
                        print("Contraseña erronea");
                        connection.Disconnect();
                    }
                }
            }
        }
    }
    private void ProccessGetPlayerTokenRequestResult(PlayerData player, string token, NetworkConnection connection)
    {

        if (string.IsNullOrEmpty(token))
        {
            print("Ha habido fallo al obtener token, se desconecta al usuario");
            connection.Disconnect();
        }
        else
        {
            print("Token obtenido, se manda al usuario: " + token);
            LoginResultMessage result = new LoginResultMessage();
            result.ResultCode = "success";
            result.Token = token;
            connection.Send<LoginResultMessage>(result);

            RegisterPlayerData(player, token, connection);
        }

    }

    /**********************************************
    ****       BUSCAR PARTIDA DE NUEVO        *****
    ***********************************************/
    private void OnServerBuscarPartidaMessageHandler(NetworkConnection conn, BuscarPartidaMessage msg)
    {
        /*
            1. Comprobar si el usuario(msg.Token) existe en usuariosLogeados
            2. Si existe, conseguimos el PlayerData asociado a ese username con un MongoDBRquest.GetPlayerRequest(token, conn, ProcessGetPlayerRequestBuscarPartida);
            3. Si no existe, conn.Disconnect();
        */
        TokenDecoded aux = new TokenDecoded(msg.Token, verify: false);
        print("BuscarPartidaMessage recibido de usuario: " + aux.userTag);

        if (usuariosLogeados.Contains(aux.userTag))
        {
            StartCoroutine(MongoDBRequests.GetPlayerRequest(msg.Token, conn, ProcessGetPlayerRequestBuscarPartida));
        }
        else
        {
            print("No es un usuario logeado");
            conn.Disconnect();
        }

    }

    private void ProcessGetPlayerRequestBuscarPartida(PlayerData player, string token, NetworkConnection conn)
    {
        print("ProcessGetPlayerRequest");
        if (player == null)
        {
            print("Player es null");
            conn.Disconnect();
        }
        else
        {
            print("Se encontró el jugador");
            ProccessGetPlayerTokenRequestResult(player, token, conn);
        }
    }


    /**********************************************
    ****              DISCONNECT              *****
    ***********************************************/
    private void OnServerDisconnectMessageHandler(NetworkConnection connection, DisconnectMessage msg)
    {
        /*
            1. Comprobar si usuario(msg.Token) existe en usuariosLogeados
            2. Si existe, usuariosLogeados.Remove(usuario(msg.Token))
            3. connection.Disconnect();
        */
        TokenDecoded aux = new TokenDecoded(msg.Token, verify: false);
        print("DisconnectMessage recibido de usuario: " + aux.userTag);

        if (usuariosLogeados.Contains(aux.userTag))
        {
            usuariosLogeados.Remove(aux.userTag);
            datosUsuariosLogeados.Remove(datosUsuariosLogeados.Find(p => p.player_tag == aux.userTag));
        }

        connection.Send<DisconnectSuccessMessage>(new DisconnectSuccessMessage());
    }


    // Registrar no significa aqui NADA con la base de datos
    private void RegisterPlayerData(PlayerData player, string token, NetworkConnection connection)
    {
        usernameConnection.Add(player.player_tag, connection);
        connectionUsername.Add(connection, player.player_tag);
        usernameToken.Add(player.player_tag, token);
        datosUsuariosLogeados.Add(player);

        usuariosLogeados.Add(player.player_tag);

        if (usernameCoroutine.ContainsKey(player.player_tag))
        {
            Coroutine aux = usernameCoroutine[player.player_tag];
            usernameCoroutine.Remove(player.player_tag);
            StopCoroutine(aux);
        }
        usernameCoroutine.Add(player.player_tag, StartCoroutine(IsLoggedRoutine(player.player_tag)));

        connectionNonce.Remove(connection);

        AddToMatchmaking(player);
    }

    private IEnumerator IsLoggedRoutine(string username)
    {
        yield return new WaitForSeconds(loggedTime);
        if (usuariosLogeados.Contains(username))
        {
            usuariosLogeados.Remove(username);
            datosUsuariosLogeados.Remove(datosUsuariosLogeados.Find(p => p.player_tag == username));
        }
    }

    private void AddToMatchmaking(PlayerData player)
    {
        matchmaker.Add(player);
        matchmaker.Process();
    }

    /**********************************************
    *********           REGISTER          *********
    ***********************************************/
    private void OnServerRegisterMessageHandler(NetworkConnection connection, RegisterMessage msg)
    {
        // print("Mensaje de Register recibido con username: " + msg.Username + " y password: " + msg.Password);
        PlayerData aux = new PlayerData(msg.Username, msg.Password);
        StartCoroutine(MongoDBRequests.PostPlayerRequest(aux.Stringify(), connection, ProccessRegisterPostRequestResult));
    }
    private void ProccessRegisterPostRequestResult(bool registrado, NetworkConnection connection)
    {
        RegisterResultMessage result = new RegisterResultMessage();

        if (!registrado) // No se ha registrado
        {
            print("Fallo al registrar usuario");
            result.ResultCode = "error";
        }
        else
        {
            print("Éxito al registrar usuario");
            result.ResultCode = "success";
        }

        connection.Send<RegisterResultMessage>(result);
    }


    /**********************************************
    *********           END MATCH         *********
    ***********************************************/
    private void OnServerTablaClasificacionMessageHandler(NetworkConnection connection, TablaClasificacionMessage message)
    {
        print("TablaClasificacionMessage recibido");
        connection.Send<ResultReceivedMessage>(new ResultReceivedMessage());
        // Procesar y hacer requests de put
        for (int i = 0; i < message.Tokens.Count; ++i)
        {
            TokenDecoded aux = new TokenDecoded(message.Tokens[i], verify: false);
            PlayerData data = datosUsuariosLogeados.Find(p => p.player_tag == aux.userTag);
            if (data != default(PlayerData))
            {
                print("OnServerTablaClasificacion: Jugador encontrado - " + aux.userTag);
                if (i == 0)
                {
                    data.partidas_ganadas++;
                }
                else if (i == matchmaker.PlayersPerMatch - 1)
                {
                    data.partidas_ultimo++;
                }
                // Falta puesto medio pero para eso hay que saber cual es el numero de partidas jugadas
                StartCoroutine(MongoDBRequests.PutPlayerRequest(data, connection, ProccessPutPlayerRequest));
            }
            else
            {
                print("OnServerTablaClasificacion: Jugador NO encontrado - " + aux.userTag);
                // El cliente está desconectado, hay que realizar un get request.
                StartCoroutine(MongoDBRequests.GetPlayerRequest_EndMatch(aux.userTag, i, connection, ProccessGetPlayerRequestEndMatch));

            }
        }
    }

    private void ProccessGetPlayerRequestEndMatch(PlayerData player, int puesto, NetworkConnection conn)
    {
        if (player != null)
        {
            if (puesto == 0)
            {
                player.partidas_ganadas++;
            }
            else if (puesto == matchmaker.PlayersPerMatch - 1)
            {
                player.partidas_ultimo++;
            }

            StartCoroutine(MongoDBRequests.PutPlayerRequest(player, conn, ProccessPutPlayerRequest));
        }
        else
        {
            print("Usuario no encontrado, debe haber un error con el servidor");
            conn.Disconnect();
        }
    }

    private void ProccessPutPlayerRequest(bool success, NetworkConnection connection)
    {
        if (!success)
        {
            print("Request fallida, desconectamos el server");
            connection.Disconnect();
        }
    }

    public override void OnStartServer()
    {
        NetworkServer.RegisterHandler<LoginMessage>(OnServerLoginMessageHandler);
        NetworkServer.RegisterHandler<RegisterMessage>(OnServerRegisterMessageHandler);
        NetworkServer.RegisterHandler<LoginStartMessage>(OnServerLoginStartHandler);
        NetworkServer.RegisterHandler<BuscarPartidaMessage>(OnServerBuscarPartidaMessageHandler);
        NetworkServer.RegisterHandler<DisconnectMessage>(OnServerDisconnectMessageHandler);
        NetworkServer.RegisterHandler<TablaClasificacionMessage>(OnServerTablaClasificacionMessageHandler);

        if(PruebaParaUno)
        {
            matchmaker.PlayersPerMatch = 1;
        }
    }
    public override void OnStopServer()
    {
        usernameConnection.Clear();
        usernameToken.Clear();
        connectionUsername.Clear();
        connectionNonce.Clear();
        datosUsuariosLogeados.Clear();

        base.OnStopServer();
    }

    public override void OnServerConnect(NetworkConnection conn)
    {
        print("Se intenta conectar");
        if (NetworkServer.connections.Count > maxConnections) // Si ya está el máximo número de conexiones, lo mandamos a su casa
        {
            print("Se desconecta");
            conn.Disconnect();
            return;
        }
    }

    public override void OnServerReady(NetworkConnection conn)
    {
        ChangeUIMessage msg = new ChangeUIMessage("UI_Matchmaking");
        conn.Send<ChangeUIMessage>(msg);

        base.OnServerReady(conn);
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        connectionNonce.Remove(conn); // Esto se manda a todas las conexiones que quieran hacer login (LoginStartMessage)

        if (connectionUsername.ContainsKey(conn)) // Esto es exclusivo de los que hayan hecho login correctamente
        {
            string username = connectionUsername[conn];
            connectionUsername.Remove(conn);
            usernameConnection.Remove(username);
            usernameToken.Remove(username);
            matchmaker.Remove(username);
        }
        base.OnServerDisconnect(conn);
    }



    /****************************************************************************/
    /**************       FUNCIONES PARA SERVIDOR PARTIDA         ***************/
    /****************************************************************************/

    public void StartMatch(List<string> usernames)
    {
        string puerto = puertoActual.ToString();
        IncrementPort();


        string tokens = "";
        foreach (var username in usernames)
        {
            tokens += " " + usernameToken[username];
        }

        string matchIP = MatchServerIPs[matchServerIndex];
        string argumentos = matchIP + " " + puerto + " 0 "; // ejecutable puerto servidor (IPOriginal puertoOriginal inutiles) combatServerIPs tokens 
        matchServerIndex = (matchServerIndex +1) % MatchServerIPs.Count;

        
            argumentos += CombatServerIPs[combatServerIndex];
            combatServerIndex = (combatServerIndex +1) % CombatServerIPs.Count;
            argumentos += " " + CombatServerIPs[combatServerIndex];
            combatServerIndex = (combatServerIndex +1) % CombatServerIPs.Count;

        

        argumentos += tokens;



        // Creamos Server Partida
        ExecuteCommand(GlobalVariables.matchServerScriptPath, argumentos);

        // Cambiamos a todos los jugadores que designa usernames
        ChangeConnectionMessage msg = new ChangeConnectionMessage();
        msg.Ip = matchIP;
        msg.Port = puerto;
        foreach (string username in usernames)
        {
            usernameConnection[username].Send<ChangeConnectionMessage>(msg);
        }
    }



    [Server]
    private void ExecuteCommand(string command, string arguments)
    {
        print("Execute command: " + command + " " + arguments);
        var processInfo = new ProcessStartInfo(command);
        processInfo.CreateNoWindow = true;
        processInfo.UseShellExecute = false;
        processInfo.RedirectStandardError = true;
        processInfo.RedirectStandardOutput = true;
        processInfo.Arguments = arguments;

        var process = Process.Start(processInfo);

        process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
            Console.WriteLine("output_id_" + process.Id + ">>" + e.Data);
        process.BeginOutputReadLine();

        process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
            Console.WriteLine("error_id_" + process.Id + ">>" + e.Data);
        process.BeginErrorReadLine();
    }

    public static byte[] GetHash(string inputString)
    {
        using (HashAlgorithm algorithm = SHA256.Create())
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
    }

    public static string GetHashString(string inputString)
    {
        StringBuilder sb = new StringBuilder();
        foreach (byte b in GetHash(inputString))
            sb.Append(b.ToString("X2"));

        return sb.ToString().ToLowerInvariant();
    }
}
