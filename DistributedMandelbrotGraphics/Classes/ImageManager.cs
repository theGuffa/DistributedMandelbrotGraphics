using MandelCalculation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DistributedMandelbrotGraphics.Classes {

	public class ImageManager {

		private readonly UIManager _uiManager;
		private readonly CalcTaskManager _calcTaskManager;
		private readonly object _sync;
		private readonly PictureBox _box;
		private Bitmap _image;
		private int[,] _data;
		private IntColor[] _intColors;
		private int _totalTasks, _tasksDone;
		private int _batchGroup;

		public decimal Left { get; private set; }
		public decimal Top { get; private set; }
		public decimal Scale { get; private set; }
		public int W { get; private set; }
		public int H { get; private set; }
		public ColorSetInfo ColorsInfo { get; private set; }
		public int ColorOffset { get; private set; }
		public SmoothingMode Smoothing { get; private set; }

		public CalcPrecision Precision { get; set; }
		public bool AutoPrecision { get; set; }
		public int Depth { get; set; }
		public int Parts { get; set; }

		private int DataW => Smoothing == SmoothingMode.None ? W : W * 2;
		private int DataH => Smoothing == SmoothingMode.Quadruple ? H * 2 : H;

		public ImageManager(int w, int h, int parts, PictureBox box, UIManager uiManager, CalcTaskManager calcTaskManager) {
			_uiManager = uiManager;
			_calcTaskManager = calcTaskManager;
			_sync = new object();
			_box = box;
			W = w;
			H = h;
			_image = null;
			Reset();
			Parts = parts;
			_totalTasks = 0;
			_tasksDone = 0;
			_batchGroup = 0;
		}

		public void NextBatch() {
			_batchGroup++;
		}

		public void Reset() {
			Scale = 4.0m / (decimal)W;
			Left = -2.5m;
			Top = (decimal)H * 0.5m * Scale;
			Precision = CalcPrecision.Single;
			AutoPrecision = true;
			Depth = 100;
			Smoothing = SmoothingMode.None;
			ColorsInfo = ColorSets.Get("Standard");
			ColorOffset = 0;
			_intColors = ColorsInfo.CreateColors();
			if (_image == null) {
				_image = new Bitmap(W, H);
				_box.Image = _image;
				_data = new int[DataW, DataH];
			}
			FillData(0, 0, W, H, FillColor.Transparent);
			SetColors();
		}

		public bool CheckAutoPrecision() {
			CalcPrecision expected =
				Scale < 0.0000000000000001m ? CalcPrecision.Decimal :
				Scale < 0.0000001m ? CalcPrecision.Double :
				CalcPrecision.Single;
			bool change = Precision != expected;
			Precision = expected;
			return change;
		}

		private void SetProgress() {
			_uiManager.SetProgress(_totalTasks > 0 ? (_tasksDone < _totalTasks ? 1000 * _tasksDone / _totalTasks : 1000) : 0);
		}

		public void ClearProgress() {
			_totalTasks = 0;
			_tasksDone = 0;
			SetProgress();
		}

		public void AddTasks(int cnt) {
			_totalTasks += cnt;
			SetProgress();
		}

		public (float cx, float cy) Zoom(int x, int y, bool zoomIn, decimal zoom) {
			ImageBoxCalc calc = GetCalc();
			(decimal fx, decimal fy) = calc.BoxToImage(x, y);
			decimal ix = Left + fx * Scale;
			decimal iy = Top - fy * Scale;
			if (zoomIn) {
				Scale /= zoom;
			} else {
				Scale *= zoom;
			}
			Left = ix - fx * Scale;
			Top = iy + fy * Scale;
			return ((float)(fx / W), (float)(fy / H));
		}

		//public (decimal cx, decimal cy) ZoomOut(int x, int y, decimal zoom) {
		//	ImageBoxCalc calc = GetCalc();
		//	(decimal fx, decimal fy) = calc.BoxToImage(x, y);
		//	decimal ix = Left + fx * Scale;
		//	decimal iy = Top - fy * Scale;
		//	Scale *= zoom;
		//	Left = ix - fx * Scale;
		//	Top = iy + fy * Scale;
		//	return (fx / W, fy / H);
		//}

		public void SetSize(int w, int h) {
			_image.Dispose();
			decimal cx = Left + Scale * W / 2;
			decimal cy = Top - Scale * H / 2;
			Scale *= W + H;
			W = w;
			H = h;
			Scale /= W + H;
			Left = cx - Scale * W / 2;
			Top = cy + Scale * H / 2;
			_image = new Bitmap(W, H);
			_data = new int[DataW, DataH];
			_box.Image = _image;
		}

		public void SetSmoothing(SmoothingMode smoothing) {
			Smoothing = smoothing;
			_data = new int[DataW, DataH];
		}

		private void SetColors() {
			SetColors(0, 0, W, H);
		}

		private void SetColors(int x, int y, int w, int h) {
			lock (_sync) {
				//Stopwatch sw = Stopwatch.StartNew();
				//SetColorsMulti(x, y, w, h);
				SetColorsSingle(x, y, w, h);
				//sw.Stop();
				//_uiManager.Info($"{(1000.0 * sw.ElapsedTicks / Stopwatch.Frequency):N3} ms");
			}
		}

		#region Locked

		private const int _createdColor = unchecked((int)0xffd3d3d3); // LightGray
		private const int _workingColor = unchecked((int)0xff483d8B); // DarkSlateBlue

		private static int GetFillColor(int fillColor) {
			switch ((FillColor)fillColor) {
				case FillColor.Created: return _createdColor;
				case FillColor.Working: return _workingColor;
			}
			return 0;
		}

		private void SetColorsMulti(int drawX, int drawY, int drawW, int drawH) {
			int offset = ColorOffset;
			int dataW = _data.GetLength(0);
			int dataH = _data.GetLength(1);
			BitmapData bitmapData = _image.LockBits(new Rectangle(drawX, drawY, drawW, drawH), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			unsafe {
				byte* ptr = (byte*)bitmapData.Scan0.ToPointer();
				Parallel.For(0, drawH, yOffset => {
					int c = 0;
					int y = drawY + yOffset;
					if (y >= 0 && y < H) {
						int* intPtr = (int*)(ptr + bitmapData.Stride * yOffset);
						for (int xOffset = 0; xOffset < drawW; xOffset++) {
							int x = drawX + xOffset;
							if (x >= 0 && x < W) {
								switch (Smoothing) {
									case SmoothingMode.None: {
											int i = _data[x, y];
											if (i < 0) {
												c = GetFillColor(i);
											} else {
												c = _intColors[(i + offset) % _intColors.Length].Color;
											}
										}
										break;
									case SmoothingMode.Double: {
											int x2 = x * 2;
											int i = _data[x2, y];
											if (i < 0) {
												c = GetFillColor(i);
											} else {
												IntColor c1 = _intColors[(i + offset) % _intColors.Length];
												IntColor c2 = _intColors[(_data[x2 + 1, y] + offset) % _intColors.Length];
												c = IntColor.Avg(c1, c2).Color;
											}
										}
										break;
									case SmoothingMode.Quadruple: {
											int x2 = x * 2;
											int y2 = y * 2;
											int i = _data[x2, y2];
											if (i < 0) {
												c = GetFillColor(i);
											} else {
												IntColor c1 = _intColors[(i + offset) % _intColors.Length];
												IntColor c2 = _intColors[(_data[x2 + 1, y2] + offset) % _intColors.Length];
												IntColor c3 = _intColors[(_data[x2, y2 + 1] + offset) % _intColors.Length];
												IntColor c4 = _intColors[(_data[x2 + 1, y2 + 1] + offset) % _intColors.Length];
												c = IntColor.Avg(c1, c2, c3, c4).Color;
											}
										}
										break;
								}
								*intPtr = c;
								intPtr++;
							} else {
								intPtr++;
							}
						}
					}
				});
			}
			_image.UnlockBits(bitmapData);
		}

		private void SetColorsSingle(int drawX, int drawY, int drawW, int drawH) {
			int offset = ColorOffset;
			int dataW = _data.GetLength(0);
			int dataH = _data.GetLength(1);
			BitmapData bitmapData = _image.LockBits(new Rectangle(drawX, drawY, drawW, drawH), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			int gap = bitmapData.Stride;
			unsafe {
				int c = 0;
				byte* ptr = (byte*)bitmapData.Scan0.ToPointer();
				for (int yOffset = 0; yOffset < drawH; yOffset++) {
					int y = drawY + yOffset;
					if (y >= 0 && y < H) {
						int* intPtr = (int*)ptr;
						for (int xOffset = 0; xOffset < drawW; xOffset++) {
							int x = drawX + xOffset;
							if (x >= 0 && x < W) {
								switch (Smoothing) {
									case SmoothingMode.None: {
											int i = _data[x, y];
											if (i < 0) {
												c = GetFillColor(i);
											} else {
												c = _intColors[(i + offset) % _intColors.Length].Color;
											}
										}
										break;
									case SmoothingMode.Double: {
											int x2 = x * 2;
											int i = _data[x2, y];
											if (i < 0) {
												c = GetFillColor(i);
											} else {
												IntColor c1 = _intColors[(i + offset) % _intColors.Length];
												IntColor c2 = _intColors[(_data[x2 + 1, y] + offset) % _intColors.Length];
												c = IntColor.Avg(c1, c2).Color;
											}
										}
										break;
									case SmoothingMode.Quadruple: {
											int x2 = x * 2;
											int y2 = y * 2;
											int i = _data[x2, y2];
											if (i < 0) {
												c = GetFillColor(i);
											} else {
												IntColor c1 = _intColors[(i + offset) % _intColors.Length];
												IntColor c2 = _intColors[(_data[x2 + 1, y2] + offset) % _intColors.Length];
												y2++;
												IntColor c3 = _intColors[(_data[x2, y2] + offset) % _intColors.Length];
												IntColor c4 = _intColors[(_data[x2 + 1, y2] + offset) % _intColors.Length];
												c = IntColor.Avg(c1, c2, c3, c4).Color;
											}
										}
										break;
								}
								*intPtr = c;
								intPtr++;
							} else {
								intPtr++;
							}
						}
					}
					ptr += gap;
				}
			}
			_image.UnlockBits(bitmapData);
		}

		#endregion

		public void SetColor(ColorSetInfo info) {
			ColorsInfo = info;
			_intColors = ColorsInfo.CreateColors();
			SetColors();
			_uiManager.UpdateImage();
		}

		public void SetColorOffset(int offset) {
			ColorOffset = (_intColors.Length + offset) % _intColors.Length;
			SetColors();
			_uiManager.UpdateImage();
		}

		public ImageBoxCalc GetCalc() => new ImageBoxCalc(_box, W, H);

		private void Invalidate(int x, int y, int w, int h) {
			ImageBoxCalc calc = GetCalc();
			(decimal fx, decimal fy) = calc.ImageToBox(x, y);
			int px = (int)Math.Floor(fx) - 1;
			int py = (int)Math.Floor(fy) - 1;
			int pw = (int)Math.Ceiling(fx + w * calc.PicScale) - px + 2;
			int ph = (int)Math.Ceiling(fy + h * calc.PicScale) - py + 2;
			_box.Invalidate(new Rectangle(px, py, pw, ph));
		}

		private (int x1, int y1, int x2, int y2) GetInside(CalcTask task) {
			int x1 = task.X;
			int y1 = task.Y;
			int x2 = x1 + task.W;
			int y2 = y1 + task.H;
			x1 = Math.Max(Math.Max(x1, 0) - task.X, 0);
			y1 = Math.Max(Math.Max(y1, 0) - task.Y, 0);
			x2 = Math.Min(Math.Min(x2, W) - task.X, task.W);
			y2 = Math.Min(Math.Min(y2, H) - task.Y, task.H);
			return (x1, y1, x2, y2);
		}

		private enum FillColor {
			Created = -1,
			Working = -2,
			Transparent = -3
		}

		private void FillData(int x1, int y1, int x2, int y2, FillColor color) {
			if (Smoothing != SmoothingMode.None) {
				x1 *= 2;
				x2 *= 2;
			}
			if (Smoothing == SmoothingMode.Quadruple) {
				y1 *= 2;
				y2 *= 2;
			}
			for (int y = y1; y < y2; y++) {
				for (int x = x1; x < x2; x++) {
					_data[x, y] = (int)color;
				}
			}
		}

		private void FillData(CalcTask task, FillColor color) {
			(int x1, int y1, int x2, int y2) = GetInside(task);
			int w = x2 - x1;
			int h = y2 - y1;
			if (w > 0 && h > 0) {
				x1 += task.X;
				y1 += task.Y;
				x2 += task.X;
				y2 += task.Y;
				FillData(x1, y1, x2, y2, color);
				SetColors(x1, y1, w, h);
				Invalidate(x1, y1, w, h);
			}
		}

		public void DrawTaskCreated(CalcTask task) {
			if (task.BatchGroup == _batchGroup) {
				FillData(task, FillColor.Created);
			}
		}

		public void DrawTaskWaiting(CalcTask task) {
			if (task.BatchGroup == _batchGroup) {
				FillData(task, FillColor.Working);
			}
		}

		public void DrawTask(CalcTask task, int[,] data) {
			if (task.BatchGroup == _batchGroup) {
				(int x1, int y1, int x2, int y2) = GetInside(task);
				if (x2 > x1 && y2 > y1) {
					int dataX1 = x1, dataY1 = y1, dataX2 = x2, dataY2 = y2, taskX = task.X, taskY = task.Y;
					if (task.Smoothing != SmoothingMode.None) {
						dataX1 *= 2;
						dataX2 *= 2;
						taskX *= 2;
					}
					if (task.Smoothing == SmoothingMode.Quadruple) {
						dataY1 *= 2;
						dataY2 *= 2;
						taskY *= 2;
					}
					for (int y = dataY1; y < dataY2; y++) {
						for (int x = dataX1; x < dataX2; x++) {
							_data[taskX + x, taskY + y] = data[x, y];
						}
					}
					SetColors(task.X + x1, task.Y + y1, x2 - x1, y2 - y1);
					Invalidate(task.X + x1, task.Y + y1, x2 - x1, y2 - y1);
				}
				_tasksDone++;
				SetProgress();
			}
		}

		private CalcTask CreateTask(int x, int y, int w, int h, decimal left, decimal top) => new CalcTask(_calcTaskManager, _batchGroup, x, y, w, h, left, top, Scale, Precision, Depth, Smoothing);

		public List<CalcTask> DivideCalculation(float centerX = 0.5f, float centerY = 0.5f) {
			double k = (double)H / (double)W;
			int w = (int)Math.Round(Math.Sqrt(Parts / k));
			int h = (int)Math.Round((double)Parts / (double)w);
			List<CalcTask> tasks = new List<CalcTask>();
			int itemY = 0;
			decimal ci = Top;
			for (int y = 0; y < h; y++) {
				int y2 = (y + 1) * H / h;
				int itemH = y2 - itemY;
				decimal cr = Left;
				int itemX = 0;
				for (int x = 0; x < w; x++) {
					int x2 = (x + 1) * W / w;
					int itemW = x2 - itemX;
					tasks.Add(CreateTask(itemX, itemY, itemW, itemH, cr, ci));
					cr += itemW * Scale;
					itemX = x2;
				}
				ci -= itemH * Scale;
				itemY = y2;
			}
			int cx = (int)Math.Round(W * centerX);
			int cy = (int)Math.Round(H * centerY);
			tasks.Sort((a, b) => {
				double da = a.Distance(cx, cy);
				double db = b.Distance(cx, cy);
				return da == db ? 0 : da < db ? -1 : 1;
			});
			return tasks;
		}

		private const int MaxTaskSize = 500;

		private void AddTasks(List<CalcTask> tasks, int x, int y, int w, int h) {
			int cntX = w / MaxTaskSize;
			int cntY = h / MaxTaskSize;
			int tileW = cntX == 0 ? w : (w + cntX - 1) / cntX;
			int tileH = cntY == 0 ? h : (h + cntY - 1) / cntY;
			for (int yy = y; yy < y + h; yy += tileH) {
				int thisH = Math.Min(tileH, y + h - yy);
				for (int xx = x; xx < x + w; xx += tileW) {
					int thisW = Math.Min(tileW, x + w - xx);
					tasks.Add(CreateTask(xx, yy, thisW, thisH, Left + xx * Scale, Top - yy * Scale));
				}
			}
		}

		public List<CalcTask> CreateBatchPan(int dx, int dy) {
			List<CalcTask> tasks = new List<CalcTask>();
			int x = 0;
			int w = W;
			if (dx < 0) {
				AddTasks(tasks, W + dx, 0, -dx, H);
				w += dx;
			} else if (dx > 0) {
				AddTasks(tasks, 0, 0, dx, H);
				x += dx;
				w -= dx;
			}
			if (dy < 0) {
				AddTasks(tasks, x, H + dy, w, -dy);
			} else if (dy > 0) {
				AddTasks(tasks, x, 0, w, dy);
			}
			return tasks;
		}

		private (int start, int end, int add) GetLoop(int d, int size) {
			if (d < 0) {
				return (size - 1, -1, -1);
			} else {
				return (0, size, 1);
			}
		}

		private void PanData(int dx, int dy) {
			int w = W, h = H;
			if (Smoothing != SmoothingMode.None) {
				dx *= 2;
				w *= 2;
			}
			if (Smoothing == SmoothingMode.Quadruple) {
				dy *= 2;
				h *= 2;
			}
			(int xStart, int xEnd, int xAdd) = GetLoop(dx, w);
			(int yStart, int yEnd, int yAdd) = GetLoop(dy, h);
			for (int y = yStart; y != yEnd; y += yAdd) {
				int yy = y + dy;
				bool inside = yy >= 0 && yy < h;
				for (int x = xStart; x != xEnd; x += xAdd) {
					if (inside) {
						int xx = x + dx;
						if (xx >= 0 && xx < w) {
							_data[x, y] = _data[xx, yy];
						} else {
							_data[x, y] = (int)FillColor.Transparent;
						}
					} else {
						_data[x, y] = (int)FillColor.Transparent;
					}
				}
			}
		}

		private void PanImage(int dx, int dy) {
			BitmapData bitmapData = _image.LockBits(new Rectangle(0, 0, W, H), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			int w = W, h = H;
			(int xStart, int xEnd, int xAdd) = GetLoop(dx, w);
			(int yStart, int yEnd, int yAdd) = GetLoop(dy, h);
			unsafe {
				for (int y = yStart; y != yEnd; y += yAdd) {
					byte* line = (byte*)bitmapData.Scan0 + bitmapData.Stride * y;
					int* dest = (int*)line + xStart;
					int* src = (int*)(line + bitmapData.Stride * dy) + xStart + dx;
					int yy = y + dy;
					bool inside = yy >= 0 && yy < h;
					for (int x = xStart; x != xEnd; x += xAdd) {
						if (inside) {
							int xx = x + dx;
							if (xx >= 0 && xx < w) {
								*dest = *src;
							} else {
								*dest = 0;
							}
						} else {
							*dest = 0;
						}
						dest += xAdd;
						src += xAdd;
					}
				}
			}
			_image.UnlockBits(bitmapData);
		}

		public List<CalcTask> Pan(int dx, int dy) {
			lock (_sync) {
				PanData(-dx, -dy);
				PanImage(-dx, -dy);
			}
			_uiManager.UpdateImage();
			Left -= dx * Scale;
			Top += dy * Scale;
			return CreateBatchPan(dx, dy);
		}

		public void SaveImage(string fileName) {
			string ext = Path.GetExtension(fileName);
			switch (ext.ToUpperInvariant()) {
				case ".JPG":
				case ".JPEG":
					Guid id = ImageFormat.Jpeg.Guid;
					ImageCodecInfo encoder = ImageCodecInfo.GetImageDecoders().Single(c => c.FormatID == id);
					EncoderParameters parameters = new EncoderParameters(1);
					parameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)100);
					_image.Save(fileName, encoder, parameters);
					break;
				case ".TIF":
				case ".TIFF":
					_image.Save(fileName, ImageFormat.Tiff);
					break;
				case ".BMP":
					_image.Save(fileName, ImageFormat.Bmp);
					break;
				case ".PNG":
					_image.Save(fileName, ImageFormat.Png);
					break;
				default:
					throw new Exception("Unknown image format.");
			}
		}

		private string FormatDecimal(decimal value) => value.ToString(CultureInfo.InvariantCulture);

		public void SaveCoordinates(string fileName) {
			string data = Json.Object
				.Add("left", FormatDecimal(Left))
				.Add("top", FormatDecimal(Top))
				.Add("scale", FormatDecimal(Scale))
				.Add("w", W)
				.Add("h", H)
				.Add("colors", ColorsInfo.Code)
				.Add("offset", ColorOffset)
				.Add("precision", Precision.ToString())
				.Add("depth", Depth)
				.Add("smoothing", Smoothing.ToString())
				.ToString();
			File.WriteAllText(fileName, data, Encoding.UTF8);
		}

		private decimal ParseDecimal(Json.JsonValue value) {
			string s = value.AsString;
			return Decimal.Parse(s, CultureInfo.InvariantCulture);
		}

		public bool LoadCoordinates(string fileName) {
			try {
				string data = File.ReadAllText(fileName, Encoding.UTF8);
				Json.JsonObject obj = Json.Parse(data).AsObject;
				decimal left = ParseDecimal(obj["left"]);
				decimal top = ParseDecimal(obj["top"]);
				decimal scale = ParseDecimal(obj["scale"]);
				int w = obj["w"].AsInteger;
				int h = obj["h"].AsInteger;
				string colorsName = obj["colors"].AsString;
				int offset = obj["offset"].AsInteger;
				string precisionName = obj["precision"].AsString;
				CalcPrecision precision;
				switch (precisionName) {
					case "Single": precision = CalcPrecision.Single; break;
					case "Double": precision = CalcPrecision.Double; break;
					case "Decimal": precision = CalcPrecision.Decimal; break;
					default: throw new Exception($"Unknown precision '{precisionName}'.");
				}
				int depth = obj["depth"].AsInteger;
				string smoothingName = obj["smoothing"].AsString;
				SmoothingMode smoothing;
				switch (smoothingName) {
					case "false":
					case "None": smoothing = SmoothingMode.None; break;
					case "Double": smoothing = SmoothingMode.Double; break;
					case "true":
					case "Quadruple": smoothing = SmoothingMode.Quadruple; break;
					default: throw new Exception($"Unknown smoothing '{smoothingName}'.");
				}
				Left = left;
				Top = top;
				Scale = scale;
				ColorsInfo = ColorSets.Get(colorsName);
				ColorOffset = offset;
				_intColors = ColorsInfo.CreateColors();
				Precision = precision;
				Depth = depth;
				Smoothing = smoothing;
				SetSize(w, h);
				return true;
			} catch (Exception ex) {
				MessageBox.Show($"Invalid format ({ex.Message}).");
				return false;
			}
		}

	}

}
