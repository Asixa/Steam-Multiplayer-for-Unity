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

        var rpc = target as SteamRPC;

        var components = GetComponents(rpc);
        var names = GetComponentNames(components);
	    GUILayout.Space(10);
        GUILayout.BeginVertical("Steam Multiplayer RPC", "window", GUILayout.Height(10),GUILayout.Width(50));
        EditorGUILayout.BeginVertical("box");
        for (var i = 0; i < rpc.entries.Count;)
        {
            GUILayout.BeginHorizontal();
            {
                if (DrawTarget(rpc, i, components, names))
                {
                    DrawProperties(rpc, rpc.entries[i],i);
                    ++i;
                }
            }
            GUILayout.EndHorizontal();
        }
        GUI.backgroundColor = Color.green;

        if (GUILayout.Button("Add a New Sync Function"))
        {
            var ent = new SteamRPC.Entry { target = components[0] };
            rpc.entries.Add(ent);
            EditorUtility.SetDirty(rpc);
        }
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndVertical();
	    GUILayout.EndVertical();
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

    private static string[] GetComponentNames(List<Component> list)
    {
        var names = new string[list.Count + 1];
        names[0] = "<None>";
        for (var i = 0; i < list.Count; ++i)
            names[i + 1] = list[i].GetType().ToString();
        return names;
    }

    private static bool DrawTarget(SteamRPC _rpc, int index, List<Component> components, string[] names)
    {
        var ent = _rpc.entries[index];
        if (ent.target == null)
        {
            ent.target = components[0];
            EditorUtility.SetDirty(_rpc);
        }

        var oldIndex = 0;
        var tname = (ent.target != null) ? ent.target.GetType().ToString() : "<None>";

        for (var i = 1; i < names.Length; ++i)
        {
            if (names[i] != tname) continue;
            oldIndex = i;
            break;
        }


        GUI.skin.label.alignment=TextAnchor.MiddleCenter;

        // EditorGUILayout.HelpBox(index.ToString(), MessageType.None);
        EditorGUILayout.LabelField(index.ToString(), GUILayout.Width(20f), GUILayout.Height(14f));
        
        
        GUI.backgroundColor = Color.white;
        var newIndex = EditorGUILayout.Popup(oldIndex, names);



        if (newIndex != oldIndex)
        {
            ent.target = (newIndex == 0) ? null : components[newIndex - 1];
            EditorUtility.SetDirty(_rpc);
        }
        return true;
    }

    private static void DrawProperties(SteamRPC _rpc, SteamRPC.Entry nowEntry,int index)
    {
        if (nowEntry.target == null) return;

        var Methods = nowEntry.target.GetType().GetMethods(
            BindingFlags.Instance | BindingFlags.Public);
        Component igorne = new MonoBehaviour();
        var igorne_methond = igorne.GetType().GetMethods(
            BindingFlags.Public | BindingFlags.Instance);

        var oldIndex = 0;
        var names = new List<string> {"<None>"};
        var avalible_mthods = (from t in Methods let ignore = igorne_methond.Any(t1 => t1.Name == t.Name) where !ignore select t).ToList();

        foreach (var t in avalible_mthods)
        {
            if (t.Name == nowEntry.method_name)oldIndex = names.Count;
            names.Add(t.Name);
        }

        var new_index = EditorGUILayout.Popup(oldIndex, names.ToArray());
        GUI.backgroundColor = Color.red;
        var delete = GUILayout.Button("X", GUILayout.Width(24f), GUILayout.Height(14f));
        if (delete)
        {
            _rpc.entries.RemoveAt(index);
            EditorUtility.SetDirty(_rpc);
            return;
        }
        if (new_index == oldIndex) return;
        nowEntry.method = (new_index == 0) ? null : avalible_mthods[new_index - 1];
        nowEntry.method_name =nowEntry.method==null?"<None>": nowEntry.method.Name;
        EditorUtility.SetDirty(_rpc);
    }
}
