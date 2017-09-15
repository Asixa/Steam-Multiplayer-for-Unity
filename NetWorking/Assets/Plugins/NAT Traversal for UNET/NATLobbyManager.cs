#if UNITY_5_4 || UNITY_5_5 || UNITY_5_6 || UNITY_5_7 || UNITY_5_8 || UNITY_5_9 || UNITY_6 || UNITY_2017 || UNITY_2018 || UNITY_2019 || UNITY_2020
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.SceneManagement;

namespace NATTraversal
{
    public class NATLobbyManager : NATTraversal.NetworkManager
    {
        struct PendingPlayer
        {
            public NetworkConnection conn;
            public GameObject lobbyPlayer;
        }

        // configuration
        [SerializeField]
        bool m_ShowLobbyGUI = true;
        [SerializeField]
        int m_MaxPlayers = 4;
        [SerializeField]
        int m_MaxPlayersPerConnection = 1;
        [SerializeField]
        int m_MinPlayers;
        [SerializeField]
        NATLobbyPlayer m_LobbyPlayerPrefab;
        [SerializeField]
        GameObject m_GamePlayerPrefab;
        [SerializeField]
        string m_LobbyScene = "";
        [SerializeField]
        string m_PlayScene = "";

        // runtime data
        List<PendingPlayer> m_PendingPlayers = new List<PendingPlayer>();
        public NATLobbyPlayer[] lobbySlots;

        // static message objects to avoid runtime-allocations
        static LobbyReadyToBeginMessage s_ReadyToBeginMessage = new LobbyReadyToBeginMessage();
        static IntegerMessage s_SceneLoadedMessage = new IntegerMessage();
        static LobbyReadyToBeginMessage s_LobbyReadyToBeginMessage = new LobbyReadyToBeginMessage();

        // properties
        public bool showLobbyGUI { get { return m_ShowLobbyGUI; } set { m_ShowLobbyGUI = value; } }
        public int maxPlayers { get { return m_MaxPlayers; } set { m_MaxPlayers = value; } }
        public int maxPlayersPerConnection { get { return m_MaxPlayersPerConnection; } set { m_MaxPlayersPerConnection = value; } }
        public int minPlayers { get { return m_MinPlayers; } set { m_MinPlayers = value; } }
        public NATLobbyPlayer lobbyPlayerPrefab { get { return m_LobbyPlayerPrefab; } set { m_LobbyPlayerPrefab = value; } }
        public GameObject gamePlayerPrefab { get { return m_GamePlayerPrefab; } set { m_GamePlayerPrefab = value; } }
        public string lobbyScene { get { return m_LobbyScene; } set { m_LobbyScene = value; offlineScene = value; } }
        public string playScene { get { return m_PlayScene; } set { m_PlayScene = value; } }

        public class LobbyReadyToBeginMessage : MessageBase
        {
            public byte slotId;
            public bool readyState;

            public override void Deserialize(NetworkReader reader)
            {
                slotId = reader.ReadByte();
                readyState = reader.ReadBoolean();
            }

            public override void Serialize(NetworkWriter writer)
            {
                writer.Write(slotId);
                writer.Write(readyState);
            }
        }

        void OnValidate()
        {
            if (m_MaxPlayers <= 0)
            {
                m_MaxPlayers = 1;
            }

            if (m_MaxPlayersPerConnection <= 0)
            {
                m_MaxPlayersPerConnection = 1;
            }

            if (m_MaxPlayersPerConnection > maxPlayers)
            {
                m_MaxPlayersPerConnection = maxPlayers;
            }

            if (m_MinPlayers < 0)
            {
                m_MinPlayers = 0;
            }

            if (m_MinPlayers > m_MaxPlayers)
            {
                m_MinPlayers = m_MaxPlayers;
            }

            if (m_LobbyPlayerPrefab != null)
            {
                var uv = m_LobbyPlayerPrefab.GetComponent<NetworkIdentity>();
                if (uv == null)
                {
                    m_LobbyPlayerPrefab = null;
                    Debug.LogWarning("LobbyPlayer prefab must have a NetworkIdentity component.");
                }
            }

            if (m_GamePlayerPrefab != null)
            {
                var uv = m_GamePlayerPrefab.GetComponent<NetworkIdentity>();
                if (uv == null)
                {
                    m_GamePlayerPrefab = null;
                    Debug.LogWarning("GamePlayer prefab must have a NetworkIdentity component.");
                }
            }
        }

        Byte FindSlot()
        {
            for (byte i = 0; i < maxPlayers; i++)
            {
                if (lobbySlots[i] == null)
                {
                    return i;
                }
            }
            return Byte.MaxValue;
        }

        void SceneLoadedForPlayer(NetworkConnection conn, GameObject lobbyPlayerGameObject)
        {
            var lobbyPlayer = lobbyPlayerGameObject.GetComponent<NATLobbyPlayer>();
            if (lobbyPlayer == null)
            {
                // not a lobby player.. dont replace it
                return;
            }

            string loadedSceneName = SceneManager.GetSceneAt(0).name;
            if (LogFilter.logDebug) { Debug.Log("NATLobby SceneLoadedForPlayer scene:" + loadedSceneName + " " + conn); }

            if (loadedSceneName == m_LobbyScene)
            {
                // cant be ready in lobby, add to ready list
                PendingPlayer pending;
                pending.conn = conn;
                pending.lobbyPlayer = lobbyPlayerGameObject;
                m_PendingPlayers.Add(pending);
                return;
            }

            var controllerId = lobbyPlayerGameObject.GetComponent<NetworkIdentity>().playerControllerId;
            var gamePlayer = OnLobbyServerCreateGamePlayer(conn, controllerId);
            if (gamePlayer == null)
            {
                // get start position from base class
                Transform startPos = GetStartPosition();
                if (startPos != null)
                {
                    gamePlayer = (GameObject)Instantiate(gamePlayerPrefab, startPos.position, startPos.rotation);
                }
                else
                {
                    gamePlayer = (GameObject)Instantiate(gamePlayerPrefab, Vector3.zero, Quaternion.identity);
                }
            }

            if (!OnLobbyServerSceneLoadedForPlayer(lobbyPlayerGameObject, gamePlayer))
            {
                return;
            }

            // replace lobby player with game player
            NetworkServer.ReplacePlayerForConnection(conn, gamePlayer, controllerId);
        }

        static int CheckConnectionIsReadyToBegin(NetworkConnection conn)
        {
            int countPlayers = 0;
            for (int i = 0; i < conn.playerControllers.Count; i++)
            {
                var player = conn.playerControllers[i];
                if (player.IsValid)
                {
                    var lobbyPlayer = player.gameObject.GetComponent<NATLobbyPlayer>();
                    if (lobbyPlayer.readyToBegin)
                    {
                        countPlayers += 1;
                    }
                }
            }
            return countPlayers;
        }

        public void CheckReadyToBegin()
        {
            string loadedSceneName = SceneManager.GetSceneAt(0).name;
            if (loadedSceneName != m_LobbyScene)
            {
                return;
            }

            int readyCount = 0;
            int playerCount = 0;

            for (int i = 0; i < NetworkServer.connections.Count; i++)
            {
                var conn = NetworkServer.connections[i];

                if (conn == null)
                    continue;

                playerCount += 1;
                readyCount += CheckConnectionIsReadyToBegin(conn);
            }
            if (m_MinPlayers > 0 && readyCount < m_MinPlayers)
            {
                // not enough players ready yet.
                return;
            }

            if (readyCount < playerCount)
            {
                // not all players are ready yet
                return;
            }

            m_PendingPlayers.Clear();
            OnLobbyServerPlayersReady();
        }

        public void ServerReturnToLobby()
        {
            if (!NetworkServer.active)
            {
                Debug.Log("ServerReturnToLobby called on client");
                return;
            }
            ServerChangeScene(m_LobbyScene);
        }

        void CallOnClientEnterLobby()
        {
            OnLobbyClientEnter();
            for (int i = 0; i < lobbySlots.Length; i++)
            {
                var player = lobbySlots[i];
                if (player == null)
                    continue;

                player.readyToBegin = false;
                player.OnClientEnterLobby();
            }
        }

        void CallOnClientExitLobby()
        {
            OnLobbyClientExit();
            for (int i = 0; i < lobbySlots.Length; i++)
            {
                var player = lobbySlots[i];
                if (player == null)
                    continue;

                player.OnClientExitLobby();
            }
        }

        public bool SendReturnToLobby()
        {
            if (client == null || !client.isConnected)
            {
                return false;
            }

            var msg = new EmptyMessage();
            client.Send(MsgType.LobbyReturnToLobby, msg);
            return true;
        }

        // ------------------------ server handlers ------------------------

