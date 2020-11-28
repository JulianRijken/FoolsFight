using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerListItem : MonoBehaviour
{
	[SerializeField] TMP_Text text;
	Player player;

	public void SetUp(Player _player)
	{
		player = _player;
		text.text = PhotonNetwork.NickName;
    }

	//public override void OnPlayerLeftRoom(Player otherPlayer)
	//{
	//	if (player == otherPlayer)
	//	{
	//		Destroy(gameObject);
	//	}
	//}

	//public override void OnLeftRoom()
	//{
	//	Destroy(gameObject);
	//}
}
