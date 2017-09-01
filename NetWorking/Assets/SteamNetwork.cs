using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Steamworks;
public class SteamNetwork : MonoBehaviour {

	// Use this for initialization
	void Start ()
	{
	    print("注入Steam："+SteamAPI.Init());
	   // InitTimer();

        //FindLobbies();
	    CreateLobbies();
	}

    void Update()
    {
        SteamAPI.RunCallbacks();
    }

    CallResult<LobbyMatchList_t> CLobbyListManager=new CallResult<LobbyMatchList_t>();
    CallResult<LobbyCreated_t> CLobbyCreator = new CallResult<LobbyCreated_t>();
    CallResult<LobbyEnter_t> CLobbyJoin = new CallResult<LobbyEnter_t>();
    void FindLobbies()
    {
        var a = SteamMatchmaking.RequestLobbyList();
        CLobbyListManager.Set(a,OnLobbyMatchList);
    }

    void OnLobbyMatchList(LobbyMatchList_t pCallback, bool bIOFailure)
    {
        print("找到服务器："+pCallback.m_nLobbiesMatching);
    }

    void CreateLobbies()
    {
        ELobbyType lobby_type=ELobbyType.k_ELobbyTypePublic;
        
        var a = SteamMatchmaking.CreateLobby(lobby_type,8);
        CLobbyCreator.Set(a, OnLobbyCreated);
    }

    private ulong LobbyID;
    void OnLobbyCreated(LobbyCreated_t pCallbacks, bool bIOFailure)
    {
        print("创建服务器：" + pCallbacks.m_eResult);
        print("服务器ID"+pCallbacks.m_ulSteamIDLobby);
        LobbyID = pCallbacks.m_ulSteamIDLobby;
        JoinLobby();
    }

    void JoinLobby()
    {
        CSteamID id=new CSteamID(LobbyID);
        var a = SteamMatchmaking.JoinLobby(id);
        CLobbyJoin.Set(a,OnLobbyJoined);
    }
    void OnLobbyJoined(LobbyEnter_t pCallbacks, bool bIOFailure)
    {
        if (!bIOFailure)
        {
            print("--" );
        }
        print("加入服务器：" + bIOFailure);
        print("服务器上锁" + pCallbacks);
        print(SteamMatchmaking.GetNumLobbyMembers(new CSteamID(LobbyID)));

    }

}
