using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class NetWorkingCore : MonoBehaviour {

    public static int VirtualPort = 233;
    public static int TimeoutSec = 300;

    static Callback<SocketStatusCallback_t> m_SocketStatusCallback = Callback<SocketStatusCallback_t>.Create(OnSocketStatusCallback);
    static Callback<P2PSessionRequest_t> m_P2PSessionRequest = Callback<P2PSessionRequest_t>.Create(OnP2PSessionRequest);
    static List<CSteamID> players = new List<CSteamID>();

    public void testing()
    {
        SendPackets("hellow!!");
    }

    // Use this for initialization
    void Start ()
    {
        InvokeRepeating("testing", 0, 0.005f);
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

    #region SendPacket
    /// <summary>
    /// Send the package to someone.
    /// </summary>
    public static void SendPackets(CSteamID target,object data)
    {
        MemoryStream ms = new MemoryStream();
        new BinaryFormatter().Serialize(ms, data);
        SendPackets(target, ms.GetBuffer());
    }
    /// <summary>
    /// Send the package to someone.
    /// </summary>
    public static void SendPackets(CSteamID target,byte[] data)
    {
        SteamNetworking.SendP2PPacket(target, data, (uint)data.Length, EP2PSend.k_EP2PSendReliable);
    }
    /// <summary>
    /// Send the package to everyone except the server.
    /// </summary>
    public static void SendPackets(object data)
    {
        MemoryStream ms = new MemoryStream();
        new BinaryFormatter().Serialize(ms, data);
        SendPackets(ms.GetBuffer());
    }
    /// <summary>
    /// Send the package to everyone except the server.
    /// </summary>
    public static void SendPackets(byte[] data)
    {
        foreach (CSteamID item in players)
        {
            SendPackets(item, data);
        }
    }
    #endregion

    public static object ReadPackets(uint length)
    {
        CSteamID sender;
        uint data_length;
        byte[] data = new byte[length];
        SteamNetworking.ReadP2PPacket(data, length, out data_length, out sender);
        MemoryStream ms = new MemoryStream(data);
        object o = new BinaryFormatter().Deserialize(ms);
        //print("收到包：" + o + " 发送者：" + sender.m_SteamID);
        return o;
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
