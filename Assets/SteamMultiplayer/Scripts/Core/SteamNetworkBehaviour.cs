//=============================================
// Network connection base class
//=============================================

using System;
using SteamMultiplayer.Lobby;
using Steamworks;
using UnityEngine;

namespace SteamMultiplayer.Core
{
    [RequireComponent(typeof(Identity))]
    public class SteamNetworkBehaviour : MonoBehaviour
    {
        public void Awake()
        {
            _identity = GetComponent<Identity>();
        }

        public bool IsLocalObject{get { return identity.IsLocalSpawned; }}
        public bool IsHost { get { return NetworkLobbyManager.instance.isHost; } }
        [HideInInspector]
        public Identity identity { get { return _identity ?? (_identity = GetComponent<Identity>()); } }
        private Identity _identity;
        public void rpcCall(int funcIndex, params object[] values)
        {
            NetworkControl.SendPackets(new P2PPackage(new NetworkControl.RPCInfo(funcIndex, values), P2PPackageType.RPC,identity), EP2PSend.k_EP2PSendReliable);
        }
    }

    public enum P2PPackageType
    {
        SyncTransform,
        SeverClose,
        Instantiate,
        Broadcast,
        SendMessage,
        Sync,
        DeleteObject,
        LoadScene,
        RPC,
        Custom,
        AnimatorState,
        AnimatorParamter,
        LeftLobby
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

