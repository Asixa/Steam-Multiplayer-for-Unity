using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Statistics : MonoBehaviour {

    public static ulong UpstreamPerSec;
    public static ulong DownstreamPerSec;
    public static ulong UpstreamTotal;
    public static ulong DownstreamTotal;

    void Start()
    {
        InvokeRepeating("Zero", 1, 1);
    }

    void Zero()
    {
        UpstreamPerSec = 0;
        DownstreamPerSec = 0;
    }

    public static void Upstream(ulong l)
    {
        UpstreamPerSec += l;
        UpstreamTotal += l;
    }

    public static void Downstream(ulong l)
    {
        DownstreamPerSec += l;
        DownstreamTotal += l;
    }
}
