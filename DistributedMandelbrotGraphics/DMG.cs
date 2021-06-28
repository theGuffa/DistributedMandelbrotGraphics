using DistributedMandelbrotGraphics.Classes;
using MandelCalculation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DistributedMandelbrotGraphics {

	public partial class DMG : Form {

		private readonly UIManager _uiManager;
		private readonly CalcManager _calcManager;
		private ImageManager _imageManager;
		private CalcTaskManager _calcTaskManager;
		private Timer _updateImageTimer, _updateWorkersTimer, _panTimer;
		private CalcBatch _batch;
		private string _fileName;
		private decimal _sequenceZoom;
		private bool _sequenceRepeat;
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
			_fileName = null;
			_sequenceZoom = 1.01m;
			_sequenceRepeat = false;
		}

		private void DMG_Load(object sender, EventArgs e) {
			// Get user settings for the application
			_settings = new Settings();
			// Populate the worker list
			_calcManager.LoadNodes(_settings);
			// Create internal array and image from size settings
			_imageManager = new ImageManager(_settings.Width, _settings.Height, _settings.Parts, ImageBox, _uiManager, _calcTaskManager, OnComplete);
			// Draw initial set
			Draw();
			
			// Create a timer for updating the image box
			_updateImageTimer = new Timer { Interval = Settings.UpdateImageInterval };
			_updateImageTimer.Tick += UpdateImageTick;
			_updateImageTimer.Start();

			// Create a timer for updating the worker list and the progress bar
			_updateWorkersTimer = new Timer { Interval = Settings.UpdateWorkersInterval };
			_updateWorkersTimer.Tick += UpdateWorkersTick;
			_updateWorkersTimer.Start();

			// Create a timer for detecting a period of no movement during panning
			_panTimer = new Timer { Interval = Settings.PanInterval };
			_panTimer.Tick += PanTick;
			
			// Mouse wheel events can't be set in the form designer
			ImageBox.MouseWheel += ImageBox_MouseWheel;

			// Create menu items for color sets and iterations
			ColorSets.CreateMenuItems(MenuImageColorSet, SetColor);
			ColorSets.SetMenu(_imageManager.ColorsInfo);
			Iterations.CreateMenuItems(MenuCalculationIterations, SetIterations);
			Iterations.SetMenu(_imageManager.Iterations);

			// Set menu options
			SetParameters();
			SetPartsMenu();
			SetShowWorkers(_settings.ShowWorkers, false);
		}

		// Callback from the image manager when all results has been put into the image
		private void OnComplete() {
			// Run next step if automatic sequence is running and the zoom hasn't reached the limit
			if (_sequenceRepeat) {
				if (_imageManager.Scale * _imageManager.W < 60.0m) {
					if (InvokeRequired) {
						// Invoke the method in the main thread if we are in a different thread
						Invoke(new Action(ExportAndZoom));
					} else {
						ExportAndZoom();
					}
				} else {
					_sequenceRepeat = false;
					SetImageSequenceMenu();
				}
			}
		}

		// Event handler for image box updates
		private void UpdateImageTick(object sender, EventArgs e) {
			_uiManager.Update(_imageManager);
		}

		// Event handler for worker list updates
		private void UpdateWorkersTick(object sender, EventArgs e) {
			_uiManager.UpdateWorkers();
		}

		// Stop current calculations and prepare for complete recalculation
		private void DeactivateBatch() {
			if (_batch != null) {
				_batch.Deactivate();
				_batch = null;
				_imageManager.NextBatch();
				_imageManager.ClearProgress();
			}
		}

		private void AddTasks(List<CalcTask> tasks) {
			// Create a batch if there isn't one
			if (_batch == null) {
				_batch = new CalcBatch(_calcTaskManager);
			}
			// Add tasks to the batch
			_batch.Add(tasks);
			if (_batch.Running) {
				// As batch is already running, just currect the number of tasks
				_imageManager.AddTasks(tasks.Count);
			} else {
				// Start the batch
				_imageManager.ClearProgress();
				_imageManager.AddTasks(tasks.Count);
				_batch.RunAsync(_calcManager);
			}
		}

		// Create tasks for drawing the entire image
		private void Draw(float centerX = 0.5f, float centerY = 0.5f) {
			// Stop the previuos batch if there is one
			DeactivateBatch();
			// Create task objects
			List<CalcTask> tasks = _imageManager.DivideCalculation(centerX, centerY);
			// Run the tasks in a new batch
			AddTasks(tasks);
		}

		// Create tasks for redrawing parts of the image containing a specific iteration value
		private void DrawIterationsOver(int oldIterations) {
			List<CalcTask> tasks = _imageManager.DivideCalculation().Where(t => _imageManager.TaskContainsValue(t, oldIterations)).ToList();
			AddTasks(tasks);
		}

		// Make the image box work in a center-when-smaller/zoom-when-bigger mode.
		private void CheckImageBoxMode() {
			if (_imageManager.W < ImageBox.Width && _imageManager.H < ImageBox.Height) {
				ImageBox.SizeMode = PictureBoxSizeMode.CenterImage;
			} else {
				ImageBox.SizeMode = PictureBoxSizeMode.Zoom;
			}
		}

		// Pan the image by a number of pixels
		private void PanBy(int dx, int dy) {
			ImageBoxCalc calc = _imageManager.GetCalc();
			// Convert screen pixels to image pixels
			dx = (int)Math.Round(dx / calc.PicScale);
			dy = (int)Math.Round(dy / calc.PicScale);
			if (dx != 0 || dy != 0) {
				// Pan tasks in the queues
				_batch.Offset(dx, dy);
				// Create tasks to draw the revealed parts of the image
				List<CalcTask> tasks = _imageManager.Pan(dx, dy);
				AddTasks(tasks);
				// Show new coordinates
				SetCoordinatesInfo();
			}
		}

		// Zoom the image centered on a point by a specific rate
		private void Zoom(int x, int y, bool zoomIn, decimal zoomRate) {
			// Zoom the image
			(float cx, float cy) = _imageManager.Zoom(x, y, zoomIn, zoomRate);
			// Check auto precision
			if (_imageManager.AutoPrecision) {
				if (_imageManager.CheckAutoPrecision()) {
					SetPrecisionMenu();
				}
			}
			// Redraw centered on the zoom oint
			Draw(cx, cy);
			// Show new coordinates
			SetCoordinatesInfo();
		}
		
		// Pan the image 
		private void Pan(int x, int y, bool force) {
			int dx = x - _panX;
			int dy = y - _panY;
			// Check if forced or if movement is larg enough
			if ((force && (dx != 0 || dy != 0)) || Math.Abs(dx) >= Settings.PanSensetivity || Math.Abs(dy) >= Settings.PanSensetivity) {
				// 
				_lastMove = DateTime.UtcNow;
				_panX = x;
				_panY = y;
				PanBy(dx, dy);
			}
		}

		// Start panning
		private void StartPan(int x, int y) {
			_panning = true;
			// Store starting point
			_panX = x;
			_panY = y;
			// Keep time for determining 
			_lastMove = DateTime.UtcNow;
			// Start timer for detecting periods of none movement
			_panTimer.Start();
		}

		// Force pan to point and then stop panning
		private void StopPan(int x, int y) {
			Pan(x, y, true);
			StopPan();
		}

		// Stop panning
		private void StopPan() {
			_panning = false;
			_panTimer.Stop();
		}

		// Event handler for pan timer
		private void PanTick(object sender, EventArgs e) {
			if (_panning) {
				// Check for a period of no movement
				if ((DateTime.UtcNow - _lastMove).TotalMilliseconds > Settings.PanInactivityMs) {
					// Convert screen pixels to imagebox pixels
					Point p = ImageBox.PointToClient(MousePosition);
					// Pan image
					Pan(p.X, p.Y, true);
				}
			}
		}

		// Methods for setting sections in the status bar
		#region Set info

		private void SetSizeInfo() {
			StatusLabelSize.Text = $"{_imageManager.W} x {_imageManager.H}";
		}

		private void SetColorInfo() {
			StatusLabelColors.Text = $"Colors: {_imageManager.ColorsInfo.Name} + {_imageManager.ColorOffset}";
		}

		private void SetPrecisionInfo() {
			StatusLabelPrecision.Text = $"{(_imageManager.AutoPrecision ? "Auto: " : String.Empty)}{_imageManager.Precision}, {_imageManager.Iterations} iterations";
		}

		private void SetSmoothingInfo() {
			StatusLabelSmoothing.Text = $"Smoothing: {_imageManager.Smoothing}";
		}

		private void SetCoordinatesInfo() {
			StatusLabelCoordinates.Text = FormattableString.Invariant($"Coor: {_imageManager.Left} ; {_imageManager.Top} : {_imageManager.Scale}");
		}

		private void SetSequenceInfo() {
			string zoom;
			switch (_sequenceZoom) {
				case 1.0025m: zoom = "0.25"; break;
				case 1.005m: zoom = "0.5"; break;
				case 1.01m: zoom = "1"; break;
				case 1.02m: zoom = "2"; break;
				default: zoom = "3"; break;
			}
			StatusLabelSequence.Text = $"Sequence: {zoom}% {(_sequenceRepeat ? " On" : " Off")}";
		}

		#endregion

		// Methods for setting status of menu controls
		#region Set menu

		// Mark the selected item from a set of items
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

		private void SetImageSequenceZoomMenu() {
			MenuImageSequenceZoom025.Checked = _sequenceZoom == 1.0025m;
			MenuImageSequenceZoom05.Checked = _sequenceZoom == 1.005m;
			MenuImageSequenceZoom1.Checked = _sequenceZoom == 1.01m;
			MenuImageSequenceZoom2.Checked = _sequenceZoom == 1.02m;
			MenuImageSequenceZoom3.Checked = _sequenceZoom == 1.03m;
			SetSequenceInfo();
		}

		private void SetImageSequenceMenu() {
			MenuImageSequenceStep.Enabled = _fileName != null;
			MenuImageSequenceRun.Enabled = _fileName != null;
			MenuImageSequenceRun.Checked = _sequenceRepeat;
			SetSequenceInfo();
		}

		private void SetPrecisionChangeMenu() {
			MenuCalculationPrecisionDecrease.Enabled = !_imageManager.AutoPrecision && _imageManager.Precision != CalcPrecision.Single;
			MenuCalculationPrecisionIncrease.Enabled = !_imageManager.AutoPrecision && _imageManager.Precision != CalcPrecision.FixedPoint;
		}

		private void SetPrecisionMenu() {
			ToolStripMenuItem item =
				_imageManager.Precision == CalcPrecision.Single ? MenuCalculationPrecisionSingle :
				_imageManager.Precision == CalcPrecision.Double ? MenuCalculationPrecisionDouble :
				MenuCalculationPrecisionFixedPoint;
			CheckMenuItem(item,
				MenuCalculationPrecisionSingle,
				MenuCalculationPrecisionDouble,
				MenuCalculationPrecisionFixedPoint
			);
			SetPrecisionChangeMenu();
			SetPrecisionInfo();
		}

		private void SetAutoPrecisionMenu() {
			MenuCalculationPrecisionAuto.Checked = _imageManager.AutoPrecision;
			MenuCalculationPrecisionSingle.Enabled = !_imageManager.AutoPrecision;
			MenuCalculationPrecisionDouble.Enabled = !_imageManager.AutoPrecision;
			MenuCalculationPrecisionFixedPoint.Enabled = !_imageManager.AutoPrecision;
			SetPrecisionChangeMenu();
			SetPrecisionInfo();
		}

		private void SetIterationsMenu() {
			Iterations.SetMenu(_imageManager.Iterations);
			MenuCalculationIterationsDecrease.Enabled = _imageManager.Iterations != Iterations.MinValue;
			MenuCalculationIterationsIncrease.Enabled = _imageManager.Iterations != Iterations.MaxValue;
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

		// Methods for changing porperties
		#region Set properties

		private void SetSize(int w, int h) {
			if (w != _imageManager.W || h != _imageManager.H) {
				// Stop calculations
				DeactivateBatch();
				// Create new internal array and image
				_imageManager.SetSize(w, h);
				// Mark item in the size menu
				SetSizeMenu();
				// Set mode of image box
				CheckImageBoxMode();
				// Redraw image
				Draw();
				// Store size in user settings
				_settings.Width = w;
				_settings.Height = h;
				_settings.Save();
			}
		}

		private void SetColor(ColorSetInfo info) {
			if (info != _imageManager.ColorsInfo) {
				// Create palette and redraw image
				_imageManager.SetColor(info);
				// Mark item in colors menu and show in status bar
				SetColorMenu();
			}
		}

		private void SetColorOffset(int offset) {
			// Redraw image
			_imageManager.SetColorOffset(offset);
			// Show offset in status bar
			SetColorInfo();
		}

		private void SetPrecision(CalcPrecision precision) {
			if (precision != _imageManager.Precision) {
				// Change precision
				_imageManager.Precision = precision;
				// Mark item in menu and update status bar
				SetPrecisionMenu();
				// Redraw image with the new precision
				Draw();
			}
		}

		private void ToggleAutoPrecision() {
			// Toggle auto precison
			_imageManager.AutoPrecision = !_imageManager.AutoPrecision;
			// Mark/unmark menu item and update status bar
			SetAutoPrecisionMenu();
			if (_imageManager.AutoPrecision) {
				// Check auto precision if it was turned on
				if (_imageManager.CheckAutoPrecision()) {
					SetPrecisionMenu();
					Draw();
				}
			}
		}

		private void SetIterations(int iterations) {
			if (iterations != _imageManager.Iterations) {
				int oldIterations = _imageManager.Iterations;
				// Set iterations
				_imageManager.Iterations = iterations;
				// Check if drawing is complete
				if (_batch == null || !_batch.Running) {
					if (iterations > oldIterations) {
						// Only readraw parts of the image that contains pixels at the maximum
						DrawIterationsOver(oldIterations);
					} else {
						// Just limit the values in the result to the new iterations
						_imageManager.ReduceIterations();
					}
				} else {
					// Redraw image with the new iterations
					Draw();
				}
				// Mark menu item and update status bar
				SetIterationsMenu();
			}
		}

		private void SetSmoothing(SmoothingMode mode) {
			if (mode != _imageManager.Smoothing) {
				// Set smoothing mode
				_imageManager.SetSmoothing(mode);
				// Mark menu item and update status bar
				SetSmoothingMenu();
				// Redraw image with the new smoothing mode
				Draw();
			}
		}

		private void SetParts(int parts) {
			if (parts != _imageManager.Parts) {
				// Set parts count
				SetParts(parts, true);
			}
		}

		private void SetParameters() {
			// Update menus and status bar for all parameters
			SetSizeMenu();
			SetColorMenu();
			SetImageSequenceZoomMenu();
			SetImageSequenceMenu();
			SetPrecisionMenu();
			SetAutoPrecisionMenu();
			SetIterationsMenu();
			SetSmoothingMenu();
			CheckImageBoxMode();
			SetCoordinatesInfo();
		}

		#endregion

		// Event handlers for the image box
		#region ImageBox

		private void ImageBox_MouseWheel(object sender, MouseEventArgs e) {
			// Determine zoom rate
			decimal zoom = 1.1m;
			switch (Control.ModifierKeys) {
				case Keys.Shift: zoom = 1.01m; break;
				case Keys.Control: zoom = 1.2m; break;
				case Keys.Alt: zoom = 1.5m; break;
			}
			// Zoom image centered on mouse pointer
			Zoom(e.X, e.Y, e.Delta > 0, zoom);
		}

		private void ImageBox_Resize(object sender, EventArgs e) {
			// Update image box mode depending on size
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

		// Event handlers for window menus

		#region File menu

		private void MenuFileNew_Click(object sender, EventArgs e) {
			// Stop calculations and start over
			DeactivateBatch();
			_imageManager.Reset();
			SetParameters();
			Draw();
		}

		private void MenuFileLoad_Click(object sender, EventArgs e) {
			OpenFileDialog.Title = "Load parameters";
			if (_fileName != null) {
				OpenFileDialog.FileName = Path.GetFileNameWithoutExtension(_fileName);
			}
			DialogResult result = OpenFileDialog.ShowDialog();
			if (result == DialogResult.OK) {
				DeactivateBatch();
				string name = OpenFileDialog.FileName;
				if (_imageManager.LoadCoordinates(name)) {
					_fileName = name;
					SetParameters();
					Draw();
				}
			}
		}

		private void MenuFileSaveAs_Click(object sender, EventArgs e) {
			SaveFileDialog.Title = "Save parameters";
			if (_fileName != null) {
				SaveFileDialog.FileName = Path.GetFileNameWithoutExtension(_fileName);
			}
			DialogResult result = SaveFileDialog.ShowDialog();
			if (result == DialogResult.OK) {
				_fileName = SaveFileDialog.FileName;
				_imageManager.SaveCoordinates(_fileName);
				SetImageSequenceMenu();
			}
		}

		private void MenuFileExit_Click(object sender, EventArgs e) {
			DeactivateBatch();
			Close();
		}

		#endregion

		#region Image menu

		private void MenuImageExportAs_Click(object sender, EventArgs e) {
			ExportImageDialog.Title = "Export image";
			if (_fileName != null) {
				ExportImageDialog.FileName = Path.GetFileNameWithoutExtension(_fileName);
			}
			DialogResult result = ExportImageDialog.ShowDialog();
			if (result == DialogResult.OK) {
				_fileName = ExportImageDialog.FileName;
				_imageManager.SaveImage(_fileName);
				SetImageSequenceMenu();
			}
		}

		private void ExportIndexed() {
			string name = Path.GetFileNameWithoutExtension(_fileName);
			int cnt = 0;
			while (cnt < name.Length && Char.IsDigit(name[name.Length - 1 - cnt])) {
				cnt++;
			}
			int index;
			if (cnt == 0) {
				cnt = 5;
				index = 99999;
			} else {
				index = Int32.Parse(name.Substring(name.Length - cnt)) - 1;
				name = name.Substring(0, name.Length - cnt);
			}
			string num = index.ToString();
			while (num.Length < cnt) {
				num = "0" + num;
			}
			_fileName = Path.Combine(Path.GetDirectoryName(_fileName), name + num + ".jpg");
			_imageManager.SaveImage(_fileName);
		}

		private void MenuImageExportIndexed_Click(object sender, EventArgs e) {
			ExportIndexed();
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
			// Show custom size dialog
			ImageSize size = new ImageSize(_imageManager.W, _imageManager.H);
			DialogResult result = size.ShowDialog();
			if (result == DialogResult.OK) {
				// Change size
				int w = (int)size.ImageSizeNumericWidth.Value;
				int h = (int)size.ImageSizeNumericHeight.Value;
				SetSize(w, h);
			}
		}

		private void ImageMenuZoomCenter(bool zoomIn, decimal zoomRate) {
			Zoom(ImageBox.Width / 2, ImageBox.Height / 2, zoomIn, zoomRate);
		}

		private void MenuImageZoomIn1_Click(object sender, EventArgs e) => ImageMenuZoomCenter(true, 1.01m);
		private void MenuImageZoomIn10_Click(object sender, EventArgs e) => ImageMenuZoomCenter(true, 1.1m);
		private void MenuImageZoomIn20_Click(object sender, EventArgs e) => ImageMenuZoomCenter(true, 1.2m);
		private void MenuImageZoomIn50_Click(object sender, EventArgs e) => ImageMenuZoomCenter(true, 1.5m);
		private void MenuImageZoomOut1_Click(object sender, EventArgs e) => ImageMenuZoomCenter(false, 1.01m);
		private void MenuImageZoomOut10_Click(object sender, EventArgs e) => ImageMenuZoomCenter(false, 1.1m);
		private void MenuImageZoomOut20_Click(object sender, EventArgs e) => ImageMenuZoomCenter(false, 1.2m);
		private void MenuImageZoomOut50_Click(object sender, EventArgs e) => ImageMenuZoomCenter(false, 1.5m);

		private void MenuImageSequenceZoom025_Click(object sender, EventArgs e) {
			_sequenceZoom = 1.0025m;
			SetImageSequenceZoomMenu();
		}

		private void MenuImageSequenceZoom05_Click(object sender, EventArgs e) {
			_sequenceZoom = 1.005m;
			SetImageSequenceZoomMenu();
		}

		private void MenuImageSequenceZoom1_Click(object sender, EventArgs e) {
			_sequenceZoom = 1.01m;
			SetImageSequenceZoomMenu();
		}

		private void MenuImageSequenceZoom2_Click(object sender, EventArgs e) {
			_sequenceZoom = 1.02m;
			SetImageSequenceZoomMenu();
		}

		private void MenuImageSequenceZoom3_Click(object sender, EventArgs e) {
			_sequenceZoom = 1.03m;
			SetImageSequenceZoomMenu();
		}

		private void ExportAndZoom() {
			ExportIndexed();
			Zoom(ImageBox.Width / 2, ImageBox.Height / 2, false, _sequenceZoom);
		}

		private void MenuImageSequenceStep_Click(object sender, EventArgs e) {
			ExportAndZoom();
		}

		private void MenuImageSequenceRun_Click(object sender, EventArgs e) {
			if (_sequenceRepeat) {
				_sequenceRepeat = false;
			} else {
				_sequenceRepeat = true;
				ExportAndZoom();
			}
			SetImageSequenceMenu();
		}

		private void MenuImageColorSetPrevious_Click(object sender, EventArgs e) => SetColor(ColorSets.Previous(_imageManager.ColorsInfo));
		private void MenuImageColorSetNext_Click(object sender, EventArgs e) => SetColor(ColorSets.Next(_imageManager.ColorsInfo));

		private void MenuImageColorOffsetDecrease10_Click(object sender, EventArgs e) => SetColorOffset(_imageManager.ColorOffset - 10);
		private void MenuImageColorOffsetIncrease10_Click(object sender, EventArgs e) => SetColorOffset(_imageManager.ColorOffset + 10);
		private void MenuImageColorOffsetDecrease1_Click(object sender, EventArgs e) => SetColorOffset(_imageManager.ColorOffset - 1);
		private void MenuImageColorOffsetIncrease1_Click(object sender, EventArgs e) => SetColorOffset(_imageManager.ColorOffset + 1);

		#endregion

		#region Calculation menu

		private void MenuCalculationPrecisionDecrease_Click(object sender, EventArgs e) {
			switch (_imageManager.Precision) {
				case CalcPrecision.Double: SetPrecision(CalcPrecision.Single); break;
				case CalcPrecision.FixedPoint: SetPrecision(CalcPrecision.Double); break;
			}
		}

		private void MenuCalculationPrecisionIncrease_Click(object sender, EventArgs e) {
			switch (_imageManager.Precision) {
				case CalcPrecision.Single: SetPrecision(CalcPrecision.Double); break;
				case CalcPrecision.Double: SetPrecision(CalcPrecision.FixedPoint); break;
			}
		}

		private void MenuCalculationPrecisionSingle_Click(object sender, EventArgs e) => SetPrecision(CalcPrecision.Single);
		private void MenuCalculationPrecisionDouble_Click(object sender, EventArgs e) => SetPrecision(CalcPrecision.Double);
		private void MenuCalculationPrecisionFixedPoint128_Click(object sender, EventArgs e) => SetPrecision(CalcPrecision.FixedPoint);

		private void MenuCalculationPrecisionAuto_Click(object sender, EventArgs e) => ToggleAutoPrecision();

		private void MenuCalculationIterationsDecrease_Click(object sender, EventArgs e) {
			int? depth = Iterations.Previous(_imageManager.Iterations);
			if (depth.HasValue) {
				SetIterations(depth.Value);
			}
		}

		private void MenuCalculationIterationsIncrease_Click(object sender, EventArgs e) {
			int? depth = Iterations.Next(_imageManager.Iterations);
			if (depth.HasValue) {
				SetIterations(depth.Value);
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

		#endregion

		#region Help menu

		private void MenuHelpAbout_Click(object sender, EventArgs e) {
			new About().ShowDialog();
		}

		private void MenuHelpQuickGuide_Click(object sender, EventArgs e) {
			new QuickGuide().ShowDialog();
		}

		#endregion

		// Event handlers for worker list contect menu
		#region Worker List Menu

		private void CalculatorListMenuAdd_Click(object sender, EventArgs e) {
			// Show add worker dialog
			AddCalculator add = new AddCalculator();
			DialogResult result = add.ShowDialog();
			if (result == DialogResult.OK) {
				// Add worker to list
				_calcManager.Add(new TcpCalcNode(_calcManager, add.AddCalculatorTextBoxIP.Text, Int32.Parse(add.AddCalculatorTextBoxPort.Text)));
				// Save nodes in user settings
				_calcManager.SaveNodes(_settings);
			}
		}

		private void CalculatorListMenuRemove_Click(object sender, EventArgs e) {
			// Remove seleted item
			ListView.SelectedListViewItemCollection selected = CalculatorList.SelectedItems;
			foreach (ListViewItem item in selected) {
				_calcManager.Remove(item);
			}
			// Save nodes in user settings
			_calcManager.SaveNodes(_settings);
		}

		private void CalculatorListMenuRecheck_Click(object sender, EventArgs e) {
			ListView.SelectedListViewItemCollection selected = CalculatorList.SelectedItems;
			foreach (ListViewItem item in selected) {
				int index = item.Index;
				// Run recheck in a separate thread
				Task.Run(async () => {
					// Recheck node
					await _calcManager.Recheck(index);
					// Save nodes in user settings
					_calcManager.SaveNodes(_settings);
				});
			}
		}

		private async Task Enable(List<CalcNode> nodes) {
			foreach (CalcNode node in nodes) {
				// Enable node
				await _calcManager.Enable(node);
			}
			// Save nodes in user settings
			_calcManager.SaveNodes(_settings);
		}

		private void CalculatorListMenuEnable_Click(object sender, EventArgs e) {
			// Get selected nodes
			ListView.SelectedListViewItemCollection selected = CalculatorList.SelectedItems;
			List<CalcNode> nodes = new List<CalcNode>();
			foreach (ListViewItem item in selected) {
				nodes.Add((CalcNode)item.Tag);
			}
			// Run enable in a separate thread
			Task.Run(() => Enable(nodes));
		}

		private void CalculatorListMenuDisable_Click(object sender, EventArgs e) {
			// Get selected item
			ListView.SelectedListViewItemCollection selected = CalculatorList.SelectedItems;
			foreach (ListViewItem item in selected) {
				// Disable node
				CalcNode node = (CalcNode)item.Tag;
				_calcManager.Disable(node);
			}
			_calcManager.SaveNodes(_settings);
		}

		#endregion

		// Event handlers for image box context menu
		#region Image Menu

		private void ImageMenuCenter_Click(object sender, EventArgs e) {
			// Get coordinates where the mouse was clicked
			Point p = new Point(ImageMenu.Left, ImageMenu.Top);
			// Convert to image box coordinates
			p = ImageBox.PointToClient(p);
			// Calculate how much to pan by to center image
			int dx = ImageBox.Width / 2 - p.X;
			int dy = ImageBox.Height / 2 - p.Y;
			// Pan image
			PanBy(dx, dy);
		}

		private void ImageMenuZoom(bool zoomIn, decimal zoomRate) {
			// Get coordinates where the mouse was clicked
			Point p = new Point(ImageMenu.Left, ImageMenu.Top);
			// Convert from sceen coordinates to image box coordinates
			p = ImageBox.PointToClient(p);
			// Zoom centered on coordinates
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
