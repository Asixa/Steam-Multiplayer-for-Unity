#if UNITY_5_3 || UNITY_5_4 || UNITY_5_5 || UNITY_5_6 || UNITY_5_7 || UNITY_5_8 || UNITY_5_9 || UNITY_6
using System;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Networking;
using UnityObject = UnityEngine.Object;

namespace NATTraversal
{
    [CustomEditor(typeof(NATTraversal.NetworkManager), true)]
    [CanEditMultipleObjects]
    public class NATNetworkManagerEditor : Editor
    {
        protected SerializedProperty m_DontDestroyOnLoadProperty;
        protected SerializedProperty m_RunInBackgroundProperty;
        protected SerializedProperty m_ScriptCRCCheckProperty;
        SerializedProperty m_NetworkAddressProperty;

        SerializedProperty m_NetworkPortProperty;
        SerializedProperty m_ServerBindToIPProperty;
        SerializedProperty m_ServerBindAddressProperty;
        SerializedProperty m_MaxDelayProperty;
        SerializedProperty m_MaxBufferedPacketsProperty;
        SerializedProperty m_AllowFragmentationProperty;

        protected SerializedProperty m_LogLevelProperty;
        SerializedProperty m_MatchHostProperty;
        SerializedProperty m_MatchPortProperty;
        SerializedProperty m_MatchNameProperty;
        SerializedProperty m_MatchSizeProperty;

        SerializedProperty m_PlayerPrefabProperty;
        SerializedProperty m_AutoCreatePlayerProperty;
        SerializedProperty m_PlayerSpawnMethodProperty;
        SerializedProperty m_SpawnListProperty;

        SerializedProperty m_CustomConfigProperty;

        SerializedProperty m_UseWebSocketsProperty;
        SerializedProperty m_UseSimulatorProperty;
        SerializedProperty m_SimulatedLatencyProperty;
        SerializedProperty m_PacketLossPercentageProperty;

        SerializedProperty m_ChannelListProperty;
        ReorderableList m_ChannelList;

        GUIContent m_ShowNetworkLabel;
        GUIContent m_ShowSpawnLabel;

        GUIContent m_OfflineSceneLabel;
        GUIContent m_OnlineSceneLabel;
        protected GUIContent m_DontDestroyOnLoadLabel;
        protected GUIContent m_RunInBackgroundLabel;
        protected GUIContent m_ScriptCRCCheckLabel;

        GUIContent m_MaxConnectionsLabel;
        GUIContent m_MinUpdateTimeoutLabel;
        GUIContent m_ConnectTimeoutLabel;
        GUIContent m_DisconnectTimeoutLabel;
        GUIContent m_PingTimeoutLabel;

        GUIContent m_ThreadAwakeTimeoutLabel;
        GUIContent m_ReactorModelLabel;
        GUIContent m_ReactorMaximumReceivedMessagesLabel;
        GUIContent m_ReactorMaximumSentMessagesLabel;

        GUIContent m_MaxBufferedPacketsLabel;
        GUIContent m_AllowFragmentationLabel;
        GUIContent m_UseWebSocketsLabel;
        GUIContent m_UseSimulatorLabel;
        GUIContent m_LatencyLabel;
        GUIContent m_PacketLossPercentageLabel;
        GUIContent m_MatchHostLabel;
        GUIContent m_MatchPortLabel;
        GUIContent m_MatchNameLabel;
        GUIContent m_MatchSizeLabel;

        ReorderableList m_SpawnList;

        protected bool m_Initialized;

        protected NATTraversal.NetworkManager m_NetworkManager;

