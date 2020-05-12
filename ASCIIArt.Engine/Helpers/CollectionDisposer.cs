using System;
using System.Collections.Generic;
using System.Text;

namespace ASCIIArt.Engine.Helpers
{
    internal class CollectionDisposer:
        IDisposable
    {
        private Func<IEnumerable<IDisposable>> selectDisposeElm;

        public CollectionDisposer(Func<IEnumerable<IDisposable>> disposeElmSelector)
        {
            selectDisposeElm = disposeElmSelector;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    foreach(var elm in selectDisposeElm())
                    {
                        elm.Dispose();
                    }
                }

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}
