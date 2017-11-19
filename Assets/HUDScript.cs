using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDScript : MonoBehaviour {

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
    void Start () {

        //Find my children controls!
        if (GameObject.Find("BigText") != null)
        {
            myBigText = GameObject.Find("BigText").GetComponent<Text>();
            myBigText.rectTransform.localScale = new Vector3(4, 1, 1);
            myBigText.text= "";
        }
        //if (GameObject.Find("LittleText") != null)
        //{
        //    myLittleText = GameObject.Find("LittleText").GetComponent<Text>();
        //}
        ItemImageStack = new Image[4];

        for (int i = 0; i < ItemImageStack.Length; i++) {
            string objName = "Item" + i + "Image";
            if (GameObject.Find(objName) != null)
            {
                ItemImageStack[i] = GameObject.Find(objName).GetComponent<Image>();
            }
        }

        myStaminaBar = GameObject.Find("StaminaBar").GetComponent<Image>();
        mySecondWindBar = GameObject.Find("SecondWindBar").GetComponent<Image>();

        mySecondWindBar.rectTransform.localScale = new Vector3(0, 1, 1);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    /// <summary>
    /// Put in the fraction of currentStamina / MAX Stamina
    /// </summary>
    /// <param name="fraction"></param>
    public void SetStaminaBar(float fraction) {
        myStaminaBar.rectTransform.localScale = new Vector3(fraction, 1, 1);
    }

    public void SetSecondWindBar(float fraction){
        mySecondWindBar.rectTransform.localScale = new Vector3(fraction, 1, 1);
    }

    public void SetItemStack(List<eItemType> itemTypeList){
        int lastItemIndex = -1;
        for (int i = 0; i < ItemImageStack.Length; i++) {
            //Reset Sizes
            ItemImageStack[i].rectTransform.localScale = new Vector3(1, 1, 1);

            if (itemTypeList.Count > i){
                ItemImageStack[i].color = Color.white;
                if (itemTypeList[i] == eItemType.bomb)
                {
                    ItemImageStack[i].sprite = BombSprite;
                }
                else if (itemTypeList[i] == eItemType.bunny)
                {
                    ItemImageStack[i].sprite = BunnySprite;
                }
                else if (itemTypeList[i] == eItemType.torch)
                {
                    ItemImageStack[i].sprite = TorchSprite;
                }
                else if (itemTypeList[i] == eItemType.trap)
                {
                    ItemImageStack[i].sprite = TrapSprite;
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

    public void SetBigText(string textStr) {
        myBigText.text = textStr;
    }
    
}
