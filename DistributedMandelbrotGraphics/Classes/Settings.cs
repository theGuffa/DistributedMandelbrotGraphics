using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedMandelbrotGraphics.Classes {

	public class Settings : ApplicationSettingsBase {

		// The size of the buffer of past calculation times
		public const int NodePerformanceBufferSize = 50;

		// Rate for update timers
		public const int UpdateImageInterval = 20;
		public const int UpdateWorkersInterval = 50;
		public const int PanInterval = 100;

		// Number of pixels for panning to react immediately
		public const int PanSensetivity = 5;

		// Number of milliseconds to determine no movement while panning
		public const int PanInactivityMs = 300;

		// Maximum size of squares created when panning
		public const int MaxTaskSquareSize = 200;

		// Active flag for the internal worker
		[UserScopedSetting]
		[DefaultSettingValue("true")]
		public bool InternalActive {
			get => (bool)this["InternalActive"];
			set => this["InternalActive"] = value;
		}

		// Settings for external workers
		[UserScopedSetting]
		[DefaultSettingValue("[]")]
		public string Nodes {
			get => (string)this["Nodes"];
			set => this["Nodes"] = value;
		}

		// Settings for external workers as a json array
		public Json.JsonArray NodesArray {
			get => Json.Parse(Nodes).AsArray;
			set => Nodes = value.ToString();
		}

		// Setting for calculation parts
		[UserScopedSetting]
		[DefaultSettingValue("500")]
		public int Parts {
			get => (int)this["Parts"];
			set => this["Parts"] = value;
		}

		// Settings for showing worker list
		[UserScopedSetting]
		[DefaultSettingValue("true")]
		public bool ShowWorkers {
			get => (bool)this["ShowWorkers"];
			set => this["ShowWorkers"] = value;
		}

		// Setting for image width
		[UserScopedSetting]
		[DefaultSettingValue("1920")]
		public int Width {
			get => (int)this["Width"];
			set => this["Width"] = value;
		}

		// Setting for image height
		[UserScopedSetting]
		[DefaultSettingValue("1080")]
		public int Height {
			get => (int)this["Height"];
			set => this["Height"] = value;
		}

	}

}
