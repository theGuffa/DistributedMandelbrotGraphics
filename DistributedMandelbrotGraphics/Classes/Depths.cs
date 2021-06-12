using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DistributedMandelbrotGraphics.Classes {

	public class DepthInfo {

		public int Depth { get; private set; }

		public ToolStripMenuItem MenuItem { get; set; }

		public DepthInfo(int depth) {
			Depth = depth;
		}

	}

	public static class Depths {

		private static DepthInfo[] _info;

		static Depths() {
			_info = new DepthInfo[] {
				new DepthInfo(100),
				new DepthInfo(200),
				new DepthInfo(400),
				new DepthInfo(800),
				new DepthInfo(1000),
				new DepthInfo(2000),
				new DepthInfo(4000),
				new DepthInfo(8000),
				new DepthInfo(10000),
				new DepthInfo(20000),
				new DepthInfo(40000),
				new DepthInfo(800000),
				new DepthInfo(1000000)
			};
		}

		public static int MinValue => _info[0].Depth;
		public static int MaxValue => _info[_info.Length - 1].Depth;

		public static void CreateMenuItems(ToolStripMenuItem menu, Action<int> SetDepth) {
			foreach (var info in _info) {
				info.MenuItem = new ToolStripMenuItem(info.Depth.ToString());
				menu.DropDownItems.Add(info.MenuItem);
				info.MenuItem.Click += (object sender, EventArgs e) => { SetDepth(info.Depth); };
			}
		}

		public static void SetMenu(int depth) {
			foreach (var info in _info) {
				info.MenuItem.Checked = info.Depth == depth;
			}
		}

		public static int Index(int depth) {
			return _info.Select((c, i) => c.Depth == depth ? i : (int?)null).Single(i => i.HasValue).Value;
		}

		public static int? Previous(int depth) {
			int index = Index(depth);
			if (index > 0) {
				return _info[index - 1].Depth;
			} else {
				return null;
			}
		}

		public static int? Next(int depth) {
			int index = Index(depth);
			if (index < _info.Length - 1) {
				return _info[index + 1].Depth;
			} else {
				return null;
			}
		}

	}

}
