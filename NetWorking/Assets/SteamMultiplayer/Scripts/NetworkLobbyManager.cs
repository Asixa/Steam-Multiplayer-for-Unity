using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Steamworks;
using System.Text;
using UnityEngine.Events;
using UnityEngine.UI;

public class NetworkLobbyManager : MonoBehaviour {
    [Serializable]
    public struct NetworkOpinion_s
    {
        public int Port;
        public int TimeoutSec;
    }
    [Layout]
    public NetworkOpinion_s NetworkOpinion=new NetworkOpinion_s
    {
        Port = 233,TimeoutSec = 300
    };

    [Serializable]
    public struct SpawnInfo_s
    {
        public List<GameObject> SpawnablePrefab;
    }

    [Layout] public SpawnInfo_s SpawnInfo;


    private CSteamID lobby;

    CallResult<LobbyEnter_t> CLobbyJoin = new CallResult<LobbyEnter_t>();       //加入
    CallResult<LobbyCreated_t> CLobbyCreator = new CallResult<LobbyCreated_t>();//创建

    protected Callback<LobbyChatUpdate_t> m_LobbyChatUpdate;// 聊天面板
    protected Callback<LobbyChatMsg_t> m_LobbyChatMsg;      // 聊天面板

    [HideInInspector]
    public string chatstring = null;

    public static NetworkLobbyManager instance;
    public NetworkLobbyManager()
    {
        if (instance != null)
        {
            throw new Exception("There are more than one manager in the scene");
        }
        instance = this;
    }

    void Start ()
	{
	    SteamAPI.Init();
	    ChatInit();
	}

    void Update()
    {
        SteamAPI.RunCallbacks();
    }

    public void JoinLobby(CSteamID lobby)
    {
        this.lobby=lobby;
        CLobbyJoin.Set(SteamMatchmaking.JoinLobby(this.lobby), OnLobbyJoined);
    }

    public void LeaveLobby()
    {
        SendChatMessage(SteamFriends.GetPersonaName() + " Lefted the Lobby",false);
        SteamMatchmaking.LeaveLobby(lobby);
        if (lobby_leaved != null) lobby_leaved.Invoke();
    }

    public void CreateLobby()
    {
        var mumber_count = 8;
        var lobby_type = ELobbyType.k_ELobbyTypePublic;
        CLobbyCreator.Set(SteamMatchmaking.CreateLobby(lobby_type, mumber_count), OnLobbyCreated);
    }

    public void SendChatMessage(string t,bool Chat=true)
    {
        string content=Chat? DateTime.Now.ToString("HH:mm:ss") + " "+SteamFriends.GetPersonaName()+": " + t :t;
        SteamMatchmaking.SendLobbyChatMsg(lobby, Encoding.Default.GetBytes(content), content.Length + 1);
    }

    //*************初始化*************

    void ChatInit()
    {
        m_LobbyChatUpdate = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);
        m_LobbyChatMsg = Callback<LobbyChatMsg_t>.Create(OnLobbyChatMsg);
    }

    //*************回调*************

    void OnLobbyChatUpdate(LobbyChatUpdate_t pCallback)
    {
        Debug.Log("[" + LobbyChatUpdate_t.k_iCallback + " - LobbyChatUpdate] - " + pCallback.m_ulSteamIDLobby + " -- " + pCallback.m_ulSteamIDUserChanged + " -- " + pCallback.m_ulSteamIDMakingChange + " -- " + pCallback.m_rgfChatMemberStateChange);
    }

    void OnLobbyChatMsg(LobbyChatMsg_t pCallback)
    {

       // Debug.Log("[" + LobbyChatMsg_t.k_iCallback + " - LobbyChatMsg] - " + pCallback.m_ulSteamIDLobby + " -- " + pCallback.m_ulSteamIDUser + " -- " + pCallback.m_eChatEntryType + " -- " + pCallback.m_iChatID);
        CSteamID SteamIDUser;
        var Data = new byte[4096];
        EChatEntryType ChatEntryType;
        var ret = SteamMatchmaking.GetLobbyChatEntry((CSteamID)pCallback.m_ulSteamIDLobby, (int)pCallback.m_iChatID, out SteamIDUser, Data, Data.Length, out ChatEntryType);
        // Debug.Log("GetLobbyChatEntry(" + (CSteamID)pCallback.m_ulSteamIDLobby + ", " + (int)pCallback.m_iChatID + ", out SteamIDUser, Data, Data.Length, out ChatEntryType) : " + ret + " -- " + SteamIDUser + " -- " + System.Text.Encoding.UTF8.GetString(Data) + " -- " + ChatEntryType);

        chatstring = Encoding.UTF8.GetString(Data);
        print(chatstring);
        if (lobby_chat_msg_recevied != null) lobby_chat_msg_recevied.Invoke(chatstring);
    }

    void OnLobbyCreated(LobbyCreated_t pCallbacks, bool bIOFailure)
    {
        lobby = new CSteamID(pCallbacks.m_ulSteamIDLobby);
        if (lobby_created != null) lobby_created.Invoke();
        JoinLobby(lobby);
    }

    void OnLobbyJoined(LobbyEnter_t pCallbacks, bool bIOFailure)
    {
        NetWorkingCore.CreateConnections(lobby);
        if (lobby_joined != null) lobby_joined.Invoke();
        SendChatMessage(SteamFriends.GetPersonaName()+" Joined the Lobby",false);
        //加入完成
    }

    //************委托************

    public delegate void LobbyEvent();
    public LobbyEvent lobby_joined;
    public LobbyEvent lobby_created;
    public LobbyEvent lobby_leaved;

    public delegate void LobbyChatMsgRecevied(string t);
    public LobbyChatMsgRecevied lobby_chat_msg_recevied;

}
