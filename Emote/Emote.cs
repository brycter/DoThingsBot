using System;
using System.IO;
using System.Collections.ObjectModel;

using Decal.Adapter;
using Decal.Adapter.Wrappers;

namespace DoThingsBot.Emote {
    class Emote : IDisposable {
        public Emote() {
            try {
            }
            catch (Exception ex) { Util.LogException(ex); }
        }

        private bool disposed;

        public void Dispose() {
            Dispose(true);

            // Use SupressFinalize in case a subclass
            // of this type implements a finalizer.
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            // If you need thread safety, use a lock around these 
            // operations, as well as in your methods that use the resource.
            if (!disposed) {
                if (disposing) {
                }

                // Indicate that the instance has been disposed.
                disposed = true;
            }
        }

        public void Wave() {

        }
    }
}
