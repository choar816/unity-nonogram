using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Box : MonoBehaviour
{
    public enum State
    {
        None,
        Closed,
        Crossed,
        Open
    }

    public State m_State;
    public int order;
    UIButton button;
    bool isLeftClicked;
    bool isRightClicked;

    void Awake()
    {
        button = gameObject.GetComponent<UIButton>();
        isLeftClicked = false;
        isRightClicked = false;
        m_State = State.Closed;
        order = -1;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            isLeftClicked = true;
            isRightClicked = false;
        }
        else if (Input.GetMouseButton(1))
        {
            isLeftClicked = false;
            isRightClicked = true;
        }
        else
        {
            isLeftClicked = false;
            isRightClicked = false;
            Board.Instance.isDragStarted = false;
            Board.Instance.dragStartDestState = State.None;
            Board.Instance.UpdateCurrentBoardStatus();
            Board.Instance.CrossDragBoxes();
        }

        if (isLeftClicked | isRightClicked)
        {
            Board.Instance.dragBoxOrderList.Add(order);
        }
    }

    public void OnClickBox()
    {
        if (m_State == Board.Instance.dragStartDestState)
            return;

        if (isLeftClicked)
        {
            if (m_State == State.Closed)
                OpenBox();
            else if (m_State == State.Open || m_State == State.Crossed)
                CloseBox();
        }
        else if (isRightClicked)
        {
            if (m_State == State.Closed)
                CrossBox();
            else if (m_State == State.Open || m_State == State.Crossed)
                CloseBox();
        }
    }

    void OpenBox()
    {
        m_State = State.Open;
        button.normalSprite = "dot";
        button.hoverSprite = "dot";
        button.pressedSprite = "dot";
        button.defaultColor = Color.black;
        Board.Instance.UpdateCurrentBoardStatus();
        if (!Board.Instance.isDragStarted)
        {
            Board.Instance.isDragStarted = true;
            Board.Instance.dragStartDestState = State.Open;
        }
    }

    public void CrossBox()
    {
        m_State = State.Crossed;
        button.normalSprite = "cross";
        button.hoverSprite = "cross";
        button.pressedSprite = "cross";
        button.defaultColor = Color.white;
        if (!Board.Instance.isDragStarted)
        {
            Board.Instance.isDragStarted = true;
            Board.Instance.dragStartDestState = State.Crossed;
        }
    }

    public void CloseBox()
    {
        m_State = State.Closed;
        button.normalSprite = "dot";
        button.hoverSprite = "dot";
        button.pressedSprite = "dot";
        button.defaultColor = Color.white;
        Board.Instance.UpdateCurrentBoardStatus();
        if (!Board.Instance.isDragStarted)
        {
            Board.Instance.isDragStarted = true;
            Board.Instance.dragStartDestState = State.Closed;
        }
    }
}
