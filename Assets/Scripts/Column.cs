using UnityEngine;
using Mirror;
public class Column : MonoBehaviour
{
    public int col;
    public Vector3 spawnLocation;
    public Vector3 targetlocation;

    private void Awake()
    {
        // Se quiser inicializar spawnLocation e targetlocation com valores padrão baseados na posição do objeto Column na cena:
        if (spawnLocation == Vector3.zero)
            spawnLocation = transform.position + Vector3.up * 1f; // 1 unidade acima da coluna
        if (targetlocation == Vector3.zero)
            targetlocation = spawnLocation;
    }

    private void OnMouseDown()
    {
        if (!NetworkClient.active) return;

        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null && gm.isLocalPlayer)
        {
            gm.CmdTryPlay(col);
        }
    }
}



