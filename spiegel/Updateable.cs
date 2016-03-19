using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spiegel
{
    class Updateable
    {
        public TimeSpan updatePeriod { get; private set; }

        protected Updateable(TimeSpan updatePeriod)
        {
            this.updatePeriod = updatePeriod;
        }

        public virtual void update() { }
    }
}
