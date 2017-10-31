
using System;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using SteamMultiplayer;

[CustomEditor(typeof(SteamRPC))]
public class RPCInspector : Editor
{
	public override void OnInspectorGUI ()
	{

        SteamRPC rpc = target as SteamRPC;

        List<Component> components = GetComponents(rpc);
        string[] names = GetComponentNames(components);

        EditorGUILayout.BeginVertical("box");
        for (int i = 0; i < rpc.entries.Count;)
        {
            GUILayout.BeginHorizontal();
            {
                if (DrawTarget(rpc, i, components, names))
                {
                    DrawProperties(rpc, rpc.entries[i]);
                    ++i;
                }
            }
            GUILayout.EndHorizontal();
        }
        GUI.backgroundColor = Color.green;

        if (GUILayout.Button("Add a New RPC Function"))
        {
            var ent = new SteamRPC.Entry { target = components[0] };
            rpc.entries.Add(ent);
            EditorUtility.SetDirty(rpc);
        }
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndVertical();

        //*******************DEBUG*******************
        //   GUILayout.Space(4f);
        //   EditorGUILayout.BeginVertical("box");
        //GUILayout.Label(rpc.entries.Count.ToString());
        //   foreach (var t in rpc.entries)
        //    GUILayout.Label(t.target.GetType() + " -- " +t.MethodName+"--"+ (t.method == null ? "null" : t.method.Name));
        //   GUILayout.Label(rpc.mList.Count.ToString());
        //foreach (var t in rpc.mList)
        //    GUILayout.Label(t.target.GetType() + " -- " + t.MethodName + "--" + (t.method == null ? "null" : t.method.Name));
        //EditorGUILayout.EndVertical();
	    //*******************DEBUG*******************
    }

    private static List<Component> GetComponents(SteamRPC sync)
    {
        Component[] comps = sync.GetComponents<Component>();

        List<Component> list = new List<Component>();

        for (int i = 0, imax = comps.Length; i < imax; ++i)
        {
            if (comps[i] != sync && comps[i].GetType() != typeof(MonoBehaviour))
            {
                list.Add(comps[i]);
            }
        }
        return list;
    }

    private static string[] GetComponentNames(List<Component> list)
    {
        var names = new string[list.Count + 1];
        names[0] = "<None>";
        for (var i = 0; i < list.Count; ++i)
            names[i + 1] = list[i].GetType().ToString();
        return names;
    }

    private static bool DrawTarget(SteamRPC sync, int index, List<Component> components, string[] names)
    {
        var ent = sync.entries[index];
        if (ent.target == null)
        {
            ent.target = components[0];
            EditorUtility.SetDirty(sync);
        }

        int oldIndex = 0;
        string tname = (ent.target != null) ? ent.target.GetType().ToString() : "<None>";

        for (var i = 1; i < names.Length; ++i)
        {
            if (names[i] != tname) continue;
            oldIndex = i;
            break;
        }

        GUI.backgroundColor = Color.red;
        bool delete = GUILayout.Button("X", GUILayout.Width(24f), GUILayout.Height(14f));
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
            EditorUtility.SetDirty(sync);
        }

        return true;
    }

    private static void DrawProperties(SteamRPC _rpc, SteamRPC.Entry nowEntry)
    {
        if (nowEntry.target == null) return;

        MethodInfo[] Methods = nowEntry.target.GetType().GetMethods(
            BindingFlags.Instance | BindingFlags.Public);
        Component igorne = new MonoBehaviour();
        var igorne_methond = igorne.GetType().GetMethods(
            BindingFlags.Public | BindingFlags.Instance);

        int oldIndex = 0;
        List<string> names = new List<string>();
        names.Add("<None>");
        var AvalibleMthods = new List<MethodInfo>();
        foreach (var t in Methods)
        {
            var ignore = igorne_methond.Any(t1 => t1.Name == t.Name);
            if (ignore) continue;
            AvalibleMthods.Add(t);
        }

        foreach (var t in AvalibleMthods)
        {
            if (t.Name == nowEntry.MethodName)oldIndex = names.Count;
            names.Add(t.Name);
        }

        var new_index = EditorGUILayout.Popup(oldIndex, names.ToArray());

        if (new_index != oldIndex)
        {
            nowEntry.method = (new_index == 0) ? null : AvalibleMthods[new_index - 1];
            nowEntry.MethodName =nowEntry.method==null?"<None>": nowEntry.method.Name;
            EditorUtility.SetDirty(_rpc);
        }
    }
}
