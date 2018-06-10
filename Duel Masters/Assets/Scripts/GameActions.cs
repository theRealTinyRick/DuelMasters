using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameActions : MonoBehaviour {

	private DuelManager duelManager;

	private Transform effectResolveLocation;
	private enum ActionType{AddToMana, Draw, Cast, DeclareAttack};

	[SerializeField] private GameObject detailParent;
	[SerializeField] private Image cardImage;
	[SerializeField] private TextMeshProUGUI cardDescription;

	[SerializeField] private GameObject battleInfo;
	[SerializeField] private Image battleCard1;
	[SerializeField] private Image battleCard2;

	[SerializeField] GameObject revealedCardPanel;
	[SerializeField] GameObject revealedCardsButton;
	[SerializeField] GameObject revealedCardsParent;

	public Effects effects;

	private void Start(){
		effects = GetComponent<Effects>();
		duelManager = DuelManager.instance;
		effectResolveLocation = duelManager.effectResolveLocation;
	}

	private void Update(){
		InputHandler();
		RunPassiveAbilities(duelManager.player1);
		RunPassiveAbilities(duelManager.player2);
	}

	private void InputHandler(){
		if(Input.GetMouseButtonDown(0)){
			MouseActivate();
		}

		if(Input.GetMouseButtonDown(1)){
			CancelCurrentAction();
		}

		ShowCardDetails();
	}

	public static List <GameObject> ShuffleDeck(List <GameObject> deck){
		//use this function to set a deck to equal a shuffled version of itself
		List<GameObject> shuffledDeck = new List<GameObject>();
		while(deck.Count > 0){
			int randomIndex = Random.Range(0, deck.Count);
			shuffledDeck.Add(deck[randomIndex]);
			deck.Remove(deck[randomIndex]);
		}
		return shuffledDeck;
	}

	public IEnumerator Draw(Player owner, int numberToDraw=1){
		for(int i =0; i<numberToDraw;i++){
			if(owner.Deck.Count > 0 && owner.Hand.Count <= 9){
				int randomIndex = Random.Range(0, owner.Deck.Count());
				GameObject newCard = Instantiate(owner.Deck[randomIndex],owner.deckPos.transform.position,
				owner.deckPos.rotation);
				owner.Hand.Add(newCard);
				owner.Deck.Remove(owner.Deck[randomIndex]);  
				newCard.GetComponent<CardBase>().currentLocation = DuelManager.CardLocations.Hand;
				newCard.GetComponent<CardBase>().owner = owner;
				yield return new WaitForSeconds(0.5f);
			}
		}
		yield return null;
	}

	public IEnumerator AddShields(Player owner, int numberToAdd){
		for(int i = 0; i < numberToAdd; i++){
			int randomIndex = Random.Range(0, owner.Deck.Count());
			GameObject newCard = Instantiate(owner.Deck[randomIndex], owner.deckPos.position, owner.deckPos.rotation);
			newCard.GetComponent<CardBase>().owner = owner;
			owner.Sheilds.Add(newCard);
			owner.Deck.Remove(owner.Deck[randomIndex]);
			newCard.GetComponent<CardBase>().owner = owner;
			newCard.GetComponent<CardBase>().currentLocation = DuelManager.CardLocations.ShieldZone;
			yield return new WaitForSeconds(0.5f);
		}
		yield return new WaitForEndOfFrame();
	}

	public IEnumerator Cast(Player cardOwner, GameObject card){
		duelManager.duelState = DuelManager.DuelState.Casting;
		CardBase cardInfo = card.GetComponent<CardBase>();
		if(cardOwner.BattleZone.Count < 10){
			//evolution creatures
			if(cardInfo.cardType == CardBase.CardType.EvolutionCreature){
				//summon evolution creature
				duelManager.duelState = DuelManager.DuelState.EvolutionTargeting;

				//move the card to the resolution space to show the player the card upclose
				while(Vector3.Distance(card.transform.position, effectResolveLocation.transform.position) > 0.25){
					card.transform.position = Vector3.Lerp(card.transform.position, effectResolveLocation.position, .2f);
					card.transform.rotation = Quaternion.Lerp(card.transform.rotation, effectResolveLocation.rotation, .2f);
					yield return new WaitForEndOfFrame();
				}
				
				while(!duelManager.evoTarget){
					//wait for an evolution target
					yield return new WaitForEndOfFrame();
				}

				int indexOfTarget = cardOwner.BattleZone.IndexOf(duelManager.evoTarget);
				cardOwner.BattleZone.Remove(duelManager.evoTarget);
				cardOwner.BattleZone.Insert(indexOfTarget, card);
				cardOwner.Hand.Remove(card);
				cardInfo.evolvedCard.Add(duelManager.evoTarget);
				duelManager.evoTarget.SetActive(false);

				cardInfo.currentLocation = DuelManager.CardLocations.BattleZone;
				cardInfo.owner.UseMana(cardInfo.manaCost);
				StartCoroutine(SummonEffect(cardInfo)); //start the battlecry effect

			}else if(cardInfo.cardType == CardBase.CardType.Creature){
				//------------------------------------------------------------summon the normal creature
				cardOwner.BattleZone.Add(card);

				switch(cardInfo.currentLocation){
					case DuelManager.CardLocations.Hand:
						cardOwner.Hand.Remove(card);
						break;
					case DuelManager.CardLocations.ManaZone:
						cardOwner.ManaZone.Remove(card);
						break;					
					case DuelManager.CardLocations.Graveyard:
						cardOwner.GraveYard.Remove(card);
						break;
				}
				cardInfo.currentLocation = DuelManager.CardLocations.BattleZone;

				cardInfo.owner.UseMana(cardInfo.manaCost);

				StartCoroutine(SummonEffect(cardInfo)); //start the battlecry effect
			}


			else if(cardInfo.cardType == CardBase.CardType.Spell){ //---------------------SPELL
				//move the spell to the resolve location
				duelManager.activeCard = card;

				while(Vector3.Distance(card.transform.position, effectResolveLocation.position) > 0.55f){
					card.transform.position = Vector3.Lerp(card.transform.position, effectResolveLocation.position, .2f);
					card.transform.rotation = Quaternion.Lerp(card.transform.rotation, effectResolveLocation.rotation, .2f);
					yield return new WaitForEndOfFrame();
				}

				//determine wether or not the spell needs you to target
				if(cardInfo.needsTarget){
					duelManager.duelState = DuelManager.DuelState.Targeting;
					duelManager.actionPrompt.text = "Select A Target";
					for(int i = 0; i < cardInfo.numberOfTargets; i++){
						while(!duelManager.targetCard){//wait for a target to be selected
							//the action of targeting will happen in MouseActivate()
							yield return new WaitForEndOfFrame();
						}
						//start spell eff here ------------------If the owner of the spell is the com it will target before cast
						if(cardOwner == duelManager.player2){
							effects.SpellEffects(card, cardOwner, duelManager.targetCard);
							yield return new WaitForSeconds(1f);
						}
						duelManager.targetCard = null;
					}
				}

				//the spell needs no target and will resolve the effect
				else{
					yield return new WaitForSeconds(1.0f);
					effects.SpellEffects(card, cardOwner);
				}

				while(duelManager.duelState == DuelManager.DuelState.Searching){
					yield return new WaitForEndOfFrame();
				}
				Resolve(true, card, Effects.EffectType.OnSpellCast);
			}
		}
	}

	private IEnumerator SummonEffect(CardBase cardInfo){
		if(cardInfo.effectType.Contains(Effects.EffectType.OnSummon)){
			if(cardInfo.needsTarget){
				duelManager.duelState = DuelManager.DuelState.Targeting;
				duelManager.activeCard = cardInfo.gameObject;

				for(int i = 0; i < cardInfo.numberOfTargets; i++){
					while(!duelManager.targetCard){
						//look in mouse activate
						yield return new WaitForEndOfFrame();
					}
					duelManager.targetCard = null;
				}
				
				duelManager.activeCard = null;
				
			}else{
				effects.OnSummon(cardInfo.owner, cardInfo.gameObject);
			}
		}
		Resolve(true, cardInfo.gameObject, Effects.EffectType.AnotherSummon);
		yield return null;
	}

	private void Resolve(bool resolved, GameObject card, Effects.EffectType trigger = Effects.EffectType.None){
		CardBase cardInfo = card.GetComponent<CardBase>();
		
		if(resolved){
			if(cardInfo.cardType == CardBase.CardType.Spell){
				cardInfo.owner.GraveYard.Add(cardInfo.gameObject); 
				switch(cardInfo.currentLocation){
						case DuelManager.CardLocations.Hand:
							cardInfo.owner.Hand.Remove(cardInfo.gameObject);
							break;
						default:
							cardInfo.owner.Hand.Remove(cardInfo.gameObject);
							break;
				}
				if(duelManager.turnPlayer == cardInfo.owner)
					cardInfo.owner.UseMana(cardInfo.manaCost);
			}
		}else if(!resolved){
			if(cardInfo.cardType == CardBase.CardType.Creature || cardInfo.cardType == CardBase.CardType.EvolutionCreature){
				if(cardInfo.keyWord.Contains(CardBase.KeyWordEffects.MustResolveEffect)){
					Destroy(cardInfo.owner, cardInfo.gameObject);
				}
			}
		}

		cardInfo.GetComponent<BoxCollider>().enabled = true;
		duelManager.activeCard = null;
		duelManager.targetCard = null;
		duelManager.evoTarget = null;

		//This is where I will trigger effects for casting or summoning
		if(EffectListener.instance.DispatchEventToAllActiveCards(trigger, cardInfo, cardInfo.owner)){
			duelManager.duelState = DuelManager.DuelState.None;
		}
	}

	public IEnumerator AddToMana(Player owner, GameObject card){
		CardBase cardInfo = card.GetComponent<CardBase>();
		owner.ManaZone.Add(card);
		switch(cardInfo.currentLocation){
			case DuelManager.CardLocations.Deck:
				owner.Deck.Remove(card);
				break;
			case DuelManager.CardLocations.Hand:
				owner.Hand.Remove(card);
				break;
			case DuelManager.CardLocations.BattleZone:
				owner.BattleZone.Remove(card);
				break;
			case DuelManager.CardLocations.ShieldZone:
				owner.Sheilds.Remove(card);
				break;
			case DuelManager.CardLocations.Graveyard:
				owner.GraveYard.Remove(card);
				break;
		}
		cardInfo.currentLocation = DuelManager.CardLocations.ManaZone;

		if(duelManager.turnPhase == DuelManager.TurnPhase.Mana){
			owner.ResetMana();
			duelManager.SkipPhase();
		}

		owner.UpdateManaCiv();
		duelManager.activeCard = null;
		yield return new WaitForEndOfFrame();
	}

	public IEnumerator Attack(GameObject attackingCard, GameObject target){
		duelManager.duelState = DuelManager.DuelState.Attacking;
		duelManager.attackingCard = attackingCard;
		duelManager.activeCard = attackingCard;
		CardBase attackingInfo = attackingCard.GetComponent<CardBase>();
		CardBase targetInfo = null;
		Player targetPlayer = null; //this is for winning the game

		if(attackingInfo.effectType.Contains(Effects.EffectType.OnAttack)){
			effects.OnAttack(attackingInfo.owner, attackingCard);
		}

		while(duelManager.duelState != DuelManager.DuelState.Attacking){
			yield return new WaitForEndOfFrame();
		}

		if(attackingInfo.keyWord.Contains(CardBase.KeyWordEffects.PowerAttacker)){
			effects.StartCoroutine(effects.PowerAttacker(attackingCard));
		}

		if(duelManager.player1.gameObject== target ){
			//tadomeda
			targetPlayer = duelManager.player1;
		}else if(duelManager.player2.gameObject == target){
			//tadomeda
			targetPlayer = duelManager.player2;
		}else{
			targetInfo = target.GetComponent<CardBase>();
			battleInfo.SetActive(true);
			battleCard1.sprite = attackingCard.GetComponentInChildren<Image>().sprite;
			battleCard2.sprite = target.GetComponentInChildren<Image>().sprite;
		}

		//check for a blocker
		duelManager.duelState = DuelManager.DuelState.ChoosingBlocker;
		
		if(duelManager.turnPlayer == duelManager.player1){
			yield return new WaitForSeconds(1);
			duelManager.aIDuelest.TriggerBlock(attackingInfo);
		}

		while(!duelManager.activeBlocker && duelManager.duelState == DuelManager.DuelState.ChoosingBlocker){
			//wait for a blocker to be chosen
			int blockers = 0; 
			foreach(GameObject card in duelManager.player1.BattleZone){
				CardBase cardInfo = card.GetComponent<CardBase>();
				if(cardInfo.keyWord.Contains(CardBase.KeyWordEffects.Blocker)){
					blockers++;
				}
			}
			if(blockers == 0){
				duelManager.duelState = DuelManager.DuelState.Attacking;
			}
			
			yield return new WaitForEndOfFrame();
		}
		

		if(duelManager.activeBlocker){
			battleCard2.sprite = duelManager.activeBlocker.GetComponentInChildren<Image>().sprite;
			yield return new WaitForSeconds(0.5f);
			duelManager.activeBlocker.GetComponent<CardBase>().isTapped = true;
			Battle(attackingCard, duelManager.activeBlocker);
		}else{
			if(targetPlayer){
				if(targetPlayer == duelManager.player1){
					duelManager.winner = duelManager.player2;
				}else{
					duelManager.winner = duelManager.player1;
				}
				duelManager.StopAllCoroutines();
				duelManager.StartCoroutine(duelManager.Win(duelManager.winner));
				effects.StopAllCoroutines();
				StopAllCoroutines();
			}
			else if(targetInfo.currentLocation == DuelManager.CardLocations.BattleZone)
				Battle(attackingCard, target);
			else
				StartCoroutine(BreakShields(attackingInfo.gameObject, targetInfo.owner));
		}
		attackingInfo.isTapped = true;
		//-------------->>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>

		attackingCard.GetComponent<BoxCollider>().enabled = true;

		yield return new WaitForEndOfFrame();
	}

	public void Battle(GameObject attackingCard, GameObject defendingCard){
		CardBase attackingInfo = attackingCard.GetComponent<CardBase>();
		CardBase defendingInfo = defendingCard.GetComponent<CardBase>();
		CardBase winner = null;
		CardBase loser = null;

		if(attackingInfo.power > defendingInfo.power){
			Destroy(defendingInfo.owner, defendingCard);
			if(defendingInfo.keyWord.Contains(CardBase.KeyWordEffects.Slayer)){
				Destroy(attackingInfo.owner, attackingCard);
			}
			winner = attackingInfo;
			loser = defendingInfo;

		}else if(attackingInfo.power < defendingInfo.power){
			Destroy(attackingInfo.owner, attackingCard);
			if(attackingInfo.keyWord.Contains(CardBase.KeyWordEffects.Slayer)){
				Destroy(defendingInfo.owner, defendingCard);
			}
			winner = defendingInfo;
			loser = attackingInfo;

		}else if(attackingInfo.power == defendingInfo.power){
			Destroy(defendingInfo.owner, defendingCard);
			Destroy(attackingInfo.owner, attackingCard);
		}

		attackingInfo.power = attackingInfo.originalPower;

		if(winner){
			if(winner.effectType.Contains(Effects.EffectType.OnBattle)){
				effects.OnBattle(winner, winner, loser);
			}
		}

		if(attackingInfo.effectType.Contains(Effects.EffectType.OnBlocked)){
			effects.OnBlock(attackingInfo, duelManager.activeBlocker.GetComponent<CardBase>());
		}
		duelManager.attackingCard = null;
		duelManager.activeCard = null;
		duelManager.activeBlocker = null;
		battleInfo.SetActive(false);
		duelManager.duelState = DuelManager.DuelState.None;///////////////////////////////
	}

	public IEnumerator BreakShields(GameObject attackingCard, Player shieldOwner){
		
		CardBase cardInfo = attackingCard.GetComponent<CardBase>();
		for(int i = 0; i < cardInfo.numberOfShieldsToBreak; i++){
			if(shieldOwner.Sheilds.Count > 0){
				int randomIndex = Random.Range(0, shieldOwner.Sheilds.Count);
				GameObject shield = shieldOwner.Sheilds[randomIndex];
				shieldOwner.Hand.Add(shield);
				shieldOwner.Sheilds.Remove(shield);
				CardBase shieldInfo = shield.GetComponent<CardBase>();
				shieldInfo.currentLocation = DuelManager.CardLocations.Hand;

				duelManager.attackingCard = null;
				duelManager.activeCard = null;
				duelManager.activeBlocker = null;
				battleInfo.SetActive(false);

				if(shieldInfo.keyWord.Contains(CardBase.KeyWordEffects.SheildTrigger)){
					duelManager.activeCard =  shieldInfo.gameObject;
					if(shieldInfo.owner == duelManager.player2){
						AITargeting targeting = new AITargeting();
						if(targeting.FindTarget(shieldInfo)){
							StartCoroutine(Cast(shieldInfo.owner, shieldInfo.gameObject));
						}else
							StartCoroutine(Cast(shieldInfo.owner, shieldInfo.gameObject));
					}else
						StartCoroutine(Cast(shieldInfo.owner, shieldInfo.gameObject));

					while(duelManager.duelState != DuelManager.DuelState.None){
						yield return new WaitForEndOfFrame();
					}		
				}else{
					duelManager.duelState = DuelManager.DuelState.None;
				}
				cardInfo.isTapped = true;
			}
		}
		yield return null;
	}

	public void Destroy(Player owner, GameObject card){
		CardBase cardInfo = card.GetComponent<CardBase>();
		owner.GraveYard.Add(card);

		switch(cardInfo.currentLocation){
			case DuelManager.CardLocations.Deck:
				owner.Deck.Remove(card);
				break;
			case DuelManager.CardLocations.Hand:
				owner.Hand.Remove(card);
				break;
			case DuelManager.CardLocations.BattleZone:
				owner.BattleZone.Remove(card);
				break;
			case DuelManager.CardLocations.ShieldZone:
				owner.Sheilds.Remove(card);
				break;
			case DuelManager.CardLocations.Graveyard:
				owner.GraveYard.Remove(card);
				break;
		}

		cardInfo.currentLocation = DuelManager.CardLocations.Graveyard;

		//check for onDestroy effect
		if(cardInfo.effectType.Contains(Effects.EffectType.OnDestroy)){
			effects.OnCardDestroy(owner, card);
		}
	}

	public bool CostIsMet(Player owner, GameObject card){
		CardBase cardInfo = card.GetComponent<CardBase>();
		if(cardInfo.manaCost <= owner.availableMana && 
		owner.availableCiv.Contains(cardInfo.civilizations[0])){
			return true;
		}else
			return false;
	}

	private bool CheckEvolutionTarget(GameObject evolutionCard, GameObject possibleTarget){
		CardBase evoInfo = evolutionCard.GetComponent<CardBase>();
		CardBase targetInfo = possibleTarget.GetComponent<CardBase>();
		for(int i = 0; i < targetInfo.race.Count; i++){
			if(targetInfo.race[i] == evoInfo.evolutionTarget[0] && 
			targetInfo.currentLocation == DuelManager.CardLocations.BattleZone && 
			evoInfo.owner == targetInfo.owner){
				return true;
			}
		}
		return false;
	}

	public IEnumerator RevealCards(List <GameObject> cardsToReveal, int targetCount){
		List <GameObject> revealedCards = new List<GameObject>();

		revealedCardPanel.SetActive(true);
		duelManager.searchList = cardsToReveal;
		duelManager.duelState = DuelManager.DuelState.Searching;
		for(int i = 0; i < cardsToReveal.Count; i++){
			CardBase card = cardsToReveal[i].GetComponent<CardBase>();
			GameObject newButton = Instantiate(revealedCardsButton, Vector3.zero, Quaternion.identity);
			newButton.transform.SetParent(revealedCardsParent.transform);
			RevealedCardItem revealedCardItem = newButton.GetComponent<RevealedCardItem>();
			revealedCardItem.card = card.gameObject;
			revealedCards.Add(newButton);
		}

		for(int i = 0; i < targetCount; i ++){
			while(!duelManager.targetCard){ ///wrong
				yield return new WaitForEndOfFrame();
			}
		}

		foreach(GameObject card in revealedCards){
			Destroy(card);
		}

		revealedCards.Clear();

		revealedCardPanel.SetActive(false);
		duelManager.duelState = DuelManager.DuelState.None;
		yield return null;
	}

	public void SelectCard(GameObject card){
		duelManager.targetCard = card;
		duelManager.duelState = DuelManager.DuelState.Resolving;
	}

	private void RunPassiveAbilities(Player owner){
		for(int i = 0; i < owner.BattleZone.Count; i++){
			CardBase cardInfo = owner.BattleZone[i].GetComponent<CardBase>();
			if(cardInfo.effectType.Contains(Effects.EffectType.Passive)){
				effects.PassiveEffects(owner, cardInfo);
			}
		}
	}

	private void MouseActivate(){
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if(Physics.Raycast(ray,out hit, 100)){
			if(hit.transform.tag == "Card"){
				GameObject card = hit.transform.gameObject;
				CardBase cardInfo = card.GetComponent<CardBase>();
				if(duelManager.duelState == DuelManager.DuelState.None){
					// if the duel state is none you can generally make a motion to trigger an action

					if(cardInfo.currentLocation == DuelManager.CardLocations.Hand &&
					cardInfo.owner == duelManager.player1 && duelManager.turnPhase == DuelManager.TurnPhase.Mana){
						duelManager.activeCard = card;
						StartCoroutine(MoveAndTarget(card, ActionType.AddToMana));
					}//Add a card to mana

					else if(cardInfo.currentLocation == DuelManager.CardLocations.Hand &&
					cardInfo.owner == duelManager.player1 && duelManager.turnPhase == DuelManager.TurnPhase.Main){
						if(CostIsMet(cardInfo.owner, hit.transform.gameObject)){
							duelManager.activeCard = card;
							StartCoroutine(MoveAndTarget(card, ActionType.Cast));
						}
					}//cast a spell or summon a creature
					
					else if(cardInfo.currentLocation == DuelManager.CardLocations.BattleZone 
					&& duelManager.turnPhase == DuelManager.TurnPhase.Battle){
						duelManager.activeCard = cardInfo.gameObject;
						StartCoroutine(MoveAndTarget(cardInfo.gameObject, ActionType.DeclareAttack));
					}//declare an attack on your attackphase

				}else if(duelManager.duelState == DuelManager.DuelState.Targeting){
					//if you are in the targeting duelstate you likely got here from a card effect that requires a target to resolve
					//in this case you are trying to select a target for effect resolution

					GameObject activeCard = duelManager.activeCard;
					CardBase activeInfo = activeCard.GetComponent<CardBase>();
					CardBase targetInfo = hit.transform.gameObject.GetComponent<CardBase>();

					if(activeInfo.cardType == CardBase.CardType.Spell){//check to see if the target is good
						//if it is you will immediate activate the spell with the duelmanager.target
						if(effects.SpellEffects(activeCard, activeInfo.owner, targetInfo.gameObject, targetInfo.owner)){
							duelManager.targetCard = cardInfo.gameObject;
						}
					}else{//check to see if the target is good
					//if it is you will immediate activate the spell with the duelmanager.target
						if(effects.CreatureEffects(activeCard, activeInfo.owner, targetInfo.gameObject, targetInfo.owner)){
							duelManager.targetCard = cardInfo.gameObject;
						}
					}	

				}else if(duelManager.duelState == DuelManager.DuelState.EvolutionTargeting){
					//if you attempted to summon an evolution creature you will enter this state where as the clicked card will
					//be checked if it is a proper target
					if(cardInfo.owner == duelManager.activeCard.GetComponent<CardBase>().owner){
						if(CheckEvolutionTarget(duelManager.activeCard, cardInfo.gameObject)){
							Debug.Log("target is good diff problem");
							duelManager.evoTarget = cardInfo.gameObject;
						}
					}
				
				}else if(duelManager.duelState == DuelManager.DuelState.ChoosingBlocker){
					//you have reached this state during an attack and you have clicked a blocker
					CardBase attackingInfo = duelManager.activeCard.GetComponent<CardBase>();
					if(cardInfo.owner != attackingInfo.owner){
						if(cardInfo.keyWord.Contains(CardBase.KeyWordEffects.Blocker) && !cardInfo.isTapped){
							duelManager.activeBlocker = card;
						}
					}
				}
			}
		}
	}

	private IEnumerator MoveAndTarget(GameObject card, ActionType action){
		CardBase cardInfo = card.GetComponent<CardBase>();
		card.GetComponent<BoxCollider>().enabled = false;
		Player owner = cardInfo.owner;
		Vector3 startPos = card.transform.position;

		while(Input.GetMouseButton(0)){
			//this section will simply move the card where you would like to place it
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if(Physics.Raycast(ray,out hit, 100)){
				if(hit.transform.tag == "GameBoard" || hit.transform.tag == "Card" || hit.transform.tag == "Player"){
					Vector3 tp = hit.point;
					tp.y += 0.75f;
					card.transform.position = Vector3.Lerp(card.transform.position, tp, 0.2f);
					card.transform.rotation = Quaternion.Lerp(card.transform.rotation, 
					owner.battleZonePos.rotation, .5f);
				}
			}
			yield return new WaitForFixedUpdate();
		}

		//as soon as the mouse button is release this section will begin
		if(Vector3.Distance(card.transform.position, startPos) > 2){
			//the above will determine if the card should activate based on how far the player has moved it
			if(action == ActionType.Cast){
				StartCoroutine(Cast(cardInfo.owner, card));
			}else if(action == ActionType.AddToMana){
				StartCoroutine(AddToMana(cardInfo.owner, card));
			}else if(action == ActionType.DeclareAttack){
				//this section of the function will envoke if the actionType is declaring an attack
				//It will then move through the following conditions to check if the attack is valid
				Ray ray2 = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit2;
				if(Physics.Raycast(ray2,out hit2, 100)){
					if(hit2.transform.tag == "Card"){

						CardBase targetInfo = hit2.transform.GetComponent<CardBase>();
						//here im checking all possible situations to be able to attack a creature

						if(cardInfo.owner != targetInfo.owner && !cardInfo.keyWord.Contains(CardBase.KeyWordEffects.CanNotAttack)){
							if(targetInfo.currentLocation == DuelManager.CardLocations.BattleZone){
								if(cardInfo.keyWord.Contains(CardBase.KeyWordEffects.CanAttackUntappedCreatures)){
									StartCoroutine(Attack(cardInfo.gameObject, targetInfo.gameObject));
								}else if(targetInfo.canBeAttackedWhileUptapped){
									StartCoroutine(Attack(cardInfo.gameObject, targetInfo.gameObject));
								}else if(targetInfo.isTapped){
									StartCoroutine(Attack(cardInfo.gameObject, targetInfo.gameObject));
								}else{
									duelManager.activeCard = null;
									duelManager.duelState = DuelManager.DuelState.None;
								}
							}else if(targetInfo.currentLocation == DuelManager.CardLocations.ShieldZone){
								if(!cardInfo.keyWord.Contains(CardBase.KeyWordEffects.Skirmisher)){
									StartCoroutine(Attack(cardInfo.gameObject, targetInfo.gameObject));
								}
							}
						}

					}else if(hit2.transform.tag == "Player" && !cardInfo.keyWord.Contains(CardBase.KeyWordEffects.Skirmisher)){
						if(duelManager.player1.shieldObject == hit2.transform.gameObject){
							if(duelManager.player1.Sheilds.Count <= 0)
								StartCoroutine(Attack(cardInfo.gameObject, duelManager.player1.gameObject));
						}else if(duelManager.player2.shieldObject == hit2.transform.gameObject){
							if(duelManager.player2.Sheilds.Count <= 0)
								StartCoroutine(Attack(cardInfo.gameObject, duelManager.player2.gameObject));
						}
					}else{
						duelManager.activeCard = null;
						duelManager.duelState = DuelManager.DuelState.None;
					}
				}	
			}
		}else{
			duelManager.activeCard = null;
			duelManager.duelState = DuelManager.DuelState.None;
		}

		card.GetComponent<BoxCollider>().enabled = true;
		yield return null;
	}

	private void ShowCardDetails(){
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if(Physics.Raycast(ray, out hit, 100)){
			if(hit.transform.tag == "Card"){
				detailParent.SetActive(true);
				Sprite cardDetail = hit.transform.GetComponentInChildren<Image>().sprite;
				CardBase cardInfo = hit.transform.GetComponent<CardBase>();

				if(cardInfo.currentLocation != DuelManager.CardLocations.ShieldZone){
					if(cardInfo.owner == duelManager.player2 && cardInfo.currentLocation == DuelManager.CardLocations.Hand){
						detailParent.SetActive(false);
					}else{
						cardImage.sprite = cardDetail;
						cardDescription.text = ("Name: " + cardInfo._name + "\n" + 
						"Type: " + cardInfo.cardType);

						cardDescription.text += ("\nCost: "+ cardInfo.manaCost);


						if(cardInfo.cardType == CardBase.CardType.Creature || cardInfo.cardType == CardBase.CardType.EvolutionCreature){
							cardDescription.text += ("\nPower:" + cardInfo.power);
						}

						if(cardInfo.numberOfShieldsToBreak == 2){
							cardDescription.text += ("\nDouble Breaker");
						}else if(cardInfo.numberOfShieldsToBreak == 3){
							cardDescription.text += ("\nTriple Breaker");
						}

						if(cardInfo.cardText != ""){
							cardDescription.text += ("\nDescription: " + cardInfo.cardText + "\n");
						}
					}
					
				}else{
					detailParent.SetActive(false);
				}
			}else{
				detailParent.SetActive(false);
			}
		}else{
			detailParent.SetActive(false);
		}
	}

	public void CancelCurrentAction(){
		if(duelManager.duelState == DuelManager.DuelState.Targeting ||
			duelManager.duelState == DuelManager.DuelState.EvolutionTargeting){
			if(duelManager.duelState != DuelManager.DuelState.ChoosingCards){
				if(duelManager.activeCard.GetComponent<CardBase>().owner == duelManager.player1){
					StopAllCoroutines();
					Resolve(false, duelManager.activeCard);
					if(duelManager.activeCard)
						duelManager.activeCard.GetComponent<BoxCollider>().enabled=true;
					duelManager.activeCard = null;
					duelManager.targetCard = null;
					duelManager.duelState = DuelManager.DuelState.None;
					battleInfo.SetActive(false);
				}
			}
		}

		if(duelManager.duelState == DuelManager.DuelState.ChoosingBlocker){
			duelManager.duelState = DuelManager.DuelState.Resolving;
		}
	}
}
