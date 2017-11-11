using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class GridCreator : MonoBehaviour {

    [Serializable]
	public class GridInfo
    {
        public float offset;
        public float extraOffset;
        public int xCount = 10;
        public int yCount = 10;
        public GameObject gridSpot;
        public String gridName;
        public GameObject gridMiddlePoint;
    }

    [Serializable]
    public class AI_Info
    {
        public bool isAI;
        public AI_GridManager aI_GridManager;
    }

    [Serializable]
    public class PlayerInfo
    {
        public PlayerManager playerManager;
    }

    private Vector3 useOffset;
    private int gridCount;

    public GridInfo gridInfo = new GridInfo();
    public AI_Info ai_Info = new AI_Info();
    public PlayerInfo playerInfo = new PlayerInfo();

	void Start () {
        BuildGrid();
	}

    void BuildGrid()
    {
        GameObject gridHolder = new GameObject();
        gridHolder.transform.name = gridInfo.gridName;
        gridHolder.transform.parent = transform;
        
        for (int y = 0; y < gridInfo.yCount; y++)
        {
            for (int x = 0; x < gridInfo.xCount; x++)
            {
                useOffset = new Vector3(0, 0, 0);
                useOffset.x = gridInfo.offset * x;
                useOffset.y = gridInfo.offset * y;
                Vector3 spawnPos = gridInfo.gridMiddlePoint.transform.position;
                spawnPos.x += gridInfo.extraOffset;
                spawnPos += useOffset;
                
                GameObject newGridSpot = Instantiate(gridInfo.gridSpot, spawnPos, Quaternion.identity) as GameObject;
                newGridSpot.transform.name = gridCount.ToString();
                newGridSpot.transform.parent = gridHolder.transform;
                if (ai_Info.isAI)
                {
                    newGridSpot.transform.tag = "AI_Grid";
                    ai_Info.aI_GridManager.ownGrid.gridList[x,y] = gridCount;
                    ai_Info.aI_GridManager.ownGrid.gridObjectList[gridCount] = newGridSpot;
                    ai_Info.aI_GridManager.SetUpButton(newGridSpot, gridCount);
                }
                else
                {
                    playerInfo.playerManager.ownGrid.gridList[x, y] = gridCount;
                    playerInfo.playerManager.SetUpButton(newGridSpot, gridCount);
                    playerInfo.playerManager.ownGrid.gridObjectList[gridCount] = newGridSpot;
                }
                gridCount++;
            }
        }
        if (ai_Info.isAI)
        {
            this.GetComponent<AI_GridManager>().PlaceShips();
        }
        else
        {
            this.GetComponent<ShipPosFixer>().DoCheck(1);
        }
    }
}
