using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using System;

public class PlayerList : MonoBehaviour
{

    static PlayerList thePlayerList = null;
    public static List<Player> Players { get { return thePlayerList._players; } set { thePlayerList._players = value; } }

    List<Player> _players = new List<Player>();

    public const int MIN_PLAYERS = 2;
    public const int MAX_PLAYERS = 4;

    void OnEnable()
    {
        thePlayerList = this;
    }
    void OnDestroy()
    {
        thePlayerList = null;
    }

    public static Player playerAtPosition(int position)
    {
        return Players.Find(p => p.Position == position);
    }

    public static Player nextPlayer(Player player)
    {
        return playerLeftOfPlayer(player);
    }

    public static Player playerLeftOfPlayer(Player player)
    {
        int pIndex = Players.IndexOf(player);
        int index = (pIndex == Players.Count - 1) ? 0 : pIndex + 1;
        return Players[index];
    }

    public static Player playerRightOfPlayer(Player player)
    {
        int pIndex = Players.IndexOf(player);
        int index = (pIndex == 0 ? Players.Count - 1 : pIndex - 1);
        return Players[index];
    }

    public static void setOrderToClockwiseWithStartAt(Player player)
    {
        thePlayerList._players =
          thePlayerList._players.OrderBy(
            p => (p.Position - player.Position + MAX_PLAYERS) % MAX_PLAYERS).ToList();
    }

    public static List<Player> GetAllPlayers()
    {
        return thePlayerList._players;
    }
    
    public static Player? GetPlayerByRole(Player.Roles? role)
    {
        return thePlayerList._players.Find(p => p.Role == role);
    }
}
