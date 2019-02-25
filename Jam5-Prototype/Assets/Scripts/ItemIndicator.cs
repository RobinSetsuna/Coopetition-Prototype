using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemIndicator : MonoBehaviour
{

	private GameObject Pointer;
	
	// Use this for initialization
	void Start ()
	{
		Pointer = GetComponentInChildren<Animator>().gameObject;
	}

	public void Enable()
	{
		Pointer.SetActive(true);
	}
 

	public void Disable()
	{
		Pointer.SetActive(false);
	}

}
