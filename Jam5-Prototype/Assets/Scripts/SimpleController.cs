using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleController : MonoBehaviour
{

	private float speed = 2f;
	private Rigidbody2D rb2d;

	[SerializeField] private int index;
	// Use this for initialization
	void Start ()
	{
		rb2d = GetComponent<Rigidbody2D>();
	}
	
	// Update is called once per frame
	void Update () {
		float h = Input.GetAxis("Horizontal" + index);
		float v = Input.GetAxis("Vertical" + index);
		rb2d.velocity = new Vector2( h * speed,v *speed);
	}
}
