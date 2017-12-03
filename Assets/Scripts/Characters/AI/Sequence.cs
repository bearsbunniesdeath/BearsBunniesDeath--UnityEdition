using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sequence nodes are used to find and execute the first child that has not yet succeeded.
/// </summary>
public class Sequence : Node {

#region "Lifetime"

    /// <param name="children">Child nodes that are ticked in order</param>
    public Sequence(IEnumerable<Node> children) : base(children)
    {
    }

#endregion

    public override NodeStatus Tick()
    {
        foreach (Node node in children)
        {
            NodeStatus status = node.Tick();
            if (status == NodeStatus.Running) {
                return NodeStatus.Running;
            } else if (status == NodeStatus.Failed) {
                return NodeStatus.Failed;
            }          
        }

        return NodeStatus.Success;
    }

}
