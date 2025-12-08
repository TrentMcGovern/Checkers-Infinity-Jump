using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using NUnit.Framework.Constraints;

public class GameManager : MonoBehaviour
{

    //int to track the player health
    public int health;
    public TMP_Text healthText;
    //int to track the player's remaining turns
    public int turns;
    public TMP_Text turnsText;

    //Bool that tells us if we are hovering over the board, and determining whether or not we can move the piece
    private bool rayBoard;

    //bool that tells us whether the position the player is attempting to move to (assuming they are) is valid, and 
    private bool canMove;

    //Bool telling us whether or not we can teleoprt
    private bool canTeleport = false;

    //Plane that indicates what cell the player is hovering over
    [SerializeField]
    private GameObject cellIndicator;
    
    //Empty game object containing a script that manages all changes/handles reading the 2d array which represents the state of the board
    private GridManager gridManager;

    //Determines the size of the board (scalable boards not yet implemented)
    private int rowCount;
    private int columnCount;

    //The count of power up types - each power up will be mapped to an index in this array that contians the number of power ups
    public int powerUpTypeCount=0;
    private int[] powerupCounts;
    public Button[] powerupButtons;
    
   
   
    private Dictionary<string, Checker> playerCheckers;
    private Dictionary<string, Checker> enemyCheckers;
    private Dictionary<string, GameObject> fireHazards;
    //Assuming when attempting to move we are primed to hop over another piece, this string will tell us what item we hopped over
    private string hoppedCoordinate = "";

    [SerializeField]
    public Checker checkerPrefab;

    [SerializeField]
    public GameObject hazardPrefab;

    [SerializeField]
    public GameObject fireHazardPrefab;


    //The checker the player currently wants to move
    [SerializeField] private Checker activeChecker;
    private bool checkerIsActive = false;


    //The unity Grid object overlayed on top of the checkerboard object
    [SerializeField]
    private Grid boardGrid;

    [SerializeField]
    private Camera gameCamera;
    [Tooltip("The position of the mouse converted to coordinates in game at any given point (z value is always set to the value of gameCamera.nearClipPlane")]
    private Vector3 mouseWorldPosition;

    [Tooltip("The primary plane representing the board")]
    [SerializeField]
    private LayerMask boardPlaneLayer;

  

    void Start()
    {
        gridManager = new GridManager();
        gridManager.readCSV("CheckerGrid.csv");
        rowCount = gridManager.getDimensions().row;
        columnCount = gridManager.getDimensions().col;
        playerCheckers = new Dictionary<string, Checker>();
        enemyCheckers = new Dictionary<string, Checker>();
        fireHazards = new Dictionary<string, GameObject>(); 
        health = 3;
        turns = 50;
        powerupCounts = new int[powerUpTypeCount];

        healthText.text = "Health: " + health;
        turnsText.text = "Turns: " + turns;

        Debug.Log(playerCheckers.Count);
        createBoard();
        for (int i = 0; i < powerupButtons.Length; i++) { int capturedIterator = i; powerupCounts[capturedIterator] = 1; powerupButtons[capturedIterator].onClick.AddListener(()=>powerUp(capturedIterator)); powerupButtons[capturedIterator].gameObject.SetActive(false); }
    }



    // Update is called once per frame
    void Update()
    {

        if (enemyCheckers.Count == 0) {

            SceneManager.LoadScene(1);
        }

        if (turns == 0 || health == 0)
        {

            SceneManager.LoadScene(2);
        }

        //Press Escape to exit
        if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();

        //If statement that tells you the coordinate you picked (primarily for bug testing)
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("BOARD COORDINATES: " + boardGrid.GetCellCenterWorld(boardGrid.WorldToCell(cellIndicator.transform.position)));
        }


        if(Input.GetKey(KeyCode.Space))
        {
            powerUp(0);
        }


