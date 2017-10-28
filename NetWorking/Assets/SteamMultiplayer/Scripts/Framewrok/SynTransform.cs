using SteamMultiplayer;
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
	 
	    }
	    transform.position = Vector3.Lerp(transform.position, TargetPosition, 1f);
	}

    public void Receive(P2PPackage data)
    {
        TargetPosition = (Vector3)data.value;
    }
}
