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

	public partial class AddCalculator : Form {

		public AddCalculator() {
			InitializeComponent();
		}

		//private delegate void AddComputerDelagate(string name, string ip);

		//private void AddComputer(string name, string ip) {
		//	if (ComputerList.InvokeRequired) {
		//		ComputerList.Invoke((AddComputerDelagate)AddComputer, name, ip);
		//	} else {
		//		ComputerList.Items.Add(new ListViewItem(name));
		//	}
		//}

		//private void AddComputer(Computer computer) {
		//	AddComputer(computer.Name, computer.Ip);
		//}

		private void AddCalculator_Load(object sender, EventArgs e) {

			//Task.Run(async () => {

			//	//	DirectoryEntry root = new DirectoryEntry("WinNT:");

			//	//	foreach (DirectoryEntry computers in root.Children) {
			//	//		foreach (DirectoryEntry computer in computers.Children) {
			//	//			if (/*computer.Name != "Schema" &&*/ computer.SchemaClassName == "Computer") {
			//	//				AddComputer(computer.Name, "");
			//	//			}
			//	//		}
			//	//	}

			//	List<Computer> computers = await GetComputers();
			//	foreach (Computer computer in computers) {
			//		AddComputer(computer);
			//	}

			//});

		}

		//private class Computer {

		//	public string Name { get; private set; }
		//	public string Ip { get; private set; }

		//	public Computer(string name, string ip) {

		//	}

		//}

		//private async Task<List<Computer>> GetComputers() {
		//	List<Computer> computers = new List<Computer>();

		//	Process netUtility = new Process();
		//	netUtility.StartInfo.FileName = "net.exe";
		//	netUtility.StartInfo.CreateNoWindow = true;
		//	netUtility.StartInfo.Arguments = "view";
		//	netUtility.StartInfo.RedirectStandardOutput = true;
		//	netUtility.StartInfo.UseShellExecute = false;
		//	netUtility.StartInfo.RedirectStandardError = true;
		//	netUtility.Start();

		//	StreamReader streamReader = new StreamReader(netUtility.StandardOutput.BaseStream, netUtility.StandardOutput.CurrentEncoding);

		//	string line = "";
		//	while ((line = streamReader.ReadLine()) != null) {
		//		if (line.StartsWith("\\")) {

		//			string pcname = line.Substring(2, line.IndexOf(" ") - 2).ToUpper();
		//			//string myIP = Convert.ToString(System.Net.Dns.GetHostByName(pcname).AddressList[0].ToString());
		//			string myIP = (await System.Net.Dns.GetHostEntryAsync(pcname)).AddressList[0].ToString();
		//			//string fullname = "PC Name : " + pcname + " IP Address : " + myIP;
		//			computers.Add(new Computer(pcname, myIP));

		//		}
		//	}

		//	streamReader.Close();
		//	netUtility.WaitForExit(1000);
		//	return computers;
		//}

	}

}
