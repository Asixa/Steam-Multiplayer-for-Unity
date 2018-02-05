using UnityEditor;
using SteamMultiplayer;
using SteamMultiplayer.Core;
using UnityEngine;

[CustomEditor(typeof(Identity))]
public class IdentityInspector : Editor
{
    public override void OnInspectorGUI()
    {
        var identity = target as Identity;
        var content = "ID:" + ((identity.ID==-1)? "Unassigned" : identity.ID.ToString());
        content += "   ";
        content +=identity.IsLocalSpawned ? "Local " : "Remote";
        content += "   ";
        content += "SpawnID:" +identity.SpawnID;
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label(content, EditorStyles.boldLabel);
        EditorGUILayout.EndVertical();
        identity.DestoryOnQuit = EditorGUILayout.Toggle("Destory On Quit", identity.DestoryOnQuit);
    }
}