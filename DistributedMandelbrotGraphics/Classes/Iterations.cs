using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DistributedMandelbrotGraphics.Classes {

	// An object for keeping an iteration value and a menu item
	public class IterationsInfo {

		public int Iterations { get; private set; }

		public ToolStripMenuItem MenuItem { get; set; }

		public IterationsInfo(int iterations) {
			Iterations = iterations;
		}

	}

	public static class Iterations {

		private static readonly IterationsInfo[] _info;

		static Iterations() {
			// Create list of iteration info items
			_info = new IterationsInfo[] {
				new IterationsInfo(100),
				new IterationsInfo(200),
				new IterationsInfo(400),
				new IterationsInfo(800),
				new IterationsInfo(1000),
				new IterationsInfo(2000),
				new IterationsInfo(4000),
				new IterationsInfo(8000),
				new IterationsInfo(10000),
				new IterationsInfo(20000),
				new IterationsInfo(40000),
				new IterationsInfo(800000),
				new IterationsInfo(1000000)
			};
		}

		public static int MinValue => _info[0].Iterations;
		public static int MaxValue => _info[_info.Length - 1].Iterations;

		// Create menu items and store the references in the iteration info items
		public static void CreateMenuItems(ToolStripMenuItem menu, Action<int> SetDepth) {
			foreach (var info in _info) {
				info.MenuItem = new ToolStripMenuItem(info.Iterations.ToString());
				menu.DropDownItems.Add(info.MenuItem);
				info.MenuItem.Click += (object sender, EventArgs e) => { SetDepth(info.Iterations); };
			}
		}

		// Mark the menu item of the selected iterations value
		public static void SetMenu(int depth) {
			foreach (var info in _info) {
				info.MenuItem.Checked = info.Iterations == depth;
			}
		}

		// Get index of the item with the given iterations
		public static int Index(int iterations) {
			int i = 0;
			while (_info[i].Iterations != iterations) {
				i++;
			}
			return i;
		}

		// Get iterations of previous item, or null if it is the first item
		public static int? Previous(int iterations) {
			int index = Index(iterations);
			if (index > 0) {
				return _info[index - 1].Iterations;
			} else {
				return null;
			}
		}

		// Get iterations of next item, or null if it is the last item
		public static int? Next(int iterations) {
			int index = Index(iterations);
			if (index < _info.Length - 1) {
				return _info[index + 1].Iterations;
			} else {
				return null;
			}
		}

	}

}
