using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class PlayerManager : MonoBehaviour {

    [Serializable]
    public class OwnGrid
    {
        public int[,] gridList = new int[10, 10];
        public List<bool> usageList = new List<bool>();
        public List<GameObject> gridObjectList = new List<GameObject>();
    }

    [Serializable]
    public class ShipInfo
    {
        public int length;
        public bool placed;
        public bool sunk;
        public List<int> holdingGridCells = new List<int>();
        public List<bool> damaged = new List<bool>();
    }

    [Serializable]
    public class ShipHoldingInfo
    {
        public bool isHolding;
        public GameObject selectedShip;
        public PlacementShip selectedShipComponent;
        public int placementDir;
        public int shipID;
    }

    [Serializable]
    public class HitInfo
    {
        public GameObject[] hitImages;
    }

    public AI_GridManager ai_GridManager;
    public OwnGrid ownGrid = new OwnGrid();
    public ShipHoldingInfo shipHoldingInfo = new ShipHoldingInfo();
    public HitInfo hitInfo = new HitInfo();

    public List<ShipInfo> ships = new List<ShipInfo>();
    public List<Color> gridColors = new List<Color>();

    private List<int> tempSave = new List<int>();

    public Game_Manager gameManager;
    public OptionsMenu optionsMenu;
    public AudioManager audioManager;
    private bool forceStop;

    private void Update()
    {
        CheckRotation();
        CheckIfPaused();
    }

    private void CheckRotation()
    {
        if (shipHoldingInfo.isHolding)
        {
            if (Input.GetButtonDown("Fire2"))
            {
                if(shipHoldingInfo.placementDir == 0)
                {
                    shipHoldingInfo.placementDir = 1;
                    shipHoldingInfo.selectedShip.transform.localEulerAngles = new Vector3(0, 0, 90);
                }
                else
                {
                    shipHoldingInfo.placementDir = 0;
                    shipHoldingInfo.selectedShip.transform.localEulerAngles = new Vector3(0, 0, 0);
                }
            }
        }
    }

    private void CheckIfPaused()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            optionsMenu.ShowMenu();
        }
    }

    public void SetUpButton(GameObject b, int i)
    {
        b.GetComponent<Button>().onClick.AddListener(delegate { CheckShipPlacement(i); });
    }

    public void CheckShipPlacement(int i)
    {
        if (shipHoldingInfo.isHolding)
        {
            if (!ownGrid.usageList[i])
            {
                int checkRange = shipHoldingInfo.selectedShipComponent.length;
                int gridX;
                int gridY;
                tempSave.Clear();
                forceStop = false;
                if(i < 10)
                {
                    gridX = i;
                    gridY = 0;
                }
                else
                {
                    gridX = int.Parse(i.ToString().Substring(1, 1));
                    gridY = int.Parse(i.ToString().Substring(0, 1));

                }
                for (int r = 0; r < checkRange; r++)
                {
                    int calculatingInt;
                    if(shipHoldingInfo.placementDir == 0)
                    {
                        calculatingInt = gridX;
                    }
                    else
                    {
                        calculatingInt = gridY;
                    }
                    int tempGrid = calculatingInt += r;
                    if(tempGrid < 10)
                    {
                        int tempIndex;
                        if (shipHoldingInfo.placementDir == 0)
                        {
                            tempIndex = ownGrid.gridList[tempGrid, gridY];
                        }
                        else
                        {
                            tempIndex = ownGrid.gridList[gridX, tempGrid];
                        }
                        if (!ownGrid.usageList[tempIndex])
                        {
                            tempSave.Add(tempIndex);
                            if (r == checkRange - 1)
                            {
                                if (forceStop)
                                {
                                    return;
                                }
                                for (int t1 = 0; t1 < tempSave.Count; t1++)
                                {
                                    ownGrid.usageList[tempSave[t1]] = true;
                                   // ownGrid.gridObjectList[tempSave[t1]].GetComponent<Image>().color = gridColors[0];
                                    ships[shipHoldingInfo.shipID].holdingGridCells.Add(tempSave[t1]); 
                                }
                                shipHoldingInfo.selectedShip.transform.position = ownGrid.gridObjectList[i].transform.position;
                                ships[shipHoldingInfo.shipID].placed = true;
                                shipHoldingInfo.isHolding = false;
                                shipHoldingInfo.selectedShipComponent.isSelected = false;
                                gameManager.objectives.placedShips++;
                                gameManager.CheckObjective();
                            }
                        }
                        else
                        {
                            forceStop = true;
                        }
                    }
                }
            }
        }
    }

    public void SelectShip(GameObject g)
    {
        if (!shipHoldingInfo.isHolding)
        {
            shipHoldingInfo.selectedShip = g;
            shipHoldingInfo.selectedShipComponent = g.GetComponent<PlacementShip>();
            shipHoldingInfo.selectedShipComponent.isSelected = true;
            shipHoldingInfo.isHolding = true;
            g.GetComponent<Image>().raycastTarget = false;
            shipHoldingInfo.placementDir = 0;
            shipHoldingInfo.shipID = shipHoldingInfo.selectedShipComponent.shipID;

            ships[shipHoldingInfo.shipID].length = shipHoldingInfo.selectedShipComponent.length;

            for (int d = 0; d < shipHoldingInfo.selectedShipComponent.length; d++)
            {
                ships[shipHoldingInfo.shipID].damaged.Add(false);
            }
        }
    }

    public void TakeHit(int i, int x, int y)
    {
        if (ownGrid.usageList[i])
        {
            for (int i2 = 0; i2 < ships.Count; i2++)
            {
                if (ships[i2].holdingGridCells.Contains(i))
                {
                    ships[i2].damaged[ships[i2].holdingGridCells.IndexOf(i)] = true;
                    // ownGrid.gridObjectList[i].GetComponent<Image>().color = gridColors[2];
                    GameObject newHitImage = Instantiate(hitInfo.hitImages[0], ownGrid.gridObjectList[i].transform.position, Quaternion.identity) as GameObject;
                    newHitImage.transform.parent = transform;
                    newHitImage.transform.SetAsLastSibling();
                    if (CheckIfSinking(i2))
                    {
                        print("Ship sunk!");
                        ships[i2].sunk = true;
                        ai_GridManager.ResetTarget();
                        if (CheckIfAllSunk())
                        {
                            print("Enemy Won");
                            optionsMenu.ended = true;
                            optionsMenu.ShowMenu();
                            audioManager.PlaySound(4);
                        }
                    }
                    else
                    {
                        if (ai_GridManager.targetInfo.hasTarget)
                        {
                            ai_GridManager.targetInfo.targetHit = true;
                        }
                        else
                        {
                            ai_GridManager.targetInfo.hasTarget = true;
                            ai_GridManager.targetInfo.targetGridX = x;
                            ai_GridManager.targetInfo.targetGridY = y;
                            ai_GridManager.targetInfo.targetGrid = ai_GridManager.attackInfo.attackGrid;
                        }
                    }
                }
            }
        }
        else
        {
            // ownGrid.gridObjectList[i].GetComponent<Image>().color = gridColors[1];
            GameObject newMissImage = Instantiate(hitInfo.hitImages[1], ownGrid.gridObjectList[i].transform.position, Quaternion.identity) as GameObject;
            newMissImage.transform.parent = transform;
            newMissImage.transform.SetAsLastSibling();
            ai_GridManager.targetInfo.targetHit = false;
        }
        gameManager.objectives.objectiveID = 1;
        gameManager.CheckObjective();
    }

    private bool CheckIfAllSunk()
    {
        for (int i = 0; i < ships.Count; i++)
        {
            if (!ships[i].sunk)
            {
                return false;
            }
        }
        return true;
    }

    private bool CheckIfSinking(int i)
    {
        for (int i2 = 0; i2 < ships[i].damaged.Count; i2++)
        {
            if (!ships[i].damaged[i2])
            {
                return false;
            }
        }
        return true;
    }
}
