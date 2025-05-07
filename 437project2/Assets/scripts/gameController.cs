using UnityEngine;
using TMPro;
using System.Data.Common;
public class gameController : MonoBehaviour
{
    public playerController pc;
    public TMP_Text goalText;
    // Update is called once per frame
    void Update()
    {
        handleGoalText();
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
        }
    }
}