        //If the player clicks on the board, move the checker to that position if this is a valid checker
        if (Input.GetMouseButtonDown(0) && rayBoard && canMove && checkerIsActive)
        {

            
            gridManager.updateGridValue(boardToGrid(activeChecker.transform.position.z), boardToGrid(activeChecker.transform.position.x), '0');
            //Debug.Log("GAMER: " + boardToGrid(cellIndicator.transform.position.z))
;            if (gridManager.getGridValue(boardToGrid(cellIndicator.transform.position.z + 5f), boardToGrid(cellIndicator.transform.position.x + 5f)) == 'H')
            {
                health--;
                healthText.text = "Health: " + health;
                activeChecker.gameObject.transform.position = new Vector3((activeChecker.originalCol + 1) * 10 + 5, 0, (activeChecker.originalRow + 1) * 10 + 5);
            }
            else if (gridManager.getGridValue(boardToGrid(cellIndicator.transform.position.z + 5f), boardToGrid(cellIndicator.transform.position.x + 5f)) == 'F') 
            {
                int row = boardToGrid(cellIndicator.transform.position.z + 5f);
                int col = boardToGrid(cellIndicator.transform.position.x + 5f);

                int rowLowerBound = Math.Max( (row - 4), 0);
                int colLowerBound = Math.Max((col - 4), 0);
                int rowUpperBound = Math.Min((row + 4), rowCount-1);
                int colUpperBound = Math.Min((col + 4), columnCount - 1);

                Debug.Log("Row Lower: " + rowLowerBound + "Col Lower:  " + colLowerBound + "Row Upper: " + rowUpperBound + "Col Upper: " + colUpperBound);
                System.Random random = new System.Random();

                int randRow = random.Next(rowLowerBound, rowUpperBound);
                int randCol = random.Next(colLowerBound, colUpperBound);
                if(randCol %2 == randRow % 2)
                {
                    randCol = randCol == 0 ? randCol+1 : randCol-1;
                }
                Debug.Log("Row: " + randRow + "Col:  " + randCol );
                if (gridManager.getGridValue(randRow, randCol) != '0') 
                {
                    Debug.Log(gridManager.getGridValue(boardToGrid(cellIndicator.transform.position.z + 5f), boardToGrid(cellIndicator.transform.position.x + 5f)));
                    randRow = activeChecker.originalRow ; randCol = activeChecker.originalCol ;
                }

                if (randCol == col + 2 && randRow == row + 2)
                {
                    if (gridManager.getGridValue(row + 1,col + 1) == 'X')  { hoppedCoordinate = (row + 1) + "" + (col + 1) + "";  }
                }
                if (randCol == col + 2 && randRow == row - 2)
                {
                    if (gridManager.getGridValue(row - 1, col + 1) == 'X') { hoppedCoordinate = (row - 1) + "" + (col + 1) + ""; }
                }
                if (randCol == col - 2 && randRow == row + 2)
                {
                    if (gridManager.getGridValue(row + 1, col - 1) == 'X') { hoppedCoordinate = (row + 1) + "" + (col - 1) + ""; }
                }
                if (randCol == col - 2 && randRow == row - 2)
                {
                    if (gridManager.getGridValue(row - 1, col - 1) == 'X') { hoppedCoordinate = (row - 1) + "" + (col - 1) + ""; }
                }

                activeChecker.gameObject.transform.position = new Vector3((randCol + 1) * 10 + 5, 0, (randRow + 1) * 10 + 5);
                gridManager.updateGridValue(boardToGrid(cellIndicator.transform.position.z + 5f), boardToGrid(cellIndicator.transform.position.x + 5f), '0');
                Destroy(fireHazards[boardToGrid(cellIndicator.transform.position.z + 5f)+""+ boardToGrid(cellIndicator.transform.position.x + 5f)+""].gameObject);
                fireHazards.Remove(boardToGrid(cellIndicator.transform.position.z + 5f) + "" + boardToGrid(cellIndicator.transform.position.x + 5f) + "");
            }
            else
            {
                activeChecker.gameObject.transform.position = new Vector3(cellIndicator.transform.position.x + 5f, cellIndicator.transform.position.y, cellIndicator.transform.position.z + 5f);
            }
            gridManager.updateGridValue(boardToGrid(activeChecker.transform.position.z), boardToGrid(activeChecker.transform.position.x), 'P');
            canTeleport = false;
           

            

            //We need to check if the placed checker's position is in the final row, in which case, it needs to be kinged
            //if (boardToGrid(activeChecker.transform.position.z) == rowCount - 1)
          //  {
            //    activeChecker.IsKing = true;
           // }

            if (adjustTurnCount(boardToGrid(activeChecker.transform.position.z), boardToGrid(activeChecker.transform.position.x)))
            {
                turns--;
                turnsText.text = "Turns: " + turns;
            }

            if (hoppedCoordinate != "")
            {
                Debug.Log(hoppedCoordinate);
                gridManager.updateGridValue((int)(hoppedCoordinate[0] - '0'), (int)(hoppedCoordinate[1] - '0'), '0');
                Destroy(enemyCheckers[hoppedCoordinate].gameObject);
                enemyCheckers.Remove(hoppedCoordinate);
                hoppedCoordinate = "";
            }
            //We need to check if the placed checker's position is in the final row, in which case, it needs to be kinged
            if (boardToGrid(activeChecker.transform.position.z) == rowCount-1)
            {
                activeChecker.IsKing = true;
            }
           

           
        }

