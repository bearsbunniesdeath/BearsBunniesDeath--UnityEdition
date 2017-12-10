using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A leaf node has exactly one parent and no children
/// </summary>
public abstract class LeafNode : Node
{

    private NodeStatus status = NodeStatus.Failed;

    private System.Action initialize;
    private System.Action terminate;

    /// <param name="initialize">An action that gets called once, immediately before the node begins running</param>
    /// <param name="terminate">An action that gets called once, immediately after the node stops running</param>
    public LeafNode(System.Action initialize = null, System.Action terminate = null) : base(null)
    {
        this.initialize = initialize;
        this.terminate = terminate;
    }

    protected abstract NodeStatus Execute();

    public override NodeStatus Tick()
    {
        if (initialize != null && status != NodeStatus.Running)
            initialize.Invoke();

        status = Execute();

        if (terminate != null && status != NodeStatus.Running)
            terminate.Invoke();

        return status;
    }

}
