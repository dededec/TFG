using UnityEngine;

    public class PlayerData
    {
        public string player_tag;
        public string password; // Esto se tendrá que guardar cifrado o algo
        public int partidas_ganadas;
        public int partidas_ultimo;
        public float puesto_medio;

        public string Stringify()
        {
            return JsonUtility.ToJson(this);
        }
        public static PlayerData Parse(string json)
        {
            return JsonUtility.FromJson<PlayerData>(json);
        }

        public PlayerData(string player_tag, string password)
        {
            this.player_tag = player_tag;
            this.password = password;
            
            partidas_ganadas = 0;
            partidas_ultimo = 0;
            puesto_medio = 3f; // Si son 6 jugadores, 6/2 = 3f - Mitad de tabla
        }
    }