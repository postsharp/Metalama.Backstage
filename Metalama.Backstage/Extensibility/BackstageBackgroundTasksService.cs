// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Backstage.Extensibility;

// BackstageBackgroundTasksService is intentionally not disposable, relying instead on GC, because Metalama's
// service provider disposal implementation would dispose all backstage services for all tests, and a few
// tests would cause issues.
#pragma warning disable CA1001

public class BackstageBackgroundTasksService : IBackstageService

{
    private readonly TaskCompletionSource<bool> _completedTaskSource = new();
    private readonly SemaphoreSlim _noPendingTaskSemaphore = new( 1 );

    private int _pendingTasks;
    private bool _canEnqueue = true;

    /// <summary>
    /// Gets the default instance, which is intentionally shared in the process.
    /// It means that <see cref="CompleteAsync"/> can be only called once in the process.
    /// </summary>
    public static BackstageBackgroundTasksService Default { get; } = new();

    static BackstageBackgroundTasksService()
    {
        AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
    }

    private static void OnProcessExit( object? sender, EventArgs e )
    {
        Default.CompleteAsync().Wait();
    }

    internal void Enqueue( Func<Task> func )
    {
        this.OnTaskStarted();
        Task.Run( func ).ContinueWith( this.OnTaskCompleted );
    }

    internal void Enqueue( Action action )
    {
        this.OnTaskStarted();
        Task.Run( action ).ContinueWith( this.OnTaskCompleted );
    }

    /// <summary>
    /// Prevents new tasks to be enqueued and awaits for the completion of previously enqueued tasks. 
    /// </summary>
    private Task CompleteAsync()
    {
        this._canEnqueue = false;

        if ( this._pendingTasks == 0 )
        {
            this._completedTaskSource.TrySetResult( true );
        }

        return this._completedTaskSource.Task;
    }

    /// <summary>
    /// This method can be use in tests to wait for any point when the queue is empty but does
    /// not guarantee that no new task will be enqueued.
    /// </summary>
    internal async Task WhenNoPendingTaskAsync()
    {
        await this._noPendingTaskSemaphore.WaitAsync();
        this._noPendingTaskSemaphore.Release();
    }

    private void OnTaskStarted()
    {
        if ( !this._canEnqueue )
        {
            throw new InvalidOperationException();
        }

        if ( Interlocked.Increment( ref this._pendingTasks ) == 1 )
        {
            if ( !this._noPendingTaskSemaphore.Wait( 0 ) )
            {
                throw new InvalidOperationException();
            }
        }
    }

    private void OnTaskCompleted( Task t )
    {
        if ( Interlocked.Decrement( ref this._pendingTasks ) == 0 )
        {
            this._noPendingTaskSemaphore.Release();

            if ( !this._canEnqueue )
            {
                this._completedTaskSource.TrySetResult( true );
            }
        }
    }
}