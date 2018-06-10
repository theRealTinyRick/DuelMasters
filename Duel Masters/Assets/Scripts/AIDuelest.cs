using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIDuelest : MonoBehaviour {

	public Player player; 
	public DuelManager duelManager;
	public GameActions gameActions;
	
	private int manaCeiling = 0;

	private void Start(){
		player = GetComponent<Player>();
		duelManager = DuelManager.instance;
		gameActions = duelManager.gameActions;
	}

	public void TriggerManaPhase(){
		if(manaCeiling == 0){
			foreach(GameObject card in player.Deck){
				CardBase cardInfo = card.GetComponent<CardBase>();
				if(cardInfo.manaCost > manaCeiling)
					manaCeiling = cardInfo.manaCost;
			}
		}
		if(player.Hand.Count > 1 && player.ManaZone.Count <= manaCeiling){
			CardBase highestCostCard = player.Hand[0].GetComponent<CardBase>();
			for(int i = 0; i < player.Hand.Count; i++){
				CardBase cardBase = player.Hand[i].GetComponent<CardBase>();
				int cardBaseCostAbs = Mathf.Abs(player.availableMana - cardBase.manaCost);
				int highestCostAbs = Mathf.Abs(player.availableMana - highestCostCard.manaCost);
				if(cardBaseCostAbs > highestCostAbs){
					highestCostCard = cardBase;
				}
			}
			gameActions.StartCoroutine(gameActions.AddToMana(player, highestCostCard.gameObject));
		}
	}

	public IEnumerator TriggerCast(){
		yield return new WaitForSeconds(1);
		AITargeting targeting = new AITargeting();
		if(player.Hand.Count > 0){
			for(int i = 0; i < player.Hand.Count; i++){
				CardBase cardInfo = player.Hand[i].GetComponent<CardBase>();
				if(gameActions.CostIsMet(cardInfo.owner, cardInfo.gameObject)){
					if(cardInfo.cardType == CardBase.CardType.Spell){
						if(cardInfo.needsTarget){
							for(int j = 0; j < cardInfo.numberOfTargets; j++){
								if(targeting.FindTarget(cardInfo)){
									gameActions.StartCoroutine(gameActions.Cast(player, cardInfo.gameObject));
									yield return new WaitForSeconds(.5f);
								}
								yield return new WaitForSeconds(2);
							}

						}else{
							gameActions.StartCoroutine(gameActions.Cast(player, cardInfo.gameObject));
						}
					}else{
						//summon creature
						if(cardInfo.effectType.Contains(Effects.EffectType.OnSummon)){
							if(cardInfo.needsTarget){
								for(int j = 0; j < cardInfo.numberOfTargets; j++){
									if(targeting.FindTarget(cardInfo))
										gameActions.StartCoroutine(gameActions.Cast(player, cardInfo.gameObject));
									else if(!cardInfo.keyWord.Contains(CardBase.KeyWordEffects.MustResolveEffect)){
										gameActions.StartCoroutine(gameActions.Cast(player, cardInfo.gameObject));
										
									}
								}
							}else
								gameActions.StartCoroutine(gameActions.Cast(player, cardInfo.gameObject));
						}
						else
							gameActions.StartCoroutine(gameActions.Cast(player, cardInfo.gameObject));
					}
					yield return new WaitForSeconds(1f);
				}
				while(duelManager.duelState != DuelManager.DuelState.None){
					yield return new WaitForEndOfFrame();
				}
			}
		}
		duelManager.turnPhase = DuelManager.TurnPhase.Battle;
		//the AI will finish on its own
		yield return null;
	}

	public IEnumerator TriggerBattle(){
		List <CardBase> cardsThatCanAttack = new List <CardBase>();
		List <CardBase> cardsICanTarget = new List <CardBase>();
		Player player1 = duelManager.player1;

		for(int i = 0; i < player.BattleZone.Count; i++){
			CardBase cardInfo = player.BattleZone[i].GetComponent<CardBase>();
			if(!cardInfo.hasSummoningSickness && !cardInfo.isTapped && cardInfo && cardInfo.currentLocation == DuelManager.CardLocations.BattleZone){
				//attack a target on the oppoents side
				if(duelManager.player1.Sheilds.Count > 0){
					CardBase target = null;
					for(int j = 0; j < player1.BattleZone.Count; j++){
						CardBase possibleTarget = player1.BattleZone[j].GetComponent<CardBase>();
						if(possibleTarget.isTapped && cardInfo.power > possibleTarget.power){
							target = possibleTarget;
						}
						yield return new WaitForEndOfFrame();
					}

					if(target){
						cardInfo.isTapped = true;
						duelManager.duelState = DuelManager.DuelState.Attacking;
						gameActions.StartCoroutine(gameActions.Attack(cardInfo.gameObject, target.gameObject));
						while(duelManager.duelState != DuelManager.DuelState.None && !duelManager.activeBlocker){
							//wait for the block step
							yield return new WaitForEndOfFrame();
						}
						yield return new WaitForSeconds(1);
					}else{
						//break a shield
						cardInfo.isTapped = true;
						duelManager.duelState = DuelManager.DuelState.Attacking;
						gameActions.StartCoroutine(gameActions.Attack(cardInfo.gameObject, player1.Sheilds[0]));
						while(duelManager.duelState != DuelManager.DuelState.None && !duelManager.activeBlocker){
							//wait for the block step
							yield return new WaitForEndOfFrame();
						}
						yield return new WaitForSeconds(1);
						Debug.Log("Waiting for a trigger");
						while(duelManager.duelState != DuelManager.DuelState.None){
							Debug.Log("Waiting for a trigger");
							yield return new WaitForEndOfFrame();
						}
					}
				}
				else{//player1 has no shields
					gameActions.StartCoroutine(gameActions.Attack(cardInfo.gameObject, DuelManager.instance.player1.gameObject));
				}
			}
			while(duelManager.duelState != DuelManager.DuelState.None){
				yield return new WaitForEndOfFrame();
			}
		}
		
		duelManager.turnPhase = DuelManager.TurnPhase.End;
		yield return null;
	}

	public void TriggerBlock(CardBase attackingCard){
		CardBase defendingCardInfo = null;
		foreach (GameObject card in duelManager.player2.BattleZone){
			CardBase cardInfo = card.GetComponent<CardBase>();
			if(cardInfo.keyWord.Contains(CardBase.KeyWordEffects.Blocker)){
				if(cardInfo.power >= attackingCard.power){
					defendingCardInfo = cardInfo;
					break;
				}
			}
		}

		if(defendingCardInfo){
			duelManager.activeBlocker = defendingCardInfo.gameObject;
		}else{
			duelManager.duelState = DuelManager.DuelState.Attacking;
		}
	}

	private void BreakAShield(CardBase attackingCreature){
		StartCoroutine(gameActions.BreakShields(attackingCreature.gameObject, duelManager.player1));
	}

}
