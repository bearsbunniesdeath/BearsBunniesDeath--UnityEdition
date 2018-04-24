using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Conditional that checks if a target is within range
/// </summary>
public class WithinRange : Conditional {

    public SharedFloat radius;
    public string targetTag;
    public SharedGameObject target;
    public bool pathMustExist;                          //Also check that a path exists

    private List<GameObject> possibleTargets;

    public override void OnAwake()
    {
        //Cache all of the targets
        var targets = GameObject.FindGameObjectsWithTag(targetTag);
        possibleTargets = new List<GameObject>(targets.Length);
        possibleTargets.AddRange(targets);
    }

    public override TaskStatus OnUpdate()
    {
        IEnumerable<GameObject> targetsToCheck;

        if (pathMustExist) {
            targetsToCheck = possibleTargets.Where(t => Navigator.PathExistsBetween(transform.position, t.gameObject.transform.position));
        } else
        {
            targetsToCheck = possibleTargets;
        }       
                     
        if (targetsToCheck.Count() > 0) {
            targetsToCheck = targetsToCheck.OrderBy(t => Vector2.Distance(transform.position, t.gameObject.transform.position));
            if (Vector2.Distance(transform.position, targetsToCheck.First().transform.position) < radius.Value)
            {
                target.Value = targetsToCheck.First();
                return TaskStatus.Success;
            }
        }
        
        target.Value = null;
        return TaskStatus.Failure;          
    }

}
