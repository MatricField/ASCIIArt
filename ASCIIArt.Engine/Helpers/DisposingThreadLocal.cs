using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ASCIIArt.Engine.Helpers
{
    internal class DisposingThreadLocal<T>
        : ThreadLocal<T>
        where T : IDisposable
    {
        public DisposingThreadLocal():
            base(trackAllValues:true)
        {
        }

        public DisposingThreadLocal(Func<T> valueFactory) :
            base(valueFactory, trackAllValues: true)
        {
        }
        protected override void Dispose(bool disposing)
        {
            if(disposing)
            {
                foreach(var v in Values)
                {
                    v.Dispose();
                }
            }
            base.Dispose(disposing);
        }
    }
}
