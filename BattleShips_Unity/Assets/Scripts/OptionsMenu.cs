using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OptionsMenu : MonoBehaviour {

    public GameObject menu;
    public bool ended;

    public void ButtonInput(int i)
    {
        SceneManager.LoadScene(i);
    }

    public void ShowMenu()
    {
        transform.SetAsLastSibling();
        if (!menu.activeInHierarchy)
        {
            menu.SetActive(true);
        }
        else
        {
            if (!ended)
            {
                menu.SetActive(false);
            }
        }
    }
}
