using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightAura : MonoBehaviour
{

	[SerializeField] private float smoothTime;
	[SerializeField] private float distanceThreshHold;
	[SerializeField] private float[] lightLevel; // 4 different level of light strength
	[SerializeField] private GameObject Dark;
	
	
	private float[] tempVelocities;
	private GameObject[] lightsAuras;
	private Dictionary<GameObject, bool> bind;
	private bool active;
	public static LightAura _instance;
	// Use this for initialization
	void Start ()
	{
		_instance = this;
		// get all four players light Aura
		lightsAuras = GameObject.FindGameObjectsWithTag("LightAura");
		bind = new Dictionary<GameObject, bool>{};
		foreach (var lightsAura in lightsAuras)
		{
			lightsAura.transform.localScale = new Vector3(lightLevel[0],lightLevel[0],lightLevel[0]);
			bind.Add(lightsAura,false);
		}
		tempVelocities = new float[lightsAuras.Length];
	}
	
	// Update is called once per frame
	void FixedUpdate()
	{
		if (active)
		{
			int counter = 0;
			foreach (var lightsAura in lightsAuras)
			{
				var level = getLightLevel(lightsAura);
				var temp = Mathf.SmoothDamp(lightsAura.transform.localScale.x, lightLevel[level],
					ref tempVelocities[counter], smoothTime);
				lightsAura.transform.localScale = new Vector3(temp, temp, temp);
				bind[lightsAura] = false;
				counter++;
			}
		}
	}
	//public funciton

	public void Enable()
	{
		active = true;
		Dark.SetActive(true);
	}
	
	public void Disable()
	{
		active = false;
		Dark.SetActive(false);
	}

	// helper function
	private int getLightLevel(GameObject center)
	{
		int counter = 0;
		foreach (var lightAura in lightsAuras)
		{
			if (lightAura != center)
			{
				// check the distance
				if ((lightAura.transform.position - center.transform.position).sqrMagnitude < distanceThreshHold)
				{
					counter = counter + 1;
				}
			}
		}
		return counter;
	}
	
	

}
