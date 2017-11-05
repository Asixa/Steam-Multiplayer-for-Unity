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

    private int currentColor;
	// Use this for initialization
	void Start () {

	    if (!identity.IsLocalSpawned)
	    {
	        control.enabled = false;
	        chara.enabled = false;
	    }
	}

    void Update()
    {
        if (identity.IsLocalSpawned)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                if (currentColor < 2) currentColor++;
                else currentColor = 0;
                rpcCall(0,currentColor);
            }
        }
    }

    public void ChangeColor(int id)
    {
        print("改变颜色："+id);
        so_renderer.color = colors[id];
    }
	
}
