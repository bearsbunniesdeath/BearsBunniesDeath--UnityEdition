using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Node {
    
    protected IEnumerable<Node> children;

    public Node(IEnumerable<Node> children)
    {       
        this.children = children;
    }

    public abstract NodeStatus Tick();

}
