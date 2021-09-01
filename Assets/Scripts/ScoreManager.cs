using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class ScoreManager : MonoBehaviour
{
    [Header("Score values")]
    public Threshold AccuracyScore;
    public Threshold TimerScore;
    public Threshold TimerThreshold;
    public int ScrollScore;

    [Header("UI")]
    public List<TextMeshProUGUI> ScoreTexts;
    public Transform UIGameplay;
    public TextMeshProUGUI Score_PerfectPrefab;
    public TextMeshProUGUI Score_GoodPrefab;
    public TextMeshProUGUI Score_BadPrefab;
    public TextMeshProUGUI Score_ScrollPrefab;
    
    [Header("SFX")]
    public AudioSource PositiveAS;
    public AudioSource NegativeAS;
    public List<AudioClip> PositiveClips;
    public List<AudioClip> NegativeClips;
    
    public static ScoreManager _;
    private int currentScore;

    private void Awake()
    {
        _ = this;
    }

    private void Update()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        foreach (var txt in ScoreTexts)
        {
            txt.text = currentScore.ToString();
        }
    }

    public void AddScore(Score accuracy, float cutTimer, bool isSmallTarget, Vector2 scoreTextPos)
    {
        int score;
        
        if (accuracy == Score.Scroll)
        {
            score = ScrollScore;
        }
        else
        {
            var acc = (int)Mathf.Round(GetAccuracyScore(accuracy));
            var tim = (int)Mathf.Round(GetTimerScore(cutTimer));

            score = CalculateScore(acc, tim, isSmallTarget);
        }
        
        currentScore += score;
        
        ScoreAnimation(accuracy, scoreTextPos);
    }

    private void ScoreAnimation(Score acc, Vector2 scoreTextPos)
    {
        TextMeshProUGUI prefab;
        AudioClip aC = null;
        AudioSource aS = null;
        
        switch (acc)
        {
            case Score.Bad:
                prefab = Score_BadPrefab;
                break;
            case Score.Good:
                prefab = Score_GoodPrefab;
                aS = PositiveAS;
                aC = PositiveClips[Random.Range(0, PositiveClips.Count)];
                break;
            case Score.Perfect:
                prefab = Score_PerfectPrefab;
                aS = PositiveAS;
                aC = PositiveClips[Random.Range(0, PositiveClips.Count)];
                break;
            case Score.Scroll:
                prefab = Score_ScrollPrefab;
                aS = NegativeAS;
                aC = NegativeClips[Random.Range(0, NegativeClips.Count)];
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(acc), acc, null);
        }

        if (aS != null)
        {
            aS.PlayOneShot(aC);
        }

        var txt = Instantiate(prefab, scoreTextPos, quaternion.identity, UIGameplay);
        
        Destroy(txt.gameObject, 1.1f);
    }

    private int CalculateScore(int accuracy, int timer, bool isSmallTarget)
    {
        var score = isSmallTarget ? accuracy * 2 : accuracy;
        score += timer;

        return score;
    }
    
    private float GetAccuracyScore(Score score)
    {
        switch (score)
        {
            case Score.Bad:
                return AccuracyScore.Bad;
            case Score.Good:
                return AccuracyScore.Good;
            case Score.Perfect:
                return AccuracyScore.Perfect;
            default:
                throw new ArgumentOutOfRangeException(nameof(score), score, null);
        }
    }

    private float GetTimerScore(float timer)
    {
        if (timer <= TimerThreshold.Perfect)
        {
            return TimerScore.Perfect;
        }
        
        if (timer <= TimerThreshold.Good)
        {
            return TimerScore.Good;
        }

        return TimerScore.Bad;
    }
}

public enum Score
{
    Bad,
    Good,
    Perfect,
    Scroll
}

[Serializable]
public struct Threshold
{
    public float Bad;
    public float Good;
    public float Perfect;
}