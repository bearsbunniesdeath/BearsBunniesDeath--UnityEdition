using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action : Node
{
    private Func<NodeStatus> doThis;

    public Action(Func<NodeStatus> doThis) : base(null)
    {
        this.doThis = doThis;
    }

    public override NodeStatus Tick()
    {
        return doThis.Invoke();
    }
}
