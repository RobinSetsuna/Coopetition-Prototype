using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum playerState {
    Moveable = 1,
    Seated,
    Boosting,
    Carrying,
    Default
   }
public class Player : MonoBehaviour {
    [SerializeField] private float speed;
    [SerializeField] private float boostStrength;
    [SerializeField] private float boostFreezeDuration;

    [SerializeField] private bool IsCarrying;
    [SerializeField] private bool IsSeating;
    [SerializeField] private float sightRadius;

    [SerializeField] private int index;
    [SerializeField] private playerState currentState;
    private Rigidbody2D rb2d;
    public playerState CurrentState
    {
        // this allowed to triggger codes when the state switched, directly copied from gamemanager
        get
        {
            return currentState;
        }

        private set
        {
            if (value == currentState)
            {
                LogUtility.PrintLogFormat("Player Reset {0}.", value.ToString());
            }
            else
            {
                LogUtility.PrintLogFormat("PlayerMade a transition to {0}.", value.ToString());

                playerState previousState = currentState;
                currentState = value;
                switch (currentState)
                {
                    case playerState.Moveable:
                        //run when playerState transfered to Moveable

                        break;
                    case playerState.Seated:
                        //run when playerState transfered to seated

                        break;

                    case playerState.Boosting:
                        var temp = rb2d.velocity;
                        // get current move direction
                        var currentDirection = temp.normalized;
                        rb2d.AddForce(currentDirection * boostStrength);
                        // add force to same direction
                        StartCoroutine(releaseToIdle(boostFreezeDuration));
                        break;

                    case playerState.Carrying:
                        // change the speed and duration
                        speed = 1.5f;
                        boostFreezeDuration = boostFreezeDuration * 2;
                        break;
                }
            }
        }
    }



    // Use this for initialization
    void Start () {
        rb2d = GetComponent<Rigidbody2D>();
        IsCarrying = false;
        IsSeating = false;
        currentState = playerState.Moveable;
	}
	
	// Update is called once per frame
	void Update () {
        switch (currentState) {
            case playerState.Moveable:
                float h = Input.GetAxis("Horizontal" + index);
                float v = Input.GetAxis("Vertical" + index);
                rb2d.velocity = new Vector2(h * speed, v * speed);
                if (Input.GetButtonDown("Boost" + index)) {
                    //boosting
                    // TODO may add another code here
                    setPlayerState(playerState.Boosting);
                }
                break;
            case playerState.Boosting:
                // when boosting, freeze all input
                break;

            case playerState.Carrying:
                // I think for now using the moveable for carrying will be fine
                break;
            case playerState.Seated:
                // for now ,do nothing
                break;
        } 
	}
    public void Initialize(Vector3 position,int _index = -1)
    {
        index = _index == -1 ? index : _index;
        currentState = playerState.Moveable;
        transform.position = position;
    }

   public int GetIndex() { return index; }

    public void setPlayerState(playerState targetState) { CurrentState = targetState; }

    public void SetRadius(int overlapped)
    {
        sightRadius = 5.0f + ((1 + overlapped) * .25f); //Line of sight radius is increased based on how many other players are within range of this player 
    }


    IEnumerator releaseToIdle(float duration) {
        yield return new WaitForSeconds(duration);
        if (currentState != playerState.Seated) {
            setPlayerState(playerState.Moveable);
            // release the freezing
        }
    }

    //when player finds the chair
    private void OnTriggerEnter2D(Collider2D collider)
    {
        LogUtility.PrintLogFormat("Player", "Find Chair!");
        if (collider.tag == "Chair")
        {
            IsCarrying = true;
            GameManager.Instance.ChairFound();
        }
    }
    //when player hit player
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.tag == "Player")
        {
            if (collision.transform.GetComponent<Player>().IsCarrying)
            {
                GameManager.Instance.PlayerHit();
                LogUtility.PrintLogFormat("Player", "Hit Carrying player");
            }
        }
    }
}
