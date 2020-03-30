using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomizeBird : MonoBehaviour
{
    public FlappyAnimator animator;
    public FlappyAnimator friendAnimator;
    public Sprite[] blueBirds;
    public Sprite[] pinkBirds;
    public Sprite[] greenBirds;
    public Sprite[] yellowBirds;
    public Sprite[] redBirds;
    public Sprite[] rainBirds;
    public GameObject[] BirdButtons;
    public Image friendRespawnButton;
    
    void Awake()
    {

        //Init player color
#if !UNITY_EDITOR
        //if user's device does not allow in app puchases, hide the rainbow bird
        if(!GameKit.Instance.IAPCanPurchase() && !GameStateManager.Instance.SaveData.unlockedRain)
        {
            BirdButtons[5].SetActive(false);
        }
        else if(GameStateManager.Instance.SaveData.unlockedRain)
        {
            //if already unlocked, disable lock image
            BirdButtons[5].transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
        }

#endif
        //show selected bird by fading button
        Color fade = BirdButtons[GameStateManager.Instance.SaveData.birdColor].gameObject.GetComponent<Image>().color;
        fade.a = .5f;
        BirdButtons[GameStateManager.Instance.SaveData.birdColor].gameObject.GetComponent<Image>().color = fade;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void changeAnimatorSprites(FlappyAnimator anim, Sprite[] spriteList)
    {
        if(anim == friendAnimator)
        {
            friendRespawnButton.sprite = spriteList[2];
        }
        anim.currentDownFlap = spriteList[0];
        anim.currentMidFlap = spriteList[1];
        anim.currentUpFlap = spriteList[2];
    }
    public void BoughtRainbowBird()
    {
        //save purchased
        GameStateManager.Instance.SaveData.unlockedRain = true;
        GameStateManager.Instance.WriteSaveData();
        //disable lock image
        BirdButtons[5].transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
        
        ChangeBirdColor(5);
    }
    public void ChangeFriendColor(int color)
    {
      
        switch (color)
        {
            case 0:
                changeAnimatorSprites(friendAnimator, blueBirds);
                break;
            case 1:
                changeAnimatorSprites(friendAnimator, pinkBirds);
                break;
            case 2:
                changeAnimatorSprites(friendAnimator, greenBirds);
                break;
            case 3:
                changeAnimatorSprites(friendAnimator, yellowBirds);
                break;
            case 4:
                changeAnimatorSprites(friendAnimator, redBirds);
                break;
            case 5:
                changeAnimatorSprites(friendAnimator, rainBirds);
                break;
            default:
                break;
        }
    }
    public void ChangeBirdColor(int color)
    {
        if(color == 5 && !GameStateManager.Instance.SaveData.unlockedRain)
        {
            GameKit.Instance.PurchaseInApp("rainbird");
            return;
        }

        
        //make the previous bird button solid
        Color solid = BirdButtons[GameStateManager.Instance.SaveData.birdColor].gameObject.GetComponent<Image>().color;
        solid.a = 1f;
        BirdButtons[GameStateManager.Instance.SaveData.birdColor].gameObject.GetComponent<Image>().color = solid;

        //show selected bird by fading button
        Color fade = BirdButtons[color].gameObject.GetComponent<Image>().color;
        fade.a = .5f;
        BirdButtons[color].gameObject.GetComponent<Image>().color = fade;

        GameStateManager.Instance.SaveData.birdColor = color;
        GameStateManager.Instance.WriteSaveData();



        switch (color)
        {
            case 0:
                changeAnimatorSprites(animator, blueBirds);
                break;
            case 1:
                changeAnimatorSprites(animator, pinkBirds);
                break;
            case 2:
                changeAnimatorSprites(animator, greenBirds);
                break;
            case 3:
                changeAnimatorSprites(animator, yellowBirds);
                break;
            case 4:
                changeAnimatorSprites(animator, redBirds);
                break;
            case 5:
                changeAnimatorSprites(animator, rainBirds);
                break;
            default:
                break;
        }
    }
}
