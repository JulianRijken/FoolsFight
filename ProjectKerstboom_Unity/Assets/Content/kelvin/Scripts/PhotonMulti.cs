using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using DG.Tweening;
using Photon.Realtime;
using System.Linq;
using System.Security.Principal;
using UnityEngine.UIElements;

public class PhotonMulti : MonoBehaviourPunCallbacks 
{
	public static PhotonMulti Instance;

	[SerializeField] GameObject mainCanvas, optionCanvas, hostCanvas, findRoomCanvas, RoomCanvas;
	[SerializeField] byte playerCount; 
	[SerializeField] RectTransform roomMenutran;
    [SerializeField] TMP_InputField roomNameInputField;
	[SerializeField] TMP_Text roomNameText;
	[SerializeField] Transform roomListContent;
	[SerializeField] GameObject roomListItemPrefab;
	[SerializeField] Transform playerListContent;
	[SerializeField] GameObject PlayerListItemPrefab;
	[SerializeField] GameObject startGameButton;
	[SerializeField] TMP_Text errorText;
	[SerializeField] GameObject privateRoom;
	[SerializeField] TMP_InputField codeInputField;

	void Awake()
	{
		Instance = this;
	}

	void Start()
	{
		//Debug.Log("Connecting to Master");
		PhotonNetwork.ConnectUsingSettings();
	}

	public override void OnConnectedToMaster()
	{
		//Debug.Log("Connected to Master");
		PhotonNetwork.JoinLobby();
		PhotonNetwork.AutomaticallySyncScene = true;
	}

	public override void OnJoinedLobby()
	{
		//Debug.Log("Joined Lobby");
		mainCanvas.SetActive(true);
	}

	public void CreateRoom()
	{

		if (string.IsNullOrEmpty(roomNameInputField.text))
		{
			PhotonNetwork.CreateRoom(PhotonNetwork.NickName + "'s"+ " game ", new RoomOptions { MaxPlayers = playerCount});
		}
		else
		PhotonNetwork.CreateRoom(roomNameInputField.text + " players: ", new RoomOptions { MaxPlayers = playerCount});

	}
	public void CreatePrivateRoom()
	{
		
		if (string.IsNullOrEmpty(roomNameInputField.text))
		{
			PhotonNetwork.CreateRoom(Random.Range(0, 1000).ToString("0000"), new RoomOptions { MaxPlayers = playerCount, IsOpen = false});
		}
		else
			PhotonNetwork.CreateRoom(Random.Range(0, 1000).ToString("0000"), new RoomOptions { MaxPlayers = playerCount, IsOpen = false});
		

	}
	public override void OnCreateRoomFailed(short returnCode, string message)
	{
		base.OnCreateRoomFailed(returnCode, message);
		errorText.text = message;
	}


	public override void OnJoinedRoom()
	{
		hostCanvas.SetActive(false);
		RoomCanvas.SetActive(true);
		roomMenutran.DOAnchorPos(new Vector2(0, 0), 0.50f);

		roomNameText.text = PhotonNetwork.CurrentRoom.Name;

		Player[] players = PhotonNetwork.PlayerList;

		//SetPlayerName();


		foreach (Transform child in playerListContent)
		{
			Destroy(child.gameObject);
		}
		

		for (int i = 0; i < players.Count(); i++)
		{
			//PhotonNetwork.NickName = players.ToString();
			Instantiate(PlayerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(players[i]);
		}
		
			startGameButton.SetActive(PhotonNetwork.IsMasterClient);

		
	}

	public void JoinRoom(RoomInfo info)
	{
		PhotonNetwork.JoinRoom(info.Name);
		hostCanvas.SetActive(false);
		findRoomCanvas.SetActive(false);
    }
	
	private void SetPlayerName()
	{
        int timesNicknameFound = 0;
        string origenalName = PhotonNetwork.NickName;
        bool sameNickname = false;

        do
		{
             sameNickname = false;

            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
			{

                Debug.Log(" 1 ----------------------");
                Debug.Log(PhotonNetwork.PlayerList[i].UserId);
                Debug.Log(PhotonNetwork.LocalPlayer.UserId);
                Debug.Log("----------------------");

                Debug.Log(" 2 ----------------------");
                Debug.Log(PhotonNetwork.NickName);
                Debug.Log(PhotonNetwork.PlayerList[i].NickName);
                Debug.Log("----------------------");

                if (PhotonNetwork.PlayerList[i].UserId == PhotonNetwork.LocalPlayer.UserId)
                    continue;

				if (PhotonNetwork.PlayerList[i].NickName != PhotonNetwork.NickName)
                    continue;

                

                timesNicknameFound++;
				PhotonNetwork.NickName = $"{origenalName} {timesNicknameFound}";
				sameNickname = true;
				
			}

			if(timesNicknameFound > 20)
            {
                Debug.LogError("While Loop Stuck");
                break;
            }
		}
		while (sameNickname);


    }

	public override void OnRoomListUpdate(List<RoomInfo> roomList)
	{
		foreach (Transform trans in roomListContent)
		{
			Destroy(trans.gameObject);
		}

		for (int i = 0; i < roomList.Count; i++)
		{
			if (roomList[i].RemovedFromList)
				continue;
			Instantiate(roomListItemPrefab, roomListContent).GetComponent<RoomListing>().SetRoomInfo(roomList[i]);
			//Debug.Log("list update");
		}
	}

	public override void OnPlayerEnteredRoom(Player newPlayer)
	{
		Instantiate(PlayerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(newPlayer);
	}

	public void DropDownInput(int info)
	{
		Debug.Log("list ");
		if (info == 1)
		{
			playerCount = 2;
			Debug.Log("list 0");
		}
		if (info == 2)
		{
			playerCount = 4;
			Debug.Log("list 1");
		}
		if (info == 3)
		{
			playerCount = 6;
			Debug.Log("list 2");
		}
	}

	public void JoinprivateRoom(RoomInfo info)
	{
		Debug.Log(codeInputField.text);
		hostCanvas.SetActive(false);
		findRoomCanvas.SetActive(true);
		PhotonNetwork.JoinRoom(info.Name);
	}

}
