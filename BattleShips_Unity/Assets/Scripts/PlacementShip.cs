using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacementShip : MonoBehaviour {

    public int length;
    public bool isSelected;
    public int shipID;

    private void Update()
    {
        FollowMouse();
    }

    void FollowMouse()
    {
        if (isSelected)
        {
            transform.position = Input.mousePosition;
        }
    }
}
