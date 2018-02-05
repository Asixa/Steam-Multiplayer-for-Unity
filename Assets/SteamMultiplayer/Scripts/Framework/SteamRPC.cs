using System.Collections.Generic;
using System.Reflection;
using SteamMultiplayer.Core;
using UnityEngine;

namespace SteamMultiplayer.Framework
{
    [AddComponentMenu("SteamMultiplayer/SteamRPC")]
    public class SteamRPC : SteamNetworkBehaviour
    {
        [System.Serializable]
        public class Entry
        {
            public Component target;
            public string method_name;
            public MethodInfo method;
        }
        public List<Entry> entries = new List<Entry>();
        public List<Entry> mList = new List<Entry>();
        public object[] m_cached = null;

        void Awake()
        {
            GetComponent<Identity>().rpc = this;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                var tns = GetComponents<SteamSync>();
                if (tns.Length <= 1 || tns[0] == this) return;
                Debug.LogError("Can't have more than one " + GetType() + " per game object", gameObject);
                DestroyImmediate(this);
            }
            else
#endif
            {
               Init();
            }
        }

        public void Init()
        {
            for (int i = 0, imax = entries.Count; i < imax; ++i)
            {
                var ent = entries[i];
                if (ent.target == null) continue;
                var method = ent.target.GetType()
                    .GetMethod(ent.method_name, BindingFlags.Instance | BindingFlags.Public);
                if (method == null) continue;
                var ext = new Entry
                {
                    target = ent.target,
                    method = method,
                    method_name = ent.method_name,
                };
                ent.method = method;
                mList.Add(ext);
            }
        }

        public void Call(int id,object[] p=null)
        {
            mList[id].method.Invoke(mList[id].target, p);
        }
    }
}