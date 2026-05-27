using System;
using UnityEngine;

public class ScorableItem : BaseItem, IPickable, IThrowable
{
    public int ScoreValue;

    [Header("Score Randomization")]
    public int minScore = 1;
    public int maxScore = 5;

    private void OnEnable()
    {
        // Randomize score only on the server (or if offline)
        if (Unity.Netcode.NetworkManager.Singleton == null || Unity.Netcode.NetworkManager.Singleton.IsServer)
        {
            ScoreValue = UnityEngine.Random.Range(minScore, maxScore + 1);
        }
    }

    public void Throw(Vector3 direction, float throwForce)
    {

    }

    public void SetScoreValue(int value)
    {
        ScoreValue = value;
    }

    public int GetScoreValue()
    {
        return ScoreValue;
    }
    public override void Sync()
    {
        
    }

}
