using UnityEditor;
using SteamMultiplayer;
using UnityEngine;

[CustomEditor(typeof(Identity))]
public class IdentityInspector : Editor
{
    public override void OnInspectorGUI()
    {
        Identity identity = target as Identity;

        string content;
        content = "ID:" + ((identity.ID==-1)? "Unassigned" : identity.ID.ToString());
        content += "   ";
        content +=identity.IsLocalSpawned ? "Local " : "Remote";
        content += "   ";
        content += "SpawnID:" +identity.SpawnID;
        EditorGUILayout.HelpBox(content,MessageType.None);

    }
}