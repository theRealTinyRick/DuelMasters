using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RevealedCardItem : MonoBehaviour {

	public GameObject card; 
	
	void Start () {
		Sprite cardSprite = card.GetComponentInChildren<Image>().sprite;
		GetComponent<Image>().sprite = cardSprite;

		GetComponent<Button>().onClick.AddListener(delegate{Select(card);});
	}

	void Select(GameObject card){
		DuelManager.instance.gameActions.SelectCard(card);
	}
}
