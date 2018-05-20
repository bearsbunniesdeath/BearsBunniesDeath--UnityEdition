using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Environment
{
    interface ILightInhibitor
    {
        float DimmingTime {
            get;
                }

        float DimmingFactor {
            get;
        }
    }
}
