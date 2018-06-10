using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectListener : MonoBehaviour {

	public static EffectListener instance;
	public Effects effects;
	private DuelManager duelManager;
	
	private void Awake(){
		if(instance==null)
			instance=this;
		else if(instance!=null)
			Destroy(gameObject);
	}

	private void Start(){
		effects = DuelManager.instance.GetComponent<Effects>();
		duelManager = DuelManager.instance;
	}

	public bool DispatchEventToAllActiveCards(Effects.EffectType eventType, CardBase cardPlayed, Player cardPlayedOwner){
		Player playerToResolveFirst = null;
		Player playerToResolveSecond = null;

		List <CardBase> effectResolutionQueue = new List<CardBase>();

		//turn Player will be checked first
		if(duelManager.turnPlayer == duelManager.player1){
			playerToResolveFirst = duelManager.player1;
			playerToResolveSecond = duelManager.player2;
		}else{
			playerToResolveFirst = duelManager.player2;
			playerToResolveSecond = duelManager.player1;
		}

		//resolve all triggered effects of the turn player
		foreach(GameObject card in playerToResolveFirst.BattleZone){
			CardBase cardInfo = card.GetComponent<CardBase>();
			if(cardInfo.effectType.Contains(eventType)){
				effectResolutionQueue.Add(cardInfo);
			}
		}

		foreach(GameObject card in playerToResolveSecond.BattleZone){
			CardBase cardInfo = card.GetComponent<CardBase>();
			if(cardInfo.effectType.Contains(eventType)){
				effectResolutionQueue.Add(cardInfo);
			}
		}

		if(effectResolutionQueue.Count == 0){
			return true;
		}
		return false;

	}

	IEnumerator EffectResolution(List<CardBase> resolutionQueue){
		foreach(CardBase card in resolutionQueue){
			
			yield return new WaitForSeconds(1);
		}

		yield return null;
	}

}
