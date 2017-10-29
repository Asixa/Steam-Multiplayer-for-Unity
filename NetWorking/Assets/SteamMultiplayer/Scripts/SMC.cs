using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using SteamMultiplayer;
using UnityEngine.UI;

[Serializable]
public class SMC : MonoBehaviour
{
    public int 人数;

    public static SMC instance;

    public static CSteamID SelfID;
    public static bool IsP2PHost;
    private CSteamID P2PHost;
    public List<M_Identity>OnlineObjects=new List<M_Identity>();

    static List<SNetSocket_t> m_SNetSocket = new List<SNetSocket_t>();
    static Callback<SocketStatusCallback_t> m_SocketStatusCallback = Callback<SocketStatusCallback_t>.Create(OnSocketStatusCallback);
    static Callback<P2PSessionRequest_t> m_P2PSessionRequest = Callback<P2PSessionRequest_t>.Create(OnP2PSessionRequest);
    static List<CSteamID> PlayerList = new List<CSteamID>();

    public void Awake()
    {
        instance = this;
        SelfID = SteamUser.GetSteamID();
    }

	void Update ()
	{
	    
	    if (人数 != PlayerList.Count)
	    {
	        人数 = PlayerList.Count;
	        UpdatePlayerList();
	    }

        #region 接收数据包

        uint size;
        while (SteamNetworking.IsP2PPacketAvailable(out size))
        {
            var data = new byte[size];
            uint bytesRead;
            CSteamID remoteId;
            if (SteamNetworking.ReadP2PPacket(data, size, out bytesRead, out remoteId))
            {
                P2PPackage package = (P2PPackage)new BinaryFormatter().Deserialize(new MemoryStream(data));
                Debug.Log("从 " + SteamFriends.GetFriendPersonaName(remoteId) + "收到包" + package.type +" ID "+package.Object_identity);
                DecodeP2PCode(package);
            }
        }
        #endregion

	    JunkPackagetime -= Time.deltaTime;
	    if (JunkPackagetime <= 0)
	    {
	        JunkPackagetime = 1 / 8;
	        SendPackets(new P2PPackage(null, -1, P2PPackageType.JunkData), EP2PSend.k_EP2PSendUnreliable, false);
        }
      
    }

    private float JunkPackagetime = 0.1f;
    public void UpdatePlayerList()
    {
        if (LobbyPanel.instance.lobby_room.Player_List == null)
            LobbyPanel.instance.lobby_room.Player_List = new List<PlayerListPrefab>();
        else
        {
            foreach (PlayerListPrefab t in LobbyPanel.instance.lobby_room.Player_List)
            {
                Destroy(t);
            }
        }

        foreach (var player in PlayerList)
        {
            var one = Instantiate(LobbyPanel.instance.lobby_room.PlayerListPrefab);
            one.Name.text=SteamFriends.GetPlayerNickname(player);
            StartCoroutine(_FetchAcatar(player, one.Icon));
            one.transform.parent = LobbyPanel.instance.lobby_room.PlayerListPanel;
            one.gameObject.SetActive(true);
            Debug.Log("加载玩家列表—— "+player.m_SteamID);
        }
    }

    #region CreateConnection
    public static void CreateConnection(CSteamID player)
    {
        SteamNetworking.CreateP2PConnectionSocket(player,
            NetworkLobbyManager.instance.NetworkOpinion.Port,
            NetworkLobbyManager.instance.NetworkOpinion.TimeoutSec,
            true);
    }

    public static void CreateConnections(CSteamID lobby)
    {
        for (int i = 0; i < SteamMatchmaking.GetNumLobbyMembers(lobby); i++)
        {
            PlayerList.Add(SteamMatchmaking.GetLobbyMemberByIndex(lobby, i));
            CreateConnection(PlayerList[PlayerList.Count - 1]);
        }
    }

    #endregion

    #region DestroyConnection
    /// <param name="bNotifyRemoteEnd">true:套接字上的任何未读取数据将被丢弃，直到远端确认断开连接后，套接字才会被完全破坏</param>
    public static void DestroyConnections(bool bNotifyRemoteEnd)
    {
        foreach (var item in m_SNetSocket)
        {
            SteamNetworking.DestroySocket(item, bNotifyRemoteEnd);
        }
    }
    #endregion

    #region SendPacket

    public static void SendPackets(P2PPackage data,EP2PSend send, bool IncludeSelf = true)
    {
        //SendUnreliable – 小包，可以丢失，不需要依次发送，但要快
        //SendUnreliableNoDelay –     跟上面一样，但是不做链接检查，因为这样，它可能被丢失，但是这种方式是最快的传送方式。
        //SendReliable – 可靠的信息，大包，依次收发。
        //SendReliableWithBuffering –     跟上面一样，但是在发送前会缓冲数据，如果你发送大量的小包，它不会那么及时。（可能会延迟200ms）
        MemoryStream ms = new MemoryStream();
        new BinaryFormatter().Serialize(ms, data);
        SendPacketsToAll(ms.GetBuffer(),send,IncludeSelf);
    }

