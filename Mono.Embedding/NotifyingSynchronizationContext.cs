using System;
using System.Threading;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections.Concurrent;

namespace Mono.Embedding
{
	/// <summary>
	/// Provides a ConcurrentQueue based SynchronizationContext that uses
	/// event based notification to inform subscribers that the queue needs processed.
	/// This ensures that the queue gets processed on the associated thread.
	/// The default implementation of SynchronizationContext merely routes requests to a ThreadPool
	/// which can cause issues for solutions that require that the likes of BackgroundWorker
	/// execute their completion delegates on a particular thread. 
	/// WPF makes use of DispatcherSynchronizationContext but this object is unsuitable for
	/// use in embedded code that provides its own event processing loop as 
	/// DispatcherSynchronizationContext blocks.
	/// </summary>
	public sealed class NotifyingSynchronizationContext : SynchronizationContext
   {
		private sealed class WorkItem {
			private readonly SendOrPostCallback _callback;
			private readonly object _state;
			private readonly ManualResetEventSlim _reset;

			public WorkItem(SendOrPostCallback callback, object state, ManualResetEventSlim reset) {
				if (callback == null)
					throw new ArgumentNullException("callback");

				_callback = callback;
				_state = state;
				_reset = reset;
			}

			public void Execute() {
				_callback(_state);
				if (_reset != null) {
					_reset.Set();
				}
			}
		}

		private readonly Thread _executingThread;
		private readonly ConcurrentQueue<WorkItem> _workItems;

		public event EventHandler WorkItemsPending;

		/// <summary>
		/// Creates a SynchronizationContext that targets the current thread.
		/// </summary>
		public NotifyingSynchronizationContext()
		{
			_workItems = new ConcurrentQueue<WorkItem>();
			_executingThread = Thread.CurrentThread;
		}

		/// <summary>
		/// Returns True if the queue has work items pending.
		/// </summary>
		public bool HasWorkItems {
			get {
				return !_workItems.IsEmpty;
			}
		}

		/// <summary>
		/// If True the class logs queue processing activity to the console.
		/// </summary>
		public bool LogActivity { get; set; }

		private WorkItem ExecuteAndReturnNextWorkItem () {
			WorkItem currentItem;
			if (_workItems.TryDequeue(out currentItem)) {
				currentItem.Execute ();
			}
			return currentItem;
		}

		/// <summary>
		/// Execute all work items in the queue.
		/// </summary>
		public void ExecuteWorkItems() {
			ExecuteWorkItems(null);
		}

		private void ExecuteWorkItems(WorkItem requestedWorkItem) {

			if (LogActivity) { 
				Console.WriteLine("NSC : executing : {0} work items in queue", _workItems.Count);
				if (requestedWorkItem == null) {
					Console.WriteLine("NSC : all queued work items will be executed");
				} else {
					Console.WriteLine("NSC : queued work items will be executed until requested item completes");
				}
			}

			// execute queue until requested work item is done or queue is empty
			WorkItem executedWorkItem = null;
			int count = 0;
			do {
				executedWorkItem = ExecuteAndReturnNextWorkItem();
				if (executedWorkItem != null) count++;
			} while (executedWorkItem != null && executedWorkItem != requestedWorkItem);

			if (LogActivity) Console.WriteLine("NSC : executed {0} items", count);
		}

		private void Enqueue(WorkItem workItem) {
			_workItems.Enqueue(workItem);

			// notify
			if (WorkItemsPending != null) {
				WorkItemsPending(this, EventArgs.Empty);
			}
		}

		/// <summary>
		/// Posts the delegate to the queue and returns immediately.
		/// </summary>
		/// <param name="callback">Callback delegate.</param>
		/// <param name="state">State object passed as argument to callback delegate.</param>
		public override void Post(SendOrPostCallback callback, object state) {

			if (LogActivity) {
				if (Thread.CurrentThread == _executingThread) {
					Console.WriteLine("NSC : post on executing thread");
				} else {
					Console.WriteLine("NSC : post on non executing thread");
				}
			}

			Enqueue(new WorkItem(callback, state, null));
		}

		/// <summary>
		/// Posts the delegate to the queue, executes pending work items and 
		/// returns when the delegate parameter executes.
		/// </summary>
		/// <param name="callback">Callback delegate.</param>
		/// <param name="state">State object passed as argument to callback delegate.</param>
		public override void Send(SendOrPostCallback callback, object state) {
			if (Thread.CurrentThread == _executingThread) {

				if (LogActivity) Console.WriteLine("NSC : send on executing thread");

				// enqueue item
				WorkItem workItem = new WorkItem(callback, state, null);
				Enqueue(workItem);

				// execute queue until work item is done
				ExecuteWorkItems(workItem);
			} else 
			{
				if (LogActivity) Console.WriteLine("NSC : send on non executing thread");

				// enqueue item and block until work item completes
				using (var reset = new ManualResetEventSlim ()) {
					Enqueue(new WorkItem(callback, state, reset));

					if (LogActivity) Console.WriteLine("NSC : send waiting...");
					reset.Wait();
					if (LogActivity) Console.WriteLine("NSC : send done");
				}
			}
		}
	}
}
