using SteamMultiplayer;
using UnityEngine;

public class DemoManager : MonoBehaviour
{
    public Identity Player;
    public Transform point;
    public void SpawnPlayer()
    {
        var player = SMC.instance.Spawn(Player, point.position, point.rotation) as Identity;
        player.transform.position = point.position;
    }
}
