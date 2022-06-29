using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMatchmaker : Matchmaker
{
    public override void Process()
    {
        if(Players.Count >= PlayersPerMatch) // Cuando llegue a 4 empieza una partida
        {
            List<string> usernames = new List<string>();
            foreach(var player in Players)
            {
                usernames.Add(player.player_tag);
            }
            Players.Clear();
            Room.StartMatch(usernames);
        }
    }
}
