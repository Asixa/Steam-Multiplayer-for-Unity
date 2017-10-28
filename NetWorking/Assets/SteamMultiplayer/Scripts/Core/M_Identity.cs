using UnityEngine;
public class M_Identity : MonoBehaviour
{
    [Locked] public int ID;
    [Locked] public bool IsLocalSpawned;
    [Locked] public int SpawnID;
    public M_Identity()
    {
        ID = SMC.OnlineObjects.Count;
        SMC.OnlineObjects.Add(this);
    }
}
public class LockedAttribute : PropertyAttribute{}