using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.LevelLogic
{
    public class TouchFinishAndStartLevelCriteria :  ILevelCompleteCriteria
    {

        private bool finishedWasTouched;

        public bool IsComplete
        {
            get;
            set;
        }

        private ContactPoint FinishPoint;
        private ContactPoint StartPoint;

        public TouchFinishAndStartLevelCriteria() {
            IsComplete = false;
            FinishPoint = GameObject.FindGameObjectWithTag("Finish").GetComponent<ContactPoint>();
            StartPoint = GameObject.FindGameObjectWithTag("Start").GetComponent<ContactPoint>();
        }

        private void AttachToContactPoint() {
            FinishPoint = GameObject.FindGameObjectWithTag("Finish").GetComponent<ContactPoint>();
            StartPoint = GameObject.FindGameObjectWithTag("Start").GetComponent<ContactPoint>();
        }

        //Just update through code, not through unity engine (So I don't need to be a MonoBehaviour)
        void ILevelCompleteCriteria.Update()
        {
            //Sometimes on reset the Finish point is attached to the old copy, then it turns null.
            if (FinishPoint == null || StartPoint == null)
            {
                AttachToContactPoint();
            }

            if (!finishedWasTouched)
            {
                if (FinishPoint.IsBeingTouched)
                {
                    finishedWasTouched = true;
                }
            }
            else
            {
                Debug.Log("Halfway there.");
                if (StartPoint.IsBeingTouched)
                {
                    IsComplete = true;
                }
            }

        }
    }
}
