using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Game_Manager : MonoBehaviour {

    [Serializable]
    public class Objectives
    {
        public int objectiveID;
        public Text objectiveText;
        public int placedShips;
        public int totalShips;
    }

    [Serializable]
    public class Links
    {
        public PlayerManager playerManager;
        public AI_GridManager ai_Manager;
    }

    public class Turns
    {
        public bool isPlayersTurn;
    }

    public Objectives objectives = new Objectives();
    public Links links = new Links();
    public Turns turns = new Turns();

    private void Start()
    {
        objectives.totalShips = links.playerManager.ships.Count;
        CheckObjective();
    }

    public void CheckObjective()
    {
        switch (objectives.objectiveID)
        {
            case 0:
                objectives.objectiveText.text = "Place your ships " + objectives.placedShips + "/" + objectives.totalShips;
                if(objectives.placedShips >= objectives.totalShips)
                {
                    objectives.objectiveID++;
                    CheckObjective();
                }
                break;
            case 1:
                turns.isPlayersTurn = true;
                objectives.objectiveText.text = "It's your turn!";
                break;
            case 2:
                turns.isPlayersTurn = false;
                objectives.objectiveText.text = "It's the enemies turn!";
                links.ai_Manager.ThinkAboutAttack();
                break;
        }
    }
}
