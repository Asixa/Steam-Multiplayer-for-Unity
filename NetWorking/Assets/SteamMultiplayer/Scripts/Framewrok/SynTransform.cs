using SteamMultiplayer;
using Steamworks;
using UnityEngine;

public class SynTransform : SteamNetworkBehaviour
{
    public int TimesPerSecond = 9;
    private float CurrentTime;

    private Vector3 TargetPosition;

	void Update () {
		CurrentTime-=Time.deltaTime;
	    if (CurrentTime <= 0)
	    {
	        SendP2P(new P2PPackage(transform.position,ID,P2PPackageType.Undefined),EP2PSend.k_EP2PSendUnreliable);
	    }
	    transform.position = Vector3.Lerp(transform.position, TargetPosition, 1f);
	}

    public void Receive(object data)
    {
        TargetPosition = (Vector3)data;
    }
}
