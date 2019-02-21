using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleController : MonoBehaviour
{

	private float speed = 2f;
	private Rigidbody2D rb2d;
    private bool disabled = false;
    private bool carrying = false;
    private bool boosting = false;

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
        SetSpeed();
	}

   public Rigidbody2D GetRigidbody()
    {
        return rb2d;
    }

    /// <summary>
    /// Determines whether the controller is active (based on chair)
    /// </summary>
    /// <param name="useable"></param>
    public void SetDisabled(bool useable)
    {
        disabled = useable;
    }
    public void SetCarry(bool carry)
    {
        carrying = carry;
    }
    void SetSpeed()
    {
        if (carrying) { speed = 1.5f; }
        else if (boosting) {speed = 2.5f; }
        else { speed = 2.0f; }
    }

    public void SetBoosting(bool boosting)
    {
        this.boosting = boosting;
    }
}
