using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Steamworks;
using System.Text;
using UnityEngine.UI;

public class SteamNetwork : MonoBehaviour {

    public Text SeverID;
    public Text enter_text;
    public InputField enter_input;
    public Text chat_text;

    // Use this for initialization
    void Start ()
	{
	    print("注入Steam："+SteamAPI.Init());
	   // InitTimer();

        //FindLobbies();
	    //CreateLobbies();
	}

    void Update()
    {
        SteamAPI.RunCallbacks();

    }

    public void JoinSever()
    {
        FindLobbies();
    }

    public void EndEdit()
    {
        sendMsg(enter_text.text);
        print(1);
        enter_input.text = "";
    }

    #region FindLobby
    CallResult<LobbyMatchList_t> CLobbyListManager = new CallResult<LobbyMatchList_t>();
    void FindLobbies()
    {
        var a = SteamMatchmaking.RequestLobbyList();
        CLobbyListManager.Set(a,OnLobbyMatchList);
    }

    void OnLobbyMatchList(LobbyMatchList_t pCallback, bool bIOFailure)
    {
        print("找到服务器："+pCallback.m_nLobbiesMatching);
        lobby = SteamMatchmaking.GetLobbyByIndex((int)pCallback.m_nLobbiesMatching-1);
        JoinLobby();
    }
    #endregion

    #region CreateLobby
    CallResult<LobbyCreated_t> CLobbyCreator = new CallResult<LobbyCreated_t>();
    public void CreateLobbies()
    {
        ELobbyType lobby_type = ELobbyType.k_ELobbyTypePublic;

        var a = SteamMatchmaking.CreateLobby(lobby_type, 8);
        CLobbyCreator.Set(a, OnLobbyCreated);
    }

    private CSteamID lobby;

    void OnLobbyCreated(LobbyCreated_t pCallbacks, bool bIOFailure)
    {
        print("创建服务器：" + pCallbacks.m_eResult);
        print("服务器ID" + pCallbacks.m_ulSteamIDLobby);
        lobby = new CSteamID(pCallbacks.m_ulSteamIDLobby);
        JoinLobby();
    }
    #endregion

    #region JoinLobby
    CallResult<LobbyEnter_t> CLobbyJoin = new CallResult<LobbyEnter_t>();
    void JoinLobby()
    {
        var a = SteamMatchmaking.JoinLobby(lobby);
        //print(a.m_SteamAPICall+" ************");
        CLobbyJoin.Set(a, OnLobbyJoined);
    }

    void OnLobbyJoined(LobbyEnter_t pCallbacks, bool bIOFailure)
    {
        if (!bIOFailure)
        {
            print("--");
        }
        print("服务器ID" + pCallbacks.m_ulSteamIDLobby);
        print("服务器上锁：" + pCallbacks.m_bLocked);
        SeverID.text = pCallbacks.m_ulSteamIDLobby.ToString();
        print(SteamMatchmaking.GetNumLobbyMembers(lobby));
        ChatInit();
        sendMsg("HelloWorld");
    }


    #endregion

    void sendMsg(string t)
    {
        SteamMatchmaking.SendLobbyChatMsg(lobby, Encoding.Default.GetBytes(t), t.Length + 1);
    }
    protected Callback<LobbyChatUpdate_t> m_LobbyChatUpdate;
    protected Callback<LobbyChatMsg_t> m_LobbyChatMsg;
    void ChatInit()
    {
        m_LobbyChatUpdate = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);
        m_LobbyChatMsg = Callback<LobbyChatMsg_t>.Create(OnLobbyChatMsg);
        print("Chat system Done " + LobbyChatMsg_t.k_iCallback);
    }

    void OnLobbyChatUpdate(LobbyChatUpdate_t pCallback)
    {
        Debug.Log("[" + LobbyChatUpdate_t.k_iCallback + " - LobbyChatUpdate] - " + pCallback.m_ulSteamIDLobby + " -- " + pCallback.m_ulSteamIDUserChanged + " -- " + pCallback.m_ulSteamIDMakingChange + " -- " + pCallback.m_rgfChatMemberStateChange);
    }

    void OnLobbyChatMsg(LobbyChatMsg_t pCallback)
    {
        Debug.Log("[" + LobbyChatMsg_t.k_iCallback + " - LobbyChatMsg] - " + pCallback.m_ulSteamIDLobby + " -- " + pCallback.m_ulSteamIDUser + " -- " + pCallback.m_eChatEntryType + " -- " + pCallback.m_iChatID);
        CSteamID SteamIDUser;
        byte[] Data = new byte[4096];
        EChatEntryType ChatEntryType;
        int ret = SteamMatchmaking.GetLobbyChatEntry((CSteamID)pCallback.m_ulSteamIDLobby, (int)pCallback.m_iChatID, out SteamIDUser, Data, Data.Length, out ChatEntryType);
        Debug.Log("GetLobbyChatEntry(" + (CSteamID)pCallback.m_ulSteamIDLobby + ", " + (int)pCallback.m_iChatID + ", out SteamIDUser, Data, Data.Length, out ChatEntryType) : " + ret + " -- " + SteamIDUser + " -- " + System.Text.Encoding.UTF8.GetString(Data) + " -- " + ChatEntryType);

        chat_text.text += System.Text.Encoding.UTF8.GetString(Data) + "\n";
    }
}
