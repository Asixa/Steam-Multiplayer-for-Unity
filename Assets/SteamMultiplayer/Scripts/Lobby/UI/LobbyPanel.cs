using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using SteamMultiplayer;
using UnityEngine.UI;

public class LobbyPanel : MonoBehaviour
{
    public static LobbyPanel instance;

    [Serializable]
    public struct LobbyList
    {
        public GameObject Lobbylist;
    }
    public LobbyList lobby_list;

    [Serializable]
    public struct LobbyRoom
    {
        public GameObject lobby_room;
        public Text chat_prefab;
        public RectTransform chat_container;
        public InputField input;
        public RectTransform PlayerListPanel;
        public PlayerListPrefab PlayerListPrefab;
        public List<PlayerListPrefab> Player_List;
        public GameObject Wating;
    }
    public LobbyRoom lobby_room;

    private void Awake()
    {
        instance = this;
    }

    public void Start()
    {
        Init();
    }

    public void Init()
    {
        NetworkLobbyManager.instance.lobby_chat_msg_recevied += UpdateChatPanel;
        NetworkLobbyManager.instance.events.lobby_just_joined.AddListener(LobbyConnecting);
        NetworkLobbyManager.instance.events.lobby_leaved.AddListener(LobbyLeaved);
        NetworkControl.instance.events.JoinedLobby.AddListener(LobbyJoined);
    }

    private void LobbyConnecting()
    {
        lobby_room.lobby_room.SetActive(true);
        lobby_room.Wating.SetActive(true);
        lobby_list.Lobbylist.SetActive(false);
    }

    private void LobbyJoined()
    {
        lobby_room.Wating.SetActive(false);
    }

    private void LobbyLeaved()
    {
        foreach (var t in lobby_room.Player_List)
        {
            if (t != null)
            {
                Destroy(t.gameObject);
            }
        }
        
        lobby_room.lobby_room.SetActive(false);
        lobby_list.Lobbylist.SetActive(true);
    }

    public void InputEnd(string t)
    {
        NetworkLobbyManager.instance.SendChatMessage(t);
        lobby_room.input.text = "";
    }

    public void LeaveLobby()
    {
        NetworkLobbyManager.instance.LeaveLobby();
        ClearChatPanel();
    }

    public void ClearChatPanel()
    {
        if(history.Count==0)return;
        foreach (var t in history)
        {if(t!=null)
            Destroy(t.gameObject);
        }
    }

    public List<Text>history=new List<Text>();

    public void UpdateChatPanel(string t)
    {
        var new_chat_text = Instantiate(lobby_room.chat_prefab);
        new_chat_text.transform.parent = lobby_room.chat_container.transform;
        var size = lobby_room.chat_container.sizeDelta;
        lobby_room.chat_container.sizeDelta=new Vector2(size.x,size.y+ new_chat_text.GetComponent<RectTransform>().sizeDelta.y);

        new_chat_text.text = t;
        new_chat_text.gameObject.SetActive(true);
        history.Add(new_chat_text);
    }
}
