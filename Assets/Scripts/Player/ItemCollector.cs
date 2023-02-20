using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemCollector : MonoBehaviour
{
    [SerializeField] private Text gemText;
    [SerializeField] private GameObject gem_1;
    [SerializeField] private GameObject gem_2;
    [SerializeField] private GameObject gem_3;

    [SerializeField] private AudioSource gemSoundEffect;

    private static bool isGem_1, isGem_2, isGem_3;

    private static int gem = 0;

    private void Start()
    {
        gemText.text = "" + gem;
    }

    private void Update()
    {
        if (gem > 0 && isGem_1)
        {
            gem_1.SetActive(false);
        }

        if (gem > 0 && isGem_2)
        {
            gem_2.SetActive(false);
        }

        if (gem > 0 && isGem_3)
        {
            gem_3.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Gem"))
        {
            gemSoundEffect.Play();

            if (collision.gameObject.name.Equals("Gem (1)"))
            { 
                isGem_1 = true;
            }
            else if (collision.gameObject.name.Equals("Gem (2)"))
            {
                isGem_2 = true;
            }
            else if (collision.gameObject.name.Equals("Gem (3)"))
            {
                isGem_3 = true;
            }

            gem++;
            gemText.text = "" + gem;
        }
    }

    public void ResetTheGems()
    {
        Debug.Log("It's Working!");

        gem = 0;

        isGem_1 = false;
        isGem_2 = false;
        isGem_3 = false;

        gem_1.SetActive(true);
        gem_2.SetActive(true);
        gem_3.SetActive(true);
    }
}