        public override void OnServerConnect(NetworkConnection conn)
        {
            // numPlayers returns the player count including this one, so ok to be equal
            if (numPlayers > maxPlayers)
            {
                if (LogFilter.logWarn) { Debug.LogWarning("NATLobbyManager can't accept new connection [" + conn + "], too many players connected."); }
                conn.Disconnect();
                return;
            }

            // cannot join game in progress
            string loadedSceneName = SceneManager.GetSceneAt(0).name;
            if (loadedSceneName != m_LobbyScene)
            {
                if (LogFilter.logWarn) { Debug.LogWarning("NATLobbyManager can't accept new connection [" + conn + "], not in lobby and game already in progress."); }
                conn.Disconnect();
                return;
            }

            base.OnServerConnect(conn);
            OnLobbyServerConnect(conn);
        }

        public override void OnServerDisconnect(NetworkConnection conn)
        {
            base.OnServerDisconnect(conn);

            // if lobbyplayer for this connection has not been destroyed by now, then destroy it here
            for (int i = 0; i < lobbySlots.Length; i++)
            {
                var player = lobbySlots[i];
                if (player == null)
                    continue;

                if (player.connectionToClient == conn)
                {
                    lobbySlots[i] = null;
                    NetworkServer.Destroy(player.gameObject);
                }
            }

            OnLobbyServerDisconnect(conn);
        }

        public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
        {
            string loadedSceneName = SceneManager.GetSceneAt(0).name;
            if (loadedSceneName != m_LobbyScene)
            {
                return;
            }

            // check MaxPlayersPerConnection
            int numPlayersForConnection = 0;
            for (int i = 0; i < conn.playerControllers.Count; i++)
            {
                if (conn.playerControllers[i].IsValid)
                    numPlayersForConnection += 1;
            }

            if (numPlayersForConnection >= maxPlayersPerConnection)
            {
                if (LogFilter.logWarn) { Debug.LogWarning("NATLobbyManager no more players for this connection."); }

                var errorMsg = new EmptyMessage();
                conn.Send(MsgType.LobbyAddPlayerFailed, errorMsg);
                return;
            }

            byte slot = FindSlot();
            if (slot == Byte.MaxValue)
            {
                if (LogFilter.logWarn) { Debug.LogWarning("NATLobbyManager no space for more players"); }

                var errorMsg = new EmptyMessage();
                conn.Send(MsgType.LobbyAddPlayerFailed, errorMsg);
                return;
            }

            var newLobbyGameObject = OnLobbyServerCreateLobbyPlayer(conn, playerControllerId);
            if (newLobbyGameObject == null)
            {
                newLobbyGameObject = (GameObject)Instantiate(lobbyPlayerPrefab.gameObject, Vector3.zero, Quaternion.identity);
            }

            var newLobbyPlayer = newLobbyGameObject.GetComponent<NATLobbyPlayer>();
            newLobbyPlayer.slot = slot;
            lobbySlots[slot] = newLobbyPlayer;

            NetworkServer.AddPlayerForConnection(conn, newLobbyGameObject, playerControllerId);
        }

        public override void OnServerRemovePlayer(NetworkConnection conn, PlayerController player)
        {
            var playerControllerId = player.playerControllerId;
            byte slot = player.gameObject.GetComponent<NATLobbyPlayer>().slot;
            lobbySlots[slot] = null;
            base.OnServerRemovePlayer(conn, player);

            for (int i = 0; i < lobbySlots.Length; i++)
            {
                var lobbyPlayer = lobbySlots[i];
                if (lobbyPlayer != null)
                {
                    lobbyPlayer.GetComponent<NATLobbyPlayer>().readyToBegin = false;

                    s_LobbyReadyToBeginMessage.slotId = lobbyPlayer.slot;
                    s_LobbyReadyToBeginMessage.readyState = false;
                    NetworkServer.SendToReady(null, MsgType.LobbyReadyToBegin, s_LobbyReadyToBeginMessage);
                }
            }

            OnLobbyServerPlayerRemoved(conn, playerControllerId);
        }

        public override void ServerChangeScene(string sceneName)
        {
            if (sceneName == m_LobbyScene)
            {
                for (int i = 0; i < lobbySlots.Length; i++)
                {
                    var lobbyPlayer = lobbySlots[i];
                    if (lobbyPlayer == null)
                        continue;

                    // find the game-player object for this connection, and destroy it
                    var uv = lobbyPlayer.GetComponent<NetworkIdentity>();

                    PlayerController playerController;

                    if (GetPlayerController(uv.connectionToClient, uv.playerControllerId, out playerController))
                    {
                        NetworkServer.Destroy(playerController.gameObject);
                    }

                    if (NetworkServer.active)
                    {
                        // re-add the lobby object
                        lobbyPlayer.GetComponent<NATLobbyPlayer>().readyToBegin = false;
                        NetworkServer.ReplacePlayerForConnection(uv.connectionToClient, lobbyPlayer.gameObject, uv.playerControllerId);
                    }
                }
            }
            base.ServerChangeScene(sceneName);
        }

        bool GetPlayerController(NetworkConnection client, short playerControllerId, out PlayerController playerController)
        {
            playerController = null;
            if (client.playerControllers.Count > 0)
            {
                for (int i = 0; i < client.playerControllers.Count; i++)
                {
                    if (client.playerControllers[i].IsValid && client.playerControllers[i].playerControllerId == playerControllerId)
                    {
                        playerController = client.playerControllers[i];
                        return true;
                    }
                }
                return false;
            }
            return false;
        }

        public override void OnServerSceneChanged(string sceneName)
        {
            if (sceneName != m_LobbyScene)
            {
                // call SceneLoadedForPlayer on any players that become ready while we were loading the scene.
                for (int i = 0; i < m_PendingPlayers.Count; i++)
                {
                    var pending = m_PendingPlayers[i];
                    SceneLoadedForPlayer(pending.conn, pending.lobbyPlayer);
                }
                m_PendingPlayers.Clear();
            }

            OnLobbyServerSceneChanged(sceneName);
        }

        void OnServerReadyToBeginMessage(NetworkMessage netMsg)
        {
            if (LogFilter.logDebug) { Debug.Log("NATLobbyManager OnServerReadyToBeginMessage"); }
            netMsg.ReadMessage(s_ReadyToBeginMessage);

            PlayerController lobbyController;
            if (!GetPlayerController(netMsg.conn, (short)s_ReadyToBeginMessage.slotId, out lobbyController))
            {
                if (LogFilter.logError) { Debug.LogError("NATLobbyManager OnServerReadyToBeginMessage invalid playerControllerId " + s_ReadyToBeginMessage.slotId); }
                return;
            }

            // set this player ready
            var lobbyPlayer = lobbyController.gameObject.GetComponent<NATLobbyPlayer>();
            lobbyPlayer.readyToBegin = s_ReadyToBeginMessage.readyState;

            // tell every player that this player is ready
            var outMsg = new LobbyReadyToBeginMessage();
            outMsg.slotId = lobbyPlayer.slot;
            outMsg.readyState = s_ReadyToBeginMessage.readyState;
            NetworkServer.SendToReady(null, MsgType.LobbyReadyToBegin, outMsg);

            // maybe start the game
            CheckReadyToBegin();
        }

        void OnServerSceneLoadedMessage(NetworkMessage netMsg)
        {
            if (LogFilter.logDebug) { Debug.Log("NATLobbyManager OnSceneLoadedMessage"); }

            netMsg.ReadMessage(s_SceneLoadedMessage);

            PlayerController lobbyController;
            if (!GetPlayerController(netMsg.conn, (short)s_SceneLoadedMessage.value, out lobbyController))
            {
                if (LogFilter.logError) { Debug.LogError("NATLobbyManager OnServerSceneLoadedMessage invalid playerControllerId " + s_SceneLoadedMessage.value); }
                return;
            }

            SceneLoadedForPlayer(netMsg.conn, lobbyController.gameObject);
        }

        void OnServerReturnToLobbyMessage(NetworkMessage netMsg)
        {
            if (LogFilter.logDebug) { Debug.Log("NATLobbyManager OnServerReturnToLobbyMessage"); }

            ServerReturnToLobby();
        }

        public override void OnStartServer()
        {
            if (string.IsNullOrEmpty(m_LobbyScene))
            {
                if (LogFilter.logError) { Debug.LogError("NATLobbyManager LobbyScene is empty. Set the LobbyScene in the inspector for the NATLobbyMangaer"); }
                return;
            }

            if (string.IsNullOrEmpty(m_PlayScene))
            {
                if (LogFilter.logError) { Debug.LogError("NATLobbyManager PlayScene is empty. Set the PlayScene in the inspector for the NATLobbyMangaer"); }
                return;
            }

            if (lobbySlots.Length == 0)
            {
                lobbySlots = new NATLobbyPlayer[maxPlayers];
            }

            NetworkServer.RegisterHandler(MsgType.LobbyReadyToBegin, OnServerReadyToBeginMessage);
            NetworkServer.RegisterHandler(MsgType.LobbySceneLoaded, OnServerSceneLoadedMessage);
            NetworkServer.RegisterHandler(MsgType.LobbyReturnToLobby, OnServerReturnToLobbyMessage);

            OnLobbyStartServer();
        }

