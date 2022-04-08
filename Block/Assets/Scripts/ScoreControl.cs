using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreControl : MonoBehaviour
{
    public static int topScore = 0;
    public Text topScoreText;

    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.HasKey("TopScore"))
        {
            topScore = PlayerPrefs.GetInt("TopScore");
        }

    }

    // Update is called once per frame
    void Update()
    {
        topScoreText.text = "Top score: " + topScore.ToString();

    }
}
