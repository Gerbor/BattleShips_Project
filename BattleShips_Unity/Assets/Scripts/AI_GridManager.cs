using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class AI_GridManager : MonoBehaviour
{

    [Serializable]
    public class OwnGrid
    {
        public int[,] gridList = new int[10, 10];
        public List<bool> usageList = new List<bool>();
        public List<bool> checkedList = new List<bool>();
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
    public class AttackInfo
    {
        public List<bool> attacked = new List<bool>();
        public bool readyToAttack;
        public int attackGrid;
        public int tempTargetX;
        public int tempTargetY;
        public int attemptingIndex;
        public int hitCount;
    }

    [Serializable]
    public class TargetInfo
    {
        public bool hasTarget;
        public bool targetIsOnX;
        public bool targetIsOnY;
        public bool targetHit;
        public int tryDirection;
        public int targetGridX;
        public int targetGridY;
        public int targetGrid;
        public int searchStep;
        public bool loopBreaker;

        public bool lockX1;
        public bool lockX2;
        public bool lockY1;
        public bool lockY2;
    }

    [Serializable]
    public class HitInfo
    {
        public GameObject[] hitImages;
    }

    public OwnGrid ownGrid = new OwnGrid();
    public AttackInfo attackInfo = new AttackInfo();
    public TargetInfo targetInfo = new TargetInfo();
    public List<ShipInfo> ships = new List<ShipInfo>();
    public List<Color> gridColors = new List<Color>();
    public HitInfo hitInfo = new HitInfo();

    private List<int> tempSave = new List<int>();

    public Game_Manager gameManager;
    public PlayerManager playerManager;
    public OptionsMenu optionsMenu;
    public AudioManager audioManager;

    private void Start()
    {
        targetInfo.searchStep = 1;
    }

    public void PlaceShips()
    {
        for (int i = 0; i < ships.Count; i++)
        {
            SetDamageList(i);
            while (!ships[i].placed)
            {
                tempSave.Clear();
                int selectedGridX = randomInt(0, 10);
                int selectedGridY = randomInt(0, 10);
                int gridIndex = ownGrid.gridList[selectedGridX, selectedGridY];
                bool forceStop = false;
                if (!ownGrid.usageList[gridIndex])
                {
                    int placementDirection = randomInt(0, 2); //0 = >, 1 = ^
                    int searchRange = ships[i].length;
                    searchRange++;
                    int tempGrid;
                    for (int r1 = 1; r1 < searchRange; r1++)
                    {
                        int calculatingXorY;
                        if (placementDirection == 0)
                        {
                            calculatingXorY = selectedGridX;
                        }
                        else
                        {
                            calculatingXorY = selectedGridY;
                        }
                        tempGrid = calculatingXorY += r1;
                        if (tempGrid < 10)
                        {
                            int tempIndex;
                            if(placementDirection == 0)
                            {
                                tempIndex = ownGrid.gridList[tempGrid, selectedGridY];
                            }
                            else
                            {
                                tempIndex = ownGrid.gridList[selectedGridX, tempGrid];
                            }
                            if (!ownGrid.usageList[tempIndex])
                            {
                                tempSave.Add(tempIndex);
                                if (r1 == searchRange - 1)
                                {
                                    print("Do placement");
                                    for (int t1 = 0; t1 < tempSave.Count; t1++)
                                    {
                                        if (!forceStop)
                                        {
                                            ownGrid.usageList[tempSave[t1]] = true;
                                       //     ownGrid.gridObjectList[tempSave[t1]].GetComponent<Image>().color = gridColors[1];
                                            ships[i].holdingGridCells.Add(tempSave[t1]);
                                            ships[i].placed = true;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                print("The generated spot is allready filled, will try again");
                                forceStop = true;
                            }
                        }
                        else
                        {
                            forceStop = true;
                            print("Generated spot cant be used, start retry method");

                        }
                    }
                }
                else
                {
                    print("The generated spot is allready filled, will try again");
                }
            }
        }
        this.GetComponent<ShipPosFixer>().DoCheck(0);
    }

    private void SetDamageList(int s)
    {
        for (int i = 0; i < ships[s].length; i++)
        {
            ships[s].damaged.Add(false);
        }
    }

    public void SetUpButton(GameObject b, int i)
    {
        b.GetComponent<Button>().onClick.AddListener(delegate { CheckHit(i); } );
    }

    public void CheckHit(int i)
    {
        if (gameManager.turns.isPlayersTurn)
        {
            if (!ownGrid.checkedList[i])
            {
                ownGrid.checkedList[i] = true;
                if (ownGrid.usageList[i])
                {
                    print("Hit");
                    //   ownGrid.gridObjectList[i].GetComponent<Image>().color = gridColors[3];
                    GameObject newHitImage = Instantiate(hitInfo.hitImages[0], ownGrid.gridObjectList[i].transform.position, Quaternion.identity) as GameObject;
                    newHitImage.transform.parent = transform;
                    newHitImage.transform.SetAsLastSibling();
                    for (int i2 = 0; i2 < ships.Count; i2++)
                    {
                        if (ships[i2].holdingGridCells.Contains(i))
                        {
                            ships[i2].damaged[ships[i2].holdingGridCells.IndexOf(i)] = true;
                            if (CheckIfSinking(i2))
                            {
                                print("Ship sunk!");
                                ships[i2].sunk = true;
                                if (CheckIfAllSunk())
                                {
                                    print("Player Won");
                                    optionsMenu.ended = true;
                                    optionsMenu.ShowMenu();
                                    audioManager.PlaySound(3);
                                }
                                else
                                {
                                    audioManager.PlaySound(2);
                                }
                            }
                            else
                            {
                                audioManager.PlaySound(0);
                            }
                        }
                    }
                }
                else
                {
                    //    print("Miss");
                    //    print(i);
                    //  ownGrid.gridObjectList[i].GetComponent<Image>().color = gridColors[2];
                    audioManager.PlaySound(1);
                    GameObject newMissImage = Instantiate(hitInfo.hitImages[1], ownGrid.gridObjectList[i].transform.position, Quaternion.identity) as GameObject;
                    newMissImage.transform.parent = transform;
                    newMissImage.transform.SetAsLastSibling();
                }
                gameManager.objectives.objectiveID = 2;
                gameManager.CheckObjective();
            }
        }
    }

    public void ThinkAboutAttack()
    {
        int antiLoop = 0;
        while (!attackInfo.readyToAttack)
        {
            if (!targetInfo.hasTarget)
            {
                attackInfo.tempTargetX = randomInt(0, 10);
                attackInfo.tempTargetY = randomInt(0, 10);
                int gridIndex = ownGrid.gridList[attackInfo.tempTargetX, attackInfo.tempTargetY];
                if (!attackInfo.attacked[gridIndex])
                {
                    attackInfo.attackGrid = gridIndex;
                    attackInfo.readyToAttack = true;
                }
            }
            else
            {
                print("Attempting attack near previous location");
                int directionCalculator = 0;
                int targetGrid = 0;
                if (targetInfo.targetHit)
                {
                    if (targetInfo.tryDirection == 0 || targetInfo.tryDirection == 1)
                    {
                        targetInfo.targetIsOnX = true;
                    }
                    else
                    {
                        targetInfo.targetIsOnY = true;
                    }
                }
                else
                {
                    if (targetInfo.targetIsOnX)
                    {
                        if(targetInfo.tryDirection == 0)
                        {
                            targetInfo.lockX1 = true;
                        }
                        else
                        {
                            targetInfo.lockX2 = true;
                        }
                    }
                    else if (targetInfo.targetIsOnY)
                    {
                        if(targetInfo.tryDirection == 2)
                        {
                            targetInfo.lockY1 = true;
                        }
                        else
                        {
                            targetInfo.lockY2 = true;
                        }
                    }
                    if(targetInfo.targetIsOnX && targetInfo.lockX1 && targetInfo.lockX2)
                    {
                        ResetTarget();
                    }
                    else if (targetInfo.targetIsOnY && targetInfo.lockY1 && targetInfo.lockY2)
                    {
                        ResetTarget();
                    }

                }
                if (targetInfo.tryDirection == 0)
                {
                    directionCalculator = targetInfo.targetGridX;
                    directionCalculator += targetInfo.searchStep;
                    if(directionCalculator < 10)
                    {
                        targetGrid = ownGrid.gridList[directionCalculator, targetInfo.targetGridY];
                        if (!attackInfo.attacked[targetGrid])
                        {
                            attackInfo.attackGrid = targetGrid;
                            attackInfo.readyToAttack = true;
                     //       targetInfo.targetIsOnX = true;
                        }
                        else
                        {
                            if (targetInfo.targetIsOnX)
                            {
                                if (targetInfo.lockX2)
                                {
                                    targetInfo.tryDirection = 0;
                                    targetInfo.searchStep++;
                                }
                                else
                                {
                                    targetInfo.tryDirection++;
                                }
                            }
                            else
                            {
                                targetInfo.tryDirection++;
                            }
                        }
                    }
                    else
                    {
                        targetInfo.tryDirection++;
                    }
                }
                if(targetInfo.tryDirection == 1)
                {
                    directionCalculator = targetInfo.targetGridX;
                    directionCalculator -= targetInfo.searchStep;
                    if (directionCalculator > 0)
                    {
                        targetGrid = ownGrid.gridList[directionCalculator, targetInfo.targetGridY];
                        if (!attackInfo.attacked[targetGrid])
                        {
                            attackInfo.attackGrid = targetGrid;
                            attackInfo.readyToAttack = true;
                 //           targetInfo.targetIsOnX = true;
                        }
                        else
                        {
                            if (!targetInfo.targetIsOnX)
                            {
                                targetInfo.tryDirection++;
                            }
                            else
                            {
                                targetInfo.searchStep++;
                                if (targetInfo.lockX1)
                                {
                                    targetInfo.tryDirection = 1;
                                }
                                else
                                {
                                    targetInfo.tryDirection = 0;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (!targetInfo.targetIsOnX)
                        {
                            targetInfo.tryDirection++;
                        }
                        else
                        {
                            targetInfo.searchStep++;
                            targetInfo.tryDirection = 0;
                        }
                    }
                }
                if(targetInfo.tryDirection == 2)
                {
                    directionCalculator = targetInfo.targetGridY;
                    directionCalculator += targetInfo.searchStep;
                    if (directionCalculator < 10)
                    {
                        targetGrid = ownGrid.gridList[targetInfo.targetGridX, directionCalculator];
                        if (!attackInfo.attacked[targetGrid])
                        {
                            attackInfo.attackGrid = targetGrid;
                            attackInfo.readyToAttack = true;
            //                targetInfo.targetIsOnY = true;
                        }
                        else
                        {
                            targetInfo.tryDirection++;
                        }
                    }
                    else
                    {
                        targetInfo.tryDirection++;
                    }
                }
                if(targetInfo.tryDirection == 3)
                {
                    directionCalculator = targetInfo.targetGridY;
                    directionCalculator -= targetInfo.searchStep;
                    if (directionCalculator > 0)
                    {
                        targetGrid = ownGrid.gridList[targetInfo.targetGridX, directionCalculator];
                        if (!attackInfo.attacked[targetGrid])
                        {
                            attackInfo.attackGrid = targetGrid;
                            attackInfo.readyToAttack = true;
            //                targetInfo.targetIsOnY = true;
                        }
                        else
                        {
                            targetInfo.searchStep++;
                            if (!targetInfo.targetIsOnY)
                            {
                                targetInfo.tryDirection++;
                            }
                            else
                            {
                                if (targetInfo.lockY2)
                                {
                                    targetInfo.tryDirection = 3;
                                    targetInfo.searchStep++;
                                }
                                else
                                {
                                    targetInfo.tryDirection = 2;
                                }
                            }
                        }
                    }
                    else
                    {
                        targetInfo.searchStep++;
                        if (!targetInfo.targetIsOnY)
                        {
                            targetInfo.tryDirection++;
                        }
                        else
                        {
                            if (targetInfo.lockY1)
                            {
                                targetInfo.searchStep++;
                                ResetTarget();
                            }
                            else
                            {
                                targetInfo.tryDirection = 2;
                            }
                        }
                    }
                }
                antiLoop++;
                if(antiLoop > 15)
                {
                    print("BROKE THE LOOP");
                    ResetTarget();
                }
            } 
        }
        StartCoroutine(AttackDelay(1.5f));
    }

    public void ResetTarget()
    {
        targetInfo.hasTarget = false;
        targetInfo.targetIsOnX = false;
        targetInfo.targetIsOnY = false;
        targetInfo.tryDirection = 0;
        targetInfo.searchStep = 1;
        targetInfo.lockX1 = false;
        targetInfo.lockX2 = false;
        targetInfo.lockY1 = false;
        targetInfo.lockY2 = false;
    }

    public IEnumerator AttackDelay(float f)
    {
        print("Starting attack delay");
        yield return new WaitForSeconds(f);
        LaunchAttack();
    }

    public void LaunchAttack()
    {
        print("Launching Attack");
        if (!targetInfo.hasTarget)
        {
            playerManager.TakeHit(attackInfo.attackGrid, attackInfo.tempTargetX, attackInfo.tempTargetY);
        }
        else
        {
            playerManager.TakeHit(attackInfo.attackGrid, targetInfo.targetGridX, targetInfo.targetGridY);
        }
        attackInfo.attacked[attackInfo.attackGrid] = true;
        attackInfo.readyToAttack = false;
        attackInfo.hitCount++;
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

    private int randomInt(int min, int max)
    {
        int newInt = UnityEngine.Random.Range(min, max);
        return newInt;
    }
}
