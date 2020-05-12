using System;
using System.Collections.Generic;
using System.Text;

namespace ASCIIArt.Engine.Helpers
{
    internal class CollectionDisposer<T>:
        IDisposable
    {

        private IEnumerable<T> collection;
        private Func<T, IEnumerable<IDisposable>> selectDisposeElm;

        public CollectionDisposer(IEnumerable<T> collection, Func<T, IEnumerable<IDisposable>> disposeElmSelector)
        {
            this.collection = collection;
            this.selectDisposeElm = disposeElmSelector;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    foreach(var v in collection)
                    {
                        foreach(var elm in selectDisposeElm(v))
                        {
                            elm.Dispose();
                        }
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

    internal static class CollectionDisposer
    {
        public static CollectionDisposer<T> Create<T>(IEnumerable<T> collection, Func<T, IEnumerable<IDisposable>> disposeElmSelector)
        {
            return new CollectionDisposer<T>(collection, disposeElmSelector);
        }

        public static CollectionDisposer<T> Create<T>(IEnumerable<T> collection)
            where T : IDisposable
        {
            return new CollectionDisposer<T>(collection, id);
        }

        private static IEnumerable<IDisposable> id<T>(T e)
            where T : IDisposable
        {
            yield return e;
        }
    }
}