        public override void OnStartHost()
        {
            OnLobbyStartHost();
        }

        public override void OnStopHost()
        {
            OnLobbyStopHost();
        }

        // ------------------------ client handlers ------------------------

        public override void OnStartClient(NetworkClient lobbyClient)
        {
            if (lobbySlots.Length == 0)
            {
                lobbySlots = new NATLobbyPlayer[maxPlayers];
            }

            if (m_LobbyPlayerPrefab == null || m_LobbyPlayerPrefab.gameObject == null)
            {
                if (LogFilter.logError) { Debug.LogError("NATLobbyManager no LobbyPlayer prefab is registered. Please add a LobbyPlayer prefab."); }
            }
            else
            {
                ClientScene.RegisterPrefab(m_LobbyPlayerPrefab.gameObject);
            }

            if (m_GamePlayerPrefab == null)
            {
                if (LogFilter.logError) { Debug.LogError("NATLobbyManager no GamePlayer prefab is registered. Please add a GamePlayer prefab."); }
            }
            else
            {
                ClientScene.RegisterPrefab(m_GamePlayerPrefab);
            }

            lobbyClient.RegisterHandler(MsgType.LobbyReadyToBegin, OnClientReadyToBegin);
            lobbyClient.RegisterHandler(MsgType.LobbyAddPlayerFailed, OnClientAddPlayerFailedMessage);

            OnLobbyStartClient(lobbyClient);
        }

        public override void OnClientConnect(NetworkConnection conn)
        {
            OnLobbyClientConnect(conn);
            CallOnClientEnterLobby();
            base.OnClientConnect(conn);
        }

        public override void OnClientDisconnect(NetworkConnection conn)
        {
            OnLobbyClientDisconnect(conn);
            base.OnClientDisconnect(conn);
        }

        public override void OnStopClient()
        {
            OnLobbyStopClient();
            CallOnClientExitLobby();
        }

        public override void OnClientSceneChanged(NetworkConnection conn)
        {
            string loadedSceneName = SceneManager.GetSceneAt(0).name;
            if (loadedSceneName == m_LobbyScene)
            {
                if (client.isConnected)
                {
                    CallOnClientEnterLobby();
                }
            }
            else
            {
                CallOnClientExitLobby();
            }

            base.OnClientSceneChanged(conn);
            OnLobbyClientSceneChanged(conn);
        }

        void OnClientReadyToBegin(NetworkMessage netMsg)
        {
            netMsg.ReadMessage(s_LobbyReadyToBeginMessage);

            if (s_LobbyReadyToBeginMessage.slotId >= lobbySlots.Count())
            {
                if (LogFilter.logError) { Debug.LogError("NATLobbyManager OnClientReadyToBegin invalid lobby slot " + s_LobbyReadyToBeginMessage.slotId); }
                return;
            }

            var lobbyPlayer = lobbySlots[s_LobbyReadyToBeginMessage.slotId];
            if (lobbyPlayer == null || lobbyPlayer.gameObject == null)
            {
                if (LogFilter.logError) { Debug.LogError("NATLobbyManager OnClientReadyToBegin no player at lobby slot " + s_LobbyReadyToBeginMessage.slotId); }
                return;
            }

            lobbyPlayer.readyToBegin = s_LobbyReadyToBeginMessage.readyState;
            lobbyPlayer.OnClientReady(s_LobbyReadyToBeginMessage.readyState);
        }

        void OnClientAddPlayerFailedMessage(NetworkMessage netMsg)
        {
            if (LogFilter.logDebug) { Debug.Log("NATLobbyManager Add Player failed."); }
            OnLobbyClientAddPlayerFailed();
        }

        // ------------------------ lobby server virtuals ------------------------

        public virtual void OnLobbyStartHost()
        {
        }

        public virtual void OnLobbyStopHost()
        {
        }

        public virtual void OnLobbyStartServer()
        {
        }

        public virtual void OnLobbyServerConnect(NetworkConnection conn)
        {
        }

        public virtual void OnLobbyServerDisconnect(NetworkConnection conn)
        {
        }

        public virtual void OnLobbyServerSceneChanged(string sceneName)
        {
        }

        public virtual GameObject OnLobbyServerCreateLobbyPlayer(NetworkConnection conn, short playerControllerId)
        {
            return null;
        }

        public virtual GameObject OnLobbyServerCreateGamePlayer(NetworkConnection conn, short playerControllerId)
        {
            return null;
        }

        public virtual void OnLobbyServerPlayerRemoved(NetworkConnection conn, short playerControllerId)
        {
        }

        // for users to apply settings from their lobby player object to their in-game player object
        public virtual bool OnLobbyServerSceneLoadedForPlayer(GameObject lobbyPlayer, GameObject gamePlayer)
        {
            return true;
        }

        public virtual void OnLobbyServerPlayersReady()
        {
            // all players are readyToBegin, start the game
            ServerChangeScene(m_PlayScene);
        }

        // ------------------------ lobby client virtuals ------------------------

        public virtual void OnLobbyClientEnter()
        {
        }

        public virtual void OnLobbyClientExit()
        {
        }

        public virtual void OnLobbyClientConnect(NetworkConnection conn)
        {
        }

        public virtual void OnLobbyClientDisconnect(NetworkConnection conn)
        {
        }

        public virtual void OnLobbyStartClient(NetworkClient lobbyClient)
        {
        }

        public virtual void OnLobbyStopClient()
        {
        }

        public virtual void OnLobbyClientSceneChanged(NetworkConnection conn)
        {
        }

        // for users to handle adding a player failed on the server
        public virtual void OnLobbyClientAddPlayerFailed()
        {
        }

        // ------------------------ optional UI ------------------------

        void OnGUI()
        {
            if (!showLobbyGUI)
                return;

            string loadedSceneName = SceneManager.GetSceneAt(0).name;
            if (loadedSceneName != m_LobbyScene)
                return;

            Rect backgroundRec = new Rect(90, 180, 500, 150);
            GUI.Box(backgroundRec, "Players:");

            if (NetworkClient.active)
            {
                Rect addRec = new Rect(100, 300, 120, 20);
                if (GUI.Button(addRec, "Add Player"))
                {
                    TryToAddPlayer();
                }
            }
        }

        public void TryToAddPlayer()
        {
            if (NetworkClient.active)
            {
                short controllerId = -1;
                var controllers = client.connection.playerControllers;
                //var controllers = NetworkClient.allClients[0].connection.playerControllers;

                if (controllers.Count < maxPlayers)
                {
                    controllerId = (short)controllers.Count;
                }
                else
                {
                    for (short i = 0; i < maxPlayers; i++)
                    {
                        if (!controllers[i].IsValid)
                        {
                            controllerId = i;
                            break;
                        }
                    }
                }
                if (LogFilter.logDebug) { Debug.Log("NATLobbyManager TryToAddPlayer controllerId " + controllerId + " ready:" + ClientScene.ready); }

                if (controllerId == -1)
                {
                    if (LogFilter.logDebug) { Debug.Log("NATLobbyManager No Space!"); }
                    return;
                }

                if (ClientScene.ready)
                {
                    ClientScene.AddPlayer(controllerId);
                }
                else
                {
                    ClientScene.AddPlayer(NetworkClient.allClients[0].connection, controllerId);
                }
            }
            else
            {
                if (LogFilter.logDebug) { Debug.Log("NATLobbyManager NetworkClient not active!"); }
            }
        }
    }
}
#elif UNITY_5_3
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.SceneManagement;

namespace NATTraversal
{
    public class NATLobbyManager : NATTraversal.NetworkManager
    {
        struct PendingPlayer
        {
            public NetworkConnection conn;
            public GameObject lobbyPlayer;
        }

        // configuration
        [SerializeField]
        bool m_ShowLobbyGUI = true;
        [SerializeField]
        int m_MaxPlayers = 4;
        [SerializeField]
        int m_MaxPlayersPerConnection = 1;
        [SerializeField]
        int m_MinPlayers;
        [SerializeField]
        NATLobbyPlayer m_LobbyPlayerPrefab;
        [SerializeField]
        GameObject m_GamePlayerPrefab;
        [SerializeField]
        string m_LobbyScene = "";
        [SerializeField]
        string m_PlayScene = "";

        // runtime data
        List<PendingPlayer> m_PendingPlayers = new List<PendingPlayer>();
        public NATLobbyPlayer[] lobbySlots;

        public class LobbyReadyToBeginMessage : MessageBase
        {
            public byte slotId;
            public bool readyState;

            public override void Deserialize(NetworkReader reader)
            {
                slotId = reader.ReadByte();
                readyState = reader.ReadBoolean();
            }

            public override void Serialize(NetworkWriter writer)
            {
                writer.Write(slotId);
                writer.Write(readyState);
            }
        }

