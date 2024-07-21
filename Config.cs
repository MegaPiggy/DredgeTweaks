using Newtonsoft.Json;
using System;
using System.ComponentModel;
using UnityEngine;

namespace Tweaks;

// TODO: change UnityEngine to System.ComponentModel.Annotations
[JsonObject]
public class Config
{
	/// <summary>
	/// Spy glass shows fishing spots
	/// </summary>
	[DefaultValue(true)]
	public bool spyGlassShowsFishingSpots = true;

	/// <summary>
	/// Controller rumble
	/// </summary>
	[DefaultValue(true)]
	public bool controllerRumble = true;

	/// <summary>
	/// Boat can turn only when moving
	/// </summary>
	[DefaultValue(false)]
	public bool boatTurnsOnlyWhenMoving = false;

	/// <summary>
	/// Water ripples on fishing spot
	/// </summary>
	[DefaultValue(true)]
	public bool waterRipplesOnFishingSpot = true;

	/// <summary>
	/// Show point of interest icon when you are close to it
	/// </summary>
	[DefaultValue(true)]
	public bool showPOIicon = true;

	/// <summary>
	/// Show particle effect on special fishing spots
	/// </summary>
	[DefaultValue(true)]
	public bool aberrationParticleFXonFishingSpot = true;

	/// <summary>
	/// Aberration catch chance cap
	/// Your chances to catch aberrations will be capped at this.
	/// </summary>
	[Range(0, 1)]
	[DefaultValue(0.35f)]
	public float aberrationCatchBonusCap = 0.35f;

	/// <summary>
	/// Trawl net catch sound
	/// </summary>
	[DefaultValue(true)]
	public bool netCatchSound = true;

	/// <summary>
	/// Special fishing spots
	/// No fishing spot will give you 100% to catch aberrations if this is false.
	/// </summary>
	[DefaultValue(true)]
	public bool specialFishingSpots = true;

	/// <summary>
	/// Show player marker on map
	/// </summary>
	[DefaultValue(true)]
	public bool showPlayerMarkerOnMap = true;

	/// <summary>
	/// Show orange particles on point of interest
	/// </summary>
	[DefaultValue(true)]
	public bool showOrangeParticlesOnPOI = true;

	/// <summary>
	/// Randomize fish stock at fishing spots
	/// </summary>
	[DefaultValue(false)]
	public bool randomizeFishStock = false;

	/// <summary>
	/// Randomize dredge stock at dredging spots
	/// </summary>
	[DefaultValue(false)]
	public bool randomizeDredgeStock = false;

	/// <summary>
	/// Show point of interest glint particle FX
	/// </summary>
	[DefaultValue(true)]
	public bool showPOIglint = true;

	/// <summary>
	/// Show haste ability overheat gauge
	/// </summary>
	[DefaultValue(true)]
	public bool showBoostGauge = true;

	/// <summary>
	/// Show fishing spot info
	/// </summary>
	[DefaultValue(true)]
	public bool showFishSpotInfo = true;

	/// <summary>
	/// Show fishing minigame animation feedback
	/// </summary>
	[DefaultValue(true)]
	public bool showMinigameAnimationFeedback = true;

	/// <summary>
	/// Show relic beam particle FX
	/// </summary>
	[DefaultValue(true)]
	public bool showRelicParticles = true;

	/// <summary>
	/// Caught fish decays
	/// </summary>
	[DefaultValue(true)]
	public bool fishDecays = true;

	/// <summary>
	/// Show trawl net catch count
	/// </summary>
	[DefaultValue(true)]
	public bool showNetCatchCount = true;

	/// <summary>
	/// Abilities have no cooldown timer
	/// </summary>
	[DefaultValue(false)]
	public bool noAbilityCooldown = false;

	/// <summary>
	/// Camera field of view
	/// </summary>
	[Range(20, 80)]
	[DefaultValue(40)]
	public int cameraFOV = 40;

	/// <summary>
	/// Chance to catch research part when dredging
	/// </summary>
	[Range(0, 1)]
	[DefaultValue(0.07f)]
	public float chanceToCatchResearchPart = 0.07f;

	/// <summary>
	/// Boat movement speed multiplier
	/// </summary>
	[Range(0.5f, 5)]
	[DefaultValue(1)]
	public float boatSpeedMult = 1;

	/// <summary>
	/// Boat turning speed multiplier
	/// </summary>
	[Range(0.5f, 5)]
	[DefaultValue(1)]
	public float boatTurnMult = 1;

	/// <summary>
	/// Chance to catch aberrations during the day
	/// This setting does not affect chances of special fishing spots appearing in the world.
	/// </summary>
	[Range(0, 1)]
	[DefaultValue(0.01f)]
	public float daytimeAberrationChance = 0.01f;

