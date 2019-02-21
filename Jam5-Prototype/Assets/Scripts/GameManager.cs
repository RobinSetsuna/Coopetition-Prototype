using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

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
    [SerializeField] private float initialChairHoldTime;                    //Chair hold time for each round
    [SerializeField] private float chairDropTimeDecInterval;                //decrease the chair hold time by 1  for each total time interval
    [SerializeField] private float chairDropTimeDecOnHit;                   //decrease the current chair timer when get hit
    [SerializeField] private float chairDropTimeDecOnDrop;                  //decrease the chair Hold Time when drop the chair

    private float chairHoldTime;
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
                        chairHoldTime = initialChairHoldTime;
                        //Random generate end point, player born point, chair point
                        //generate black fog
                        CurrentGameState = GameState.Searching;
                        break;
                    case GameState.Searching:
                        StartCoroutine(Timer());
                        StartCoroutine(HighlightTimer());
                        StartCoroutine(ChairHoldTimeDecreaser());
                        break;
                    case GameState.Battle:
                        StartCoroutine(ChairTimer());
                        //Todo: Disactivate black fog
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

        playerScore = new Dictionary<string, int>
        {
            { "Player1", 0 },
            { "Player2", 0 },
            { "Player3", 0 },
            { "Player4", 0 }
        };

        roundIndex = 1;

    }
    public void PlayerHit()
    {
        chairHoldTime -= chairDropTimeDecOnHit;
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
        if(Input.GetKeyDown(KeyCode.G))
        {
            ChairFound();
        }
        if(Input.GetKeyDown(KeyCode.H))
        {
            PlayerHit();
        }
    }


    public void QuitGame()
    {
        Application.Quit();
    }
    public void ChairFound()
    {
        CurrentGameState = GameState.Battle;
    }
    public void GetChair()
    {
        StartCoroutine(ChairTimer());
    }

    IEnumerator ChairTimer()
    {
        LogUtility.PrintLogFormat("GameManager", "EnterChairTimer and Current Hold Time {0}", chairHoldTime);
        float currentChairHoldTime = chairHoldTime;
        while(CurrentGameState == GameState.Battle)
        {
            if (chairHoldTime > 0)
            {
                chairHoldTime -= Time.deltaTime;
                GameObject.Find("Text_1").GetComponent<Text>().text = chairHoldTime.ToString();
                yield return new WaitForEndOfFrame();
            }
            else
            {
                LogUtility.PrintLogFormat("GameManager", "ChairDropped");
                //Todo: drop the chair
                chairHoldTime = currentChairHoldTime - chairDropTimeDecOnDrop;
                break;
            }
        }      
        yield return null;
    }

    IEnumerator HighlightTimer()
    {
        LogUtility.PrintLogFormat("GameManager", "EnterHighlightTimer");
        float timer = 0;
        while (CurrentGameState == GameState.Searching)
        {
            if (timer < maxSearchTime)
            {
                timer += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            else
            {
                LogUtility.PrintLogFormat("GameManager", "Highlight the chair");
                //Todo: Hightlight the chair
                break;
            }
            yield return null;
        }
        yield return null;
    }
    IEnumerator ChairHoldTimeDecreaser()
    {
        LogUtility.PrintLogFormat("GameManager", "EnterChairHoldTimeDecreaser");
        float Interval = chairDropTimeDecInterval;
        float timer = 0;
        while(CurrentGameState == GameState.Searching)
        {
            if (timer > Interval)
            {
                Interval += chairDropTimeDecInterval;
                if (chairHoldTime != minChairHoldTime)
                {
                    chairHoldTime--;
                    GameObject.Find("Text_1").GetComponent<Text>().text = chairHoldTime.ToString();
                    LogUtility.PrintLogFormat("GameManager", "Chair Hold Time: {0}.", chairHoldTime);
                }
                else
                {
                    break;
                }
            }
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        yield return null;
    }
    IEnumerator Timer()
    {
        float timer = roundDuration;       

        int previousRoundIndex = roundIndex;

        while(timer > 0)
        {
            //check round state, if the round is over, reset the timer
            if(previousRoundIndex != roundIndex)
            {
                yield return null;
            }
            timer -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
            GameObject.Find("Text_2").GetComponent<Text>().text = timer.ToString();
        }
        CurrentGameState = GameState.End;
        yield return null;
    }
  
    public void EndCurrentRound(string playerInChair, string playerCarryChair)
    {
        CurrentGameState = GameState.End;
        UpdatePlayerScore(playerInChair, playerCarryChair);
    }
    //when the end poiot is triggered, call this function to update player score
    private void UpdatePlayerScore(string playerInChair, string playerCarryChair)
    {
        playerScore[playerInChair] += 1;
        playerScore[playerCarryChair] += 2;
    }

    
}
