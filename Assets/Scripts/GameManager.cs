using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class GameManager : MonoBehaviour
{
    //Bool that tells us if we are hovering over the board, and determining whether or not we can move the piece
    private bool rayBoard;

    private bool canMove;

    [SerializeField]
    private GameObject cellIndicator;

    private GridManager gridManager;

    private int rowCount;
    private int columnCount;

    //Array that contains all the checkers the player has at their disposal
    [SerializeField]
    public GameObject[] PlayerCheckers;

    //The checker the player currently wants to move
    [SerializeField] private GameObject activeChecker;

    [SerializeField]
    private Grid boardGrid;

    [SerializeField]
    private Camera gameCamera;
    [Tooltip("The position of the mouse converted to coordinates in game at any given point (z value is always set to the value of gameCamera.nearClipPlane")]
    private Vector3 mouseWorldPosition;

    [Tooltip("The primary plane representing the board")]
    [SerializeField]
    private LayerMask boardPlaneLayer;

    public Vector3 calculateWorldMousePosition()
    {

        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = gameCamera.nearClipPlane;
        mouseWorldPosition = gameCamera.ScreenToWorldPoint(mousePosition);
        Ray mouseRay = gameCamera.ScreenPointToRay(mousePosition);

        RaycastHit hit;

        if (Physics.Raycast(mouseRay,out hit, 200, boardPlaneLayer))
        {
            mouseWorldPosition = hit.point;
            rayBoard = true;
        }
        else
        {
            mouseWorldPosition = new Vector3 (50, -10, 50);
            rayBoard = false;
        }
        
        

        return mouseWorldPosition;
    }




    void Start()
    {
        gridManager = new GridManager();
        gridManager.readCSV("CheckerGrid.csv");
        rowCount = gridManager.getDimensions().row;
        columnCount = gridManager.getDimensions().col;
        createBoard();
    }



    void createBoard()
    {
        for( int i = 0; i < rowCount; i++ )
        {
            for( int j = 0; j < columnCount; j++)
            {
               char gridItem = gridManager.getGridValue(i, j);
                Debug.Log(gridItem);
                if(gridItem == 'P')
                {
                    Debug.Log("P");
                    PlayerCheckers[0].transform.position = new Vector3((j+1)*10+5, 0, (i+1)*10+5);
                    Debug.Log(PlayerCheckers[0].transform.position);
                    
                }
                if (gridItem == 'X')
                {
                    Debug.Log("X");
                    PlayerCheckers[1].transform.position = new Vector3((j + 1) * 10 + 5, 0, (i + 1) * 10 + 5);
                    Debug.Log(PlayerCheckers[1].transform.position);

                }

            }

        }


    }

    private bool checkMoveLegality(int playerRow, int playerCol, int targetRow, int targetCol, bool kingStatus)
    {
        //If the chosen checker is not a king, then we can immediately flag it as invalid if the chosen spot is behind
        if( !kingStatus && targetRow < playerRow) return false;
            Debug.Log (targetRow + "  " +  targetCol + " TEST: " + (playerRow - 1) % 8);  
       
            if( targetCol==playerCol+1 && targetRow == playerRow + 1 && gridManager.getGridValue(targetRow, targetCol) == '\0') {
                return true;
            }
            if (targetCol == playerCol + 1 && targetRow == playerRow - 1 && gridManager.getGridValue(targetRow, targetCol) == '\0')
            {
                return true;
            }
            if (targetCol == playerCol - 1 && targetRow == playerRow + 1 && gridManager.getGridValue(targetRow, targetCol) == '\0')
            {
                return true;
            }
            if (targetCol == playerCol - 1 && targetRow == playerRow - 1 && gridManager.getGridValue(targetRow, targetCol) == '\0')
            {
                return true;
            }


        if (targetCol == playerCol + 2 && targetRow == playerRow + 2 && playerCol != 7 && playerRow != 7)
        {
            if(gridManager.getGridValue(playerRow + 1, playerCol + 1) == 'X') return true;
        }
        if (targetCol == playerCol + 2 && targetRow == playerRow - 2 && playerCol != 7 && playerRow != 0)
        {
            if (gridManager.getGridValue(playerRow - 1, playerCol + 1) == 'X') return true;
           
        }
        if (targetCol == playerCol - 2 && targetRow == playerRow + 2 && playerCol != 0 && playerRow != 7)
        {
            if (gridManager.getGridValue(playerRow + 1, playerCol - 1) == 'X') return true;
          
        }
        if (targetCol == playerCol - 2 && targetRow == playerRow - 2 && playerCol != 0 && playerRow != 0)
        {
            if (gridManager.getGridValue(playerRow - 1, playerCol - 1) == 'X') return true;
        }





        return false;
    }


    private int boardToGrid(float coordinate)
    {

        coordinate = ((int)coordinate - 5) / 10 - 1 ;
   

        return (int)coordinate;
    }


    // Update is called once per frame
    void Update()
    {

        if (Input.GetMouseButtonDown(0))
        {
            //PlayerCheckers[0].transform.position = new Vector3(cellIndicator.transform.position.x + 5f, cellIndicator.transform.position.y, cellIndicator.transform.position.z + 5f);
            Debug.Log("BOARD COORDINATES: " + boardGrid.GetCellCenterWorld(boardGrid.WorldToCell(cellIndicator.transform.position)));

           
        }

        if (Input.GetMouseButtonDown(0) && rayBoard && canMove)
        {
            PlayerCheckers[0].transform.position = new Vector3(cellIndicator.transform.position.x + 5f, cellIndicator.transform.position.y, cellIndicator.transform.position.z + 5f);
            //Debug.Log(boardGrid.WorldToCell(PlayerCheckers[0].transform.position));
            //Debug.Log("GAMER");
        }


        Vector3 checkerPosition = calculateWorldMousePosition();
        //this.gameObject.transform.position = checkerPosition;
        Vector3 temp = boardGrid.CellToWorld(Vector3Int.FloorToInt(boardGrid.WorldToCell(checkerPosition)));

        if (temp.x < 0)
        {
            temp.x = temp.x * -1;
            temp.x = (int)(temp.x / 10);
            temp.x = temp.x * 10*-1;
        }
        else
        {
            temp.x = (int)(temp.x / 10);
            temp.x = temp.x * 10;
        }

        if (temp.z < 0)
        {
            temp.z = temp.z * -1;
            temp.z = (int)(temp.z / 10);
            temp.z = temp.z * 10*-1;
        }
        else
        {
            temp.z = (int)(temp.z / 10);
            temp.z = temp.z * 10;
        }

        cellIndicator.transform.position = new Vector3(temp.x, temp.y, temp.z);
      

        if (checkMoveLegality(boardToGrid(PlayerCheckers[0].transform.position.z), boardToGrid(PlayerCheckers[0].transform.position.x),boardToGrid(cellIndicator.transform.position.z+5f), boardToGrid(cellIndicator.transform.position.x+5f), false))
        {
            cellIndicator.transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = Color.green;
            canMove = true;
        }
        else
        {
            cellIndicator.transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = Color.red;
            canMove = false;
        }
    }
}
