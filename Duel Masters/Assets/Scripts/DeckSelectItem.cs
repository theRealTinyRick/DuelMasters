using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckSelectItem : MonoBehaviour {

	public List <GameObject> deck = new List<GameObject>();
	public string _name = "New Deck";
	public int index = 0;

	private void Start(){
		GetComponent<Button>().onClick.AddListener(delegate{Click();});
	}

	private void Click(){
		Debug.Log("this worked this far");
		GameManager.instance.SetPlayerDeck(index);
	}

}
