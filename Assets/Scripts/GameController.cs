﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class GameController : MonoBehaviour
{
    public CanvasGroup buttonsCanvasGroup;
    public CanvasGroup gameMenuCanvasGroup;
    public Button switchButton;
    public GameResultProcess gameResultProcess;

    [SerializeField] private Character[] playerCharacters = default;
    [SerializeField] private Character[] enemyCharacters = default;


    Character currentTarget;
    bool waitingForInput;

    bool gamePaused = false;


    // Start is called before the first frame update
    void Start()
    {
        switchButton.onClick.AddListener(NextTarget);
        StartCoroutine(GameLoop());
        FindObjectsOfType<CanvasGroup>();
    }

    public void PlayerAttack()
    {
        waitingForInput = false;
    }

    public void NextTarget()
    {
        for (int i = 0; i < enemyCharacters.Length; i++) {
            // Найти текущего персонажа (i = индекс текущего)
            if (enemyCharacters[i] == currentTarget) {
                int start = i;
                ++i;
                // Идем в сторону конца массива и ищем живого персонажа
                for (; i < enemyCharacters.Length; i++) {
                    if (enemyCharacters[i].IsDead())
                        continue;

                    // Нашли живого, меняем currentTarget
                    currentTarget.targetIndicator.gameObject.SetActive(false);
                    currentTarget = enemyCharacters[i];
                    currentTarget.targetIndicator.gameObject.SetActive(true);

                    return;
                }
                // Идем от начала массива до текущего и смотрим, если там кто живой
                for (i = 0; i < start; i++) {
                    if (enemyCharacters[i].IsDead())
                        continue;

                    // Нашли живого, меняем currentTarget
                    currentTarget.targetIndicator.gameObject.SetActive(false);
                    currentTarget = enemyCharacters[i];
                    currentTarget.targetIndicator.gameObject.SetActive(true);

                    return;
                }
                // Живых больше не осталось, не меняем currentTarget
                return;
            }
        }
    }

    Character FirstAliveCharacter(Character[] characters)
    {
        foreach (var character in characters) {
            if (!character.IsDead())
                return character;
        }
        return null;
    }

    void PlayerWon()
    {
        Debug.Log("Player won");
        gameResultProcess.ProcessWin();
    }

    void PlayerLost()
    {
        Debug.Log("Player lost");
        gameResultProcess.ProcessLose();
    }

    bool CheckEndGame()
    {
        if (FirstAliveCharacter(playerCharacters) == null) {
            PlayerLost();
            return true;
        }

        if (FirstAliveCharacter(enemyCharacters) == null) {
            PlayerWon();
            return true;
        }

        return false;
    }

    IEnumerator GameLoop()
    {
        Utility.SetCanvasGroupEnabled(buttonsCanvasGroup, false);

        while (!CheckEndGame()) {
            foreach (var player in playerCharacters) {
                if (player.IsDead())
                    continue;

                currentTarget = FirstAliveCharacter(enemyCharacters);
                if (currentTarget == null)
                    break;

                currentTarget.targetIndicator.gameObject.SetActive(true);


                Utility.SetCanvasGroupEnabled(buttonsCanvasGroup, true);
                waitingForInput = true;
                while (waitingForInput)
                    yield return null;
                Utility.SetCanvasGroupEnabled(buttonsCanvasGroup, false);

                currentTarget.targetIndicator.gameObject.SetActive(false);

                player.target = currentTarget.transform;
                player.AttackEnemy();
                while (!player.IsIdle())
                    yield return null;
            }

            foreach (var enemy in enemyCharacters) {
                if (enemy.IsDead())
                    continue;

                Character target = FirstAliveCharacter(playerCharacters);
                if (target == null)
                    break;

                enemy.target = target.transform;
                enemy.AttackEnemy();
                while (!enemy.IsIdle())
                    yield return null;
            }
        }
    }

    public void PauseGame()
    {
        SwitchGamePause(true);
    }

    public void ResumeGame()
    {
        SwitchGamePause(false);
    }

    public void MainMenu()
    {
        StopAllCoroutines();
        Time.timeScale = 1;
        LoadingScreen.instance.LoadScene("MainMenu");
    }

    public void RestartLevel()
    {
        StopAllCoroutines();
        Time.timeScale = 1;  
        LoadingScreen.instance.RestartScene();     
    }

    private void SwitchGamePause(bool pauseGame)
    {
        Time.timeScale = pauseGame ? 0 : 1;
        gamePaused = pauseGame;

        Utility.SetCanvasGroupEnabled(gameMenuCanvasGroup, pauseGame);
        Utility.SetCanvasGroupEnabled(buttonsCanvasGroup, !pauseGame);       
    }
}
