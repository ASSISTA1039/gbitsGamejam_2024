using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IActionObserver
{
    void OnAnimationCompleted(string actionName);
    void OnSoundEffectCompleted(string actionName);
}

