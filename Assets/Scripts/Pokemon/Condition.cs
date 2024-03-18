using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///  天气/异常状态 基础属性
/// </summary>
public class Condition
{
    public ConditionID Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string StartMessage { get; set; }

    /// <summary>
    /// 回合开始状态执行 委托
    /// </summary>
    public Action<Pokemon> OnStart { get; set; }
    /// <summary>
    /// 委托 返回Bool类型
    /// </summary>
    public Func<Pokemon,bool> OnBeforeMove { get; set; }
    public Action<Pokemon> OnAfterTurn { get; set; }
}
