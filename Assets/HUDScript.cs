using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class HUDScript : MonoBehaviour
{

    private Text myBigText;
    //private Text myLittleText;
    private Image[] ItemImageStack;
    private Image myStaminaBar;
    private Image mySecondWindBar;

    //Need to set these in Unity
    public Sprite TorchSprite;
    public Sprite TrapSprite;
    public Sprite BunnySprite;
    public Sprite BombSprite;

    // Use this for initialization
    void Start()
    {

        //Find my children controls!
        if (GameObject.Find("BigText") != null)
        {
            myBigText = GameObject.Find("BigText").GetComponent<Text>();
            myBigText.rectTransform.localScale = new Vector3(4, 1, 1);
            myBigText.text = "";
        }
        //if (GameObject.Find("LittleText") != null)
        //{
        //    myLittleText = GameObject.Find("LittleText").GetComponent<Text>();
        //}
        ItemImageStack = new Image[4];

        for (int i = 0; i < ItemImageStack.Length; i++)
        {
            string objName = "Item" + i + "Image";
            if (GameObject.Find(objName) != null)
            {
                ItemImageStack[i] = GameObject.Find(objName).GetComponent<Image>();
            }
        }

    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// Put in the fraction of currentStamina / MAX Stamina
    /// </summary>
    /// <param name="fraction"></param>
    public void SetStaminaBar(float fraction)
    {
        myStaminaBar.rectTransform.localScale = new Vector3(fraction, 1, 1);
    }

    public void SetSecondWindBar(float fraction)
    {
        mySecondWindBar.rectTransform.localScale = new Vector3(fraction, 1, 1);
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
