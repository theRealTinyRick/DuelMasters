using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Player : MonoBehaviour {

	//the players lists of cards
	public List<GameObject> Deck = new List<GameObject>();
	public List<GameObject> Hand = new List<GameObject>();
	public List<GameObject> BattleZone = new List<GameObject>();
	public List<GameObject> Sheilds = new List<GameObject>();
	public List<GameObject> ManaZone = new List<GameObject>();
	public List<GameObject> GraveYard = new List<GameObject>();

	//Posisitions on the gameBoard
	public Transform deckPos;
	public Transform handPos;
	public Transform battleZonePos;
	public Transform[] shieldZonePos;
	public Transform manaZonePos;
	public Transform graveYardPos;

	public Transform tappedRotation;
	
	//standard metrics
	public int totalMana;
	public int availableMana; 

	[SerializeField] private TextMeshProUGUI  playerMana;
	[SerializeField] private TextMeshProUGUI playerShields; 

	public List <CardBase.Civilizations> availableCiv = new List<CardBase.Civilizations>();

	public GameObject[] civIcons;
	public GameObject shieldObject;

	public void UseMana(int cost){
		availableMana-=cost;
		for(int i = 0; i < totalMana - availableMana; i++){
			CardBase cardInfo = ManaZone[i].GetComponent<CardBase>();
			cardInfo.isTapped = true;
		}
		playerMana.text = "Mana " + availableMana + "/" + totalMana;
	}

	public void ResetMana(){
		totalMana = ManaZone.Count;
		availableMana = totalMana;
		playerMana.text = "Mana " + availableMana + "/" + totalMana;

		foreach(GameObject card in ManaZone){
			CardBase cardInfo = card.GetComponent<CardBase>();
			cardInfo.isTapped = false;
		}
	}

	public void UpdateManaCiv(){
		for(int i = 0; i < ManaZone.Count; i++){
			CardBase cardInfo = ManaZone[i].GetComponent<CardBase>();
			for(int j = 0; j < cardInfo.civilizations.Count; j++){
				if(!availableCiv.Contains(cardInfo.civilizations[j]))
					availableCiv.Add(cardInfo.civilizations[j]);
			}
		}

		totalMana = ManaZone.Count;
		playerMana.text = "Mana " + availableMana + "/" + totalMana;

		if(availableCiv.Contains(CardBase.Civilizations.Light)){
			civIcons[0].SetActive(true);
		}else{
			civIcons[0].SetActive(false);
		}

		if(availableCiv.Contains(CardBase.Civilizations.Water)){
			civIcons[1].SetActive(true);
		}else{
			civIcons[1].SetActive(false);
		}

		if(availableCiv.Contains(CardBase.Civilizations.Darkness)){
			civIcons[2].SetActive(true);
		}else{
			civIcons[2].SetActive(false);
		}

		if(availableCiv.Contains(CardBase.Civilizations.Fire)){
			civIcons[3].SetActive(true);
		}else{
			civIcons[3].SetActive(false);
		}

		if(availableCiv.Contains(CardBase.Civilizations.Nature)){
			civIcons[4].SetActive(true);
		}else{
			civIcons[4].SetActive(false);
		}
	}
}
