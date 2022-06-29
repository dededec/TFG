using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;

public class DataBase : MonoBehaviour
{
    private Dictionary<string, List<int>> dataBase = new Dictionary<string, List<int>>();

    // ------------------ OPERACIONES DE BASE DE DATOS ------------------ 

    public void Insert(string id, List<int> data)
    {
        if(!dataBase.ContainsKey(id))
        {
            dataBase.Add(id, data);
        }
    }

    public void Delete(string id)
    {
        if(dataBase.ContainsKey(id))
        {
            dataBase.Remove(id);
        }
    }

    public void UpdateEntry(string id, List<int> newData)
    {
        if(dataBase.ContainsKey(id))
        {
            dataBase[id] = newData;
        }
    }

    public List<int> Get(string id)
    {
        if(dataBase.ContainsKey(id))
        {
            return dataBase[id];
        }
        else
        {
            return null;
        }
    }

    // No hay SELECT porque no lo veo necesario

    // ------------------ OPERACIONES DE BASE DE DATOS ------------------ 

    // public void SaveGame() {
    //     Save save = new Save();
    //     save.estadoEnable = estadoEnable;
    //     save.puntuacion = puntuacion;
    //     BinaryFormatter bf = new BinaryFormatter();
    //     FileStream file = File.Create(Application.persistentDataPath + "/gamesave.save");
    //     bf.Serialize(file, save);
    //     file.Close();

    //     Debug.Log("Guardado");
    // }

    // public void LoadGame() {
    //     if(!(File.Exists(Application.persistentDataPath + "/gamesave.save"))) {
    //         Debug.Log("No hay guardado");
    //         return;
    //     }

    //     BinaryFormatter bf = new BinaryFormatter();
    //     FileStream file = File.Open(Application.persistentDataPath + "/gamesave.save", FileMode.Open);
    //     Save save = (Save)bf.Deserialize(file);
    
    //     file.Close();
        
    //     int i=0;
    //     foreach(var tick in sprites) {
    //         tick.enabled = save.estadoEnable[i];
    //         estadoEnable[i] = save.estadoEnable[i];
    //         ++i;
    //     }
    //     puntuacion = save.puntuacion;
    //     textoPuntuacion.text = "Puntuacion: " + puntuacion;

    //     Debug.Log("Cargado");
    // }
}
