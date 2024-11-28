using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UseItem : IState
{
    private readonly MonsterStateController _controller;

    public UseItem(MonsterStateController controller)
    {
        _controller = controller;
    }

    public void OnEnter()
    {
        Debug.Log("敌人进入UseItem状态，使用道具");
        _controller.UseItem();
    }

    public void Tick()
    {
        //_controller.TransitionToState(_controller.ConfirmActionState);
    }

    public void OnExit()
    {
        Debug.Log("敌人离开UseItem状态");
    }
}

