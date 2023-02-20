using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject ring;

    private int isRing = 0;

    private void Awake()
    {
        if (Time.timeScale == 0)
        {
            ResumeGame();
        }
    }

    private void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            ring.SetActive(true);

            isRing++;

            PauseGame();
        }

        if (isRing > 1)
        {
            ring.SetActive(false);

            isRing = 0;

            ResumeGame();
        }
    }

    private void PauseGame()
    {
        Time.timeScale = 0;
    }

    private void ResumeGame()
    {
        Time.timeScale = 1;
    }
}
