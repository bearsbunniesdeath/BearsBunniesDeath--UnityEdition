using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviorTree {

    private Node root;
   
    /// <param name="root">The root node of the behavior tree</param>
    /// <exception cref="ArgumentNullException">Root node cannot be null</exception>
	public BehaviorTree(Node root)
    {
        if (root == null)
            throw new ArgumentNullException("Root node cannot be null");

        this.root = root;
    }

    public void Tick()
    {
        root.Tick();
    }

}

public enum NodeStatus
{
    Success,
    Running,
    Failed
}
