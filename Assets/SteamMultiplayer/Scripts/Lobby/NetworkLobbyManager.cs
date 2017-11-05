//                   _ooOoo_
//                  o8888888o
//                  88" . "88
//                  (| -_- |)
//                  O\  =  /O
//               ____/`---'\____
//             .'  \\|     |//  `.
//            /  \\|||  :  |||//  \
//           /  _||||| -:- |||||-  \
//           |   | \\\  -  /// |   |
//           | \_|  ''\---/''  |   |
//           \  .-\__  `-`  ___/-. /
//         ___`. .'  /--.--\  `. . __
//      ."" '<  `.___\_<|>_/___.'  >'"".
//     | | :  `- \`.;`\ _ /`;.`/ - ` : | |
//     \  \ `-.   \_ __\ /__ _/   .-` /  /
//======`-.____`-.___\_____/___.-`____.-'======
//                   `=---='
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
//           佛祖保佑       永无BUG
//  本类已经经过开光处理，绝无可能再产生bug
//=============================================
//本脚本用于大厅的创建和网络连接的数据获取
//创建者: Asixa 2017-9-x
//最新修改 Asixa 2017-10-29
//=============================================
using System;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using System.Text;
using SteamMultiplayer;
using UnityEngine.Events;

[AddComponentMenu("SteamMultiplayer/LobbyManager")]
[RequireComponent(typeof(SMC))]
public class NetworkLobbyManager : MonoBehaviour {

    public static NetworkLobbyManager instance;

    #region Variables
    //Network info struct
    [Serializable]
    public struct NetworkInfo
    {
        public int Port;
        public int TimeoutSec;
    }
    public NetworkInfo networkInfo=new NetworkInfo
    {
        Port = 233,TimeoutSec = 300
    };

    //List than contains all the prefab that can spawn
    public List<Identity> SpawnablePrefab = new List<Identity>();

    //Current Lobby
    //[HideInInspector]
    public CSteamID lobby;

    //SteamCallBakck
    CallResult<LobbyEnter_t> CLobbyJoin = new CallResult<LobbyEnter_t>();       //JoinLobby
    CallResult<LobbyCreated_t> CLobbyCreator = new CallResult<LobbyCreated_t>();//CreateLobby

    protected Callback<LobbyChatUpdate_t> m_LobbyChatUpdate;// Chat Massage
    protected Callback<LobbyChatMsg_t> m_LobbyChatMsg;      // Chat Message

    //Chating
    [HideInInspector]
    public string chatstring = null;

    #endregion

    #region Unity Reserved Functions

    public void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (instance != null)
        {
            throw new Exception("There are more than one manager in the scene");
        }
        instance = this;
    }

    void Start()
    {
        SteamAPI.Init();
        Spawnable_Object_Init();
        ChatInit();
    }

    void Update()
    {
        SteamAPI.RunCallbacks();
    }

    void OnApplicationQuit()
    {
        LeaveLobby();
    }

    #endregion

    #region Lobby Action
    public void JoinLobby(CSteamID lobby)
    {
        this.lobby = lobby;
        CLobbyJoin.Set(SteamMatchmaking.JoinLobby(this.lobby), OnLobbyJoined);
       
    }

    public void LeaveLobby()
    {
        SendChatMessage(SteamFriends.GetPersonaName() + " Lefted the Lobby", false);
        SMC.SendPacketsSafely(new P2PPackage(null,P2PPackageType.LeftLobby),false);
        SteamMatchmaking.LeaveLobby(lobby);
        lobby = new CSteamID();

        foreach (var t in SMC.instance.OnlineObjects)
        {
            if (t == null) continue;
            Destroy(t.gameObject);
        }
        SMC.instance.OnlineObjects.Clear();
        if (events.lobby_leaved != null) events.lobby_leaved.Invoke();
    }

    public void CreateLobby()
    {
        var mumber_count = 8;
        var lobby_type = ELobbyType.k_ELobbyTypePublic;
        CLobbyCreator.Set(SteamMatchmaking.CreateLobby(lobby_type, mumber_count), OnLobbyCreated);
    }
    #endregion

    #region Chat 
    public void SendChatMessage(string t, bool Chat = true)
    {
        string content = Chat ? DateTime.Now.ToString("HH:mm:ss") + " " + SteamFriends.GetPersonaName() + ": " + t : t;
        SteamMatchmaking.SendLobbyChatMsg(lobby, Encoding.Default.GetBytes(content), content.Length + 1);
    }
    #endregion

    #region Initate Functions
    void ChatInit()
    {
        m_LobbyChatUpdate = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);
        m_LobbyChatMsg = Callback<LobbyChatMsg_t>.Create(OnLobbyChatMsg);
    }

    public void Spawnable_Object_Init()
    {
        for (var i = 0; i < SpawnablePrefab.Count; i++)
        {
            SpawnablePrefab[i].SpawnID = i;
        }
    }
    #endregion

    #region CallBacks

    public void OnLobbyChatUpdate(LobbyChatUpdate_t pCallback)
    {
    }

    private void OnLobbyChatMsg(LobbyChatMsg_t pCallback)
    {
        CSteamID SteamIDUser;
        var Data = new byte[4096];
        EChatEntryType ChatEntryType;
        var ret = SteamMatchmaking.GetLobbyChatEntry((CSteamID)pCallback.m_ulSteamIDLobby, (int)pCallback.m_iChatID, out SteamIDUser, Data, Data.Length, out ChatEntryType);

        chatstring = Encoding.UTF8.GetString(Data);
        if (lobby_chat_msg_recevied != null) lobby_chat_msg_recevied.Invoke(chatstring);
    }

    private void OnLobbyCreated(LobbyCreated_t pCallbacks, bool bIOFailure)
    {
        pCallbacks.m_ulSteamIDLobby.ToString();
        lobby = new CSteamID(pCallbacks.m_ulSteamIDLobby);
        if (events.lobby_created != null) events.lobby_created.Invoke();
        JoinLobby(lobby);
    }

    private void OnLobbyJoined(LobbyEnter_t pCallbacks, bool bIOFailure)
    {
        SMC.CreateConnections(lobby);
        if (events.lobby_just_joined != null) events.lobby_just_joined.Invoke();
        SendChatMessage(SteamFriends.GetPersonaName() + " Joined the Lobby", false);
        SMC.instance.CheckJoinedLobby();
    }
    #endregion

    #region Unity Events
    [Serializable]
    public struct LobbyEvents
    {
        public UnityEvent lobby_just_joined;
        public UnityEvent lobby_created;
        public UnityEvent lobby_leaved;
    }

    public LobbyEvents events;
    public delegate void LobbyChatMsgRecevied(string t);
    public LobbyChatMsgRecevied lobby_chat_msg_recevied;
    #endregion

}
