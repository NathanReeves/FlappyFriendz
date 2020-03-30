using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlappyAnimator : MonoBehaviour
{
    int frame = 0;
    SpriteRenderer spriteRenderer;
    public Sprite currentDownFlap;
    public Sprite currentMidFlap;
    public Sprite currentUpFlap;
    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        frame++;
        if (frame == 6)
        {
            spriteRenderer.sprite = currentDownFlap;
        }
        else if(frame == 11)
        {
            spriteRenderer.sprite = currentMidFlap;
        }
        else if(frame == 17)
        {
            spriteRenderer.sprite = currentUpFlap;
            frame = 0;
        }

    }
}
