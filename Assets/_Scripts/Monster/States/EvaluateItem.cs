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
        Debug.Log("���˽���Evaluate״̬����ʼ��������");
        _controller.EvaluateItem();
    }

    public void Tick()
    {
        // ������ɺ󣬽���ѡ����״̬
        //if (_controller.IsEvaluationComplete)
        //{
        //    _controller.TransitionToState(_controller.ChooseCardState);
        //}
    }

    public void OnExit()
    {
        Debug.Log("�����뿪Evaluate״̬");
    }
}

