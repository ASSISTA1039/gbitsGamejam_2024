using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleItem : IState
{
    private readonly MonsterStateController _controller;

    public IdleItem(MonsterStateController controller)
    {
        _controller = controller;
    }

    public void OnEnter()
    {
        Debug.Log("敌人进入Idle状态，等待选牌");
    }

    public void Tick()
    {
        // 如果有可用卡牌，转移到评估状态
        //if (_controller.monster.GetItems().Count > 0)
        //{
        //    _controller.IntoItemPhase();
        //}
    }

    public void OnExit()
    {
        Debug.Log("敌人离开Idle状态");
    }
}

