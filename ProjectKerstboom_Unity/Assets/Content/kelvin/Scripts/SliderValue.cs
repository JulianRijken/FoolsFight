using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

public class SliderValue : MonoBehaviourPunCallbacks
{

	[SerializeField] private Slider slider;
	[SerializeField] private TMP_Text winsRequierdText;
	[SerializeField] private string textInforont;

	
	public override void OnJoinedRoom()
	{
		// Only enable slider for master
		slider.interactable = PhotonNetwork.IsMasterClient;

		// slider always visible 
		slider.gameObject.SetActive(true);
		winsRequierdText.gameObject.SetActive(true);

		slider.onValueChanged.AddListener(delegate { OnSliderUpdate(); });
	}

	// Only run on master
	public override void OnPlayerEnteredRoom(Player newPlayer)
	{
		if (!PhotonNetwork.IsMasterClient)	
			return;
		
		//OnSliderUpdate();
	}

	public override void OnPlayerLeftRoom(Player otherPlayer)
	{
		// If the master leaves check the slider interaction
			slider.interactable = PhotonNetwork.IsMasterClient;
	}


	// Only run on master
	public void OnSliderUpdate()
    {
		if (!PhotonNetwork.IsMasterClient)
			return;

		int winsRequierd = (int)slider.value;

		winsRequierdText.text = $"{textInforont} {winsRequierd}";
		photonView.RPC("OnSliderUpdateRPC", RpcTarget.Others, winsRequierd);
	}
	[PunRPC]
	public void OnSliderUpdateRPC(int winsRequierd)
    {
		winsRequierdText.text = $"{textInforont} {winsRequierd}"; 
		slider.value = winsRequierd;
	}
}
