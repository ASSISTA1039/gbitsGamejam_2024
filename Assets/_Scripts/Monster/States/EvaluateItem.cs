using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvaluateItem : IState
{
    private readonly MonsterStateController _controller;

    public EvaluateItem(MonsterStateController controller)
    {
        _controller = controller;
    }

    public void OnEnter()
    {
        Debug.Log("敌人进入Evaluate状态，开始评估局势");
        _controller.EvaluateItem();
    }

    public void Tick()
    {
        // 评估完成后，进入选择卡牌状态
        //if (_controller.IsEvaluationComplete)
        //{
        //    _controller.TransitionToState(_controller.ChooseCardState);
        //}
    }

    public void OnExit()
    {
        Debug.Log("敌人离开Evaluate状态");
    }
}

