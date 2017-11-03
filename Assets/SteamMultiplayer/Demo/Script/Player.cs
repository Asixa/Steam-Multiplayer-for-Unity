using System.Collections;
using System.Collections.Generic;
using SteamMultiplayer;
using UnityEngine;
using UnityStandardAssets._2D;

public class Player : SteamNetworkBehaviour
{
    public Platformer2DUserControl control;
	// Use this for initialization
	void Start () {
	    if (!identity.IsLocalSpawned)
	    {
	        control.enabled = false;
	    }
	}
	
}
