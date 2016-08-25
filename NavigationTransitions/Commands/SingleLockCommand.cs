using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NavigationTransitions
{
    public class SingleLockCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private readonly SharedLock _sharedLock;
        private readonly Func<object, Task> _execute;

        public SingleLockCommand(Func<object, Task> execute, SharedLock sharedLock = null)
        {
            if (execute == null)
                throw new ArgumentException(nameof(execute));

            _execute = execute;
            _sharedLock = sharedLock ?? new SharedLock();
        }

        public SingleLockCommand(Func<Task> execute, SharedLock sharedLock = null)
        {
            if (execute == null)
                throw new ArgumentException(nameof(execute));

            _execute = (obj) => execute();
            _sharedLock = sharedLock ?? new SharedLock();
        }

        public bool CanExecute(object parameter)
        {
            return !_sharedLock.IsLocked;
        }

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        public async void Execute(object parameter)
        {
            if (_sharedLock.TakeLock()) //Ignores code block if lock already taken in SharedLock.
            {
                var events = CanExecuteChanged;
                try
                {
                    if (events != null)
                        events(this, EventArgs.Empty);

                    await _execute(parameter);
                }
                //TODO: Should I have som exception handling here?
                finally
                {
                    _sharedLock.ReleaseLock();
                    if (events != null)
                        events(this, EventArgs.Empty);
                }
            }
        }
#pragma warning restore RECS0165
    }

    public class SingleLockCommand<TValue> : SingleLockCommand
    {
        public SingleLockCommand(Func<TValue, Task> execute, SharedLock sharedLock = null)
            : base(obj => execute((TValue)obj), sharedLock)
        {
            //
        }
    }

    public class SharedLock
    {
        private int _lock;

        private readonly Guid LogId;

        public bool IsLocked => _lock != 0;

        public SharedLock()
        {
            _lock = 0;
            LogId = Guid.NewGuid();
            System.Diagnostics.Debug.WriteLine($"Lock created: {LogId}");
        }

        public bool TakeLock()
        {
            var oldVal = Interlocked.Exchange(ref _lock, 1); //Atomic swap values.
            var lockTaken = oldVal == 0;

            if (lockTaken)
                System.Diagnostics.Debug.WriteLine($"Lock taken: {LogId}");
            else
                System.Diagnostics.Debug.WriteLine($"Lock not taken: {LogId}");

            return lockTaken;
        }

        public bool ReleaseLock()
        {
            var oldVal = Interlocked.Exchange(ref _lock, 0);
            var lockReleased = oldVal == 1;

            if (lockReleased)
                System.Diagnostics.Debug.WriteLine($"Lock released: {LogId}");
            else
                throw new InvalidOperationException($"Lock not released: {LogId}"); //Should not be possible.

            return lockReleased;
        }
    }
}
