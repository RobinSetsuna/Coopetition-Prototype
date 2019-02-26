using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
public enum playerState {
    Moveable = 1,
    Seated,
    Boosting,
    Carrying,
    Default,
    Trapped
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
    private float trappedTimer = 1.75f;

    [SerializeField] private int index;
    [SerializeField] private playerState currentState;
    [SerializeField] private GameObject dust;
    
    [SerializeField] private GameObject Star;
    
    
    [SerializeField] private RuntimeAnimatorController walking;
    [SerializeField] private RuntimeAnimatorController carrying;
    
    
    public float maxSpeed;
    public float minSpeed;
    private Rigidbody2D rb2d;
    public PersonalIndicator Indicator;
    private Animator anim;
    private Vector2 CurrentPressDirec;
    private playerState previousState;
    [SerializeField] private Transform seatPoint;
    [SerializeField] private GameObject FX;
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
                        maxSpeed = NoramlSpeed;
                        minSpeed = - NoramlSpeed;
                        boostStrength = NoramlBoostStrength;
                        boostFreezeDuration = NoramlBoostFreezeDuration;
                        if (previousState == playerState.Boosting)
                        {
                            Star.GetComponent<ParticleSystem>().enableEmission = false;
                        }
                        if (previousState == playerState.Carrying) {
                            var temp = FindChairLeader();
                            if (temp)
                            {
                            temp.seatingBinding = null;
                            }
                            
                        }
                        break;
                    case playerState.Seated:
                        //run when playerState transfered to seated
                        anim.SetBool("Seated", true);
                        break;

                    case playerState.Boosting:
                        //var temp = rb2d.velocity;
                        // get current move direction
                        var currentDirection = CurrentPressDirec;
                        rb2d.AddForce(currentDirection * boostStrength);
                        // add force to same direction
                        StartCoroutine(releaseToIdle(boostFreezeDuration));
                        break;

                    case playerState.Carrying:
                        // change the speed and duration
                        anim.runtimeAnimatorController= carrying;
                        maxSpeed = CarrySpeed;
                        minSpeed = - CarrySpeed;
                        boostStrength = CarryBoostStrength;
                        boostFreezeDuration = CarryBoostFreezeDuration;
                        if (previousState == playerState.Boosting)
                        {
                            previousState = playerState.Carrying;
                            Star.GetComponent<ParticleSystem>().enableEmission = false;
                        }
                        break;

