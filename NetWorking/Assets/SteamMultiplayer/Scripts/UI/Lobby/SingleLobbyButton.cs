using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;
public class SingleLobbyButton : MonoBehaviour
{
    public Text ID, Name, MumberCount;
    public Button button;
    public CSteamID server_id;

    public void Join()
    {
        NetworkLobbyManager.instance.JoinLobby(server_id);
    }
}
