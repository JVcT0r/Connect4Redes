public TcpClientUnity tcpClientUnity;

private void Update()
{
    if (Input.GetMouseButtonDown(0))
    {
        if (hasGameFinished) return;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
        RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
        if (!hit.collider) return;

        if (hit.collider.CompareTag("Press"))
        {
            int col = hit.collider.GetComponent<Column>().col - 1;
            ProcessMove(col, isPlayer ? "RED" : "GREEN");
            tcpClientUnity.SendMove(col, isPlayer ? "RED" : "GREEN");
        }
    }
}

public void ProcessMove(int col, string player)
{
    Vector3 spawnPos = /* calcule sua posição spawn */;
    Vector3 targetPos = /* calcule sua posição alvo */;
    GameObject circle = Instantiate(player == "RED" ? red : green);
    circle.transform.position = spawnPos;
    circle.GetComponent<Mover>().targetPostion = targetPos;

    myBoard.UpdateBoard(col, player == "RED");

    if (myBoard.Result(player == "RED"))
    {
        turnMessage.text = $"{player} Wins!";
        hasGameFinished = true;
        return;
    }

    isPlayer = !isPlayer;
    turnMessage.text = isPlayer ? RED_MESSAGE : GREEN_MESSAGE;
    turnMessage.color = isPlayer ? RED_COLOR : GREEN_COLOR;
}

public void ApplyRemoteMove(int col, string player)
{
    ProcessMove(col, player);
}


