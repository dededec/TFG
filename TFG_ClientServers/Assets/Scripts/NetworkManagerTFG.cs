using System;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Collections;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Diagnostics;
using MessagesTFG;
using UnityEngine.UI;

public class NetworkManagerTFG : NetworkManager
{
    [Header("Prueba para uno")]
    public bool PruebaUno = false;

    private const string UI_Fin = "UI_Fin";
    private const string UI_Menu = "UI_Menu";
    private const string UI_Partida = "UI_Partida";
    private const string UI_Prepartida = "UI_Prepartida";
    private const string UI_Combate = "UI_Combate";
    private const string UI_Cargando = "UI_Cargando";

    // 0 - Servidor Partida
    // 1 - Servidor Combate
    // 2 - Cliente
    [Header("Tipo de servidor")]
    [SerializeField] private int modoServidor = 0;
    private const int ServidorPartida = 0;
    private const int ServidorCombate = 1;
    private const int Cliente = 2;

    //---------------------------------------------------------------------
    List<string> CombatServerIPs = new List<string>(); // Debería de ser dos la verdad.

    private int puertoOriginal = 7777;
    private string IPOriginal = GlobalVariables.matchmakingServerIP;

    private int puertoInicial = 7778;
    private int puertoActual = 7778;
    private int puertoLimite = 9777;
    public void ChangePort(string port)
    {
        if (!GetComponent<Monke>().enabled)
        {
            (transport as kcp2k.KcpTransport).Port = Convert.ToUInt16(port);
        }
        else
        {
            ((transport as Monke).CommunicationTransport as kcp2k.KcpTransport).Port = Convert.ToUInt16(port);
        }
    }

    public void ChangePort(ushort port)
    {
        if (!GetComponent<Monke>().enabled)
        {
            (transport as kcp2k.KcpTransport).Port = port;
        }
        else
        {
            ((transport as Monke).CommunicationTransport as kcp2k.KcpTransport).Port = port;
        }
    }

    public ushort GetPort()
    {
        if (!GetComponent<Monke>().enabled)
        {
            return (transport as kcp2k.KcpTransport).Port;
        }
        else
        {
            return ((transport as Monke).CommunicationTransport as kcp2k.KcpTransport).Port;
        }
    }

    //---------------------------------------------------------------------

    [Header("Conexion")]
    public float tiempoEsperaConexion = 5f;
    public int intentosConexion = 10;

    private int nonce = -1; // nonce es siempre positivo (puede que tenga que cambiar)
    public void SetNonce(int newValue) => this.nonce = newValue;
    public int GetNonce() => this.nonce;

    public string token = null;
    public Color colorJugador = Color.white;
    private Color[] coloresServidor = new Color[] { Color.red, Color.blue, Color.green, Color.yellow, Color.magenta, Color.black };
    private int indexColoresServidor = 0;

    private bool cambiandoConexion = false;

    //---------------------------------------------------------------------

    [Header("Partida")]
    private bool partidaFinalizada = false;

    [SerializeField] private ClasificationManager managerClasification = null;
    private List<KeyValuePair<string, int[]>> clasificacionOrdenada = new List<KeyValuePair<string, int[]>>();

    private bool clasificacionActualizada = true;
    private int indexClasificacionActualizada = 0;

    private int maxRondas = 3; // La ronda maxRondas es la final
    private int rondaAJugar = 0;
    private List<string> tokens = new List<string>();
    private List<string> users = new List<string>();
    public List<string> Tokens() => tokens;
    private List<KeyValuePair<string, NetworkConnection>> tokenConnection = new List<KeyValuePair<string, NetworkConnection>>();
    public string GetToken(NetworkConnection conn) => tokenConnection.Find(dupla => dupla.Value == conn).Key;


    //---------------------------------------------------------------------

    [Header("UI")]
    public GameObject UIActual = null;
    public Text debugText = null;
    public bool logging = false;

    //---------------------------------------------------------------------

    [Header("Scenes")]
    [Scene] [SerializeField] private string lobbyScene = string.Empty;

    //---------------------------------------------------------------------

    [Header("Lobby")]
    [SerializeField] private RoomPlayer roomPlayerPrefab = null;
    public List<RoomPlayer> RoomPlayers { get; } = new List<RoomPlayer>();
    public int maxPlayers = 4;
    public int serverPlayers = 0;

    //---------------------------------------------------------------------

    [Header("Combate")]
    [SerializeField] private GameObject playerSpawnSystem = null;
    [SerializeField] private GameObject timerPrefab = null;
    [SerializeField] private GameObject manaRegenerationPrefab = null;
    public List<GamePlayer> GamePlayers { get; } = new List<GamePlayer>();

    //---------------------------------------------------------------------

    public static event Action OnClientConnected;
    public static event Action OnClientDisconnected;
    public static event Action<NetworkConnection> OnServerReadied;

    //---------------------------------------------------------------------


