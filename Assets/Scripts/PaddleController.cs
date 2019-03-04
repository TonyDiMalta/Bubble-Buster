using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PaddleController : MonoBehaviour {
    
    private float leftLimitX = 0;
    private float rightLimitX = Screen.width;
    private float paddleUpperY = 0;
    private bool isPaddleSelected = false;
    private int paddleFingerId = -1;
    private bool isReadyToFire = false;
    private int fireFingerId = -1;

    GameManager gameManager;
    GraphicRaycaster graphicRaycaster;
    PointerEventData pointerEventData;
    EventSystem eventSystem;
    LineRenderer lineRenderer;

    void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        graphicRaycaster = GetComponentInParent<GraphicRaycaster>();
        eventSystem = GetComponent<EventSystem>();
        lineRenderer = GetComponent<LineRenderer>();

        var rectTransform = GetComponent<RectTransform>().rect;
        leftLimitX = (rectTransform.width / 2f) * transform.lossyScale.x;
        rightLimitX = Screen.width - leftLimitX;
        paddleUpperY = transform.position.y + rectTransform.height * transform.lossyScale.y;
        
        if (lineRenderer != null)
        {
            lineRenderer.startWidth = 0.025f;
            lineRenderer.endWidth = 0.025f;
        }
    }
    
    void Update()
    {
        for (int i = 0; i < Input.touchCount; ++i)
        {
            Touch touch = Input.GetTouch(i);
            if (touch.phase == TouchPhase.Began)
            {
                if (fireFingerId != touch.fingerId &&
                    isPaddleSelected == false)
                {
                    if (TryToSelectPaddleFromPosition(touch.position) == true)
                    {
                        paddleFingerId = touch.fingerId;
                    }
                }

                if (paddleFingerId != touch.fingerId &&
                    isReadyToFire == false &&
                    touch.position.y > paddleUpperY)
                {
                    Debug.Log("Ready to fire a bubble");
                    isReadyToFire = true;
                    fireFingerId = touch.fingerId;
                    lineRenderer.enabled = true;
                }
            }
            else if (touch.fingerId == paddleFingerId)
            {
                if (touch.phase == TouchPhase.Moved)
                {
                    MovePaddleOnXAxis(touch.position.x);
                    UpdateLineRenderer(touch.position);
                }
                else if (touch.phase == TouchPhase.Ended ||
                    touch.phase == TouchPhase.Canceled)
                {
                    Debug.Log("Paddle released");
                    isPaddleSelected = false;
                    paddleFingerId = -1;
                }
            }
            else if (touch.fingerId == fireFingerId)
            {
                if (touch.phase == TouchPhase.Moved)
                {
                    UpdateLineRenderer(touch.position);
                }
                else if (touch.phase == TouchPhase.Ended ||
                    touch.phase == TouchPhase.Canceled)
                {
                    isReadyToFire = false;
                    fireFingerId = -1;
                    lineRenderer.enabled = false;

                    if (touch.phase == TouchPhase.Ended)
                    {
                        Debug.Log("Fire bubble");
                        gameManager.FireBubbleFromPosition(touch.position);
                    }
                }
            }
        }
    }

    private bool TryToSelectPaddleFromPosition(Vector3 inputPosition)
    {
        if (graphicRaycaster == null)
        {
            return false;
        }

        pointerEventData = new PointerEventData(eventSystem);
        pointerEventData.position = inputPosition;

        List<RaycastResult> results = new List<RaycastResult>();
        graphicRaycaster.Raycast(pointerEventData, results);

        foreach (RaycastResult result in results)
        {
            if (result.gameObject.CompareTag("Player") == true)
            {
                Debug.Log("Paddle selected");
                isPaddleSelected = true;
                return true;
            }
        }

        return false;
    }

    private void MovePaddleOnXAxis(float inputX)
    {
        if (inputX < leftLimitX)
        {
            inputX = leftLimitX;
        }
        else if (inputX > rightLimitX)
        {
            inputX = rightLimitX;
        }

        Vector3 paddlePos = new Vector3(inputX, transform.position.y, transform.position.z);
        transform.position = paddlePos;
    }

    private void UpdateLineRenderer(Vector3 inputPosition)
    {
        if (lineRenderer == null ||
            lineRenderer.positionCount < 2)
        {
            return;
        }
        
        Vector3 originWorldPos = Camera.main.ScreenToWorldPoint(transform.position);
        lineRenderer.SetPosition(0, originWorldPos);

        if (inputPosition.y > paddleUpperY)
        {
            Vector3 destWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(inputPosition.x, inputPosition.y, -Camera.main.transform.position.z));
            lineRenderer.SetPosition(1, destWorldPos);
        }
    }
}
