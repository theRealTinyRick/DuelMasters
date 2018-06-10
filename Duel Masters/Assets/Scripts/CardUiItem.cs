using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardUiItem : MonoBehaviour {

	public GameObject card;
	public string _name; 

	public void ActiveCard(){
		DeckBuilder.instance.activeCard = gameObject;
	}

	public void NonActiveCard(){
		DeckBuilder.instance.activeCard = null;
	}
}
