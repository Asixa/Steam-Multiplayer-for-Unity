using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;
using UnityEngine.UI;

public class LobbyPanel : MonoBehaviour
{
    public static LobbyPanel instance;
    [Serializable]
    public struct LobbyList
    {
        public GameObject Lobbylist;
    }
    [Layout]
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
    }
    [Layout]
    public LobbyRoom lobby_room;

    void Awake()
    {
        instance = this;
    }

    public void Start()
    {
        init();
    }

    void init()
    {
        NetworkLobbyManager.instance.lobby_chat_msg_recevied += UpdateChatPanel;
        NetworkLobbyManager.instance.events.lobby_joined.AddListener(LobbyJoined);
        NetworkLobbyManager.instance.events.lobby_leaved.AddListener(LobbyLeaved);
    }

    void LobbyJoined()
    {
        lobby_room.lobby_room.SetActive(true);
        lobby_list.Lobbylist.SetActive(false);
    }

    void LobbyLeaved()
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
        foreach (Text t in history)
        {if(t!=null)
            Destroy(t.gameObject);
        }
    }

    List<Text>history=new List<Text>();
    public void UpdateChatPanel(string t)
    {
        var new_chat_text = Instantiate(lobby_room.chat_prefab);
        new_chat_text.transform.parent = lobby_room.chat_container.transform;
        var size = lobby_room.chat_container.sizeDelta;
        lobby_room.chat_container.sizeDelta=new Vector2(size.x,size.y+ new_chat_text.GetComponent<RectTransform>().sizeDelta.y);
        new_chat_text.text =  t;
        new_chat_text.gameObject.SetActive(true);
        history.Add(new_chat_text);
    }
}
