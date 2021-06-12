using DistributedMandelbrotGraphics.Classes;
using MandelCalculation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DistributedMandelbrotGraphics {

	public partial class DMG : Form {

		private readonly UIManager _uiManager;
		private readonly CalcManager _calcManager;
		private ImageManager _imageManager;
		private CalcTaskManager _calcTaskManager;
		private Timer _updateTimer, _updateWorkersTimer, _panTimer;
		private CalcBatch _batch;
		private string _fileName;
		private Settings _settings;

		private bool _panning = false;
		private int _panX, _panY;
		private DateTime _lastMove;

		public DMG() {
			InitializeComponent();
			_calcTaskManager = new CalcTaskManager();
			_uiManager = new UIManager(this, _calcTaskManager);
			_calcManager = new CalcManager(_uiManager);
			_batch = null;
		}

		private void DMG_Load(object sender, EventArgs e) {
			_settings = new Settings();
			_calcManager.LoadNodes(_settings);
			_imageManager = new ImageManager(_settings.Width, _settings.Height, _settings.Parts, ImageBox, _uiManager, _calcTaskManager);
			Draw();
			_updateTimer = new Timer { Interval = 20 };
			_updateTimer.Tick += UpdateTick;
			_updateTimer.Start();
			_updateWorkersTimer = new Timer { Interval = 50 };
			_updateWorkersTimer.Tick += UpdateWorkersTick;
			_updateWorkersTimer.Start();
			_panTimer = new Timer { Interval = 100 };
			_panTimer.Tick += PanTick;
			ImageBox.MouseWheel += ImageBox_MouseWheel;
			ColorSets.CreateMenuItems(MenuImageColorSet, SetColor);
			ColorSets.SetMenu(_imageManager.ColorsInfo);
			Depths.CreateMenuItems(MenuCalculationIterations, SetDepth);
			Depths.SetMenu(_imageManager.Depth);
			SetParameters();
			SetPartsMenu();
			SetShowWorkers(_settings.ShowWorkers, false);
		}

		private void UpdateTick(object sender, EventArgs e) {
			_uiManager.Update(_calcManager, _imageManager);
		}

		private void UpdateWorkersTick(object sender, EventArgs e) {
			_uiManager.UpdateWorkers();
		}

		private void DeactivateBatch() {
			if (_batch != null) {
				_batch.Deactivate();
				_batch = null;
				_imageManager.NextBatch();
				_imageManager.ClearProgress();
			}
		}

		private void AddTasks(List<CalcTask> tasks) {
			if (_batch == null) {
				_batch = new CalcBatch(_calcTaskManager);
			}
			_batch.Add(tasks);
			if (_batch.Running) {
				_imageManager.AddTasks(tasks.Count);
			} else {
				_imageManager.ClearProgress();
				_imageManager.AddTasks(tasks.Count);
				_batch.RunAsync(_calcManager);
			}
		}

		private void Draw(float centerX = 0.5f, float centerY = 0.5f) {
			DeactivateBatch();
			List<CalcTask> tasks = _imageManager.DivideCalculation(centerX, centerY);
			AddTasks(tasks);
		}

		private void CheckImageBoxMode() {
			if (_imageManager.W < ImageBox.Width && _imageManager.H < ImageBox.Height) {
				ImageBox.SizeMode = PictureBoxSizeMode.CenterImage;
			} else {
				ImageBox.SizeMode = PictureBoxSizeMode.Zoom;
			}
		}

		private void PanBy(int dx, int dy) {
			ImageBoxCalc calc = _imageManager.GetCalc();
			dx = (int)Math.Round(dx / calc.PicScale);
			dy = (int)Math.Round(dy / calc.PicScale);
			if (dx != 0 || dy != 0) {
				_batch.Offset(dx, dy);
				List<CalcTask> tasks = _imageManager.Pan(dx, dy);
				AddTasks(tasks);
				SetCoordinatesInfo();
			}
		}

		private void Zoom(int x, int y, bool zoomIn, decimal zoomRate) {
			(float cx, float cy) = _imageManager.Zoom(x, y, zoomIn, zoomRate);
			if (_imageManager.AutoPrecision) {
				if (_imageManager.CheckAutoPrecision()) {
					SetPrecisionMenu();
				}
			}
			Draw(cx, cy);
			SetCoordinatesInfo();
		}

		private void Pan(int x, int y, bool force) {
			int dx = x - _panX;
			int dy = y - _panY;
			if (force || Math.Abs(dx) > 5 || Math.Abs(dy) > 5) {
				if (dx != 0 || dy != 0) {
					_lastMove = DateTime.UtcNow;
					_panX = x;
					_panY = y;
					PanBy(dx, dy);
				}
			}
		}

		private void StartPan(int x, int y) {
			_panning = true;
			_panX = x;
			_panY = y;
			_lastMove = DateTime.UtcNow;
			_panTimer.Start();
		}

		private void StopPan(int x, int y) {
			Pan(x, y, true);
			StopPan();
		}

		private void StopPan() {
			_panning = false;
			_panTimer.Stop();
		}

		private void PanTick(object sender, EventArgs e) {
			if (_panning) {
				Point p = MousePosition;
				p = ImageBox.PointToClient(p);
				if ((DateTime.UtcNow - _lastMove).TotalMilliseconds > 300 && (p.X != _panX || p.Y != _panY)) {
					Pan(p.X, p.Y, true);
				}
			}
		}

		#region Set info

		private void SetSizeInfo() {
			StatusLabelSize.Text = $"{_imageManager.W} x {_imageManager.H}";
		}

		private void SetColorInfo() {
			StatusLabelColors.Text = $"Colors: {_imageManager.ColorsInfo.Name} + {_imageManager.ColorOffset}";
		}

		private void SetPrecisionInfo() {
			StatusLabelPrecision.Text = $"{(_imageManager.AutoPrecision ? "Auto: " : String.Empty)}{_imageManager.Precision}, {_imageManager.Depth} iterations";
		}

		private void SetSmoothingInfo() {
			StatusLabelSmoothing.Text = $"Smoothing: {_imageManager.Smoothing}";
		}

		private void SetCoordinatesInfo() {
			StatusLabelCoordinates.Text = FormattableString.Invariant($"Coor: {_imageManager.Left} ; {_imageManager.Top} : {_imageManager.Scale}");
		}

		#endregion

		#region Set menu

		private void CheckMenuItem(ToolStripMenuItem item, params ToolStripMenuItem[] items) {
			foreach (var i in items) {
				i.Checked = i == item;
			}
		}

		private void SetSizeMenu() {
			int w = _imageManager.W;
			int h = _imageManager.H;
			ToolStripMenuItem size =
				w == 800 && h == 600 ? MenuImageSize800x600 :
				w == 1024 && h == 768 ? MenuImageSize1024x768 :
				w == 1280 && h == 800 ? MenuImageSize1280x800 :
				w == 1280 && h == 1024 ? MenuImageSize1280x1024 :
				w == 1366 & h == 768 ? MenuImageSize1366x768 :
				w == 1400 & h == 1050 ? MenuImageSize1400x1050 :
				w == 1440 & h == 900 ? MenuImageSize1440x900 :
				w == 1600 & h == 1200 ? MenuImageSize1600x1200 :
				w == 1680 & h == 1050 ? MenuImageSize1680x1050 :
				w == 1920 & h == 1200 ? MenuImageSize1920x1200 :
				w == 2048 & h == 1536 ? MenuImageSize2048x1536 :
				w == 2560 & h == 1600 ? MenuImageSize2560x1600 :
				w == 1280 & h == 720 ? MenuImageSize1280x720 :
				w == 1920 & h == 1080 ? MenuImageSize1920x1080 :
				w == 3840 & h == 2160 ? MenuImageSize3840x2160 :
				w == 7680 & h == 4320 ? MenuImageSize7680x4320 :
				w == 3000 & h == 2000 ? MenuImageSize3000x2000 :
				w == 4245 & h == 2830 ? MenuImageSize4245x2830 :
				w == 6000 & h == 4000 ? MenuImageSize6000x4000 :
				w == 2832 & h == 2124 ? MenuImageSize2832x2124 :
				w == 4000 & h == 3000 ? MenuImageSize4000x3000 :
				w == 5664 & h == 4248 ? MenuImageSize5664x4248 :
				MenuImageSizeCustom;
			CheckMenuItem(size,
				MenuImageSize800x600, MenuImageSize1024x768, MenuImageSize1280x800, MenuImageSize1280x1024,
				MenuImageSize1366x768, MenuImageSize1400x1050, MenuImageSize1440x900, MenuImageSize1600x1200,
				MenuImageSize1680x1050, MenuImageSize1920x1200, MenuImageSize2048x1536, MenuImageSize2560x1600,
				MenuImageSize1280x720, MenuImageSize1920x1080, MenuImageSize3840x2160, MenuImageSize7680x4320,
				MenuImageSize3000x2000, MenuImageSize4245x2830, MenuImageSize6000x4000,
				MenuImageSize2832x2124, MenuImageSize4000x3000, MenuImageSize5664x4248,
				MenuImageSizeCustom
			);
			SetSizeInfo();
		}

		private void SetColorMenu() {
			ColorSets.SetMenu(_imageManager.ColorsInfo);
			SetColorInfo();
		}

		private void SetPrecisionChangeMenu() {
			MenuCalculationPrecisionDecrease.Enabled = !_imageManager.AutoPrecision && _imageManager.Precision != CalcPrecision.Single;
			MenuCalculationPrecisionIncrease.Enabled = !_imageManager.AutoPrecision && _imageManager.Precision != CalcPrecision.Decimal;
		}

		private void SetPrecisionMenu() {
			ToolStripMenuItem item =
				_imageManager.Precision == CalcPrecision.Single ? MenuCalculationPrecisionSingle :
				_imageManager.Precision == CalcPrecision.Double ? MenuCalculationPrecisionDouble :
				MenuCalculationPrecisionDecimal;
			CheckMenuItem(item,
				MenuCalculationPrecisionSingle,
				MenuCalculationPrecisionDouble,
				MenuCalculationPrecisionDecimal
			);
			SetPrecisionChangeMenu();
			SetPrecisionInfo();
		}

		private void SetAutoPrecisionMenu() {
			MenuCalculationPrecisionAuto.Checked = _imageManager.AutoPrecision;
			MenuCalculationPrecisionSingle.Enabled = !_imageManager.AutoPrecision;
			MenuCalculationPrecisionDouble.Enabled = !_imageManager.AutoPrecision;
			MenuCalculationPrecisionDecimal.Enabled = !_imageManager.AutoPrecision;
			SetPrecisionChangeMenu();
			SetPrecisionInfo();
		}

		private void SetDepthMenu() {
			Depths.SetMenu(_imageManager.Depth);
			MenuCalculationIterationsDecrease.Enabled = _imageManager.Depth != Depths.MinValue;
			MenuCalculationIterationsIncrease.Enabled = _imageManager.Depth != Depths.MaxValue;
			SetPrecisionInfo();
		}

		private void SetSmoothingMenu() {
			MenuCalculationSmoothingNone.Checked = _imageManager.Smoothing == SmoothingMode.None;
			MenuCalculationSmoothingDouble.Checked = _imageManager.Smoothing == SmoothingMode.Double;
			MenuCalculationSmoothingQuadruple.Checked = _imageManager.Smoothing == SmoothingMode.Quadruple;
			SetSmoothingInfo();
		}

		private void SetPartsMenu() {
			ToolStripMenuItem item =
				_imageManager.Parts == 250 ? MenuCalculationParts250 :
				_imageManager.Parts == 500 ? MenuCalculationParts500 :
				_imageManager.Parts == 1000 ? MenuCalculationParts1000 :
				MenuCalculationParts2000;
			CheckMenuItem(item,
				MenuCalculationParts250,
				MenuCalculationParts500,
				MenuCalculationParts1000,
				MenuCalculationParts2000
			);
		}

		private void SetParts(int parts, bool saveSettings) {
			_imageManager.Parts = parts;
			SetPartsMenu();
			if (saveSettings) {
				_settings.Parts = parts;
				_settings.Save();
			}
		}

		private void SetShowWorkers(bool show, bool saveSettings) {
			MenuCalculationShowWorkers.Checked = show;
			CalculatorList.Visible = MenuCalculationShowWorkers.Checked;
			_uiManager.WorkersVisible = show;
			if (saveSettings) {
				_settings.ShowWorkers = show;
				_settings.Save();
			}
		}

		#endregion

		#region Set properties

		private void SetSize(int w, int h) {
			if (w != _imageManager.W || h != _imageManager.H) {
				DeactivateBatch();
				_imageManager.SetSize(w, h);
				SetSizeMenu();
				CheckImageBoxMode();
				Draw();
				_settings.Width = w;
				_settings.Height = h;
				_settings.Save();
			}
		}

		private void SetColor(ColorSetInfo info) {
			if (info != _imageManager.ColorsInfo) {
				_imageManager.SetColor(info);
				SetColorMenu();
			}
		}

		private void SetColorOffset(int offset) {
			_imageManager.SetColorOffset(offset);
			SetColorInfo();
		}

		private void SetPrecision(CalcPrecision precision) {
			if (precision != _imageManager.Precision) {
				_imageManager.Precision = precision;
				SetPrecisionMenu();
				Draw();
			}
		}

		private void ToggleAutoPrecision() {
			_imageManager.AutoPrecision = !_imageManager.AutoPrecision;
			SetAutoPrecisionMenu();
			if (_imageManager.AutoPrecision) {
				if (_imageManager.CheckAutoPrecision()) {
					SetPrecisionMenu();
					Draw();
				}
			}
		}

		private void SetDepth(int depth) {
			if (depth != _imageManager.Depth) {
				_imageManager.Depth = depth;
				SetDepthMenu();
				Draw();
			}
		}

		private void SetSmoothing(SmoothingMode mode) {
			if (mode != _imageManager.Smoothing) {
				_imageManager.SetSmoothing(mode);
				SetSmoothingMenu();
				Draw();
			}
		}

		private void SetParts(int parts) {
			if (parts != _imageManager.Parts) {
				SetParts(parts, true);
			}
		}

		private void SetParameters() {
			SetSizeMenu();
			SetColorMenu();
			SetPrecisionMenu();
			SetAutoPrecisionMenu();
			SetDepthMenu();
			SetSmoothingMenu();
			CheckImageBoxMode();
			SetCoordinatesInfo();
		}

		#endregion

		#region ImageBox

		private void ImageBox_MouseWheel(object sender, MouseEventArgs e) {
			decimal zoom = 1.1m;
			switch (Control.ModifierKeys) {
				case Keys.Shift: zoom = 1.01m; break;
				case Keys.Control: zoom = 1.2m; break;
				case Keys.Alt: zoom = 1.5m; break;
			}
			Zoom(e.X, e.Y, e.Delta > 0, zoom);
		}

		private void ImageBox_Resize(object sender, EventArgs e) {
			CheckImageBoxMode();
		}

		private void ImageBox_MouseDown(object sender, MouseEventArgs e) {
			if (e.Button == MouseButtons.Left) {
				StartPan(e.X, e.Y);
			}
		}

		private void ImageBox_MouseUp(object sender, MouseEventArgs e) {
			if (_panning) {
				StopPan(e.X, e.Y);
			}
		}

		private void ImageBox_MouseMove(object sender, MouseEventArgs e) {
			if (_panning) {
				Pan(e.X, e.Y, false);
			}
		}

		private void ImageBox_MouseLeave(object sender, EventArgs e) {
			if (_panning) {
				StopPan();
			}
		}

		#endregion

		#region Menu

		private void MenuFileNew_Click(object sender, EventArgs e) {
			DeactivateBatch();
			_imageManager.Reset();
			SetParameters();
			Draw();
		}

		private void MenuFileLoad_Click(object sender, EventArgs e) {
			OpenFileDialog.Title = "Load parameters";
			OpenFileDialog.FileName = _fileName;
			DialogResult result = OpenFileDialog.ShowDialog();
			if (result == DialogResult.OK) {
				DeactivateBatch();
				string name = OpenFileDialog.FileName;
				if (_imageManager.LoadCoordinates(name)) {
					_fileName = Path.GetFileNameWithoutExtension(name);
					SetParameters();
					Draw();
				}
			}
		}

		private void MenuFileSaveAs_Click(object sender, EventArgs e) {
			SaveFileDialog.Title = "Save parameters";
			SaveFileDialog.FileName = _fileName;
			DialogResult result = SaveFileDialog.ShowDialog();
			if (result == DialogResult.OK) {
				string name = SaveFileDialog.FileName;
				_imageManager.SaveCoordinates(name);
				_fileName = Path.GetFileNameWithoutExtension(name);
			}
		}

		private void MenuFileExit_Click(object sender, EventArgs e) {
			DeactivateBatch();
			Close();
		}

		private void MenuImageExportAs_Click(object sender, EventArgs e) {
			ExportImageDialog.Title = "Export image";
			ExportImageDialog.FileName = _fileName;
			DialogResult result = ExportImageDialog.ShowDialog();
			if (result == DialogResult.OK) {
				string name = ExportImageDialog.FileName;
				_imageManager.SaveImage(name);
				_fileName = Path.GetFileNameWithoutExtension(name);
			}
		}

		private void MenuImageSize800x600_Click(object sender, EventArgs e) => SetSize(800, 600);
		private void MenuImageSize1024x768_Click(object sender, EventArgs e) => SetSize(1024, 768);
		private void MenuImageSize1280x800_Click(object sender, EventArgs e) => SetSize(1280, 800);
		private void MenuImageSize1280x1024_Click(object sender, EventArgs e) => SetSize(1280, 1024);
		private void MenuImageSize1366x768_Click(object sender, EventArgs e) => SetSize(1366, 768);
		private void MenuImageSize1400x1050_Click(object sender, EventArgs e) => SetSize(1400, 1050);
		private void MenuImageSize1440x900_Click(object sender, EventArgs e) => SetSize(1440, 900);
		private void MenuImageSize1600x1200_Click(object sender, EventArgs e) => SetSize(1600, 1200);
		private void MenuImageSize1680x1050_Click(object sender, EventArgs e) => SetSize(1680, 1050);
		private void MenuImageSize1920x1200_Click(object sender, EventArgs e) => SetSize(1920, 1200);
		private void MenuImageSize2048x1536_Click(object sender, EventArgs e) => SetSize(2048, 1536);
		private void MenuImageSize2560x1600_Click(object sender, EventArgs e) => SetSize(2560, 1600);
		private void MenuImageSize1280x720_Click(object sender, EventArgs e) => SetSize(1280, 720);
		private void MenuImageSize1920x1080_Click(object sender, EventArgs e) => SetSize(1920, 1080);
		private void MenuImageSize3840x2160_Click(object sender, EventArgs e) => SetSize(3840, 2160);
		private void MenuImageSize7680x4320_Click(object sender, EventArgs e) => SetSize(7680, 4320);
		private void MenuImageSize3000x2000_Click(object sender, EventArgs e) => SetSize(3000, 2000);
		private void MenuImageSize4245x2830_Click(object sender, EventArgs e) => SetSize(4245, 2830);
		private void MenuImageSize6000X4000_Click(object sender, EventArgs e) => SetSize(6000, 4000);
		private void MenuImageSize2832x2124_Click(object sender, EventArgs e) => SetSize(2832, 2124);
		private void MenuImageSize4000x3000_Click(object sender, EventArgs e) => SetSize(4000, 3000);
		private void MenuImageSize5664x4248_Click(object sender, EventArgs e) => SetSize(5664, 4248);

		private void MenuImageSizeCustom_Click(object sender, EventArgs e) {
			ImageSize size = new ImageSize(_imageManager.W, _imageManager.H);
			DialogResult result = size.ShowDialog();
			if (result == DialogResult.OK) {
				int w = (int)size.ImageSizeNumericWidth.Value;
				int h = (int)size.ImageSizeNumericHeight.Value;
				SetSize(w, h);
			}
		}

		private void MenuImageColorSetPrevious_Click(object sender, EventArgs e) => SetColor(ColorSets.Previous(_imageManager.ColorsInfo));
		private void MenuImageColorSetNext_Click(object sender, EventArgs e) => SetColor(ColorSets.Next(_imageManager.ColorsInfo));

		private void MenuImageColorOffsetDecrease10_Click(object sender, EventArgs e) => SetColorOffset(_imageManager.ColorOffset - 10);
		private void MenuImageColorOffsetIncrease10_Click(object sender, EventArgs e) => SetColorOffset(_imageManager.ColorOffset + 10);
		private void MenuImageColorOffsetDecrease1_Click(object sender, EventArgs e) => SetColorOffset(_imageManager.ColorOffset - 1);
		private void MenuImageColorOffsetIncrease1_Click(object sender, EventArgs e) => SetColorOffset(_imageManager.ColorOffset + 1);

		private void MenuCalculationPrecisionDecrease_Click(object sender, EventArgs e) {
			switch (_imageManager.Precision) {
				case CalcPrecision.Double: SetPrecision(CalcPrecision.Single); break;
				case CalcPrecision.Decimal: SetPrecision(CalcPrecision.Double); break;
			}
		}

		private void MenuCalculationPrecisionIncrease_Click(object sender, EventArgs e) {
			switch (_imageManager.Precision) {
				case CalcPrecision.Single: SetPrecision(CalcPrecision.Double); break;
				case CalcPrecision.Double: SetPrecision(CalcPrecision.Decimal); break;
			}
		}

		private void MenuCalculationPrecisionSingle_Click(object sender, EventArgs e) => SetPrecision(CalcPrecision.Single);
		private void MenuCalculationPrecisionDouble_Click(object sender, EventArgs e) => SetPrecision(CalcPrecision.Double);
		private void MenuCalculationPrecisionDecimal_Click(object sender, EventArgs e) => SetPrecision(CalcPrecision.Decimal);

		private void MenuCalculationPrecisionAuto_Click(object sender, EventArgs e) => ToggleAutoPrecision();

		private void MenuCalculationIterationsDecrease_Click(object sender, EventArgs e) {
			int? depth = Depths.Previous(_imageManager.Depth);
			if (depth.HasValue) {
				SetDepth(depth.Value);
			}
		}

		private void MenuCalculationIterationsIncrease_Click(object sender, EventArgs e) {
			int? depth = Depths.Next(_imageManager.Depth);
			if (depth.HasValue) {
				SetDepth(depth.Value);
			}
		}

		private void MenuCalculationParts250_Click(object sender, EventArgs e) => SetParts(250);
		private void MenuCalculationParts500_Click(object sender, EventArgs e) => SetParts(500);
		private void MenuCalculationParts1000_Click(object sender, EventArgs e) => SetParts(1000);
		private void MenuCalculationParts2000_Click(object sender, EventArgs e) => SetParts(2000);

		private void MenuCalculationSmoothingNone_Click(object sender, EventArgs e) => SetSmoothing(SmoothingMode.None);
		private void MenuCalculationSmoothingDouble_Click(object sender, EventArgs e) => SetSmoothing(_imageManager.Smoothing == SmoothingMode.Double ? SmoothingMode.None : SmoothingMode.Double);
		private void MenuCalculationSmoothingQuadruple_Click(object sender, EventArgs e) => SetSmoothing(_imageManager.Smoothing == SmoothingMode.Quadruple ? SmoothingMode.None : SmoothingMode.Quadruple);

		private void MenuCalculationShowWorkers_Click(object sender, EventArgs e) {
			SetShowWorkers(!MenuCalculationShowWorkers.Checked, true);
		}

		private void MenuHelpAbout_Click(object sender, EventArgs e) {
			new About().ShowDialog();
		}

		private void MenuHelpQuickGuide_Click(object sender, EventArgs e) {
			new QuickGuide().ShowDialog();
		}

		#endregion

		#region Calculator List Menu

		private void CalculatorListMenuAdd_Click(object sender, EventArgs e) {
			AddCalculator add = new AddCalculator();
			DialogResult result = add.ShowDialog();
			if (result == DialogResult.OK) {
				_calcManager.Add(new TcpCalcNode(_calcManager, add.AddCalculatorTextBoxIP.Text, Int32.Parse(add.AddCalculatorTextBoxPort.Text)));
				_calcManager.SaveNodes(_settings);
			}
		}

		private void CalculatorListMenuRemove_Click(object sender, EventArgs e) {
			ListView.SelectedListViewItemCollection selected = CalculatorList.SelectedItems;
			foreach (ListViewItem item in selected) {
				_calcManager.Remove(item);
			}
			_calcManager.SaveNodes(_settings);
		}

		private void CalculatorListMenuRecheck_Click(object sender, EventArgs e) {
			ListView.SelectedListViewItemCollection selected = CalculatorList.SelectedItems;
			foreach (ListViewItem item in selected) {
				int index = item.Index;
				Task.Run(async () => {
					await _calcManager.Recheck(index);
					_calcManager.SaveNodes(_settings);
				});
			}
		}

		private async Task Enable(List<CalcNode> nodes) {
			foreach (CalcNode node in nodes) {
				await _calcManager.Enable(node);
			}
			_calcManager.SaveNodes(_settings);
		}

		private void CalculatorListMenuEnable_Click(object sender, EventArgs e) {
			ListView.SelectedListViewItemCollection selected = CalculatorList.SelectedItems;
			List<CalcNode> nodes = new List<CalcNode>();
			foreach (ListViewItem item in selected) {
				nodes.Add((CalcNode)item.Tag);
			}
			Task.Run(() => Enable(nodes));
		}

		private void CalculatorListMenuDisable_Click(object sender, EventArgs e) {
			ListView.SelectedListViewItemCollection selected = CalculatorList.SelectedItems;
			foreach (ListViewItem item in selected) {
				CalcNode node = (CalcNode)item.Tag;
				_calcManager.Disable(node);
			}
			_calcManager.SaveNodes(_settings);
		}

		#endregion

		#region Image Menu

		private void ImageMenuCenter_Click(object sender, EventArgs e) {
			Point p = new Point(ImageMenu.Left, ImageMenu.Top);
			p = ImageBox.PointToClient(p);
			int dx = ImageBox.Width / 2 - p.X;
			int dy = ImageBox.Height / 2 - p.Y;
			PanBy(dx, dy);
		}

		private void ImageMenuZoom(bool zoomIn, decimal zoomRate) {
			Point p = new Point(ImageMenu.Left, ImageMenu.Top);
			p = ImageBox.PointToClient(p);
			Zoom(p.X, p.Y, zoomIn, zoomRate);
		}

		private void ImageMenuZoomIn1_Click(object sender, EventArgs e) => ImageMenuZoom(true, 1.01m);
		private void ImageMenuZoomIn10_Click(object sender, EventArgs e) => ImageMenuZoom(true, 1.1m);
		private void ImageMenuZoomIn20_Click(object sender, EventArgs e) => ImageMenuZoom(true, 1.2m);
		private void ImageMenuZoomIn50_Click(object sender, EventArgs e) => ImageMenuZoom(true, 1.5m);
		private void ImageMenuZoomOut1_Click(object sender, EventArgs e) => ImageMenuZoom(false, 1.01m);
		private void ImageMenuZoomOut10_Click(object sender, EventArgs e) => ImageMenuZoom(false, 1.1m);
		private void ImageMenuZoomOut20_Click(object sender, EventArgs e) => ImageMenuZoom(false, 1.2m);
		private void ImageMenuZoomOut50_Click(object sender, EventArgs e) => ImageMenuZoom(false, 1.5m);

		#endregion

	}

}
