using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeathFlash : MonoBehaviour
{
    Image fadeImage;

    void Start()
    {
        fadeImage = GetComponent<Image>();
    }

    void Update()
    {
        
    }

    public void Flash()
    {
        StartCoroutine(FadeOut());
    }
    IEnumerator FadeOut()
    {
        Color color = fadeImage.color;
        color.a = 1f;
        fadeImage.color = color;
        while (fadeImage.color.a > 0)
        {
            color = fadeImage.color;
            color.a -= .018f;

            fadeImage.color = color;
            yield return null;
        }

        yield return null;
    }
}
