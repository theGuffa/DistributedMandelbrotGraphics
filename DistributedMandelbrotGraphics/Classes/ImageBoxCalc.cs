using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DistributedMandelbrotGraphics.Classes {

	public class ImageBoxCalc {

		public decimal BoxW { get; private set; }
		public decimal BoxH { get; private set; }
		public decimal XOfs { get; private set; }
		public decimal YOfs { get; private set; }
		public decimal PicScale { get; private set; }

		public ImageBoxCalc(PictureBox box, int W, int H) {
			BoxW = box.Width;
			BoxH = box.Height;
			if (W <= BoxW && H <= BoxH) {
				PicScale = 1.0m;
				XOfs = Math.Floor((BoxW - W) / 2.0m);
				YOfs = Math.Floor((BoxH - H) / 2.0m);
			} else {
				decimal xScale = BoxW / (decimal)W;
				decimal yScale = BoxH / (decimal)H;
				if (yScale < xScale) {
					PicScale = yScale;
					XOfs = Math.Floor((BoxW - W * PicScale) / 2.0m);
					YOfs = 0m;
				} else {
					PicScale = xScale;
					XOfs = 0m;
					YOfs = Math.Floor((BoxH - H * PicScale) / 2.0m);
				}
			}
		}

		public (decimal fx, decimal fy) ImageToBox(int x, int y) {
			decimal fx = x * PicScale + XOfs;
			decimal fy = y * PicScale + YOfs;
			return (fx, fy);
		}

		public (decimal fx, decimal fy) BoxToImage(int x, int y) {
			decimal fx = (x - XOfs) / PicScale;
			decimal fy = (y - YOfs) / PicScale;
			return (fx, fy);
		}

		public (decimal fx, decimal fy) BoxToPicture(int x, int y) {
			decimal fx = (x - XOfs);
			decimal fy = (y - YOfs);
			return (fx, fy);
		}

	}

}
