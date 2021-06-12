using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DistributedMandelbrotGraphics.Classes {

	public class UIManager {

		public enum NodeChange {
			None,
			Update,
			Remove
		}

		private readonly object _sync;
		private readonly DMG _win;

		private int _progress;
		private bool _updateProgress;
		private Dictionary<CalcNode, NodeChange> _updateNode;
		private bool _updateImage;
		private CalcTaskManager _calcTaskManager;

		public bool WorkersVisible { get; set; }

		public UIManager(DMG window, CalcTaskManager calcTaskManager) {
			_sync = new object();
			_win = window;
			_updateProgress = false;
			_updateNode = new Dictionary<CalcNode, NodeChange>();
			_calcTaskManager = calcTaskManager;
		}

		public void UpdateImage() {
			_updateImage = true;
		}

		public void UpdateWorkers() {
			lock (_sync) {

				bool nodesChanged = false;
				foreach (ListViewItem item in _win.CalculatorList.Items) {
					CalcNode node = (CalcNode)item.Tag;
					NodeChange change = _updateNode[node];
					if (change != NodeChange.None) {
						_updateNode[node] = NodeChange.None;
						if (!nodesChanged) {
							nodesChanged = true;
							_win.CalculatorList.BeginUpdate();
						}
						switch (change) {
							case NodeChange.Update:
								item.SubItems[0].Text = node.Name;
								item.SubItems[1].Text = node.State.ToString();
								item.SubItems[2].Text = node.Speed;
								break;
							case NodeChange.Remove:
								_win.CalculatorList.Items.Remove(item);
								_updateNode.Remove(node);
								break;
						}
					}
				}
				if (nodesChanged) {
					_win.CalculatorList.EndUpdate();
				}

			}
			if (_updateProgress) {
				_updateProgress = false;
				_win.Progress.Value = _progress;
			}
		}

		public void Update(CalcManager _calcManager, ImageManager imageManager) {
			lock (_sync) {
				List<ImageChange> items = _calcTaskManager.DequeueChanges();
				foreach (ImageChange item in items) {
					if (item.Data != null) {
						imageManager.DrawTask(item.Task, item.Data);
					} else if (item.Created) {
						imageManager.DrawTaskCreated(item.Task);
					} else {
						imageManager.DrawTaskWaiting(item.Task);
					}
				}
			}
			if (_updateImage) {
				_updateImage = false;
				_win.ImageBox.Invalidate();
			}
		}

		public void AddNode(CalcNode node) {
			lock (_sync) {
				ListViewItem item = new ListViewItem(new string[] { node.Name, node.State.ToString(), node.PixelCount.ToString(), node.Speed });
				item.Tag = node;
				_win.CalculatorList.Items.Add(item);
				_updateNode[node] = NodeChange.None;
			}
		}

		public void SetNodeChanged(CalcNode node, NodeChange change) {
			if (WorkersVisible) {
				lock (_sync) {
					if (change > _updateNode[node]) {
						_updateNode[node] = change;
					}
				}
			}
		}

		public void SetProgress(int value) {
			_progress = value;
			_updateProgress = true;
		}

	}

}
