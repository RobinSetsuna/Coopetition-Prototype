using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    [SerializeField] private bool boosting = false;
    [SerializeField] private bool onChair = false;
    [SerializeField] private bool carrying = false;
    [SerializeField] private float sightRadius = 5.0f;
    [SerializeField] private int playerIndex;
    [SerializeField] private SimpleController myInput;
    [SerializeField] private Rigidbody2D rb;

	// Use this for initialization
	void Start () {
        myInput = new SimpleController();
        rb = myInput.GetRigidbody();        
	}
	
	// Update is called once per frame
	void Update () {
      if(onChair)
        {
            myInput.SetDisabled(true);
        }
	}

   public void SetBoosting(bool boost) { boosting = boost; myInput.SetBoosting(boosting); }
   public void SetIndex(int index) { playerIndex = index; }
   public int GetIndex() { return playerIndex; }
   public void SetRadius(int overlapped)
    {
        sightRadius = 5.0f + ((1 + overlapped) * .25f); //Line of sight radius is increased based on how many other players are within range of this player 
    }
}
