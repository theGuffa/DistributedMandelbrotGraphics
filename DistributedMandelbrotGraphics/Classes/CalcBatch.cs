using MandelCalculation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DistributedMandelbrotGraphics.Classes {

	public class CalcBatch {

		private CalcTaskManager _calcTaskManager;
		private bool _active;

		public bool Running { get; private set; }
		public int Count { get; private set; }

		public CalcBatch(CalcTaskManager calcTaskManager) {
			_calcTaskManager = calcTaskManager;
			_active = false;
			Running = false;
			Count = 0;
		}

		public void Add(List<CalcTask> tasks) {
			_calcTaskManager.AddTasks(tasks);
			Count = _calcTaskManager.TaskCount;
		}

		public void Offset(int dx, int dy) {
			if (_active) {
				_calcTaskManager.Offset(dx, dy);
			}
		}

		public void Deactivate() {
			_active = false;
			_calcTaskManager.ClearTasks();
		}

		private bool GetTask(out CalcTask task) {
			if (_active) {
				bool waiting;
				(waiting, task) = _calcTaskManager.GetTask();
				return waiting;
			} else {
				task = null;
				return false;
			}
		}

		public void RunAsync(CalcManager calc) {
			Task.Run(() => Run(calc));
		}

		public async Task Run(CalcManager calc) {
			_active = true;
			Running = true;
			while (GetTask(out CalcTask task)) {
				if (task != null) {
					if (calc.GetFreeNode(out CalcNode node)) {
						task.SetState(TaskState.Waiting);
						Task t = node.Calculate(task).ContinueWith((Task<CalcResult> result) => {
							CalcResult res = result.Result;
							node.AddSpeed(res.Pixels.Length, res.Micro);
							task.SetDone(res.Pixels);
						});
					} else {
						await Task.Yield();
					}
				} else {
					await Task.Yield();
				}
			}
			_active = false;
			Running = false;
		}

	}

}
