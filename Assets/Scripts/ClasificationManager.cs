using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ClasificationManager : NetworkBehaviour
{
    private static Dictionary<string, List<int>> clasificacion = null; // Identificador - [1er puesto, 2º puesto, 3er puesto, puntos totales]
    
    private void Start()
    {
        DontDestroyOnLoad(transform.root.gameObject);
    }
    
    private void IncrementCell(string id, int resultado)
    {
        clasificacion[id][resultado]++;
    }

    private int CalcularPuntuacion(List<int> puestos)
    {
        int res = puestos[0] * 3;
        res += puestos[1] * 1;

        return res;
    }

    [Command]
    public void IncrementClasification(string id, int index)
    {
        clasificacion[id][index]++;
    }

}
