using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public void WriteScoresScrollable(List<string> userNames, List<int> userScores)
    {
        if (userNames.Count != userScores.Count) return;
        TextMeshProUGUI contentText = Content.GetComponent<TextMeshProUGUI>();
        for (int i = 0; i < userNames.Count; i++)
        {
            contentText.text += "<align=left>" + userNames[i] + ":<line-height=0>\n";
            contentText.text += "<align=right>" + userScores[i] + "<line-height=1em>\n";
        }
    }
}
