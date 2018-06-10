using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour {

	[SerializeField] private GameObject duelStartPopUp;
	[SerializeField] private GameObject deckSelectButton;
	[SerializeField] private Transform deckSelectParent;
	[SerializeField] private List <GameObject> player1Decks = new List <GameObject>();

	private void Start(){
		GameManager.instance.LoadGame();
	}

	public void Duel(){
		duelStartPopUp.SetActive(true);
		PopulateDeckSelector();
		//GameManager.instance.activePlayer1Deck = GameManager.instance.player1Decks[0];

		//SceneManager.LoadScene("main");
	}

	public void Back(){
		duelStartPopUp.SetActive(false);
	}

	public void PlayerDeckSelect(){
		
	}
	
	public void DeckBuilder(){
		SceneManager.LoadScene("DeckBuilder");
	}
	
	public void Save(){
		GameManager.instance.SaveGame();
	}

	public void Load(){
		GameManager.instance.LoadGame();
	}

	private void PopulateDeckSelector(){
		int i = 0;
		foreach(GameObject deck in GameManager.instance.player1Decks){
			Deck deckInfo = deck.GetComponent<Deck>();
			GameObject newDeckSelect = 
			Instantiate(deckSelectButton, transform.position, Quaternion.identity);
			newDeckSelect.transform.SetParent(deckSelectParent);
			DeckSelectItem deckSelectItem = newDeckSelect.GetComponent<DeckSelectItem>();

			deckSelectItem.deck = deckInfo.deck;
			deckSelectItem._name = deck.GetComponent<Deck>()._name;
			deckSelectItem.index = i;
			deckSelectItem.GetComponentInChildren<TextMeshProUGUI>().text = deckInfo._name;
			i++;
		}
	}

	private void ClearDeckSelector(){
		
	}
}
