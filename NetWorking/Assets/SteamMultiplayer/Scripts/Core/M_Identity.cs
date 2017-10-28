using UnityEngine;
public class M_Identity : MonoBehaviour
{
    [Locked] public int ID;
     public bool IsLocalSpawned;
    [Locked] public int SpawnID;

    public bool AutoInit;

    public void Start()
    {
        if(AutoInit)Init();
    }

    public void Init(int TargetID=-1)
    {
        if (TargetID == -1)
        {
            ID = SMC.instance. OnlineObjects.Count;
            SMC.instance.OnlineObjects.Add(this);
        }
        else
        {
            ID = TargetID;
            while (SMC.instance.OnlineObjects.Count <= TargetID)
            {
                 SMC.instance.OnlineObjects.Add(null);   
            }
            if (SMC.instance.OnlineObjects[TargetID] != null)
            {
                Debug.LogError("奇怪的物体");
            }
            SMC.instance.OnlineObjects[TargetID] = this;
        }
    }
}
public class LockedAttribute : PropertyAttribute{}