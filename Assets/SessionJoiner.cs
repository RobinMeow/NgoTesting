using System;
using Unity.Netcode;
using UnityEngine;

public class SessionJoiner : MonoBehaviour
{
    void OnGUI()
    {
        var hostBtnPos = new Rect(0, 0, 100, 100);
        if (GUI.Button(hostBtnPos, "Host"))
        {
            NetworkManager.Singleton.StartHost();
            DeactiveSessionOverlay();
        }
        else if (GUI.Button(new Rect(0, hostBtnPos.height, hostBtnPos.width, hostBtnPos.height), "Client"))
        {
            NetworkManager.Singleton.StartClient();
            DeactiveSessionOverlay();
        }
    }

    void DeactiveSessionOverlay()
    {
        enabled = false;
    }
}
