using System.Collections;
using System.IO;
using UnityEngine;


public class GridManager
{

    //Create a hashtable which we can use to map grid coordinates to indecies in a 2d array (This is temporary I am sure there is a better way to do this, probably mathematically)
    //Hashtable GridMap = new Hashtable();
    private int rowCount;
    private int columnCount;

    private char[,] gridMap;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
   


    public (int row, int col) getDimensions()
    {
        return (rowCount, columnCount);

    }

    public char getGridValue(int r, int c)
    {
        return gridMap[r, c];
    }

    public void updateGridValue(int r, int c, char newValue)
    {

        gridMap[r, c] = newValue;
    }

    public void readCSV(string csvName) {


        using (StreamReader reader = new StreamReader(Application.streamingAssetsPath + "/Grids/"+ csvName)) {

        
            string data_String = reader.ReadLine();

            rowCount = (int)(data_String.Split(',')[0][0]-'0');
            columnCount = (int)(data_String.Split(',')[1][0] - '0');

            gridMap = new char[rowCount, columnCount];

            int rowIndex = rowCount - 1;
            int columnIndex = columnCount - 1;

            //Debug.Log("ROW: "+rowIndex);
            //Debug.Log("COL: " + columnIndex);
            //int loopcount = 0;
            //while (!reader.EndOfStream)
            //{
                
                //Debug.Log(dataArray.Length);
                for (int i = rowIndex; i >=0 ; i--) {
                var dataArray = reader.ReadLine().Split(",");
                for (int j = 0; j < dataArray.Length; j++)
                    {

                        //Debug.Log("GRID COORDINATES: " + i + ", " + j);
                        if (dataArray[j] == "0")
                        {
                            gridMap[i, j] = '0';
                        }
                        else
                        {
                            //Debug.Log("ITEM DETECTED");
                            
                            gridMap[i, j] = dataArray[j][0];
                           
                        }
                      
                  
                        //Debug.Log("Coordinates: (" + i + ", "+ j+ "): " + gridMap[i, j]);
                    }
                }
                //loopcount++;
            //}
            
            

        }

        
    }

}
