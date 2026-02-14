using UnityEngine;
using static GameEnum;

public interface ITargetable
{
    Transform GetTransform();
    bool IsActive(); 
    Faction GetFaction(); 
}