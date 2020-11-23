using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
using System.Linq;

public class PhotonMulti : MonoBehaviourPunCallbacks 
{
	public static PhotonMulti Instance;

	[SerializeField] GameObject mainCanvas, optionCanvas, hostCanvas, findRoomCanvas, RoomCanvas;
	[SerializeField] TMP_InputField roomNameInputField;
	[SerializeField] TMP_Text roomNameText;
	[SerializeField] Transform roomListContent;
	[SerializeField] GameObject roomListItemPrefab;
	[SerializeField] Transform playerListContent;
	[SerializeField] GameObject PlayerListItemPrefab;
	[SerializeField] GameObject startGameButton;

	void Awake()
	{
		Instance = this;
	}

	void Start()
	{
		Debug.Log("Connecting to Master");
		PhotonNetwork.ConnectUsingSettings();
	}

	public override void OnConnectedToMaster()
	{
		Debug.Log("Connected to Master");
		PhotonNetwork.JoinLobby();
		PhotonNetwork.AutomaticallySyncScene = true;
	}

	public override void OnJoinedLobby()
	{
		mainCanvas.SetActive(true);
		Debug.Log("Joined Lobby");
	}

	public void CreateRoom()
	{

		if (string.IsNullOrEmpty(roomNameInputField.text))
		{
			PhotonNetwork.CreateRoom(PhotonNetwork.NickName + "'s"+ "game");
		}
		PhotonNetwork.CreateRoom(roomNameInputField.text);
		
	}

	public override void OnJoinedRoom()
	{
		hostCanvas.SetActive(false);
		RoomCanvas.SetActive(true);
		roomNameText.text = PhotonNetwork.CurrentRoom.Name;

		Player[] players = PhotonNetwork.PlayerList;

		foreach (Transform child in playerListContent)
		{
			Destroy(child.gameObject);
		}

		for (int i = 0; i < players.Count(); i++)
		{
			Instantiate(PlayerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(players[i]);
		}

		startGameButton.SetActive(PhotonNetwork.IsMasterClient);
	}

	public void JoinRoom(RoomInfo info)
	{
		PhotonNetwork.JoinRoom(info.Name);
		hostCanvas.SetActive(false);
		optionCanvas.SetActive(false);
		findRoomCanvas.SetActive(false);
		mainCanvas.SetActive(false);
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
			Debug.Log("list update");
		}
	}

	public override void OnPlayerEnteredRoom(Player newPlayer)
	{
		Instantiate(PlayerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(newPlayer);

	}

}
