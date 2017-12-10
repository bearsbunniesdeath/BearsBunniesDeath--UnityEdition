using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A condition is responsible for checking information in the game world
/// </summary>
public class Condition : LeafNode
{
    private Func<bool> checkThis;

    public Condition(Func<bool> checkThis)
    {
        this.checkThis = checkThis;
    }

    protected override NodeStatus Execute()
    {
        if (checkThis.Invoke()) {
            return NodeStatus.Success;
        }
        return NodeStatus.Failed;
    }
}
