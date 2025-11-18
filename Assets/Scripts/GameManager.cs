using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    //Bool that tells us if we are hovering over the board, and determining whether or not we can move the piece
    private bool rayBoard;

    [SerializeField]
    private GameObject cellIndicator;

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
            mouseWorldPosition = new Vector3 (-50, -10, -50);
            rayBoard = false;
        }
        
        

        return mouseWorldPosition;
    }




    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {


        if(Input.GetMouseButtonDown(0) && rayBoard)
        {
            gameObject.transform.position = new Vector3(cellIndicator.transform.position.x + 5f, cellIndicator.transform.position.y, cellIndicator.transform.position.z + 5f);
            Debug.Log("GAMER");
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

        cellIndicator.transform.position = new Vector3(temp.x,.1f, temp.z);

    }
}
