using MandelCalculation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedMandelbrotGraphics.Classes {

	public enum TaskState {
		Created,
		Waiting,
		Done
	}

	// An object that keeps track of a calculation task
	public class CalcTask : Calculation {

		private CalcTaskManager _calcTaskManager;

		public int BatchGroup { get; private set; }
		public TaskState State { get; private set; }
		public int X { get; private set; }
		public int Y { get; private set; }

		public void Offset(int dx, int dy) {
			X += dx;
			Y += dy;
		}

		public double CenterX => X + W / 2.0;
		public double CenterY => Y + H / 2.0;

		// Calculate the distance between a point and the center of this task
		public double Distance(int x, int y) {
			double distX = x - CenterX;
			double distY = y - CenterY;
			return Math.Sqrt(distX * distX + distY * distY);
		}

		public CalcTask(CalcTaskManager calcTaskManager, int batchGroup, int x, int y, int w, int h, decimal left, decimal top, decimal scale, CalcPrecision precision, int depth, SmoothingMode smoothing) : base(w, h, left, top, scale, precision, depth, smoothing) {
			_calcTaskManager = calcTaskManager;
			BatchGroup = batchGroup;
			State = TaskState.Created;
			X = x;
			Y = y;
		}

		public void SetState(TaskState newState) {
			if (newState != State) {
				State = newState;
				_calcTaskManager.EnqueueChange(new ImageChange(this));
			}
		}

		public void SetDone(int[,] pixels) {
			State = TaskState.Done;
			_calcTaskManager.EnqueueChange(new ImageChange(this, pixels));
		}

	}

}
