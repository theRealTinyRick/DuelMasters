using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effects : MonoBehaviour {

	GameActions gameActions;
		public enum  EffectType{None, OnSummon, OnSpellCast,OnDestroy, OnAttack, OnBattle, OnBlocked, ShieldBlast, 
		AnotherSummon, AnotherAttack, AnotherDestroy, Passive, EndOfTurn, OnBlock};

	private void Start(){
		gameActions = DuelManager.instance.gameActions;
	}

	public void TriggeredACallBack(Player owner, GameObject callBackCard, GameObject cardThatTriggeredEvent){
		CardBase cardInfo = callBackCard.GetComponent<CardBase>();
		switch(cardInfo._name){
			case "":
				//cast the effect in here
				break;
		}
	}

	public void OnSummon(Player owner, GameObject card){
		CardBase cardInfo = card.GetComponent<CardBase>();
		switch(cardInfo._name){
			case "Aqua Hulcus":
				gameActions.StartCoroutine(gameActions.Draw(owner, 1));
				break;
			case "Artisan Picora":
				SendManaZoneToGraveyard(owner, 1);
				break;
			case "Onslaughter Triceps":
				SendManaZoneToGraveyard(owner, 1);
				break;
			case "Illusionary Merfolk":
				IllusionaryMerfolk(owner);
				break;
			case "King Ripped Hide":
				gameActions.Draw(owner, 2);
				break;
			case "Saucer Head Shark":
				BounceAllCardsToHandsBasedOnPower(DuelManager.instance.player1, 2000);
				BounceAllCardsToHandsBasedOnPower(DuelManager.instance.player2, 2000);
				break;
			case "Rothus the Traveler":
				StartCoroutine(RothusTheTraveler(owner));
				break;
			case "Bronze Arm Tribe":
				AddTopCardToMana(owner);
				break;
			case "Storm Shell":
				StartCoroutine(StormShell(owner));
				break;
			case "Masked Horror Shadow of Scorn":
				OpponentDiscardsARandomCard(owner);
				break;
			case "Vampire Silphy":
				DestroyAllXPowerOrLess(DuelManager.instance.player1, 3000);
				DestroyAllXPowerOrLess(DuelManager.instance.player2, 3000);
				break;
			case "Rayla Truth Enforcer":
				StartCoroutine(RaylaTruthEnforcer(owner));
				break;
			case "Gigargon":
					StartCoroutine(Gigargon(owner));
				break;
			case "Scarlett Skyterror":
				DestoyAllOfYourOpponentsBlockers(owner);
				break;
			default: 
				break;
		}
	}

	public void OnAttack(Player owner, GameObject card){
		DuelManager.instance.duelState = DuelManager.DuelState.Casting;
		CardBase cardInfo = card.GetComponent<CardBase>();
		switch(cardInfo._name){
			case "Bolshack Dragon":
			StartCoroutine(CivBasedPowerAttacker(card, owner, owner.GraveYard, CardBase.Civilizations.Fire));
				break;
			case "Fatal Attacker Horvath":
				StartCoroutine(RaceConditionalPowerAttacker(card, owner, CardBase.Race.Human, owner.BattleZone, 2000));
				break;
			case "Stampeding Longhorn":
				StartCoroutine(CannotBeBlockedByCreaturesWithLessPower(owner, cardInfo));
				break;
		}
		DuelManager.instance.duelState = DuelManager.DuelState.Attacking;
	}

	public void OnBattle(CardBase card, CardBase winner, CardBase loser){
		switch(card._name){
			case "Bloody Squito":
				DestroyThisCreatureWhenItWinsABattle(card);
				break;
			case "Bone Spider":
				DestroyThisCreatureWhenItWinsABattle(card);
				break;		
			case "Dark Clown":
				DestroyThisCreatureWhenItWinsABattle(card);
				break;
		}
	}

	public void OnBlock(CardBase card, CardBase blocker){
		//creeping plague
		if(card.tempSlayer){
			if(!blocker.owner.GraveYard.Contains(blocker.gameObject)){
				gameActions.Destroy(blocker.owner, blocker.gameObject);
			}
		}
	}

	public void OnCardDestroy(Player owner, GameObject card){
		CardBase cardInfo = card.GetComponent<CardBase>();
		switch(cardInfo._name){
			case "Chilias the Oracle":
				OnDestroyPutInX(cardInfo, owner, owner.Hand);
				break;
			case "Coiling Vines":
				OnDestroyPutInX(cardInfo, owner, owner.ManaZone);
				break;
			case "Mighty Shouter":
				OnDestroyPutInX(cardInfo, owner, owner.ManaZone);
				break;
			case "Red EyeScorpion":
				OnDestroyPutInX(cardInfo, owner, owner.ManaZone);
				break;
		}
	}

	public void EndOfTurn(Player owner, CardBase card){
		switch(card._name){
			case "Frei Vizier of Air":
				card.isTapped = false;
				break;
			case "Ruby Grass":
				card.isTapped = false;
				break;
			case "Toel Vizier of Hope":
				UntapAllCreatures(owner);
				break;
			case "Urth Purifying Elemental":
				card.isTapped = false;
				break;
			case "":
				break;
		}
	}

	public void PassiveEffects(Player owner, CardBase card){
		switch(card._name){
			case "Tropico":
				Tropico(card, owner);
				break;
			case "Locant the Oracle":
				LocantTheOracle(owner, card);
				break;
			case "":
				break;
		}
	}

	public bool SpellEffects(GameObject playedCard, Player playedCardOwner, GameObject targetCard = null, Player targetOwner = null){
		CardBase cardInfo1 = playedCard.GetComponent<CardBase>();
		CardBase cardInfo2 = null;
		if(targetCard != null){
			cardInfo2 = targetCard.GetComponent<CardBase>();
		}

		switch(cardInfo1._name){
			case "Tornado Flame":
				if(DestroyXPowerOrLess(targetCard, playedCardOwner, 4000)){
					return true;
				}else{return false;}

			case "Crimson Hammer":
				if(DestroyXPowerOrLess(targetCard, playedCardOwner, 2000)){
					return true;
				}else{return false;}

			case "Brain Serum":
				gameActions.StartCoroutine(gameActions.Draw(playedCardOwner, 2));
				break;

			case "Spiral Gate":
				if(BounceCardToPlayersHand(targetCard, playedCardOwner)){
					return true;
				}else{return false;}

			case "Teleportation":
				if(BounceCardToPlayersHand(targetCard, playedCardOwner)){
						return true;
				}else{return false;}

			case "Virtual Tripwire":
				if(TapTargetCreature(targetCard, playedCardOwner)){
					return true;
				}
				else{return false;}

			case "Burning Power":
				if(cardInfo1.owner == cardInfo2.owner){
					StartCoroutine(TempPowerAttacker(targetCard, playedCardOwner,2000));
					return true;
				}else{return false;}
			case "Chaos Strike":
				if(playedCardOwner != targetOwner){
					StartCoroutine(TargetCanBeAttackedWhileUnTapped(targetCard, playedCardOwner));
					return true;
				}else{return false;}
				
			case "Magma Gazer":
				if(playedCardOwner == targetOwner){
					StartCoroutine(TempPowerAttacker(targetCard, playedCardOwner, 4000));
					StartCoroutine(TempDoubleBreaker(targetCard, playedCardOwner));
					return true;
				}else{return false;}
			case "Holy Awe":
				TapAllCreatures(playedCardOwner);
				break;
			case "Laser Wing":
				if(playedCardOwner == targetOwner){
					StartCoroutine(TargetCannotBeBlockedThisTurn(cardInfo2, playedCardOwner));
					return true;
				}
				break;
			case "Moonlight Flash":
				if(TapTargetCreature(targetCard, playedCardOwner)){
					return true;
				}
				break;
			case "Solar Ray":
				if(TapTargetCreature(targetCard, playedCardOwner)){
					return true;
				}
				break;
			case "Sonic Wing":
				if(playedCardOwner == targetOwner){
					StartCoroutine(TargetCannotBeBlockedThisTurn(cardInfo2, playedCardOwner));
					return true;
				}
				break;
			case "Aura Blast":
				StartCoroutine(AuraBlast(playedCardOwner));
				break;
			case "Natural Snare":
				if(AddOpponetsCreatureToMana(cardInfo2, playedCardOwner)){
					return true;
				}
				break;
			case "Pangeas Song":
				if(AddYourCreatureToMana(playedCardOwner, cardInfo2)){
					return true;
				}
				break;
			case "Ultimate Force":
				AddTopCardToMana(playedCardOwner);
				AddTopCardToMana(playedCardOwner);
				break;
			case "Creeping Plague":
				StartCoroutine(CreepingPlague(playedCardOwner));
				break;
			case "Death Smoke":
				if(DestroyTargetUntappedCreature(cardInfo2, playedCardOwner)){
					return true;
				}
				break;
			case "Ghost Touch":
				OpponentDiscardsARandomCard(playedCardOwner);
				break;
			case "Terror Pit":
				if(DestroyTargetEnemyCreature(cardInfo1.owner, cardInfo2)){
					return true;
				}
				break;
			case "Crystal Memory":
				StartCoroutine(CrystalMemory(playedCardOwner));
				return true;
			case "Dimension Gate":
				StartCoroutine(DimensionGate(playedCardOwner));
				return true;
			
		}

		return false;
	}

	public bool CreatureEffects(GameObject playedCard, Player playedCardOwner, GameObject targetCard = null, Player targetOwner = null){
		CardBase cardInfo1 = playedCard.GetComponent<CardBase>();
		CardBase cardInfo2 = null;
		if(targetCard != null){
			cardInfo2 = targetCard.GetComponent<CardBase>();
		}

		switch(cardInfo1._name){
			case "Meteosaur":
				if(DestroyXPowerOrLess(targetCard, playedCardOwner, 2000)){
					return true;
				}else{return false;}
			
			case "Unicorn Fish":
				if(BounceCardToPlayersHand(targetCard, playedCardOwner)){
					return true;
				}
				break;
			case "Miele Vizier of Lightning":
				if(TapTargetCreature(targetCard, playedCardOwner)){
					return true;
				}
				break;
			case "Poisonous Mushroom":
				if(ChooseACardToPutInManaZone(cardInfo2, playedCardOwner)){
					return true;
				}
				break;
			case "Black Feather Shadow of Rage":
				if(DestroyOneOfYourCreatures(cardInfo2, playedCardOwner)){
					return true;
				}
				break;
			case "Gigaberos":
				if(DestroyOneOfYourCreatures(cardInfo2, playedCardOwner)){
					return true;
				}
				break;
			case "Stinger Worm":
				if(DestroyOneOfYourCreatures(cardInfo2, playedCardOwner)){
					return true;
				}				
				break;
			case "Aqua Sniper":
				if(BounceCardToPlayersHand(targetCard, playedCardOwner)){
					return true;
				}else{return false;}

		}

		return false;
	}

	//------------------------------------------------
	//effects------------>>>>>>>>>>>>>>>>>>
	public void DestoyAllOfYourOpponentsBlockers(Player owner){
		Player target = null;
		if(owner == DuelManager.instance.player1){
			owner = DuelManager.instance.player2;
		}else{
			owner = DuelManager.instance.player1;
		}

		foreach(GameObject card in target.BattleZone){
			CardBase cardInfo = card.GetComponent<CardBase>();
			if(cardInfo.keyWord.Contains(CardBase.KeyWordEffects.Blocker)){
				gameActions.Destroy(cardInfo.owner, card);
			}
		}
	}

	public IEnumerator CrystalMemory(Player owner){

		gameActions.StartCoroutine(gameActions.RevealCards(owner.Deck,1));
		while(!DuelManager.instance.targetCard){
			yield return new WaitForEndOfFrame();
		}

		GameObject newCard = Instantiate(DuelManager.instance.targetCard, 
		owner.deckPos.position, owner.deckPos.rotation);
		CardBase targetInfo = newCard.GetComponent<CardBase>();

		owner.Hand.Add(newCard);
		owner.Deck.Remove(DuelManager.instance.targetCard);
		targetInfo.currentLocation = DuelManager.CardLocations.Hand;
		targetInfo.owner = owner;

		yield return null;

	}

	public IEnumerator DimensionGate(Player owner){
		List <GameObject> cards = new List<GameObject>();

		for(int i = 0;  i < owner.Deck.Count; i++){
			CardBase card = owner.Deck[i].GetComponent<CardBase>();
			if(card.cardType == CardBase.CardType.Creature){
				cards.Add(card.gameObject);
			}
		}

		if(cards.Count > 0){
			gameActions.StartCoroutine(gameActions.RevealCards(cards, 1));

			while(!DuelManager.instance.targetCard){
				yield return new WaitForEndOfFrame();
			}

			GameObject newCard = Instantiate(DuelManager.instance.targetCard, owner.deckPos.position, owner.deckPos.rotation);
			CardBase cardInfo = newCard.GetComponent<CardBase>();
			owner.Hand.Add(newCard);
			owner.Deck.Remove(DuelManager.instance.targetCard);
			cardInfo.currentLocation = DuelManager.CardLocations.Hand;
			cardInfo.owner = owner;

		}else{
			gameActions.CancelCurrentAction();
		}

		yield return null;
	}

	public IEnumerator RaylaTruthEnforcer(Player owner){
		List <GameObject> cards = new List<GameObject>();

		for(int i = 0;  i < owner.Deck.Count; i++){
			CardBase card = owner.Deck[i].GetComponent<CardBase>();
			if(card.cardType == CardBase.CardType.Spell){
				cards.Add(card.gameObject);
			}
		}

		if(cards.Count > 0){
			gameActions.StartCoroutine(gameActions.RevealCards(cards, 1));

			while(!DuelManager.instance.targetCard){
				yield return new WaitForEndOfFrame();
			}

			GameObject newCard = Instantiate(DuelManager.instance.targetCard, owner.deckPos.position, owner.deckPos.rotation);
			CardBase cardInfo = newCard.GetComponent<CardBase>();
			owner.Hand.Add(newCard);
			owner.Deck.Remove(DuelManager.instance.targetCard);
			cardInfo.currentLocation = DuelManager.CardLocations.Hand;
			cardInfo.owner = owner;

		}else{
			gameActions.CancelCurrentAction();
		}
		yield return null;
	}

	private IEnumerator Gigargon(Player owner){
		for(int i = 0; i < 2; i++){
			List <GameObject> cards = new List <GameObject>();
			for(int j = 0; j < owner.GraveYard.Count; j++){
				CardBase card = owner.GraveYard[j].GetComponent<CardBase>();
				if(card.cardType == CardBase.CardType.Creature){
					cards.Add(card.gameObject);
				}
			}

			if(cards.Count > 0){
				gameActions.StartCoroutine(gameActions.RevealCards(cards, 1));

				while(!DuelManager.instance.targetCard){
					yield return new WaitForEndOfFrame();
				}

				GameObject newCard = Instantiate(DuelManager.instance.targetCard, owner.deckPos.position, owner.deckPos.rotation);
				CardBase cardInfo = newCard.GetComponent<CardBase>();
				owner.Hand.Add(newCard);
				owner.Deck.Remove(DuelManager.instance.targetCard);
				cardInfo.currentLocation = DuelManager.CardLocations.Hand;
				cardInfo.owner = owner;

			}else{
				gameActions.CancelCurrentAction();
			}
		}
		yield return null;
	}

	private bool DestroyXPowerOrLess(GameObject targetCard, Player playedCardOwner, int maxPower){
		CardBase cardInfo = targetCard.GetComponent<CardBase>();
		if(cardInfo.owner != playedCardOwner){
			if(cardInfo.power <= maxPower){
				gameActions.Destroy(cardInfo.owner, targetCard);
				return true;
			}
		}
		return false;
	}

	private bool DestroyTargetEnemyCreature(Player owner, CardBase target){
		if(owner != target.owner &&
		target.currentLocation == DuelManager.CardLocations.BattleZone){
			gameActions.Destroy(target.owner, target.gameObject);
			return true;
		}
		return false;
	}

	private bool DestroyTargetUntappedCreature(CardBase target, Player playedCardOwner){
		if(target.owner != playedCardOwner){
			if(target.currentLocation == DuelManager.CardLocations.BattleZone){
				if(!target.isTapped){
					gameActions.Destroy(target.owner, target.gameObject);
					return true;
				}
			}
		}
		return false;
	}

	private bool DestroyOneOfYourCreatures(CardBase target, Player playedCardOwner){
		if(target.owner == playedCardOwner){
			if(target.currentLocation == DuelManager.CardLocations.BattleZone){
				gameActions.Destroy(playedCardOwner, target.gameObject);
				return true;
			}
		}
		return false;
	}

	private void DestroyAllXPowerOrLess(Player owner, int power){
		for(int i = 0; i < owner.BattleZone.Count; i++){
			CardBase card = owner.BattleZone[i].GetComponent<CardBase>();
			if(card.power <= 3000){
				gameActions.Destroy(owner, card.gameObject);
			}
		}
	}

	private void OpponentDiscardsARandomCard(Player owner){
		Player target = null;
		if(owner == DuelManager.instance.player1){
			target = DuelManager.instance.player2;
		}else{
			target = DuelManager.instance.player1;
		}

		GameObject card = target.Hand[Random.Range(0, target.Hand.Count)];
		target.GraveYard.Add(card);
		target.Hand.Remove(card);
		card.GetComponent<CardBase>().currentLocation = DuelManager.CardLocations.Graveyard;
	}

	private IEnumerator CreepingPlague(Player owner){
		List <CardBase> cards = new List <CardBase>();
		for(int i = 0; i < owner.BattleZone.Count; i++){
			CardBase card = owner.BattleZone[i].GetComponent<CardBase>();
			cards.Add(card);
			card.tempSlayer = true;
		}

		while(DuelManager.instance.turnPlayer == owner){
			yield return new WaitForEndOfFrame();
		}

		for(int j = 0; j < cards.Count; j++){
			cards[j].tempSlayer = false;
		}

		yield return null;
	}

	private void DestroyThisCreatureWhenItWinsABattle(CardBase card){
		gameActions.Destroy(card.owner, card.gameObject);
	}

	private bool ChooseACardToPutInManaZone(CardBase card, Player owner){
		if(card.owner == owner &&
		card.currentLocation == DuelManager.CardLocations.Hand){
			owner.ManaZone.Add(card.gameObject);
			owner.Hand.Remove(card.gameObject);
			owner.availableMana++;
			owner.UpdateManaCiv();
			return true;
		}
		return false;
	}

	private IEnumerator RothusTheTraveler(Player rothusOwner){
		DuelManager.instance.duelState = DuelManager.DuelState.ChoosingCards;
		DuelManager.instance.chooseLocation = rothusOwner.BattleZone;
		GameObject aiChosenCard = null;
		
		if(rothusOwner == DuelManager.instance.player2){
			DuelManager.instance.choosingPlayer = DuelManager.instance.player2;

			aiChosenCard = DuelManager.instance.player2.BattleZone[Random.Range(0, DuelManager.instance.player2.BattleZone.Count)];
			DuelManager.instance.chosenCards.Add(aiChosenCard);
			if(DuelManager.instance.player1.BattleZone.Count > 0){
				DuelManager.instance.choosingPlayer = DuelManager.instance.player1;
				while(DuelManager.instance.chosenCards.Count < 2){
					//wait till player gets one chosen
					yield return new WaitForEndOfFrame();
				}
			}
		
		}else if(rothusOwner == DuelManager.instance.player1){
			DuelManager.instance.choosingPlayer = DuelManager.instance.player1;
			while(DuelManager.instance.chosenCards.Count < 1){
					//wait till player gets one chosen
					yield return new WaitForEndOfFrame();
			}
			if(DuelManager.instance.player2.BattleZone.Count > 0){
				aiChosenCard = DuelManager.instance.player2.BattleZone[Random.Range(0, DuelManager.instance.player2.BattleZone.Count)];
			}
			DuelManager.instance.chosenCards.Add(aiChosenCard);
			DuelManager.instance.choosingPlayer = DuelManager.instance.player2;
		}

		DuelManager.instance.duelState = DuelManager.DuelState.Resolving;
		

		for(int i = 0; i < DuelManager.instance.chosenCards.Count; i++){
			if(DuelManager.instance.chosenCards[i]){
				CardBase cardInfo = DuelManager.instance.chosenCards[i].GetComponent<CardBase>();
				gameActions.Destroy(cardInfo.owner, cardInfo.gameObject);
			}
		}

		DuelManager.instance.chooseLocation = null;
		DuelManager.instance.chosenCards.Clear();
		DuelManager.instance.choosingPlayer = null;

		yield return null;
	} 

	private IEnumerator StormShell(Player owner){
		DuelManager.instance.duelState = DuelManager.DuelState.ChoosingCards;
		DuelManager.instance.chooseLocation = owner.BattleZone;
		GameObject chosenCard = null;

		if(owner == DuelManager.instance.player1){
			if(DuelManager.instance.player2.BattleZone.Count > 0){
				chosenCard = DuelManager.instance.player2.BattleZone[Random.Range(0, DuelManager.instance.player2.BattleZone.Count)];
			}else{
				DuelManager.instance.duelState = DuelManager.DuelState.None;
				DuelManager.instance.chooseLocation = null;
				DuelManager.instance.chosenCards.Clear();
				DuelManager.instance.choosingPlayer = null;
				yield break;
			}
		}else if(owner == DuelManager.instance.player2){
			if(DuelManager.instance.player1.BattleZone.Count > 0){
				while(DuelManager.instance.chosenCards.Count < 1){
					yield return new WaitForEndOfFrame();
				}
				chosenCard = DuelManager.instance.chosenCards[0];
			}else{
				DuelManager.instance.duelState = DuelManager.DuelState.None;
				DuelManager.instance.chooseLocation = null;
				DuelManager.instance.chosenCards.Clear();
				DuelManager.instance.choosingPlayer = null;
				yield break;
			}
		}

		CardBase chosenInfo = chosenCard.GetComponent<CardBase>();
		
		gameActions.Destroy(chosenInfo.owner, chosenCard);
		
		DuelManager.instance.duelState = DuelManager.DuelState.None;
		DuelManager.instance.chooseLocation = null;
		DuelManager.instance.chosenCards.Clear();
		DuelManager.instance.choosingPlayer = null;
		yield return null;
	}

	private bool TapTargetCreature(GameObject targetCard, Player playedCardOwner){
		CardBase cardInfo = targetCard.GetComponent<CardBase>();
		if(cardInfo.owner != playedCardOwner && 
		cardInfo.currentLocation == DuelManager.CardLocations.BattleZone){
			cardInfo.isTapped = true;
			return true;
		}
		return false;
	}

	private bool BounceCardToPlayersHand(GameObject targetCard, Player playedCardOwner){
		CardBase targetInfo = targetCard.GetComponent<CardBase>();
		if(targetInfo.owner != playedCardOwner && targetInfo.currentLocation == DuelManager.CardLocations.BattleZone){
			targetInfo.owner.Hand.Add(targetCard);
			targetInfo.owner.BattleZone.Remove(targetCard);
			targetInfo.currentLocation = DuelManager.CardLocations.Hand;
			targetInfo.isTapped = false;
			return true;
		}else{
			return false;
		}
	}

	public IEnumerator PowerAttacker(GameObject card){
		CardBase cardInfo = card.GetComponent<CardBase>();
		cardInfo.power = cardInfo.originalPower + cardInfo.attackingDifferential;
		while(DuelManager.instance.duelState != DuelManager.DuelState.None){

			yield return new WaitForEndOfFrame();
		}
		cardInfo.power = cardInfo.originalPower;
		yield return null;
	}

	private void OnDestroyPutInX(CardBase card, Player owner, List <GameObject> where){
		where.Add(card.gameObject);
		owner.GraveYard.Remove(card.gameObject);
		card.currentLocation = DuelManager.CardLocations.Hand;
		
		if(where == owner.ManaZone){
			owner.availableMana++;
			owner.UpdateManaCiv();
		}
	}

	public void SendManaZoneToGraveyard(Player owner, int repeats){
		for(int i = 0; i < repeats; i++){
			CardBase card = owner.ManaZone[Random.Range(0, owner.ManaZone.Count)].GetComponent<CardBase>();
			owner.GraveYard.Add(card.gameObject);
			owner.ManaZone.Remove(card.gameObject);
			card.currentLocation = DuelManager.CardLocations.Graveyard;
			owner.UpdateManaCiv();
		}
	}

	public IEnumerator CivBasedPowerAttacker(GameObject card, Player owner, List<GameObject> whereDoesItMatter, CardBase.Civilizations civ){
		CardBase cardInfo = card.GetComponent<CardBase>();
		int attackMultiplier = 0;
		for(int i = 0; i < whereDoesItMatter.Count; i++){
			CardBase graveYardInfo = whereDoesItMatter[i].GetComponent<CardBase>();
			if(graveYardInfo.civilizations.Contains(civ)){
				attackMultiplier++;
			}
		}
		cardInfo.power = (cardInfo.originalPower + (attackMultiplier*1000));

		while(DuelManager.instance.duelState != DuelManager.DuelState.None){

			yield return new WaitForEndOfFrame();
		}

		cardInfo.power = cardInfo.originalPower;
		yield return null;
	}

	private IEnumerator RaceConditionalPowerAttacker(GameObject card, Player owner, CardBase.Race race, List <GameObject> where, int attackingDifferential){
		CardBase cardInfo = card.GetComponent<CardBase>();
		for(int i = 0; i < where.Count; i++){
			if(where[i].GetComponent<CardBase>().race.Contains(race)){
				cardInfo.keyWord.Add(CardBase.KeyWordEffects.PowerAttacker);
				cardInfo.attackingDifferential = attackingDifferential;
				break;
			}
		}

		while(DuelManager.instance.turnPlayer == owner){
			yield return new WaitForEndOfFrame();
		}
		cardInfo.keyWord.Remove(CardBase.KeyWordEffects.PowerAttacker);
		yield return null;
	}

	private IEnumerator TempPowerAttacker(GameObject target, Player owner, int powerDif){
		CardBase cardInfo = target.GetComponent<CardBase>();
		if(!cardInfo.keyWord.Contains(CardBase.KeyWordEffects.PowerAttacker)){
			cardInfo.keyWord.Add(CardBase.KeyWordEffects.PowerAttacker);
			cardInfo.attackingDifferential = powerDif + cardInfo.attackingDifferential;
			while(DuelManager.instance.turnPlayer == owner){
				yield return new WaitForEndOfFrame();
			}
			cardInfo.keyWord.RemoveAll(delegate(CardBase.KeyWordEffects kw){
				return kw == CardBase.KeyWordEffects.PowerAttacker;
			});

		}else{
			cardInfo.attackingDifferential = powerDif + cardInfo.attackingDifferential;
			while(DuelManager.instance.turnPlayer == owner){
				yield return new WaitForEndOfFrame();
			}
		}

		cardInfo.attackingDifferential = cardInfo.originalAttackDifferential;
		yield return null;
	}

	private IEnumerator TempDoubleBreaker(GameObject target, Player owner){
		CardBase cardInfo = target.GetComponent<CardBase>();
		int orginalVal = cardInfo.numberOfShieldsToBreak;
		cardInfo.numberOfShieldsToBreak = 2;

		while(DuelManager.instance.turnPlayer == owner){
			yield return new WaitForEndOfFrame();
		}

		cardInfo.numberOfShieldsToBreak = orginalVal;
		yield return null;
	}

	private IEnumerator TargetCanBeAttackedWhileUnTapped(GameObject target, Player playedCardOwner){
		CardBase targetInfo = target.GetComponent<CardBase>();
		targetInfo.canBeAttackedWhileUptapped = true;
		while(DuelManager.instance.turnPlayer == playedCardOwner){
			yield return new WaitForEndOfFrame();
		}
		targetInfo.canBeAttackedWhileUptapped = false;
		yield return null;
	}

	private void IllusionaryMerfolk(Player owner){
		for(int i = 0; i < owner.BattleZone.Count; i++){
			if(owner.BattleZone[i].GetComponent<CardBase>().race.Contains(CardBase.Race.CyberLord)){
				gameActions.Draw(owner, 3); 
				break;
			}
		}
	}

	private void BounceAllCardsToHandsBasedOnPower(Player owner, int power){
		for(int i = 0; i < owner.BattleZone.Count; i++){
			CardBase cardInfo = owner.BattleZone[i].GetComponent<CardBase>();
			if(cardInfo.power <= power){
				owner.Hand.Add(cardInfo.gameObject);
				owner.BattleZone.Remove(cardInfo.gameObject);
				cardInfo.currentLocation = DuelManager.CardLocations.Hand;
			}
		}
	}

	private void Tropico(CardBase card, Player owner){
		int totalCreatures = owner.BattleZone.Count;
		if(totalCreatures >= 3){
			card.keyWord.Add(CardBase.KeyWordEffects.CanNotBeBlocked);
		}else{
			card.keyWord.Add(CardBase.KeyWordEffects.CanNotBeBlocked);
		}
	}

	private void LocantTheOracle(Player owner, CardBase card){
		for(int i = 0; i < owner.BattleZone.Count; i++){
			CardBase cardInfo = owner.BattleZone[i].GetComponent<CardBase>();
			if(cardInfo.race.Contains(CardBase.Race.AngelCommand)){
				card.power = card.originalPower + 2000;
				return;
			}
		}
		card.power = card.originalPower;
	}

	private void UntapAllCreatures(Player owner){
		for(int i = 0; i < owner.BattleZone.Count; i++){
			CardBase card = owner.BattleZone[i].GetComponent<CardBase>();
			card.isTapped = false;
		}
	}

	private void TapAllCreatures(Player owner){
		Player target = null;
		if(owner == DuelManager.instance.player1){
			target = DuelManager.instance.player2;
		}else if(owner == DuelManager.instance.player2){
			target = DuelManager.instance.player1;
		}

		if(target){
			for(int i = 0; i < target.BattleZone.Count; i++){
				CardBase card = target.BattleZone[i].GetComponent<CardBase>();
				card.isTapped = true;
			}
		}
	}

	private IEnumerator TargetCannotBeBlockedThisTurn(CardBase target, Player owner){
		if(!target.keyWord.Contains(CardBase.KeyWordEffects.CanNotBeBlocked)){
			target.keyWord.Add(CardBase.KeyWordEffects.CanNotBeBlocked);

			while(DuelManager.instance.turnPlayer == owner){
				
				yield return new WaitForEndOfFrame();
			}
			target.keyWord.Remove(CardBase.KeyWordEffects.CanNotBeBlocked);
		}

		yield return null;
	}

	private void AddTopCardToMana(Player owner){
			GameObject card = 
			Instantiate(owner.Deck[Random.Range(0, owner.Deck.Count)], owner.deckPos.position, owner.deckPos.rotation) ;
			owner.ManaZone.Add(card);
			owner.Deck.Remove(card);
			owner.availableMana++;
			owner.UpdateManaCiv();
			card.GetComponent<CardBase>().currentLocation = DuelManager.CardLocations.ManaZone;
	}

	private IEnumerator CannotBeBlockedByCreaturesWithLessPower(Player owner, CardBase card){
		Player targetPlayer = null;
		List <CardBase> blockers = new List <CardBase>();
		if(owner == DuelManager.instance.player1){
			targetPlayer = DuelManager.instance.player2; 
		}else if(owner == DuelManager.instance.player2){
			targetPlayer = DuelManager.instance.player1; 
		}

		for(int i = 0; i< targetPlayer.BattleZone.Count; i++){
			CardBase targetInfo = targetPlayer.BattleZone[i].GetComponent<CardBase>();
			if(targetInfo.keyWord.Contains(CardBase.KeyWordEffects.Blocker)){
				if(targetInfo.power < card.power){
					blockers.Add(targetInfo);
				}
			}
		}

		foreach(CardBase blocker in blockers){
			blocker.keyWord.Remove(CardBase.KeyWordEffects.Blocker);
		}

		while(DuelManager.instance.duelState != DuelManager.DuelState.None){
			
			yield return new WaitForEndOfFrame();
		}

		foreach(CardBase blocker in blockers){
			blocker.keyWord.Remove(CardBase.KeyWordEffects.Blocker);
		}

		yield return null;
	}
	
	private IEnumerator AuraBlast(Player owner){
		List<CardBase> cards = new List<CardBase>();
		for(int i = 0; i < owner.BattleZone.Count; i++){
			CardBase card = owner.BattleZone[i].GetComponent<CardBase>();
			StartCoroutine(TempPowerAttacker(card.gameObject, owner, 2000));
		}
		yield return null;
	}

	private bool AddOpponetsCreatureToMana(CardBase target, Player owner){
		if(target.owner != owner){
			if(target.currentLocation == DuelManager.CardLocations.BattleZone){
				target.owner.ManaZone.Add(target.gameObject);
				target.owner.BattleZone.Remove(target.gameObject);
				target.currentLocation = DuelManager.CardLocations.ManaZone;
				target.owner.availableMana++;
				target.owner.UpdateManaCiv();
				return true;
			}
		}
		return false;
	}

	private bool AddYourCreatureToMana(Player owner, CardBase target){
		if(target.owner == owner){
			if(target.currentLocation == DuelManager.CardLocations.BattleZone){
				owner.ManaZone.Add(target.gameObject);
				owner.BattleZone.Remove(target.gameObject);
				target.currentLocation = DuelManager.CardLocations.ManaZone;
				owner.availableMana++;
				owner.UpdateManaCiv();
				return true;
			}
		}

		return false;
	}
}