        //If the player clicks the board when there is no active checker, check if they are clicking on a checker
        if (Input.GetMouseButtonDown(0) && !checkerIsActive)
        {
            
            Vector3 mousePosition = calculateWorldMousePosition();
            Vector3 temp = boardGrid.CellToWorld(Vector3Int.FloorToInt(boardGrid.WorldToCell(mousePosition)));
            if (temp.x < 0)
            {
                temp.x = temp.x * -1;
                temp.x = (int)(temp.x / 10);
                temp.x = temp.x * 10 * -1;
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
                temp.z = temp.z * 10 * -1;
            }
            else
            {
                temp.z = (int)(temp.z / 10);
                temp.z = temp.z * 10;
            }

            char cellInhabitant = gridManager.getGridValue(boardToGrid(temp.z + 5f), boardToGrid(temp.x + 5f));

            if (cellInhabitant == 'P') 
            {  activeChecker = playerCheckers[""+ boardToGrid(temp.z + 5f) + "" + boardToGrid(temp.x + 5f)]; checkerIsActive = true; 
                for (int i = 0; i < powerupButtons.Length; i++) {
                    string buttonText = powerupButtons[i].gameObject.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text;
                    powerupButtons[i].gameObject.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text = buttonText.Split(":")[0] + ": "+ powerupCounts[i] + " Uses Remaining";
                        powerupButtons[i].gameObject.SetActive(true); 
                } 
            }
           

        }


        //This chunk of code will make the cell indicator (the colored square indicating which spot on the board you are hovering over) follow the players mouse
        {

            if (checkerIsActive)
            {
                Vector3 mousePosition = calculateWorldMousePosition();
                Vector3 temp = boardGrid.CellToWorld(Vector3Int.FloorToInt(boardGrid.WorldToCell(mousePosition)));
                if (temp.x < 0)
                {
                    temp.x = temp.x * -1;
                    temp.x = (int)(temp.x / 10);
                    temp.x = temp.x * 10 * -1;
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
                    temp.z = temp.z * 10 * -1;
                }
                else
                {
                    temp.z = (int)(temp.z / 10);
                    temp.z = temp.z * 10;
                }
                cellIndicator.transform.position = new Vector3(temp.x, temp.y, temp.z);
            }
        }
        //___________________________________________________________________________________________________________

