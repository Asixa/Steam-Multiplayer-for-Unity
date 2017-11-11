using SteamMultiplayer;
using UnityEngine;

public class DemoManager : MonoBehaviour
{
    public Identity Player;
    public Transform point;
    public void Start()
    {
        var player = NetworkControl.instance.Spawn(Player, point.position, point.rotation) as Identity;
        player.transform.position = point.position;
    }
}
