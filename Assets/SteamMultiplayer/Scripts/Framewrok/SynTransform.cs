using SteamMultiplayer;
using Steamworks;
using UnityEngine;

namespace SteamMultiplayer
{
    public class SynTransform : SteamNetworkBehaviour
    {

        public int TimesPerSecond = 9;
        private float CurrentTime;

        private Vector3 TargetPosition;

        void Update()
        {
            if (IsLocalObject)
            {
                CurrentTime -= Time.deltaTime;
                if (CurrentTime <= 0)
                {
                    CurrentTime = 1 / 9;
                    NetworkControl.SendPackets(
                        new P2PPackage(new Lib.M_Vector3(transform.position), P2PPackageType.位移同步,
                            identity),
                        EP2PSend.k_EP2PSendUnreliable, false);
                }
            }
            if (!IsLocalObject) transform.position = Vector3.Lerp(transform.position, TargetPosition, 1f);
        }

        public void Receive(Vector3 data)
        {
            TargetPosition = data;
        }
    }
}
