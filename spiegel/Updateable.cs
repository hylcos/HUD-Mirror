using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spiegel
{
    class Updateable
    {
        public TimeSpan updatePeriod { get; protected set; }

        protected Updateable(TimeSpan updatePeriod)
        {
            this.updatePeriod = updatePeriod;
        }

        public void setUpdatePeriod(TimeSpan updatePeriod)
        {
            this.updatePeriod = updatePeriod;
        }
        public virtual void update() { }
    }
}