        // static message objects to avoid runtime-allocations
        static LobbyReadyToBeginMessage s_ReadyToBeginMessage = new LobbyReadyToBeginMessage();
        static IntegerMessage s_SceneLoadedMessage = new IntegerMessage();
        static LobbyReadyToBeginMessage s_LobbyReadyToBeginMessage = new LobbyReadyToBeginMessage();

        // properties
        public bool showLobbyGUI { get { return m_ShowLobbyGUI; } set { m_ShowLobbyGUI = value; } }
        public int maxPlayers { get { return m_MaxPlayers; } set { m_MaxPlayers = value; } }
        public int maxPlayersPerConnection { get { return m_MaxPlayersPerConnection; } set { m_MaxPlayersPerConnection = value; } }
        public int minPlayers { get { return m_MinPlayers; } set { m_MinPlayers = value; } }
        public NATLobbyPlayer lobbyPlayerPrefab { get { return m_LobbyPlayerPrefab; } set { m_LobbyPlayerPrefab = value; } }
        public GameObject gamePlayerPrefab { get { return m_GamePlayerPrefab; } set { m_GamePlayerPrefab = value; } }
        public string lobbyScene { get { return m_LobbyScene; } set { m_LobbyScene = value; offlineScene = value; } }
        public string playScene { get { return m_PlayScene; } set { m_PlayScene = value; } }

        void OnValidate()
        {
            if (m_MaxPlayers <= 0)
            {
                m_MaxPlayers = 1;
            }

            if (m_MaxPlayersPerConnection <= 0)
            {
                m_MaxPlayersPerConnection = 1;
            }

            if (m_MaxPlayersPerConnection > maxPlayers)
            {
                m_MaxPlayersPerConnection = maxPlayers;
            }

            if (m_MinPlayers < 0)
            {
                m_MinPlayers = 0;
            }

            if (m_MinPlayers > m_MaxPlayers)
            {
                m_MinPlayers = m_MaxPlayers;
            }

            if (m_LobbyPlayerPrefab != null)
            {
                var uv = m_LobbyPlayerPrefab.GetComponent<NetworkIdentity>();
                if (uv == null)
                {
                    m_LobbyPlayerPrefab = null;
                    Debug.LogWarning("LobbyPlayer prefab must have a NetworkIdentity component.");
                }
            }

            if (m_GamePlayerPrefab != null)
            {
                var uv = m_GamePlayerPrefab.GetComponent<NetworkIdentity>();
                if (uv == null)
                {
                    m_GamePlayerPrefab = null;
                    Debug.LogWarning("GamePlayer prefab must have a NetworkIdentity component.");
                }
            }
        }

        Byte FindSlot()
        {
            for (byte i = 0; i < maxPlayers; i++)
            {
                if (lobbySlots[i] == null)
                {
                    return i;
                }
            }
            return Byte.MaxValue;
        }

        void SceneLoadedForPlayer(NetworkConnection conn, GameObject lobbyPlayerGameObject)
        {
            var lobbyPlayer = lobbyPlayerGameObject.GetComponent<NATLobbyPlayer>();
            if (lobbyPlayer == null)
            {
                // not a lobby player.. dont replace it
                return;
            }

            string loadedSceneName = SceneManager.GetSceneAt(0).name;
            if (LogFilter.logDebug) { Debug.Log("NATLobby SceneLoadedForPlayer scene:" + loadedSceneName + " " + conn); }

            if (loadedSceneName == m_LobbyScene)
            {
                // cant be ready in lobby, add to ready list
                PendingPlayer pending;
                pending.conn = conn;
                pending.lobbyPlayer = lobbyPlayerGameObject;
                m_PendingPlayers.Add(pending);
                return;
            }

            var controllerId = lobbyPlayerGameObject.GetComponent<NetworkIdentity>().playerControllerId;
            var gamePlayer = OnLobbyServerCreateGamePlayer(conn, controllerId);
            if (gamePlayer == null)
            {
                // get start position from base class
                Transform startPos = GetStartPosition();
                if (startPos != null)
                {
                    gamePlayer = (GameObject)Instantiate(gamePlayerPrefab, startPos.position, startPos.rotation);
                }
                else
                {
                    gamePlayer = (GameObject)Instantiate(gamePlayerPrefab, Vector3.zero, Quaternion.identity);
                }
            }

            if (!OnLobbyServerSceneLoadedForPlayer(lobbyPlayerGameObject, gamePlayer))
            {
                return;
            }

            // replace lobby player with game player
            NetworkServer.ReplacePlayerForConnection(conn, gamePlayer, controllerId);
        }

        static int CheckConnectionIsReadyToBegin(NetworkConnection conn)
        {
            int countPlayers = 0;
            foreach (var player in conn.playerControllers)
            {
                if (player.IsValid)
                {
                    var lobbyPlayer = player.gameObject.GetComponent<NATLobbyPlayer>();
                    if (lobbyPlayer.readyToBegin)
                    {
                        countPlayers += 1;
                    }
                }
            }
            return countPlayers;
        }

        public void CheckReadyToBegin()
        {
            string loadedSceneName = SceneManager.GetSceneAt(0).name;
            if (loadedSceneName != m_LobbyScene)
            {
                return;
            }

            int readyCount = 0;

            foreach (var conn in NetworkServer.connections)
            {
                if (conn == null)
                    continue;

                readyCount += CheckConnectionIsReadyToBegin(conn);
            }
            if (m_MinPlayers > 0 && readyCount < m_MinPlayers)
            {
                // not enough players ready yet.
                return;
            }

            m_PendingPlayers.Clear();
            OnLobbyServerPlayersReady();
        }

        public void ServerReturnToLobby()
        {
            if (!NetworkServer.active)
            {
                Debug.Log("ServerReturnToLobby called on client");
                return;
            }
            ServerChangeScene(m_LobbyScene);
        }

        void CallOnClientEnterLobby()
        {
            OnLobbyClientEnter();
            foreach (var player in lobbySlots)
            {
                if (player == null)
                    continue;

                player.readyToBegin = false;
                player.OnClientEnterLobby();
            }
        }

        void CallOnClientExitLobby()
        {
            OnLobbyClientExit();
            foreach (var player in lobbySlots)
            {
                if (player == null)
                    continue;

                player.OnClientExitLobby();
            }
        }

        public bool SendReturnToLobby()
        {
            if (client == null || !client.isConnected)
            {
                return false;
            }

            var msg = new EmptyMessage();
            client.Send(MsgType.LobbyReturnToLobby, msg);
            return true;
        }

        // ------------------------ server handlers ------------------------

        public override void OnServerConnect(NetworkConnection conn)
        {
            if (numPlayers >= maxPlayers)
            {
                conn.Disconnect();
                return;
            }

            // cannot join game in progress
            string loadedSceneName = SceneManager.GetSceneAt(0).name;
            if (loadedSceneName != m_LobbyScene)
            {
                conn.Disconnect();
                return;
            }

            base.OnServerConnect(conn);
            OnLobbyServerConnect(conn);
        }

        public override void OnServerDisconnect(NetworkConnection conn)
        {
            base.OnServerDisconnect(conn);

            // if lobbyplayer for this connection has not been destroyed by now, then destroy it here
            for (int i = 0; i < lobbySlots.Length; i++)
            {
                var player = lobbySlots[i];
                if (player == null)
                    continue;

                if (player.connectionToClient == conn)
                {
                    lobbySlots[i] = null;
                    NetworkServer.Destroy(player.gameObject);
                }
            }

            OnLobbyServerDisconnect(conn);
        }

        public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
        {
            string loadedSceneName = SceneManager.GetSceneAt(0).name;
            if (loadedSceneName != m_LobbyScene)
            {
                return;
            }

            // check MaxPlayersPerConnection
            int numPlayersForConnection = 0;
            foreach (var player in conn.playerControllers)
            {
                if (player.IsValid)
                    numPlayersForConnection += 1;
            }

            if (numPlayersForConnection >= maxPlayersPerConnection)
            {
                if (LogFilter.logWarn) { Debug.LogWarning("NATLobbyManager no more players for this connection."); }

                var errorMsg = new EmptyMessage();
                conn.Send(MsgType.LobbyAddPlayerFailed, errorMsg);
                return;
            }

            byte slot = FindSlot();
            if (slot == Byte.MaxValue)
            {
                if (LogFilter.logWarn) { Debug.LogWarning("NATLobbyManager no space for more players"); }

                var errorMsg = new EmptyMessage();
                conn.Send(MsgType.LobbyAddPlayerFailed, errorMsg);
                return;
            }

            var newLobbyGameObject = OnLobbyServerCreateLobbyPlayer(conn, playerControllerId);
            if (newLobbyGameObject == null)
            {
                newLobbyGameObject = (GameObject)Instantiate(lobbyPlayerPrefab.gameObject, Vector3.zero, Quaternion.identity);
            }

            var newLobbyPlayer = newLobbyGameObject.GetComponent<NATLobbyPlayer>();
            newLobbyPlayer.slot = slot;
            lobbySlots[slot] = newLobbyPlayer;

            NetworkServer.AddPlayerForConnection(conn, newLobbyGameObject, playerControllerId);
        }

