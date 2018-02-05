using System.Collections;
using System.Collections.Generic;
using SteamMultiplayer.Lobby;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public void Left()
    {
        NetworkLobbyManager.instance.LeaveLobby();
    }
}
