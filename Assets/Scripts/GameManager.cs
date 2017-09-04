using UnityEngine;
using System.Collections;
using Assets.Scripts.Map;
using Assets.Scripts;
using Assets.Scripts.LevelLogic;

public class GameManager : MonoBehaviour {

    public static GameManager instance = null;
    public BoardManager boardScript;
    public ILevelCompleteCriteria LevelCriteria;

    //Awake is always called before any Start functions
    void Awake()
    {
        //Check if instance already exists
        if (instance == null)

            //if not, set instance to this
            instance = this;

        //If instance already exists and it's not this:
        else if (instance != this)

            //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
            Destroy(gameObject);

        //Sets this to not be destroyed when reloading scene
        DontDestroyOnLoad(gameObject);              
    }

    void Start()
    {
        //Call the InitGame function to initialize the first level 
        InitGame();

        //Update the grid graph once every second
        //TODO: Disable the graph refresh for now.
        //We don't need it and there is a sizable performance hit
        //InvokeRepeating("RescanGridGraph", 0f, 1f);
    }

    //Initializes the game for each level.
    void InitGame()
    {
        //Call the SetupScene function of the BoardManager script, pass it current level number.
        boardScript.SetupScene();
        LevelCriteria = new TouchFinishAndStartLevelCriteria();
    }

    // Update is called once per frame
    void Update () {

        LevelCriteria.Update();
        if (LevelCriteria.IsComplete) {
            //Congrats!! You beat the level
            //Pretty massive hack for music, but c'mon! 
            AudioSource auds = this.GetComponent<AudioSource>();
            if (!auds.isPlaying) {
                this.GetComponent<AudioSource>().Play();
            }
            Debug.Log("WIN!");
        }

        if (Input.GetKeyDown(KeyCode.Tab)) {
            AudioSource auds = this.GetComponent<AudioSource>();
            if (auds.isPlaying)
            {
                this.GetComponent<AudioSource>().Stop();
            }

            boardScript.ClearBoard();
            InitGame();
            
            MapInventory.UpdateGlobalBunnyList();
        }

    }

    private void RescanGridGraph()
    {
        AstarPath.active.Scan();
    }
}
