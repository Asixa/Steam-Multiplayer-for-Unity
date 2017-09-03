using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using SteamMultiplayer;

public class SMC : MonoBehaviour
{

    public static bool IsP2PHost;
    private CSteamID P2PHost;
    public static List<M_Identity>OnlineObjects=new List<M_Identity>();


    static Callback<SocketStatusCallback_t> m_SocketStatusCallback = Callback<SocketStatusCallback_t>.Create(OnSocketStatusCallback);
    static Callback<P2PSessionRequest_t> m_P2PSessionRequest = Callback<P2PSessionRequest_t>.Create(OnP2PSessionRequest);
    static List<CSteamID> PlayerList = new List<CSteamID>();
	
	void Update ()
    {
        uint length;
        if(SteamNetworking.IsP2PPacketAvailable(out length))
        {
            ReadPackets(length);
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
    /// Send the package to someone.
    public static void SendPackets(CSteamID target,object data)
    {
        var ms = new MemoryStream();
        new BinaryFormatter().Serialize(ms, data);
        SendPackets(target, ms.GetBuffer());
    }
    /// Send the package to someone.
    public static void SendPackets(CSteamID target,byte[] data)
    {
        SteamNetworking.SendP2PPacket(target, data, (uint)data.Length, EP2PSend.k_EP2PSendReliable);
    }
    /// Send the package to everyone except the server.
    public static void SendPackets(object data)
    {
        MemoryStream ms = new MemoryStream();
        new BinaryFormatter().Serialize(ms, data);
        SendPackets(ms.GetBuffer());
    }
    /// Send the package to everyone except the server.
    public static void SendPackets(byte[] data)
    {
        foreach (var item in PlayerList)
        {
            SendPackets(item, data);
        }
    }
    #endregion

    public static object ReadPackets(uint length)
    {
        CSteamID sender;
        uint data_length;
        var data = new byte[length];
        SteamNetworking.ReadP2PPacket(data, length, out data_length, out sender);
        var ms = new MemoryStream(data);
        P2PPackage package = (P2PPackage)new BinaryFormatter().Deserialize(ms);
        //print("收到包：" + o + " 发送者：" + sender.m_SteamID);
        return package;
    }

    private static void OnSocketStatusCallback(SocketStatusCallback_t pCallback)
    {
        print(pCallback.m_steamIDRemote.m_SteamID);
    }

    private static void OnP2PSessionRequest(P2PSessionRequest_t pCallback)
    {
        SteamNetworking.AcceptP2PSessionWithUser(pCallback.m_steamIDRemote);
        PlayerList.Add(pCallback.m_steamIDRemote);
    }
}