        public override void OnServerRemovePlayer(NetworkConnection conn, PlayerController player)
        {
            var playerControllerId = player.playerControllerId;
            byte slot = player.gameObject.GetComponent<NATLobbyPlayer>().slot;
            lobbySlots[slot] = null;
            base.OnServerRemovePlayer(conn, player);

            foreach (var p in lobbySlots)
            {
                if (p != null)
                {
                    p.GetComponent<NATLobbyPlayer>().readyToBegin = false;

                    s_LobbyReadyToBeginMessage.slotId = p.slot;
                    s_LobbyReadyToBeginMessage.readyState = false;
                    NetworkServer.SendToReady(null, MsgType.LobbyReadyToBegin, s_LobbyReadyToBeginMessage);
                }
            }

            OnLobbyServerPlayerRemoved(conn, playerControllerId);
        }

        public override void ServerChangeScene(string sceneName)
        {
            if (sceneName == m_LobbyScene)
            {
                foreach (var lobbyPlayer in lobbySlots)
                {
                    if (lobbyPlayer == null)
                        continue;

                    // find the game-player object for this connection, and destroy it
                    var uv = lobbyPlayer.GetComponent<NetworkIdentity>();

                    PlayerController playerController;
                    if (GetPlayerController(uv.connectionToClient, uv.playerControllerId, out playerController))
                    {
                        NetworkServer.Destroy(playerController.gameObject);
                    }

                    if (NetworkServer.active)
                    {
                        // re-add the lobby object
                        lobbyPlayer.GetComponent<NATLobbyPlayer>().readyToBegin = false;
                        NetworkServer.ReplacePlayerForConnection(uv.connectionToClient, lobbyPlayer.gameObject, uv.playerControllerId);
                    }
                }
            }
            base.ServerChangeScene(sceneName);
        }

        public override void OnServerSceneChanged(string sceneName)
        {
            if (sceneName != m_LobbyScene)
            {
                // call SceneLoadedForPlayer on any players that become ready while we were loading the scene.
                foreach (var pending in m_PendingPlayers)
                {
                    SceneLoadedForPlayer(pending.conn, pending.lobbyPlayer);
                }
                m_PendingPlayers.Clear();
            }

            OnLobbyServerSceneChanged(sceneName);
        }

        void OnServerReadyToBeginMessage(NetworkMessage netMsg)
        {
            if (LogFilter.logDebug) { Debug.Log("NATLobbyManager OnServerReadyToBeginMessage"); }
            netMsg.ReadMessage(s_ReadyToBeginMessage);

            PlayerController lobbyController;
            if (!GetPlayerController(netMsg.conn, (short)s_ReadyToBeginMessage.slotId, out lobbyController))
            {
                if (LogFilter.logError) { Debug.LogError("NATLobbyManager OnServerReadyToBeginMessage invalid playerControllerId " + s_ReadyToBeginMessage.slotId); }
                return;
            }

            // set this player ready
            var lobbyPlayer = lobbyController.gameObject.GetComponent<NATLobbyPlayer>();
            lobbyPlayer.readyToBegin = s_ReadyToBeginMessage.readyState;

            // tell every player that this player is ready
            var outMsg = new LobbyReadyToBeginMessage();
            outMsg.slotId = lobbyPlayer.slot;
            outMsg.readyState = s_ReadyToBeginMessage.readyState;
            NetworkServer.SendToReady(null, MsgType.LobbyReadyToBegin, outMsg);

            // maybe start the game
            CheckReadyToBegin();
        }

        void OnServerSceneLoadedMessage(NetworkMessage netMsg)
        {
            if (LogFilter.logDebug) { Debug.Log("NATLobbyManager OnSceneLoadedMessage"); }

            netMsg.ReadMessage(s_SceneLoadedMessage);

            PlayerController lobbyController;
            if (!GetPlayerController(netMsg.conn, (short)s_SceneLoadedMessage.value, out lobbyController))
            {
                if (LogFilter.logError) { Debug.LogError("NATLobbyManager OnServerSceneLoadedMessage invalid playerControllerId " + s_SceneLoadedMessage.value); }
                return;
            }

            SceneLoadedForPlayer(netMsg.conn, lobbyController.gameObject);
        }

        bool GetPlayerController(NetworkConnection client, short playerControllerId, out PlayerController playerController)
        {
            playerController = null;
            if (client.playerControllers.Count > 0)
            {
                for (int i = 0; i < client.playerControllers.Count; i++)
                {
                    if (client.playerControllers[i].IsValid && client.playerControllers[i].playerControllerId == playerControllerId)
                    {
                        playerController = client.playerControllers[i];
                        return true;
                    }
                }
                return false;
            }
            return false;
        }

        void OnServerReturnToLobbyMessage(NetworkMessage netMsg)
        {
            if (LogFilter.logDebug) { Debug.Log("NATLobbyManager OnServerReturnToLobbyMessage"); }

            ServerReturnToLobby();
        }

        public override void OnStartServer()
        {
            if (string.IsNullOrEmpty(m_LobbyScene))
            {
                if (LogFilter.logError) { Debug.LogError("NATLobbyManager LobbyScene is empty. Set the LobbyScene in the inspector for the NATLobbyMangaer"); }
                return;
            }

            if (string.IsNullOrEmpty(m_PlayScene))
            {
                if (LogFilter.logError) { Debug.LogError("NATLobbyManager PlayScene is empty. Set the PlayScene in the inspector for the NATLobbyMangaer"); }
                return;
            }

            if (lobbySlots.Length == 0)
            {
                lobbySlots = new NATLobbyPlayer[maxPlayers];
            }

            NetworkServer.RegisterHandler(MsgType.LobbyReadyToBegin, OnServerReadyToBeginMessage);
            NetworkServer.RegisterHandler(MsgType.LobbySceneLoaded, OnServerSceneLoadedMessage);
            NetworkServer.RegisterHandler(MsgType.LobbyReturnToLobby, OnServerReturnToLobbyMessage);

            OnLobbyStartServer();
        }

        public override void OnStartHost()
        {
            OnLobbyStartHost();
        }

        public override void OnStopHost()
        {
            OnLobbyStopHost();
        }

        // ------------------------ client handlers ------------------------

        public override void OnStartClient(NetworkClient lobbyClient)
        {
            if (lobbySlots.Length == 0)
            {
                lobbySlots = new NATLobbyPlayer[maxPlayers];
            }

            if (m_LobbyPlayerPrefab == null || m_LobbyPlayerPrefab.gameObject == null)
            {
                if (LogFilter.logError) { Debug.LogError("NATLobbyManager no LobbyPlayer prefab is registered. Please add a LobbyPlayer prefab."); }
            }
            else
            {
                ClientScene.RegisterPrefab(m_LobbyPlayerPrefab.gameObject);
            }

            if (m_GamePlayerPrefab == null)
            {
                if (LogFilter.logError) { Debug.LogError("NATLobbyManager no GamePlayer prefab is registered. Please add a GamePlayer prefab."); }
            }
            else
            {
                ClientScene.RegisterPrefab(m_GamePlayerPrefab);
            }

            lobbyClient.RegisterHandler(MsgType.LobbyReadyToBegin, OnClientReadyToBegin);
            lobbyClient.RegisterHandler(MsgType.LobbyAddPlayerFailed, OnClientAddPlayerFailedMessage);

            OnLobbyStartClient(lobbyClient);
        }

        public override void OnClientConnect(NetworkConnection conn)
        {
            OnLobbyClientConnect(conn);
            CallOnClientEnterLobby();
            base.OnClientConnect(conn);
        }

        public override void OnClientDisconnect(NetworkConnection conn)
        {
            OnLobbyClientDisconnect(conn);
            base.OnClientDisconnect(conn);
        }

        public override void OnStopClient()
        {
            OnLobbyStopClient();
            CallOnClientExitLobby();
        }

        public override void OnClientSceneChanged(NetworkConnection conn)
        {
            string loadedSceneName = SceneManager.GetSceneAt(0).name;
            if (loadedSceneName == m_LobbyScene)
            {
                if (client.isConnected)
                {
                    CallOnClientEnterLobby();
                }
            }
            else
            {
                CallOnClientExitLobby();
            }

            base.OnClientSceneChanged(conn);
            OnLobbyClientSceneChanged(conn);
        }

