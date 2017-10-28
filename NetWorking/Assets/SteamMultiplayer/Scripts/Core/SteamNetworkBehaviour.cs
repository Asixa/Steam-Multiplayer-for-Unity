using System;
using Steamworks;
using UnityEngine;

namespace SteamMultiplayer
{
    public enum P2PPackageType
    {
        Undefined,
        Method,
        Int,
        String,
        Float,
        Instantiate
    }
    [Serializable]
    public struct P2PPackage
    {
        public object value;
        public int Object_identity;
        public P2PPackageType type;
        public P2PPackage(object v,int id, P2PPackageType type)
        {
            this.type = type;
            value = v;
            Object_identity = id;
        }
    }

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


}

