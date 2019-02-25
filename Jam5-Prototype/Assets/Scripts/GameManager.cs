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
    [SerializeField] private GameObject TriggerBox;
    private float chairHoldTime;
    private int roundIndex;
    private bool isChairTimerOn;
    private GameObject chairs;
    private GameObject exits;
    private GameObject player0;
    private GameObject player1;
    private GameObject player2;
    private GameObject player3;
    public bool highlight;

    public GameObject blackMask;
    public GameObject roundText;
    public GameObject roundResultText;
    public GameObject info;
    public GameObject title;
    public GameObject buttonText;
    public GameObject pairs;
    public GameObject [] players;
    public GameObject exit;
    public GameObject chair;

    UnityEvent DropChair;

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
                        roundText.GetComponent<Text>().text = "Round" + roundIndex;
                        chairHoldTime = initialChairHoldTime;
                        isChairTimerOn = false;
                        highlight = false;
                        PlayerOnChair = null;
                        PlayerCarryChair = null;
                        Spawn();
                        blackMask.SetActive(true);
                        CurrentGameState = GameState.Searching;
                        break;
                    case GameState.Searching:
                        StartCoroutine(Timer());
                        StartCoroutine(HighlightTimer());
                        StartCoroutine(ChairHoldTimeDecreaser());
                        break;
                    case GameState.Battle:
                        blackMask.SetActive(false);
                        chairs.GetComponent<ItemIndicator>().Disable();
                        //exits.transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = true;
                        break;
                    case GameState.End:
                        //Distroy former points and black fog
                        ActiveOption(false);
                        roundIndex += 1;
                        if (roundIndex % (totalRounds + 1) == 0)
                        {
                            int max = 0;
                            string winner = "nobody";
                            foreach(KeyValuePair<string,int> p in playerScore)
                            {
                                if (p.Value >= max)
                                {
                                    max = p.Value;
                                    winner = p.Key;
                                }                              
                            }
                            if(CheckWinner(max))
                            {
                                ShowScore("Winner is " + winner + "!");
                            }
                            else
                            {
                                totalRounds++;
                                ShowScore("Round End");                              
                            }
                        }
                        else
                        {
                            ShowScore("Round End");
                        }
                        break;
                }
            }
        }
    }
    void Start()
    {
        //initial current round index
        roundIndex = 1;


        //Random generate end point, player born point, chair point
        chairs = Instantiate(this.chair, GameObject.Find("Level").transform);
        exits = Instantiate(exit, GameObject.Find("Level").transform);   
        player0 = Instantiate(players[0]);
        player1 = Instantiate(players[1]);
        player2 = Instantiate(players[2]);
        player3 = Instantiate(players[3]);
        ResponsibleCamera._instance.SetPlayers();
        player0.GetComponentInChildren<PersonalIndicator>().setTarget(chairs.transform);
        player1.GetComponentInChildren<PersonalIndicator>().setTarget(chairs.transform);
        player2.GetComponentInChildren<PersonalIndicator>().setTarget(chairs.transform);
        player3.GetComponentInChildren<PersonalIndicator>().setTarget(chairs.transform);
        ActiveOption(false);

        //set initial game state
        CurrentGameState = GameState.Initial;
        //initial player score 
        playerScore = new Dictionary<string, int>
        {
            { "Player0(Clone)", 0 },
            { "Player1(Clone)", 0 },
            { "Player2(Clone)", 0 },
            { "Player3(Clone)", 0 }
        };
        
        
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
        playerCarryChair = null;
        if(CurrentGameState != GameState.End)
            LogUtility.PrintLogFormat("GameManager", "ChairDropped");
        else
            LogUtility.PrintLogFormat("GameManager", "Stop Chair Timer for New Round");

        isChairTimerOn = false;
        //drop the chair////////////////
        //DropChair.Invoke();  
        var players = FindObjectsOfType<Player>();
        foreach (var player in players)
        {
            if (player.CurrentState == playerState.Carrying)
            {
                player.setPlayerState(playerState.Moveable);
                break;
            }
        }
        //decrese hold time on every drop
        if (currentChairHoldTime - chairHoldTimeDecOnDrop < minChairHoldTime)
            chairHoldTime = minChairHoldTime;
        else
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
                highlight = true;
                chairs.transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = true;
                ResponsibleCamera._instance.focusAt(chairs.transform);
                StartCoroutine(LookAtReleaser());
                break;
            }
            yield return null;
        }
        yield return null;
    }
    IEnumerator LookAtReleaser()
    {
        float timer = 0;
        while(timer < 3)
        {
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        ResponsibleCamera._instance.reset();
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
            GameObject.Find("TextTotalTime").GetComponent<Text>().text = "Time Left: " + (int)timer;
        }
        if(currentGameState != GameState.End)
            CurrentGameState = GameState.End;
        yield return null;
    }

    private bool CheckWinner(int max)
    {
        int temp = 0;
        foreach (KeyValuePair<string, int> p in playerScore)
        {
            if (p.Value == max)
            {
                temp++;
            }
        }
        if (temp > 1)
            return false;
        else
            return true;
    }

    private void UpdatePlayerScore()
    {
        playerScore[playerOnChair] += 1;
        playerScore[playerCarryChair] += 2;
    }

    private void ShowScore(string t)
    {
        //Debug.Log("player0 score:"+ playerScore["Player0"]);
        //Debug.Log("player1 score:" + playerScore["Player1"]);
        //Debug.Log("player2 score:" + playerScore["Player2"]);
        //Debug.Log("player3 score:" + playerScore["Player3"]);
        if (roundIndex % (totalRounds + 1) == 0)
        {
            buttonText.GetComponent<Text>().text = "Quit Game";
        }
        title.GetComponent<Text>().text = t;
        roundResultText.GetComponent<Text>().text = "Player0 Score:" + playerScore["Player0(Clone)"] + "\n" +
                                                    "Player1 Score:" + playerScore["Player1(Clone)"] + "\n" +
                                                    "Player2 Score:" + playerScore["Player2(Clone)"] + "\n" +
                                                    "Player3 Score:" + playerScore["Player3(Clone)"];
        roundResultText.SetActive(true);
    }

    private void Spawn()
    {
        int index = roundIndex - 1;
        Transform[] t = pairs.transform.GetChild(index).GetComponentsInChildren<Transform>();
        chairs.transform.position = t[1].position;
        exits.transform.position = t[2].position;
        player0.transform.position = t[3].position;
        player1.transform.position = t[4].position;
        player2.transform.position = t[5].position;
        player3.transform.position = t[6].position;
        ActiveOption(true);
    }

    private void ActiveOption(bool b)
    {
        chairs.SetActive(b);
        exits.SetActive(b);
        player0.SetActive(b);
        player1.SetActive(b);
        player2.SetActive(b);
        player3.SetActive(b);
    }
    ///////////////// For other script to call///////////////////////
    public void EndCurrentRound()        //each player has a string id, like Player1, Player2, Player3 ...
    {
        UpdatePlayerScore();
        CurrentGameState = GameState.End;     
    }

    public void CarryChair(string p)
    {
        if(!isChairTimerOn)
            StartCoroutine(ChairTimer());
        playerCarryChair = p;
        StartCoroutine(WaitForSeconds(5f, p + " Carrys the Chair!"));
        
    }

    public void SitOnChair(string p)
    {
        if (CurrentGameState != GameState.Battle)
            CurrentGameState = GameState.Battle;
        playerOnChair = p;
        StartCoroutine(WaitForSeconds(5f, p + " Sits on the Chair!"));
    }

    public void PlayerHit()
    {
        if(isChairTimerOn)
            chairHoldTime -= chairHoldTimeDecOnHit;
    }
    public void NextRound()
    {
        if (roundIndex % (totalRounds + 1) == 0)
        {
            QuitGame();
        }
        else
        {
            CurrentGameState = GameState.Initial;
            roundResultText.SetActive(false);
        }          
    }

    public void InstantiateTriggerBox(Vector3 pos)
    {
        Instantiate(TriggerBox, pos, Quaternion.identity);
    }

    IEnumerator WaitForSeconds(float f, string text)
    {
        info.GetComponent<Text>().text = text;
        info.SetActive(true);
        yield return new WaitForSecondsRealtime(f);
        info.SetActive(false);
        yield return null;
    }
}
