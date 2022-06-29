using UnityEngine;
using UnityEngine.UI;
using Mirror;
using MessagesTFG;

public class FinPartidaButtons : MonoBehaviour
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

    public Button botonBuscarPartida;
    public Button botonCerrarSesion;

    // Start is called before the first frame update
    void Start()
    {
        botonBuscarPartida.onClick.AddListener(delegate ()
        {
            Room.BuscarPartida();
        });

        botonCerrarSesion.onClick.AddListener(delegate ()
        {
            Room.CerrarSesion();
        });
    }
}
