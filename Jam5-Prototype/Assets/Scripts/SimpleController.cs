using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleController : MonoBehaviour
{


    private float speed;
    private bool disabled = false;
    private bool carrying = false;
    private bool boosting = false;
    private Rigidbody2D rb2d;


    // Use this for initialization
    void Start ()
	{
		rb2d = GetComponent<Rigidbody2D>();
	}
	
	// Update is called once per frame
	void Update () {
	
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
