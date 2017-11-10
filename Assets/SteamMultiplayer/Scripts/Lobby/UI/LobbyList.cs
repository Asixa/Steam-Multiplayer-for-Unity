using Steamworks;
using UnityEngine;
public class LobbyList : MonoBehaviour {

    CallResult<LobbyMatchList_t> m_LobbyMatchListCallResult;

    private SingleLobbyButton[] m_items;
    private Lobby[] m_Lobbies;
    public SingleLobbyButton item_prefab;
    public GameObject container;

    private void Start ()
    {
        SteamAPI.Init();
		Prepare();
        Refresh();
	}

    public void Prepare()
    {
        m_LobbyMatchListCallResult = CallResult<LobbyMatchList_t>.Create(OnLobbyMatchList);
    }

    private void Update () {
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

    public void DesplayList()
    {
        var nLobbies = m_Lobbies.Length;
        if (m_items!=null&&m_items.Length> 0)
        foreach (var t in m_items)
        {
            Destroy(t.gameObject);
        }

        var rect_transform = container.GetComponent<RectTransform>();
        rect_transform.sizeDelta=new Vector2(rect_transform.sizeDelta.x, nLobbies * item_prefab.GetComponent<RectTransform>().sizeDelta.y);

        m_items = new SingleLobbyButton[nLobbies];
        for (var i = 0; i < nLobbies; ++i)
        {
            m_items[i] = Instantiate(item_prefab);
            m_items[i].gameObject.SetActive(true);
            m_items[i].name = gameObject.name + " item at (" + i + ")";
            m_items[i].transform.parent = container.transform;
            m_items[i].ID.text=i.ToString();
            m_items[i].Name.text = SteamMatchmaking.GetLobbyData(m_Lobbies[i].m_SteamID, "name");
            if (m_items[i].Name.text == "") m_items[i].Name.text = "NULL";
            m_items[i].MumberCount.text = m_Lobbies[i].m_Members.Length + "/" + m_Lobbies[i].m_MemberLimit;
            m_items[i].server_id = m_Lobbies[i].m_SteamID;
            m_items[i].button.onClick.AddListener(delegate {
            });
        }
    }

    private void OnLobbyMatchList(LobbyMatchList_t pCallback, bool bIOFailure)
    {
        if (bIOFailure)
        {
            var reason = SteamUtils.GetAPICallFailureReason(m_LobbyMatchListCallResult.Handle);
            Debug.LogError("OnLobbyMatchList encountered an IOFailure due to: " + reason);
            return;
        }
        if (pCallback.m_nLobbiesMatching == 0)
        {
            return;
        }

        m_Lobbies = new Lobby[pCallback.m_nLobbiesMatching];
        for (var i = 0; i < pCallback.m_nLobbiesMatching; ++i)
        { 
            UpdateLobbyInfo(SteamMatchmaking.GetLobbyByIndex(i), ref m_Lobbies[i]);
        }
        DesplayList();
    }

    private static void UpdateLobbyInfo(CSteamID steamIDLobby, ref Lobby outLobby)
    {
        outLobby.m_SteamID = steamIDLobby;
        outLobby.m_Owner = SteamMatchmaking.GetLobbyOwner(steamIDLobby);
        outLobby.m_Members = new LobbyMembers[SteamMatchmaking.GetNumLobbyMembers(steamIDLobby)];
        outLobby.m_MemberLimit = SteamMatchmaking.GetLobbyMemberLimit(steamIDLobby);

        var nDataCount = SteamMatchmaking.GetLobbyDataCount(steamIDLobby);
        outLobby.m_Data = new LobbyMetaData[nDataCount];
        for (var i = 0; i < nDataCount; ++i)
        {
            var lobby_data_ret = SteamMatchmaking.GetLobbyDataByIndex(steamIDLobby, i, out outLobby.m_Data[i].m_Key, Constants.k_nMaxLobbyKeyLength, out outLobby.m_Data[i].m_Value, Constants.k_cubChatMetadataMax);
            if (lobby_data_ret) continue;
            Debug.LogError("SteamMatchmaking.GetLobbyDataByIndex returned false.");
            continue;
        }
    }

    #region Structs
    private struct Lobby
    {
        public CSteamID m_SteamID;
        public CSteamID m_Owner;
        public LobbyMembers[] m_Members;
        public int m_MemberLimit;
        public LobbyMetaData[] m_Data;
    }

    private struct LobbyMetaData
    {
        public string m_Key;
        public string m_Value;
    }

    private struct LobbyMembers
    {
        public CSteamID m_SteamID;
        public LobbyMetaData[] m_Data;
    }
    #endregion

}
