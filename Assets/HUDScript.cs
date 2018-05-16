using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class HUDScript : MonoBehaviour
{
    private PlayerBehaviour_1 myPlayer;
    private ItemManagerScript myPlayerItemManager;
    private Text myBigText;

    private Image[] ItemImageStack;
    private Image[] DashImageStack;

    private const string DEAD_PLAYER_STRING = "DEAD.";

    //Need to set these in Unity
    public Sprite TorchSprite;
    public Sprite TrapSprite;
    public Sprite BunnySprite;
    public Sprite BombSprite;
    public Sprite DashSprite;

    // Use this for initialization
    void Start()
    {
        myPlayer = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerBehaviour_1>();
        myPlayerItemManager = myPlayer.gameObject.GetComponentInChildren<ItemManagerScript>();

        //Find my children controls!
        if (GameObject.Find("BigText") != null)
        {
            myBigText = GameObject.Find("BigText").GetComponent<Text>();
            myBigText.rectTransform.localScale = new Vector3(4, 1, 1);
            myBigText.text = "";
        }
        ItemImageStack = new Image[4];

        for (int i = 0; i < ItemImageStack.Length; i++)
        {
            string objName = "Item" + i + "Image";
            if (GameObject.Find(objName) != null)
            {
                ItemImageStack[i] = GameObject.Find(objName).GetComponent<Image>();
            }
        }

        DashImageStack = new Image[4];

        for (int i = 0; i < DashImageStack.Length; i++)
        {
            string objName = "Dash" + i + "Image";
            if (GameObject.Find(objName) != null)
            {
                DashImageStack[i] = GameObject.Find(objName).GetComponent<Image>();
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        UpdateItemHUD();
        UpdatePlayerState();
    }

    private void UpdatePlayerState()
    {
        //TODO: Dead?? For additional lives
        if (myPlayer.IsDead && myBigText.text != DEAD_PLAYER_STRING) {
            myBigText.text = DEAD_PLAYER_STRING;
        } else if (!myPlayer.IsDead && myBigText.text != "") {
            myBigText.text = "";
        }

        //Number of Dashes Available:
        //For now assume we will only have a max of 4 dashes
        for (int i = 0; i < 4; i++) {
            if (myPlayer.AvailableNumberOfDashes > i)
            {
                DashImageStack[i].color = Color.white;
                DashImageStack[i].sprite = DashSprite;

            }
            else {
                DashImageStack[i].color = Color.clear;
                DashImageStack[i].sprite = null;
            }
        }


    }

    private void UpdateItemHUD()
    {
        List<eHUDItemType> itemTypeList = new List<eHUDItemType>();
        foreach (IHoldableObject currItem in myPlayerItemManager.HeldObjects)
        {
            itemTypeList.Add(HUDScript.GetHUDTypeFromIHoldable(currItem));
        }

        SetItemStack(itemTypeList);
    }

    public void SetItemStack(List<eHUDItemType> itemTypeList)
    {
        int lastItemIndex = -1;
        for (int i = 0; i < ItemImageStack.Length; i++)
        {
            //Reset Sizes
            ItemImageStack[i].rectTransform.localScale = new Vector3(1, 1, 1);

            if (itemTypeList.Count > i)
            {
                ItemImageStack[i].color = Color.white;
                if (itemTypeList[i] == eHUDItemType.bomb)
                {
                    ItemImageStack[i].sprite = BombSprite;
                }
                else if (itemTypeList[i] == eHUDItemType.torch)
                {
                    ItemImageStack[i].sprite = TorchSprite;
                }
                else if (itemTypeList[i] == eHUDItemType.trap)
                {
                    ItemImageStack[i].sprite = TrapSprite;
                }
                else {
                    ItemImageStack[i].sprite = BunnySprite;
                     if (itemTypeList[i] == eHUDItemType.bunnyFemale)
                    {
                        ItemImageStack[i].color = Color.red;
                    }
                    else if (itemTypeList[i] == eHUDItemType.bunnyMale)
                    {
                        ItemImageStack[i].color = Color.blue;
                    }
                }
                
                lastItemIndex = i;
            }
            else
            {
                ItemImageStack[i].sprite = null;
                ItemImageStack[i].color = Color.clear;
            }
        }
        //Make the last item (top of stack) a little bigger to show player they will drop this
        if (lastItemIndex >= 0)
        {
            ItemImageStack[lastItemIndex].rectTransform.localScale = new Vector3(1.5f, 1.5f, 1);
        }
    }

    public void SetBigText(string textStr)
    {
        myBigText.text = textStr;
    }

    internal static eHUDItemType GetHUDTypeFromIHoldable(IHoldableObject item)
    {
        if (item.TypeOfItem == eItemType.bomb)
        {
            return eHUDItemType.bomb;
        }
        else if (item.TypeOfItem == eItemType.trap)
        {
            return eHUDItemType.trap;
        }
        else if (item.TypeOfItem == eItemType.torch)
        {
            return eHUDItemType.torch;
        }
        else
        {
            //Most be a bunny at this point.
            BunnyBehaviour asBunny = (BunnyBehaviour)item;
            if (asBunny.Gender == BunnyBehaviour.eBunnyGender.female)
            {
                return eHUDItemType.bunnyFemale;
            }
            else if (asBunny.Gender == BunnyBehaviour.eBunnyGender.male)
            {
                return eHUDItemType.bunnyMale;
            }
            return eHUDItemType.bunny;
        }
    }

}
