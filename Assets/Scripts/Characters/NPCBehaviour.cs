using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using System.Linq;

public abstract class NPCBehaviour : MonoBehaviour {

    [SerializeField]
    private bool _isAlive = true;

    public bool IsAlive
    {
        get
        {
            return _isAlive;
        }
        set
        {
            if (!value)
            {
                _isAlive = value;

                CancelInvoke(RepeatingMethod.UpdatePath);
                CancelInvoke(RepeatingMethod.UpdateBehaviour);
            }           
        }
    }

    protected enum RepeatingMethod {
        UpdatePath,
        UpdateBehaviour
    }

    [SerializeField]
    protected List<Vector2> path = new List<Vector2>();
    [SerializeField]
    protected int currentWayPoint;        //Index of the path we are moving towards

    protected abstract void UpdatePath();
    protected abstract void UpdateBehaviour();

    protected void StartInvokeRepeating()
    {
        InvokeRepeating("UpdatePath", 2f, 3f);
        InvokeRepeating("UpdateBehaviour", 2f, 1f);
    }

    protected void MoveTowards(Vector2 point, float maxDistanceDelta)
    {
        transform.position = Vector2.MoveTowards(transform.position, point, maxDistanceDelta);
    }

    protected void MoveAlongPath(float maxDistanceDelta, float nextWayPointTriggerDistance)
    {
        if (path != null && path.Count > 0)
        {
            transform.position = Vector2.MoveTowards(transform.position, path[currentWayPoint], maxDistanceDelta);
            if (Vector2.Distance(transform.position, path[currentWayPoint]) < nextWayPointTriggerDistance && currentWayPoint < path.Count - 1)
            {
                currentWayPoint++;
            }
        }
    }

    protected void LerpAlongPath(float percentage, float nextWayPointTriggerDistance)
    {
        if (path != null && path.Count > 0)
        {
            transform.position = Vector2.Lerp(transform.position, path[currentWayPoint], percentage);
            if (Vector2.Distance(transform.position, path[currentWayPoint]) < nextWayPointTriggerDistance && currentWayPoint < path.Count - 1)
            {
                currentWayPoint++;
            }
        }
    }

    protected void OnPathCalculated(Path p)
    {
        if (p.error)
        {
            path = null;
        }
        else
        {
            path = p.vectorPath.Select(v => (Vector2)v).ToList();
            if (path.Count > 1)
            {
                //Ignore the first point in the path
                //This causes a hiccup in the smoothness of the bear movement
                path = path.Skip(1).ToList();
            }
        }
        currentWayPoint = 0;
    }

    /// <summary>
    /// Update an InvokeRepeating rate
    /// </summary>
    /// <param name="methodName">The name of the InvokeRepeating method</param>
    /// <param name="start">The time to start InvokeRepeating</param>
    /// <param name="rate">The rate of InvokeRepeating</param>
    protected void ChangeInvokeRate(RepeatingMethod method, float start, float rate)
    {
        CancelInvoke(method);
        InvokeRepeating(method.ToString(), start, rate);
    }

    protected void CancelInvoke(RepeatingMethod method)
    {
        CancelInvoke(method.ToString());
    }
}
