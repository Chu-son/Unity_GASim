using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIcontroller : MonoBehaviour {

    public Text generation;
    public string Generation
    {
        set { this.generation.text = string.Format("Generation:{0}", value); }
        get { return this.generation.text; }
    }

    public Text eliteButton;
    public string EliteButtonText
    {
        set { this.eliteButton.text = value; }
        get { return this.eliteButton.text; }
    }

    public enum Menus {ALL,MAINMENU,PARAMETERMENU,LOAD};
    public GameObject menuContainer;
    public void menuReverseActive()
    {
        closeOtherMenu(Menus.MAINMENU);
        menuSetActive(!menuContainer.activeSelf);
    }
    public void menuSetActive(bool isActive)
    {
        if (isActive) Time.timeScale = 0;
        else Time.timeScale = 1;

        menuContainer.SetActive(isActive);
    }

    public GameObject parametersContainer;
    public void parametersUIReverseActive()
    {
        closeOtherMenu(Menus.PARAMETERMENU);
        parametersUISetActive(!parametersContainer.activeSelf);

    }
    public void parametersUISetActive(bool isActive)
    {
        if (isActive) Time.timeScale = 0;
        else Time.timeScale = 1;

        parametersContainer.SetActive(isActive);
    }

    public void closeOtherMenu(Menus menus = Menus.ALL)
    {
        if (menus != Menus.MAINMENU) menuSetActive(false);
        if (menus != Menus.PARAMETERMENU) parametersUISetActive(false);
        if (menus != Menus.LOAD) loadUISetActive(false);
    }

    public GameObject loadUIContainer;
    public GameObject savedDataContainer;
    public void loadUIReverseActive()
    {
        closeOtherMenu(Menus.LOAD);
        loadUISetActive(!loadUIContainer.activeSelf);

    }
    public void loadUISetActive(bool isActive)
    {
        if (isActive)
        {
            Time.timeScale = 0;
            savedDataContainer.GetComponent<LoadDatas>().ListData();
        }
        else Time.timeScale = 1;

        loadUIContainer.SetActive(isActive);
    }

    public Button fastButton;
    public void fastForward()
    {
        if(Time.timeScale == 1)
        {
            fastButton.GetComponent<ButtonTextAccesser>().ButtonText = "Fast";
            Time.timeScale = 3;
        }
        else
        {
            fastButton.GetComponent<ButtonTextAccesser>().ButtonText = "Normal";
            Time.timeScale = 1;
        }
    }
}
