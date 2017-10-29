//=============================================
//这个类是用来设定物体的网络ID
//创建者 Asixa 2017-9-x
//最新修改 Asixa 2017-10-29
//=============================================
using UnityEngine;
public class M_Identity : MonoBehaviour
{
    [Locked] public int ID=-1;
     public bool IsLocalSpawned;
    [Locked] public int SpawnID;
    public bool AutoInit;
    public int TargetID = -1;

    public void Init()
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