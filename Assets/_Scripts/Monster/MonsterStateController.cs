using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public class MonsterStateController : MonoBehaviour
{
    public Monster monster;
    public GameManager manager;
    public GameItem evaluateItem;

    private StateMachine _stateMachine;
    private bool inItemPhase;

    private bool isUsingItem;
    private bool isEvaluationComplete;

    public void Init(Monster monster, GameManager manager)
    {
        this.monster = monster;
        this.manager = manager;
    }

    private void Awake()
    {

        _stateMachine = new StateMachine();

        var other = new Other();
        var idle = new IdleItem(this);
        var evaluate = new EvaluateItem(this);
        var useItem = new UseItem(this);
        var finish = new FinishItem(this);

        void At(IState to, IState from, Func<bool> condition) => _stateMachine.AddTransition(to, from, condition);
        At(other, idle, StartItemPhase());
        At(idle, evaluate, HaveItem());
        At(idle, finish, NotHaveItem());
        At(evaluate, useItem , CompletesEvaluatedItem());
        At(useItem, evaluate, HaveItem());
        At(useItem, finish, NotHaveItem());
        At(finish, other, EndItemPhase());

        //_stateMachine.AddAnyTransition(flee, () => enemyDetector.EnemyInRange);
        //At(flee, search, () => enemyDetector.EnemyInRange == false);

        _stateMachine.SetState(other);

        Func<bool> StartItemPhase() => () => inItemPhase == true;
        //Func<bool> StartChooseItem() => () => inItemPhase == true;
        Func<bool> CompletesEvaluatedItem() => () => isEvaluationComplete == true && evaluateItem != null;
        Func<bool> HaveItem() => () => isUsingItem == true && monster.GetItems().Count > 0;
        Func<bool> NotHaveItem() => () => isUsingItem == true && monster.GetItems().Count == 0;
        Func<bool> EndItemPhase() => () => inItemPhase == false;

    }

    private void Update()
    {
        _stateMachine.Tick();
    }

    public void IntoItemPhase()
    {
        inItemPhase = true;
    }

    public void EvaluateItem()
    {
        // 根据局势评估道具的优先级
        // 根据道具策略选择进入下一个状态
        List<GameItem> items = monster.GetItems();
        List<string> strings = new List<string>();
        for (int i = 0; i < items.Count; i++)
        {
            strings.Add(items[i].itemName);
        }

        if (items.Contains(new PeekItem()))
        {
            evaluateItem = new PeekItem();

        }
        else if (items.Contains(new ForceChangeCardItem()))
        {
            evaluateItem = new ForceChangeCardItem();
        }
        else
        {
            evaluateItem = null;
        }
        // 选择将要使用的道具
        isEvaluationComplete = true; // 模拟评估完成
    }

    public void UseItem()
    {
        if(evaluateItem == null)
        {
            return;
        }

        Debug.Log($"敌人使用道具：{evaluateItem.itemName}");

        // 执行道具效果
        evaluateItem.Use(manager.player, manager.monster, manager.GetPlayerSelectedCard(), manager.GetMonsterSelectedCard());

        // 移除敌人道具
        monster.RemoveItem(evaluateItem);
        evaluateItem = null;

        isUsingItem = true;

    }

    public void EndItemPhase()
    {
        // 结束道具阶段逻辑
        Debug.Log("敌人道具阶段结束");

        isUsingItem = false;
        inItemPhase = false;
    }
}
