using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

namespace SteamMultiplayer
{
    public enum SyncMode
    {
        Safely,Quickly
    }
    [AddComponentMenu("SteamMultiplayer/SteamSync")]
    [ExecuteInEditMode]
    [RequireComponent(typeof(Identity))]
    public class SteamSync :SteamNetworkBehaviour
    {
        [System.Serializable]
        public class SavedEntry
        {
            public Component target;
            public string propertyName;
        }
        public List<SavedEntry> entries = new List<SavedEntry>();

        public int updatesPerSecond = 20;
        private float CurrentTime;
        public SyncMode syn_mode;

       public class ExtendedEntry : SavedEntry
        {
            public FieldInfo field;
            public PropertyInfo property;
            public object last_value;
        }

        public List<ExtendedEntry> mList = new List<ExtendedEntry>();
        object[] mCached = null;

        void Awake()
        {
            GetComponent<Identity>().sync = this;
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
                // Find all properties, converting the saved list into the usable list of reflected properties
                for (int i = 0, imax = entries.Count; i < imax; ++i)
                {
                    var ent = entries[i];
                    if (ent.target == null || string.IsNullOrEmpty(ent.propertyName)) continue;
                    var field = ent.target.GetType()
                        .GetField(ent.propertyName, BindingFlags.Instance | BindingFlags.Public);
                    if (field != null)
                    {
                        var ext = new ExtendedEntry
                        {
                            target = ent.target,
                            field = field,
                            last_value = field.GetValue(ent.target)
                        };
                        mList.Add(ext);
                        continue;
                    }
                    else
                    {
                        var pro = ent.target.GetType()
                            .GetProperty(ent.propertyName, BindingFlags.Instance | BindingFlags.Public);

                        if (pro != null)
                        {
                            var ext = new ExtendedEntry
                            {
                                target = ent.target,
                                property = pro,
                                last_value = pro.GetValue(ent.target, null)
                            };
                            mList.Add(ext);
                            continue;
                        }
                        else
                        {
                            Debug.LogError("Unable to find property: '" + ent.propertyName + "' on " +
                                           ent.target.GetType());
                        }
                    }
                }
            }
        }

        public void Update()
        {
        if(!Application.isPlaying)return;
            CurrentTime -= Time.deltaTime;
            if (mList.Count <= 0) return;
            if (updatesPerSecond <= 0) return;
            if (!(CurrentTime <= 0)) return;
            CurrentTime = 1f / updatesPerSecond;
            Sync();
        }

        public void Sync()
        {
            if (identity == null)
            {
                identity = GetComponent<Identity>();
            }
            if (!identity.IsLocalSpawned)return;
            
            if (mList.Count == 0 || !enabled) return;
            var initial = false;
            var changed = false;

            if (mCached == null)
            {
                initial = true;
                mCached = new object[mList.Count];
            }

            for (var i = 0; i < mList.Count; ++i)
            {
                var ext = mList[i];

                object val = (ext.field != null)
                    ? val = ext.field.GetValue(ext.target)
                    : val = ext.property.GetValue(ext.target, null);
                if(val.GetType()==(typeof(Vector3)))val=new Lib.M_Vector3((Vector3)val);
                if (val.GetType() == (typeof(Quaternion))) val = new Lib.M_Quaternion((Quaternion)val);

                if (!val.Equals(ext.last_value))
                    changed = true;

                if (!initial && !changed) continue;
                ext.last_value = val;
                mCached[i] = val;
            }

            if (!changed) return;

            if (syn_mode==SyncMode.Quickly)
            {
                SMC.SendPacketsQuicklly(new P2PPackage(mCached,P2PPackageType.Sync,identity),false);
            }
            else
            {
                SMC.SendPacketsSafely(new P2PPackage(mCached, P2PPackageType.Sync, identity), false);
            }
        }

        public void OnSync(object[] val)
        {
            if (!enabled) return;
            for (var i = 0; i < mList.Count; ++i)
            {
                ExtendedEntry ext = mList[i];
                ext.last_value = val[i];
                if (ext.field != null)
                {
                    if (ext.field.FieldType == typeof(Vector3))
                    {
                        ext.field.SetValue(ext.target, Lib.To_Vector3((Lib.M_Vector3)ext.last_value));
                    }
                    else if (ext.field.FieldType == typeof(Quaternion))
                    {
                        ext.field.SetValue(ext.target, Lib.To_Quaternion((Lib.M_Quaternion)ext.last_value));
                    }
                    else
                    {
                        ext.field.SetValue(ext.target, ext.last_value);
                    }    
                }
                else
                {
                    if (ext.property.PropertyType == typeof(Vector3))
                    {
                        ext.property.SetValue(ext.target, Lib.To_Vector3((Lib.M_Vector3)ext.last_value),null);
                    }
                    else if (ext.property.PropertyType == typeof(Quaternion))
                    {
                        ext.property.SetValue(ext.target, Lib.To_Quaternion((Lib.M_Quaternion)ext.last_value),null);
                    }
                    else
                    {
                        ext.property.SetValue(ext.target, ext.last_value, null);
                    }
                }
            }
        }
    }
}