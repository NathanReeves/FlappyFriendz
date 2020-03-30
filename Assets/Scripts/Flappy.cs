using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Flappy : MonoBehaviour
{

    public Sprite GetReadySprite;
    public float RotateUpSpeed = 1, RotateDownSpeed = 1;
    public GameObject IntroGUI, DeathGUI;
    public Collider2D restartButtonGameCollider;
    public float VelocityPerJump = 3;
    public float XSpeed = 1;
    public Friend friend;
    public int Score = 0;
    public Text ScoreText;
    public Text EndScoreText;
    public Text BestScoreText;
    public GameObject newScoreTag;
    public Vector3 startPosition;
    Vector3 birdRotation = Vector3.zero;
    GameKit gk;
    DeathFlash deathFlash;
    SerializeGameState gameState = new SerializeGameState(Vector3.zero);
    MatchManager match;
    void Awake()
    {
        
    }
    private void OnEnable()
    {
        startPosition = gameObject.transform.position;
        if (GameStateManager.Instance.isHost)
        {
            //avoid friends starting on top of each other
            startPosition.y = .4f;
        }
        gameObject.transform.position = startPosition;
    }
    void Start()
    {
        match = GameObject.Find("MatchManager").GetComponent<MatchManager>();
        gameState.isAlive = true;
        gk = GameKit.Instance;
        deathFlash = GameObject.Find("DeathFlash").GetComponent<DeathFlash>();
    }


    void Update()
    {

        //GetReady
        if (GameStateManager.GameState == GameState.Intro)
        {
            //MoveX();
            if (Input.GetMouseButtonDown(0))
            {
                GameStateManager.GameState = GameState.Playing;
                Flap();
                GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;

                IntroGUI.SetActive(false);
                ScoreManagerScript.Score = 0;
            }
        }

        else if (GameStateManager.GameState == GameState.Playing)
        {

            MoveX();
            if (Input.GetMouseButtonDown(0))
            {
                Flap();
            }
            RotateFlappy();
        }

#if !UNITY_EDITOR
        sendGameState(transform.position);
#endif

    }

    void sendGameState(Vector2 v)
    {
        gameState.position = v;
        gameState.zRotation = birdRotation.z;
        string state = JsonUtility.ToJson(gameState);
        gk.sendData(state);
    }


    void MoveX()
    {
        transform.position += new Vector3(Time.deltaTime * XSpeed, 0, 0);
    }

    void Flap()
    {
        GetComponent<Rigidbody2D>().velocity = new Vector2(0, VelocityPerJump);
    }

    public void RespawnOnFriend(int friendScore)
    {
        Score = friendScore;
        gameState.score = Score;
        gameState.isAlive = true;
        ScoreText.text = Score.ToString();
        birdRotation = Vector3.zero;
        transform.eulerAngles = birdRotation;
        GetComponent<Rigidbody2D>().velocity = new Vector2(0, .75f);
    }

    private void RotateFlappy()
    {
        
        if (GetComponent<Rigidbody2D>().velocity.y > 0)
        {
            birdRotation = new Vector3(0, 0, Mathf.Clamp(birdRotation.z +  6, -90, 45));
            transform.eulerAngles = birdRotation;
        }
        else
        {
            birdRotation = new Vector3(0, 0, Mathf.Clamp(birdRotation.z -3, -90, 45));
            transform.eulerAngles = birdRotation;
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (GameStateManager.GameState == GameState.Playing)
        {
            if (col.gameObject.tag == "Pipeblank") //pipeblank is an empty gameobject with a collider between the two pipes
            {
                Score++;
                gameState.score = Score;
                ScoreText.text = Score.ToString();
            }
            else if (col.gameObject.tag == "Pipe")
            {
                FlappyDies();
            }
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (GameStateManager.GameState == GameState.Playing)
        {
            if (col.gameObject.tag == "Floor")
            {
                FlappyDies();
            }
        }
    }
    public void UpdateScore(int fScore)
    {
        //use friend score if higher
        if (fScore > Score)
        {
            Score = fScore;
        }
        //show current score
        gameState.score = Score;
        ScoreText.text = Score.ToString();
        EndScoreText.text = Score.ToString();

        //save file & post to leaderboard
        if (Score > GameStateManager.Instance.SaveData.highestScore)
        {
            GameStateManager.Instance.SaveData.highestScore = Score;
            GameStateManager.Instance.WriteSaveData();
            newScoreTag.SetActive(true);
        }
        if (Score > 0)
        {
            GameKit.Instance.PostLeaderboard("score", Score);
        }
        BestScoreText.text = GameStateManager.Instance.SaveData.highestScore.ToString();
    }
    IEnumerator die()
    {
        yield return new WaitForSeconds(1.8f);
        UpdateScore(match.friendGameState.score);
        GameStateManager.GameState = GameState.Dead;
        match.ShowDeath();
        yield return null;
    }
    void FlappyDies()
    {
        gameState.isAlive = false;
        deathFlash.Flash();
        GameStateManager.GameState = GameState.Respawning;
        StartCoroutine(die());
        
    }

}