                    case playerState.Trapped:
                        maxSpeed = 0;
                        minSpeed = 0;
                        StartCoroutine(releaseToIdle(trappedTimer));
                        break;
                }
            }
        }
    }



    // Use this for initialization
    void Start () {
        rb2d = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        Indicator = GetComponentInChildren<PersonalIndicator>();
        Indicator.Initialize();
        Star.GetComponent<ParticleSystem>().enableEmission = false;
        CurrentState = playerState.Moveable;
        seatingBinding = null;
        //maxSpeed = NoramlSpeed;
        //minSpeed = -NoramlSpeed;
        boostStrength = NoramlBoostStrength;
        boostFreezeDuration = NoramlBoostFreezeDuration;

    }
	
	// Update is called once per frame
	void Update () {
	    
	    //prevent bouncing out of level
	    if (Mathf.Abs(transform.position.x) > 16f || Mathf.Abs(transform.position.y) > 10.2f)
	    {
	        if (currentState != playerState.Default)
	        {
	            setPlayerState(playerState.Default);
	            StartCoroutine(resetPos());
	        }
	    }

	    switch (currentState) {
            case playerState.Moveable:
                if (index == 2 || index == 3)
                {
                    index += 2;
                }
                   
                float h = Input.GetAxis("Horizontal" + index);
                float v = Input.GetAxis("Vertical" + index);
                anim.SetFloat("h",h);
                anim.SetFloat("v",v);
                CurrentPressDirec = new Vector2(h, v);
                physicsInputHelper(h, v);
                if (Mathf.Abs(h) < 0.1f && Mathf.Abs(v) < 0.1f)
                {
                    dust.GetComponent<ParticleSystem>().enableEmission = false;
                }
                else
                {
                    dust.GetComponent<ParticleSystem>().enableEmission = true;
                }
                
                //rb2d.velocity = new Vector2(h * speed, v * speed);
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
                CurrentPressDirec = new Vector2(h1, v1);
                physicsInputHelper(h1, v1);
                if (Mathf.Abs(h1) < 0.1f && Mathf.Abs(v1) < 0.1f)
                {
                    dust.GetComponent<ParticleSystem>().enableEmission = false;
                }
                else
                {
                    dust.GetComponent<ParticleSystem>().enableEmission = true;
                }
                
                //rb2d.velocity = new Vector2(h1 * speed, v1 * speed);
                if (Input.GetButtonDown("Boost" + index)) {
                    //boosting
                    // TODO may add another code here
                    setPlayerState(playerState.Boosting);
                }
                break;
            case playerState.Seated:
                //bind the position to seating
                if (seatingBinding&&GameManager.Instance.PlayerCarryChair != null)
                {
                    transform.position = seatingBinding.position;
                }

                break;
        } 
	}
    public void Initialize(Vector3 position,int _index = -1)
    {
        rb2d = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        index = _index == -1 ? index : _index;
        transform.position = position;
        rb2d.simulated = true;
        GetComponent<CapsuleCollider2D>().enabled = true;
        CurrentState = playerState.Moveable;
    }

    public int GetIndex() { return index; }

    public void setPlayerState(playerState targetState) { CurrentState = targetState; }

    IEnumerator resetPos()
    {
        yield return  new WaitForSeconds(1f);
        transform.position = Vector3.zero;
        setPlayerState(playerState.Moveable);
    }

    IEnumerator releaseToIdle(float duration) {
        yield return new WaitForSeconds(duration);
        if (currentState != playerState.Seated) {
            if (previousState != playerState.Carrying)
            {
                setPlayerState(playerState.Moveable);
            }
            else
            {

                if (GameManager.Instance.PlayerCarryChair == null)
                {
                    setPlayerState(playerState.Moveable);
                }
                else{
                    setPlayerState(playerState.Carrying);
                }
              
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
                anim.SetBool("Seated", true);
                rb2d.simulated = false;
                GetComponent<CapsuleCollider2D>().enabled = false;
                GameManager.Instance.SitOnChair(gameObject.name);
                //GetComponent<BoxCollider2D>().enabled = true;
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
        if(collider.tag == "Trap")
        {
            currentState = playerState.Trapped;
        }
    }

    private void physicsInputHelper(float h, float v)
    {
              
        // calculate speed on X axis
        if (Mathf.Abs(rb2d.velocity.x) < maxSpeed )
        {
            if (Mathf.Abs(h) > 0.1f)
            {
                var direction = Vector3.right * h * 4f;
                if (direction.x * rb2d.velocity.x < 0)
                {
                    direction = direction * 2;
                }

                rb2d.AddForce(direction);
            }
            else
            {
                // reduce speed,friction
                var direction = rb2d.velocity.normalized;
                rb2d.AddForce(new Vector2(-direction.x*10f, 0f));
                //rb2d.AddForce(new Vector2(0f,-direction.y*10f));
            }
        }else{
            if (Mathf.Abs(h) > 0.1f)
            {
                //do nothing
                if (h * rb2d.velocity.x < 0)
                {
                    rb2d.AddForce(new Vector2(h*100f,0f));
                }
            }
            else
            {
                // reduce speed,friction
                var direction = rb2d.velocity.normalized;
                rb2d.AddForce(new Vector2(-direction.x*10f, 0f));
                //rb2d.AddForce(new Vector2(0f,-direction.y*10f));
            }
        }
        
        
        // calculate speed on Y axis
        if (Mathf.Abs(rb2d.velocity.y) < maxSpeed )
        {
            if (Mathf.Abs(v) > 0.1f)
            {
                var direction = Vector3.up * v * 4f;
                if (direction.y * rb2d.velocity.y < 0)
                {
                    direction = direction * 2;
                }

                rb2d.AddForce(direction);
            }
            else
            {
                // reduce speed,friction
                var direction = rb2d.velocity.normalized;
                rb2d.AddForce(new Vector2(0f,-direction.y*10f));
                //rb2d.AddForce(new Vector2(0f,-direction.y*10f));
            }
        }else{
            if (Mathf.Abs(v) > 0.1f)
            {
                //do nothing
                if (v * rb2d.velocity.y < 0)
                {
                    rb2d.AddForce(new Vector2(0f,h*100f));
                }
            }
            else
            {
                // reduce speed,friction
                var direction = rb2d.velocity.normalized;
                rb2d.AddForce(new Vector2(0f,-direction.y*10f));
                //rb2d.AddForce(new Vector2(0f,-direction.y*10f));
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
        if (collision.transform.tag == "Player" && currentState == playerState.Boosting)
        {
            var direction = (collision.transform.position - transform.position).normalized;
            float force = Random.Range(400, 500);
            if (GameManager.Instance.PlayerCarryChair == collision.transform.name)
            {
                GameManager.Instance.PlayerHit();
                //Bouncing Back
                force *= 0.8f; 
                LogUtility.PrintLogFormat("Player", "player hit");
            }
            collision.gameObject.GetComponent<Rigidbody2D>().AddForce(direction * force);
            Vector3 midPoint = transform.transform.position + (collision.transform.position - transform.position) / 2;
            Destroy(Instantiate(FX, midPoint, Quaternion.identity),0.5f);
            
            StartCoroutine(Hurt( collision.gameObject.GetComponent<SpriteRenderer>()));
        }
    }

    IEnumerator Hurt(SpriteRenderer target)
    {
        // Let me do the stupid thing here
        target.color = Color.grey;
        yield return new WaitForSeconds(0.1f);
        target.color = Color.white;
        yield return new WaitForSeconds(0.1f);
        target.color = Color.grey;
        yield return new WaitForSeconds(0.1f);
        target.color = Color.white;
        //hell yeah Animation, lol
    }
    
}
