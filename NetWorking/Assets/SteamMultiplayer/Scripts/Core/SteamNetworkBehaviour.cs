//=============================================
// 网络连接基类
//创建者 Asixa 2017-9-x
//最新修改 Asixa 2017-10-29
//=============================================

using System;
using UnityEngine;

namespace SteamMultiplayer
{
    [RequireComponent(typeof(M_Identity))]
    public class SteamNetworkBehaviour : MonoBehaviour
    {
        public void Awake()
        {
            identity = GetComponent<M_Identity>();
        }

        public bool IsLocalObject{get { return identity.IsLocalSpawned; }}

        public int ID { get { return identity.ID; } }
        public M_Identity identity { get; set; }
    }

    public enum P2PPackageType
    {
        位移同步,
        Method,
        Int,
        String,
        Float,
        Instantiate,
        JunkData,
    }
    [Serializable]
    public struct P2PPackage
    {
        public object value;
        public int Object_identity;
        public P2PPackageType type;
        public int ObjectSpawnID;
        public P2PPackage(object v, int id, P2PPackageType type,int SpawnID=-1)
        {
            this.type = type;
            value = v;
            Object_identity = id;
            ObjectSpawnID = SpawnID;
        }
    }

}

