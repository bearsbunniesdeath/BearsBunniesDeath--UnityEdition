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

    public LeafNode() : base(null)
    {
    }
    
    /// <summary>
    /// An action that gets called once, immediately before the node begins running
    /// </summary>
    public System.Action Initialize { get; set; }

    /// <summary>
    /// An action that gets called once, immediately after the node stops running
    /// </summary>
    public System.Action Terminate { get; set; }

    protected abstract NodeStatus Execute();

    public override NodeStatus Tick()
    {
        if (Initialize != null && status != NodeStatus.Running)
            Initialize.Invoke();

        status = Execute();

        if (Terminate != null && status != NodeStatus.Running)
            Terminate.Invoke();

        return status;
    }

}
