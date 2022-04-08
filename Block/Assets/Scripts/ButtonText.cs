using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonText : MonoBehaviour
{
    [SerializeField] private Text textButtonBuy;
    
    

    // Update is called once per frame
    void Update()
    {
        if(PlayerPrefs.HasKey("ads") == false)
        {
            textButtonBuy.text = "Remove Ads";
        }
        else
        {
            textButtonBuy.text = "Thanks:)";
        }
    }
}
