using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Exit : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerEnter2D(Collider2D collision)
    {       
        if (collision.tag == "Player")
        {          
            if(GameManager.Instance.PlayerCarryChair == collision.name)
            {
                LogUtility.PrintLogFormat("Door", "{0} win!", GameManager.Instance.PlayerCarryChair);
                GameManager.Instance.EndCurrentRound();
            }

        }
    }
}
