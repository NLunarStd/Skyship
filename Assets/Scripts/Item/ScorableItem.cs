using System;
using UnityEngine;

public class ScorableItem : BaseItem, IPickable, IThrowable
{
    public int ScoreValue;
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
