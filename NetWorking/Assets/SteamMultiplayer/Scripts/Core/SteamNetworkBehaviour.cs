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
        Reserve
    }
    public class P2PPackage
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
        public SteamNetworkBehaviour()
        {
            identity = GetComponent<M_Identity>();
        }

        public void SendP2P(object value, EP2PSend send)
        {
           SMC.SendPackets(new P2PPackage(value, identity.ID,P2PPackageType.Undefined), send);
        }

        public int ID { get { return identity.ID; } }
        public M_Identity identity { get; set; }
    }


}