        void OnClientReadyToBegin(NetworkMessage netMsg)
        {
            netMsg.ReadMessage(s_LobbyReadyToBeginMessage);

            if (s_LobbyReadyToBeginMessage.slotId >= lobbySlots.Count())
            {
                if (LogFilter.logError) { Debug.LogError("NATLobbyManager OnClientReadyToBegin invalid lobby slot " + s_LobbyReadyToBeginMessage.slotId); }
                return;
            }

            var lobbyPlayer = lobbySlots[s_LobbyReadyToBeginMessage.slotId];
            if (lobbyPlayer == null || lobbyPlayer.gameObject == null)
            {
                if (LogFilter.logError) { Debug.LogError("NATLobbyManager OnClientReadyToBegin no player at lobby slot " + s_LobbyReadyToBeginMessage.slotId); }
                return;
            }

            lobbyPlayer.readyToBegin = s_LobbyReadyToBeginMessage.readyState;
            lobbyPlayer.OnClientReady(s_LobbyReadyToBeginMessage.readyState);
        }

        void OnClientAddPlayerFailedMessage(NetworkMessage netMsg)
        {
            if (LogFilter.logDebug) { Debug.Log("NATLobbyManager Add Player failed."); }
            OnLobbyClientAddPlayerFailed();
        }

        // ------------------------ lobby server virtuals ------------------------

        public virtual void OnLobbyStartHost()
        {
        }

        public virtual void OnLobbyStopHost()
        {
        }

        public virtual void OnLobbyStartServer()
        {
        }

        public virtual void OnLobbyServerConnect(NetworkConnection conn)
        {
        }

        public virtual void OnLobbyServerDisconnect(NetworkConnection conn)
        {
        }

        public virtual void OnLobbyServerSceneChanged(string sceneName)
        {
        }

        public virtual GameObject OnLobbyServerCreateLobbyPlayer(NetworkConnection conn, short playerControllerId)
        {
            return null;
        }

        public virtual GameObject OnLobbyServerCreateGamePlayer(NetworkConnection conn, short playerControllerId)
        {
            return null;
        }

        public virtual void OnLobbyServerPlayerRemoved(NetworkConnection conn, short playerControllerId)
        {
        }

        // for users to apply settings from their lobby player object to their in-game player object
        public virtual bool OnLobbyServerSceneLoadedForPlayer(GameObject lobbyPlayer, GameObject gamePlayer)
        {
            return true;
        }

        public virtual void OnLobbyServerPlayersReady()
        {
            // all players are readyToBegin, start the game
            ServerChangeScene(m_PlayScene);
        }

        // ------------------------ lobby client virtuals ------------------------

        public virtual void OnLobbyClientEnter()
        {
        }

        public virtual void OnLobbyClientExit()
        {
        }

        public virtual void OnLobbyClientConnect(NetworkConnection conn)
        {
        }

        public virtual void OnLobbyClientDisconnect(NetworkConnection conn)
        {
        }

        public virtual void OnLobbyStartClient(NetworkClient lobbyClient)
        {
        }

        public virtual void OnLobbyStopClient()
        {
        }

        public virtual void OnLobbyClientSceneChanged(NetworkConnection conn)
        {
        }

        // for users to handle adding a player failed on the server
        public virtual void OnLobbyClientAddPlayerFailed()
        {
        }

        // ------------------------ optional UI ------------------------

        void OnGUI()
        {
            if (!showLobbyGUI)
                return;

            string loadedSceneName = SceneManager.GetSceneAt(0).name;
            if (loadedSceneName != m_LobbyScene)
                return;

            Rect backgroundRec = new Rect(90, 180, 500, 150);
            GUI.Box(backgroundRec, "Players:");

            if (NetworkClient.active)
            {
                Rect addRec = new Rect(100, 300, 120, 20);
                if (GUI.Button(addRec, "Add Player"))
                {
                    TryToAddPlayer();
                }
            }
        }

        public void TryToAddPlayer()
        {
            if (NetworkClient.active)
            {
                short controllerId = -1;
                var controllers = NetworkClient.allClients[0].connection.playerControllers;

                if (controllers.Count < maxPlayers)
                {
                    controllerId = (short)controllers.Count;
                }
                else
                {
                    for (short i = 0; i < maxPlayers; i++)
                    {
                        if (!controllers[i].IsValid)
                        {
                            controllerId = i;
                            break;
                        }
                    }
                }
                if (LogFilter.logDebug) { Debug.Log("NATLobbyManager TryToAddPlayer controllerId " + controllerId + " ready:" + ClientScene.ready); }

                if (controllerId == -1)
                {
                    if (LogFilter.logDebug) { Debug.Log("NATLobbyManager No Space!"); }
                    return;
                }

                if (ClientScene.ready)
                {
                    ClientScene.AddPlayer(controllerId);
                }
                else
                {
                    ClientScene.AddPlayer(NetworkClient.allClients[0].connection, controllerId);
                }
            }
            else
            {
                if (LogFilter.logDebug) { Debug.Log("NATLobbyManager NetworkClient not active!"); }
            }
        }
    }
}
#elif UNITY_5_2
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

namespace NATTraversal
{
    public class NATLobbyManager : NATTraversal.NetworkManager
    {
        struct PendingPlayer
        {
            public NetworkConnection conn;
            public GameObject lobbyPlayer;
        }

        // configuration
        [SerializeField]
        bool m_ShowLobbyGUI = true;
        [SerializeField]
        int m_MaxPlayers = 4;
        [SerializeField]
        int m_MaxPlayersPerConnection = 1;
        [SerializeField]
        int m_MinPlayers;
        [SerializeField]
        NATLobbyPlayer m_LobbyPlayerPrefab;
        [SerializeField]
        GameObject m_GamePlayerPrefab;
        [SerializeField]
        string m_LobbyScene = "";
        [SerializeField]
        string m_PlayScene = "";

        // runtime data
        List<PendingPlayer> m_PendingPlayers = new List<PendingPlayer>();
        public NATLobbyPlayer[] lobbySlots;

        public class LobbyReadyToBeginMessage : MessageBase
        {
            public byte slotId;
            public bool readyState;

            public override void Deserialize(NetworkReader reader)
            {
                slotId = reader.ReadByte();
                readyState = reader.ReadBoolean();
            }

            public override void Serialize(NetworkWriter writer)
            {
                writer.Write(slotId);
                writer.Write(readyState);
            }
        }

        // static message objects to avoid runtime-allocations
        static LobbyReadyToBeginMessage s_ReadyToBeginMessage = new LobbyReadyToBeginMessage();
        static IntegerMessage s_SceneLoadedMessage = new IntegerMessage();
        static LobbyReadyToBeginMessage s_LobbyReadyToBeginMessage = new LobbyReadyToBeginMessage();

        // properties
        public bool showLobbyGUI { get { return m_ShowLobbyGUI; } set { m_ShowLobbyGUI = value; } }
        public int maxPlayers { get { return m_MaxPlayers; } set { m_MaxPlayers = value; } }
        public int maxPlayersPerConnection { get { return m_MaxPlayersPerConnection; } set { m_MaxPlayersPerConnection = value; } }
        public int minPlayers { get { return m_MinPlayers; } set { m_MinPlayers = value; } }
        public NATLobbyPlayer lobbyPlayerPrefab { get { return m_LobbyPlayerPrefab; } set { m_LobbyPlayerPrefab = value; } }
        public GameObject gamePlayerPrefab { get { return m_GamePlayerPrefab; } set { m_GamePlayerPrefab = value; } }
        public string lobbyScene { get { return m_LobbyScene; } set { m_LobbyScene = value; offlineScene = value; } }
        public string playScene { get { return m_PlayScene; } set { m_PlayScene = value; } }

        void OnValidate()
        {
            if (m_MaxPlayers <= 0)
            {
                m_MaxPlayers = 1;
            }

            if (m_MaxPlayersPerConnection <= 0)
            {
                m_MaxPlayersPerConnection = 1;
            }

            if (m_MaxPlayersPerConnection > maxPlayers)
            {
                m_MaxPlayersPerConnection = maxPlayers;
            }

            if (m_MinPlayers < 0)
            {
                m_MinPlayers = 0;
            }

            if (m_MinPlayers > m_MaxPlayers)
            {
                m_MinPlayers = m_MaxPlayers;
            }
        }

        Byte FindSlot()
        {
            for (byte i = 0; i < maxPlayers; i++)
            {
                if (lobbySlots[i] == null)
                {
                    return i;
                }
            }
            return Byte.MaxValue;
        }

