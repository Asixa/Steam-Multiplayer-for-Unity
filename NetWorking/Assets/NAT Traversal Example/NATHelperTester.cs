using UnityEngine;
using NATTraversal;
using UnityEngine.Networking;

public class NATHelperTester : MonoBehaviour
{
    public ushort directConnectPort = 7777;
    ulong hostGUID = 0;
    string hostGUIDString = "";

    NATHelper natHelper;

    void Awake()
    {
        LogFilter.currentLogLevel = LogFilter.Debug;
        natHelper = GetComponent<NATHelper>();
        
        // Calling this early makes port forwarding go faster
        natHelper.findNatDevice();

        // Connect to Facilitator for punchthrough
        natHelper.StartCoroutine(natHelper.connectToNATFacilitator());

        NetworkTransport.Init();
    }

    void OnGUI()
    {
        if (!natHelper.isConnectedToFacilitator)
        {
            GUI.enabled = false;
        }

        if (!natHelper.isPunchingThrough && !natHelper.isListeningForPunchthrough)
        {
            if (GUI.Button(new Rect(10, 10, 150, 40), "Listen for Punchthrough"))
            {
                Debug.Log("Listening for punchthrough");
                natHelper.StartCoroutine(natHelper.startListeningForPunchthrough(onHolePunchedServer));
            }
        }
        else if (natHelper.isListeningForPunchthrough)
        {
            if (GUI.Button(new Rect(10, 10, 150, 40), "Stop Listening"))
            {
                natHelper.StopListeningForPunchthrough();
            }
        }

        if (natHelper.isListeningForPunchthrough)
        {
            GUI.Label(new Rect(170, 10, 170, 20), "Host GUID");
            GUI.TextField(new Rect(170, 30, 200, 20), natHelper.guid.ToString());
        }
        else if (!natHelper.isPunchingThrough)
        {
            if (GUI.Button(new Rect(10, 60, 150, 40), "Punchthrough"))
            {
                Debug.Log("Trying to punch through");
                natHelper.StartCoroutine(natHelper.punchThroughToServer(hostGUID, onHolePunchedClient));
            }

            GUI.Label(new Rect(170, 60, 170, 20), "Host GUID");
            hostGUIDString = GUI.TextField(new Rect(170, 80, 200, 20), hostGUIDString);
            ulong.TryParse(hostGUIDString, out hostGUID);
        }

        if (GUI.Button(new Rect(10, 110, 150, 40), "Forward port"))
        {
            Debug.Log("Forward port: " + directConnectPort);
            natHelper.mapPort(directConnectPort, directConnectPort, 0, Protocol.Both, "NAT Test", onPortMappingDone);
        }

        if (natHelper.isForwardingPort || !natHelper.isDoneFindingNATDevice)
        {
            if (GUI.Button(new Rect(10, 160, 150, 40), "Stop port forwarding"))
            {
                natHelper.stopPortForwarding();
            }
        }
    }

    void onHolePunchedServer(int portToListenOn, ulong clientGUID)
    {
        Debug.Log("Start a server listening on this port: " + portToListenOn + " for client " + clientGUID);
    }

    void onHolePunchedClient(int clientPort, int serverPort, bool success)
    {
        if (success)
        {
            Debug.Log("Start a socket on " + clientPort + " and connect to the server on " + serverPort);
        }
        else
        {
            Debug.Log("Punchthrough failed.");
        }
    }

    void onPortMappingDone(Open.Nat.Mapping mapping, bool isError)
    {
        if (isError)
        {
            Debug.Log("Port mapping failed");
        }
        else
        {
            Debug.Log("Port " + mapping.PublicPort + " mapped (" + mapping.Protocol + ")");
        }
    }
}
