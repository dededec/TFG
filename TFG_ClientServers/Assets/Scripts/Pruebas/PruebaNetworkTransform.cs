using System.IO;
using UnityEngine;
using Mirror;


public class PruebaNetworkTransform : NetworkBehaviour
{
    private NetworkManagerTFG room;
    private NetworkManagerTFG Room
    {
        get
        {
            if (room != null) { return room; }
            return room = NetworkManager.singleton as NetworkManagerTFG;
        }
    }
    public static string DebugFilePath = "/home/david/Escritorio/TFG/Pruebas/01_NetworkTransform/";
    private StreamWriter sw;
    public Transform TransformJugador;
    private Vector3 lastPosition;
    private float tiempoDesdeUltimoCambio;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!isLocalPlayer)
        {
            enabled = false;
        }
        else
        {
            lastPosition = RoundVector(TransformJugador.position);
            TokenDecoded aux = new TokenDecoded(Room.token, verify: false);

            DebugFilePath = "/home/david/Escritorio/TFG/Pruebas/01_NetworkTransform/" + "debug_" + aux.userTag + ".txt";
            sw = File.AppendText(DebugFilePath);
            // sw.AutoFlush = true;
            sw.WriteLine("x nueva\tx antigua\ttiempo hasta cambio");
        }
    }

    private void OnDisable()
    {
        if (!isLocalPlayer) return;

        sw.Close();
        DebugFilePath = "/home/david/Escritorio/TFG/Pruebas/01_NetworkTransform/";
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer) return;
        

        // Vector3 posActual = TransformJugador.position;
        Vector3 posActual = RoundVector(TransformJugador.position);
        if (lastPosition.x != posActual.x)
        {
            sw.WriteLine(posActual.x + "\t" + lastPosition.x + "\t" + tiempoDesdeUltimoCambio);
            lastPosition = posActual;
            tiempoDesdeUltimoCambio = 0f;
        }
        else
        {
            tiempoDesdeUltimoCambio += Time.deltaTime;
        }
    }

    private Vector3 RoundVector(Vector3 v)
    {
        v.x = Mathf.Round(v.x * 100f) / 100f;
        v.y = Mathf.Round(v.y * 100f) / 100f;
        v.z = Mathf.Round(v.z * 100f) / 100f;

        return v;
    }
}




