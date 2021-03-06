﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour {

    public Player player1;
    public Player player2;

    public GameObject pauseMenu;
    public GameObject controlsMenu;
    public GameObject gameOverMenu;
    public UnityEngine.EventSystems.EventSystem pauseSystem;
    public UnityEngine.EventSystems.EventSystem controlSystem;
    public UnityEngine.EventSystems.EventSystem gameOverSystem;

    private bool canPause;
    private bool isPaused;


    public Text p1HP;
    public Text p2HP;
    public Text p1Speed;
    public Text p2Speed;
    public Text p1Bullet;
    public Text p2Bullet;


	// Use this for initialization
	void Start () {
        Time.timeScale = 1;
        canPause = true;
        isPaused = false;
        UnloadAll();
        UpdateNumbers();

	}
	
	// Update is called once per frame
	void Update () {
        UpdateNumbers();
		if (Input.GetButtonDown("Pause") && !isPaused && canPause)
        {
            Pause();
        } else
        if (Input.GetButtonDown("Pause") && isPaused)
        {
            UnPause();
        }
	}


    public void LoadLevel(string level)
    {
        SceneManager.LoadScene(level, LoadSceneMode.Single);
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void Restart()
    {
        Time.timeScale = 1;
        string scene = SceneManager.GetActiveScene().name;
        LoadLevel(scene);
    }

    public void Pause()
    {
        isPaused = true;
        Time.timeScale = 0;
        LoadSystem(pauseSystem);
        LoadMenu(pauseMenu);
    }

    public void UnPause()
    {
        isPaused = false;
        Time.timeScale = 1;
        UnloadAll();
    }

    public void UnloadMenu(GameObject menu)
    {
        menu.SetActive(false);
    }

    public void LoadMenu(GameObject menu)
    {
        menu.SetActive(true);
    }

    private void UnloadAll()
    {
        UnloadMenu(pauseMenu);
        UnloadMenu(controlsMenu);
        UnloadMenu(gameOverMenu);
        UnloadSystem(pauseSystem);
        UnloadSystem(controlSystem);
        UnloadSystem(gameOverSystem);
    }

    public void LoadSystem(UnityEngine.EventSystems.EventSystem system)
    {
        system.gameObject.SetActive(true);
    }

    public void UnloadSystem(UnityEngine.EventSystems.EventSystem system)
    {
        system.gameObject.SetActive(false);
    }

    public void UpdateNumbers()
    {
        p1HP.text = Mathf.Round(player1.GetHealth()).ToString();
        p2HP.text = Mathf.Round(player2.GetHealth()).ToString();
        p1Speed.text = player1.GetSpeed().ToString();
        p2Speed.text = player2.GetSpeed().ToString();
        p1Bullet.text = player1.GetShootSpeed().ToString();
        p2Bullet.text = player2.GetShootSpeed().ToString();
    }

    public void GameOver()
    {
        UnloadAll();
        canPause = false;
        StartCoroutine(EndSlowTime());
    }

    private IEnumerator EndSlowTime()
    {
        for (int i = 0; i < 100; ++i)
        {
            Time.timeScale -= 0.01f;
            yield return new WaitForSecondsRealtime(0.013f);
        }
        LoadMenu(gameOverMenu);
        LoadSystem(gameOverSystem);
        Time.timeScale = 0;
    }
}
