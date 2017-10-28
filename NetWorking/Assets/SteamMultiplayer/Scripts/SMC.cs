using System;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using SteamMultiplayer;

[Serializable]
public class SMC : MonoBehaviour
{
    public int 人数;

    public static SMC instance;

    public static CSteamID SelfID;
    public static bool IsP2PHost;
    private CSteamID P2PHost;
    public  List<M_Identity>OnlineObjects=new List<M_Identity>();


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
	    人数 = PlayerList.Count;
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
    }

    #region CreateConnecttion
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

    #region SendPacket

    public static void SendPackets(P2PPackage data,EP2PSend send, bool IncludeSelf = true)
    {
        //k_EP2PSendUnreliable – 小包，可以丢失，不需要依次发送，但要快
        //k_EP2PSendUnreliableNoDelay –     跟上面一样，但是不做链接检查，因为这样，它可能被丢失，但是这种方式是最快的传送方式。
        //k_EP2PSendReliable – 可靠的信息，大包，依次收发。
        //k_EP2PSendReliableWithBuffering –     跟上面一样，但是在发送前会缓冲数据，如果你发送大量的小包，它不会那么及时。（可能会延迟200ms）
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
            case P2PPackageType.Undefined:
                instance.OnlineObjects[package.Object_identity].GetComponent<SynTransform>().Receive(To_Vector3((M_Vector3)package.value));
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
                obj.Init(package.Object_identity);
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
        obj.Init();
        obj.ID = instance.OnlineObjects.Count;
        instance.OnlineObjects.Add(obj);

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


}
