using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using UnityEngine.Networking.Types;
using NetworkManager = NATTraversal.NetworkManager;
using UnityEngine.UI;
public class SteamNetworkManager :NATTraversal.NetworkManager
{
    public string ConnectAddress;

    public UnityEngine.UI.InputField eip;
    public UnityEngine.UI.InputField iip;

    public InputField oipv6;
    public InputField ipv6;

    public InputField out_field;
    public InputField out_field2;
    public InputField out_field3;
    public InputField out_field4;
    //创建房间  
    void Start()
    {
        out_field.text = GetExternalIP();

        out_field2.text =GetLocalIp();
        
        out_field4.text = getLocalIPv6();

    }
    public void StartupHost()
    {
        //SetPort();
        var a=StartHostAll("Hello World", 16);


    }
    
    //加入游戏  
    public void JoinGame()
    {
        StartClientAll(
           eip.text,
            iip.text,
            7777,
            233,
            NetworkID.Invalid,
            oipv6.text,
            ipv6.text
            );
        /*public virtual void StartClientAll(
         * string hostExternalIP,
         * string hostInternalIP, 
         * int directConnectPort = 0, 
         * ulong hostGUID = 0, 
         * NetworkID matchID = NetworkID.Invalid, 
         * string hostExternalIPv6 = "", 
         * string hostInternalIPv6 = "",
         * NetworkMatch.DataResponseDelegate<MatchInfo> joinMatchCallback = null,
         * string matchPassword = "",
         * int eloScore = 0, 
         * int requestDomain = 0,
         * bool matchAlreadyJoined = false
         * );
         */    
    //StartClientAll();
    }

    public static string GetLocalIp()
    {
        string[] Ips = GetLocalIpAddress();

        foreach (string ip in Ips) if (ip.StartsWith("10.80.")) return ip;
        foreach (string ip in Ips) if (ip.Contains(".")) return ip;

        return "127.0.0.1";
    }
    public static string[] GetLocalIpAddress()
    {
        string hostName = Dns.GetHostName();                    //获取主机名称  
        IPAddress[] addresses = Dns.GetHostAddresses(hostName); //解析主机IP地址  

        string[] IP = new string[addresses.Length];             //转换为字符串形式  
        for (int i = 0; i < addresses.Length; i++) IP[i] = addresses[i].ToString();

        return IP;
    }

    private static string _eip;
    static bool ResolveExternalIP(string url)
    {
#if !UNITY_WINRT
        if (string.IsNullOrEmpty(url)) return false;

        try
        {
            WebClient web = new WebClient();
            string text = web.DownloadString(url).Trim();
            if (text != null)
            {
                _eip = text;
                return true;
            }
        }
        catch (System.Exception) { }
#endif
        return false;
    }

    public string GetExternalIP()
    {
        if (ResolveExternalIP("http://icanhazip.com")) return _eip;
        if (ResolveExternalIP("http://bot.whatismyipaddress.com")) return _eip;
        if (ResolveExternalIP("http://ipinfo.io/ip")) return _eip;

        return "Get External IP failed";
    }

}
