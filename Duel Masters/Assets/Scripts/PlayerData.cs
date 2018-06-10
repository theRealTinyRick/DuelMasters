using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;

[Serializable]
class PlayerData{
	public string userName = "Gadnuuk";
	public List <PlayerDeck> savedDecks = new List<PlayerDeck>(); 
}