using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardBase : MonoBehaviour {

	public Player owner;

	public enum CardType{Creature, EvolutionCreature, VortextEvolution, Spell};
	public CardType cardType;

	public int manaCost;
	public string _name = "";
	public enum Civilizations{Light, Water, Darkness, Fire, Nature};
	public List <Civilizations> civilizations;
	public int power;
	public int originalPower;
	public int numberOfShieldsToBreak;
	public bool isTapped;
	public bool hasSummoningSickness = true;
	public string cardText = ""; 

	public enum Race{AngelCommand, LiquidPeople, Leviathan, DemonCommmand, ArmoredWyvern, ArmoredDragon, HornedBeast, GiantInsect,
	LightBringer, Guardian, StarlightTree, Initiate, Berserker, CyberVirus, Fish, GelFish, CyberLord, Ghost, BrainJacker, LivingDead,
	Chimera, ParasiteWorm, DarkLord, Armorloid, MachineEater, Human, Dragonoid, RockBeast, BeastFolk, TreeFolk, ColonyBeetle, BaloonMushroom};
	
	public List <Race> race;
	public Race[] evolutionTarget;
	public List <GameObject> evolvedCard = new List<GameObject>();

	public enum KeyWordEffects{Blocker, Slayer, Skirmisher, CanNotAttack, PowerAttacker, CanNotBeBlocked, CanAttackUntappedCreatures, MustResolveEffect, OnSummon, SheildTrigger}
	public List <KeyWordEffects> keyWord = new List <KeyWordEffects>();
	public List <Effects.EffectType> effectType = new List<Effects.EffectType>();

	public bool tempSlayer;
	public int attackingDifferential;
	public int originalAttackDifferential;
	public bool canBeAttackedWhileUptapped = false;

	public bool needsTarget = false;
	public int numberOfTargets;

	public DuelManager.CardLocations currentLocation;

	private void Start(){
		originalPower = power; 
		originalAttackDifferential = attackingDifferential;
	}

	private void GenerateCardText(){
		//build a function that can genrate basic text for a card
	}
}
