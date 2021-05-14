using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Diagnostics;
using kcp2k;
using MessagesTFG;

public class NetworkManagerTFG : NetworkManager
{
    // 0 - Servidor Partida 
    // 1 - Servidor Combate
    // 2 - Cliente
    [Header("Tipo de servidor")]
    [SerializeField] private int modoServidor = 0;
    [SerializeField] private string puertoOriginal = "7777";
    private int puertoInicial = 7777;
    private int puertoActual = 7777;
    private int puertoLimite = 9777;

    //---------------------------------------------------------------------

    [Header("Scenes")]
    [Scene] [SerializeField] private string lobbyScene = string.Empty;
    [Scene] [SerializeField] private string menuScene = string.Empty;
    [Scene] [SerializeField] private string combatScene = string.Empty;

    //---------------------------------------------------------------------

    [Header("Lobby")]
    [SerializeField] private RoomPlayer roomPlayerPrefab = null;
    public List<RoomPlayer> RoomPlayers { get; } = new List<RoomPlayer>();
    public int maxPlayers;

    //---------------------------------------------------------------------

    [Header("Game")]
    [SerializeField] private GameObject playerSpawnSystem = null;
    [SerializeField] private GameObject timerPrefab = null;
    // [SerializeField] private GamePlayer gamePlayerPrefab = null;
    public List<GamePlayer> GamePlayers { get; } = new List<GamePlayer>();

    /*
    Quiero que esto lo creen los servers
    Que aguante aunque hagan StopServer y luego StartClient
    para poder cambiar, desde el server de combate, la clasificación en el servidor de partida
    */
    public ClasificationManager ManagerClasification = null;
    public GameObject ClasificacionManagerPrefab = null;

    //---------------------------------------------------------------------

    public static event Action OnClientConnected;
    public static event Action OnClientDisconnected;
    public static event Action<NetworkConnection> OnServerReadied;

    //---------------------------------------------------------------------


    public override void Start()
    {
        string[] args = System.Environment.GetCommandLineArgs(); // El primer elemento es el nombre del ejecutable

        if (args.Length > 2)
        {
            (transport as KcpTransport).Port = Convert.ToUInt16(args[1]); //UInt16 = ushort
            modoServidor = Convert.ToInt32(args[2]);

            if (modoServidor == 1)
            {
                maxConnections = maxConnections / 2;
            }
        }
        else if (args.Length == 1) // Es cliente
        {
            (transport as KcpTransport).Port = Convert.ToUInt16(7777);
            modoServidor = 2;
        }

        base.Start();
    }

    // Llamada desde Timer cuando TimeRemaining == 0
    // Lo hago así para evitar usar Update aquí todo el rato
    public void EndGame()
    {

        var aux = GamePlayers.ToList();
        GamePlayers.Clear();

        foreach (var gamePlayer in aux.ToList())
        {
            aux.Remove(gamePlayer);
            gamePlayer.ChangeConnection(puertoOriginal);

            // yield return new WaitForSeconds(2f);
        }

        /*
        StopServer();
        ChangePort(puertoOriginal);
        StartClient();
        // CombatResultMessage resultado = new CombatResultMessage()
        // {
        //     PlayerNames = new string[];
        // };

        // NetworkClient.Send<CombatResultMessage>(resultado, 0);
        StopClient();
        */

        Application.Quit(12345);
    }

    [Client]
    public void StartChangeConnection(string port)
    {
        if(!NetworkClient.active) return;
        StartCoroutine(ChangeConnection(port));
    }

    [Client]
    public IEnumerator ChangeConnection(string port)
    {
        // 1. Desconectar del server
        StopClient();
        print("Cliente parado");
        yield return new WaitForSeconds(5f);

        // 2. Cambiar puerto
        ChangePort(port);
        print("Puerto cambiado");

        // 3. Conectar de nuevo con StartClient()
        StartClient();
        print("Cliente empezado");
        yield return new WaitForSeconds(5f);
    }


    // Llamamos a esto cuando el server comienza (o el host)
    public override void OnStartServer()
    {
        spawnPrefabs = Resources.LoadAll<GameObject>("SpawneablePrefabs").ToList();
        maxPlayers = maxConnections;
    }

    public override void OnStopServer()
    {
        RoomPlayers.Clear();
        GamePlayers.Clear();
        base.OnStopServer();
    }

