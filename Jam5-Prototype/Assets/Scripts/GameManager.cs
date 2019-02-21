using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum GameState : int
{
    Initial = 1,
    Searching,
    Battle,
    End,
}

public class GameManager : MonoBehaviour {

    public static GameManager Instance = null;

    [SerializeField] private Dictionary<string, int> playerScore;
    [SerializeField] private GameState currentGameState;
    [SerializeField] private float roundDuration;
    [SerializeField] private float maxSearchTime;
    [SerializeField] private float minChairHoldTime;
    [SerializeField] private int totalRounds;
    [SerializeField] private float initialChairDropTime;
    [SerializeField] private float chairDropTimeDecInterval;
    [SerializeField] private float chairDropTimeDecOnHit;                   //decrease the current chair timer when get hit
    [SerializeField] private float chairDropTimeDecOnDrop;                  //decrease the chair Hold Time when drop the chair

    public UnityEvent OnPlayerHit;                                          //invoke this event to decrease the chair timer
    private float chairDropTime;
    private int roundIndex;


    public GameState CurrentGameState
    {
        get
        {
            return currentGameState;
        }

        private set
        {
            if (value == currentGameState)
            {
                LogUtility.PrintLogFormat("GameManager", "Reset {0}.", value);
            }
            else
            {
                LogUtility.PrintLogFormat("GameManager", "Made a transition to {0}.", value);

                GameState previousGameState = CurrentGameState;
                currentGameState = value;
                switch (currentGameState)
                {
                    case GameState.Initial:
                        chairDropTime = initialChairDropTime;
                        //Random generate end point, player born point, chair point
                        //generate black fog
                        break;

                    case GameState.Searching:
                        RoundStart();

                        break;
                    case GameState.Battle:
                        //Todo:Disactivate black fog
                        break;
                    case GameState.End:
                        roundIndex += 1;
                        if(roundIndex % (totalRounds + 1) == 0)
                        {
                            //Game Over
                        }
                        break;
                }
            }
        }
    }
    void Start()
    {
        //set initial game state
        CurrentGameState = GameState.Initial;

        playerScore.Add("Player1", 0);
        playerScore.Add("Player2", 0);
        playerScore.Add("Player3", 0);
        playerScore.Add("Player4", 0);

        roundIndex = 1;

        OnPlayerHit.AddListener(PlayerHit);
    }
    private void PlayerHit()
    {
        chairDropTime -= chairDropTimeDecOnHit;
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {

    }


    public void QuitGame()
    {
        Application.Quit();
    }

    public void GetChair()
    {
        StartCoroutine(ChairTimer());
    }
    IEnumerator ChairTimer()
    {
        float currentChairDropTime = chairDropTime;
        while (chairDropTime > 0)
        {
            chairDropTime -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        //Todo: drop the chair

        chairDropTime = currentChairDropTime - chairDropTimeDecOnDrop;
        yield return null;
    }

    IEnumerator Timer()
    {
        float timer = roundDuration;       
        float Interval = chairDropTimeDecInterval;
        GameState previousGameState = CurrentGameState;

        while(timer > 0)
        {
            //check game state, if state changed, reset the timer
            if(previousGameState != CurrentGameState)
            {
                timer = roundDuration;
            }
            //if elapsed time exceed the max search time for chair, then highlight the chair
            if((roundDuration - timer) > maxSearchTime)
            {
                //Todo: Highlight the stair
            }
            if((roundDuration - timer) > Interval)
            {
                Interval += Interval;               
                if(chairDropTime != minChairHoldTime)           
                {
                    chairDropTime--;
                }
                    
            }
            timer -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        CurrentGameState = GameState.Initial;
        yield return null;
    }

    void RoundStart()
    {
        //timer for each round
        StartCoroutine(Timer());

    }
    public void EndCurrentRound(string playerInChair, string playerCarryChair)
    {
        CurrentGameState = CurrentGameState + 1;
        UpdatePlayerScore(playerInChair, playerCarryChair);
    }
    //when the end poiot is triggered, call this function to update player score
    private void UpdatePlayerScore(string playerInChair, string playerCarryChair)
    {
        playerScore[playerInChair] += 1;
        playerScore[playerCarryChair] += 2;
    }

    
}
