using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ClasificationManager : MonoBehaviour
{
    private Dictionary<string, int[]> clasificacion = new Dictionary<string, int[]>(); // Identificador - [1er puesto, 2º puesto, 3er puesto, puntos totales]
    public string[] combatResult = new string[3];
    public int deadPlayers = 0;

    private NetworkManagerTFG room;
    private NetworkManagerTFG Room
    {
        get
        {
            if (room != null) { return room; }
            return room = NetworkManager.singleton as NetworkManagerTFG;
        }
    }

    private void Start() => DontDestroyOnLoad(transform.root.gameObject);

    public void AddNewPlayer(string token)
    {
        if (clasificacion.ContainsKey(token)) return;
        clasificacion.Add(token, new int[] { 0, 0, 0, 0 });
    }

    public void IncrementCell(string token, int resultado)
    {
        clasificacion[token][resultado]++;
        CalcularPuntuacion(token);
    }

    private int CalcularPuntuacion(int[] puestos)
    {
        int res = puestos[0] * 3;
        res += puestos[1] * 1;

        return res;
    }

    private void CalcularPuntuacion(string token) => clasificacion[token][3] = CalcularPuntuacion(clasificacion[token]);
    public int GetPuntuacion(string token) => clasificacion[token][3];

    // Devuelve los jugadores que ya han muerto
    [Server]
    public int PlayerDead()
    {
        // combatResult[1-deadPlayers] = token;
        deadPlayers++;
        return deadPlayers;
    }

    public List<KeyValuePair<string, int[]>> GetClasificacionOrdenada()
    {
        List<KeyValuePair<string, int[]>> clasificacionFinal = new List<KeyValuePair<string, int[]>>();
        List<string> tokens = Room.Tokens();

        foreach(string tk in tokens)
        {
            clasificacionFinal.Add(new KeyValuePair<string, int[]>(tk, clasificacion[tk]));
        }

        clasificacionFinal.Sort((p,q) => p.Value[3].CompareTo(q.Value[3]));
        clasificacionFinal.Reverse();
        return clasificacionFinal;
    }
}

