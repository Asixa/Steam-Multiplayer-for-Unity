using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

namespace SteamMultiplayer
{
    public enum SyncMode
    {
        Safely,Quickly
    }
    [ExecuteInEditMode]
    [RequireComponent(typeof(Identity))]
    public class AutoSync :SteamNetworkBehaviour
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
        public SyncMode synMode;

        class ExtendedEntry : SavedEntry
        {
            public FieldInfo field;
            public PropertyInfo property;
            public object lastValue;
        }

        private List<ExtendedEntry> mList = new List<ExtendedEntry>();
        object[] mCached = null;

        void Awake()
        {
            GetComponent<Identity>().auto_sync = this;
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                AutoSync[] tns = GetComponents<AutoSync>();

                if (tns.Length > 1 && tns[0] != this)
                {
                    Debug.LogError("Can't have more than one " + GetType() + " per game object", gameObject);
                    DestroyImmediate(this);
                }
            }
            else
#endif
            {
                // Find all properties, converting the saved list into the usable list of reflected properties
                for (int i = 0, imax = entries.Count; i < imax; ++i)
                {
                    SavedEntry ent = entries[i];

                    if (ent.target != null && !string.IsNullOrEmpty(ent.propertyName))
                    {
                        FieldInfo field = ent.target.GetType()
                            .GetField(ent.propertyName, BindingFlags.Instance | BindingFlags.Public);

                        if (field != null)
                        {
                            ExtendedEntry ext = new ExtendedEntry
                            {
                                target = ent.target,
                                field = field,
                                lastValue = field.GetValue(ent.target)
                            };
                            mList.Add(ext);
                            continue;
                        }
                        else
                        {
                            PropertyInfo pro = ent.target.GetType()
                                .GetProperty(ent.propertyName, BindingFlags.Instance | BindingFlags.Public);

                            if (pro != null)
                            {
                                ExtendedEntry ext = new ExtendedEntry
                                {
                                    target = ent.target,
                                    property = pro,
                                    lastValue = pro.GetValue(ent.target, null)
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
                CurrentTime -= Time.deltaTime;
                if (mList.Count > 0)
                {
                    if (updatesPerSecond > 0)
                    {
                        if (CurrentTime <= 0)
                        {
                            CurrentTime = 1/updatesPerSecond;
                            Sync();
                        }
                    }
                    else enabled = false;
                }
                else enabled = false;
            }
        }

        public void Sync()
        {
            if (mList.Count == 0 || !enabled) return;
            bool initial = false;
            bool changed = false;

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

                if (!val.Equals(ext.lastValue))
                    changed = true;

                if (!initial && !changed) continue;
                ext.lastValue = val;
                mCached[i] = val;
            }

            if (!changed) return;

            if (synMode==SyncMode.Quickly)
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
                ext.lastValue = val[i];
                if (ext.field != null) ext.field.SetValue(ext.target, ext.lastValue);
                else ext.property.SetValue(ext.target, ext.lastValue, null);
            }
        }
    }
}