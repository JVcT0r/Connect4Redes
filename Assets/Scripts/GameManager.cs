using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : NetworkBehaviour
{
    [SerializeField] GameObject red, green;


    bool isPlayer, hasGameFinished;

    [SerializeField] Text turnMessage;

    const string RED_MESSAGE = "Red's Turn";
    const string GREEN_MESSAGE = "Greens's Turn";

    Color RED_COLOR = new Color(231, 29, 54, 255) / 255;
    Color GREEN_COLOR = new Color(0, 222, 1, 255) / 255;

    Board myBoard;

    public static GameManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        isPlayer = true;
        hasGameFinished = false;
        turnMessage.text = RED_MESSAGE;
        turnMessage.color = RED_COLOR;
        myBoard = new Board();
    }

    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += (clientId) =>
        {
            Debug.Log("Client with id " + clientId + " joined");
            if (NetworkManager.Singleton.IsHost &&
                NetworkManager.Singleton.ConnectedClients.Count == 2)
            {
                SpawnBoard();
            }
        };
    }

    [SerializeField] private GameObject boardPrefab;
    private GameObject newBoard;

    private void SpawnBoard()
    {
        newBoard = Instantiate(boardPrefab);
        newBoard.GetComponent<NetworkObject>().Spawn();
    }


    public void GameStart()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    public void GameQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }



    private void Update()
    {
        //MousePressRpc();
        
        if (Input.GetMouseButtonDown(0))
        {
            //If GameFinsished then return
            if (hasGameFinished) return;
            
            //Raycast2D
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
            if (!hit.collider) return;
            
            

            if (hit.collider.CompareTag("Press"))
            {
                //Check out of Bounds
                if (hit.collider.gameObject.GetComponent<Column>().targetlocation.y > 1.5f) return;

                //Spawn the GameObject
                Vector3 spawnPos = hit.collider.gameObject.GetComponent<Column>().spawnLocation;
                Vector3 targetPos = hit.collider.gameObject.GetComponent<Column>().targetlocation;
                SpawnCircleRpc(spawnPos, targetPos);
                
                /*GameObject circle = Instantiate(isPlayer ? red : green);
                circle.GetComponent<Mover>().targetPostion = targetPos;
                circle.transform.position = spawnPos;
                */

                
                //Increase the targetLocationHeight
                hit.collider.gameObject.GetComponent<Column>().targetlocation = new Vector3(targetPos.x, targetPos.y + 0.7f, targetPos.z);

                //UpdateBoard
                myBoard.UpdateBoardRpc(hit.collider.gameObject.GetComponent<Column>().col - 1, isPlayer);
                UpdateBoardRpc();
                if(hasGameFinished) return;
                
                /*
                if(myBoard.Result(isPlayer))
                {
                    turnMessage.text = (isPlayer ? "Red" : "Green") + " Wins!";
                    hasGameFinished = true;
                    return;
                }
                */
                
                ChangeTurnRpc();
                
                /*
                //TurnMessage
                turnMessage.text = !isPlayer ? RED_MESSAGE : GREEN_MESSAGE;
                turnMessage.color = !isPlayer ? RED_COLOR : GREEN_COLOR;

                //Change PlayerTurn
                isPlayer = !isPlayer;*/
            }
        }
    }

    /*
    [Rpc(SendTo.ClientsAndHost)]
    void  GameFinishedRpc()
    {
        //If GameFinsished then return
        if (hasGameFinished) return;
    }
    */
    

    [Rpc(SendTo.ClientsAndHost)]
    void SpawnCircleRpc(Vector3 spawnPos, Vector3 targetPos)
    {
        /*
        //Raycast2D
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
        RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
        
        //Check out of Bounds
        if (hit.collider.gameObject.GetComponent<Column>().targetlocation.y > 1.5f) return;
        */

        //Spawn the GameObject
        GameObject circle = Instantiate(isPlayer ? red : green);
        circle.GetComponent<Mover>().targetPostion = targetPos;
        circle.transform.position = spawnPos;

        //hit.collider.gameObject.GetComponent<Column>().targetlocation = new Vector3(targetPos.x, targetPos.y + 0.7f, targetPos.z);
    }

    
    
    
    [Rpc(SendTo.ClientsAndHost)]
    void ChangeTurnRpc()
    {
        //TurnMessage
        turnMessage.text = !isPlayer ? RED_MESSAGE : GREEN_MESSAGE;
        turnMessage.color = !isPlayer ? RED_COLOR : GREEN_COLOR;

        //Change PlayerTurn
        isPlayer = !isPlayer;
    }

    [Rpc(SendTo.ClientsAndHost)]
    void UpdateBoardRpc()
    {
        if(myBoard.Result(isPlayer))
        {
            turnMessage.text = (isPlayer ? "Red" : "Green") + " Wins!";
            hasGameFinished = true;
        }
    }
}
/*
[Rpc(SendTo.ClientsAndHost)]
    void MousePressRpc()
    {}
*/
