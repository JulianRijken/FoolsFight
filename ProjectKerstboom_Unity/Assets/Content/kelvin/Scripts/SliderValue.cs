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
	float roundsSlect;
	public TMP_Text text;

	public GameObject sliderobj;
	public GameObject textobj;

	public GameObject prisliderobj;
	public GameObject pritextobj;

	// Update is called once per frame
	void Update()
    {
		text.text = roundsSlect + "  rounds";
		roundsSlect = sliderval.value;
		Slidervalue();
		if (PhotonNetwork.IsMasterClient)
		{
			sliderobj.SetActive(true);
			textobj.SetActive(true);
			prisliderobj.SetActive(true);
			pritextobj.SetActive(true);
		}
	}

	void Slidervalue()
	{
		if (PhotonNetwork.IsMasterClient)
		{
			if (sliderval.value == 1)
			{
				roundsSlect = 3;
			}
			if (sliderval.value == 2)
			{
				roundsSlect = 5;
			}
			if (sliderval.value == 3)
			{
				roundsSlect = 7;
			}
			if (sliderval.value == 4)
			{
				roundsSlect = 9;
			}
		}
		
	}
	
}
