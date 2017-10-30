using System;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections.Generic;
using System.Net;
using SteamMultiplayer;

[CustomEditor(typeof(AutoSync))]
public class AutoSyncInspector : Editor
{
	public override void OnInspectorGUI ()
	{
		AutoSync sync = target as AutoSync;

		List<Component> components = GetComponents(sync);
		string[] names = GetComponentNames(components);

        EditorGUILayout.BeginVertical("box");
        for (int i = 0; i < sync.entries.Count; )
		{
			GUILayout.BeginHorizontal();
			{
				if (DrawTarget(sync, i, components, names))
				{
					DrawProperties(sync, sync.entries[i]);
					++i;
				}
			}
			GUILayout.EndHorizontal();
		}
	    
        GUI.backgroundColor = Color.green;

		if (GUILayout.Button("Add a New Synchronized Property"))
		{
			AutoSync.SavedEntry ent = new AutoSync.SavedEntry();
			ent.target = components[0];
			sync.entries.Add(ent);
			EditorUtility.SetDirty(sync);
		}
		GUI.backgroundColor = Color.white;
	    EditorGUILayout.EndVertical();

        GUILayout.Space(4f);
	    EditorGUILayout.BeginVertical("box");
        sync.synMode = (SyncMode)EditorGUILayout.EnumPopup("Mode", sync.synMode);
        int updates = EditorGUILayout.IntField("Updates Per Second", sync.updatesPerSecond);
	    EditorGUILayout.EndVertical();

        if (sync.updatesPerSecond != updates)
		{
			EditorUtility.SetDirty(sync);
		}
	}

	static List<Component> GetComponents (AutoSync sync)
	{
		Component[] comps = sync.GetComponents<Component>();

		List<Component> list = new List<Component>();

		for (int i = 0, imax = comps.Length; i < imax; ++i)
		{
			if (comps[i] != sync && comps[i].GetType() != typeof(SteamMultiplayer.SteamNetworkBehaviour))
			{
				list.Add(comps[i]);
			}
		}
		return list;
	}

	static string[] GetComponentNames (List<Component> list)
	{
		string[] names = new string[list.Count+ 1];
		names[0] = "<None>";
		for (int i = 0; i < list.Count; ++i)
			names[i + 1] = list[i].GetType().ToString();
		return names;
	}

	static bool DrawTarget (AutoSync sync, int index, List<Component> components, string[] names)
	{
		AutoSync.SavedEntry ent = sync.entries[index];
		
		if (ent.target == null)
		{
			ent.target = components[0];
			EditorUtility.SetDirty(sync);
		}

		int oldIndex = 0;
		string tname = (ent.target != null) ? ent.target.GetType().ToString() : "<None>";
		
		for (int i = 1; i < names.Length; ++i)
		{
			if (names[i] == tname)
			{
				oldIndex = i;
				break;
			}
		}

		GUI.backgroundColor = Color.red;
        bool delete = GUILayout.Button("X", GUILayout.Width(24f),GUILayout.Height(14f));
		GUI.backgroundColor = Color.white;
		int newIndex = EditorGUILayout.Popup(oldIndex, names);

		if (delete)
		{
			sync.entries.RemoveAt(index);
			EditorUtility.SetDirty(sync);
			return false;
		}

		if (newIndex != oldIndex)
		{
			ent.target = (newIndex == 0) ? null : components[newIndex - 1];
			ent.propertyName = "";
			EditorUtility.SetDirty(sync);
		}

		return true;
	}

	static void DrawProperties (AutoSync sync, AutoSync.SavedEntry saved)
	{
		if (saved.target == null) return;

		FieldInfo[] fields = saved.target.GetType().GetFields(
			BindingFlags.Instance | BindingFlags.Public);

		PropertyInfo[] properties = saved.target.GetType().GetProperties(
			BindingFlags.Instance | BindingFlags.Public);

		int oldIndex = 0;
		List<string> names = new List<string>();
		names.Add("<None>");

		for (int i = 0; i < fields.Length; ++i)
		{
			if (CanBeSerialized(fields[i].FieldType))
			{
				if (fields[i].Name == saved.propertyName) oldIndex = names.Count;
				names.Add(fields[i].Name);
			}
		} 
		
		for (int i = 0; i < properties.Length; ++i)
		{
			PropertyInfo pi = properties[i];

			if (CanBeSerialized(pi.PropertyType) && pi.CanWrite && pi.CanRead)
			{
				if (pi.Name == saved.propertyName) oldIndex = names.Count;
				names.Add(pi.Name);
			}
		}

		int newIndex = EditorGUILayout.Popup(oldIndex, names.ToArray(), GUILayout.Width(90f));

		if (newIndex != oldIndex)
		{
			saved.propertyName = (newIndex == 0) ? "" : names[newIndex];
			EditorUtility.SetDirty(sync);
		}
	}
    static public bool CanBeSerialized(Type type)
    {
        if (type == typeof(bool)) return true;
        if (type == typeof(byte)) return true;
        if (type == typeof(ushort)) return true;
        if (type == typeof(int)) return true;
        if (type == typeof(uint)) return true;
        if (type == typeof(float)) return true;
        if (type == typeof(string)) return true;
        if (type == typeof(Vector2)) return true;
        if (type == typeof(Vector3)) return true;
        if (type == typeof(Vector4)) return true;
        if (type == typeof(Quaternion)) return true;
        if (type == typeof(Color32)) return true;
        if (type == typeof(Color)) return true;
        if (type == typeof(DateTime)) return true;
        if (type == typeof(IPEndPoint)) return true;
        if (type == typeof(bool[])) return true;
        if (type == typeof(byte[])) return true;
        if (type == typeof(ushort[])) return true;
        if (type == typeof(int[])) return true;
        if (type == typeof(uint[])) return true;
        if (type == typeof(float[])) return true;
        if (type == typeof(string[])) return true;
        return false;
    }
}
