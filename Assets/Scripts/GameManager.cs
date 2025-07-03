using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    GameObject red, green;

    bool isPlayer, hasGameFinished;

    [SerializeField]
    Text turnMessage;

    const string RED_MESSAGE = "Red's Turn";
    const string GREEN_MESSAGE = "Greens's Turn";

    Color RED_COLOR = new Color(231, 29, 54, 255) / 255;
    Color GREEN_COLOR = new Color(0, 222, 1, 255) / 255;

    Board myBoard;


    private void Awake()
    {
        isPlayer = true;
        hasGameFinished = false;
        turnMessage.text = RED_MESSAGE;
        turnMessage.color = RED_COLOR;
        myBoard = new Board();
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
        if(Input.GetMouseButtonDown(0))
        {
            
            if (hasGameFinished) return;

            
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
            if (!hit.collider) return;

            if(hit.collider.CompareTag("Press"))
            {
                
                if (hit.collider.gameObject.GetComponent<Column>().targetlocation.y > 1.5f) return;

                
                Vector3 spawnPos = hit.collider.gameObject.GetComponent<Column>().spawnLocation;
                Vector3 targetPos = hit.collider.gameObject.GetComponent<Column>().targetlocation;
                GameObject circle = Instantiate(isPlayer ? red : green);
                circle.transform.position = spawnPos;
                circle.GetComponent<Mover>().targetPostion = targetPos;

                
                hit.collider.gameObject.GetComponent<Column>().targetlocation = new Vector3(targetPos.x, targetPos.y + 0.7f, targetPos.z);

                
                myBoard.UpdateBoard(hit.collider.gameObject.GetComponent<Column>().col - 1, isPlayer);
                if(myBoard.Result(isPlayer))
                {
                    turnMessage.text = (isPlayer ? "Red" : "Green") + " Wins!";
                    hasGameFinished = true;
                    return;
                }

                
                turnMessage.text = !isPlayer ? RED_MESSAGE : GREEN_MESSAGE;
                turnMessage.color = !isPlayer ? RED_COLOR : GREEN_COLOR;

               
                isPlayer = !isPlayer;
            }

        }
    }

}