    public override void Start()
    {
        string[] args = System.Environment.GetCommandLineArgs(); // El primer elemento es el nombre del ejecutable

        if (args.Length > 2) // Es servidor - programa puerto modoservidor usuario1 usuario2 usuario3 ...
        {
            ChangePort(args[1]);

            puertoInicial = Convert.ToInt32(args[1]) + 1; // Si nos pasan 7777, es 7778, [7778 -> 9778]
            puertoActual = puertoInicial;
            puertoLimite = puertoInicial + 2000;

            modoServidor = Convert.ToInt32(args[2]);
        }
        else if (args.Length == 1) // Es cliente, no hay argumentos al empezar - solo queremos que se conecte al matchmaking
        {
            networkAddress = GlobalVariables.matchmakingServerIP;
            ChangePort(GlobalVariables.matchmakingServerPort);
            modoServidor = Cliente;
            UIActual = GameObject.FindGameObjectWithTag(UI_Menu);
        }

        base.Start();
    }

    //----------------------------------------------------------------------------------------------------------------
    //--------------------------------------- CLIENTE ----------------------------------------------------------------
    //----------------------------------------------------------------------------------------------------------------

    public override void OnStartClient()
    {
        var spawneablePrefabs = Resources.LoadAll<GameObject>("SpawneablePrefabs");
        foreach (var prefab in spawneablePrefabs)
        {
            NetworkClient.RegisterPrefab(prefab); // ClientScene.RegisterPrefab(prefab);
        }

        NetworkClient.RegisterHandler<LoginResultMessage>(OnClientLoginResultMessageHandler);
        NetworkClient.RegisterHandler<RegisterResultMessage>(OnClientRegisterResultMessageHandler);
        NetworkClient.RegisterHandler<ChangeConnectionMessage>(OnClientChangeConnectionMessageHandler);
        NetworkClient.RegisterHandler<NonceMessage>(OnClientNonceMessageHandler);
        NetworkClient.RegisterHandler<GameEndedMessage>(OnClientGameEndedMessageHandler);
        NetworkClient.RegisterHandler<PlayerColorMessage>(OnClientPlayerColorMessage);
        NetworkClient.RegisterHandler<TablaClasificacionMessage>(OnClientTablaClasificacionMessageHandler);
        NetworkClient.RegisterHandler<ChangeUIMessage>(OnClientChangeUIMessage);

        if (modoServidor == ServidorCombate || modoServidor == ServidorPartida)
        {
            NetworkClient.RegisterHandler<ResultReceivedMessage>(OnClientCombatResultReceivedMessage);
        }
        else if (modoServidor == Cliente)
        {
            NetworkClient.RegisterHandler<DisconnectSuccessMessage>(OnClientDisconnectSuccessMessageHandler);
            NetworkClient.RegisterHandler<CombatUsersMessage>(OnClientCombatUsersMessageHandler);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // print("OnSceneLoaded: " + scene.name);
        // print(mode);
        if (scene.name.StartsWith("Menu"))
        {
            if (cambiandoConexion)
            {
                SetActiveChildren(GameObject.FindGameObjectWithTag(UI_Menu), false);
                SetActiveChildren(GameObject.FindGameObjectWithTag(UI_Cargando), true);
                debugText = GameObject.FindGameObjectWithTag("DebugText").GetComponent<Text>(); // El debugText del UI anterior no se recoge 
            }
            else
            {
                UIActual = GameObject.FindGameObjectWithTag(UI_Menu);
            }
        }
    }

    public void ChangeConnection(string port)
    {
        if (!NetworkClient.active) return;
        StartCoroutine(ChangeConnectionCoroutine(networkAddress, port));
    }

    public void ChangeConnection(string ip, string port)
    {
        if (!NetworkClient.active) return;
        StartCoroutine(ChangeConnectionCoroutine(ip, port));
    }

    public IEnumerator ChangeConnectionCoroutine(string ip, string port)
    {
        // 1. Desconectar del server
        print("ChangeConnectionRoutine comenzada");
        cambiandoConexion = true;
        StopClient();

        do
        {
            yield return new WaitForSeconds(tiempoEsperaConexion);
        } while (NetworkClient.active);


        // 2. Cambiar IP y puerto
        networkAddress = ip;
        ChangePort(port);

        // 3. Conectar de nuevo con StartClient()
        StartClient();
        do
        {
            yield return new WaitForSeconds(tiempoEsperaConexion);
        } while (!NetworkClient.isConnected);
        cambiandoConexion = false;
    }


    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
        OnClientConnected?.Invoke(); // Lanzamos el evento con lo que queramos hacer
    }