        void SceneLoadedForPlayer(NetworkConnection conn, GameObject lobbyPlayerGameObject)
        {
            var lobbyPlayer = lobbyPlayerGameObject.GetComponent<NATLobbyPlayer>();
            if (lobbyPlayer == null)
            {
                // not a lobby player.. dont replace it
                return;
            }

            if (LogFilter.logDebug) { Debug.Log("NATLobby SceneLoadedForPlayer scene:" + Application.loadedLevelName + " " + conn); }

            if (Application.loadedLevelName == m_LobbyScene)
            {
                // cant be ready in lobby, add to ready list
                PendingPlayer pending;
                pending.conn = conn;
                pending.lobbyPlayer = lobbyPlayerGameObject;
                m_PendingPlayers.Add(pending);
                return;
            }

            var controllerId = lobbyPlayerGameObject.GetComponent<NetworkIdentity>().playerControllerId;
            var gamePlayer = OnLobbyServerCreateGamePlayer(conn, controllerId);
            if (gamePlayer == null)
            {
                // get start position from base class
                Transform startPos = GetStartPosition();
                if (startPos != null)
                {
                    gamePlayer = (GameObject)Instantiate(gamePlayerPrefab, startPos.position, startPos.rotation);
                }
                else
                {
                    gamePlayer = (GameObject)Instantiate(gamePlayerPrefab, Vector3.zero, Quaternion.identity);
                }
            }

            if (!OnLobbyServerSceneLoadedForPlayer(lobbyPlayerGameObject, gamePlayer))
            {
                return;
            }

            // replace lobby player with game player
            NetworkServer.ReplacePlayerForConnection(conn, gamePlayer, controllerId);
        }

        static bool CheckConnectionIsReadyToBegin(NetworkConnection conn)
        {
            foreach (var player in conn.playerControllers)
            {
                if (player.IsValid)
                {
                    var lobbyPlayer = player.gameObject.GetComponent<NATLobbyPlayer>();
                    if (!lobbyPlayer.readyToBegin)
                        return false;
                }
            }
            return true;
        }

        public void CheckReadyToBegin()
        {
            if (Application.loadedLevelName != m_LobbyScene)
            {
                return;
            }

            int readyCount = 0;

            foreach (var conn in NetworkServer.connections)
            {
                if (conn == null)
                    continue;

                if (CheckConnectionIsReadyToBegin(conn))
                {
                    readyCount += 1;
                }
                else
                {
                    return;
                }
            }

            foreach (var conn in NetworkServer.localConnections)
            {
                if (conn == null)
                    continue;

                if (CheckConnectionIsReadyToBegin(conn))
                {
                    readyCount += 1;
                }
                else
                {
                    return;
                }
            }
            if (m_MinPlayers > 0 && readyCount < m_MinPlayers)
            {
                // not enough players ready yet.
                return;
            }

            m_PendingPlayers.Clear();
            OnLobbyServerPlayersReady();
        }

        public void ServerReturnToLobby()
        {
            if (!NetworkServer.active)
            {
                Debug.Log("ServerReturnToLobby called on client");
                return;
            }
            ServerChangeScene(m_LobbyScene);
        }

        void CallOnClientEnterLobby()
        {
            OnLobbyClientEnter();
            foreach (var player in lobbySlots)
            {
                if (player == null)
                    continue;

                player.readyToBegin = false;
                player.OnClientEnterLobby();
            }
        }

        void CallOnClientExitLobby()
        {
            OnLobbyClientExit();
            foreach (var player in lobbySlots)
            {
                if (player == null)
                    continue;

                player.OnClientExitLobby();
            }
        }

        public bool SendReturnToLobby()
        {
            if (client == null || !client.isConnected)
            {
                return false;
            }

            var msg = new EmptyMessage();
            client.Send(MsgType.LobbyReturnToLobby, msg);
            return true;
        }

        // ------------------------ server handlers ------------------------

        public override void OnServerConnect(NetworkConnection conn)
        {
            if (numPlayers >= maxPlayers)
            {
                conn.Disconnect();
                return;
            }

            // cannot join game in progress
            if (Application.loadedLevelName != m_LobbyScene)
            {
                conn.Disconnect();
                return;
            }

            base.OnServerConnect(conn);
            OnLobbyServerConnect(conn);
        }

        public override void OnServerDisconnect(NetworkConnection conn)
        {
            base.OnServerDisconnect(conn);
            OnLobbyServerDisconnect(conn);
        }

        public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
        {
            if (Application.loadedLevelName != m_LobbyScene)
            {
                return;
            }

            // check MaxPlayersPerConnection
            int numPlayersForConnection = 0;
            foreach (var player in conn.playerControllers)
            {
                if (player.IsValid)
                    numPlayersForConnection += 1;
            }

            if (numPlayersForConnection >= maxPlayersPerConnection)
            {
                if (LogFilter.logWarn) { Debug.LogWarning("NATLobbyManager no more players for this connection."); }

                var errorMsg = new EmptyMessage();
                conn.Send(MsgType.LobbyAddPlayerFailed, errorMsg);
                return;
            }

            byte slot = FindSlot();
            if (slot == Byte.MaxValue)
            {
                if (LogFilter.logWarn) { Debug.LogWarning("NATLobbyManager no space for more players"); }

                var errorMsg = new EmptyMessage();
                conn.Send(MsgType.LobbyAddPlayerFailed, errorMsg);
                return;
            }

            var newLobbyGameObject = OnLobbyServerCreateLobbyPlayer(conn, playerControllerId);
            if (newLobbyGameObject == null)
            {
                newLobbyGameObject = (GameObject)Instantiate(lobbyPlayerPrefab.gameObject, Vector3.zero, Quaternion.identity);
            }

            var newLobbyPlayer = newLobbyGameObject.GetComponent<NATLobbyPlayer>();
            newLobbyPlayer.slot = slot;
            lobbySlots[slot] = newLobbyPlayer;

            NetworkServer.AddPlayerForConnection(conn, newLobbyGameObject, playerControllerId);
        }

        public override void OnServerRemovePlayer(NetworkConnection conn, PlayerController player)
        {
            var playerControllerId = player.playerControllerId;
            byte slot = player.gameObject.GetComponent<NATLobbyPlayer>().slot;
            lobbySlots[slot] = null;
            base.OnServerRemovePlayer(conn, player);

            foreach (var p in lobbySlots)
            {
                if (p != null)
                {
                    p.GetComponent<NATLobbyPlayer>().readyToBegin = false;

                    s_LobbyReadyToBeginMessage.slotId = p.slot;
                    s_LobbyReadyToBeginMessage.readyState = false;
                    NetworkServer.SendToReady(null, MsgType.LobbyReadyToBegin, s_LobbyReadyToBeginMessage);
                }
            }

            OnLobbyServerPlayerRemoved(conn, playerControllerId);
        }

        public override void ServerChangeScene(string sceneName)
        {
            if (sceneName == m_LobbyScene)
            {
                foreach (var lobbyPlayer in lobbySlots)
                {
                    if (lobbyPlayer == null)
                        continue;

                    // find the game-player object for this connection, and destroy it
                    var uv = lobbyPlayer.GetComponent<NetworkIdentity>();

                    PlayerController playerController;
                    if (GetPlayerController(uv.connectionToClient, uv.playerControllerId, out playerController))
                    {
                        NetworkServer.Destroy(playerController.gameObject);
                    }

                    if (NetworkServer.active)
                    {
                        // re-add the lobby object
                        lobbyPlayer.GetComponent<NATLobbyPlayer>().readyToBegin = false;
                        NetworkServer.ReplacePlayerForConnection(uv.connectionToClient, lobbyPlayer.gameObject, uv.playerControllerId);
                    }
                }
            }
            base.ServerChangeScene(sceneName);
        }

        public override void OnServerSceneChanged(string sceneName)
        {
            if (sceneName != m_LobbyScene)
            {
                // call SceneLoadedForPlayer on any players that become ready while we were loading the scene.
                foreach (var pending in m_PendingPlayers)
                {
                    SceneLoadedForPlayer(pending.conn, pending.lobbyPlayer);
                }
                m_PendingPlayers.Clear();
            }

            OnLobbyServerSceneChanged(sceneName);
        }

        void OnServerReadyToBeginMessage(NetworkMessage netMsg)
        {
            if (LogFilter.logDebug) { Debug.Log("NATLobbyManager OnServerReadyToBeginMessage"); }
            netMsg.ReadMessage(s_ReadyToBeginMessage);

            PlayerController lobbyController;
            if (!GetPlayerController(netMsg.conn, (short)s_ReadyToBeginMessage.slotId, out lobbyController))
            {
                if (LogFilter.logError) { Debug.LogError("NATLobbyManager OnServerReadyToBeginMessage invalid playerControllerId " + s_ReadyToBeginMessage.slotId); }
                return;
            }

            // set this player ready
            var lobbyPlayer = lobbyController.gameObject.GetComponent<NATLobbyPlayer>();
            lobbyPlayer.readyToBegin = s_ReadyToBeginMessage.readyState;

            // tell every player that this player is ready
            var outMsg = new LobbyReadyToBeginMessage();
            outMsg.slotId = lobbyPlayer.slot;
            outMsg.readyState = s_ReadyToBeginMessage.readyState;
            NetworkServer.SendToReady(null, MsgType.LobbyReadyToBegin, outMsg);

            // maybe start the game
            CheckReadyToBegin();
        }

