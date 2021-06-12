using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DistributedMandelbrotGraphics {

	public partial class ImageSize : Form {

		public ImageSize(int w, int h) {
			InitializeComponent();
			ImageSizeNumericWidth.Value = w;
			ImageSizeNumericHeight.Value = h;
		}

	}

}
