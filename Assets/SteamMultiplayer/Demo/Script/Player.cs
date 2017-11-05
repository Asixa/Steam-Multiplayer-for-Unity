using System.Collections;
using System.Collections.Generic;
using SteamMultiplayer;
using UnityEngine;
using UnityStandardAssets._2D;

public class Player : SteamNetworkBehaviour
{
    public Platformer2DUserControl control;
    public PlatformerCharacter2D chara;
    public SpriteRenderer so_renderer;
    public List<Color32> colors;

    private int current_color;
	// Use this for initialization
    private void Start () {

	    if (!identity.IsLocalSpawned)
	    {
	        control.enabled = false;
	        chara.enabled = false;
	    }
	}

    private void Update()
    {
        if (identity.IsLocalSpawned)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                if (current_color < 2) current_color++;
                else current_color = 0;
                rpcCall(0,current_color);
            }
        }
    }

    public void ChangeColor(int id)
    {
        so_renderer.color = colors[id];
    }
	
}
