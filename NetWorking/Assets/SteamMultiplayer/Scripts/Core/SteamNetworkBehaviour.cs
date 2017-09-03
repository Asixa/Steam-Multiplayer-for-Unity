using System;
using System.Threading;
using UnityEngine;

namespace SteamMultiplayer
{
    public class P2PPackage
    {
        public object value;
        public ulong Object_identity;
        public int type;
        public P2PPackage(object v,ulong id,int type)
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

        public void Send(object value)
        {
            new P2PPackage(value, identity.ID,P2PPackageType.undefined);
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

        public bool IsP2PHost{get { return SMC.IsP2PHost;;}}
    }

    public class P2PPackageType
    {
        public const int
            undefined = 0,
            spawn = 1
        ;
    }
}