        protected void Init()
        {
            if (m_Initialized)
            {
                return;
            }
            m_Initialized = true;
            m_NetworkManager = target as NATTraversal.NetworkManager;

            m_ShowNetworkLabel = new GUIContent("Network Info", "Network host names and ports");
            m_ShowSpawnLabel = new GUIContent("Spawn Info", "Registered spawnable objects");
            m_OfflineSceneLabel = new GUIContent("Offline Scene", "The scene loaded when the network goes offline (disconnected from server)");
            m_OnlineSceneLabel = new GUIContent("Online Scene", "The scene loaded when the network comes online (connected to server)");
            m_DontDestroyOnLoadLabel = new GUIContent("Dont Destroy On Load", "Persist the network manager across scene changes.");
            m_RunInBackgroundLabel = new GUIContent("Run in Background", "This ensures that the application runs when it does not have focus. This is required when testing multiple instances on a single machine, but not recommended for shipping on mobile platforms.");
            m_ScriptCRCCheckLabel = new GUIContent("Script CRC Check", "Enables a CRC check between server and client that ensures the NetworkBehaviour scripts match. This may not be appropriate in some cases, such a when the client and server are different Unity projects.");

            m_MaxConnectionsLabel = new GUIContent("Max Connections", "Maximum number of network connections");
            m_MinUpdateTimeoutLabel = new GUIContent("Min Update Timeout", "Minimum time network thread waits for events");
            m_ConnectTimeoutLabel = new GUIContent("Connect Timeout", "Time to wait for timeout on connecting");
            m_DisconnectTimeoutLabel = new GUIContent("Disconnect Timeout", "Time to wait for detecting disconnect");
            m_PingTimeoutLabel = new GUIContent("Ping Timeout", "Time to wait for ping messages");

            m_ThreadAwakeTimeoutLabel = new GUIContent("Thread Awake Timeout", "The minimum time period when system will check if there are any messages for send (or receive).");
            m_ReactorModelLabel = new GUIContent("Reactor Model", "Defines reactor model for the network library");
            m_ReactorMaximumReceivedMessagesLabel = new GUIContent("Reactor Max Recv Messages", "Defines maximum amount of messages in the receive queue");
            m_ReactorMaximumSentMessagesLabel = new GUIContent("Reactor Max Sent Messages", "Defines maximum message count in sent queue");

            m_MaxBufferedPacketsLabel = new GUIContent("Max Buffered Packets", "The maximum number of packets that can be buffered by a NetworkConnection for each channel. This corresponds to the 'ChannelOption.MaxPendingBuffers' channel option.");
            m_AllowFragmentationLabel = new GUIContent("Packet Fragmentation", "This allow NetworkConnection instances to fragment packets that are larger than the maxPacketSize, up to a maximum of 64K. This can cause delays in sending large packets, but is usually preferable to send failures.");
            m_UseWebSocketsLabel = new GUIContent("Use WebSockets", "This makes the server listen for connections using WebSockets. This allows WebGL clients to connect to the server.");
            m_UseSimulatorLabel = new GUIContent("Use Network Simulator", "This simulates network latency and packet loss on clients. Useful for testing under internet-like conditions");
            m_LatencyLabel = new GUIContent("Simulated Average Latency", "The amount of delay in milliseconds to add to network packets");
            m_PacketLossPercentageLabel = new GUIContent("Simulated Packet Loss", "The percentage of packets that should be dropped");
            m_MatchHostLabel = new GUIContent("MatchMaker Host URI", "The URI for the MatchMaker.");
            m_MatchPortLabel = new GUIContent("MatchMaker Port", "The port for the MatchMaker.");
            m_MatchNameLabel = new GUIContent("Match Name", "The name that will be used when creating a match in MatchMaker.");
            m_MatchSizeLabel = new GUIContent("Maximum Match Size", "The maximum size for the match. This value is compared to the maximum size specified in the service configuration at multiplayer.unity3d.com and the lower of the two is enforced. It must be greater than 1. This is typically used to override the match size for various game modes.");

            // top-level properties
            m_DontDestroyOnLoadProperty = serializedObject.FindProperty("m_DontDestroyOnLoad");
            m_RunInBackgroundProperty = serializedObject.FindProperty("m_RunInBackground");
            m_ScriptCRCCheckProperty = serializedObject.FindProperty("m_ScriptCRCCheck");
            m_LogLevelProperty = serializedObject.FindProperty("m_LogLevel");

            // network foldout properties
            m_NetworkAddressProperty = serializedObject.FindProperty("m_NetworkAddress");
            m_NetworkPortProperty = serializedObject.FindProperty("m_NetworkPort");
            m_ServerBindToIPProperty = serializedObject.FindProperty("m_ServerBindToIP");
            m_ServerBindAddressProperty = serializedObject.FindProperty("m_ServerBindAddress");
            m_MaxDelayProperty = serializedObject.FindProperty("m_MaxDelay");
            m_MaxBufferedPacketsProperty = serializedObject.FindProperty("m_MaxBufferedPackets");
            m_AllowFragmentationProperty = serializedObject.FindProperty("m_AllowFragmentation");
            m_MatchHostProperty = serializedObject.FindProperty("m_MatchHost");
            m_MatchPortProperty = serializedObject.FindProperty("m_MatchPort");
            m_MatchNameProperty = serializedObject.FindProperty("matchName");
            m_MatchSizeProperty = serializedObject.FindProperty("matchSize");

            // spawn foldout properties
            m_PlayerPrefabProperty = serializedObject.FindProperty("m_PlayerPrefab");
            m_AutoCreatePlayerProperty = serializedObject.FindProperty("m_AutoCreatePlayer");
            m_PlayerSpawnMethodProperty = serializedObject.FindProperty("m_PlayerSpawnMethod");
            m_SpawnListProperty = serializedObject.FindProperty("m_SpawnPrefabs");

            m_SpawnList = new ReorderableList(serializedObject, m_SpawnListProperty);
            m_SpawnList.drawHeaderCallback = DrawHeader;
            m_SpawnList.drawElementCallback = DrawChild;
            m_SpawnList.onReorderCallback = Changed;
            m_SpawnList.onAddDropdownCallback = AddButton;
            m_SpawnList.onRemoveCallback = RemoveButton;
            m_SpawnList.onChangedCallback = Changed;
            m_SpawnList.onReorderCallback = Changed;
            m_SpawnList.onAddCallback = Changed;
            m_SpawnList.elementHeight = 16; // this uses a 16x16 icon. other sizes make it stretch.

            // network configuration
            m_CustomConfigProperty = serializedObject.FindProperty("m_CustomConfig");
            m_ChannelListProperty = serializedObject.FindProperty("m_Channels");
            m_ChannelList = new ReorderableList(serializedObject, m_ChannelListProperty);
            m_ChannelList.drawHeaderCallback = ChannelDrawHeader;
            m_ChannelList.drawElementCallback = ChannelDrawChild;
            m_ChannelList.onReorderCallback = ChannelChanged;
            m_ChannelList.onAddDropdownCallback = ChannelAddButton;
            m_ChannelList.onRemoveCallback = ChannelRemoveButton;
            m_ChannelList.onChangedCallback = ChannelChanged;
            m_ChannelList.onReorderCallback = ChannelChanged;
            m_ChannelList.onAddCallback = ChannelChanged;

            // Network Simulator
            m_UseWebSocketsProperty = serializedObject.FindProperty("m_UseWebSockets");
            m_UseSimulatorProperty = serializedObject.FindProperty("m_UseSimulator");
            m_SimulatedLatencyProperty = serializedObject.FindProperty("m_SimulatedLatency");
            m_PacketLossPercentageProperty = serializedObject.FindProperty("m_PacketLossPercentage");
        }

        static void ShowPropertySuffix(GUIContent content, SerializedProperty prop, string suffix)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(prop, content);
            GUILayout.Label(suffix, EditorStyles.toolbarTextField, GUILayout.Width(64));
            EditorGUILayout.EndHorizontal();
        }

        protected void ShowSimulatorInfo()
        {
            EditorGUILayout.PropertyField(m_UseSimulatorProperty, m_UseSimulatorLabel);

            if (m_UseSimulatorProperty.boolValue)
            {
                EditorGUI.indentLevel += 1;

                if (Application.isPlaying && m_NetworkManager.client != null)
                {
                    // read only at runtime
                    EditorGUILayout.LabelField(m_LatencyLabel, new GUIContent(m_NetworkManager.simulatedLatency + " milliseconds"));
                    EditorGUILayout.LabelField(m_PacketLossPercentageLabel, new GUIContent(m_NetworkManager.packetLossPercentage + "%"));
                }
                else
                {
                    // Latency
                    int oldLatency = m_NetworkManager.simulatedLatency;
                    EditorGUILayout.BeginHorizontal();
                    int newLatency = EditorGUILayout.IntSlider(m_LatencyLabel, oldLatency, 1, 400);
                    GUILayout.Label("millsec", EditorStyles.toolbarTextField, GUILayout.Width(64));
                    EditorGUILayout.EndHorizontal();
                    if (newLatency != oldLatency)
                    {
                        m_SimulatedLatencyProperty.intValue = newLatency;
                    }

                    // Packet Loss
                    float oldPacketLoss = m_NetworkManager.packetLossPercentage;
                    EditorGUILayout.BeginHorizontal();
                    float newPacketLoss = EditorGUILayout.Slider(m_PacketLossPercentageLabel, oldPacketLoss, 0f, 20f);
                    GUILayout.Label("%", EditorStyles.toolbarTextField, GUILayout.Width(64));
                    EditorGUILayout.EndHorizontal();
                    if (newPacketLoss != oldPacketLoss)
                    {
                        m_PacketLossPercentageProperty.floatValue = newPacketLoss;
                    }
                }

                EditorGUI.indentLevel -= 1;
            }
        }