        void OnServerSceneLoadedMessage(NetworkMessage netMsg)
        {
            if (LogFilter.logDebug) { Debug.Log("NATLobbyManager OnSceneLoadedMessage"); }

            netMsg.ReadMessage(s_SceneLoadedMessage);

            PlayerController lobbyController;
            if (!GetPlayerController(netMsg.conn, (short)s_SceneLoadedMessage.value, out lobbyController))
            {
                if (LogFilter.logError) { Debug.LogError("NATLobbyManager OnServerSceneLoadedMessage invalid playerControllerId " + s_SceneLoadedMessage.value); }
                return;
            }

            SceneLoadedForPlayer(netMsg.conn, lobbyController.gameObject);
        }

        bool GetPlayerController(NetworkConnection client, short playerControllerId, out PlayerController playerController)
        {
            playerController = null;
            if (client.playerControllers.Count > 0)
            {
                for (int i = 0; i < client.playerControllers.Count; i++)
                {
                    if (client.playerControllers[i].IsValid && client.playerControllers[i].playerControllerId == playerControllerId)
                    {
                        playerController = client.playerControllers[i];
                        return true;
                    }
                }
                return false;
            }
            return false;
        }

        void OnServerReturnToLobbyMessage(NetworkMessage netMsg)
        {
            if (LogFilter.logDebug) { Debug.Log("NATLobbyManager OnServerReturnToLobbyMessage"); }

            ServerReturnToLobby();
        }

        public override void OnStartServer()
        {
            if (lobbySlots.Length == 0)
            {
                lobbySlots = new NATLobbyPlayer[maxPlayers];
            }

            NetworkServer.RegisterHandler(MsgType.LobbyReadyToBegin, OnServerReadyToBeginMessage);
            NetworkServer.RegisterHandler(MsgType.LobbySceneLoaded, OnServerSceneLoadedMessage);
            NetworkServer.RegisterHandler(MsgType.LobbyReturnToLobby, OnServerReturnToLobbyMessage);

            OnLobbyStartServer();
        }

        public override void OnStartHost()
        {
            OnLobbyStartHost();
        }

        public override void OnStopHost()
        {
            OnLobbyStopHost();
        }

        // ------------------------ client handlers ------------------------

        public override void OnStartClient(NetworkClient lobbyClient)
        {
            if (lobbySlots.Length == 0)
            {
                lobbySlots = new NATLobbyPlayer[maxPlayers];
            }

            if (m_LobbyPlayerPrefab == null || m_LobbyPlayerPrefab.gameObject == null)
            {
                if (LogFilter.logError) { Debug.LogError("NATLobbyManager no LobbyPlayer prefab is registered. Please add a LobbyPlayer prefab."); }
            }
            else
            {
                ClientScene.RegisterPrefab(m_LobbyPlayerPrefab.gameObject);
            }

            if (m_GamePlayerPrefab == null)
            {
                if (LogFilter.logError) { Debug.LogError("NATLobbyManager no GamePlayer prefab is registered. Please add a GamePlayer prefab."); }
            }
            else
            {
                ClientScene.RegisterPrefab(m_GamePlayerPrefab);
            }

            lobbyClient.RegisterHandler(MsgType.LobbyReadyToBegin, OnClientReadyToBegin);
            lobbyClient.RegisterHandler(MsgType.LobbyAddPlayerFailed, OnClientAddPlayerFailedMessage);

            OnLobbyStartClient(lobbyClient);
        }

        public override void OnClientConnect(NetworkConnection conn)
        {
            OnLobbyClientConnect(conn);
            CallOnClientEnterLobby();
            base.OnClientConnect(conn);
        }

        public override void OnClientDisconnect(NetworkConnection conn)
        {
            OnLobbyClientDisconnect(conn);
            base.OnClientDisconnect(conn);
        }

        public override void OnStopClient()
        {
            OnLobbyStopClient();
            CallOnClientExitLobby();
        }

        public override void OnClientSceneChanged(NetworkConnection conn)
        {
            if (Application.loadedLevelName == lobbyScene)
            {
                if (client.isConnected)
                {
                    CallOnClientEnterLobby();
                }
            }
            else
            {
                CallOnClientExitLobby();
            }

            base.OnClientSceneChanged(conn);
            OnLobbyClientSceneChanged(conn);
        }

        void OnClientReadyToBegin(NetworkMessage netMsg)
        {
            netMsg.ReadMessage(s_LobbyReadyToBeginMessage);

            if (s_LobbyReadyToBeginMessage.slotId >= lobbySlots.Count())
            {
                if (LogFilter.logError) { Debug.LogError("NATLobbyManager OnClientReadyToBegin invalid lobby slot " + s_LobbyReadyToBeginMessage.slotId); }
                return;
            }

            var lobbyPlayer = lobbySlots[s_LobbyReadyToBeginMessage.slotId];
            if (lobbyPlayer == null || lobbyPlayer.gameObject == null)
            {
                if (LogFilter.logError) { Debug.LogError("NATLobbyManager OnClientReadyToBegin no player at lobby slot " + s_LobbyReadyToBeginMessage.slotId); }
                return;
            }

            lobbyPlayer.readyToBegin = s_LobbyReadyToBeginMessage.readyState;
            lobbyPlayer.OnClientReady(s_LobbyReadyToBeginMessage.readyState);
        }

        void OnClientAddPlayerFailedMessage(NetworkMessage netMsg)
        {
            if (LogFilter.logDebug) { Debug.Log("NATLobbyManager Add Player failed."); }
            OnLobbyClientAddPlayerFailed();
        }

        // ------------------------ lobby server virtuals ------------------------

        public virtual void OnLobbyStartHost()
        {
        }

        public virtual void OnLobbyStopHost()
        {
        }

        public virtual void OnLobbyStartServer()
        {
        }

        public virtual void OnLobbyServerConnect(NetworkConnection conn)
        {
        }

        public virtual void OnLobbyServerDisconnect(NetworkConnection conn)
        {
        }

        public virtual void OnLobbyServerSceneChanged(string sceneName)
        {
        }

        public virtual GameObject OnLobbyServerCreateLobbyPlayer(NetworkConnection conn, short playerControllerId)
        {
            return null;
        }

        public virtual GameObject OnLobbyServerCreateGamePlayer(NetworkConnection conn, short playerControllerId)
        {
            return null;
        }

        public virtual void OnLobbyServerPlayerRemoved(NetworkConnection conn, short playerControllerId)
        {
        }

        // for users to apply settings from their lobby player object to their in-game player object
        public virtual bool OnLobbyServerSceneLoadedForPlayer(GameObject lobbyPlayer, GameObject gamePlayer)
        {
            return true;
        }

        public virtual void OnLobbyServerPlayersReady()
        {
            // all players are readyToBegin, start the game
            ServerChangeScene(m_PlayScene);
        }

        // ------------------------ lobby client virtuals ------------------------

        public virtual void OnLobbyClientEnter()
        {
        }

        public virtual void OnLobbyClientExit()
        {
        }

        public virtual void OnLobbyClientConnect(NetworkConnection conn)
        {
        }

        public virtual void OnLobbyClientDisconnect(NetworkConnection conn)
        {
        }

        public virtual void OnLobbyStartClient(NetworkClient lobbyClient)
        {
        }

        public virtual void OnLobbyStopClient()
        {
        }

        public virtual void OnLobbyClientSceneChanged(NetworkConnection conn)
        {
        }

        // for users to handle adding a player failed on the server
        public virtual void OnLobbyClientAddPlayerFailed()
        {
        }

        // ------------------------ optional UI ------------------------

        void OnGUI()
        {
            if (!showLobbyGUI)
                return;

            if (Application.loadedLevelName != m_LobbyScene)
                return;

            Rect backgroundRec = new Rect(90, 180, 500, 150);
            GUI.Box(backgroundRec, "Players:");

            if (NetworkClient.active)
            {
                Rect addRec = new Rect(100, 300, 120, 20);
                if (GUI.Button(addRec, "Add Player"))
                {
                    TryToAddPlayer();
                }
            }
        }

        public void TryToAddPlayer()
        {
            if (NetworkClient.active)
            {
                short controllerId = -1;
                var controllers = NetworkClient.allClients[0].connection.playerControllers;

                if (controllers.Count < maxPlayers)
                {
                    controllerId = (short)controllers.Count;
                }
                else
                {
                    for (short i = 0; i < maxPlayers; i++)
                    {
                        if (!controllers[i].IsValid)
                        {
                            controllerId = i;
                            break;
                        }
                    }
                }
                if (LogFilter.logDebug) { Debug.Log("NATLobbyManager TryToAddPlayer controllerId " + controllerId + " ready:" + ClientScene.ready); }

                if (controllerId == -1)
                {
                    if (LogFilter.logDebug) { Debug.Log("NATLobbyManager No Space!"); }
                    return;
                }

                if (ClientScene.ready)
                {
                    ClientScene.AddPlayer(controllerId);
                }
                else
                {
                    ClientScene.AddPlayer(NetworkClient.allClients[0].connection, controllerId);
                }
            }
            else
            {
                if (LogFilter.logDebug) { Debug.Log("NATLobbyManager NetworkClient not active!"); }
            }
        }
    }
}
#endif