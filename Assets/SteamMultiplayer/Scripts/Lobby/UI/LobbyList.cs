using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;
public class LobbyList : MonoBehaviour {

    CallResult<LobbyMatchList_t> m_LobbyMatchListCallResult;

    void Start ()
    {
        SteamAPI.Init();
		prepare();
        Refresh();
	}

    void prepare()
    {
        m_LobbyMatchListCallResult = CallResult<LobbyMatchList_t>.Create(OnLobbyMatchList);
    }


	void Update () {
		SteamAPI.RunCallbacks();
	}

    public void Refresh()
    {
        m_LobbyMatchListCallResult.Set(SteamMatchmaking.RequestLobbyList());
    }

    public void CreateLobby()
    {
        NetworkLobbyManager.instance.CreateLobby();
    }


    private SingleLobbyButton[] m_items;
    private Lobby[] m_Lobbies;
    public SingleLobbyButton itemPrefab;
    public GameObject Container;

    public void DesplayList()
    {

        int nLobbies = m_Lobbies.Length;
        if (m_items!=null&&m_items.Length> 0)
        foreach (var t in m_items)
        {
            Destroy(t.gameObject);
        }

        //m_StatusText.text = "Found " + nLobbies + " lobbies:";

      //  RectTransform itemRectT = itemPrefab.GetComponent<RectTransform>();
        RectTransform rectTransform = Container.GetComponent<RectTransform>();
        rectTransform.sizeDelta=new Vector2(rectTransform.sizeDelta.x, nLobbies * itemPrefab.GetComponent<RectTransform>().sizeDelta.y);

        m_items = new SingleLobbyButton[nLobbies];
        for (var i = 0; i < nLobbies; ++i)
        {
            m_items[i] = Instantiate(itemPrefab);
            m_items[i].gameObject.SetActive(true);
            m_items[i].name = gameObject.name + " item at (" + i + ")";
            m_items[i].transform.parent = Container.transform;


            //RectTransform newRectT = m_items[i].GetComponent<RectTransform>();
            //newRectT.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, rectTransform.rect.width);
            //newRectT.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, i * itemRectT.rect.height, itemRectT.rect.height);

            {
                m_items[i].ID.text=i.ToString();
                m_items[i].Name.text = SteamFriends.GetFriendPersonaName(m_Lobbies[i].m_Owner);
                m_items[i].Name.text = m_items[i].Name.text == "" ? "NULL" : m_items[i].Name.text + "'s Lobby";
                m_items[i].MumberCount.text = m_Lobbies[i].m_Members.Length + "/" + m_Lobbies[i].m_MemberLimit;
                m_items[i].server_id = m_Lobbies[i].m_SteamID;
               
            }
            m_items[i].button.onClick.AddListener(delegate {
                //m_CurrentlySelected = ThisIsDumb;
                //System.Text.StringBuilder test = new System.Text.StringBuilder();
                //test.Append("Lobby Data list:\n");
                //for (int j = 0; j < m_Lobbies[m_CurrentlySelected].m_Data.Length; ++j)
                //{
                //    test.Append(m_Lobbies[m_CurrentlySelected].m_Data[j].m_Key);
                //    test.Append(" - ");
                //    test.Append(m_Lobbies[m_CurrentlySelected].m_Data[j].m_Value);
                //    test.Append("\n");
                //}

                //test.Append("Members list:\n");
                //for (int j = 0; j < m_Lobbies[m_CurrentlySelected].m_Members.Length; ++j)
                //{
                //    test.Append(j);
                //    test.Append(". ");
                //    test.Append(m_Lobbies[m_CurrentlySelected].m_Members[j].m_SteamID);
                //    test.Append(" - ");
                //    test.Append(SteamFriends.GetFriendPersonaName(m_Lobbies[m_CurrentlySelected].m_Members[j].m_SteamID));
                //    test.Append("\n");
                //}

                //m_ServerInfoText.text = test.ToString();

                //m_JoinButton.interactable = true;
            });
        }
    }


    void OnLobbyMatchList(LobbyMatchList_t pCallback, bool bIOFailure)
    {
        if (bIOFailure)
        {
            ESteamAPICallFailure reason = SteamUtils.GetAPICallFailureReason(m_LobbyMatchListCallResult.Handle);
            Debug.LogError("OnLobbyMatchList encountered an IOFailure due to: " + reason);
            return; // TODO: Recovery
        }

        Debug.Log("[刷新到大厅]:" + pCallback.m_nLobbiesMatching);

        if (pCallback.m_nLobbiesMatching == 0)
        {
            //*************没有找到服务器
            return;
        }

        m_Lobbies = new Lobby[pCallback.m_nLobbiesMatching];
        for (int i = 0; i < pCallback.m_nLobbiesMatching; ++i)
        {
            UpdateLobbyInfo(SteamMatchmaking.GetLobbyByIndex(i), ref m_Lobbies[i]);

            /*uint IP;
			ushort Port;
			CSteamID GameServerSteamID;
			bool lobbyGameServerRet = SteamMatchmaking.GetLobbyGameServer(m_Lobbies[i].m_SteamID, out IP, out Port, out GameServerSteamID);
			print("IP: " + IP);
			print("Port: " + Port);
			print("GSID: " + GameServerSteamID);*/
        }

        DesplayList();
        //ChangeState(EChatClientState.DisplayResults);
    }
    void UpdateLobbyInfo(CSteamID steamIDLobby, ref Lobby outLobby)
    {
        outLobby.m_SteamID = steamIDLobby;
        outLobby.m_Owner = SteamMatchmaking.GetLobbyOwner(steamIDLobby);
        outLobby.m_Members = new LobbyMembers[SteamMatchmaking.GetNumLobbyMembers(steamIDLobby)];
        outLobby.m_MemberLimit = SteamMatchmaking.GetLobbyMemberLimit(steamIDLobby);

        int nDataCount = SteamMatchmaking.GetLobbyDataCount(steamIDLobby);
        outLobby.m_Data = new LobbyMetaData[nDataCount];
        for (int i = 0; i < nDataCount; ++i)
        {
            bool lobbyDataRet = SteamMatchmaking.GetLobbyDataByIndex(steamIDLobby, i, out outLobby.m_Data[i].m_Key, Constants.k_nMaxLobbyKeyLength, out outLobby.m_Data[i].m_Value, Constants.k_cubChatMetadataMax);
            if (!lobbyDataRet)
            {
                Debug.LogError("SteamMatchmaking.GetLobbyDataByIndex returned false.");
                continue;
            }

        }
    }

    #region Structs
    struct Lobby
    {
        public CSteamID m_SteamID;
        public CSteamID m_Owner;
        public LobbyMembers[] m_Members;
        public int m_MemberLimit;
        public LobbyMetaData[] m_Data;
    }

    struct LobbyMetaData
    {
        public string m_Key;
        public string m_Value;
    }

    struct LobbyMembers
    {
        public CSteamID m_SteamID;
        public LobbyMetaData[] m_Data;
    }
    #endregion

}