    private static void SendPacketsToAll(byte[] data,EP2PSend send,bool IncludeSelf)
    {
        foreach (var item in PlayerList)
        {
            if(!IncludeSelf&&item==SelfID)continue;
            Debug.Log("向"+item.m_SteamID+"发送数据包");
            SteamNetworking.SendP2PPacket(item, data, (uint)data.Length,send);
        }
    }
    #endregion

    #region  委托
    private static void OnSocketStatusCallback(SocketStatusCallback_t pCallback)
    {
        print(pCallback.m_steamIDRemote.m_SteamID);
    }

    private static void OnP2PSessionRequest(P2PSessionRequest_t pCallback)
    {
        SteamNetworking.AcceptP2PSessionWithUser(pCallback.m_steamIDRemote);
        PlayerList.Add(pCallback.m_steamIDRemote);
    }

    #endregion

    private static void DecodeP2PCode(P2PPackage package)
    {
        
        switch (package.type)
        {
            case P2PPackageType.位移同步:
                Debug.Log("同步物体ID"+package.Object_identity);
                while (instance.OnlineObjects.Count <= package.Object_identity)
                {
                    instance.OnlineObjects.Add(null);
                }
                if (instance.OnlineObjects[package.Object_identity] == null)
                {
                    var obj2= Instantiate(NetworkLobbyManager.instance.SpawnInfo.Spawnable_objects.objects[package.ObjectSpawnID]);
                    obj2.TargetID = package.Object_identity;
                    obj2.Init();
                }
                instance.OnlineObjects[package.Object_identity].GetComponent<SynTransform>().Receive(To_Vector3((M_Vector3)package.value));
                break;
            case P2PPackageType.SeverClose:
                Debug.Log("服务器关闭");
                DestroyConnections(true);
                break;
            case P2PPackageType.Method:
                break;
            case P2PPackageType.Int:
                break;
            case P2PPackageType.String:
                break;
            case P2PPackageType.Float:
                break;
            case P2PPackageType.Instantiate:
                var info = (SpawnInfo) package.value;
                var obj=Instantiate(NetworkLobbyManager.instance.SpawnInfo.Spawnable_objects.objects[info.SpawnID],To_Vector3(info.pos),To_Quaternion(info.rot));
                obj.TargetID = package.Object_identity;
                obj.Init();
                break;
            case P2PPackageType.JunkData:
                Debug.Log("收到垃圾信息");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }


    #region 生成
    [Serializable]
    public struct SpawnInfo
    {
        public M_Vector3 pos;
        public M_Quaternion rot;
        public int SpawnID;

        public SpawnInfo(M_Vector3 pos, M_Quaternion rot, int ID)
        {
            this.pos = pos;
            this.rot = rot;
            SpawnID = ID;
        }
    }

    public object Spawn(M_Identity id, Vector3 pos = new Vector3(), Quaternion rot = new Quaternion())
    {
       
        var obj= Instantiate(NetworkLobbyManager.instance.SpawnInfo.Spawnable_objects.objects[id.SpawnID]);
        obj.IsLocalSpawned = true;
        obj.TargetID = instance.OnlineObjects.Count;
        obj.Init(); 

        SendPackets(
            new P2PPackage(new SpawnInfo(new M_Vector3(pos), new M_Quaternion(rot), obj.SpawnID), obj.ID,
                P2PPackageType.Instantiate),
            EP2PSend.k_EP2PSendReliable, false);
        return obj;

    }

    #endregion


    #region 重写Unity数学模型

    #region 重写Vector3
    [Serializable]
    public struct M_Vector3
    {
        public float x, y, z;
        public M_Vector3(Vector3 v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }
    }
    public static Vector3 To_Vector3(M_Vector3 input)
    {
        return new Vector3(input.x, input.y, input.z);
    }
    #endregion

    #region 重写 Quaternion
    [Serializable]
    public struct M_Quaternion
    {
        public float x, y, z, w;
        public M_Vector3 eulerAngles;
        public M_Quaternion(Quaternion input)
        {
            x = input.x;
            y = input.y;
            z = input.z;
            w = input.w;
            eulerAngles = new M_Vector3(input.eulerAngles);
        }
    }
    public static Quaternion To_Quaternion(M_Quaternion input)
    {
        return new Quaternion { x = input.x, y = input.y, z = input.z, w = input.w, eulerAngles = To_Vector3(input.eulerAngles) };
    }
    #endregion

    #endregion

    #region 获取头像
    private uint width, height;
    private Texture2D downloadedAvatar;
    IEnumerator _FetchAcatar(CSteamID id, RawImage ui)
    {
        var AvatarInt = SteamFriends.GetLargeFriendAvatar(id);
        while (AvatarInt == -1)
        {
            yield return null;
        }
        if (AvatarInt > 0)
        {
            SteamUtils.GetImageSize(AvatarInt, out width, out height);

            if (width > 0 && height > 0)
            {
                byte[] avatarStream = new byte[4 * (int)width * (int)height];
                SteamUtils.GetImageRGBA(AvatarInt, avatarStream, 4 * (int)width * (int)height);

                downloadedAvatar = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false);
                downloadedAvatar.LoadRawTextureData(avatarStream);
                downloadedAvatar.Apply();

                ui.texture = downloadedAvatar;
            }
        }
    }
    #endregion



}
