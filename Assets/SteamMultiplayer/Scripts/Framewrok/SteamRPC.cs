using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace SteamMultiplayer
{
    [AddComponentMenu("SteamMultiplayer/RPC")]
    public class SteamRPC : SteamNetworkBehaviour
    {
        [System.Serializable]
        public class Entry
        {
            public Component target;
            public string MethodName;
            public MethodInfo method;
        }
        public List<Entry> entries = new List<Entry>();

        //public class ExtendedEntry : Entry
        //{
        //    public MethodInfo method;
        //}

        public List<Entry> mList = new List<Entry>();
        object[] mCached = null;

        void Awake()
        {
            GetComponent<Identity>().rpc = this;
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                var tns = GetComponents<SteamSync>();
                if (tns.Length > 1 && tns[0] != this)
                {
                    Debug.LogError("Can't have more than one " + GetType() + " per game object", gameObject);
                    DestroyImmediate(this);
                }
            }
            else
#endif
            {
               Set();
            }

        }

        public void Set()
        {
            for (int i = 0, imax = entries.Count; i < imax; ++i)
            {
                var ent = entries[i];
                if (ent.target != null)
                {
                    var method = ent.target.GetType()
                        .GetMethod(ent.MethodName, BindingFlags.Instance | BindingFlags.Public);

                    if (method != null)
                    {
                        var ext = new Entry
                        {
                            target = ent.target,
                            method = method,
                            MethodName = ent.MethodName,
                        };
                        ent.method = method;
                        mList.Add(ext);
                        continue;
                    }
                }
            }
        }
    }
}