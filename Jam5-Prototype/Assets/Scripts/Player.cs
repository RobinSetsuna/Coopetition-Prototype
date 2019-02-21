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

    [SerializeField] private bool carrying;
    [SerializeField] private float sightRadius;

    [SerializeField] private int index;
    private Rigidbody2D rb2d;
    private playerState currentState;
    // Use this for initialization
    void Start () {
        rb2d = GetComponent<Rigidbody2D>();
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
                    Boost();
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

    public void Boost() {

        // get current move direction
        var temp = rb2d.velocity;
        var currentDirection = temp.normalized;
        rb2d.AddForce(currentDirection * boostStrength);
        StartCoroutine(releaseToIdle(boostFreezeDuration));
    }
    public void Carry() {
        speed = 1.5f;
        boostFreezeDuration = boostFreezeDuration * 2;
    }

   public int GetIndex() { return index; }

   public void SetRadius(int overlapped)
    {
        sightRadius = 5.0f + ((1 + overlapped) * .25f); //Line of sight radius is increased based on how many other players are within range of this player 
    }


    IEnumerator releaseToIdle(float duration) {
        yield return new WaitForSeconds(duration);
        if (currentState != playerState.Seated) {
            currentState = playerState.Moveable;
            // release the freezing
        }
    }
}
