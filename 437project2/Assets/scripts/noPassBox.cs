using UnityEngine;

public class noPassBox : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private gameController gc;
    void Start()
    {
        gc = GameObject.Find("gameController").GetComponent<gameController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (gc.enemiesList.Count == 0)
        {
            Destroy(gameObject);
        } 
    }
}
