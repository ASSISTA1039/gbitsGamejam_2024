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
        Debug.Log("���˽���Finish״̬����ɵ��߽׶�");
        _controller.EndItemPhase();
    }

    public void Tick()
    {
        // ���������ֱ�����
    }

    public void OnExit()
    {
        Debug.Log("�����뿪Finish״̬");
    }
}

