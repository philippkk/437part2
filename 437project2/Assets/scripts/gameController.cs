using UnityEngine;
using TMPro;
using System.Data.Common;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
public class gameController : MonoBehaviour
{
    public playerController pc;
    public TMP_Text goalText;
    public int enemiesToSpawn = 5;
    public List<Transform> patrolPoints;

    public GameObject[] enemies;
    public List<GameObject> enemiesList = new List<GameObject>();
    void Start()
    {
        SpawnEnemies();
    }
    void Update()
    {
        handleGoalText();

        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetScene();
        }

        if (enemiesList.Count == 0 && pc.goalIndex == 1)
        {
            pc.goalIndex = 2;
        }
    }

    void ResetScene()
    {
        //todo: reload the current scene
        EditorSceneManager.LoadScene(EditorSceneManager.GetActiveScene().name);
    }
    void handleGoalText()
    {
        if (pc.goalIndex == 0)
        {
            goalText.text = "Find the sword!";
        }
        else if (pc.goalIndex == 1)
        {
            goalText.text = "Use the sword ;)";
        }else if (pc.goalIndex == 2)
        {
            goalText.text = "go play checkers now lol";
        }
    }

    void SpawnEnemies()
    {
        for (int i = 0; i < enemiesToSpawn; i++)
        {
            int randomIndex = Random.Range(0, enemies.Length);
            GameObject enemy = Instantiate(enemies[randomIndex], new Vector3(Random.Range(-50, 50), 0, Random.Range(-50, 50)), Quaternion.identity);
            enemiesList.Add(enemy);
        }
    }
}
