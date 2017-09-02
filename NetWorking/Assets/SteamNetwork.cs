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
        chat_text.text = chat_text.text + t + "\n";
    }

    CallResult<LobbyChatMsg_t> CLobbyChat = new CallResult<LobbyChatMsg_t>();
    void ChatInit()
    {
        CLobbyChat.Set(new SteamAPICall_t(LobbyChatMsg_t.k_iCallback),OnLobbyChatRecieved);
        print("Chat system Done "+LobbyChatMsg_t.k_iCallback);
    }

    void OnLobbyChatRecieved(LobbyChatMsg_t pCallbacks, bool bIOFailure)
    {
        print(2);
        chat_text.text = chat_text.text + pCallbacks.ToString() + "\n";
        print("Chat:"+pCallbacks.m_eChatEntryType.ToString());
    }
}
