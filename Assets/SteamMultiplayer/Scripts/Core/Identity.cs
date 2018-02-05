//=============================================
//This class aims to set the identity of a network object
//=============================================

using SteamMultiplayer.Framework;
using Steamworks;
using UnityEngine;

namespace SteamMultiplayer.Core
{
    [AddComponentMenu("SteamMultiplayer/Identity")]
    public class Identity : MonoBehaviour
    {
        public int ID = -1;             // The index of the network objects list;
        public bool IsLocalSpawned;     // To Check if this object is spawn by current client;
        public int SpawnID;             // The index of the spawnable objects list;
        public int TargetID = -1;       // The id that this object will be in other clients;
        public SteamSync sync;          // SteamSync Conponent;
        public SteamRPC rpc;            // SteamRPC Conponent;
        public SteamAnimator anim;      // SteamAnimator Conponent;
        public bool DestoryOnQuit;      // if this is True,this object will be destoried when the player leave the game;
        public CSteamID host;           // The SteamID of the host player

        public void Init()
        {
            if (TargetID == -1) //If TargetID equal -1, that means this object is spawned by localplayer, it will be set a new ID;
            {
                ID = NetworkControl.instance.OnlineObjects.Count;
                NetworkControl.instance.OnlineObjects.Add(this);
            }
            else // if Target ID doesnt equal -1,that means this object is spawn by other player, it will by set the ID of its owner;
            {
                ID = TargetID;
                while (NetworkControl.instance.OnlineObjects.Count <= TargetID)
                {
                    NetworkControl.instance.OnlineObjects.Add(null);
                }
                if (NetworkControl.instance.OnlineObjects[TargetID] != null)
                {
                    Debug.LogError("物体ID发生冲突");
                }
                NetworkControl.instance.OnlineObjects[TargetID] = this;
            }
        }
    }
}