    public override void OnClientSceneChanged(NetworkConnection conn)
    {
        if (SceneManager.GetActiveScene().name.StartsWith("Lobby"))
        {
            if (!string.IsNullOrEmpty(token))
            {
                //Enviamos mensaje de token
                TokenMessage msg = new TokenMessage();
                msg.Token = token;
                NetworkClient.Send<TokenMessage>(msg);
            }
        }
        base.OnClientSceneChanged(conn);
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);
        OnClientDisconnected?.Invoke(); // Lanzamos el evento con lo que queramos hacer
        if(SceneManager.GetActiveScene().name.StartsWith("Menu"))
        {
            ChangeUIActual(UI_Menu);
        }
    }

    public void CerrarSesion() => StartCoroutine(CerrarSesionCoroutine());
    public IEnumerator CerrarSesionCoroutine()
    {
        yield return ChangeConnectionCoroutine(GlobalVariables.matchmakingServerIP, GlobalVariables.matchmakingServerPort);
        while (!NetworkClient.ready)
        {
            yield return new WaitForSeconds(tiempoEsperaConexion);
        }

        DisconnectMessage msg = new DisconnectMessage();
        msg.Token = token;
        NetworkClient.Send<DisconnectMessage>(msg);
    }

    public void BuscarPartida() => StartCoroutine(BuscarPartidaCoroutine());
    private IEnumerator BuscarPartidaCoroutine()
    {
        yield return ChangeConnectionCoroutine(GlobalVariables.matchmakingServerIP, GlobalVariables.matchmakingServerPort);
        while (!NetworkClient.ready)
        {
            yield return new WaitForSeconds(tiempoEsperaConexion);
        }

        BuscarPartidaMessage msg = new BuscarPartidaMessage();
        msg.Token = token;
        NetworkClient.Send<BuscarPartidaMessage>(msg);
    }

    private void UI_FinActive(int puesto)
    {
        if (SceneManager.GetActiveScene().name.StartsWith("Lobby"))
        {
            ChangeUIActual(UI_Fin);
            GameObject textoPuesto = GameObject.FindGameObjectWithTag("TextoPuesto");
            textoPuesto.GetComponent<TMPro.TMP_Text>().text = "HAS QUEDADO EN EL PUESTO " + (puesto + 1);
        }
    }

    /****************************************************************************/
    /**************      FUNCIONES PARA MENSAJES EN CLIENTE       ***************/
    /****************************************************************************/

    private void OnClientLoginResultMessageHandler(LoginResultMessage msg)
    {
        print("Recibido LoginResultMessage: (" + msg.ResultCode + "," + msg.Token + ")");
        if (!string.IsNullOrEmpty(msg.Token))
        {
            token = msg.Token;
        }
        logging = false;
        SetDebugText("BUSCANDO PARTIDA...", disappear: false);
    }

    private void OnClientRegisterResultMessageHandler(RegisterResultMessage msg)
    {
        print("Recibido RegisterResultMessage: (" + msg.ResultCode + ")");
        StopClient();
    }

    private void OnClientNonceMessageHandler(NonceMessage msg)
    {
        print("Recibido NonceMessage: (" + msg.Nonce + ")");
        nonce = msg.Nonce;
    }

    private void OnClientChangeConnectionMessageHandler(ChangeConnectionMessage msg)
    {
        print("Recibido ChangeConnectionMessage: (" + msg.Ip + "," + msg.Port + ")");
        ChangeConnection(msg.Ip, msg.Port);
    }

    private void OnClientGameEndedMessageHandler(GameEndedMessage msg)
    {
        UI_FinActive(msg.Puesto);
    }

    private void OnClientCombatResultReceivedMessage(ResultReceivedMessage msg)
    {
        if (modoServidor == ServidorCombate || modoServidor == ServidorPartida) // Miro lo del server para que no se le pueda enviar a un jugador
        {
            print("CombatResultReceivedMessage. Server Quitting.");
            StopClient();
            Application.Quit();
        }
    }

    private void OnClientPlayerColorMessage(PlayerColorMessage msg)
    {
        print("Color obtenido: " + msg.Color);
        colorJugador = msg.Color;
    }

    private void OnClientTablaClasificacionMessageHandler(TablaClasificacionMessage msg)
    {
        print("TablaClasificacion recibida");
        if (UIActual.tag == UI_Prepartida) // Solo los que estén esperando (los otros estarán en UI_Fin)
        {
            ChangeUIActual(UI_Partida);
        }

        List<KeyValuePair<string, int[]>> clasificacion = new List<KeyValuePair<string, int[]>>();
        for (int i = 0; i < msg.Tokens.Count; ++i)
        {
            TokenDecoded aux = new TokenDecoded(msg.Tokens[i], verify: false);
            clasificacion.Add(new KeyValuePair<string, int[]>(aux.userTag, msg.Resultados[i]));
        }
        clasificacionOrdenada = clasificacion;
        ActualizarTablaClasificacion();
    }

    private void OnClientChangeUIMessage(ChangeUIMessage msg)
    {
        print("Mensaje ChangeUI con tag: " + msg.Tag);
        ChangeUIActual(msg.Tag);
    }

    private void OnClientDisconnectSuccessMessageHandler(DisconnectSuccessMessage msg)
    {
        print("DisconnectSuccessMessage");
        StopClient();
        if (SceneManager.GetActiveScene().name.StartsWith("Menu")) // Debería de ir porque con StopClient cambia la escena
        {
            ChangeUIActual(UI_Menu);
        }
    }

    private void OnClientCombatUsersMessageHandler(CombatUsersMessage msg)
    {
        print("CombatUsersMessage Received");
        ChangeUIActual(UI_Combate);
        SetUICombate(msg.Users);
    }

    public void ChangeUIActual(string tag)
    {
        if (UIActual != null)
        {
            SetActiveChildren(UIActual, false);
        }

        UIActual = GameObject.FindGameObjectWithTag(tag);
        SetActiveChildren(UIActual, true);
        debugText = GameObject.FindGameObjectWithTag("DebugText").GetComponent<Text>(); // El debugText del UI anterior no se recoge
    }

    private void SetUICombate(List<string> usuarios)
    {
        Text[] textUsuarios = UIActual.GetComponentsInChildren<Text>();
        for (int i = 0; i < usuarios.Count; ++i)
        {
            textUsuarios[i].text = usuarios[i];
        }
    }

    private void ActualizarTablaClasificacion()
    {
        /*
            Asocio el jugador en el puesto i con el hijo i de la tabla
            Tabla - puesto1 puesto2 puesto3 puesto4 columnas
        */
        if (!SceneManager.GetActiveScene().name.StartsWith("Lobby"))
        {
            print("No estamos en LobbyScene");
            return;
        }

        // Esto nos dice si el jugador está en la partida o si ya ha perdido (por ej.: antes de la final)
        if (UIActual.tag != UI_Partida)
        {
            return;
        }

        Transform tablaClasificacion = UIActual.transform.Find("TablaClasificacion");

        for (int i = 0; i < clasificacionOrdenada.Count; ++i)
        {
            var jugador_i = clasificacionOrdenada[i];
            var puesto_i = tablaClasificacion.GetChild(i);
            var texto = puesto_i.GetChild(0).gameObject.GetComponent<Text>();
            texto.text = jugador_i.Key; // usuario

            for (int j = 1; j < puesto_i.childCount; ++j) // partidos1o partidos2o partidos3o total
            {
                texto = puesto_i.GetChild(j).gameObject.GetComponent<Text>();
                texto.text = jugador_i.Value[j - 1].ToString(); // 0..3
            }
        }
    }


    // LOGIN
    public void Login(string username, string password, Text debugTextArg)
    {
        logging = true;
        networkAddress = GlobalVariables.matchmakingServerIP;
        ChangePort(GlobalVariables.matchmakingServerPort);
        print("Intentamos conectarnos a ip " + networkAddress + " y puerto " + GetPort());
        StartClient();
        StartCoroutine(EnviaLogin(username, password, debugTextArg));
    }

    private IEnumerator EnviaLogin(string username, string password, Text debugTextArg)
    {
        int i = 0;
        while (i < intentosConexion && !NetworkClient.isConnected)
        {
            yield return new WaitForSeconds(tiempoEsperaConexion);
            ++i;
        }

        if (i >= intentosConexion && !NetworkClient.isConnected)
        {
            debugTextArg.text = "Error: agotado tiempo de conexion";
            print("Error: agotado tiempo de conexion");
            StopClient();
            yield break;
        }

        NetworkClient.Send<LoginStartMessage>(new LoginStartMessage()); // Le decimos al server que queremos hacer login (nos enviará un Nonce)

        i = 0;
        while (i < intentosConexion && GetNonce() == -1) // Para esto hay que estar conectado
        {
            yield return new WaitForSeconds(1f);
            ++i;
        }

        if (GetNonce() == -1) // No nos ha enviado el servidor el Nonce.
        {
            debugTextArg.text = "Error: agotado tiempo de conexion";
            print("Error: agotado tiempo de conexion");
            StopClient();
            yield break;
        }

        int aux = GetNonce();
        using (SHA256 sha = SHA256.Create())
        {
            LoginMessage login = new LoginMessage();
            login.Username = username;
            login.Password = GetHashString(GetHashString(password) + aux.ToString()); // h( h(password) + nonce )

            NetworkClient.Send<LoginMessage>(login);
            print("Enviamos LoginMessage");
        }
        SetNonce(-1);
    }


    // REGISTER
    public void Register(string username, string password)
    {
        StartClient();
        StartCoroutine(EnviaRegister(username, password));
    }

    private IEnumerator EnviaRegister(string username, string password)
    {
        StartClient();

        RegisterMessage register = new RegisterMessage();
        register.Username = username;
        register.Password = GetHashString(password); // Aplicamos SHA256 a la contraseña

        while (!NetworkClient.isConnected) // Mientras no se conecte
        {
            yield return new WaitForEndOfFrame();
        }

        NetworkClient.Send<RegisterMessage>(register);
        print("Enviamos RegisterMessage");
    }


    //----------------------------------------------------------------------------------------------------------------
    //--------------------------------------- SERVIDOR ---------------------------------------------------------------
    //----------------------------------------------------------------------------------------------------------------

    public override void OnStartServer()
    {
        spawnPrefabs = Resources.LoadAll<GameObject>("SpawneablePrefabs").ToList();
        managerClasification = gameObject.GetComponent<ClasificationManager>();
        NetworkServer.RegisterHandler<TokenMessage>(OnServerTokenMessageHandler);

        string[] args = System.Environment.GetCommandLineArgs(); // ejecutable puerto servidor IPOriginal puertoOriginal combatServerIPs tokens 

        if (modoServidor == ServidorPartida)
        {
            NetworkServer.RegisterHandler<CombatResultMessage>(OnServerCombatResultMessageHandler);
            maxPlayers = maxConnections;
            if (PruebaUno)
            {
                maxConnections = 1;
                maxPlayers = 1;
            }

            CombatServerIPs.Add(args[3]);
            CombatServerIPs.Add(args[4]);

            for (int i = 0; i < maxPlayers; ++i)
            {
                tokens.Add(args[5 + i]);
                TokenDecoded aux = new TokenDecoded(args[5 + i], verify: false);
                users.Add(aux.userTag);
                managerClasification.AddNewPlayer(args[5 + i]);
            }
        }
        else if (modoServidor == ServidorCombate) // ejecutable puerto servidor IPOriginal puertoOriginal tokens 
        {
            maxConnections = maxConnections / 2;
            maxPlayers = maxConnections;
            if (PruebaUno)
            {
                maxConnections = 1;
                maxPlayers = 1;
            }

            IPOriginal = args[3];
            puertoOriginal = Convert.ToInt16(args[4]);

            for (int i = 0; i < maxPlayers; ++i)
            {
                tokens.Add(args[5 + i]);
                TokenDecoded aux = new TokenDecoded(args[5 + i], verify: false);
                users.Add(aux.userTag);
                print("Cargado token " + args[5 + i] + " en server modo " + modoServidor);
            }
        }


    }

    // Llamada desde Timer cuando TimeRemaining == 0
    // Lo hago así para evitar usar Update aquí todo el rato
    public void EndCombat() => StartCoroutine(EndCombatCoroutine());
    public IEnumerator EndCombatCoroutine()
    {
        var combatResult = GamePlayers.OrderByDescending(x => x.playerControl.GetHealth()).ToList();
        int i = 0;
        foreach (var player in combatResult)
        {
            managerClasification.combatResult[i] = player.playerControl.Token;
            ++i;
        }

        ChangeConnectionMessage msg = new ChangeConnectionMessage();
        msg.Ip = IPOriginal;
        msg.Port = puertoOriginal.ToString();
        NetworkServer.SendToAll<ChangeConnectionMessage>(msg);
        yield return new WaitForEndOfFrame();

        GamePlayers.Clear();

        StopServer();
        while (NetworkServer.active)
        {
            yield return new WaitForEndOfFrame();
        }


        CombatResultMessage resultado = new CombatResultMessage();
        resultado.PlayerTokens = managerClasification.combatResult;

        ChangePort((ushort) puertoOriginal);
        networkAddress = IPOriginal;
        StartClient();
        while (!NetworkClient.isConnected)
        {
            yield return new WaitForEndOfFrame();
        }

        NetworkClient.Send<CombatResultMessage>(resultado);
    }

    public override void OnStopServer()
    {
        RoomPlayers.Clear();
        GamePlayers.Clear();
        base.OnStopServer();
    }

    public override void OnServerConnect(NetworkConnection conn)
    {
        if (SceneManager.GetActiveScene().path != lobbyScene) // Si no estamos en el lobby, lo mandamos a su casa
        {
            conn.Disconnect();
            return;
        }

        if (numPlayers > maxConnections) // Si ya está el máximo número de jugadores, lo mandamos a su casa
        {
            conn.Disconnect();
            return;
        }
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        // Quitar de las estructuras de datos.
        if (conn.identity != null)
        {
            print("OnServerDisconnect: conn.identity no es null");
            var player = conn.identity.GetComponent<RoomPlayer>();
            RoomPlayers.Remove(player);
        }

        if (tokenConnection.Exists(p => p.Value == conn))
        {
            print("OnServerDisconnect: Existe token asociado a la conexión");
            tokenConnection.Remove(tokenConnection.Find(p => p.Value == conn));
            if (modoServidor == ServidorPartida)
            {
                serverPlayers--;
                if (serverPlayers < 0) serverPlayers = 0;
                if (partidaFinalizada && serverPlayers <= 0) // partidaFinalizada implica que serverPlayers ha sido maxPlayers (buscar partidaFinalizada = true para comprobarlo)
                {
                    StartCoroutine(EndMatch());
                }
            }
        }


        base.OnServerDisconnect(conn);
    }

    private IEnumerator EndMatch()
    {
        /*
            Cambiamos conexión al server matchmaking y enviamos resultado de la partida.
        */
        yield return StartCoroutine(ChangeConnectionCoroutine(GlobalVariables.matchmakingServerIP, GlobalVariables.matchmakingServerPort));
        TablaClasificacionMessage clasificacionMessage = new TablaClasificacionMessage();
        List<string> tokensOrdenados = new List<string>();
        List<int[]> resultados = new List<int[]>();
        foreach (var kvp in clasificacionOrdenada)
        {
            tokensOrdenados.Add(kvp.Key);
            resultados.Add(kvp.Value);
        }
        clasificacionMessage.Tokens = tokensOrdenados;
        clasificacionMessage.Resultados = resultados;
        NetworkClient.Send<TablaClasificacionMessage>(clasificacionMessage);
    }

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        if (modoServidor == ServidorPartida)
        {
            if (SceneManager.GetActiveScene().path == lobbyScene && rondaAJugar <= maxRondas)
            {
                RoomPlayer roomPlayerInstance = Instantiate(roomPlayerPrefab);
                if (indexColoresServidor < maxPlayers)
                {
                    PlayerColorMessage msg = new PlayerColorMessage();
                    msg.Color = coloresServidor[indexColoresServidor];
                    indexColoresServidor++;
                    conn.Send<PlayerColorMessage>(msg);
                }


                NetworkServer.AddPlayerForConnection(conn, roomPlayerInstance.gameObject); // Esto se encarga de spawnearlo en todos los clientes también
                RoomPlayers.Add(roomPlayerInstance);

                if ((numPlayers == maxPlayers && rondaAJugar < maxRondas) || (numPlayers == maxPlayers / 2 && rondaAJugar == maxRondas))
                {
                    NextRound();
                }
            }
        }
        else if (modoServidor == ServidorCombate)
        {
            if (SceneManager.GetActiveScene().path == lobbyScene)
            {
                RoomPlayer roomPlayerInstance = Instantiate(roomPlayerPrefab);
                NetworkServer.AddPlayerForConnection(conn, roomPlayerInstance.gameObject); // Esto se encarga de spawnearlo en todos los clientes también
                RoomPlayers.Add(roomPlayerInstance);

                if (numPlayers == maxPlayers)
                {
                    StartGame();
                }
            }
        }
    }

    private void IncrementPort()
    {
        puertoActual++;
        if (puertoActual > puertoLimite)
        {
            puertoActual = puertoInicial;
        }
    }


    private void ChangePlayersToCombatServer(string ip, string puerto, List<int> indices)
    {
        ChangeConnectionMessage msg = new ChangeConnectionMessage();
        msg.Ip = ip;
        msg.Port = puerto;

        foreach (int indice in indices)
        {
            KeyValuePair<string, NetworkConnection> result = tokenConnection.SingleOrDefault(dupla => dupla.Key == tokens[indice]);
            if (result.Equals(default(KeyValuePair<string, NetworkConnection>)))
            {
                print("No se ha encontrado token en tokenConnection");
            }
            else
            {
                result.Value.Send<ChangeConnectionMessage>(msg);
            }
        }
    }

    private void ChangePlayersToCombatServer(string ip, string puerto, string[] tks)
    {
        ChangeConnectionMessage msg = new ChangeConnectionMessage();
        msg.Ip = ip;
        msg.Port = puerto;

        foreach (string tk in tks)
        {
            KeyValuePair<string, NetworkConnection> result = tokenConnection.SingleOrDefault(dupla => dupla.Key == tk);
            if (result.Equals(default(KeyValuePair<string, NetworkConnection>)))
            {
                print("No se ha encontrado token en tokenConnection");
            }
            else
            {
                result.Value.Send<ChangeConnectionMessage>(msg);
            }
        }
    }

    private void ChangePlayersToCombatServer(string puerto, List<int> indices) => ChangePlayersToCombatServer(GlobalVariables.matchmakingServerIP, puerto, indices);

    private void ChangePlayersToCombatServer(string puerto, string[] tks) => ChangePlayersToCombatServer(GlobalVariables.matchmakingServerIP, puerto, tks);

    private string GetTokens(List<int> indices)
    {
        string res = "";
        foreach (int indice in indices)
        {
            res += " " + tokens[indice];
        }
        return res;
    }

    private List<int> GetRemainingIndexes(List<int> indices, int max)
    {
        List<int> res = Enumerable.Range(0, max).ToList();
        foreach (int indice in indices)
        {
            if (res.Contains(indice))
            {
                res.Remove(indice);
            }
        }
        return res;
    }

    private void StartCombat()
    {
        rondaAJugar++;

        string ip = IPManager.GetIP(ADDRESSFAM.IPv4);

        string puerto = puertoActual.ToString();
        string argumentos = CombatServerIPs[0] + " " + puerto + " 1 " + ip + " " + (puertoInicial - 1).ToString(); // Lo de ip y puerto inicial es para que el server de combate sepa a donde conectarse de vuelta
        // El primer argumento es pal ssh que cree el server donde debe hacerlo

        if (PruebaUno)
        {
            argumentos += " " + tokens[0];
            IncrementPort();

            ExecuteCommand(GlobalVariables.combatServerScriptPath, argumentos); // argumentos: puerto server ip puertooriginal tokens

            List<int> listaUno = new List<int>();
            listaUno.Add(0);
    
            ChangePlayersToCombatServer(CombatServerIPs[0], puerto, listaUno);

            RoomPlayers.Clear();
            return;
        }

        int tam = maxPlayers / 2;
        List<int> indicesRandom = new List<int>();
        System.Random rnd = new System.Random();

        for (int i = 0; i < tam; ++i)
        {
            int indice;
            do
            {
                indice = rnd.Next(0, maxPlayers);
            } while (indicesRandom.Contains(indice)); // Que no esté el número
            indicesRandom.Add(indice);
        }

        List<int> restoIndices = GetRemainingIndexes(indicesRandom, maxPlayers);

        argumentos += GetTokens(indicesRandom);
        IncrementPort();

        ExecuteCommand(GlobalVariables.combatServerScriptPath, argumentos);
        ChangePlayersToCombatServer(CombatServerIPs[0], puerto, indicesRandom); // ChangePlayersToCombatServer(puerto, 0, tam);

        puerto = puertoActual.ToString();
        argumentos = CombatServerIPs[1] + " " + puerto + " 1 " + ip + " " + (puertoInicial - 1).ToString();
        argumentos += GetTokens(restoIndices);
        IncrementPort();

        ExecuteCommand(GlobalVariables.combatServerScriptPath, argumentos);
        ChangePlayersToCombatServer(CombatServerIPs[1], puerto, restoIndices);

        RoomPlayers.Clear();
    }

    private void StartCombat(string[] players)
    {
        rondaAJugar++;

        string ip = IPManager.GetIP(ADDRESSFAM.IPv4);
        
        string puerto = puertoActual.ToString();
        string argumentos = CombatServerIPs[0] + " " + puerto + " 1 " + ip + " " + (puertoInicial - 1).ToString() + " "; // Lo de puerto inicial es para que el server de combate sepa a donde conectarse de vuelta

        foreach (var tk in players)
        {
            argumentos += tk + " ";
        }
        IncrementPort();

        ExecuteCommand(GlobalVariables.combatServerScriptPath, argumentos);
        ChangePlayersToCombatServer(CombatServerIPs[0], puerto, players);

        RoomPlayers.Clear();
    }

    public override void OnServerReady(NetworkConnection conn)
    {
        base.OnServerReady(conn);
        if (SceneManager.GetActiveScene().name.StartsWith("Combat"))
        {
            OnServerReadied?.Invoke(conn);
        }
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        if (sceneName.StartsWith("Combat"))
        {
            GameObject playerSpawnSystemInstance = Instantiate(playerSpawnSystem);
            NetworkServer.Spawn(playerSpawnSystemInstance); // no le pasamos conexion porque así el servidor tiene autoridad (no se conecta a ningun servidor)

            GameObject timerInstance = Instantiate(timerPrefab); // Empezamos Timer.
            NetworkServer.Spawn(timerInstance);

            Instantiate(manaRegenerationPrefab);
        }
    }


    /****************************************************************************/
    /**************       FUNCIONES PARA SERVIDOR COMBATE         ***************/
    /****************************************************************************/

    [Server]
    private void ExecuteCommand(string command, string arguments)
    {
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

    public int GetDeadPlayers() => managerClasification.deadPlayers;
    public void PlayerDead()
    {
        int deadPlayers = managerClasification.PlayerDead();
        if (deadPlayers >= maxPlayers - 1) // Si solo queda 1 jugador, terminamos la pelea
        {
            EndCombat();
        }
    }

    private void StartGame()
    {
        if (SceneManager.GetActiveScene().path == lobbyScene)
        {
            ServerChangeScene("Combat");
        }
    }

    /****************************************************************************/
    /**************      FUNCIONES PARA MENSAJES EN SERVIDOR      ***************/
    /****************************************************************************/

    private void OnServerTokenMessageHandler(NetworkConnection conn, TokenMessage msg)
    {
        print("Recibido TokenMessage: (" + msg.Token + ")");
        StartCoroutine(OnServerTokenMessageCoroutine(conn, msg));
    }
    private IEnumerator OnServerTokenMessageCoroutine(NetworkConnection conn, TokenMessage msg)
    {
        if (tokens.Contains(msg.Token)) // Si el token coincide con uno que nos hayan pasado
        {
            if (modoServidor == ServidorPartida)
            {
                ChangeUIMessage uimsg = new ChangeUIMessage(UI_Prepartida);
                conn.Send<ChangeUIMessage>(uimsg);

                KeyValuePair<string, NetworkConnection> dupla = new KeyValuePair<string, NetworkConnection>(msg.Token, conn);
                tokenConnection.Add(dupla);

                serverPlayers++;
                print("OnServerTokenMessage: serverPlayers: " + serverPlayers + " --- MaxPlayers: " + maxPlayers);
                if (serverPlayers == maxPlayers) // Si estamos todos los jugadores
                {
                    int i = 0;
                    while (!clasificacionActualizada && i < intentosConexion) // Esperar a la clasificacion es mejor que esperar a los jugadores
                    {
                        ++i;
                        yield return new WaitForSeconds(tiempoEsperaConexion);
                    }

                    if (!clasificacionActualizada)
                    {
                        print("Agotado tiempo de espera, mensaje de clasificación no recibido");
                        rondaAJugar--; // ¿Repetir la ronda? 
                    }

                    clasificacionOrdenada = managerClasification.GetClasificacionOrdenada();
                    TablaClasificacionMessage clasificacionMessage = new TablaClasificacionMessage();
                    List<string> tokensOrdenados = new List<string>();
                    List<int[]> resultados = new List<int[]>();
                    foreach (var kvp in clasificacionOrdenada)
                    {
                        tokensOrdenados.Add(kvp.Key);
                        resultados.Add(kvp.Value);
                    }
                    clasificacionMessage.Tokens = tokensOrdenados;
                    clasificacionMessage.Resultados = resultados;

                    if (rondaAJugar == maxRondas) // Toca la final
                    {
                        bool necesitaDesempate = false;
                        int puntuacionAux = clasificacionOrdenada[maxPlayers / 2].Value[3];

                        for (i = 0; i < maxPlayers / 2; ++i)
                        {
                            /*
                            maxPlayers/2 porque son los que entrarian en la final, empates despues de eso nos la suda.
                            SOLO nos interesa el primero que se queda fuera para ver si hay desempate, ya que hacemos otra ronda entre todos.
                            */
                            print(clasificacionOrdenada[i].Value[3] + " == " + puntuacionAux);
                            if (clasificacionOrdenada[i].Value[3] == puntuacionAux)
                            {
                                necesitaDesempate = true;
                                break;
                            }
                        }


                        if (necesitaDesempate)
                        {
                            print("Hay desempate");
                            rondaAJugar--; // Se juega otra ronda
                            NetworkServer.SendToAll<TablaClasificacionMessage>(clasificacionMessage);
                        }
                        else
                        {
                            print("No hay desempate, se juega final");
                            foreach (var tk in tokens)
                            {
                                int index = clasificacionOrdenada.FindIndex(p => p.Key == tk);
                                if (index >= maxPlayers / 2) // Si no entra en la final
                                {
                                    GameEndedMessage result = new GameEndedMessage();
                                    result.Puesto = index;
                                    NetworkConnection connection = tokenConnection.Find(p => p.Key == tk).Value;
                                    connection.Send<GameEndedMessage>(result);
                                }
                                else if (index >= 0) //Entra en la final
                                {
                                    NetworkConnection connection = tokenConnection.Find(p => p.Key == tk).Value;
                                    connection.Send<TablaClasificacionMessage>(clasificacionMessage);
                                }
                                else
                                {
                                    print("Usuario no encontrado");
                                }
                            }
                        }
                    }
                    else if (rondaAJugar > maxRondas) // Si estamos todos y ya no toca nada más, pasamos mensaje a los de la final
                    {
                        GameEndedMessage gameEndedMessage = new GameEndedMessage();
                        for (i = 0; i < maxPlayers; ++i) // maxPlayers es el maxPlayers original dividido por 2
                        {
                            gameEndedMessage.Puesto = i;
                            string tk = clasificacionOrdenada[i].Key;
                            NetworkConnection connection = tokenConnection.Find(p => p.Key == tk).Value;
                            connection.Send<GameEndedMessage>(gameEndedMessage);
                        }

                        partidaFinalizada = true; // Oficialmente hemos acabado la partida
                    }
                    else
                    {
                        NetworkServer.SendToAll<TablaClasificacionMessage>(clasificacionMessage);
                        print("Se envía mensaje de tabla de clasificación");
                    }
                }
            }
            else if (modoServidor == ServidorCombate)
            {
                KeyValuePair<string, NetworkConnection> dupla = new KeyValuePair<string, NetworkConnection>(msg.Token, conn);
                tokenConnection.Add(dupla);

                CombatUsersMessage cumsg = new CombatUsersMessage();
                cumsg.Users = users;
                conn.Send<CombatUsersMessage>(cumsg);
            }
        }
        else // Si no es correcto, conn.Disconnect()
        {
            print("Token no encontrado");
            conn.Disconnect();
        }
    }

    private void OnServerCombatResultMessageHandler(NetworkConnection conn, CombatResultMessage msg)
    {
        print("Recibido CombatResultMessage: (" + msg.PlayerTokens[0] + "," + msg.PlayerTokens[1] + ")");

        managerClasification.IncrementCell(msg.PlayerTokens[0], 0); // Estructura es token, {primero, segundo, tercero, puntuaciontotal}
        managerClasification.IncrementCell(msg.PlayerTokens[1], 2);
        clasificacionOrdenada = managerClasification.GetClasificacionOrdenada();

        indexClasificacionActualizada++;
        print("rondaAJugar: " + rondaAJugar + " --- maxRondas: " + maxRondas);
        if ((rondaAJugar <= maxRondas && indexClasificacionActualizada == 2) || rondaAJugar > maxRondas)
        {
            clasificacionActualizada = true;
            indexClasificacionActualizada = 0;
        }

        conn.Send<ResultReceivedMessage>(new ResultReceivedMessage());
    }

    private void NextRound()
    {
        if (rondaAJugar == maxRondas) // Toca final
        {
            maxPlayers /= 2;  // Lo divido entre dos porque a los otros dos no los tenemos ya en cuenta.
            string[] playersFinal = new string[maxPlayers];
            for (int i = 0; i < maxPlayers; ++i)
            {
                playersFinal[i] = clasificacionOrdenada[i].Key;
            }

            clasificacionActualizada = false;
            StartCombat(playersFinal);
        }
        else if (rondaAJugar < maxRondas)
        {
            print("Se juega ronda " + rondaAJugar);
            clasificacionActualizada = false;
            StartCombat();
        }
    }

    //----------------------------------------------------------------------------------

    private void printList(List<int> list)
    {
        foreach (int element in list)
        {
            print(element);
        }
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

    public static void SetActiveRecursively(GameObject obj, bool state)
    {
        obj.SetActive(state);
        foreach (Transform child in obj.transform)
        {
            SetActiveRecursively(child.gameObject, state);
        }
    }

    public static void SetActiveChildren(GameObject obj, bool state)
    {
        foreach (Transform child in obj.transform)
        {
            SetActiveRecursively(child.gameObject, state);
        }
    }

    public void SetDebugText(string text, bool disappear)
    {
        if (disappear)
        {
            StartCoroutine(SetDebugTextCoroutine(text));
        }
        else
        {
            debugText.text = text;
        }
    }

    private IEnumerator SetDebugTextCoroutine(string text)
    {
        debugText.text = text;
        yield return new WaitForSeconds(3f);
        debugText.text = "";
    }
}
