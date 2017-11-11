using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipPosFixer : MonoBehaviour {

    public bool[] checks;
    public GameObject ships;

    public void DoCheck(int i)
    {
        checks[i] = true;
        if (CheckIfReady())
        {
            ships.transform.SetAsLastSibling();
        }
    }

    bool CheckIfReady()
    {
        for (int i = 0; i < checks.Length; i++)
        {
            if (!checks[i])
            {
                return false;
            }
        }
        return true;
    }
}
