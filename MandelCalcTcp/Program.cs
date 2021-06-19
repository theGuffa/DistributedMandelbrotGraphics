using MandelCalculation;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace MandelCalcTcp {

	class Program {

		private static string GetIp() {
			IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
			foreach (IPAddress ip in host.AddressList) {
				if (ip.AddressFamily == AddressFamily.InterNetwork) {
					return ip.ToString();
				}
			}
			throw new Exception("No IPv4 address found.");
		}

		static void Main(string[] args) {

			int port = 33000;
			string ip = null;

			foreach (string s in args) {
				if (s.StartsWith("port=")) {
					if (Int32.TryParse(s.Substring(5), out int p) && p >= 1 && p <= 65535) {
						port = p;
					} else {
						Console.WriteLine($"Illegal port number.");
					}
				} else if (s.StartsWith("ip=")) {
					string i = s.Substring(3);
					if (Regex.IsMatch(i, @"^\d+\.\d+\.\d+\.\d+$")) {
						ip = i;
					} else {
						Console.WriteLine($"Illegal IP number '{i}'.");
					}
				} else {
					Console.WriteLine($"Unknown parameter '{s}'.");
				}
			}

			while (true) {

				TcpListener server = null;
				try {
					int cores = Environment.ProcessorCount;

					if (ip == null) {
						ip = GetIp();
					}
					IPAddress localAddr = IPAddress.Parse(ip);

					Console.WriteLine($"Distrinbuted Mandelbrot Graphics v0.9.1 calculation worker - using {cores} cores, listening on {ip},{port}.");

					server = new TcpListener(localAddr, port);

					// Start listening for client requests.
					server.Start();
					byte[] bytes = new byte[256];
					string data = null;

					// Enter the listening loop.
					while (true) {
						Console.Write("Waiting... ");

						// Perform a blocking call to accept requests.
						// You could also use server.AcceptSocket() here.
						TcpClient client = server.AcceptTcpClient();
						Console.Write("Connected. ");

						data = null;

						// Get a stream object for reading and writing
						NetworkStream stream = client.GetStream();

						int i;

						// Loop to receive all the data sent by the client.
						while ((i = stream.Read(bytes, 0, bytes.Length)) != 0) {
							// Translate data bytes to a ASCII string.
							data = System.Text.Encoding.UTF8.GetString(bytes, 0, i);
							//Console.WriteLine("Received: {0}", data);

							// Process the data sent by the client.

							byte[] msg;
							if (data == "Check") {

								Console.WriteLine("Check");
								msg = Encoding.UTF8.GetBytes("ok:" + Environment.MachineName);

							} else {

								Stopwatch sw = Stopwatch.StartNew();

								Calculation c = new Calculation(data);
								CalcResult result = c.Calculate(cores);
								msg = CalcUtil.PackPixels(result.Pixels);

								sw.Stop();
								long micro = CalcUtil.MicroSeconds(sw);
								Console.WriteLine($"{result.Pixels.Length} pixels calculated in {micro} μs, packed to {msg.Length} bytes.");
							}
							//byte[] msg = System.Text.Encoding.UTF8.GetBytes(data);

							// Send back a response.
							byte[] len = BitConverter.GetBytes(msg.Length);
							stream.Write(len, 0, 4);
							stream.Write(msg, 0, msg.Length);
							//Console.WriteLine("Sent: {0}", data);
						}

						// Shutdown and end connection
						client.Close();
					}
				} catch (SocketException ex) {
					Console.WriteLine("SocketException: " + ex.Message);
				} catch (Exception ex) {
					Console.WriteLine("Exception: " + ex.Message);
				} finally {
					// Stop listening for new clients.
					server.Stop();
				}

			}

		}

	}

}
