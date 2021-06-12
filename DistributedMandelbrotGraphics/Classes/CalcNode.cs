using MandelCalculation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DistributedMandelbrotGraphics.Classes {

	public class PerformanceItem {

		public long PixelCount { get; private set; }
		public long Micro { get; private set; }

		public PerformanceItem(long pixelCount, long micro) {
			PixelCount = pixelCount;
			Micro = micro;
		}

	}

	public enum NodeState {
		Unknown,
		Checking,
		Idle,
		Running,
		Failed,
		Disabled
	}

	public abstract class CalcNode {

		private CalcManager _manager;
		private NodeState _state;
		private RingBuffer<PerformanceItem> _performance;

		public string Name { get; set; }

		public NodeState State {
			get => _state;
			set {
				_state = value;
				_manager.SetNodeChanged(this, UIManager.NodeChange.Update);
			}
		}

		public long PixelCount => _performance.Sum(p => p.PixelCount);
		public long Micro => _performance.Sum(p => p.Micro);

		public string Speed {
			get {
				long micro = Micro;
				return micro > 0 ? (1000000 * PixelCount / micro).ToString("N0") : "-";
			}
		}

		public void AddSpeed(int count, long micro) {
			_performance.Add(new PerformanceItem(count, micro));
			_manager.SetNodeChanged(this, UIManager.NodeChange.Update);
		}

		protected CalcNode(CalcManager manager, NodeState state) {
			_manager = manager;
			_state = state;
			_performance = new RingBuffer<PerformanceItem>(100);
		}

		public abstract Task<string> Check();

		public abstract Task<CalcResult> Calculate(CalcTask task);

		public abstract void Close();

		public abstract Json.JsonObject ToJson();

	}

	public class InternalCalcNode : CalcNode {

		public int Cores { get; private set; }

		public InternalCalcNode(CalcManager manager, int cores, bool active) : base(manager, active ? NodeState.Idle : NodeState.Disabled) {
			Name = "Internal";
			Cores = cores;
		}

		public override async Task<CalcResult> Calculate(CalcTask task) {
			CalcResult result = await Task.Run(() => {
				return task.Calculate(Cores);
			});
			State = NodeState.Idle;
			return result;
		}

		public override Task<string> Check() {
			return Task.FromResult(String.Empty);
		}

		public override void Close() {
		}

		public override Json.JsonObject ToJson() => Json.Object.Add("type", "Internal");

	}

	public class HttpCalcNode : CalcNode {

		public string BaseUrl { get; private set; }
		public string CheckUrl { get; private set; }
		public string Url { get; private set; }

		public HttpCalcNode(CalcManager manager, string name, string url, NodeState state) : base(manager, state) {
			BaseUrl = url;
			CheckUrl = url + "/Check";
			Url = url + "/Calc?t=";
			Name = name;
		}

		public override async Task<string> Check() {
			if (CheckUrl != null) {
				try {
					using (HttpClient client = new HttpClient()) {
						string result = await client.GetStringAsync(CheckUrl);
						if (result.StartsWith("ok:")) {
							return "HTTP " + result.Substring(3);
						} else {
							return null;
						}
					}
				} catch (Exception) {
					return null;
				}
			} else {
				return String.Empty;
			}
		}

		public override async Task<CalcResult> Calculate(CalcTask task) {
			try {
				using (HttpClient client = new HttpClient()) {
					Stopwatch sw = Stopwatch.StartNew();
					byte[] data = await client.GetByteArrayAsync(Url + task.ToString());
					sw.Stop();
					int[,] result = CalcUtil.UnpackPixels(data, task.Smoothing != SmoothingMode.None ? task.W * 2 : task.W, task.Smoothing == SmoothingMode.Quadruple ? task.H * 2 : task.H);
					State = NodeState.Idle;
					return new CalcResult(result, CalcUtil.MicroSeconds(sw));
				}
			} catch (Exception) {
				task.SetState(TaskState.Created);
				State = NodeState.Failed;
				return null;
			}
		}

		public override void Close() {
		}

		public override Json.JsonObject ToJson() => Json.Object
			.Add("type", "HTTP")
			.Add("url", BaseUrl)
			.Add("active", State != NodeState.Disabled);

	}

	public class TcpCalcNode : CalcNode {

		private TcpClient _client;
		private NetworkStream _stream;

		public string Ip { get; private set; }
		public int Port { get; private set; }

		public TcpCalcNode(CalcManager manager, string name, string ip, int port, NodeState state) : base(manager, state) {
			Ip = ip;
			Port = port;
			Name = name;
			_client = null;
		}

		public TcpCalcNode(CalcManager manager, string ip, int port) : this(manager, ip, ip, port, NodeState.Unknown) { }

		private async Task<byte[]> Send(string cmd) {
			try {

				byte[] data = System.Text.Encoding.UTF8.GetBytes(cmd);

				if (_client == null) {
					_client = new TcpClient(Ip, Port);
					_stream = _client.GetStream();
				}

				await _stream.WriteAsync(data, 0, data.Length);

				// Read the first batch of the TcpServer response bytes.
				// Read length
				data = new byte[4];
				int bytes = await _stream.ReadAsync(data, 0, 4);
				int len = BitConverter.ToInt32(data, 0);
				// Read data
				data = new byte[len];
				int ofs = 0;
				while (ofs < len) {
					bytes = await _stream.ReadAsync(data, ofs, data.Length - ofs);
					ofs += bytes;
				}

				// Close everything.
				//_stream.Close();
				//_client.Close();
				return data;
			} catch (SocketException /*ex*/) {
				//Console.WriteLine("SocketException: {0}", ex);
				return null;
			}

		}

		public override async Task<string> Check() {
			byte[] response = await Send("Check");
			if (response != null) {
				string result = Encoding.UTF8.GetString(response);
				if (result.StartsWith("ok:")) {
					return "TCP " + result.Substring(3);
				} else {
					return null;
				}
			} else {
				return null;
			}
		}

		public override async Task<CalcResult> Calculate(CalcTask task) {
			Stopwatch sw = Stopwatch.StartNew();
			byte[] data = await Send(task.ToString());
			sw.Stop();
			if (data != null) {
				int[,] result = CalcUtil.UnpackPixels(data, task.Smoothing != SmoothingMode.None ? task.W * 2 : task.W, task.Smoothing == SmoothingMode.Quadruple ? task.H * 2 : task.H);
				State = NodeState.Idle;
				return new CalcResult(result, CalcUtil.MicroSeconds(sw));
			} else {
				task.SetState(TaskState.Created);
				State = NodeState.Failed;
				return null;
			}
		}

		public override void Close() {
			if (_client != null) {
				_stream.Close();
				_client.Close();
				_client = null;
			}
		}

		public override Json.JsonObject ToJson() => Json.Object
			.Add("type", "TCP")
			.Add("ip", Ip)
			.Add("port", Port)
			.Add("active", State != NodeState.Disabled);

	}

}
