using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

[Serializable] 
public class PlayerDeck{
	
	public string _name = "Deck Name";
	public List <string> DeckList = new List<string>();
}
