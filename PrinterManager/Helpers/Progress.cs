using System;
using System.Threading;

namespace PrinterManager.Helpers
{
    public interface IProgress<T>
    {
        void Report(T value);
    }

    public class Progress<T> : IProgress<T>
    {
        private readonly SynchronizationContext _context;
        private readonly Action<T> _handler;

        public Progress(Action<T> handler)
        {
            _handler = handler;
            _context = SynchronizationContext.Current ?? new SynchronizationContext();
        }

        public void Report(T value)
        {
            T val = value;
            _context.Post(_ => _handler(val), null);
        }
    }
}
