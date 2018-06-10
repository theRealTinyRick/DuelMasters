using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class AITargeting{

	Effects effects;

	public bool FindTarget(CardBase cardPlayed){
		effects = DuelManager.instance.gameActions.effects;
		switch(cardPlayed._name){
			case "Crimson Hammer":
				if(HasTargetXPowerOrLess(DuelManager.instance.player1, 2000)){
					DuelManager.instance.targetCard = XPowerOrLess(DuelManager.instance.player1, 2000);
					return true;
				}  
				break;
			case "Burning Power":
				if(HasAnyCardsInBattleZoneThatCanAttack(cardPlayed.owner)){
					DuelManager.instance.targetCard = FindTheStrongestCreature(cardPlayed.owner);
					return true;
				}
				break;
			case "Magma Gazer":
				if(HasAnyCardsInBattleZoneThatCanAttack(cardPlayed.owner)){
					DuelManager.instance.targetCard = FindTheStrongestCreature(cardPlayed.owner);
					return true;
				}
				break;
			case "Tornado Flame":
				if(HasTargetXPowerOrLess(DuelManager.instance.player1, 4000)){
					DuelManager.instance.targetCard = XPowerOrLess(DuelManager.instance.player1, 4000);
					return true;
				}
				break;
			case "MoonLightFlash":
				if(HasUntappedCreatures(DuelManager.instance.player1)){
					DuelManager.instance.targetCard = FindStrongestUntappedCreature(DuelManager.instance.player1);
					return true;
				}
				break;
			case "Laser Wing":
				if(HasAnyCardsInBattleZoneThatCanAttack(cardPlayed.owner)){
					DuelManager.instance.targetCard = FindTheStrongestCreature(cardPlayed.owner);
					return true;
				}
				break;
			case "Solar Ray":
				if(HasUntappedCreatures(DuelManager.instance.player1)){
					if(cardPlayed.owner.BattleZone.Count > 0){
						DuelManager.instance.targetCard = FindStrongestUntappedCreature(DuelManager.instance.player1);
						return true;
					}
				}
				break;
			case "Death Smoke":
				if(HasUntappedCreatures(DuelManager.instance.player1)){
					DuelManager.instance.targetCard = FindStrongestUntappedCreature(DuelManager.instance.player1);
					return true;	
				}
				break;
			case "Terror Pit":
				if(DuelManager.instance.player1.BattleZone.Count > 0){
					DuelManager.instance.targetCard = FindTheStrongestCreature(DuelManager.instance.player1);	
					return true;
				}
				break;
			case "Natural Snare":
				if(DuelManager.instance.player1.BattleZone.Count > 0){
					DuelManager.instance.targetCard = FindTheStrongestCreature(DuelManager.instance.player1);	
					return true;
				}
				break;
			case "Pangeas Song":
				if(cardPlayed.owner.BattleZone.Count > 0){
					DuelManager.instance.targetCard = FindTheWeakestCreature(cardPlayed.owner);
					return true;
				}
				break;
			case "Spiral Gate":
				if(DuelManager.instance.player1.BattleZone.Count > 0){
					DuelManager.instance.targetCard = FindTheStrongestCreature(DuelManager.instance.player1);
					return true;
				}
				break;
			case "Teleportation":
				if(DuelManager.instance.player1.BattleZone.Count > 0){
					DuelManager.instance.targetCard = FindTheStrongestCreature(DuelManager.instance.player1);
					return true;
				}
				break;
			case "Virtual Tripwire":
				if(DuelManager.instance.player1.BattleZone.Count > 0){
					DuelManager.instance.targetCard = FindTheStrongestCreature(DuelManager.instance.player1);
					return true;
				}
				break;
			case "Black Feather Shadow of Rage":
				if(cardPlayed.owner.BattleZone.Count > 0){
					DuelManager.instance.targetCard = FindTheWeakestCreature(cardPlayed.owner);
					return true;
				}
				break;
			case "Gigaberos":
				if(cardPlayed.owner.BattleZone.Count > 1){
					DuelManager.instance.targetCard = FindTheWeakestCreature(cardPlayed.owner);
					return true;
				}
				break;
			case "Stinger Worm":
				if(cardPlayed.owner.BattleZone.Count > 0){
					DuelManager.instance.targetCard = FindTheWeakestCreature(cardPlayed.owner);
					return true;
				}
				break;
			case "Meteosaur":
				if(HasTargetXPowerOrLess(DuelManager.instance.player1, 2000)){
					DuelManager.instance.targetCard = XPowerOrLess(DuelManager.instance.player1, 2000);					
					return true;
				}
				break;
			case "Miele Vizier of Lightning":
				if(DuelManager.instance.player1.BattleZone.Count > 0){
					DuelManager.instance.targetCard = FindTheWeakestCreature(cardPlayed.owner);
					return true;
				}
				break;
			case "Poisonous Mushroom":
				if(cardPlayed.owner.Hand.Count > 1){
					DuelManager.instance.targetCard = FindHighestCostCard(DuelManager.instance.player1, DuelManager.instance.player1.Hand);				
					return true;
				}
				break;
			case "Crystal Memory":
				if(cardPlayed.owner.Deck.Count > 5){
					DuelManager.instance.targetCard = FindHighestCostCard(cardPlayed.owner, cardPlayed.owner.Deck);
					return true;
				}
				break;
		}      
		return false;
	}

	private bool HasAnyCardsInBattleZoneThatCanAttack(Player owner){
		if(owner.BattleZone.Count > 0){
			foreach(GameObject card in owner.BattleZone){
				CardBase cardInfo = card.GetComponent<CardBase>();
				if(!cardInfo.isTapped){
					if(!cardInfo.keyWord.Contains(CardBase.KeyWordEffects.CanNotAttack)){
						return true;
					}
				}
			}
		}

		return false;
	}

	private GameObject FindTheStrongestCreature(Player owner){
		GameObject result = null;
		foreach(GameObject card in owner.BattleZone){
			if(!result)
				result = card;
			else{
				CardBase cardInfo = card.GetComponent<CardBase>();
				CardBase resultInfo = result.GetComponent<CardBase>();
				if(cardInfo.power > resultInfo.power){
					result = card;
				}
			}
		}
		return result;
	}

	//finds the target
	private GameObject XPowerOrLess(Player targetPlayer, int maxPower){
		GameObject target = null;
		foreach(GameObject creature in targetPlayer.BattleZone){
			CardBase creatureInfo = creature.GetComponent<CardBase>();
			if(creatureInfo.power <= maxPower){
				target = creature;
			}
		}
		return target;
	}

	//determines if there is one
	private bool HasTargetXPowerOrLess(Player targetPlayer, int maxPower){
		foreach(GameObject creature in targetPlayer.BattleZone){
			CardBase creatureInfo = creature.GetComponent<CardBase>();
			if(creatureInfo.power <= maxPower){
				return true;
			}
		}
		return false;
	}

	private bool HasUntappedCreatures(Player targetPlayer){
		foreach(GameObject card in targetPlayer.BattleZone){
			CardBase cardInfo = card.GetComponent<CardBase>();
			if(!cardInfo.isTapped){
				return true;
			}
		}
		return false;
	}
	
	private GameObject FindStrongestUntappedCreature(Player targetPlayer){
		GameObject target = null;
		foreach(GameObject card in targetPlayer.BattleZone){
			CardBase cardInfo = card.GetComponent<CardBase>();
			if(!cardInfo.isTapped){
				if(!target){
					target = cardInfo.gameObject;
				}else{
					CardBase targetInfo = target.GetComponent<CardBase>();
					if(cardInfo.power > targetInfo.power){
						target = cardInfo.gameObject;
					}
				}
			}
		}
		return target;
	}

	private GameObject FindTheWeakestCreature(Player targetPlayer){
		GameObject target = null;
		foreach(GameObject card in targetPlayer.BattleZone){
			CardBase cardInfo = card.GetComponent<CardBase>();
			if(!target){
				target = card;
			}else{
				if(cardInfo.power < target.GetComponent<CardBase>().power){
					target = card;
				}
			}
		}
		return target;
	}

	private GameObject FindHighestCostCard(Player owner, List <GameObject> where){
		GameObject result = null;
		foreach(GameObject card in where){
			CardBase cardInfo = card.GetComponent<CardBase>();
			if(!result){
				result = card;
			}else{
				if(card.GetComponent<CardBase>().manaCost < cardInfo.manaCost){
					result = card.gameObject;
				}
			}
		}
		return result;
	}
}
