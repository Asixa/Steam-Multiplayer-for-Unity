using System;
using UnityEngine;

namespace SteamMultiplayer
{
    public enum P2PPackageType
    {
        Undefined,
        Method,
        Int,
        String,
        Float
    }

    public class P2PPackage
    {
        public object value;
        public ulong Object_identity;
        public P2PPackageType type;
        public P2PPackage(object v,ulong id, P2PPackageType type)
        {
            this.type = type;
            value = v;
            Object_identity = id;
        }
        
    }

    [RequireComponent(typeof(M_Identity))]
    public class SteamNetworkBehaviour : MonoBehaviour
    {
        public SteamNetworkBehaviour()
        {
            identity = GetComponent<M_Identity>();
        }

        public void SendP2P(object value)
        {
           SMC.SendPackets(new P2PPackage(value, identity.ID,P2PPackageType.Undefined));
        }

        public M_Identity identity { get; set; }
    }

    public class SMO : MonoBehaviour
    {
        public GameObject Spawn(GameObject orginal, Vector3 position=new Vector3(), Quaternion rotation=new Quaternion())
        {
            var id = orginal.GetComponent<M_Identity>();
            if (id==null) {
                throw new Exception("The object is not spawnable");
                return null;
            }
            var obj = Instantiate(id, position, rotation);
            SMC.OnlineObjects.Add(obj);
            return obj.gameObject;  
        }

        public void Delete(GameObject g)
        {
            Destroy(g);
            var id = g.GetComponent<M_Identity>();
            SMC.OnlineObjects.Remove(id);
        }
        
    }

}

