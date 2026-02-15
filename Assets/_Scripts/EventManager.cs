using System;

public static class EventManager
{
    public static event Action OnEnterShipControl;
    public static void RaiseEnterShipControl()
    {
        OnEnterShipControl?.Invoke();
    }

    public static event Action OnExitShipControl;
    public static void RaiseExitShipContorl()
    {
        OnExitShipControl?.Invoke(); 
    }

    public static event Action<bool> OnBoardShip;
    public static void RaiseBoardShip(bool value)
    {
        OnBoardShip?.Invoke(value);
    }
}