	/// <summary>
	/// Chance to catch aberrations at night
	/// This setting does not affect chances of special fishing spots appearing in the world.
	/// </summary>
	[Range(0, 1)]
	[DefaultValue(0.03f)]
	public float nighttimeAberrationChance = 0.03f;

	/// <summary>
	/// Trawl net catch rate multiplier
	/// </summary>
	[Range(0, 10)]
	[DefaultValue(1)]
	public float netCatchRateMult = 1;

	/// <summary>
	/// Chance to catch materials with trawl net
	/// </summary>
	[Range(0, 1)]
	[DefaultValue(0)]
	public float netCatchMaterialChance = 0;

	/// <summary>
	/// Chance to remove a fishing spot for a day
	/// </summary>
	[Range(0, 1)]
	[DefaultValue(0)]
	public float fishingSpotDisableChance;

	/// <summary>
	/// Haste ability speed multiplier
	/// Your haste speed will be multiplied by this if this value is more then 1
	/// </summary>
	[Range(1, 5)]
	[DefaultValue(1)]
	public float boostSpeedMult = 1;

	/// <summary>
	/// Haste ability heat loss rate multiplier
	/// </summary>
	[Range(0.1f, 10)]
	[DefaultValue(1)]
	public float boostCooldownMult = 1;

	/// <summary>
	/// Day/night length multiplier
	/// The higher the value the faster time passes when you move.
	/// This does not affect anything else.
	/// </summary>
	[Range(0.1f, 10)]
	[DefaultValue(1)]
	public float dayLengthMult = 1;

	/// <summary>
	/// Trawl net weight
	/// Percent of trawl net weight added to boat weight.
	/// </summary>
	[Range(0, 1)]
	[DefaultValue(0)]
	public float netBoatMassMult = 0;

	/// <summary>
	/// Boat cargo weight
	/// This is percent of boat cargo weight added to boat weight.
	/// When you have no free space in inventory your boat's weight will double if you set this to 100%.
	/// </summary>
	[Range(0, 1)]
	[DefaultValue(0)]
	public float cargoBoatMassMult = 0;

	/// <summary>
	/// Bonus chance to catch aberrations at low sanity
	/// Bonus chance to catch aberrations that scales with your sanity.
	/// For example, if you set this to 50% your chance to catch aberrations will increase by 25% when your sanity is at 50% and by 50% when your sanity is at 0%.
	/// </summary>
	[Range(0, 1)]
	[DefaultValue(0)]
	public float sanityAberrationCatchBonus = 0;

	/// <summary>
	/// Sanity change rate multiplier
	/// Your sanity recovery and loss rate will be multiplied by this
	/// </summary>
	[Range(0.1f, 10)]
	public float sanityMultiplier = 1f;

	/// <summary>
	/// Crab pot catch chance
	/// </summary>
	[Range(0, 1)]
	[DefaultValue(1)]
	public float crabPotCatchChance = 1;

	/// <summary>
	/// Trawl net catch chance
	/// </summary>
	[Range(0, 1)]
	[DefaultValue(1)]
	public float netCatchChance = 1;

	/// <summary>
	/// Crab pot durability multiplier
	/// </summary>
	[Range(0.1f, 10)]
	[DefaultValue(1)]
	public float crabPotDurabilityMultiplier = 1;

	/// <summary>
	/// Safe collision speed threshold
	/// You will not take any damage if your boat speed is below this when you collide with something.
	/// </summary>
	[Range(0, 10)]
	[DefaultValue(0)]
	public float safeCollisionMagnitudeThreshold = 0;

	/// <summary>
	/// Crab pot catch interval multiplier
	/// Time needed for crab pot to catch anything will be multiplied by this
	/// </summary>
	[Range(0.01f, 3)]
	[DefaultValue(1)]
	public float crabPotCatchRateMult = 1;

	/// <summary>
	/// Min atrophy aberrations
	/// Min number of aberrations caught by atrophy ability
	/// </summary>
	[Range(0, 10)]
	[DefaultValue(1)]
	public int minAtrophyAberrations = 1;

	/// <summary>
	/// Trawl net gets damaged over time
	/// </summary>
	[DefaultValue(true)]
	public bool netBreaks = true;

	/// <summary>
	/// Fishing spots
	/// </summary>
	[DefaultValue("vanilla")]
	public Spots fishingSpots = Spots.Vanilla;

	/// <summary>
	/// Dredge spots
	/// </summary>
	[DefaultValue("vanilla")]
	public Spots dredgeSpots = Spots.Vanilla;
}
