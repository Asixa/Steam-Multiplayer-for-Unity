using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using System.Text;

public class NetWorkingCore : MonoBehaviour {

    public static int VirtualPort = 233;
    public static int TimeoutSec = 300;

    static Callback<SocketStatusCallback_t> m_SocketStatusCallback = Callback<SocketStatusCallback_t>.Create(OnSocketStatusCallback);
    static Callback<P2PSessionRequest_t> m_P2PSessionRequest = Callback<P2PSessionRequest_t>.Create(OnP2PSessionRequest);
    static List<CSteamID> players = new List<CSteamID>();

    public static void Init()
    {

    }

    // Use this for initialization
    void Start ()
    {
    }
	
	// Update is called once per frame
	void Update ()
    {
        uint length;
        if(SteamNetworking.IsP2PPacketAvailable(out length))
        {
            ReadPackets(length);
        }
	}

    public void testing()
    {
        SendPackets(Encoding.Default.GetBytes("hellow!"));
    }

    public static void CreateConnection(CSteamID player)
    {
        SteamNetworking.CreateP2PConnectionSocket(player, VirtualPort, TimeoutSec, true);
    }
    public static void CreateConnections(CSteamID lobby)
    {
        for (int i = 0; i < SteamMatchmaking.GetNumLobbyMembers(lobby); i++)
        {
            players.Add(SteamMatchmaking.GetLobbyMemberByIndex(lobby, i));
            CreateConnection(players[players.Count - 1]);
        }
    }

    public static void SendPackets(CSteamID target,byte[] data)
    {
        SteamNetworking.SendP2PPacket(target, data, (uint)data.Length, EP2PSend.k_EP2PSendReliable);
    }
    public static void SendPackets(byte[] data)
    {
        foreach (CSteamID item in players)
        {
            SteamNetworking.SendP2PPacket(item, data, (uint)data.Length, EP2PSend.k_EP2PSendReliable);
        }
    }
    public static void ReadPackets(uint length)
    {
        CSteamID sender;
        uint data_length;
        byte[] data = new byte[length];
        SteamNetworking.ReadP2PPacket(data, length, out data_length, out sender);
        print("收到包："+Encoding.UTF8.GetString(data));
    }

    static void OnSocketStatusCallback(SocketStatusCallback_t pCallback)
    {
        print(pCallback.m_steamIDRemote.m_SteamID);
    }
    static void OnP2PSessionRequest(P2PSessionRequest_t pCallback)
    {
        SteamNetworking.AcceptP2PSessionWithUser(pCallback.m_steamIDRemote);
        players.Add(pCallback.m_steamIDRemote);
    }
}