        protected void ShowConfigInfo()
        {
            bool oldCustomConfig = m_NetworkManager.customConfig;
            EditorGUILayout.PropertyField(m_CustomConfigProperty, new GUIContent("Advanced Configuration"));

            // Populate default channels first time a custom config is created.
            if (m_CustomConfigProperty.boolValue)
            {
                if (!oldCustomConfig)
                {
                    if (m_NetworkManager.channels.Count == 0)
                    {
                        m_NetworkManager.channels.Add(QosType.ReliableSequenced);
                        m_NetworkManager.channels.Add(QosType.Unreliable);
                        m_NetworkManager.customConfig = true;
                        m_CustomConfigProperty.serializedObject.Update();
                        m_ChannelList.serializedProperty.serializedObject.Update();
                    }
                }
            }

            if (m_NetworkManager.customConfig)
            {
                EditorGUI.indentLevel += 1;
                var maxConn = serializedObject.FindProperty("m_MaxConnections");
                ShowPropertySuffix(m_MaxConnectionsLabel, maxConn, "connections");

                m_ChannelList.DoLayoutList();

                maxConn.isExpanded = EditorGUILayout.Foldout(maxConn.isExpanded, "Timeouts");
                if (maxConn.isExpanded)
                {
                    EditorGUI.indentLevel += 1;
                    var minUpdateTimeout = serializedObject.FindProperty("m_ConnectionConfig.m_MinUpdateTimeout");
                    var connectTimeout = serializedObject.FindProperty("m_ConnectionConfig.m_ConnectTimeout");
                    var disconnectTimeout = serializedObject.FindProperty("m_ConnectionConfig.m_DisconnectTimeout");
                    var pingTimeout = serializedObject.FindProperty("m_ConnectionConfig.m_PingTimeout");

                    ShowPropertySuffix(m_MinUpdateTimeoutLabel, minUpdateTimeout, "millisec");
                    ShowPropertySuffix(m_ConnectTimeoutLabel, connectTimeout, "millisec");
                    ShowPropertySuffix(m_DisconnectTimeoutLabel, disconnectTimeout, "millisec");
                    ShowPropertySuffix(m_PingTimeoutLabel, pingTimeout, "millisec");
                    EditorGUI.indentLevel -= 1;
                }

                var threadAwakeTimeout = serializedObject.FindProperty("m_GlobalConfig.m_ThreadAwakeTimeout");
                threadAwakeTimeout.isExpanded = EditorGUILayout.Foldout(threadAwakeTimeout.isExpanded, "Global Config");
                if (threadAwakeTimeout.isExpanded)
                {
                    EditorGUI.indentLevel += 1;
                    var reactorModel = serializedObject.FindProperty("m_GlobalConfig.m_ReactorModel");
                    var reactorMaximumReceivedMessages = serializedObject.FindProperty("m_GlobalConfig.m_ReactorMaximumReceivedMessages");
                    var reactorMaximumSentMessages = serializedObject.FindProperty("m_GlobalConfig.m_ReactorMaximumSentMessages");

                    ShowPropertySuffix(m_ThreadAwakeTimeoutLabel, threadAwakeTimeout, "millisec");
                    EditorGUILayout.PropertyField(reactorModel, m_ReactorModelLabel);
                    ShowPropertySuffix(m_ReactorMaximumReceivedMessagesLabel, reactorMaximumReceivedMessages, "messages");
                    ShowPropertySuffix(m_ReactorMaximumSentMessagesLabel, reactorMaximumSentMessages, "messages");
                    EditorGUI.indentLevel -= 1;
                }

                EditorGUI.indentLevel -= 1;
            }
        }

        protected void ShowSpawnInfo()
        {
            m_PlayerPrefabProperty.isExpanded = EditorGUILayout.Foldout(m_PlayerPrefabProperty.isExpanded, m_ShowSpawnLabel);
            if (!m_PlayerPrefabProperty.isExpanded)
            {
                return;
            }

            EditorGUI.indentLevel += 1;

            //The NATLobbyManager doesnt use playerPrefab, it has its own player prefab slots, so dont show this
            if (m_NetworkManager.GetType() != typeof(NATLobbyManager))
            {
                EditorGUILayout.PropertyField(m_PlayerPrefabProperty);
            }

            EditorGUILayout.PropertyField(m_AutoCreatePlayerProperty);
            EditorGUILayout.PropertyField(m_PlayerSpawnMethodProperty);


            EditorGUI.BeginChangeCheck();
            m_SpawnList.DoLayoutList();
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }

            EditorGUI.indentLevel -= 1;
        }

        protected SceneAsset GetSceneObject(string sceneObjectName)
        {
            if (string.IsNullOrEmpty(sceneObjectName))
            {
                return null;
            }

            foreach (var editorScene in EditorBuildSettings.scenes)
            {
                if (editorScene.path.IndexOf(sceneObjectName) != -1)
                {
                    return AssetDatabase.LoadAssetAtPath(editorScene.path, typeof(SceneAsset)) as SceneAsset;
                }
            }
            if (LogFilter.logWarn) { Debug.LogWarning("Scene [" + sceneObjectName + "] cannot be used with networking. Add this scene to the 'Scenes in the Build' in build settings."); }
            return null;
        }

        protected void ShowNetworkInfo()
        {
            m_NetworkAddressProperty.isExpanded = EditorGUILayout.Foldout(m_NetworkAddressProperty.isExpanded, m_ShowNetworkLabel);
            if (!m_NetworkAddressProperty.isExpanded)
            {
                return;
            }
            EditorGUI.indentLevel += 1;

            if (EditorGUILayout.PropertyField(m_UseWebSocketsProperty, m_UseWebSocketsLabel))
            {
                NetworkServer.useWebSockets = m_NetworkManager.useWebSockets;
            }

            EditorGUILayout.PropertyField(m_NetworkAddressProperty);
            EditorGUILayout.PropertyField(m_NetworkPortProperty);
            EditorGUILayout.PropertyField(m_ServerBindToIPProperty);
            if (m_NetworkManager.serverBindToIP)
            {
                EditorGUI.indentLevel += 1;
                EditorGUILayout.PropertyField(m_ServerBindAddressProperty);
                EditorGUI.indentLevel -= 1;
            }
            EditorGUILayout.PropertyField(m_ScriptCRCCheckProperty, m_ScriptCRCCheckLabel);
            EditorGUILayout.PropertyField(m_MaxDelayProperty);
            if (m_MaxBufferedPacketsProperty != null)
            {
                EditorGUILayout.PropertyField(m_MaxBufferedPacketsProperty, m_MaxBufferedPacketsLabel);
            }
            if (m_AllowFragmentationProperty != null)
            {
                EditorGUILayout.PropertyField(m_AllowFragmentationProperty, m_AllowFragmentationLabel);
            }
            EditorGUILayout.PropertyField(m_MatchHostProperty, m_MatchHostLabel);
            EditorGUILayout.PropertyField(m_MatchPortProperty, m_MatchPortLabel);
            EditorGUILayout.PropertyField(m_MatchNameProperty, m_MatchNameLabel);
            EditorGUILayout.PropertyField(m_MatchSizeProperty, m_MatchSizeLabel);

            EditorGUI.indentLevel -= 1;
        }

        protected void ShowScenes()
        {
            var offlineObj = GetSceneObject(m_NetworkManager.offlineScene);
            var newOfflineScene = EditorGUILayout.ObjectField(m_OfflineSceneLabel, offlineObj, typeof(SceneAsset), false);
            if (newOfflineScene == null)
            {
                var prop = serializedObject.FindProperty("m_OfflineScene");
                prop.stringValue = "";
                EditorUtility.SetDirty(target);
            }
            else
            {
                if (newOfflineScene.name != m_NetworkManager.offlineScene)
                {
                    var sceneObj = GetSceneObject(newOfflineScene.name);
                    if (sceneObj == null)
                    {
                        Debug.LogWarning("The scene " + newOfflineScene.name + " cannot be used. To use this scene add it to the build settings for the project");
                    }
                    else
                    {
                        var prop = serializedObject.FindProperty("m_OfflineScene");
                        prop.stringValue = newOfflineScene.name;
                        EditorUtility.SetDirty(target);
                    }
                }
            }

            var onlineObj = GetSceneObject(m_NetworkManager.onlineScene);
            var newOnlineScene = EditorGUILayout.ObjectField(m_OnlineSceneLabel, onlineObj, typeof(SceneAsset), false);
            if (newOnlineScene == null)
            {
                var prop = serializedObject.FindProperty("m_OnlineScene");
                prop.stringValue = "";
                EditorUtility.SetDirty(target);
            }
            else
            {
                if (newOnlineScene.name != m_NetworkManager.onlineScene)
                {
                    var sceneObj = GetSceneObject(newOnlineScene.name);
                    if (sceneObj == null)
                    {
                        Debug.LogWarning("The scene " + newOnlineScene.name + " cannot be used. To use this scene add it to the build settings for the project");
                    }
                    else
                    {
                        var prop = serializedObject.FindProperty("m_OnlineScene");
                        prop.stringValue = newOnlineScene.name;
                        EditorUtility.SetDirty(target);
                    }
                }
            }
        }

        protected void ShowDerivedProperties(Type baseType, Type superType)
        {
            bool first = true;

            SerializedProperty property = serializedObject.GetIterator();
            bool expanded = true;
            while (property.NextVisible(expanded))
            {
                // ignore properties from base class.
                var f = baseType.GetField(property.name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                var p = baseType.GetProperty(property.name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (f == null && superType != null)
                {
                    f = superType.GetField(property.name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                }

                if (p == null && superType != null)
                {
                    p = superType.GetProperty(property.name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                }

                if (f == null && p == null)
                {
                    if (first)
                    {
                        first = false;
                        EditorGUI.BeginChangeCheck();
                        serializedObject.Update();

                        EditorGUILayout.Separator();
                    }
                    EditorGUILayout.PropertyField(property, true);
                    expanded = false;
                }
            }
            if (!first)
            {
                serializedObject.ApplyModifiedProperties();
                EditorGUI.EndChangeCheck();
            }
        }

        public override void OnInspectorGUI()
        {
            if (m_DontDestroyOnLoadProperty == null || m_DontDestroyOnLoadLabel == null)
                m_Initialized = false;

            Init();

            serializedObject.Update();
            EditorGUILayout.PropertyField(m_DontDestroyOnLoadProperty, m_DontDestroyOnLoadLabel);
            EditorGUILayout.PropertyField(m_RunInBackgroundProperty, m_RunInBackgroundLabel);

            if (EditorGUILayout.PropertyField(m_LogLevelProperty))
            {
                LogFilter.currentLogLevel = (int)m_NetworkManager.logLevel;
            }

            ShowScenes();
            ShowNetworkInfo();
            ShowSpawnInfo();
            ShowConfigInfo();
            ShowSimulatorInfo();
            serializedObject.ApplyModifiedProperties();

            ShowDerivedProperties(typeof(UnityEngine.Networking.NetworkManager), null);
        }

        static void DrawHeader(Rect headerRect)
        {
            GUI.Label(headerRect, "Registered Spawnable Prefabs:");
        }

        internal void DrawChild(Rect r, int index, bool isActive, bool isFocused)
        {
            SerializedProperty prefab = m_SpawnListProperty.GetArrayElementAtIndex(index);
            GameObject go = (GameObject)prefab.objectReferenceValue;

            GUIContent label;
            if (go == null)
            {
                label = new GUIContent("Empty", "Drag a prefab with a NetworkIdentity here");
            }
            else
            {
                var uv = go.GetComponent<NetworkIdentity>();
                if (uv != null)
                {
                    label = new GUIContent(go.name, "AssetId: [" + uv.assetId + "]");
                }
                else
                {
                    label = new GUIContent(go.name, "No Network Identity");
                }
            }

            var newGameObject = (GameObject)EditorGUI.ObjectField(r, label, go, typeof(GameObject), false);
            if (newGameObject == null)
            {
                m_NetworkManager.spawnPrefabs[index] = null;
                EditorUtility.SetDirty(target);
                return;
            }
            if (newGameObject.GetComponent<NetworkIdentity>())
            {
                if (m_NetworkManager.spawnPrefabs[index] != newGameObject)
                {
                    m_NetworkManager.spawnPrefabs[index] = newGameObject;
                    EditorUtility.SetDirty(target);
                }
            }
            else
            {
                if (LogFilter.logError) { Debug.LogError("Prefab " + newGameObject + " cannot be added as spawnable as it doesn't have a NetworkIdentity."); }
            }
        }

        internal void Changed(ReorderableList list)
        {
            EditorUtility.SetDirty(target);
        }

        internal void AddButton(Rect rect, ReorderableList list)
        {
            m_NetworkManager.spawnPrefabs.Add(null);
            m_SpawnList.index = m_SpawnList.count - 1;
            EditorUtility.SetDirty(target);
        }

        internal void RemoveButton(ReorderableList list)
        {
            m_NetworkManager.spawnPrefabs.RemoveAt(m_SpawnList.index);
            m_SpawnListProperty.DeleteArrayElementAtIndex(m_SpawnList.index);
            if (list.index >= m_SpawnListProperty.arraySize)
            {
                list.index = m_SpawnListProperty.arraySize - 1;
            }
            EditorUtility.SetDirty(target);
        }

        // List widget functions

        static void ChannelDrawHeader(Rect headerRect)
        {
            GUI.Label(headerRect, "Qos Channels:");
        }

        internal void ChannelDrawChild(Rect r, int index, bool isActive, bool isFocused)
        {
            QosType qos = m_NetworkManager.channels[index];
            QosType newValue = (QosType)EditorGUI.EnumPopup(r, "Channel #" + index, qos);
            if (newValue != qos)
            {
                m_NetworkManager.channels[index] = newValue;
                EditorUtility.SetDirty(target);
            }
        }

        internal void ChannelChanged(ReorderableList list)
        {
            EditorUtility.SetDirty(target);
        }

        internal void ChannelAddButton(Rect rect, ReorderableList list)
        {
            m_NetworkManager.channels.Add(QosType.ReliableSequenced);
            m_ChannelList.index = m_ChannelList.count - 1;
            EditorUtility.SetDirty(target);
        }

        internal void ChannelRemoveButton(ReorderableList list)
        {
            if (m_NetworkManager.channels.Count == 1)
            {
                if (LogFilter.logError) { Debug.LogError("Cannot remove channel. There must be at least one QoS channel."); }
                return;
            }
            m_NetworkManager.channels.RemoveAt(m_ChannelList.index);
            m_ChannelListProperty.DeleteArrayElementAtIndex(m_ChannelList.index);
            if (list.index >= m_ChannelListProperty.arraySize)
            {
                list.index = m_ChannelListProperty.arraySize - 1;
            }
            EditorUtility.SetDirty(target);
        }
    }
}
#elif UNITY_5_2
using System;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Networking;
using UnityObject = UnityEngine.Object;

namespace NATTraversal
{
    [CustomEditor(typeof(NATTraversal.NetworkManager), true)]
    [CanEditMultipleObjects]
    public class NATNetworkManagerEditor : Editor
    {
        protected SerializedProperty m_DontDestroyOnLoadProperty;
        protected SerializedProperty m_RunInBackgroundProperty;
        protected SerializedProperty m_ScriptCRCCheckProperty;
        SerializedProperty m_NetworkAddressProperty;

        SerializedProperty m_OnlineSceneProperty;
        SerializedProperty m_OfflineSceneProperty;

        SerializedProperty m_SendPeerInfoProperty;
        SerializedProperty m_UseWebSocketsProperty;
        SerializedProperty m_NetworkPortProperty;
        SerializedProperty m_ServerBindToIPProperty;
        SerializedProperty m_ServerBindAddressProperty;
        SerializedProperty m_MaxDelayProperty;
        protected SerializedProperty m_LogLevelProperty;
        SerializedProperty m_MatchHostProperty;
        SerializedProperty m_MatchPortProperty;

        SerializedProperty m_PlayerPrefabProperty;
        SerializedProperty m_AutoCreatePlayerProperty;
        SerializedProperty m_PlayerSpawnMethodProperty;
        SerializedProperty m_SpawnListProperty;

        SerializedProperty m_CustomConfigProperty;

        SerializedProperty m_UseSimulatorProperty;
        SerializedProperty m_SimulatedLatencyProperty;
        SerializedProperty m_PacketLossPercentageProperty;

        SerializedProperty m_ChannelListProperty;
        ReorderableList m_ChannelList;

        GUIContent m_ShowNetworkLabel;
        GUIContent m_ShowSpawnLabel;

        GUIContent m_OfflineSceneLabel;
        GUIContent m_OnlineSceneLabel;
        protected GUIContent m_DontDestroyOnLoadLabel;
        GUIContent m_UseWebSocketsLabel;
        GUIContent m_SendPeerInfoLabel;
        protected GUIContent m_RunInBackgroundLabel;
        protected GUIContent m_ScriptCRCCheckLabel;

        GUIContent m_MaxConnectionsLabel;
        GUIContent m_MinUpdateTimeoutLabel;
        GUIContent m_ConnectTimeoutLabel;
        GUIContent m_DisconnectTimeoutLabel;
        GUIContent m_PingTimeoutLabel;

        GUIContent m_UseSimulatorLabel;
        GUIContent m_LatencyLabel;
        GUIContent m_PacketLossPercentageLabel;

        ReorderableList m_SpawnList;

        protected bool m_Initialized;

        protected NATTraversal.NetworkManager m_NetworkManager;

        protected void Init()
        {
            if (m_Initialized)
            {
                return;
            }
            m_Initialized = true;
            m_NetworkManager = target as NATTraversal.NetworkManager;

            m_ShowNetworkLabel = new GUIContent("Network Info", "Network host names and ports");
            m_ShowSpawnLabel = new GUIContent("Spawn Info", "Registered spawnable objects");
            m_OfflineSceneLabel = new GUIContent("Offline Scene", "The scene loaded when the network goes offline (disconnected from server)");
            m_OnlineSceneLabel = new GUIContent("Online Scene", "The scene loaded when the network comes online (connected to server)");
            m_DontDestroyOnLoadLabel = new GUIContent("Dont Destroy On Load", "Persist the network manager across scene changes.");
            m_UseWebSocketsLabel = new GUIContent("Use WebSockets", "This makes the server listen for connections using WebSockets. This allows WebGL clients to connect to the server.");
            m_SendPeerInfoLabel = new GUIContent("Send Peer Info", "Send information about all peers to each peer. Used for \"Host Migration\".");
            m_RunInBackgroundLabel = new GUIContent("Run in Background", "This ensures that the application runs when it does not have focus. This is required when testing multiple instances on a single machine, but not recommended for shipping on mobile platforms.");
            m_ScriptCRCCheckLabel = new GUIContent("Script CRC Check", "Enables a CRC check between server and client that ensures the NetworkBehaviour scripts match. This may not be appropriate in some cases, such a when the client and server are different Unity projects.");

            m_MaxConnectionsLabel = new GUIContent("Max Connections", "Maximum number of network connections");
            m_MinUpdateTimeoutLabel = new GUIContent("Min Update Timeout", "Minimum time network thread waits for events");
            m_ConnectTimeoutLabel = new GUIContent("Connect Timeout", "Time to wait for timeout on connecting");
            m_DisconnectTimeoutLabel = new GUIContent("Disconnect Timeout", "Time to wait for detecting disconnect");
            m_PingTimeoutLabel = new GUIContent("Ping Timeout", "Time to wait for ping messages");

            m_UseSimulatorLabel = new GUIContent("Use Network Simulator", "This simulates network latency and packet loss on clients. Useful for testing under internet-like conditions");
            m_LatencyLabel = new GUIContent("Simulated Average Latency", "The amount of delay in milliseconds to add to network packets");
            m_PacketLossPercentageLabel = new GUIContent("Simulated Packet Loss", "The percentage of packets that should be dropped");

            // top-level properties
            m_DontDestroyOnLoadProperty = serializedObject.FindProperty("m_DontDestroyOnLoad");
            m_RunInBackgroundProperty = serializedObject.FindProperty("m_RunInBackground");
            m_ScriptCRCCheckProperty = serializedObject.FindProperty("m_ScriptCRCCheck");
            m_LogLevelProperty = serializedObject.FindProperty("m_LogLevel");

            // network foldout properties
            m_UseWebSocketsProperty = serializedObject.FindProperty("m_UseWebSockets");
            m_SendPeerInfoProperty = serializedObject.FindProperty("m_SendPeerInfo");
            m_NetworkAddressProperty = serializedObject.FindProperty("m_NetworkAddress");
            m_NetworkPortProperty = serializedObject.FindProperty("m_NetworkPort");
            m_ServerBindToIPProperty = serializedObject.FindProperty("m_ServerBindToIP");
            m_ServerBindAddressProperty = serializedObject.FindProperty("m_ServerBindAddress");
            m_MaxDelayProperty = serializedObject.FindProperty("m_MaxDelay");
            m_MatchHostProperty = serializedObject.FindProperty("m_MatchHost");
            m_MatchPortProperty = serializedObject.FindProperty("m_MatchPort");

            m_OnlineSceneProperty = serializedObject.FindProperty("m_OnlineScene");
            m_OfflineSceneProperty = serializedObject.FindProperty("m_OfflineScene");

            // spawn foldout properties
            m_PlayerPrefabProperty = serializedObject.FindProperty("m_PlayerPrefab");
            m_AutoCreatePlayerProperty = serializedObject.FindProperty("m_AutoCreatePlayer");
            m_PlayerSpawnMethodProperty = serializedObject.FindProperty("m_PlayerSpawnMethod");
            m_SpawnListProperty = serializedObject.FindProperty("m_SpawnPrefabs");

            m_SpawnList = new ReorderableList(serializedObject, m_SpawnListProperty);
            m_SpawnList.drawHeaderCallback = DrawHeader;
            m_SpawnList.drawElementCallback = DrawChild;
            m_SpawnList.onReorderCallback = Changed;
            m_SpawnList.onAddDropdownCallback = AddButton;
            m_SpawnList.onRemoveCallback = RemoveButton;
            m_SpawnList.onChangedCallback = Changed;
            m_SpawnList.onReorderCallback = Changed;
            m_SpawnList.onAddCallback = Changed;
            m_SpawnList.elementHeight = 16; // this uses a 16x16 icon. other sizes make it stretch.

            // network configuration
            m_CustomConfigProperty = serializedObject.FindProperty("m_CustomConfig");
            m_ChannelListProperty = serializedObject.FindProperty("m_Channels");
            m_ChannelList = new ReorderableList(serializedObject, m_ChannelListProperty);
            m_ChannelList.drawHeaderCallback = ChannelDrawHeader;
            m_ChannelList.drawElementCallback = ChannelDrawChild;
            m_ChannelList.onReorderCallback = ChannelChanged;
            m_ChannelList.onAddDropdownCallback = ChannelAddButton;
            m_ChannelList.onRemoveCallback = ChannelRemoveButton;
            m_ChannelList.onChangedCallback = ChannelChanged;
            m_ChannelList.onReorderCallback = ChannelChanged;
            m_ChannelList.onAddCallback = ChannelChanged;

            // Network Simulator
            m_UseSimulatorProperty = serializedObject.FindProperty("m_UseSimulator");
            m_SimulatedLatencyProperty = serializedObject.FindProperty("m_SimulatedLatency");
            m_PacketLossPercentageProperty = serializedObject.FindProperty("m_PacketLossPercentage");
        }

        static void ShowPropertySuffix(GUIContent content, SerializedProperty prop, string suffix)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(prop, content);
            GUILayout.Label(suffix, EditorStyles.toolbarTextField, GUILayout.Width(64));
            EditorGUILayout.EndHorizontal();
        }

        protected void ShowSimulatorInfo()
        {
            EditorGUILayout.PropertyField(m_UseSimulatorProperty, m_UseSimulatorLabel);

            if (m_UseSimulatorProperty.boolValue)
            {
                EditorGUI.indentLevel += 1;

                if (Application.isPlaying && m_NetworkManager.client != null)
                {
                    // read only at runtime
                    EditorGUILayout.LabelField(m_LatencyLabel, new GUIContent(m_NetworkManager.simulatedLatency + " milliseconds"));
                    EditorGUILayout.LabelField(m_PacketLossPercentageLabel, new GUIContent(m_NetworkManager.packetLossPercentage + "%"));
                }
                else
                {
                    // Latency
                    int oldLatency = m_NetworkManager.simulatedLatency;
                    EditorGUILayout.BeginHorizontal();
                    int newLatency = EditorGUILayout.IntSlider(m_LatencyLabel, oldLatency, 1, 400);
                    GUILayout.Label("millsec", EditorStyles.toolbarTextField, GUILayout.Width(64));
                    EditorGUILayout.EndHorizontal();
                    if (newLatency != oldLatency)
                    {
                        m_SimulatedLatencyProperty.intValue = newLatency;
                    }

                    // Packet Loss
                    float oldPacketLoss = m_NetworkManager.packetLossPercentage;
                    EditorGUILayout.BeginHorizontal();
                    float newPacketLoss = EditorGUILayout.Slider(m_PacketLossPercentageLabel, oldPacketLoss, 0f, 20f);
                    GUILayout.Label("%", EditorStyles.toolbarTextField, GUILayout.Width(64));
                    EditorGUILayout.EndHorizontal();
                    if (newPacketLoss != oldPacketLoss)
                    {
                        m_PacketLossPercentageProperty.floatValue = newPacketLoss;
                    }
                }

                EditorGUI.indentLevel -= 1;
            }
        }

        protected void ShowConfigInfo()
        {
            bool oldCustomConfig = m_NetworkManager.customConfig;
            EditorGUILayout.PropertyField(m_CustomConfigProperty, new GUIContent("Advanced Configuration"));

            // Populate default channels first time a custom config is created.
            if (m_CustomConfigProperty.boolValue)
            {
                if (!oldCustomConfig)
                {
                    if (m_NetworkManager.channels.Count == 0)
                    {
                        m_NetworkManager.channels.Add(QosType.ReliableSequenced);
                        m_NetworkManager.channels.Add(QosType.Unreliable);
                        m_NetworkManager.customConfig = true;
                        m_CustomConfigProperty.serializedObject.Update();
                        m_ChannelList.serializedProperty.serializedObject.Update();
                    }
                }
            }

            if (m_NetworkManager.customConfig)
            {
                EditorGUI.indentLevel += 1;
                var maxConn = serializedObject.FindProperty("m_MaxConnections");
                ShowPropertySuffix(m_MaxConnectionsLabel, maxConn, "connections");

                m_ChannelList.DoLayoutList();

                maxConn.isExpanded = EditorGUILayout.Foldout(maxConn.isExpanded, "Timeouts");
                if (maxConn.isExpanded)
                {
                    var minUpdateTimeout = serializedObject.FindProperty("m_ConnectionConfig.m_MinUpdateTimeout");
                    var connectTimeout = serializedObject.FindProperty("m_ConnectionConfig.m_ConnectTimeout");
                    var disconnectTimeout = serializedObject.FindProperty("m_ConnectionConfig.m_DisconnectTimeout");
                    var pingTimeout = serializedObject.FindProperty("m_ConnectionConfig.m_PingTimeout");

                    ShowPropertySuffix(m_MinUpdateTimeoutLabel, minUpdateTimeout, "millisec");
                    ShowPropertySuffix(m_ConnectTimeoutLabel, connectTimeout, "millisec");
                    ShowPropertySuffix(m_DisconnectTimeoutLabel, disconnectTimeout, "millisec");
                    ShowPropertySuffix(m_PingTimeoutLabel, pingTimeout, "millisec");
                }
                EditorGUI.indentLevel -= 1;
            }
        }

        protected void ShowSpawnInfo()
        {
            m_PlayerPrefabProperty.isExpanded = EditorGUILayout.Foldout(m_PlayerPrefabProperty.isExpanded, m_ShowSpawnLabel);
            if (!m_PlayerPrefabProperty.isExpanded)
            {
                return;
            }

            EditorGUI.indentLevel += 1;

            EditorGUILayout.PropertyField(m_PlayerPrefabProperty);
            EditorGUILayout.PropertyField(m_AutoCreatePlayerProperty);
            EditorGUILayout.PropertyField(m_PlayerSpawnMethodProperty);


            EditorGUI.BeginChangeCheck();
            m_SpawnList.DoLayoutList();
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }

            EditorGUI.indentLevel -= 1;
        }

        protected UnityObject GetSceneObject(string sceneObjectName)
        {
            if (string.IsNullOrEmpty(sceneObjectName))
            {
                return null;
            }

            foreach (var editorScene in EditorBuildSettings.scenes)
            {
                if (editorScene.path.IndexOf(sceneObjectName) != -1)
                {
                    return AssetDatabase.LoadAssetAtPath(editorScene.path, typeof(UnityObject));
                }
            }
            if (LogFilter.logWarn) { Debug.LogWarning("Scene [" + sceneObjectName + "] cannot be used with networking. Add this scene to the 'Scenes in the Build' in build settings."); }
            return null;
        }

        protected void ShowNetworkInfo()
        {
            m_NetworkAddressProperty.isExpanded = EditorGUILayout.Foldout(m_NetworkAddressProperty.isExpanded, m_ShowNetworkLabel);
            if (!m_NetworkAddressProperty.isExpanded)
            {
                return;
            }
            EditorGUI.indentLevel += 1;

            // Web sockets were introduced part way through 5.2 so we need to check to see if it's actually available.
            // Once again reflection saves the day
            FieldInfo useWebSocketsFieldNetworkManager = typeof(NATTraversal.NetworkManager).GetField("useWebsSockets", BindingFlags.Public | BindingFlags.Instance);
            FieldInfo useWebSocketsFieldServer = typeof(NetworkServer).GetField("useWebsSockets", BindingFlags.Public | BindingFlags.Static);

            if (useWebSocketsFieldServer != null)
            {
                if (EditorGUILayout.PropertyField(m_UseWebSocketsProperty, m_UseWebSocketsLabel))
                {
                    useWebSocketsFieldServer.SetValue(null, useWebSocketsFieldNetworkManager.GetValue(m_NetworkManager));
                }

            }
            if (EditorGUILayout.PropertyField(m_SendPeerInfoProperty, m_SendPeerInfoLabel))
            {
                NetworkServer.sendPeerInfo = m_NetworkManager.sendPeerInfo;
            }
            EditorGUILayout.PropertyField(m_NetworkAddressProperty);
            EditorGUILayout.PropertyField(m_NetworkPortProperty);
            EditorGUILayout.PropertyField(m_ServerBindToIPProperty);
            if (m_NetworkManager.serverBindToIP)
            {
                EditorGUI.indentLevel += 1;
                EditorGUILayout.PropertyField(m_ServerBindAddressProperty);
                EditorGUI.indentLevel -= 1;
            }
            EditorGUILayout.PropertyField(m_ScriptCRCCheckProperty, m_ScriptCRCCheckLabel);
            EditorGUILayout.PropertyField(m_MaxDelayProperty);
            EditorGUILayout.PropertyField(m_MatchHostProperty);
            EditorGUILayout.PropertyField(m_MatchPortProperty);

            EditorGUI.indentLevel -= 1;
            Utility.useRandomSourceID = EditorGUILayout.Toggle("Multiple Client Mode:", Utility.useRandomSourceID);
        }

        protected void ShowScenes()
        {
            var offlineObj = GetSceneObject(m_NetworkManager.offlineScene);
            var newOfflineScene = EditorGUILayout.ObjectField(m_OfflineSceneLabel, offlineObj, typeof(UnityObject), true);
            if (newOfflineScene == null)
            {
                m_OfflineSceneProperty.stringValue = "";
                EditorUtility.SetDirty(target);
            }
            else
            {
                if (newOfflineScene.name != m_NetworkManager.offlineScene)
                {
                    m_OfflineSceneProperty.stringValue = newOfflineScene.name;
                    EditorUtility.SetDirty(target);
                }
            }

            var onlineObj = GetSceneObject(m_NetworkManager.onlineScene);
            var newOnlineScene = EditorGUILayout.ObjectField(m_OnlineSceneLabel, onlineObj, typeof(UnityObject), true);
            if (newOnlineScene == null)
            {
                m_OnlineSceneProperty.stringValue = "";
                EditorUtility.SetDirty(target);
            }
            else
            {
                if (newOnlineScene.name != m_NetworkManager.onlineScene)
                {
                    m_OnlineSceneProperty.stringValue = newOnlineScene.name;
                    EditorUtility.SetDirty(target);
                }
            }
        }

        protected void ShowDerivedProperties(Type baseType, Type superType)
        {
            bool first = true;

            SerializedProperty property = serializedObject.GetIterator();
            bool expanded = true;
            while (property.NextVisible(expanded))
            {
                // ignore properties from base class.
                var f = baseType.GetField(property.name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                var p = baseType.GetProperty(property.name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (f == null && superType != null)
                {
                    f = superType.GetField(property.name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                }

                if (p == null && superType != null)
                {
                    p = superType.GetProperty(property.name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                }

                if (f == null && p == null)
                {
                    if (first)
                    {
                        first = false;
                        EditorGUI.BeginChangeCheck();
                        serializedObject.Update();

                        EditorGUILayout.Separator();
                    }
                    EditorGUILayout.PropertyField(property, true);
                    expanded = false;
                }
            }
            if (!first)
            {
                serializedObject.ApplyModifiedProperties();
                EditorGUI.EndChangeCheck();
            }
        }

        public override void OnInspectorGUI()
        {
            if (m_DontDestroyOnLoadProperty == null || m_DontDestroyOnLoadLabel == null)
                m_Initialized = false;

            Init();

            serializedObject.Update();
            EditorGUILayout.PropertyField(m_DontDestroyOnLoadProperty, m_DontDestroyOnLoadLabel);
            EditorGUILayout.PropertyField(m_RunInBackgroundProperty, m_RunInBackgroundLabel);

            if (EditorGUILayout.PropertyField(m_LogLevelProperty))
            {
                LogFilter.currentLogLevel = (int)m_NetworkManager.logLevel;
            }

            ShowScenes();
            ShowNetworkInfo();
            ShowSpawnInfo();
            ShowConfigInfo();
            ShowSimulatorInfo();
            serializedObject.ApplyModifiedProperties();

            ShowDerivedProperties(typeof(UnityEngine.Networking.NetworkManager), null);
        }

        static void DrawHeader(Rect headerRect)
        {
            GUI.Label(headerRect, "Registered Spawnable Prefabs:");
        }

        internal void DrawChild(Rect r, int index, bool isActive, bool isFocused)
        {
            SerializedProperty prefab = m_SpawnListProperty.GetArrayElementAtIndex(index);
            GameObject go = (GameObject)prefab.objectReferenceValue;

            GUIContent label;
            if (go == null)
            {
                label = new GUIContent("Empty", "Drag a prefab with a NetworkIdentity here");
            }
            else
            {
                var uv = go.GetComponent<NetworkIdentity>();
                if (uv != null)
                {
                    label = new GUIContent(go.name, "AssetId: [" + uv.assetId + "]");
                }
                else
                {
                    label = new GUIContent(go.name, "No Network Identity");
                }
            }

            var newGameObject = (GameObject)EditorGUI.ObjectField(r, label, go, typeof(GameObject), false);
            if (newGameObject == null)
            {
                return;
            }
            if (newGameObject.GetComponent<NetworkIdentity>())
            {
                if (m_NetworkManager.spawnPrefabs[index] != newGameObject)
                {
                    m_NetworkManager.spawnPrefabs[index] = newGameObject;
                    EditorUtility.SetDirty(target);
                }
            }
            else
            {
                if (LogFilter.logError) { Debug.LogError("Prefab " + newGameObject + " cannot be added as spawnable as it doesn't have a NetworkIdentity."); }
            }
        }

        internal void Changed(ReorderableList list)
        {
            EditorUtility.SetDirty(target);
        }

        internal void AddButton(Rect rect, ReorderableList list)
        {
            m_NetworkManager.spawnPrefabs.Add(null);
            m_SpawnList.index = m_SpawnList.count - 1;
            EditorUtility.SetDirty(target);
        }

        internal void RemoveButton(ReorderableList list)
        {
            m_NetworkManager.spawnPrefabs.RemoveAt(m_SpawnList.index);
            m_SpawnListProperty.DeleteArrayElementAtIndex(m_SpawnList.index);
            if (list.index >= m_SpawnListProperty.arraySize)
            {
                list.index = m_SpawnListProperty.arraySize - 1;
            }
            EditorUtility.SetDirty(target);
        }

        // List widget functions

        static void ChannelDrawHeader(Rect headerRect)
        {
            GUI.Label(headerRect, "Qos Channels:");
        }

        internal void ChannelDrawChild(Rect r, int index, bool isActive, bool isFocused)
        {
            QosType qos = m_NetworkManager.channels[index];
            QosType newValue = (QosType)EditorGUI.EnumPopup(r, "Channel #" + index, qos);
            if (newValue != qos)
            {
                m_NetworkManager.channels[index] = newValue;
                EditorUtility.SetDirty(target);
            }
        }

        internal void ChannelChanged(ReorderableList list)
        {
            EditorUtility.SetDirty(target);
        }

        internal void ChannelAddButton(Rect rect, ReorderableList list)
        {
            m_NetworkManager.channels.Add(QosType.Reliable);
            m_ChannelList.index = m_ChannelList.count - 1;
            EditorUtility.SetDirty(target);
        }

        internal void ChannelRemoveButton(ReorderableList list)
        {
            if (m_NetworkManager.channels.Count == 1)
            {
                if (LogFilter.logError) { Debug.LogError("Cannot remove channel. There must be at least one QoS channel."); }
                return;
            }
            m_NetworkManager.channels.RemoveAt(m_ChannelList.index);
            m_ChannelListProperty.DeleteArrayElementAtIndex(m_ChannelList.index);
            if (list.index >= m_ChannelListProperty.arraySize)
            {
                list.index = m_ChannelListProperty.arraySize - 1;
            }
            EditorUtility.SetDirty(target);
        }
    }
}
#endif