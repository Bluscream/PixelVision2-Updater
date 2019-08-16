using Microsoft.Win32;
using Pixelvision_Updater;
using Pixelvision_Updater.Properties;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Unzipper
{
	public class Form1 : Form
	{
		private string updateUrl = "http://lolwut.net/projex/pixelvision/";

		private string liveVersion = "";

		private string betaVersion = "";

		private string tempLocation = "";

		private string steamPath = "C:\\Program Files (x86)\\Steam\\skins\\PixelVision";

		private string downloadPath = "";

		private string rootPath = "";

		private string legacyRootPath = "";

		private string version = "";

		private string versionUrl = "";

		private bool betaEnabled = false;

		private bool legacyDownload = false;

		private bool savedSettings = false;

		private bool savedLocale = false;

		private string backgroundResult = "";

		private bool connection = true;

		private string hashString = "";

		private bool corrupted = false;

		private bool structureInteg = false;

		private bool createdFolder = false;

		private bool aborted = false;

		private bool extractError = false;

		private Process steamProcess;

		private string steamProcessPath;

		private int counter = 0;

		private IContainer components = null;

		private TextBox sourceBox;

		private Label label1;

		private Button downloadBtn;

		private Button extractBtn;

		private Label label2;

		private TextBox targetBox;

		private CheckBox wipeCheck;

		private BackgroundWorker backgroundUpdater;

		private Label statusLbl;

		private BackgroundWorker backgroundExtracter;

		private Label label3;

		private FolderBrowserDialog targetBrowser;

		private Button browseBtn;

		private CheckBox betaCheck;

		private CheckBox restartCheck;

		private Timer restartTimer;

		private ToolTip restartTooltip;

		private Label updateLbl;

		private Button settingsBtn;

		private ToolTip legacyTooltip;

		public Form1()
		{
			this.InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			Assembly executingAssembly = Assembly.GetExecutingAssembly();
			FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(executingAssembly.Location);
			string[] array = versionInfo.FileVersion.Split(new char[]
			{
				'.'
			});
			this.Text = "PixelVision² Updater v" + array[0] + "." + array[1];
			string[] commandLineArgs = Environment.GetCommandLineArgs();
			bool flag = commandLineArgs.Length > 1;
			if (flag)
			{
				string[] array2 = commandLineArgs;
				for (int i = 0; i < array2.Length; i++)
				{
					string text = array2[i];
					bool flag2 = text != "" && text != null;
					if (flag2)
					{
						FileInfo fileInfo = new FileInfo(Assembly.GetExecutingAssembly().Location);
						bool flag3 = File.Exists(fileInfo.DirectoryName + "\\" + text);
						if (flag3)
						{
							try
							{
								File.Delete(fileInfo.DirectoryName + "\\" + text);
							}
							catch (UnauthorizedAccessException var_11_DD)
							{
								MessageBox.Show(string.Concat(new string[]
								{
									"An access error occured when attempting to delete the old PixelVision Updater tool. Please ensure that the specified file/path is not read only and try again.",
									Environment.NewLine,
									Environment.NewLine,
									fileInfo.DirectoryName,
									"\\oldVersionFile.exe"
								}));
							}
						}
					}
				}
			}
			bool flag4 = Settings.Default.targetFolder != "";
			if (flag4)
			{
				this.targetBox.Text = Settings.Default.targetFolder;
			}
			else
			{
				this.getPathFromRegistry();
			}
			bool beta = Settings.Default.beta;
			if (beta)
			{
				this.betaCheck.Checked = true;
			}
			else
			{
				this.betaCheck.Checked = false;
			}
			bool restart = Settings.Default.restart;
			if (restart)
			{
				this.restartCheck.Checked = true;
			}
			else
			{
				this.restartCheck.Checked = false;
			}
			bool flag5 = !this.backgroundUpdater.IsBusy;
			if (flag5)
			{
				this.statusLbl.Text = "Getting version info...";
				this.backgroundUpdater.RunWorkerAsync();
			}
		}

		private void getPathFromRegistry()
		{
			try
			{
				RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\Valve\\Steam");
				this.steamPath = (string)registryKey.GetValue("SteamPath");
				this.steamPath += "/skins/PixelVision";
				this.steamPath = this.steamPath.Replace("/", "\\");
				this.steamPath = this.steamPath.Substring(0, 1).ToUpper() + this.steamPath.Substring(1, this.steamPath.Length - 1);
			}
			catch
			{
			}
			string[] array = this.steamPath.Split(new char[]
			{
				'\\'
			});
			for (int i = 0; i < array.Length; i++)
			{
				bool flag = array[i].ToLower() == "program files";
				if (flag)
				{
					array[i] = "Program Files";
				}
				else
				{
					bool flag2 = array[i].ToLower() == "program files (x86)";
					if (flag2)
					{
						array[i] = "Program Files (x86)";
					}
					else
					{
						bool flag3 = array[i].ToLower() == "steam";
						if (flag3)
						{
							array[i] = "Steam";
						}
					}
				}
			}
			this.steamPath = "";
			string[] array2 = array;
			for (int j = 0; j < array2.Length; j++)
			{
				string str = array2[j];
				bool flag4 = this.steamPath == "";
				if (flag4)
				{
					this.steamPath = str;
				}
				else
				{
					this.steamPath = this.steamPath + "\\" + str;
				}
			}
			this.targetBox.Text = this.steamPath;
		}

		private void browseBtn_Click(object sender, EventArgs e)
		{
			this.targetBrowser.ShowDialog();
			bool flag = this.targetBrowser.SelectedPath != "";
			if (flag)
			{
				this.targetBox.Text = this.targetBrowser.SelectedPath;
				this.steamPath = this.targetBrowser.SelectedPath;
			}
			this.label3.Focus();
		}

		private void downloadBtn_Click(object sender, EventArgs e)
		{;
            if (this.downloadBtn.Text == "Retry") {
                this.downloadBtn.Text = "Download";
                this.Form1_Load(null, null);
                return;
            }
			bool flag = this.downloadPath != "";
			if (flag)
			{
				bool flag2 = Directory.Exists(this.downloadPath);
				if (flag2)
				{
					Directory.Delete(this.downloadPath, true);
				}
			}
            var url = this.sourceBox.Text;
			HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
			httpWebRequest.Timeout = 15000;
			httpWebRequest.Method = "HEAD";
			try
			{
				using ((HttpWebResponse)httpWebRequest.GetResponse())
				{
				}
			}
			catch (Exception ex)
			{
                    this.DownloadException(ex, url);
				return;
			}
			this.tempLocation = Path.GetTempPath() + "temp_" + DateTime.Now.ToString("MM\\-dd\\-yyyy_HH\\-mm\\-ss");
			this.downloadPath = this.tempLocation;
			this.enableElements(false);
			this.extractBtn.Enabled = false;
			this.statusLbl.Text = "Downloading...";
			bool @checked = this.betaCheck.Checked;
			if (@checked)
			{
				this.legacyDownload = true;
			}
			else
			{
				this.legacyDownload = false;
			}
			Directory.CreateDirectory(this.tempLocation);
			WebClient webClient = new WebClient();
			webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(this.doneDownloading);
			webClient.DownloadFileAsync(new Uri(this.sourceBox.Text), this.tempLocation + "/temp.zip");
			this.label1.Focus();
		}

		private void doneDownloading(object sender, AsyncCompletedEventArgs e)
		{
			this.statusLbl.Text = "Finished downloading, ready to install.";
			this.enableElements(true);
			this.extractBtn.Enabled = true;
		}

        private void retryElements() {
            this.downloadBtn.Text = "Retry";
            this.downloadBtn.Enabled = true;
        }

		private void enableElements(bool arg)
		{
			this.downloadBtn.Enabled = arg;
			this.betaCheck.Enabled = arg;
			this.sourceBox.Enabled = arg;
			bool flag = !this.betaEnabled && this.betaCheck.Checked;
			if (flag)
			{
				this.betaCheck.Checked = false;
			}
		}

		private void betaCheck_CheckedChanged(object sender, EventArgs e)
		{
			Settings.Default.beta = this.betaCheck.Checked;
			Settings.Default.Save();
			bool @checked = this.betaCheck.Checked;
			if (@checked)
			{
				this.sourceBox.Text = this.betaVersion;
			}
			else
			{
				this.sourceBox.Text = this.liveVersion;
			}
		}

		private void extractBtn_Click(object sender, EventArgs e)
		{
			this.settingsBtn.Enabled = false;
			bool flag = this.steamPath != "";
			if (flag)
			{
				bool flag2 = this.steamPath.Substring(1, 2) == ":/" || this.steamPath.Substring(1, 2) == ":\\";
				if (flag2)
				{
					bool flag3 = Directory.Exists(this.steamPath.Substring(0, this.steamPath.LastIndexOf("\\")));
					if (flag3)
					{
						this.statusLbl.Text = "Installing...";
						bool flag4 = !this.backgroundExtracter.IsBusy;
						if (flag4)
						{
							this.backgroundExtracter.RunWorkerAsync();
						}
					}
					else
					{
						MessageBox.Show("Skins directory not found.");
					}
				}
				else
				{
					MessageBox.Show("Invalid directory.");
					this.getPathFromRegistry();
				}
			}
			else
			{
				MessageBox.Show("Nothing entered.");
				this.getPathFromRegistry();
			}
		}

		private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(sourceDirName);
			DirectoryInfo[] directories = directoryInfo.GetDirectories();
			bool flag = !directoryInfo.Exists;
			if (flag)
			{
				throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDirName);
			}
			bool flag2 = !Directory.Exists(destDirName);
			if (flag2)
			{
				Directory.CreateDirectory(destDirName);
			}
			FileInfo[] files = directoryInfo.GetFiles();
			FileInfo[] array = files;
			for (int i = 0; i < array.Length; i++)
			{
				FileInfo fileInfo = array[i];
				string destFileName = Path.Combine(destDirName + "\\", fileInfo.Name);
				fileInfo.CopyTo(destFileName, false);
			}
			if (copySubDirs)
			{
				DirectoryInfo[] array2 = directories;
				for (int j = 0; j < array2.Length; j++)
				{
					DirectoryInfo directoryInfo2 = array2[j];
					string destDirName2 = Path.Combine(destDirName + "\\", directoryInfo2.Name);
					Form1.DirectoryCopy(directoryInfo2.FullName, destDirName2, copySubDirs);
				}
			}
		}

		private void backgroundUpdater_DoWork(object sender, DoWorkEventArgs e)
		{
			this.backgroundResult = "";
			using (WebClient webClient = new WebClient())
			{
				try
				{
					this.backgroundResult = webClient.DownloadString(this.updateUrl);
				}
				catch (WebException var_1_29)
				{
					MessageBox.Show("Failed to connect to the internet, please check your antivirus or firewall settings and try again.");
					this.connection = false;
				}
			}
		}

		private void backgroundUpdater_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			bool flag = this.connection;
			if (flag) {
				this.statusLbl.Text = "Idle...";
				Regex regex = new Regex("<[^>]+>");
				bool flag2 = regex.IsMatch(this.backgroundResult);
				if (flag2)
				{
					this.statusLbl.Text = "Failed to check for updated version, please try again later.";
				}
				else
				{
					bool flag3 = this.backgroundResult.ToLower() == "no downloads available";
					if (flag3)
					{
						this.statusLbl.Text = "All available download locations are currently inaccessible.";
					}
					else
					{
						bool flag4 = this.backgroundResult.ToLower() == "error";
						if (flag4)
						{
							this.statusLbl.Text = "An error occurred getting a download link, please try again later.";
						}
						else
						{
							string[] array = this.backgroundResult.Split(new char[]
							{
								';'
							});
							string[] array2 = array;
							for (int i = 0; i < array2.Length; i++)
							{
								string text = array2[i];
								string[] array3 = text.Split(new char[]
								{
									'='
								});
								bool flag5 = array3[0] == "url";
								if (flag5)
								{
									this.liveVersion = "";
									for (int j = 1; j < array3.Length; j++)
									{
										bool flag6 = j > 1;
										if (flag6)
										{
											this.betaVersion += "=";
										}
										this.liveVersion += array3[j];
									}
									bool flag7 = !this.betaCheck.Checked;
									if (flag7)
									{
										this.sourceBox.Text = this.liveVersion;
									}
								}
								else
								{
									bool flag8 = array3[0] == "root";
									if (flag8)
									{
										this.rootPath = array3[1];
									}
									else
									{
										bool flag9 = array3[0] == "legacyroot";
										if (flag9)
										{
											this.legacyRootPath = array3[1];
										}
										else
										{
											bool flag10 = array3[0] == "version";
											if (flag10)
											{
												this.version = array3[1];
											}
											else
											{
												bool flag11 = array3[0] == "tool";
												if (flag11)
												{
													this.versionUrl = array3[1];
												}
												else
												{
													bool flag12 = array3[0] == "legacy";
													if (flag12)
													{
														bool flag13 = array3[1] == "false";
														if (flag13)
														{
															this.betaEnabled = false;
														}
														else
														{
															this.betaVersion = "";
															for (int k = 1; k < array3.Length; k++)
															{
																bool flag14 = k > 1;
																if (flag14)
																{
																	this.betaVersion += "=";
																}
																this.betaVersion += array3[k];
															}
															this.betaEnabled = true;
															bool @checked = this.betaCheck.Checked;
															if (@checked)
															{
																this.sourceBox.Text = this.betaVersion;
															}
														}
													}
												}
											}
										}
									}
								}
							}
							bool flag15 = this.updateAvailable();
							if (flag15)
							{
								this.updateLbl.Visible = true;
								DialogResult dialogResult = MessageBox.Show(string.Concat(new string[]
								{
									"A new version (",
									this.version.Substring(0, 3),
									") of this tool is available.",
									Environment.NewLine,
									Environment.NewLine,
									"Would you like to update now?"
								}), "Update available", MessageBoxButtons.YesNo);
								bool flag16 = dialogResult == DialogResult.Yes;
								if (flag16)
								{
									this.updateVersion();
								}
								else
								{
									this.label1.Focus();
								}
							}
							this.enableElements(true);
						}
					}
				}
			} else {
                this.retryElements();
            }
		}

		private bool updateAvailable()
		{
			Assembly executingAssembly = Assembly.GetExecutingAssembly();
			FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(executingAssembly.Location);
			int num = int.Parse(versionInfo.FileVersion.Replace(".", ""));
			int num2 = int.Parse(this.version.Replace(".", ""));
			return num2 > num;
		}

		private void updateLbl_Click(object sender, EventArgs e)
		{
			DialogResult dialogResult = MessageBox.Show(string.Concat(new string[]
			{
				"A new version (",
				this.version.Substring(0, 3),
				") of this tool is available.",
				Environment.NewLine,
				Environment.NewLine,
				"Would you like to update now?"
			}), "Update available", MessageBoxButtons.YesNo);
			bool flag = dialogResult == DialogResult.Yes;
			if (flag)
			{
				this.updateVersion();
			}
			else
			{
				this.label1.Focus();
			}
		}

        private void DownloadException(Exception ex, string url = "") {
            this.statusLbl.Text = "Download Error (" + ex.Message + ")";
            MessageBox.Show($"Error downloading\n\n{url}\n{ex.Message}\n{ex.StackTrace}");
        }

		private void updateVersion()
		{
			try
			{
				FileInfo fileInfo = new FileInfo(Assembly.GetExecutingAssembly().Location);
				this.hashString = this.HashFile(fileInfo.Name);
				File.Move(fileInfo.FullName, fileInfo.DirectoryName + "\\" + this.hashString);
                var url = this.versionUrl;
				HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
				httpWebRequest.Timeout = 15000;
				httpWebRequest.Method = "HEAD";
				try
				{
					using ((HttpWebResponse)httpWebRequest.GetResponse())
					{
					}
				}
				catch (Exception ex)
				{
                    this.DownloadException(ex, url);
					return;
				}
				this.enableElements(false);
				this.extractBtn.Enabled = false;
				this.statusLbl.Text = "Updating to new version...";
				WebClient webClient = new WebClient();
				webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(this.doneUpdating);
				webClient.DownloadFileAsync(new Uri(this.versionUrl), fileInfo.FullName);
			}
			catch (UnauthorizedAccessException ex)
			{
				MessageBox.Show(string.Concat(new object[]
				{
					"Error. Access Denied.",
					Environment.NewLine,
					Environment.NewLine,
					ex
				}));
			}
		}

		private void doneUpdating(object sender, AsyncCompletedEventArgs e)
		{
			FileInfo fileInfo = new FileInfo(Assembly.GetExecutingAssembly().Location);
			Process.Start(fileInfo.FullName, this.hashString);
			base.Close();
		}

		public string HashFile(string filePath)
		{
			string result;
			using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				result = this.HashFile(fileStream);
			}
			return result;
		}

		public string HashFile(FileStream stream)
		{
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = stream != null;
			if (flag)
			{
				stream.Seek(0L, SeekOrigin.Begin);
				MD5 mD = MD5.Create();
				byte[] array = mD.ComputeHash(stream);
				byte[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					byte b = array2[i];
					stringBuilder.Append(b.ToString("x2"));
				}
				stream.Seek(0L, SeekOrigin.Begin);
			}
			return stringBuilder.ToString();
		}

		private void targetBox_TextChanged(object sender, EventArgs e)
		{
			Settings.Default.targetFolder = this.targetBox.Text;
			Settings.Default.Save();
			this.steamPath = this.targetBox.Text;
		}

		private void backgroundExtracter_DoWork(object sender, DoWorkEventArgs e)
		{
			this.corrupted = false;
			this.savedSettings = false;
			this.savedLocale = false;
			this.structureInteg = true;
			this.createdFolder = false;
			this.aborted = false;
			this.extractError = false;
			try
			{
				ZipFile.ExtractToDirectory(this.tempLocation + "/temp.zip", this.tempLocation);
			}
			catch (Exception ex)
			{
				Directory.Delete(this.tempLocation, true);
				bool flag = ex is InvalidDataException || ex is InvalidOperationException;
				if (flag)
				{
					this.corrupted = true;
					return;
				}
				throw;
			}
			bool flag2 = Form1.DirSize(new DirectoryInfo(this.tempLocation)) < 1000000L;
			if (flag2)
			{
				MessageBox.Show("Failed to extract archive, possibly corrupted. Please download a new copy.");
				this.extractError = true;
			}
			bool flag3 = !this.extractError;
			if (flag3)
			{
				bool flag4 = Directory.Exists(this.steamPath);
				if (flag4)
				{
					bool flag5 = !this.wipeCheck.Checked;
					if (flag5)
					{
						bool flag6 = File.Exists(this.steamPath + "/settings.ini");
						if (flag6)
						{
							File.Copy(this.steamPath + "/settings.ini", this.tempLocation + "/settings.ini");
							this.savedSettings = true;
						}
						bool flag7 = File.Exists(this.steamPath + "/locale.ini");
						if (flag7)
						{
							File.Copy(this.steamPath + "/locale.ini", this.tempLocation + "/locale.ini");
							this.savedLocale = true;
						}
					}
					bool flag8 = this.steamPath.ToLower().Contains("pixelvision") && this.steamPath.ToLower().Contains("skins");
					if (flag8)
					{
						Directory.Delete(this.steamPath, true);
					}
					else
					{
						MessageBox.Show("Target directory should be the PixelVision skin folder, please double check your target directory.");
						this.aborted = true;
					}
				}
				else
				{
					Directory.CreateDirectory(this.steamPath);
					this.createdFolder = true;
				}
				bool flag9 = !this.aborted;
				if (flag9)
				{
					bool flag10 = this.legacyDownload;
					string str;
					if (flag10)
					{
						str = this.legacyRootPath;
					}
					else
					{
						str = this.rootPath;
					}
					bool flag11 = Directory.Exists(this.tempLocation + str);
					if (flag11)
					{
						Form1.DirectoryCopy(this.tempLocation + str, this.steamPath, true);
					}
					else
					{
						this.structureInteg = false;
					}
					bool flag12 = this.savedSettings && this.structureInteg;
					if (flag12)
					{
						File.Copy(this.tempLocation + "/settings.ini", this.steamPath + "/settings.ini", true);
					}
					bool flag13 = this.savedLocale && this.structureInteg;
					if (flag13)
					{
						File.Copy(this.tempLocation + "/locale.ini", this.steamPath + "/locale.ini", true);
					}
				}
			}
			Directory.Delete(this.tempLocation, true);
		}

		public static long DirSize(DirectoryInfo d)
		{
			long num = 0L;
			FileInfo[] files = d.GetFiles();
			FileInfo[] array = files;
			for (int i = 0; i < array.Length; i++)
			{
				FileInfo fileInfo = array[i];
				num += fileInfo.Length;
			}
			DirectoryInfo[] directories = d.GetDirectories();
			DirectoryInfo[] array2 = directories;
			for (int j = 0; j < array2.Length; j++)
			{
				DirectoryInfo d2 = array2[j];
				num += Form1.DirSize(d2);
			}
			return num;
		}

		private void backgroundExtracter_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			this.extractBtn.Enabled = false;
			this.settingsBtn.Enabled = true;
			bool flag = this.corrupted;
			if (flag)
			{
				this.statusLbl.Text = "Archive is corrupted. Could not extract.";
			}
			else
			{
				bool flag2 = !this.structureInteg;
				if (flag2)
				{
					this.statusLbl.Text = "Structure not recognized. Could not install.";
				}
				else
				{
					bool flag3 = this.createdFolder;
					if (flag3)
					{
						this.statusLbl.Text = "Created PixelVision folder. Finished installing.";
					}
					else
					{
						bool flag4 = this.aborted || this.extractError;
						if (flag4)
						{
							this.statusLbl.Text = "Idle...";
						}
						else
						{
							this.statusLbl.Text = "Finished installing.";
						}
					}
				}
			}
			bool flag5 = this.restartCheck.Checked && !this.corrupted;
			if (flag5)
			{
				Process[] processes = Process.GetProcesses();
				for (int i = 0; i < processes.Length; i++)
				{
					Process process = processes[i];
					bool flag6 = process.ProcessName == "Steam";
					if (flag6)
					{
						this.statusLbl.Text = "Closing Steam...";
						this.steamProcess = process;
						this.steamProcessPath = process.MainModule.FileName;
						process.Kill();
						this.restartTimer.Start();
						this.counter = 0;
					}
				}
			}
		}

		private void restartCheck_CheckedChanged(object sender, EventArgs e)
		{
			Settings.Default.restart = this.restartCheck.Checked;
			Settings.Default.Save();
		}

		private void restartTimer_Tick(object sender, EventArgs e)
		{
			this.counter++;
			bool flag = this.counter > 15;
			if (flag)
			{
				this.statusLbl.Text = "Steam could not be restarted.";
				this.restartTimer.Stop();
			}
			try
			{
				bool hasExited = this.steamProcess.HasExited;
				if (hasExited)
				{
					Process.Start(this.steamProcessPath);
					this.statusLbl.Text = "Steam has been restarted.";
					this.restartTimer.Stop();
				}
			}
			catch (Exception var_2_7B)
			{
				Process.Start(this.steamProcessPath);
				this.statusLbl.Text = "Steam has been restarted.";
				this.restartTimer.Stop();
			}
		}

		private void restartCheck_MouseHover(object sender, EventArgs e)
		{
			this.restartTooltip.Show("WARNING: This will force close your Steam client" + Environment.NewLine + "and possibly any games you have open", this.restartCheck);
		}

		private void Form1_Resize(object sender, EventArgs e)
		{
			this.downloadBtn.Location = new Point(base.Size.Width - 123, 23);
			this.settingsBtn.Location = new Point(base.Size.Width - 123, 56);
			this.extractBtn.Location = new Point(base.Size.Width - 123, 90);
			this.browseBtn.Location = new Point(base.Size.Width - 188, 90);
			this.sourceBox.Size = new Size(base.Size.Width - 141, 20);
			this.targetBox.Size = new Size(base.Size.Width - 206, 20);
		}

		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			bool flag = Directory.Exists(this.tempLocation);
			if (flag)
			{
				Directory.Delete(this.tempLocation, true);
			}
		}

		private void settingsBtn_Click(object sender, EventArgs e)
		{
			this.statusLbl.Focus();
			bool flag = File.Exists(this.steamPath + "\\settings.ini");
			if (flag)
			{
				new SettingsForm
				{
					settingsPath = this.steamPath + "\\settings.ini"
				}.ShowDialog();
			}
			else
			{
				MessageBox.Show("settings.ini not found. The skin needs to be installed first before you can modify options.");
			}
		}

		private void betaCheck_MouseHover(object sender, EventArgs e)
		{
			this.restartTooltip.Show("This option will download the old version of" + Environment.NewLine + "PixelVision before it became PixelVision²", this.betaCheck);
		}

		protected override void Dispose(bool disposing)
		{
			bool flag = disposing && this.components != null;
			if (flag)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.sourceBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.downloadBtn = new System.Windows.Forms.Button();
            this.extractBtn = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.targetBox = new System.Windows.Forms.TextBox();
            this.wipeCheck = new System.Windows.Forms.CheckBox();
            this.backgroundUpdater = new System.ComponentModel.BackgroundWorker();
            this.statusLbl = new System.Windows.Forms.Label();
            this.backgroundExtracter = new System.ComponentModel.BackgroundWorker();
            this.label3 = new System.Windows.Forms.Label();
            this.targetBrowser = new System.Windows.Forms.FolderBrowserDialog();
            this.browseBtn = new System.Windows.Forms.Button();
            this.betaCheck = new System.Windows.Forms.CheckBox();
            this.restartCheck = new System.Windows.Forms.CheckBox();
            this.restartTimer = new System.Windows.Forms.Timer(this.components);
            this.restartTooltip = new System.Windows.Forms.ToolTip(this.components);
            this.updateLbl = new System.Windows.Forms.Label();
            this.settingsBtn = new System.Windows.Forms.Button();
            this.legacyTooltip = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // sourceBox
            // 
            this.sourceBox.BackColor = System.Drawing.Color.White;
            this.sourceBox.Enabled = false;
            this.sourceBox.ForeColor = System.Drawing.Color.Black;
            this.sourceBox.Location = new System.Drawing.Point(12, 25);
            this.sourceBox.Name = "sourceBox";
            this.sourceBox.ReadOnly = true;
            this.sourceBox.Size = new System.Drawing.Size(297, 20);
            this.sourceBox.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(90, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Download source";
            // 
            // downloadBtn
            // 
            this.downloadBtn.Enabled = false;
            this.downloadBtn.Location = new System.Drawing.Point(315, 23);
            this.downloadBtn.Name = "downloadBtn";
            this.downloadBtn.Size = new System.Drawing.Size(95, 23);
            this.downloadBtn.TabIndex = 3;
            this.downloadBtn.Text = "Download";
            this.downloadBtn.UseVisualStyleBackColor = true;
            this.downloadBtn.Click += new System.EventHandler(this.downloadBtn_Click);
            // 
            // extractBtn
            // 
            this.extractBtn.Enabled = false;
            this.extractBtn.Location = new System.Drawing.Point(315, 90);
            this.extractBtn.Name = "extractBtn";
            this.extractBtn.Size = new System.Drawing.Size(95, 23);
            this.extractBtn.TabIndex = 7;
            this.extractBtn.Text = "Install";
            this.extractBtn.UseVisualStyleBackColor = true;
            this.extractBtn.Click += new System.EventHandler(this.extractBtn_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 76);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(86, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "PixelVision folder";
            // 
            // targetBox
            // 
            this.targetBox.Location = new System.Drawing.Point(12, 92);
            this.targetBox.Name = "targetBox";
            this.targetBox.Size = new System.Drawing.Size(232, 20);
            this.targetBox.TabIndex = 5;
            this.targetBox.Text = "C:\\Program Files (x86)\\Steam\\skins\\PixelVision";
            this.targetBox.TextChanged += new System.EventHandler(this.targetBox_TextChanged);
            // 
            // wipeCheck
            // 
            this.wipeCheck.AutoSize = true;
            this.wipeCheck.Location = new System.Drawing.Point(12, 53);
            this.wipeCheck.Name = "wipeCheck";
            this.wipeCheck.Size = new System.Drawing.Size(90, 17);
            this.wipeCheck.TabIndex = 3;
            this.wipeCheck.Text = "Wipe settings";
            this.wipeCheck.UseVisualStyleBackColor = true;
            // 
            // backgroundUpdater
            // 
            this.backgroundUpdater.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundUpdater_DoWork);
            this.backgroundUpdater.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundUpdater_RunWorkerCompleted);
            // 
            // statusLbl
            // 
            this.statusLbl.AutoSize = true;
            this.statusLbl.ForeColor = System.Drawing.Color.Black;
            this.statusLbl.Location = new System.Drawing.Point(48, 119);
            this.statusLbl.Name = "statusLbl";
            this.statusLbl.Size = new System.Drawing.Size(33, 13);
            this.statusLbl.TabIndex = 0;
            this.statusLbl.Text = "Idle...";
            // 
            // backgroundExtracter
            // 
            this.backgroundExtracter.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundExtracter_DoWork);
            this.backgroundExtracter.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundExtracter_RunWorkerCompleted);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 119);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(40, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Status:";
            // 
            // targetBrowser
            // 
            this.targetBrowser.RootFolder = System.Environment.SpecialFolder.MyComputer;
            // 
            // browseBtn
            // 
            this.browseBtn.Location = new System.Drawing.Point(250, 90);
            this.browseBtn.Name = "browseBtn";
            this.browseBtn.Size = new System.Drawing.Size(59, 23);
            this.browseBtn.TabIndex = 6;
            this.browseBtn.Text = "Browse";
            this.browseBtn.UseVisualStyleBackColor = true;
            this.browseBtn.Click += new System.EventHandler(this.browseBtn_Click);
            // 
            // betaCheck
            // 
            this.betaCheck.AutoSize = true;
            this.betaCheck.Enabled = false;
            this.betaCheck.Location = new System.Drawing.Point(108, 53);
            this.betaCheck.Name = "betaCheck";
            this.betaCheck.Size = new System.Drawing.Size(61, 17);
            this.betaCheck.TabIndex = 4;
            this.betaCheck.Text = "Legacy";
            this.betaCheck.UseVisualStyleBackColor = true;
            this.betaCheck.CheckedChanged += new System.EventHandler(this.betaCheck_CheckedChanged);
            this.betaCheck.MouseHover += new System.EventHandler(this.betaCheck_MouseHover);
            // 
            // restartCheck
            // 
            this.restartCheck.AutoSize = true;
            this.restartCheck.Location = new System.Drawing.Point(175, 53);
            this.restartCheck.Name = "restartCheck";
            this.restartCheck.Size = new System.Drawing.Size(93, 17);
            this.restartCheck.TabIndex = 8;
            this.restartCheck.Text = "Restart Steam";
            this.restartCheck.UseVisualStyleBackColor = true;
            this.restartCheck.CheckedChanged += new System.EventHandler(this.restartCheck_CheckedChanged);
            this.restartCheck.MouseHover += new System.EventHandler(this.restartCheck_MouseHover);
            // 
            // restartTimer
            // 
            this.restartTimer.Interval = 1000;
            this.restartTimer.Tick += new System.EventHandler(this.restartTimer_Tick);
            // 
            // restartTooltip
            // 
            this.restartTooltip.AutoPopDelay = 500000;
            this.restartTooltip.InitialDelay = 10;
            this.restartTooltip.ReshowDelay = 5;
            // 
            // updateLbl
            // 
            this.updateLbl.AutoSize = true;
            this.updateLbl.Cursor = System.Windows.Forms.Cursors.Hand;
            this.updateLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.updateLbl.ForeColor = System.Drawing.Color.Blue;
            this.updateLbl.Location = new System.Drawing.Point(299, 119);
            this.updateLbl.Name = "updateLbl";
            this.updateLbl.Size = new System.Drawing.Size(114, 13);
            this.updateLbl.TabIndex = 9;
            this.updateLbl.Text = "New version available!";
            this.updateLbl.Visible = false;
            this.updateLbl.Click += new System.EventHandler(this.updateLbl_Click);
            // 
            // settingsBtn
            // 
            this.settingsBtn.Location = new System.Drawing.Point(315, 57);
            this.settingsBtn.Name = "settingsBtn";
            this.settingsBtn.Size = new System.Drawing.Size(95, 23);
            this.settingsBtn.TabIndex = 10;
            this.settingsBtn.Text = "Skin Settings";
            this.settingsBtn.UseVisualStyleBackColor = true;
            this.settingsBtn.Click += new System.EventHandler(this.settingsBtn_Click);
            // 
            // legacyTooltip
            // 
            this.legacyTooltip.AutoPopDelay = 500000;
            this.legacyTooltip.InitialDelay = 10;
            this.legacyTooltip.ReshowDelay = 5;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(428, 150);
            this.Controls.Add(this.settingsBtn);
            this.Controls.Add(this.updateLbl);
            this.Controls.Add(this.restartCheck);
            this.Controls.Add(this.betaCheck);
            this.Controls.Add(this.browseBtn);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.statusLbl);
            this.Controls.Add(this.wipeCheck);
            this.Controls.Add(this.extractBtn);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.targetBox);
            this.Controls.Add(this.downloadBtn);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.sourceBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(2000, 179);
            this.MinimumSize = new System.Drawing.Size(438, 179);
            this.Name = "Form1";
            this.Text = "PixelVision² Updater v1.0";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Resize += new System.EventHandler(this.Form1_Resize);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
	}
}
