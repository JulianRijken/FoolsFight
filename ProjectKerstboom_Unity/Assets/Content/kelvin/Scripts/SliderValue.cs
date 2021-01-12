using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

public class SliderValue : MonoBehaviour
{
	public Slider sliderval;
	float wins;
	public TMP_Text text;

	public GameObject sliderobj;
	public GameObject textobj;

	public GameObject prisliderobj;
	public GameObject pritextobj;

	// Update is called once per frame
	void Update()
    {
		text.text =  "score points to win: " + wins ;
		wins = sliderval.value;

		if (PhotonNetwork.IsMasterClient)
		{
			sliderobj.SetActive(true);
			textobj.SetActive(true);
			prisliderobj.SetActive(true);
			pritextobj.SetActive(true);
		}
	}

}
