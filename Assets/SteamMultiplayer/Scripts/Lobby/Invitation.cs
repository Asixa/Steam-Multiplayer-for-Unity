using System;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;

namespace SteamMultiplayer
{
    public class Invitation : MonoBehaviour
    {
        public static Invitation instance;
        public Text text;
        public GameObject Panel;
        private CSteamID lobby;

        public void Awake()
        {
            instance = this;
        }

        public void Start()
        {
            var CommandLine = Environment.CommandLine;
            if (CommandLine.Contains("+connect_lobby"))
            {
                var a = Environment.GetCommandLineArgs();
                var id=new CSteamID(ulong.Parse(a[a.Length-1]));
                NetworkLobbyManager.instance.JoinLobby(id);
            }
            
        }

        public void ShowInvite(CSteamID id, CSteamID lobby)
        {
            Panel.SetActive(true);
            text.text = SteamFriends.GetFriendPersonaName(id) + "邀请你加入游戏";
            this.lobby = lobby;
        }

        public void Accept()
        {
            NetworkLobbyManager.instance.JoinLobby(lobby);
        }
    }
}
