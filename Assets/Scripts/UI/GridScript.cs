using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnEnable()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(GetComponent<RectTransform>().parent.parent.name);
        Debug.Log(GetComponent<RectTransform>().parent.parent.GetComponent<RectTransform>().sizeDelta.x);

        var width = GetComponent<RectTransform>().parent.parent.GetComponent<RectTransform>().sizeDelta.x;
        var height = GetComponent<RectTransform>().parent.parent.GetComponent<RectTransform>().sizeDelta.y;

        float scoreWidth;
        float scoreHeight;
        float spacingWidth;
        float spacingHeight;

        int columns;
        

        if (width > 1700)
        {
            columns = 3;
            
            scoreWidth = 500;
            spacingWidth = (width - columns * scoreWidth) / (2*columns);
        }else if (width > 1200)
        {
            columns = 3;

            var scalingFactor = width / 1700;
            
            scoreWidth = 500 * scalingFactor;
            spacingWidth = (width - columns * scoreWidth) / (2*columns);
        }
        else
        {
            columns = 2;

            var scalingFactor = width / 1200;
            
            scoreWidth = 500 * scalingFactor;
            spacingWidth = ((width - columns * scoreWidth) / (2*columns));
        }

        if (height > 900)
        {
            scoreHeight = 350;
            spacingHeight = (height - 2 * scoreHeight) / 4;
        }
        else
        {
            var scalingFactor = height / 900;
            
            scoreHeight = 350 * scalingFactor;
            spacingHeight = (height - 2 * scoreHeight) / 4;
        }

        List<RectTransform> children = GetComponentsInDirectChildren<RectTransform>(gameObject);

        for (int i = 0; i < children.Count; i++)
        {
            RectTransform child = children[i];
            
            child.anchorMin = new Vector2(0, 1);
            child.anchorMax = new Vector2(0, 1);
            child.pivot = new Vector2(0, 1);
            
            child.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, scoreWidth);
            child.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, scoreHeight);

            var x = (scoreWidth + 2 * spacingWidth) * (i % columns) + spacingWidth;
            var y = -((scoreHeight + 2 * spacingHeight) * (float)Math.Floor((float)i/columns) + spacingHeight);
            
            Debug.Log("Child: " + child.name + "\nWidth: " + width + "\nSpacing Width: " + spacingWidth + "\nSpacing Height: " + spacingHeight + "\nx: " + x + "\ny: " + y);
            child.anchoredPosition = new Vector3(x, y, 0);
        }
    }
    
    public static List<T> GetComponentsInDirectChildren<T>(GameObject gameObject) where T : Component
    {
        int length = gameObject.transform.childCount;
        List<T> components = new List<T>(length);
        for (int i = 0; i < length; i++)
        {
            T comp = gameObject.transform.GetChild(i).GetComponent<T>();
            if (comp != null) components.Add(comp);
        }
        return components;
    }
}
