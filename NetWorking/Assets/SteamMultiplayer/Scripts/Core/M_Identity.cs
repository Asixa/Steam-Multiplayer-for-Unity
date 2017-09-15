using UnityEngine;

public class M_Identity : MonoBehaviour
{
    [Locked] public ulong ID;
    [Locked] public bool IsLocalSpawned;
}
public class LockedAttribute : PropertyAttribute{}