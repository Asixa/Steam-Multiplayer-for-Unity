using System;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections.Generic;
using System.Net;
using SteamMultiplayer;

[CustomEditor(typeof(SteamSync))]
public class SyncInspector : Editor
{
	public override void OnInspectorGUI ()
	{
        var sync = target as SteamSync;
		var components = GetComponents(sync);
		var names = GetComponentNames(components);
	    GUILayout.Space(10);
	    GUILayout.BeginVertical("Steam Multiplayer Sync", "window", GUILayout.Height(10));
        EditorGUILayout.BeginVertical("box");
        for (var i = 0; i < sync.entries.Count; )
		{
			GUILayout.BeginHorizontal();
			{
				if (DrawTarget(sync, i, components, names))
				{
					DrawProperties(sync, sync.entries[i], i);
					++i;
				}
			}
			GUILayout.EndHorizontal();
		}
	    
        GUI.backgroundColor = Color.green;
		if (GUILayout.Button("Add a New Synchronized Property"))
		{
		    var ent = new SteamSync.SavedEntry {target = components[0]};
		    sync.entries.Add(ent);
			EditorUtility.SetDirty(sync);
		}

		GUI.backgroundColor = Color.white;
	    EditorGUILayout.EndVertical();

        GUILayout.Space(4f);
	    EditorGUILayout.BeginVertical("box");
        sync.syn_mode = (SyncMode)EditorGUILayout.EnumPopup("Mode", sync.syn_mode);
        var updates = EditorGUILayout.IntField("Updates Per Second", sync.updatesPerSecond);
        EditorGUILayout.EndVertical();
	    GUILayout.EndVertical();
        if (sync.updatesPerSecond != updates)
		{
			EditorUtility.SetDirty(sync);
		    sync.updatesPerSecond = updates;
		}
	}

    private static List<Component> GetComponents (SteamSync sync)
	{
		var comps = sync.GetComponents<Component>();
		var list = new List<Component>();
		for (int i = 0, imax = comps.Length; i < imax; ++i)
		{
			if (comps[i] != sync && comps[i].GetType() != typeof(MonoBehaviour))
			{
				list.Add(comps[i]);
			}
		}
		return list;
	}

    private static string[] GetComponentNames (List<Component> list)
	{
		var names = new string[list.Count+ 1];
		names[0] = "<None>";
		for (var i = 0; i < list.Count; ++i)
			names[i + 1] = list[i].GetType().ToString();
		return names;
	}

    private static bool DrawTarget (SteamSync sync, int index, List<Component> components, string[] names)
	{
		var ent = sync.entries[index];
		if (ent.target == null)
		{
			ent.target = components[0];
			EditorUtility.SetDirty(sync);
		}
		var old_index = 0;
		var tname = (ent.target != null) ? ent.target.GetType().ToString() : "<None>";
		
		for (var i = 1; i < names.Length; ++i)
		{
		    if (names[i] != tname) continue;
		    old_index = i;
		    break;
		}


		GUI.backgroundColor = Color.white;
		var new_index = EditorGUILayout.Popup(old_index, names);


		if (new_index != old_index)
		{
			ent.target = (new_index == 0) ? null : components[new_index - 1];
			ent.property_name = "";
			EditorUtility.SetDirty(sync);
		}

		return true;
	}

    private static void DrawProperties (SteamSync sync, SteamSync.SavedEntry saved,int index)
	{
		if (saved.target == null) return;

		var fields = saved.target.GetType().GetFields(
			BindingFlags.Instance | BindingFlags.Public);

		var properties = saved.target.GetType().GetProperties(
			BindingFlags.Instance | BindingFlags.Public);

		var old_index = 0;
	    var names = new List<string> {"<None>"};

	    foreach (var t in fields)
		{
		    if (!CanBeSerialized(t.FieldType)) continue;
		    if (t.Name == saved.property_name) old_index = names.Count;
		    names.Add(t.Name);
		} 
		foreach (var pi in properties)
		{
		    if (!CanBeSerialized(pi.PropertyType) || !pi.CanWrite || !pi.CanRead) continue;
		    if (pi.Name == saved.property_name) old_index = names.Count;
		    names.Add(pi.Name);
		}

		var newIndex = EditorGUILayout.Popup(old_index, names.ToArray());
	    GUI.backgroundColor = Color.red;
	    var delete = GUILayout.Button("X", GUILayout.Width(24f), GUILayout.Height(14f));

	    if (delete)
	    {
	        sync.entries.RemoveAt(index);
	        EditorUtility.SetDirty(sync);
	        return;
	    }
        if (newIndex != old_index)
		{
			saved.property_name = (newIndex == 0) ? "" : names[newIndex];
			EditorUtility.SetDirty(sync);
		}
	}

    public static bool CanBeSerialized(Type type)
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
        if (type == typeof(Lib.M_Vector3)) return true;
        if (type == typeof(Lib.M_Quaternion)) return true;
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
