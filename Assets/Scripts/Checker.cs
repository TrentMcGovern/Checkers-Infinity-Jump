using UnityEngine;

public class Checker : MonoBehaviour
{
    private bool isEnemy;

    public int originalRow = -1;
    public int originalCol = -1;  

    [SerializeField]
    private bool isKing;
    public bool IsKing
    {
        get { return isKing; }
        set { isKing = value; gameObject.GetComponent<Renderer>().material.color = Color.blue; }
    }

    /// <summary>
    /// Sets the checker to be a checker the player has control over
    /// </summary>
    public void turnPlayer()
    {
        isEnemy = false;
    }

    /// <summary>
    /// Sets the checker to be a checker the player has to capture
    /// </summary>
    public void turnEnemy()
    {
        isEnemy = true;
        gameObject.GetComponent<Renderer>().material.color = Color.red;
    }


    public bool getIsPlayer()
    {
        return !isEnemy;
    }

    public bool getIsEnemy()
    {
        return isEnemy;
    }

    public void setCoordinates(int row, int col)
    {
       

        originalRow = row; originalCol = col;
    }

 
}
