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
    public static SMC instance;

    public static bool IsP2PHost;
    private CSteamID P2PHost;
    public static List<M_Identity>OnlineObjects=new List<M_Identity>();


    static Callback<SocketStatusCallback_t> m_SocketStatusCallback = Callback<SocketStatusCallback_t>.Create(OnSocketStatusCallback);
    static Callback<P2PSessionRequest_t> m_P2PSessionRequest = Callback<P2PSessionRequest_t>.Create(OnP2PSessionRequest);
    static List<CSteamID> PlayerList = new List<CSteamID>();

    public void Awake()
    {
        instance = this;
    }

	void Update ()
    {
        uint size;
        while (SteamNetworking.IsP2PPacketAvailable(out size))
        {
            var data = new byte[size];
            uint bytesRead;
            CSteamID remoteId;
            if (SteamNetworking.ReadP2PPacket(data, size, out bytesRead, out remoteId))
            {
                
                P2PPackage package = (P2PPackage)new BinaryFormatter().Deserialize(new MemoryStream(data));
                Debug.Log("从 " + SteamFriends.GetFriendPersonaName(remoteId) + "收到包" + package.type);
                DecodeP2PCode(package);
            }
        }
    }

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

    #region SendPacket

    public static void SendPackets(P2PPackage data,EP2PSend send)
    {
        //k_EP2PSendUnreliable – 小包，可以丢失，不需要依次发送，但要快
        //k_EP2PSendUnreliableNoDelay –     跟上面一样，但是不做链接检查，因为这样，它可能被丢失，但是这种方式是最快的传送方式。
        //k_EP2PSendReliable – 可靠的信息，大包，依次收发。
        //k_EP2PSendReliableWithBuffering –     跟上面一样，但是在发送前会缓冲数据，如果你发送大量的小包，它不会那么及时。（可能会延迟200ms）
        MemoryStream ms = new MemoryStream();
        new BinaryFormatter().Serialize(ms, data);
        SendPacketsToAll(ms.GetBuffer(),send);
    }
    /// Send the package to everyone
    private static void SendPacketsToAll(byte[] data,EP2PSend send,bool IncludeSelf=true)
    {
        foreach (var item in PlayerList)
        {
            if(IncludeSelf)
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
                OnlineObjects[package.Object_identity].GetComponent<SynTransform>().Receive(package.value);
                break;
            case P2PPackageType.Method:
                break;
            case P2PPackageType.Int:
                break;
            case P2PPackageType.String:
                break;
            case P2PPackageType.Float:
                break;
            case P2PPackageType.Reserve:
                var info = (SpawnInfo) package.value;
                Instantiate(NetworkLobbyManager.instance.SpawnInfo.Spawnable_objects.objects[info.SpawnID],info.pos,info.rot);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }


    [Serializable]
    public class SpawnInfo
    {
        public Vector3 pos;
        public Quaternion rot;
        public int SpawnID;

        public SpawnInfo(Vector3 p,Quaternion r,int id)
        {
            pos = p;
            rot = r;
            SpawnID = id;
        }
    }
    //
    public object Spawn(M_Identity id,Vector3 pos=new Vector3(),Quaternion rot=new Quaternion())
    {
       var a=JsonUtility.ToJson(new P2PPackage(new SpawnInfo(pos, rot, id.SpawnID), -1, P2PPackageType.Reserve));

        var p = JsonUtility.FromJson<P2PPackage>(a);
        Debug.Log(p.value+"  "+p.type+" "+p.Object_identity);
        //
        
        return null;
        SendPackets(new P2PPackage(new SpawnInfo(pos,rot,id.SpawnID),-1,P2PPackageType.Reserve),EP2PSend.k_EP2PSendReliable);
        return Instantiate(NetworkLobbyManager.instance.SpawnInfo.Spawnable_objects.objects[id.SpawnID]);
    }
}
