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
    [SerializeField] private float chairHoldTimeDecOnInterval;              //decrease the chair hold time by 1  for each total time interval
    [SerializeField] private float chairHoldTimeDecOnHit;                   //decrease the current chair timer when get hit
    [SerializeField] private float chairHoldTimeDecOnDrop;                  //decrease the chair Hold Time when drop the chair
    [SerializeField] private string playerOnChair;
    [SerializeField] private string playerCarryChair;

    private float chairHoldTime;
    private int roundIndex;
    private bool isChairTimerOn;
    public GameObject blackMask;

    UnityEvent DropChair;
    UnityEvent Highlight;

    public string PlayerOnChair
    {
        get { return playerOnChair; }
        set { playerOnChair = value; }
    }

    public string PlayerCarryChair
    {
        get { return playerCarryChair; }
        set { playerCarryChair = value; }
    }

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
                        isChairTimerOn = false;
                        PlayerOnChair = null;
                        PlayerCarryChair = null;
                        //Random generate end point, player born point, chair point
                        //generate black fog
                        blackMask.SetActive(true);
                        CurrentGameState = GameState.Searching;
                        break;
                    case GameState.Searching:
                        StartCoroutine(Timer());
                        StartCoroutine(HighlightTimer());
                        StartCoroutine(ChairHoldTimeDecreaser());
                        break;
                    case GameState.Battle:
                        //Disactivate black fog
                        blackMask.SetActive(false);
                        break;
                    case GameState.End:
                        //Distroy former points and black fog
                        roundIndex += 1;
                        if (roundIndex % (totalRounds + 1) == 0)
                        {
                            //Game Over
                            //Show total scores and the winner!\
                        }
                        else
                        {
                            Debug.Log("player0 score:"+ playerScore["Player0"]);
                            Debug.Log("player1 score:" + playerScore["Player1"]);
                            Debug.Log("player2 score:" + playerScore["Player2"]);
                            Debug.Log("player3 score:" + playerScore["Player3"]);
                            //CurrentGameState = GameState.Initial;
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
            { "Player0", 0 },
            { "Player1", 0 },
            { "Player2", 0 },
            { "Player3", 0 }
        };

        roundIndex = 1;
        
        LightAura._instance.Enable();
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
        if (Input.GetKeyDown(KeyCode.H))
        {
            CurrentGameState = GameState.Initial;
        }
    }
    
    public void QuitGame()
    {
        Application.Quit();
    }

    IEnumerator ChairTimer()
    {
        isChairTimerOn = true;
        LogUtility.PrintLogFormat("GameManager", "EnterChairTimer and Current Hold Time {0}", chairHoldTime);
        float currentChairHoldTime = chairHoldTime;

        while(CurrentGameState == GameState.Battle && chairHoldTime > 0)
        {
            chairHoldTime -= Time.deltaTime;
            GameObject.Find("TextHoldTime").GetComponent<Text>().text = chairHoldTime.ToString();
            yield return new WaitForEndOfFrame();  
        }
        if(CurrentGameState != GameState.End)
            LogUtility.PrintLogFormat("GameManager", "ChairDropped");
        else
            LogUtility.PrintLogFormat("GameManager", "Stop Chair Timer for New Round");

        isChairTimerOn = false;       
        //drop the chair////////////////
        //DropChair.Invoke();  
        //decrese hold time on every drop
        chairHoldTime = currentChairHoldTime - chairHoldTimeDecOnDrop;
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
                //Hightlight the chair///////////////
                //Highlight.Invoke();
                break;
            }
            yield return null;
        }
        yield return null;
    }
    IEnumerator ChairHoldTimeDecreaser()
    {
        LogUtility.PrintLogFormat("GameManager", "EnterChairHoldTimeDecreaser");
        float Interval = chairHoldTimeDecOnInterval;
        float timer = 0;
        while(CurrentGameState == GameState.Searching)
        {
            if (timer > Interval)
            {
                Interval += chairHoldTimeDecOnInterval;
                if (chairHoldTime != minChairHoldTime)
                {
                    chairHoldTime--;
                    GameObject.Find("TextHoldTime").GetComponent<Text>().text = chairHoldTime.ToString();
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

        while(currentGameState != GameState.End && timer > 0)
        {
            //check round state, if the round is over, reset the timer
            if(previousRoundIndex != roundIndex)
            {
                yield return null;
            }
            timer -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
            GameObject.Find("TextTotalTime").GetComponent<Text>().text = timer.ToString();
        }
        if(currentGameState != GameState.End)
            CurrentGameState = GameState.End;
        yield return null;
    }

    private void UpdatePlayerScore()
    {
        playerScore[playerOnChair] += 1;
        playerScore[playerCarryChair] += 2;
    }

    ///////////////// For other script to call///////////////////////
    public void EndCurrentRound()        //each player has a string id, like Player1, Player2, Player3 ...
    {
        if (isChairTimerOn)
            StopCoroutine(ChairTimer());
        UpdatePlayerScore();
        CurrentGameState = GameState.End;     
    }

    public void CarryChair(string p)
    {
        if(!isChairTimerOn)
            StartCoroutine(ChairTimer());
        playerCarryChair = p;
        
    }
    public void SitOnChair(string p)
    {
        if (CurrentGameState != GameState.Battle)
            CurrentGameState = GameState.Battle;
        playerOnChair = p;
    }

    public void PlayerHit()
    {
        if(isChairTimerOn)
            chairHoldTime -= chairHoldTimeDecOnHit;
    }
}
