using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A selector (fallback) node is used to find and execute the first child that does not fail. 
/// </summary>
public class Sequence : Node {

#region "Lifetime"

    /// <param name="children">Child node in order of most importance</param>
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
