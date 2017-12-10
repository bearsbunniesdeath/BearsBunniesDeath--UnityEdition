﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An action is a leaf node responsible for making changes to the game world
/// </summary>
public class Action : LeafNode
{
    private Func<NodeStatus> doThis;

    public Action(Func<NodeStatus> doThis, System.Action initialize = null, System.Action terminate = null) : base(initialize, terminate)
    {
        this.doThis = doThis;
    }

    protected override NodeStatus Execute()
    {
        return doThis.Invoke();
    }
}
