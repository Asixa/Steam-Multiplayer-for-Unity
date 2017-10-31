//=============================================
// 网络连接基类
//创建者 Asixa 2017-9-x
//最新修改 Fangxm 2017-10-29
//=============================================

using System;
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
    }

    public enum P2PPackageType
    {
        位移同步,
        SeverClose,
        Instantiate,
        JunkData,
        RPC,
        Sync,
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
            Object_identity =identity.ID;
            ObjectSpawnID = identity.SpawnID;
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

