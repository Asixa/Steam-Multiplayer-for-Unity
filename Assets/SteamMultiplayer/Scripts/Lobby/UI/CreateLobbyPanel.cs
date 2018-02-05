using System.Collections;
using System.Collections.Generic;
using SteamMultiplayer.Lobby;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;

public class CreateLobbyPanel : MonoBehaviour
{
    public InputField name;
    public Toggle Public_lobby, Friend_lobby;
    public ELobbyType currentType;
    public Text MumberCountShow;
    public Slider MumberCount;

    public void Create()
    {
        NetworkLobbyManager.instance.CreateLobby((int)MumberCount.value,
            Public_lobby.isOn
            ?ELobbyType.k_ELobbyTypePublic 
            : ELobbyType.k_ELobbyTypeFriendsOnly,
            name.text==""
            ?SteamFriends.GetPersonaName()+"'s Lobby"
            :name.text);
    }

    public void SildeChange()
    {
        MumberCountShow.text = MumberCount.value + " players";
    }
}
