using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedMandelbrotGraphics.Classes {

	public class Settings : ApplicationSettingsBase {

		[UserScopedSetting]
		[DefaultSettingValue("true")]
		public bool InternalActive {
			get => (bool)this["InternalActive"];
			set => this["InternalActive"] = value;
		}

		[UserScopedSetting]
		[DefaultSettingValue("[]")]
		public string Nodes {
			get => (string)this["Nodes"];
			set => this["Nodes"] = value;
		}

		public Json.JsonArray NodesArray {
			get => Json.Parse(Nodes).AsArray;
			set => Nodes = value.ToString();
		}

		[UserScopedSetting]
		[DefaultSettingValue("500")]
		public int Parts {
			get => (int)this["Parts"];
			set => this["Parts"] = value;
		}

		[UserScopedSetting]
		[DefaultSettingValue("true")]
		public bool ShowWorkers {
			get => (bool)this["ShowWorkers"];
			set => this["ShowWorkers"] = value;
		}

		[UserScopedSetting]
		[DefaultSettingValue("1920")]
		public int Width {
			get => (int)this["Width"];
			set => this["Width"] = value;
		}

		[UserScopedSetting]
		[DefaultSettingValue("1080")]
		public int Height {
			get => (int)this["Height"];
			set => this["Height"] = value;
		}

	}

}
