using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class DeckBuilder : MonoBehaviour {

	public static DeckBuilder instance;

	[SerializeField] List <GameObject> Cards = new List<GameObject>(); // ALLL game cards
	[SerializeField] public List <GameObject> DeckList = new List<GameObject>();

	[SerializeField] private Button cardItem;
	[SerializeField] private List <Button> cardItemList = new List <Button>();
	[SerializeField] private GameObject cardItemParent;
	[SerializeField] public GameObject activeCard; ///means this is the card the player has moused over
	[SerializeField] private List <GameObject> deckItems;//items that appear in the deck list

	[SerializeField] private TextMeshProUGUI deckCountInfo;

	[SerializeField] private GameObject deckList;//UI element

	[SerializeField] private GameObject savedDeckPopUp;
	[SerializeField] private TMP_InputField inputField;

	[SerializeField] private GameObject deckSelectPopUp;
	[SerializeField] private GameObject deckSelectButton;
	[SerializeField] private Transform deckSelectParent;

	private void Awake(){
		if(!instance)
			instance = this;
		else
			Destroy(gameObject);
	}

	private void Start(){
		LoadAllCards();
		// deckSelectPopUp.SetActive(false);
		// StartCoroutine(SelectADeck());
	}

	private void Update(){
		RegisterClick();
		deckCountInfo.text = (DeckList.Count + "/40");
	}

	private IEnumerator SelectADeck(){
		if(GameManager.instance.player1Decks.Count > 0){
			deckSelectPopUp.SetActive(true);
			yield return new WaitForSeconds(2);
			PopulateDeckSelector();
		}
		yield return null;
	}

	private void PopulateDeckSelector(){
		// foreach(Deck deck in GameManager.instance.player1Decks){
		// 	GameObject newDeckSelect = 
		// 	Instantiate(deckSelectButton, transform.position, Quaternion.identity);
		// 	newDeckSelect.transform.SetParent(deckSelectParent);

		// 	DeckSelectItem deckInfo = newDeckSelect.GetComponent<DeckSelectItem>();
		// 	//deckInfo.deck = deck;
		// 	deckInfo._name = deck._name;
		// 	deckInfo.GetComponentInChildren<TextMeshProUGUI>().text = deckInfo._name;
		// }
	}

	public void EditSelectedDeck(){
		deckSelectPopUp.SetActive(false);
		ClearDeckList();
		RenderDeck();
	}

	private void LoadAllCards(){
		foreach(GameObject card in GameManager.instance.AllCards){
			CardBase cardInfo = card.GetComponent<CardBase>();
			Button newCardItem = Instantiate(cardItem, cardItemParent.transform.position, Quaternion.identity);
			newCardItem.GetComponent<Image>().sprite = cardInfo.GetComponentInChildren<Image>().sprite;
			newCardItem.transform.SetParent(cardItemParent.transform);

			CardUiItem cardUiItem = newCardItem.GetComponent<CardUiItem>();
			cardUiItem.card = card;
			cardUiItem._name = cardUiItem.card.GetComponent<CardBase>()._name;

			cardItemList.Add(newCardItem);
		}
	}

	public void RegisterClick(){
		if(Input.GetMouseButtonDown(0)){
			if(DeckList.Count < 40 && activeCard){
				CardBase cardInfo = activeCard.GetComponent<CardUiItem>().card.GetComponent<CardBase>();
				int numberOfCopies = 0;
				foreach(GameObject card in DeckList){
					if(card == cardInfo.gameObject){
						numberOfCopies++;
					}
				}
				if(numberOfCopies < 4){
					DeckList.Add(cardInfo.gameObject);
				}
					ClearDeckList();
					RenderDeck();
			}else{
				Debug.Log("You have the maximum of forty cards in your deck");
			}
		}
		if(Input.GetMouseButtonDown(1)){
			if(activeCard){
				CardBase cardInfo = activeCard.GetComponent<CardUiItem>().card.GetComponent<CardBase>();
				DeckList.Remove(cardInfo.gameObject);
				ClearDeckList();
				RenderDeck();
			}

		}
	}

	private void RenderDeck(){
		foreach(GameObject card in DeckList){
			CardBase cardInfo = card.GetComponent<CardBase>();
			Button newCardItem = Instantiate(cardItem, cardItemParent.transform.position, Quaternion.identity);
			newCardItem.GetComponent<Image>().sprite = cardInfo.GetComponentInChildren<Image>().sprite;
			newCardItem.transform.SetParent(deckList.transform);
			CardUiItem cardUiItem = newCardItem.GetComponent<CardUiItem>();
			cardUiItem.card = card;
			cardUiItem._name = cardUiItem.card.GetComponent<CardBase>()._name;

			deckItems.Add(newCardItem.gameObject);///the items that appear in the deck list
		}	
	}

	private void ClearDeckList(){
		foreach(GameObject card in deckItems){
			Destroy(card);
		}
		deckItems.Clear();
	}

	public void OpenSaveDeckPopUp(){
		savedDeckPopUp.SetActive(true);
	}

	public void SaveNewDeck(){
		if(DeckList.Count >= 4){
			GameObject newDeck = Instantiate(GameManager.instance.deckPrefab, GameManager.instance.transform.position, GameManager.instance.transform.rotation);
			Deck newDeckInfo = newDeck.GetComponent<Deck>();
			newDeckInfo._name = inputField.text;
			newDeckInfo.deck = DeckList;
			GameManager.instance.player1Decks.Add(newDeck);
		}
		savedDeckPopUp.SetActive(false);
	}

	public void Cancel(){
		inputField.text = "";
		savedDeckPopUp.SetActive(false);
	}

	public void BackToMainMenu(){
		GameManager.instance.SaveGame();
		GameManager.instance.LoadGame();
		SceneManager.LoadScene("Main Menu");
	}

	public void ClearAllDecks(){
		GameManager.instance.DeleteAllDecks();
	}

}
