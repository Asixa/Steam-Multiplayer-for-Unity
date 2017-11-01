//=============================================
// 网络连接基类
//创建者 Asixa 2017-9-x
//最新修改 Fangxm 2017-10-29
//=============================================

using System;
using Steamworks;
using UnityEngine;

namespace SteamMultiplayer
{
    [RequireComponent(typeof(Identity))]
    public class SteamNetworkBehaviour : MonoBehaviour
    {
        public void Awake()
        {
            identity = GetComponent<Identity>();
        }

        public bool IsLocalObject{get { return identity.IsLocalSpawned; }}
        public Identity identity;

        public void rpcCall(int funcIndex, params object[] values)
        {
            SMC.SendPackets(new P2PPackage(new SMC.RPCInfo(funcIndex, values), P2PPackageType.RPC), EP2PSend.k_EP2PSendReliable,false);
        }
    }

    public enum P2PPackageType
    {
        位移同步,
        SeverClose,
        Instantiate,
        JunkData,
        SendMessage,
        Sync,
        DeleteObject,
        LoadScene,
        RPC
    }
    [Serializable]
    public struct P2PPackage
    {
        public object value;
        public int Object_identity;
        public P2PPackageType type;
        public int ObjectSpawnID;
        public P2PPackage(object v, P2PPackageType type,Identity identity)
        {
            this.type = type;
            value = v;
            if (identity != null)
            {
                Object_identity = identity.ID;
                ObjectSpawnID = identity.SpawnID;
            }
            else
            {
                Object_identity = -1;
                ObjectSpawnID = -1;
            }
        }
        public P2PPackage(object v, P2PPackageType type)
        {
            this.type = type;
            value = v;
            Object_identity = -1;
            ObjectSpawnID = -1;
        }
    }

}

