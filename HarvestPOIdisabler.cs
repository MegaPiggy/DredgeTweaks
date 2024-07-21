using UnityEngine;

namespace Tweaks;

public class HarvestPOIdisabler : MonoBehaviour
{
	public int checkDay = -1;

	public bool enabled_ = true;

	public void OnEnable()
	{
		if (checkDay < GameManager.Instance.Time.Day)
		{
			checkDay = GameManager.Instance.Time.Day;
			enabled_ = Main.Config.fishingSpotDisableChance <= Random.value;
		}
		base.gameObject.SetActive(enabled_);
	}

	public void OnDisable()
	{
	}

	public void OnTriggerEnter(Collider other)
	{
	}

	public void OnTriggerStay(Collider other)
	{
	}
}
