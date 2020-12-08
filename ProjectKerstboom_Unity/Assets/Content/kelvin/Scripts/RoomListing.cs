using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RoomListing : MonoBehaviour
{
    [SerializeField]
    private TMP_Text text;

    public RoomInfo info;

    public void SetRoomInfo(RoomInfo roomInfo)
    {
        info = roomInfo;
        text.text = roomInfo.Name +","+  roomInfo.MaxPlayers;
    }

    public void Onclick()
    {
        if (info.MaxPlayers == info.PlayerCount)
        {
            Debug.Log("full");
        }
        else 
        PhotonMulti.Instance.JoinRoom(info);
    }
}
