using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyList : MonoBehaviour
{
    public static MyList instance;
    public List<Transform> t=new List<Transform>();
    void Awake()
    {
        instance = this;
    }
}
