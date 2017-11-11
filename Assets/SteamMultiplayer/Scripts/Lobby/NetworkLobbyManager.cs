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
using UnityEngine.SceneManagement;

[AddComponentMenu("SteamMultiplayer/LobbyManager")]
[RequireComponent(typeof(NetworkControl))]
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
    [Serializable]
    public struct SceneInfo
    {
        public string OnlineScene;
        public string LobbyScene;
        [HideInInspector] public string CurrentScene;
    }

    public SceneInfo scenes;

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
    protected Callback<GameLobbyJoinRequested_t> m_LobbyInvite;

    //Chating
    [HideInInspector]
    public string chatstring = null;

    public CSteamID host { get { return NetworkControl.PlayerList[0]; } }
    public bool isHost { get { return NetworkControl.SelfID == host; } }
    #endregion

    #region Unity Reserved Functions

    public void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (instance != null)
        {
            throw new Exception("There are more than one manager in the scene");
        }
        if (instance != null) Destroy(gameObject);
        instance = this;
    }

    void Start()
    {
        SteamAPI.Init();
        Spawnable_Object_Init();
        ChatInit();
        m_LobbyInvite=Callback<GameLobbyJoinRequested_t>.Create(OnInvited);
    }

    void Update()
    {
        SteamAPI.RunCallbacks();
        if(lobby.m_SteamID!=0)
        if (SteamMatchmaking.GetLobbyData(lobby, "ready") == "1")
        {
            Play();
        }
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
        NetworkControl.SendPacketsSafely(new P2PPackage(null,P2PPackageType.LeftLobby),false);
        SteamMatchmaking.LeaveLobby(lobby);
        lobby = new CSteamID();

        foreach (var t in NetworkControl.instance.OnlineObjects)
        {
            if (t == null) continue;
            Destroy(t.gameObject);
        }
        NetworkControl.instance.OnlineObjects.Clear();
        if (events.lobby_leaved != null) events.lobby_leaved.Invoke();
        Destroy(gameObject);
        if (scenes.CurrentScene != scenes.LobbyScene)
        {
            SceneManager.LoadScene(scenes.LobbyScene);
            scenes.CurrentScene = scenes.LobbyScene;
        }
    }

    public void CreateLobby()
    {
        var mumber_count = 8;
        var lobby_type = ELobbyType.k_ELobbyTypePublic;
        CLobbyCreator.Set(SteamMatchmaking.CreateLobby(lobby_type, mumber_count), OnLobbyCreated);
    }

    private string LobbyName;
    public void CreateLobby(int mumbercount, ELobbyType type, string name)
    {
        LobbyName = name;
        CLobbyCreator.Set(SteamMatchmaking.CreateLobby(type, mumbercount), OnLobbyCreated);
    }

    public bool InviteFriend(CSteamID id)
    {
      return SteamMatchmaking.InviteUserToLobby(lobby, id);
    }

    public void Invite()
    {
        SteamFriends.ActivateGameOverlayInviteDialog(lobby);
    }
    #endregion

    #region Chat 
    public void SendChatMessage(string t, bool Chat = true)
    {
        var content = Chat ? DateTime.Now.ToString("HH:mm:ss") + " " + SteamFriends.GetPersonaName() + ": " + t : t;
        var _byte = Encoding.Default.GetBytes(content);
        SteamMatchmaking.SendLobbyChatMsg(lobby, Encoding.Default.GetBytes(content), _byte.Length + 1);
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

        chatstring = Encoding.Default.GetString(Data);
        if (lobby_chat_msg_recevied != null) lobby_chat_msg_recevied.Invoke(chatstring);
    }

    private void OnLobbyCreated(LobbyCreated_t pCallbacks, bool bIOFailure)
    {
        pCallbacks.m_ulSteamIDLobby.ToString();
        lobby = new CSteamID(pCallbacks.m_ulSteamIDLobby);
        print("設置大廳名稱為"+LobbyName+" "+SteamMatchmaking.SetLobbyData(lobby, "name", LobbyName));
        if (events.lobby_created != null) events.lobby_created.Invoke();
        JoinLobby(lobby);
    }

    private void OnLobbyJoined(LobbyEnter_t pCallbacks, bool bIOFailure)
    {
        NetworkControl.CreateConnections(lobby);
        if (events.lobby_just_joined != null) events.lobby_just_joined.Invoke();
        SendChatMessage(SteamFriends.GetPersonaName() + " Joined the Lobby", false);
        NetworkControl.instance.CheckJoinedLobby();
    }

    private void OnInvited(GameLobbyJoinRequested_t pCallbacks)
    {
        print("收到邀请"+ SteamFriends.GetFriendPersonaName(pCallbacks.m_steamIDFriend));
        // if (Invitation.instance == null) return;
        // Invitation.instance.ShowInvite(pCallbacks.m_steamIDFriend,pCallbacks.m_steamIDLobby);
        JoinLobby(pCallbacks.m_steamIDLobby);
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

    public void PlayButtonDowm()
    {
        SteamMatchmaking.SetLobbyData(lobby, "ready", "1");
        //  SceneManager.LoadScene(scenes.OnlineScene.name);
    }

    private void Play()
    {
        if (scenes.CurrentScene != scenes.OnlineScene)
        {
            SceneManager.LoadScene(scenes.OnlineScene);
            scenes.CurrentScene = scenes.OnlineScene;
        }
    }

}
