using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class GameObjectIdentifier{
    private string _name;
    private string _tag;

    public string Name { get { return _name; } }
    public string Tag { get { return _tag; } }

    public GameObjectIdentifier(string name, string tag)
    {
        _name = name;
        _tag = tag;
    }
}
