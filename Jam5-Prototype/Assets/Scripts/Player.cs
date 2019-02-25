using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
public enum playerState {
    Moveable = 1,
    Seated,
    Boosting,
    Carrying,
    Default
   }
public class Player : MonoBehaviour {
    [SerializeField] private float NoramlSpeed;
    [SerializeField] private float NoramlBoostStrength;
    [SerializeField] private float NoramlBoostFreezeDuration;
    
    [SerializeField] private float CarrySpeed;
    [SerializeField] private float CarryBoostStrength;
    [SerializeField] private float CarryBoostFreezeDuration;
    
    private float speed;
    private float boostStrength;
    private float boostFreezeDuration;

    [SerializeField] private int index;
    [SerializeField] private playerState currentState;
    [SerializeField] private GameObject dust;
    
    [SerializeField] private GameObject Star;
    
    
    [SerializeField] private RuntimeAnimatorController walking;
    [SerializeField] private RuntimeAnimatorController carrying;
    private Rigidbody2D rb2d;
    public PersonalIndicator Indicator;
    private Animator anim;
    private playerState previousState;
    [SerializeField] private Transform seatPoint;
    public Transform seatingBinding;
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
                previousState = currentState;
                currentState = value;
                switch (currentState)
                {
                    case playerState.Moveable:
                        //run when playerState transfered to Moveable
                        anim.runtimeAnimatorController = walking;
                        speed = NoramlSpeed;
                        boostStrength = NoramlBoostStrength;
                        boostFreezeDuration = NoramlBoostFreezeDuration;
                        if (previousState == playerState.Boosting)
                        {
                            Star.GetComponent<ParticleSystem>().enableEmission = false;
                        }

                        if (previousState == playerState.Carrying)
                        {
                            FindChairLeader().seatingBinding = seatPoint;
                        }

                        break;
                    case playerState.Seated:
                        //run when playerState transfered to seated
                        anim.SetBool("Seated", true);
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
                        anim.runtimeAnimatorController= carrying;
                        speed = CarrySpeed;
                        boostStrength = CarryBoostStrength;
                        boostFreezeDuration = CarryBoostFreezeDuration;
                        if (previousState == playerState.Boosting)
                        {
                            Star.GetComponent<ParticleSystem>().enableEmission = false;
                        }
                        break;
                }
            }
        }
    }



    // Use this for initialization
    void Start () {
        rb2d = GetComponent<Rigidbody2D>();
        currentState = playerState.Moveable;
        anim = GetComponent<Animator>();
        Indicator = GetComponentInChildren<PersonalIndicator>();
        Indicator.Initialize();
        Star.GetComponent<ParticleSystem>().enableEmission = false;
        speed = NoramlSpeed;
        boostStrength = NoramlBoostStrength;
        boostFreezeDuration = NoramlBoostFreezeDuration;
    }
	
	// Update is called once per frame
	void Update () {
        switch (currentState) {
            case playerState.Moveable:
                if (index == 2 || index == 3)
                    index += 2;
                float h = Input.GetAxis("Horizontal" + index);
                float v = Input.GetAxis("Vertical" + index);
                anim.SetFloat("h",h);
                anim.SetFloat("v",v);
                if (Mathf.Abs(h) < 0.1f && Mathf.Abs(v) < 0.1f)
                {
                    dust.GetComponent<ParticleSystem>().enableEmission = false;
                }
                else
                {
                    dust.GetComponent<ParticleSystem>().enableEmission = true;
                }
                
                rb2d.velocity = new Vector2(h * speed, v * speed);
                if (Input.GetButtonDown("Boost" + index)) {
                    //boosting
                    // TODO may add another code here
                    setPlayerState(playerState.Boosting);
                }
                break;
            case playerState.Boosting:
                // when boosting, freeze all input
                Star.GetComponent<ParticleSystem>().enableEmission = true;
                break;

            case playerState.Carrying:
                // I think for now using the moveable for carrying will be fine
                if (index == 2 || index == 3)
                    index += 2;
                float h1 = Input.GetAxis("Horizontal" + index);
                float v1 = Input.GetAxis("Vertical" + index);
                anim.SetFloat("h",h1);
                anim.SetFloat("v",v1);
                if (Mathf.Abs(h1) < 0.1f && Mathf.Abs(v1) < 0.1f)
                {
                    dust.GetComponent<ParticleSystem>().enableEmission = false;
                }
                else
                {
                    dust.GetComponent<ParticleSystem>().enableEmission = true;
                }
                
                rb2d.velocity = new Vector2(h1 * speed, v1 * speed);
                if (Input.GetButtonDown("Boost" + index)) {
                    //boosting
                    // TODO may add another code here
                    setPlayerState(playerState.Boosting);
                }
                break;
            case playerState.Seated:
                //bind the position to seating
                if (seatingBinding)
                {
                    transform.position = seatingBinding.position;
                }

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


    IEnumerator releaseToIdle(float duration) {
        yield return new WaitForSeconds(duration);
        if (currentState != playerState.Seated) {
            if (previousState != playerState.Carrying)
            {
                setPlayerState(playerState.Moveable);
            }
            else
            {
                setPlayerState(playerState.Carrying);  
            }
            // release the freezing
        }
    }

    //when player finds the chair
    private void OnTriggerEnter2D(Collider2D collider)
    {        
        if (collider.tag == "Chair")
        {           
            if (GameManager.Instance.PlayerOnChair == null)
            {
                LogUtility.PrintLogFormat("Player", "{0} Sit on Chair!", gameObject.name);
                collider.gameObject.SetActive(false);
                rb2d.velocity = Vector3.zero;
                transform.position = collider.transform.position - new Vector3(0,0.1f,0);
                CurrentState = playerState.Seated;
                rb2d.simulated = false;
                GetComponent<CapsuleCollider2D>().enabled = false;
                GameManager.Instance.SitOnChair(gameObject.name);
                //GetComponent<BoxCollider2D>().enabled = true;
                transform.tag = "Chair";
                //triggerBox.SetActive(true);
                GameManager.Instance.InstantiateTriggerBox(transform.position);
                //Todo: Disable movement
            }
            else if (GameManager.Instance.PlayerCarryChair == null)
            {
                collider.gameObject.SetActive(false);
                Destroy(collider.gameObject);
                LogUtility.PrintLogFormat("Player", "{0} Carry Chair!", gameObject.name);
                CurrentState = playerState.Carrying;
                FindChairLeader().seatingBinding = seatPoint;
                GameManager.Instance.CarryChair(gameObject.name);
            }
        }
    }

    private Player FindChairLeader()
    {
        var players = FindObjectsOfType<Player>();
        foreach (var player in players)
        {
            if (player.CurrentState == playerState.Seated)
            {
                return player;
            }
        }
        return null;
    }

    //when player hit player
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.tag == "Player")
        {
            if (GameManager.Instance.PlayerCarryChair == collision.transform.name)
            {
                GameManager.Instance.PlayerHit();
                //Bouncing Back
                var direction = (collision.transform.position - transform.position).normalized;
                var force = Random.Range(30, 150);
                collision.gameObject.GetComponent<Rigidbody2D>().AddForce(direction * force); 
                LogUtility.PrintLogFormat("Player", "player hit");
            }
        }
    }
    
}
