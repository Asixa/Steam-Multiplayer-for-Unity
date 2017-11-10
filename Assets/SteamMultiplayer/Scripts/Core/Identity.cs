//=============================================
//这个类是用来设定物体的网络ID
//创建者 Asixa 2017-9-x
//最新修改 Asixa 2017-10-29
//=============================================

using Steamworks;
using UnityEngine;

namespace SteamMultiplayer
{
    [AddComponentMenu("SteamMultiplayer/Identity")]
    public class Identity : MonoBehaviour
    {
        public int ID = -1;
        public bool IsLocalSpawned;
        public int SpawnID;
        public int TargetID = -1;
        public SteamSync sync;
        public SteamRPC rpc;
        public SteamAnimator anim;
        public bool DestoryOnQuit;
        public CSteamID host;

        public void Init()
        {

            if (TargetID == -1)
            {
                ID = NetworkControl.instance.OnlineObjects.Count;
                NetworkControl.instance.OnlineObjects.Add(this);
            }
            else
            {
                ID = TargetID;
                while (NetworkControl.instance.OnlineObjects.Count <= TargetID)
                {
                    NetworkControl.instance.OnlineObjects.Add(null);
                }
                if (NetworkControl.instance.OnlineObjects[TargetID] != null)
                {
                    Debug.LogError("奇怪的物体");
                }
                NetworkControl.instance.OnlineObjects[TargetID] = this;
            }
        }
    }
}
