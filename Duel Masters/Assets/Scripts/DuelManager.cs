using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class DuelManager : MonoBehaviour {
	public static DuelManager instance;	

	public enum DuelState{None, Casting, Attacking, Targeting, ChoosingBlocker, Resolving, EvolutionTargeting, ChoosingCards, Searching}; //use choosing cards for special effects that require it ie Rothus
	public DuelState duelState = DuelState.None;

	public enum TurnPhase{Draw, Mana, Main, Battle, End}
	public TurnPhase turnPhase = TurnPhase.Draw;
	[SerializeField] private TextMeshProUGUI currentPhaseText;

	public GameObject activeCard;
	public GameObject targetCard;
	public GameObject evoTarget;

	public GameObject attackingCard;
	public GameObject defendingCard;
	public GameObject activeBlocker;

	public List <GameObject> searchList;//denots where the player should look for a card

	public List <GameObject> chosenCards = new List <GameObject>();//use this for the state "Choosing cards for special effects"- clear the list when resolved
	public Player choosingPlayer; //same as the above property. use only for special effects "rothus for example"
	public List <GameObject> chooseLocation;

	public GameActions gameActions;
	public Transform effectResolveLocation; 

	public GameObject _playerObj;
	public GameObject _computerObj;
	public Player player1;
	public Player player2;
	public AIDuelest aIDuelest;
	public Player turnPlayer;
	
	public Player winner = null;

	public List <GameObject> playerDeck = new List<GameObject>();
	public List <GameObject> computerDeck = new List<GameObject>();

	[HideInInspector] public enum CardLocations{Deck, Hand, ManaZone, BattleZone, ShieldZone, Graveyard, Evolved};
	[HideInInspector] public enum Civilizations{Light, Water, Darkness, Fire, Nature};

	public List <GameObject> cardsInPlay = new List<GameObject>();

	[SerializeField] private TextMeshProUGUI turnPlayerText;
	[SerializeField] public TextMeshProUGUI actionPrompt;

	[SerializeField] private TextMeshProUGUI winnerText;

	private void Awake(){
		if(!instance){
			instance = this;
		}else if(instance){
			Destroy(gameObject);
		}

		player1 = _playerObj.GetComponent<Player>();
		player2 = _computerObj.GetComponent<Player>();
		aIDuelest = _computerObj.GetComponent<AIDuelest>();
		gameActions = GetComponent<GameActions>();
		turnPlayer = player1;
	}

	private void Start(){
		//SetDecks();
		StartCoroutine(StartOfGame());
	}

	IEnumerator StartOfGame(){
		player1.ResetMana();
		player2.ResetMana();
		SetDecks();
		yield return new WaitForSeconds(2);
		gameActions.StartCoroutine(gameActions.AddShields(player1, 5));
		gameActions.StartCoroutine(gameActions.AddShields(player2, 5));
		yield return new WaitForSeconds(3);
		gameActions.StartCoroutine(gameActions.Draw(player1, 5));
		gameActions.StartCoroutine(gameActions.Draw(player2, 5));
		turnPhase = TurnPhase.Draw;
		currentPhaseText.text = "Start";

		yield return new WaitForSeconds(3);
		turnPhase = TurnPhase.Draw;
		StartCoroutine(TurnManager());
	}
	
	private void Update(){
		setHandPosition(player1);
		setHandPosition(player2);

		setBattleZonePostion(player1);
		setBattleZonePostion(player2);
		
		setShieldZonePosition(player1);
		setShieldZonePosition(player2);

		setManaZonePosition(player1);
		setManaZonePosition(player2);
		setGraveYardPosition();  

		currentPhaseText.text = turnPhase.ToString();
	}

	public void SetDecks(){
		playerDeck = GameManager.instance.activeDeck.deck;
		player1.Deck = playerDeck;
		player1.Deck = GameActions.ShuffleDeck(player1.Deck);


		GameObject newComDeck = Instantiate(GameManager.instance.computerDecks[0].gameObject, transform.position, transform.rotation);
		computerDeck = newComDeck.GetComponent<Deck>().deck;
		player2.Deck = computerDeck;
		player2.Deck = GameActions.ShuffleDeck(player2.Deck);
	}

	public IEnumerator TurnManager(){
		turnPlayer = player1;
		while(!winner){
			if(turnPhase == TurnPhase.Draw){
				if(turnPlayer == player1){
					turnPlayerText.text = "Player 1 Turn";
					turnPlayerText.color = Color.blue;
				}else{
					turnPlayerText.text = "Player 2 Turn";
					turnPlayerText.color = Color.red;
				}
				yield return new WaitForSeconds(1f);
				gameActions.StartCoroutine(gameActions.Draw(turnPlayer, 1));
				turnPlayer.ResetMana();
				UntapAllCards(turnPlayer);

				yield return new WaitForSeconds(1);
				turnPhase = TurnPhase.Mana;
			}
			
			if(turnPhase == TurnPhase.Mana){
				if(turnPlayer == player2){
					yield return new WaitForSeconds(1f);
					aIDuelest.TriggerManaPhase();
					turnPhase = TurnPhase.Main;
				}else{
					//players turn
				}
				yield return new WaitForEndOfFrame();
			}
				
			if(turnPhase == TurnPhase.Main){

				while(duelState != DuelState.None){
					yield return new WaitForEndOfFrame();
				}

				if(turnPlayer == player1){
					if(CheckForMainPhaseAutoEnd(player1)){
						SkipPhase();
					}//else just keep on rollin
				}else{
					aIDuelest.StartCoroutine(aIDuelest.TriggerCast());
					while(turnPhase == TurnPhase.Main){
						yield return new WaitForEndOfFrame();
					}
				}
			}
			
			while(turnPhase == TurnPhase.Battle){
				while(duelState != DuelState.None){
					yield return new WaitForEndOfFrame();
				}

				if(turnPlayer == player1){
					if(CheckForBattlePhaseAutoEnd(player1)){
						SkipPhase();
					}
				}else{
					aIDuelest.StartCoroutine(aIDuelest.TriggerBattle());
					while(turnPhase == TurnPhase.Battle){
						// I make a loop here to pause to ensure that this coroutine will not
						//trigger twice
						yield return new WaitForEndOfFrame();
					}
				}
				yield return new WaitForEndOfFrame();
			}

			if(turnPhase == TurnPhase.End){
				while(duelState != DuelState.None){
					yield return new WaitForEndOfFrame();
				}
				StartCoroutine(TriggerEndOfTurnEffects(turnPlayer));
				TakeAwayAttackDifferentials(turnPlayer);
				
				if(turnPlayer == player1)
					turnPlayer = player2;
				else if(turnPlayer == player2)
					turnPlayer = player1;

				TakeAwaySummoningSickness(turnPlayer);

				turnPhase = TurnPhase.Draw;
			}
			yield return new WaitForEndOfFrame();
		}
		yield return null;
	} 

	public IEnumerator Win(Player winner){
		winnerText.gameObject.SetActive(true);
		winnerText.text = winner.gameObject.name + " Wins!";
		//GameManager.instance.activePlayer1Deck = null;
		yield return new WaitForSeconds(3);
		SceneManager.LoadScene("Main Menu");
		yield return null;
	}

	public void SkipPhase(){
		if(duelState == DuelState.None){
			switch(turnPhase){
				case TurnPhase.Draw:
					turnPhase = TurnPhase.Mana;
					break;
				case TurnPhase.Mana:
					turnPhase = TurnPhase.Main;
					break;
				case TurnPhase.Main:
					turnPhase = TurnPhase.Battle;
					break;
				case TurnPhase.Battle:
					turnPhase = TurnPhase.End;
					break;
				case TurnPhase.End:
					turnPhase = TurnPhase.Draw;
					break;
			}
		}
	}

	private bool CheckForMainPhaseAutoEnd(Player owner){
		int cardsPlayerCanCast = 0;
		foreach(GameObject card in owner.Hand){
			if(gameActions.CostIsMet(owner,card)){
				cardsPlayerCanCast++;
			}
		}
		if(cardsPlayerCanCast > 0){
			return false;
		}
		return true;
	}

	private bool CheckForBattlePhaseAutoEnd(Player owner){
		int cardThatCanAttack = 0;
		foreach(GameObject card in owner.BattleZone){
			CardBase cardInfo = card.GetComponent<CardBase>();
			if(!cardInfo.isTapped && !cardInfo.hasSummoningSickness){
				if(!cardInfo.keyWord.Contains(CardBase.KeyWordEffects.CanNotAttack)){
					if(cardInfo.keyWord.Contains(CardBase.KeyWordEffects.Skirmisher)){
						bool hasTarget = false;
						foreach(GameObject creature in player2.BattleZone){
							CardBase creatureInfo = creature.GetComponent<CardBase>();
							if(creatureInfo.isTapped){
								hasTarget = true;
							}
						}	
						if(hasTarget)
							cardThatCanAttack++;
					}else
						cardThatCanAttack++;
				}
			}
		}
		if(cardThatCanAttack > 0){
			return false;
		}
		return true;
	}

	private void setHandPosition( Player owner){
		//player
		for(int i = 0; i < owner.Hand.Count; i++){
			if(DuelManager.instance.activeCard !=  owner.Hand[i] && owner.Hand.Count > 0){
				Vector3 tp = owner.handPos.position;
				float posMultiplier = 2;
				if(owner.Hand.Count>=6){
					posMultiplier = 1.5f;
				}

				if(i == 0){
					owner.Hand[i].transform.position = Vector3.Lerp(owner.Hand[i].transform.position, tp, .2f);
				}else{
					tp.x = owner.handPos.position.x + (posMultiplier*i);
					owner.Hand[i].transform.position = Vector3.Lerp(owner.Hand[i].transform.position, tp, .2f);
				}


				//set the rotation
				owner.Hand[i].transform.rotation = Quaternion.Lerp(owner.Hand[i].transform.rotation,
				owner.handPos.rotation, 0.2f);
			}
		}
	}

	private void setBattleZonePostion(Player owner){
		for(int i = 0; i < owner.BattleZone.Count; i++){
			if(DuelManager.instance.activeCard !=  owner.BattleZone[i] && owner.BattleZone.Count > 0){
				//set the position
				Vector3 tp = owner.battleZonePos.position;
				float posMultiplier = 2.5f;

				tp.x = owner.handPos.position.x + (posMultiplier*i);
				owner.BattleZone[i].transform.position = Vector3.Lerp(owner.BattleZone[i].transform.position, tp, .2f);

				//set the rotation
				if(owner.BattleZone[i].GetComponent<CardBase>().isTapped){
					owner.BattleZone[i].transform.rotation = Quaternion.Lerp(owner.BattleZone[i].transform.rotation,
					owner.tappedRotation.rotation, 0.5f);
					
				}else{
					owner.BattleZone[i].transform.rotation = Quaternion.Lerp(owner.BattleZone[i].transform.rotation,
					owner.battleZonePos.rotation, 0.2f);
				}
			}
		}
	}

	private void setShieldZonePosition(Player owner){
		if(!activeCard && !attackingCard){
			for(int i = 0; i < owner.Sheilds.Count; i++){
				Vector3 tp = owner.shieldZonePos[i].position;
				owner.Sheilds[i].transform.position = Vector3.Lerp(owner.Sheilds[i].transform.position, tp, .2f);
				owner.Sheilds[i].transform.rotation = Quaternion.Lerp(owner.Sheilds[i].transform.rotation,
				owner.shieldZonePos[i].rotation, 0.2f);
			}
		}
	}

	private void setManaZonePosition(Player owner){
		//player
		for(int i = 0; i < owner.ManaZone.Count; i++){
			Vector3 tp = owner.manaZonePos.position;
			float posMultiplier = 2.5f;
			
			if(i == 0){
				owner.ManaZone[i].transform.position = Vector3.Lerp(owner.ManaZone[i].transform.position, tp, .2f);
			}else{
				tp.x = owner.manaZonePos.position.x + (posMultiplier * i);
				owner.ManaZone[i].transform.position = Vector3.Lerp(owner.ManaZone[i].transform.position, tp, 2f);
			}

			CardBase cardInfo = owner.ManaZone[i].GetComponent<CardBase>();
			if(cardInfo.isTapped){
				owner.ManaZone[i].transform.rotation = Quaternion.Lerp(owner.ManaZone[i].transform.rotation, owner.tappedRotation.rotation, 0.5f);
			}else{
				owner.ManaZone[i].transform.rotation = Quaternion.Lerp(owner.ManaZone[i].transform.rotation, owner.manaZonePos.rotation, .3f);
			}
		}
	}

	private void setGraveYardPosition(){
		//player
		for(int i = 0; i < player1.GraveYard.Count; i++){
			player1.GraveYard[i].transform.position = Vector3.Lerp(player1.GraveYard[i].transform.position,
			player1.graveYardPos.position, 0.2f);

			player1.GraveYard[i].transform.rotation = Quaternion.Lerp(player1.GraveYard[i].transform.rotation,
			player1.graveYardPos.rotation, 0.2f);

			//turn off the colliders in the graveyard to keep cards from flying everwhere
			player1.GraveYard[i].GetComponent<BoxCollider>().enabled = false;
		}
		//computer

		for(int j = 0; j < player2.GraveYard.Count; j++){
			player2.GraveYard[j].transform.position = Vector3.Lerp(player2.GraveYard[j].transform.position,
			player2.graveYardPos.position, 0.2f);

			player2.GraveYard[j].transform.rotation = Quaternion.Lerp(player2.GraveYard[j].transform.rotation,
			player2.graveYardPos.rotation, 0.2f);

			//turn off the colliders in the graveyard to keep cards from flying everwhere
			player2.GraveYard[j].GetComponent<BoxCollider>().enabled = false;
		}
	}

	private void UntapAllCards(Player owner){
		for(int i = 0; i <  owner.BattleZone.Count; i++){
			owner.BattleZone[i].GetComponent<CardBase>().isTapped = false;
		}
	}

	private void TakeAwaySummoningSickness(Player owner){
		for(int i = 0; i < owner.BattleZone.Count; i++){
			CardBase cardInfo = owner.BattleZone[i].GetComponent<CardBase>();
			cardInfo.hasSummoningSickness = false;
		}
	}

	private void TakeAwayAttackDifferentials(Player owner){
		for(int i = 0; i < owner.BattleZone.Count; i++){
			CardBase cardInfo = owner.BattleZone[i].GetComponent<CardBase>();
			cardInfo.power = cardInfo.originalPower;
		}
	}

	private IEnumerator TriggerEndOfTurnEffects(Player owner){
		for(int i = 0; i < owner.BattleZone.Count; i++){
			CardBase card = owner.BattleZone[i].GetComponent<CardBase>();
			if(card.effectType.Contains(Effects.EffectType.EndOfTurn)){
				EffectListener.instance.effects.EndOfTurn(owner, card);
				yield return new WaitForSeconds(.5f);
			}
		}
		yield return null;
	}
}
