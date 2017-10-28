using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoManager : MonoBehaviour
{
    public M_Identity Player;
    public Transform point;
    public void SpawnPlayer()
    {
        SMC.instance.Spawn(Player, point.position, point.rotation);
    }
}