    public override void OnStartClient()
    {
        var spawneablePrefabs = Resources.LoadAll<GameObject>("SpawneablePrefabs");
        foreach (var prefab in spawneablePrefabs)
        {
            ClientScene.RegisterPrefab(prefab);
        }
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
        OnClientConnected?.Invoke(); // Lanzamos el evento con lo que queramos hacer
    }
    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);
        ChangePort(puertoOriginal); // Cambiamos al puerto original de partida
        OnClientDisconnected?.Invoke(); // Lanzamos el evento con lo que queramos hacer
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
        if (conn.identity != null)
        {
            var player = conn.identity.GetComponent<RoomPlayer>();
            RoomPlayers.Remove(player);
        }
        base.OnServerDisconnect(conn);
    }

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        if (modoServidor == 0) // Servidor Partida
        {
            if (SceneManager.GetActiveScene().path == lobbyScene)
            {
                RoomPlayer roomPlayerInstance = Instantiate(roomPlayerPrefab);

                NetworkServer.AddPlayerForConnection(conn, roomPlayerInstance.gameObject); // Esto se encarga de spawnearlo en todos los clientes también

                RoomPlayers.Add(roomPlayerInstance);

                if (numPlayers == maxPlayers)
                {
                    StartCombat();
                }
            }
        }
        else if (modoServidor == 1) // Servidor Combate
        {
            if (SceneManager.GetActiveScene().path == lobbyScene)
            {
                RoomPlayer roomPlayerInstance = Instantiate(roomPlayerPrefab);
                NetworkServer.AddPlayerForConnection(conn, roomPlayerInstance.gameObject); // Esto se encarga de spawnearlo en todos los clientes también

                RoomPlayers.Add(roomPlayerInstance);

                print("Jugadores conectados: " + numPlayers + " - MaxConnections: " + maxConnections);
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
    
    private void ChangePlayersToCombatServer(string puerto, int inicio, int fin)
    {
        List<RoomPlayer> aux = RoomPlayers.GetRange(inicio, fin);
        foreach (var roomPlayer in aux.ToList())
        {
            roomPlayer.ChangeConnection(puerto);
            // yield return new WaitForSeconds(2f);
        }
    }

    private void StartCombat()
    {
        int tam = RoomPlayers.Count / 2;

        string puerto = puertoActual.ToString();
        string argumentos = puerto + " 1";
        IncrementPort();

        ExecuteCommand("/home/david/Escritorio/GameDev/ProyectosUnity/TFG_AllInOne/Builds/CuartaBuild_10segundos/iniciarServerCombate.sh", argumentos);
        ChangePlayersToCombatServer(puerto, 0, tam);

        puerto = puertoActual.ToString();
        argumentos = puerto + " 1";
        IncrementPort();

        ExecuteCommand("/home/david/Escritorio/GameDev/ProyectosUnity/TFG_AllInOne/Builds/CuartaBuild_10segundos/iniciarServerCombate.sh", argumentos);
        ChangePlayersToCombatServer(puerto, tam, tam);

        RoomPlayers.Clear();
    }

    private void StartGame()
    {
        if (SceneManager.GetActiveScene().path == lobbyScene)
        {
            ServerChangeScene("Combat");
        }
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
        if (sceneName.StartsWith("Lobby"))
        {
            ManagerClasification = Instantiate(ClasificacionManagerPrefab).GetComponent<ClasificationManager>();
        }

        if (sceneName.StartsWith("Combat"))
        {
            GameObject playerSpawnSystemInstance = Instantiate(playerSpawnSystem);
            NetworkServer.Spawn(playerSpawnSystemInstance); // no le pasamos conexion porque así el servidor tiene autoridad (no se conecta a ningun servidor)

            GameObject timerInstance = Instantiate(timerPrefab); // Empezamos Timer.
            NetworkServer.Spawn(timerInstance);
        }
    }

    public void ChangePort(string port)
    {
        (transport as kcp2k.KcpTransport).Port = Convert.ToUInt16(port);
    }
    public ushort GetPort()
    {
        return (transport as kcp2k.KcpTransport).Port;
    }

    
    /**************************************************************************/
    /**************       FUNCION PARA SERVIDOR COMBATE         ***************/
    /**************************************************************************/

    [Server]
    private void ExecuteCommand(string command, string arguments)
    {

        // var processInfo = new ProcessStartInfo("cmd.exe", "/c " + command);
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

        /*
        NO podemos hacer WaitForExit, ya que entonces no podemos decirle a los clientes que se tienen que conectar
        a otro lado. Es más, solo se crea un servidor de combate en ese caso.
        */
        // process.WaitForExit();
        // Console.WriteLine("ExitCode: {0}", process.ExitCode);
        // process.Close();

        /*
        IDEA: Lanzar el proceso y empezar una coroutine de 2 minutos aprox (duración de una partida).
        A los dos minutos procesamos el ExitCode y terminamos el proceso.
        Los jugadores pueden que estén sin hacer nada unos segundos, después se les manda a otra partida.

        PROBLEMA: Super poco fiable porque lo mismo son 2 minutos o lo mismo son 3 con cosas de conexiones.
        Tener al servidor de partida congelado esperando es un poco una mierda, pero es verdad que el servidor
        de partida no tiene otra cosa que hacer entre tanto (CREO, lo mismo hay que hablarlo con Juanjo).
        */
    }

}
