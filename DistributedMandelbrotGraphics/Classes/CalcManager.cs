using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DistributedMandelbrotGraphics.Classes {

	public class CalcManager {

		public List<CalcNode> Nodes;

		private UIManager _uiManager;

		public CalcManager(UIManager uiManager) {
			_uiManager = uiManager;
			Nodes = new List<CalcNode>();
		}
		
		// Get worker information from user settings and create nodes
		public void LoadNodes(Settings settings) {
			Add(new InternalCalcNode(this, Math.Max(1, Environment.ProcessorCount - 2), settings.InternalActive));
			Json.JsonArray nodes = settings.NodesArray;
			foreach (Json.JsonObject node in nodes.GetObjects()) {
				switch (node["type"].AsString) {
					case "HTTP": Add(new HttpCalcNode(this, "HTTP", node["url"].AsString, node["active"].AsBoolean ? NodeState.Unknown : NodeState.Disabled)); break;
					case "TCP": Add(new TcpCalcNode(this, "TCP", node["ip"].AsString, node["port"].AsInteger, node["active"].AsBoolean ? NodeState.Unknown : NodeState.Disabled)); break;
					default: throw new NotImplementedException();
				}
			}
		}

		// Save worker information to user settings
		public void SaveNodes(Settings settings) {
			settings.InternalActive = Nodes[0].State != NodeState.Disabled;
			settings.NodesArray = Json.Array.Add(Nodes.Skip(1), n => n.ToJson());
			settings.Save();
		}

		public async Task Recheck(int index) {
			Nodes[index].Close();
			await Check(Nodes[index]);
		}

		private async Task Check(CalcNode node) {
			node.State = NodeState.Checking;
			string name = await node.Check();
			if (name != null) {
				if (name.Length > 0) {
					node.Name = name;
				}
				node.State = NodeState.Idle;
			} else {
				node.State = NodeState.Failed;
			}
			SetNodeChanged(node, UIManager.NodeChange.Update);
		}

		public void Add(CalcNode node) {
			Nodes.Add(node);
			_uiManager.AddNode(node);
			_uiManager.SetNodeChanged(node, UIManager.NodeChange.Update);
			if (node.State == NodeState.Unknown) {
				Task.Run(async () => {
					await Check(node);
				});
			}
		}

		public void Remove(ListViewItem item) {
			int index = item.Index;
			CalcNode node = (CalcNode)item.Tag;
			if (!(node is InternalCalcNode)) {
				Nodes.RemoveAt(index);
				_uiManager.SetNodeChanged(node, UIManager.NodeChange.Remove);
			}
		}

		public bool GetFreeNode(out CalcNode freeNode) {
			foreach (CalcNode node in Nodes) {
				if (node.State == NodeState.Idle) {
					node.State = NodeState.Running;
					freeNode = node;
					return true;
				}
			}
			freeNode = null;
			return false;
		}

		public void SetNodeChanged(CalcNode node, UIManager.NodeChange change) {
			_uiManager.SetNodeChanged(node, change);
		}

		public async Task Enable(CalcNode node) {
			if (node is InternalCalcNode) {
				node.State = NodeState.Idle;
				SetNodeChanged(node, UIManager.NodeChange.Update);
			} else {
				await Check(node);
			}
		}

		public void Disable(CalcNode node) {
			node.Close();
			node.State = NodeState.Disabled;
			_uiManager.SetNodeChanged(node, UIManager.NodeChange.Update);
		}

	}

}
