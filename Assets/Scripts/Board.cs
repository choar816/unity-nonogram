using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Board : MonoBehaviour
{
    private static Board instance = null;
    public static Board Instance
    {
        get
        {
            if (null == instance)
            {
                return null;
            }
            return instance;
        }
    }
    private void Awake()
    {
        if (null == instance)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public Transform Grid_Box;
    public GameObject BoxPrefab;
    public Transform Grid_Number_Col;
    public GameObject NumberColPrefab;
    public Transform Grid_Number_Row;
    public GameObject NumberRowPrefab;
    public GameObject WinPanel;

    List<Box> boxList;
    List<List<int>> rowNumberList;
    List<GameObject> rowNumberObjList;
    List<List<int>> colNumberList;
    List<GameObject> colNumberObjList;

    int[] answerBoard;
    List<int[]> answerCandidateList;
    int[] currentBoard;

    public Box.State dragStartDestState;
    public bool isDragStarted;
    public List<int> dragBoxOrderList;

    public const int BOARD_SIZE = 5;
    const int BOX_SIZE = 60;
    Color COLOR_SOLVED = new Color(0.5f, 0.5f, 0.5f);


    // Start is called before the first frame update
    void Start()
    {
        boxList = new List<Box>();
        rowNumberList = new List<List<int>>();
        colNumberList = new List<List<int>>();
        rowNumberObjList = new List<GameObject>();
        colNumberObjList = new List<GameObject>();
        answerCandidateList = new List<int[]>();
        answerCandidateList.Add(new int[]
        {
            0, 1, 0, 1, 0,
            1, 1, 1, 1, 1,
            1, 1, 1, 1, 1,
            0, 1, 1, 1, 0,
            0, 0, 1, 0, 0,
        });
        answerCandidateList.Add(new int[]
        {
            0, 0, 1, 0, 0,
            0, 1, 1, 1, 0,
            1, 1, 1, 1, 1,
            0, 0, 1, 0, 0,
            0, 0, 1, 0, 0,
        });
        answerCandidateList.Add(new int[]
        {
            0, 0, 1, 0, 0,
            0, 1, 1, 1, 0,
            1, 1, 1, 1, 1,
            0, 1, 1, 1, 0,
            0, 0, 1, 0, 0,
        });
        SetRandomAnswer();
        currentBoard = Enumerable.Repeat(0, BOARD_SIZE * BOARD_SIZE).ToArray();
        WinPanel.SetActive(false);
        dragStartDestState = Box.State.None;
        isDragStarted = false;
        dragBoxOrderList = new List<int>();

        CreateBoxes();
        CreateRowNumbers();
        CreateColNumbers();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void CreateBoxes()
    {
        for (int i=0; i<BOARD_SIZE*BOARD_SIZE; ++i)
        {
            GameObject box = Instantiate(BoxPrefab, Grid_Box);
            box.GetComponent<Box>().order = i;
            boxList.Add(box.GetComponent<Box>());
        }
        Grid_Box.GetComponent<UIGrid>().Reposition();
    }

    void CreateRowNumbers()
    {
        for (int i = 0; i < BOARD_SIZE; ++i)
        {
            List<int> numberList = CalculateRowNumberList(i);
            rowNumberList.Add(numberList);

            GameObject numberObject = Instantiate(NumberRowPrefab, Grid_Number_Row);
            numberObject.GetComponentInChildren<UILabel>().text = string.Join(" ", numberList);
            rowNumberObjList.Add(numberObject);
        }
    }

    void CreateColNumbers()
    {
        for (int i = 0; i < BOARD_SIZE; ++i)
        {
            List<int> numberList = CalculateColNumberList(i);
            colNumberList.Add(numberList);

            GameObject numberObject = Instantiate(NumberColPrefab, Grid_Number_Col);
            numberObject.GetComponentInChildren<UILabel>().text = string.Join(" ", numberList);
            colNumberObjList.Add(numberObject);
        }
        Grid_Number_Col.GetComponent<UIGrid>().Reposition();
    }

    public bool NeedUpdateNumberAtRow(int index)
    {
        List<int> numberList = new List<int>();

        int countOne = 0;
        for (int i = 0; i < BOARD_SIZE; ++i)
        {
            if (boxList[index * BOARD_SIZE + i].m_State == Box.State.Open)
            {
                countOne += 1;
                if (i == BOARD_SIZE - 1)
                    numberList.Add(countOne);
            }
            else
            {
                if (countOne != 0)
                    numberList.Add(countOne);
                countOne = 0;
            }
        }

        if (numberList.Count == 0)
            numberList.Add(0);

        if (numberList.Count != rowNumberList[index].Count)
            return false;
        for (int i=0; i<numberList.Count; ++i)
        {
            if (numberList[i] != rowNumberList[index][i])
                return false;
        }

        return true;
    }

    public bool NeedUpdateNumberAtCol(int index)
    {
        List<int> numberList = new List<int>();

        int countOne = 0;
        for (int i = 0; i < BOARD_SIZE; ++i)
        {
            if (boxList[index + i * BOARD_SIZE].m_State == Box.State.Open)
            {
                countOne += 1;
                if (i == BOARD_SIZE - 1)
                    numberList.Add(countOne);
            }
            else
            {
                if (countOne != 0)
                    numberList.Add(countOne);
                countOne = 0;
            }
        }

        if (numberList.Count == 0)
            numberList.Add(0);

        if (numberList.Count != colNumberList[index].Count)
            return false;
        for (int i = 0; i < numberList.Count; ++i)
        {
            if (numberList[i] != colNumberList[index][i])
                return false;
        }

        return true;
    }

    public void CrossDragBoxes()
    {
        if (dragBoxOrderList.Count == 0)
            return;

        foreach (int order in dragBoxOrderList)
        {
            if (NeedUpdateNumberAtRow(order / BOARD_SIZE))
                CrossEmptyBoxesAtRow(order / BOARD_SIZE);
            if (NeedUpdateNumberAtCol(order % BOARD_SIZE))
                CrossEmptyBoxesAtCol(order % BOARD_SIZE);
        }
        UpdateNumberColor();
        dragBoxOrderList.Clear();
        CheckAnswer();
    }

    // 숫자 확인 및 색상 업데이트
    void UpdateNumberColor()
    {
        for (int i=0; i<BOARD_SIZE; ++i)
        {
            if (NeedUpdateNumberAtRow(i))
            {
                rowNumberObjList[i].GetComponentInChildren<UILabel>().color = COLOR_SOLVED;
            }
            else
            {
                rowNumberObjList[i].GetComponentInChildren<UILabel>().color = Color.white;
            }

            if (NeedUpdateNumberAtCol(i))
            {
                colNumberObjList[i].GetComponentInChildren<UILabel>().color = COLOR_SOLVED;
            }
            else
            {
                colNumberObjList[i].GetComponentInChildren<UILabel>().color = Color.white;
            }
        }
    }

    void CrossEmptyBoxesAtRow(int index)
    {
        for (int i=0; i < BOARD_SIZE; ++i)
        {
            if (boxList[index * BOARD_SIZE + i].m_State == Box.State.Closed)
            {
                boxList[index * BOARD_SIZE + i].CrossBox();
            }
        }
        rowNumberObjList[index].GetComponentInChildren<UILabel>().color = Color.black;
    }

    void CrossEmptyBoxesAtCol(int index)
    {
        for (int i = 0; i < BOARD_SIZE; ++i)
        {
            if (boxList[index + i * BOARD_SIZE].m_State == Box.State.Closed)
            {
                boxList[index + i * BOARD_SIZE].CrossBox();
            }
        }
    }

    public void UpdateCurrentBoardStatus()
    {
        for (int i=0; i<BOARD_SIZE*BOARD_SIZE; ++i)
        {
            if (boxList[i].m_State == Box.State.Open)
                currentBoard[i] = 1;
            else
                currentBoard[i] = 0;
        }
    }

    List<int> CalculateRowNumberList(int index)
    {
        List<int> numberList = new List<int>();

        int countOne = 0;
        for (int i = 0; i < BOARD_SIZE; ++i)
        {
            if (answerBoard[index * BOARD_SIZE + i] == 1)
            {
                countOne += 1;
                if (i == BOARD_SIZE - 1)
                    numberList.Add(countOne);
            }
            else
            {
                if (countOne != 0)
                    numberList.Add(countOne);
                countOne = 0;
            }
        }

        if (numberList.Count == 0)
            numberList.Add(0);
        return numberList;
    }

    List<int> CalculateColNumberList(int index)
    {
        List<int> numberList = new List<int>();

        int countOne = 0;
        for (int i = 0; i < BOARD_SIZE; ++i)
        {
            if (answerBoard[i * BOARD_SIZE + index] == 1)
            {
                countOne += 1;
                if (i == BOARD_SIZE - 1)
                    numberList.Add(countOne);
            }
            else
            {
                if (countOne != 0)
                    numberList.Add(countOne);
                countOne = 0;
            }
        }

        if (numberList.Count == 0)
            numberList.Add(0);
        return numberList;
    }

    public void CheckAnswer()
    {
        for (int i = 0; i < BOARD_SIZE * BOARD_SIZE; ++i)
        {
            if (currentBoard[i] != answerBoard[i])
                return;
        }

        // answer
        WinPanel.SetActive(true);
    }

    public void RestartGame()
    {
        currentBoard = Enumerable.Repeat(0, BOARD_SIZE * BOARD_SIZE).ToArray();
        foreach (Box box in boxList)
        {
            box.CloseBox();
        }
        WinPanel.SetActive(false);
        SetRandomAnswer();
    }

    void SetRandomAnswer()
    {
        answerBoard = answerCandidateList[Random.Range(0, answerCandidateList.Count)];
    }
}
