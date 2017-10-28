using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnableObjects : ScriptableObject
{
    public List<M_Identity> objects=new List<M_Identity>();

    public void Init()
    {
        for (var i = 0; i < objects.Count; i++)
        {
            objects[i].SpawnID = i;
        }
    }
}
