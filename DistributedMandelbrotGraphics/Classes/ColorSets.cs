using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DistributedMandelbrotGraphics.Classes {

	public enum ColorSet {
		Standard,
		Warm,
		Cold,
		ChocolatePeach,
		RedBlue,
		Test
	}

	public class ColorSetInfo {

		public string Code { get; private set; }
		public string Name { get; private set; }
		public Color[] Points { get; private set; }

		public ToolStripMenuItem MenuItem { get; set; }

		public ColorSetInfo(string code, string name, params Color[] points) {
			Code = code;
			Name = name;
			Points = points;
		}

		public IntColor[] CreateColors() {
			return ColorSets.CreateColors(Points);
		}

	}

	public class ColorSets {

		private static ColorSetInfo[] _info;

		static ColorSets() {
			_info = new ColorSetInfo[] {
				new ColorSetInfo("Standard", "Standard", Color.Black, Color.White, Color.Blue, Color.Green),
				new ColorSetInfo("Warm", "Warm", Color.Red, Color.Yellow, Color.Orange, Color.Purple),
				new ColorSetInfo("Cold", "Cool", Color.Black, Color.Blue, Color.White, Color.DarkGreen),
				new ColorSetInfo("ChocolatePeach", "Chocolate peach", Color.Chocolate, Color.MintCream, Color.PeachPuff, Color.Black),
				new ColorSetInfo("RedBlue", "Red blue", Color.Red, Color.Black, Color.Blue, Color.White),
				new ColorSetInfo("GoldenTeal", "Golden teal", Color.SaddleBrown, Color.DarkGoldenrod, Color.Wheat, Color.Teal),
				new ColorSetInfo("RedCoral", "Red coral", Color.Maroon, Color.White, Color.DarkKhaki, Color.LightCoral),
				new ColorSetInfo("GreenBlue", "Green blue", Color.Black, Color.DarkGreen, Color.White, Color.DarkBlue),
				new ColorSetInfo("AquaCyan", "Aqua cyan", Color.LightCyan, Color.Black, Color.Aqua, Color.DarkKhaki),
				new ColorSetInfo("CrimsonKhaki", "Crimson khaki", Color.Crimson, Color.Yellow, Color.Black, Color.Khaki),
				new ColorSetInfo("LavenderSpring", "Lavender spring", Color.Lavender, Color.Black, Color.White, Color.SpringGreen),
				new ColorSetInfo("AzureFirebrick", "Azure firebrick", Color.Azure, Color.Black, Color.Firebrick, Color.Yellow),
				new ColorSetInfo("BlueMaroon", "Blue maroon", Color.Black, Color.DarkBlue, Color.DarkSlateGray, Color.Maroon),
				new ColorSetInfo("BeigeBlue", "Beige blue", Color.Black, Color.Beige, Color.White, Color.DarkSlateBlue),
				new ColorSetInfo("RoyalYellow", "Royal yellow", Color.LightYellow, Color.Black, Color.RoyalBlue, Color.White),
				new ColorSetInfo("LimeSky", "Lime sky", Color.LimeGreen, Color.Black, Color.DeepSkyBlue, Color.White),
				new ColorSetInfo("RedOrange", "Red orange", Color.Red, Color.Black, Color.Yellow, Color.Orange, Color.White),
				new ColorSetInfo("PinkBlue", "Pink blue", Color.White, Color.HotPink, Color.Black, Color.DarkBlue),
				new ColorSetInfo("VioletGold", "Violet gold", Color.DarkViolet, Color.Yellow, Color.Gold, Color.Black),
				new ColorSetInfo("OrangeSalmon", "Orange salmon", Color.White, Color.DarkOrange, Color.Black, Color.LightSalmon),
				new ColorSetInfo("BlackWhite", "Black and white", Color.Black, Color.White),
				new ColorSetInfo("Sepia", "Sepia", Color.Black, Color.Khaki, Color.LightYellow)
			};
		}

		public static void CreateMenuItems(ToolStripMenuItem menu, Action<ColorSetInfo> SetColor) {
			foreach (var info in _info) {
				info.MenuItem = new ToolStripMenuItem(info.Name);
				menu.DropDownItems.Add(info.MenuItem);
				info.MenuItem.Click += (object sender, EventArgs e) => { SetColor(info); };
			}
		}

		public static void SetMenu(ColorSetInfo info) {
			foreach (var color in _info) {
				color.MenuItem.Checked = color == info;
			}
		}

		public static ColorSetInfo Get(string code) {
			return _info.Single(i => i.Code == code);
		}

		public static int Index(ColorSetInfo info) {
			return _info.Select((c, i) => c == info ? i : (int?)null).Single(i => i.HasValue).Value;
		}

		public static IntColor[] CreateColors(params Color[] points) {
			List<IntColor> colors = new List<IntColor>();
			for (int i = 0; i < points.Length; i++) {
				Color color1 = points[i];
				Color color2 = points[(i + 1) % points.Length];
				int dist = Math.Max(Math.Max(
					Math.Abs(color1.R - color2.R),
					Math.Abs(color1.G - color2.G)),
					Math.Abs(color1.B - color2.B));
				for (int ofs = 0; ofs < dist; ofs++) {
					colors.Add(new IntColor(
						(byte)(color1.R + (color2.R - color1.R) * ofs / dist),
						(byte)(color1.G + (color2.G - color1.G) * ofs / dist),
						(byte)(color1.B + (color2.B - color1.B) * ofs / dist)
					));
				}
			}
			return colors.ToArray();
		}

		public static ColorSetInfo Previous(ColorSetInfo colorsInfo) {
			return _info[(Index(colorsInfo) + _info.Length - 1) % _info.Length];
		}

		public static ColorSetInfo Next(ColorSetInfo colorsInfo) {
			return _info[(Index(colorsInfo) + 1) % _info.Length];
		}

	}

}
