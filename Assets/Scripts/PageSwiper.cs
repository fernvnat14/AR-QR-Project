using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PageSwiper : MonoBehaviour, IDragHandler, IEndDragHandler
{
    private Vector3 panelLocation;
    public float percentThreshold = 0.2f;
    public float easing = 0.5f;

    public GameObject greenText;
    public GameObject greenArrow;
    public GameObject button;

    private bool isEnd;
    private bool isBegin;
    private int page = 1;

    // Start is called before the first frame update
    void Start()
    {
        panelLocation = transform.position;
    }

    private void Update()
    {
        if (page == 1)
        {
            greenText.SetActive(true);
            greenArrow.SetActive(true);
            button.SetActive(false);
            isBegin = true;
            isEnd = false;
        }
        else if (page == 2)
        {
            greenText.SetActive(true);
            greenArrow.SetActive(true);
            button.SetActive(false);
            isEnd = false;
            isBegin = false;
        }
        else if (page == 3)
        {
            greenText.SetActive(false);
            greenArrow.SetActive(false);
            button.SetActive(true);
            isEnd = true;
            isBegin = false;
        }
        
    }

    public void LetsTryButton()
    {
        SceneManager.LoadScene("LOGIN-PAGE");
    }

    public void OnDrag(PointerEventData data)
    {
        float difference = data.pressPosition.x - data.position.x;
        transform.position = panelLocation - new Vector3(difference, 0, 0);
    }

    public void OnEndDrag(PointerEventData data)
    {
        float percentage = (data.pressPosition.x - data.position.x) / Screen.width;
        if (Mathf.Abs(percentage) >= percentThreshold)
        {
            Vector3 newLocation = panelLocation;
            if (percentage > 0)
            {
                if (isEnd)
                {
                    newLocation += new Vector3(0, 0, 0);
                }
                else
                {
                    newLocation += new Vector3(-Screen.width, 0, 0);
                    page += 1;
                }
            }
            else if (percentage < 0)
            {
                if (isBegin)
                {
                    newLocation += new Vector3(0, 0, 0);
                }
                else
                {
                    newLocation += new Vector3(Screen.width, 0, 0);
                    page -= 1;
                }
            }

            StartCoroutine(SmoothMove(transform.position, newLocation, easing));
            panelLocation = newLocation;
        }
        else
        {
            StartCoroutine(SmoothMove(transform.position, panelLocation, easing));
        }
    }

    IEnumerator SmoothMove(Vector3 startPos, Vector3 endPos, float seconds)
    {
        float t = 0f;
        while (t <= 1.0)
        {
            t += Time.deltaTime / seconds;
            transform.position = Vector3.Lerp(startPos, endPos, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
    }
}
