using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

	public static GameManager instance;
	public enum State{MainMenu, Duel};
	[SerializeField] private List <PlayerDeck> savedPlayer1Decks = new List<PlayerDeck>(); //this is the persistant data the the game will load from

	public List <GameObject> player1Decks = new List<GameObject>();
	public List <Deck> computerDecks  = new List<Deck>();
	public Deck activeDeck;
	public GameObject deckPrefab;

	[SerializeField] private GameObject duelMenu;

	public List <GameObject> AllCards = new List<GameObject>();
	
	private void Awake(){
		if(instance==null)
			instance = this;
		else if(instance != null)
			Destroy(gameObject);
		DontDestroyOnLoad(gameObject);

	}

	private void Update(){
		if(Input.GetKeyDown(KeyCode.Escape)){

		}
	}

	private void Start(){
		LoadGame();
		Debug.Log(Application.persistentDataPath);
	}

	public void SaveGame(){
		BinaryFormatter bf = new BinaryFormatter();
		FileStream file = File.Create(Application.persistentDataPath + "/playerInfo.dat");

		PlayerData data = new PlayerData();
		
		for(int i = 0; i < player1Decks.Count; i++){
			List <string> deckList = new List <string>();
			foreach(GameObject card in player1Decks[i].GetComponent<Deck>().deck){
				deckList.Add(card.GetComponent<CardBase>()._name);
			}
			PlayerDeck playerDeck = new PlayerDeck();
			playerDeck._name = player1Decks[i].GetComponent<Deck>()._name;
			playerDeck.DeckList = deckList;

			data.savedDecks.Add(playerDeck);
			player1Decks[i].transform.SetParent(GameManager.instance.transform);
		}

		bf.Serialize(file, data);
		file.Close();
	}

	public void LoadGame(){
		if(File.Exists(Application.persistentDataPath + "/playerInfo.dat")){
			BinaryFormatter bf = new BinaryFormatter();
			FileStream file = File.Open(Application.persistentDataPath + "/playerInfo.dat", FileMode.Open);
			PlayerData data = (PlayerData)bf.Deserialize(file);
			file.Close();
			savedPlayer1Decks = data.savedDecks;
			foreach(GameObject deck in player1Decks){
				Destroy(deck.gameObject);
			}
			player1Decks.Clear();

			foreach(PlayerDeck deck in savedPlayer1Decks){
				List <GameObject> convertedDeck = new List <GameObject>();
				for(int i = 0; i < deck.DeckList.Count; i++){
					foreach (GameObject card in AllCards){
						if(card.GetComponent<CardBase>()._name == deck.DeckList[i]){
							convertedDeck.Add(card);
							break;
						}
					}					
				}
				GameObject newDeck = Instantiate(deckPrefab, transform.position, transform.rotation);
				Deck newDeckInfo = newDeck.GetComponent<Deck>();
				newDeckInfo._name = deck._name;
				newDeckInfo.deck = convertedDeck;
				
				player1Decks.Add(newDeck);
				newDeck.transform.SetParent(GameManager.instance.transform);
			}
		}		  
	}

	public void SetPlayerDeck(int index){
		GameManager.instance.activeDeck = GameManager.instance.player1Decks[index].GetComponent<Deck>();
		Scene scene  = SceneManager.GetActiveScene();
		if(scene.name == "Main Menu"){
			SceneManager.LoadScene("Duel");
		}
		else if(scene.name == "DeckBuilder"){
			//DeckBuilder.instance.deckBeingEdited = activeDeck;
			//DeckBuilder.instance.EditSelectedDeck();
		}
	}	

	public void SetComputerDeck(DeckSelectItem deckItem){

	}

	public void DeleteAllDecks(){
		foreach(GameObject deck in player1Decks){
			Destroy(deck);
		}
		savedPlayer1Decks.Clear();
		player1Decks.Clear();
		SaveGame();
		LoadGame();
	}
}


