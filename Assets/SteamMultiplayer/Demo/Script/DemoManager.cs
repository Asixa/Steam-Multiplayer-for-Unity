using SteamMultiplayer;
using UnityEngine;

public class DemoManager : MonoBehaviour
{
    public Identity Player;
    public Transform point;
    public void SpawnPlayer()
    {
        SMC.instance.Spawn(Player, point.position, point.rotation);
    }
}
