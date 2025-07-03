using UnityEngine;

public class Message : MonoBehaviour
{
    public GameObject gameObject;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Start()
    {
        gameObject = GetComponent<GameObject>();
        Debug.Log(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
