using System.Collections;
using System.Collections.Generic;
using MVS.Helios;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RTTManager : MonoBehaviour
{
    private TMP_Text _rttText;
    private TMP_Text _fpsText;
    
    //DataStorage
    public List<long> data = new List<long>();
    public int maxDataPoints = 100;
    
    //Graph
    public RectTransform graphContainer;
    public UILineRenderer uiLineRenderer;
    public Sprite circleSprite;
    private List<GameObject> circleGameObjects = new List<GameObject>();

    private void Awake()
    {
        _rttText = GetComponent<TMP_Text>();
    }

    private void Start()
    {
        StartCoroutine(GetRtt());
        ShowGraph(data);
    }
    
    float deltaTime = 0.0f;

    private void Update()
    {
        UpdateGraph(data);
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }

    IEnumerator GetRtt()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(0.1f);
            if (HeliosNetwork.IsConnected)
            {
                data.Add(HeliosNetwork.GetCurrentRTT());
                if (data.Count > maxDataPoints)
                {
                    data.RemoveAt(0);
                }
                _rttText.text = $"RTT : {HeliosNetwork.GetCurrentRTT()}";
                if (HeliosNetwork.GetCurrentRTT() > 100)
                {
                    float msec = deltaTime * 1000.0f;
                    float fps = 1.0f / deltaTime;
                    string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
                }
            }   
        }
    }
    
    //Graph
    private GameObject CreateCircle(Vector2 anchoredPosition)
    {
        GameObject gameObject = new GameObject("circle", typeof(Image));
        gameObject.transform.SetParent(graphContainer, false);
        gameObject.GetComponent<Image>().sprite = circleSprite;
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(11, 11);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        circleGameObjects.Add(gameObject);
        return gameObject;
    }
    
    public void ShowGraph(List<long> valueList)
    {
        float graphHeight = graphContainer.sizeDelta.y;
        float yMaximum = 100f; // 예제에서는 100을 최대값으로 사용합니다.
        float xSize = 50f;

        for (int i = 0; i < circleGameObjects.Count; i++)
        {
            Destroy(circleGameObjects[i]);
        }
        circleGameObjects.Clear();
        
        List<Vector2> points = new List<Vector2>();

        for (int i = 0; i < valueList.Count; i++)
        {
            float xPosition = i * xSize;
            float yPosition = (valueList[i] / yMaximum) * graphHeight;
            CreateCircle(new Vector2(xPosition, yPosition));
            points.Add(new Vector2(xPosition / graphContainer.sizeDelta.x, yPosition / graphContainer.sizeDelta.y));
        }
        
        uiLineRenderer.SetPoints(points);
    }
    
    public void UpdateGraph(List<long> valueList)
    {
        float graphHeight = graphContainer.sizeDelta.y;
        float yMaximum = 100f;
        float xSize = graphContainer.sizeDelta.x / valueList.Count;

        if (circleGameObjects.Count > valueList.Count)
        {
            Destroy(circleGameObjects[0]);
            circleGameObjects.RemoveAt(0);
        }

        for (int i = 0; i < circleGameObjects.Count; i++)
        {
            RectTransform rectTransform = circleGameObjects[i].GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(i * xSize, (valueList[i] / yMaximum) * graphHeight);
        }

        if (circleGameObjects.Count < valueList.Count)
        {
            float xPosition = (circleGameObjects.Count) * xSize;
            float yPosition = (valueList[circleGameObjects.Count] / yMaximum) * graphHeight;
            CreateCircle(new Vector2(xPosition, yPosition));
        }
        
        List<Vector2> points = new List<Vector2>();
        for (int i = 0; i < valueList.Count; i++)
        {
            points.Add(new Vector2(i * xSize / graphContainer.sizeDelta.x, valueList[i] / yMaximum));
        }

        uiLineRenderer.SetPoints(points);
    }
}