        //This if statement will check if the cell the player is hovering over is a cell they can move to, and adjust the indicator color accordingly
        if (checkerIsActive)
        {
            
            if (checkMoveLegality(boardToGrid(activeChecker.gameObject.transform.position.z), boardToGrid(activeChecker.gameObject.transform.position.x), boardToGrid(cellIndicator.transform.position.z + 5f), boardToGrid(cellIndicator.transform.position.x + 5f), activeChecker.IsKing ))
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

    /// <summary>
    /// Convert the position of the mouse on screen to a position in the worldspace
    /// </summary>
    /// <returns></returns>
    public Vector3 calculateWorldMousePosition()
    {

        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = gameCamera.nearClipPlane;
        mouseWorldPosition = gameCamera.ScreenToWorldPoint(mousePosition);
        Ray mouseRay = gameCamera.ScreenPointToRay(mousePosition);

        RaycastHit hit;

        if (Physics.Raycast(mouseRay, out hit, 200, boardPlaneLayer))
        {
            mouseWorldPosition = hit.point;
            rayBoard = true;
        }
        else
        {
            mouseWorldPosition = new Vector3(50, -10, 50);
            rayBoard = false;
        }

        return mouseWorldPosition;
    }


    /// <summary>
    /// Reads the 2d array in the grid manager, and populates the scene with the necessary board/checker placement/obstacles 
    /// </summary>
    void createBoard()
    {
        for (int i = 0; i < rowCount; i++)
        {
            for (int j = 0; j < columnCount; j++)
            {
                char gridItem = gridManager.getGridValue(i, j);
                Debug.Log(gridItem);
                if (gridItem == 'P')
                {
                    Debug.Log("P");
                    playerCheckers.Add((""+i) + ("" + j) , Instantiate(checkerPrefab, new Vector3(0, 0, 0), checkerPrefab.transform.rotation));
                    playerCheckers[("" + i) + ("" + j)].turnPlayer();
                    playerCheckers[("" + i) + ("" + j)].gameObject.transform.position = new Vector3((j + 1) * 10 + 5, 0, (i + 1) * 10 + 5);
                    playerCheckers[("" + i) + ("" + j)].setCoordinates(i, j);


                }
                if (gridItem == 'X')
                {
                    Debug.Log("X");
                    enemyCheckers.Add(("" + i) + ("" + j), Instantiate(checkerPrefab, new Vector3(0, 0, 0), checkerPrefab.transform.rotation));
                    enemyCheckers[("" + i) + ("" + j)].turnEnemy();
                    enemyCheckers[("" + i) + ("" + j)].gameObject.transform.position = new Vector3((j + 1) * 10 + 5, 0, (i + 1) * 10 + 5);
                    enemyCheckers[("" + i) + ("" + j)].setCoordinates(i, j);

                }

                if (gridItem == 'H')
                {
                    Debug.Log("H");
                    Instantiate(hazardPrefab, new Vector3((j + 1) * 10+5 , 5, (i + 1) * 10+5 ), hazardPrefab.transform.rotation);
                }

                if ( gridItem == 'F')
                {
                    Debug.Log("F");
                    fireHazards.Add(("" + i) + ("" + j), Instantiate(fireHazardPrefab, new Vector3((j + 1) * 10 + 5, 5, (i + 1) * 10 + 5), fireHazardPrefab.transform.rotation));
                }

            }

        }


    }


    /// <summary>
    /// Determines whether or not a move is legal
    /// </summary>
    /// <param name="playerRow">The row coordinate (z) of the checker the player is trying to move</param>
    /// <param name="playerCol">The column coordinate (x) of the checker the player is trying to move</param>
    /// <param name="targetRow">The row coordinate (z) of the space the player wants to move to</param>
    /// <param name="targetCol">The row coordinate (x) of the space the player wants to move to</param>
    /// <param name="kingStatus"></param>
    /// <returns></returns>
    private bool checkMoveLegality(int playerRow, int playerCol, int targetRow, int targetCol, bool kingStatus )
    {

    

        hoppedCoordinate = "";

        if (canTeleport && gridManager.getGridValue(targetRow, targetCol) != 'X')
        {
            return true;
        }

        //If the chosen checker is not a king, then we can immediately flag it as invalid if the chosen spot is behind
        if (!kingStatus && targetRow < playerRow) return false;
        //Debug.Log(targetRow + "  " + targetCol + " TEST: " + (playerRow - 1) % 8);

        if (targetCol == playerCol + 1 && targetRow == playerRow + 1 && gridManager.getGridValue(targetRow, targetCol) != 'X')
        {
            return true;
        }
        if (targetCol == playerCol + 1 && targetRow == playerRow - 1 && gridManager.getGridValue(targetRow, targetCol) != 'X')
        {
            return true;
        }
        if (targetCol == playerCol - 1 && targetRow == playerRow + 1 && gridManager.getGridValue(targetRow, targetCol) != 'X')
        {
            return true;
        }
        if (targetCol == playerCol - 1 && targetRow == playerRow - 1 && gridManager.getGridValue(targetRow, targetCol) != 'X')
        {
            return true;
        }


        if (targetCol == playerCol + 2 && targetRow == playerRow + 2 && playerCol != 7 && playerRow != 7 )
        {
            if (gridManager.getGridValue(playerRow + 1, playerCol + 1) == 'X' && gridManager.getGridValue(targetRow, targetCol) != 'X') { hoppedCoordinate = (playerRow +1) + "" + (playerCol +1) + ""  ; return true; }
        }
        if (targetCol == playerCol + 2 && targetRow == playerRow - 2 && playerCol != 7 && playerRow != 0)
        {
            if (gridManager.getGridValue(playerRow - 1, playerCol + 1) == 'X' && gridManager.getGridValue(targetRow, targetCol) != 'X') { hoppedCoordinate = (playerRow - 1) + "" + (playerCol + 1) + ""; return true; }

        }
        if (targetCol == playerCol - 2 && targetRow == playerRow + 2 && playerCol != 0 && playerRow != 7)
        {
            if (gridManager.getGridValue(playerRow + 1, playerCol - 1) == 'X' && gridManager.getGridValue(targetRow, targetCol) != 'X') { hoppedCoordinate = (playerRow + 1) + "" + (playerCol - 1) + ""; return true; }

        }
        if (targetCol == playerCol - 2 && targetRow == playerRow - 2 && playerCol != 0 && playerRow != 0)
        {
            if (gridManager.getGridValue(playerRow - 1, playerCol - 1) == 'X' && gridManager.getGridValue(targetRow, targetCol) != 'X') { hoppedCoordinate = (playerRow - 1) + "" + (playerCol - 1) + ""; return true; }
        }

        return false;
    }

    /// <summary>  
    /// Converts a coordinate on the board to it's corresponding value in the grid managers 2d array
    /// </summary>
    /// <param name="coordinate">The coordinate we want to convert to array</param>
    /// <returns></returns>
    private int boardToGrid(float coordinate)
    {

        coordinate = ((int)coordinate - 5) / 10 - 1;


        return (int)coordinate;
    }

    /// <summary>
    /// Adjusts the number of turns each time the player makes the move (mostly checks to see if the player should lose a turn that move, or if they should keep one)
    /// </summary>
    /// <param name="playerRow">The row the player checker is in</param>
    /// <param name="playerColumn">The column the player checker is in</param>
    private bool adjustTurnCount(int playerRow, int playerCol)
    {
        if (hoppedCoordinate == "") { return true; }

        if (playerCol != 7 && playerRow != 7)
        {
            if (gridManager.getGridValue(playerRow + 1, playerCol + 1) == 'X') { return false; }
        }
        if (playerCol != 7 && playerRow != 0)
        {
            if (gridManager.getGridValue(playerRow - 1, playerCol + 1) == 'X' && activeChecker.IsKing) { return false; }

        }
        if (playerCol != 0 && playerRow != 7)
        {
            if (gridManager.getGridValue(playerRow + 1, playerCol - 1) == 'X') { return false; }

        }
        if (playerCol != 0 && playerRow != 0)
        {
            if (gridManager.getGridValue(playerRow - 1, playerCol - 1) == 'X' && activeChecker.IsKing) { return false; }
        }

        return true;
    }

    private void powerUp(int powerUpIndex)
    {
        if (powerupCounts[powerUpIndex] == 0) { return; }
        Debug.Log(powerUpIndex);
        powerupCounts[powerUpIndex]--;
        string buttonText = powerupButtons[powerUpIndex].gameObject.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text;
        powerupButtons[powerUpIndex].gameObject.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text = buttonText.Split(":")[0] + ": " + powerupCounts[powerUpIndex] + " Uses Remaining";
        switch (powerUpIndex)
        {
            case 0:
                activeChecker.IsKing = true; break;
                
            case 1:
                canTeleport = true; break;

        }
    }

}
