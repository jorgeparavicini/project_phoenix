using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WriteScores : MonoBehaviour
{

    public TextMeshProUGUI TitleText;
    public Transform Content;

    public void WriteTitle(string title)
    {
        TitleText.text = title;
    }

    public void WriteScoresScrollable(string userName, int userScore)
    {
        TextMeshProUGUI contentText = Content.GetComponent<TextMeshProUGUI>();
        contentText.text += "<align=left>" + userName + ":<line-height=0>\n";
        contentText.text += "<align=right>" + userScore + "<line-height=1em>\n";
        
            /*"<align=left>This is the left aligned text<line-height=0>";
            "<align=right>5,000<line-height=1em>";*/
    }
}
