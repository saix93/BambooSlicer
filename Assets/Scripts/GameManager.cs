using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public BambooStickSpawner Spawner;

    [Header("Gameplay")]
    public float MaxGameTime;
    public float MediumSpeedGameTime;
    public float FastSpeedGameTime;
    public float FastestSpeedGameTime;
    public SmallTargetSpawner SmallTargetSpawner;
    public float TimeToStartTicking = 10;
    public float ClockFadeTime = 1;
    public AudioClip ClockTick;
    public AudioSource ClockAlarm;

    [Header("Layer Order")]
    public int lo_newStick = 50;
    public int lo_stickCut = 20;

    [Header("Cut Forces")]
    public Vector2 MinMaxXForce;
    public Vector2 MinMaxYForce;
    public Vector2 MinMaxTorque;
    
    [Header("Data")]
    public float xOffset = .25f;

    [Header("UI")]
    public TextMeshProUGUI TimerText;
    public GameObject GameOverScreen;

    [Header("Misc")]
    
    public static GameManager _;
    private AudioSource audioSource;
    public bool GameRunning;

    private float currentGameTime;
    private bool floatTicking;

    private void Awake()
    {
        _ = this;
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        var newStick = SpawnNewBambooStick();
        newStick.UpdateOrderInLayer(lo_newStick);

        currentGameTime = MaxGameTime;
        SmallTargetSpawner.SetStatus(SpawnerStatus.Slow);
        GameRunning = true;
        floatTicking = false;
    }

    private void Update()
    {
        if (!GameRunning) return;
        
        currentGameTime -= Time.deltaTime;

        if (!floatTicking && currentGameTime <= TimeToStartTicking)
        {
            floatTicking = true;
            StartCoroutine(ClockAudio());
        }
        
        TimerText.text = ((int)Mathf.Ceil(currentGameTime)).ToString();
        
        CheckGameTime();
    }

    private void CheckGameTime()
    {
        if (currentGameTime <= MediumSpeedGameTime)
        {
            SmallTargetSpawner.SetStatus(SpawnerStatus.Medium);
        }

        if (currentGameTime <= FastSpeedGameTime)
        {
            SmallTargetSpawner.SetStatus(SpawnerStatus.Fast);
        }

        if (currentGameTime <= FastestSpeedGameTime)
        {
            SmallTargetSpawner.SetStatus(SpawnerStatus.Fastest);
        }

        if (currentGameTime <= 0)
        {
            StartCoroutine(EndGame());
        }
    }

    private IEnumerator EndGame()
    {
        GameRunning = false;
        
        yield return new WaitForSeconds(2f);
        
        GameOverScreen.SetActive(true);
    }

    private IEnumerator ClockAudio()
    {
        var wait = new WaitForSeconds(1f);
        
        while (currentGameTime >= 0)
        {
            audioSource.PlayOneShot(ClockTick);

            yield return wait;
        }
        
        ClockAlarm.Play();
        
        float t = ClockFadeTime;
        
        while (t > 0)
        {
            yield return null;
            
            t -= Time.deltaTime;
            ClockAlarm.volume = t / ClockFadeTime;
        }
    }

    public void CutBamboo(Vector2 inters1, Vector2 inters2, BambooStick stick)
    {
        stick.DisableCutTarget();
        
        // Upper cut
        var upperCut = Instantiate(stick);
        upperCut.DisableCollider();
        upperCut.UpdateOrderInLayer(lo_stickCut);
        upperCut.gameObject.name = "UpperCut";

        // Lower cut
        var lowerCut = Instantiate(upperCut);
        lowerCut.DisableCollider();
        lowerCut.gameObject.name = "LowerCut";
        
        // Create new runtime materials
        upperCut.SetChildrenRenderers();
        lowerCut.SetChildrenRenderers();
        
        // Cut through both sticks
        upperCut.CutThrough(inters1, inters2, true);
        lowerCut.CutThrough(inters1, inters2, false);

        // Add random force
        var randomDirection = Random.value >= 0.5;
        upperCut.AddRandomForce(MinMaxXForce, MinMaxYForce, randomDirection);
        upperCut.AddRandomRotation(MinMaxTorque);
        lowerCut.AddRandomForce(MinMaxXForce, MinMaxYForce, !randomDirection);
        lowerCut.AddRandomRotation(MinMaxTorque);

        var isSmallTarget = stick.isSmallTarget;

        DestroyImmediate(stick.gameObject);

        if (isSmallTarget) return;
        
        var newStick = SpawnNewBambooStick();
        newStick.RecalculateBounds();
        newStick.UpdateOrderInLayer(lo_newStick);
    }

    private BambooStick SpawnNewBambooStick()
    {
        return Spawner.SpawnBambooStick(xOffset);
    }

    public void Retry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Exit()
    {
        SceneManager.LoadScene(0);
    }
}
