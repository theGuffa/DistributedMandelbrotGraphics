using System;
using System.Collections.Generic;

namespace DistributedMandelbrotGraphics.Classes {

	public struct ImageChange {

		public CalcTask Task { get; private set; }
		public int[,] Data { get; private set; }
		public bool Created { get; private set; }

		public ImageChange(CalcTask task, int[,] data) {
			Task = task;
			Data = data;
			Created = false;
		}

		public ImageChange(CalcTask task) {
			Task = task;
			Data = null;
			Created = task != null && task.State == TaskState.Created;
		}

	}

	public class CalcTaskManager {

		private readonly object _sync;
		private readonly List<CalcTask> _tasks;
		private readonly Queue<ImageChange> _changes;

		public CalcTaskManager() {
			_sync = new Object();
			_tasks = new List<CalcTask>();
			_changes = new Queue<ImageChange>();
		}

		public int TaskCount {
			get {
				lock (_sync) {
					return _tasks.Count;
				}
			}
		}

		public void AddTasks(List<CalcTask> tasks) {
			lock (_sync) {
				_tasks.AddRange(tasks);
			}
		}

		public void Offset(int dx, int dy) {
			lock (_sync) {
				foreach (var task in _tasks) {
					task.Offset(dx, dy);
				}
			}
		}

		public (bool waiting, CalcTask task) GetTask() {
			bool waiting = false;
			lock (_sync) {
				foreach (CalcTask t in _tasks) {
					switch (t.State) {
						case TaskState.Created:
							return (true, t);
						case TaskState.Waiting:
							waiting = true;
							break;
					}
				}
			}
			return (waiting, null);
		}


		public void ClearTasks() {
			lock (_sync) {
				_tasks.Clear();
			}
		}

		//public List<CalcTask> GetDone() {
		//	List<CalcTask> done = new List<CalcTask>();
		//	lock (_sync) {
		//		_tasks.RemoveAll(c => {
		//			if (c.State == TaskState.Done) {
		//				done.Add(c);
		//				return true;
		//			} else {
		//				return false;
		//			}
		//		});
		//	}
		//	return done;
		//}

		public void EnqueueChange(ImageChange change) {
			lock (_sync) {
				_changes.Enqueue(change);
			}
		}

		public List<ImageChange> DequeueChanges() {
			List<ImageChange> changes = new List<ImageChange>();
			lock (_sync) {
				while (_changes.TryDequeue(out ImageChange change)) {
					changes.Add(change);
				}
			}
			return changes;
		}

	}

}
