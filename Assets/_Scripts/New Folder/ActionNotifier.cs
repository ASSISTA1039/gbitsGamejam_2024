using System;
using System.Collections.Generic;

public class ActionNotifier
{
    private List<IActionObserver> observers = new List<IActionObserver>();

    // 注册观察者
    public void AddObserver(IActionObserver observer)
    {
        if (!observers.Contains(observer))
        {
            observers.Add(observer);
        }
    }

    // 移除观察者
    public void RemoveObserver(IActionObserver observer)
    {
        observers.Remove(observer);
    }

    // 通知所有观察者动画完成
    public void NotifyAnimationCompleted(string actionName)
    {
        foreach (var observer in observers)
        {
            observer.OnAnimationCompleted(actionName);
        }
    }

    // 通知所有观察者音效完成
    public void NotifySoundEffectCompleted(string actionName)
    {
        foreach (var observer in observers)
        {
            observer.OnSoundEffectCompleted(actionName);
        }
    }
}
