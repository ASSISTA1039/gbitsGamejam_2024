using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishItem : IState
{
    private MonsterStateController _controller;

    public FinishItem(MonsterStateController controller)
    {
        _controller = controller;
    }

    public void OnEnter()
    {
        Debug.Log("敌人进入Finish状态，完成道具阶段");
        _controller.EndItemPhase();
    }

    public void Tick()
    {
        // 无需操作，直接完成
    }

    public void OnExit()
    {
        Debug.Log("敌人离开Finish状态");
    }
}

