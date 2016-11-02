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
		{
			bool flag = this.downloadPath != "";
			if (flag)
			{
				bool flag2 = Directory.Exists(this.downloadPath);
				if (flag2)
				{
					Directory.Delete(this.downloadPath, true);
				}
			}
			HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(this.sourceBox.Text);
			httpWebRequest.Timeout = 15000;
			httpWebRequest.Method = "HEAD";
			try
			{
				using ((HttpWebResponse)httpWebRequest.GetResponse())
				{
				}
			}
			catch (Exception var_5_85)
			{
				this.statusLbl.Text = "Could not resolve download. Please try again later.";
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
			if (flag)
			{
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

		private void updateVersion()
		{
			try
			{
				FileInfo fileInfo = new FileInfo(Assembly.GetExecutingAssembly().Location);
				this.hashString = this.HashFile(fileInfo.Name);
				File.Move(fileInfo.FullName, fileInfo.DirectoryName + "\\" + this.hashString);
				HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(this.versionUrl);
				httpWebRequest.Timeout = 15000;
				httpWebRequest.Method = "HEAD";
				try
				{
					using ((HttpWebResponse)httpWebRequest.GetResponse())
					{
					}
				}
				catch (Exception var_4_8E)
				{
					this.statusLbl.Text = "Could not resolve download. Please try again later.";
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
			this.components = new Container();
			ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(Form1));
			this.sourceBox = new TextBox();
			this.label1 = new Label();
			this.downloadBtn = new Button();
			this.extractBtn = new Button();
			this.label2 = new Label();
			this.targetBox = new TextBox();
			this.wipeCheck = new CheckBox();
			this.backgroundUpdater = new BackgroundWorker();
			this.statusLbl = new Label();
			this.backgroundExtracter = new BackgroundWorker();
			this.label3 = new Label();
			this.targetBrowser = new FolderBrowserDialog();
			this.browseBtn = new Button();
			this.betaCheck = new CheckBox();
			this.restartCheck = new CheckBox();
			this.restartTimer = new Timer(this.components);
			this.restartTooltip = new ToolTip(this.components);
			this.updateLbl = new Label();
			this.settingsBtn = new Button();
			this.legacyTooltip = new ToolTip(this.components);
			base.SuspendLayout();
			this.sourceBox.BackColor = Color.White;
			this.sourceBox.Enabled = false;
			this.sourceBox.ForeColor = Color.Black;
			this.sourceBox.Location = new Point(12, 25);
			this.sourceBox.Name = "sourceBox";
			this.sourceBox.ReadOnly = true;
			this.sourceBox.Size = new Size(297, 20);
			this.sourceBox.TabIndex = 2;
			this.label1.AutoSize = true;
			this.label1.Location = new Point(10, 9);
			this.label1.Name = "label1";
			this.label1.Size = new Size(90, 13);
			this.label1.TabIndex = 1;
			this.label1.Text = "Download source";
			this.downloadBtn.Enabled = false;
			this.downloadBtn.Location = new Point(315, 23);
			this.downloadBtn.Name = "downloadBtn";
			this.downloadBtn.Size = new Size(95, 23);
			this.downloadBtn.TabIndex = 3;
			this.downloadBtn.Text = "Download";
			this.downloadBtn.UseVisualStyleBackColor = true;
			this.downloadBtn.Click += new EventHandler(this.downloadBtn_Click);
			this.extractBtn.Enabled = false;
			this.extractBtn.Location = new Point(315, 90);
			this.extractBtn.Name = "extractBtn";
			this.extractBtn.Size = new Size(95, 23);
			this.extractBtn.TabIndex = 7;
			this.extractBtn.Text = "Install";
			this.extractBtn.UseVisualStyleBackColor = true;
			this.extractBtn.Click += new EventHandler(this.extractBtn_Click);
			this.label2.AutoSize = true;
			this.label2.Location = new Point(10, 76);
			this.label2.Name = "label2";
			this.label2.Size = new Size(86, 13);
			this.label2.TabIndex = 4;
			this.label2.Text = "PixelVision folder";
			this.targetBox.Location = new Point(12, 92);
			this.targetBox.Name = "targetBox";
			this.targetBox.Size = new Size(232, 20);
			this.targetBox.TabIndex = 5;
			this.targetBox.Text = "C:\\Program Files (x86)\\Steam\\skins\\PixelVision";
			this.targetBox.TextChanged += new EventHandler(this.targetBox_TextChanged);
			this.wipeCheck.AutoSize = true;
			this.wipeCheck.Location = new Point(12, 53);
			this.wipeCheck.Name = "wipeCheck";
			this.wipeCheck.Size = new Size(90, 17);
			this.wipeCheck.TabIndex = 3;
			this.wipeCheck.Text = "Wipe settings";
			this.wipeCheck.UseVisualStyleBackColor = true;
			this.backgroundUpdater.DoWork += new DoWorkEventHandler(this.backgroundUpdater_DoWork);
			this.backgroundUpdater.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.backgroundUpdater_RunWorkerCompleted);
			this.statusLbl.AutoSize = true;
			this.statusLbl.ForeColor = Color.Black;
			this.statusLbl.Location = new Point(48, 119);
			this.statusLbl.Name = "statusLbl";
			this.statusLbl.Size = new Size(33, 13);
			this.statusLbl.TabIndex = 0;
			this.statusLbl.Text = "Idle...";
			this.backgroundExtracter.DoWork += new DoWorkEventHandler(this.backgroundExtracter_DoWork);
			this.backgroundExtracter.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.backgroundExtracter_RunWorkerCompleted);
			this.label3.AutoSize = true;
			this.label3.Location = new Point(9, 119);
			this.label3.Name = "label3";
			this.label3.Size = new Size(40, 13);
			this.label3.TabIndex = 0;
			this.label3.Text = "Status:";
			this.targetBrowser.RootFolder = Environment.SpecialFolder.MyComputer;
			this.browseBtn.Location = new Point(250, 90);
			this.browseBtn.Name = "browseBtn";
			this.browseBtn.Size = new Size(59, 23);
			this.browseBtn.TabIndex = 6;
			this.browseBtn.Text = "Browse";
			this.browseBtn.UseVisualStyleBackColor = true;
			this.browseBtn.Click += new EventHandler(this.browseBtn_Click);
			this.betaCheck.AutoSize = true;
			this.betaCheck.Enabled = false;
			this.betaCheck.Location = new Point(108, 53);
			this.betaCheck.Name = "betaCheck";
			this.betaCheck.Size = new Size(61, 17);
			this.betaCheck.TabIndex = 4;
			this.betaCheck.Text = "Legacy";
			this.betaCheck.UseVisualStyleBackColor = true;
			this.betaCheck.CheckedChanged += new EventHandler(this.betaCheck_CheckedChanged);
			this.betaCheck.MouseHover += new EventHandler(this.betaCheck_MouseHover);
			this.restartCheck.AutoSize = true;
			this.restartCheck.Location = new Point(175, 53);
			this.restartCheck.Name = "restartCheck";
			this.restartCheck.Size = new Size(93, 17);
			this.restartCheck.TabIndex = 8;
			this.restartCheck.Text = "Restart Steam";
			this.restartCheck.UseVisualStyleBackColor = true;
			this.restartCheck.CheckedChanged += new EventHandler(this.restartCheck_CheckedChanged);
			this.restartCheck.MouseHover += new EventHandler(this.restartCheck_MouseHover);
			this.restartTimer.Interval = 1000;
			this.restartTimer.Tick += new EventHandler(this.restartTimer_Tick);
			this.restartTooltip.AutoPopDelay = 500000;
			this.restartTooltip.InitialDelay = 10;
			this.restartTooltip.ReshowDelay = 5;
			this.updateLbl.AutoSize = true;
			this.updateLbl.Cursor = Cursors.Hand;
			this.updateLbl.Font = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Underline, GraphicsUnit.Point, 0);
			this.updateLbl.ForeColor = Color.Blue;
			this.updateLbl.Location = new Point(299, 119);
			this.updateLbl.Name = "updateLbl";
			this.updateLbl.Size = new Size(114, 13);
			this.updateLbl.TabIndex = 9;
			this.updateLbl.Text = "New version available!";
			this.updateLbl.Visible = false;
			this.updateLbl.Click += new EventHandler(this.updateLbl_Click);
			this.settingsBtn.Location = new Point(315, 57);
			this.settingsBtn.Name = "settingsBtn";
			this.settingsBtn.Size = new Size(95, 23);
			this.settingsBtn.TabIndex = 10;
			this.settingsBtn.Text = "Skin Settings";
			this.settingsBtn.UseVisualStyleBackColor = true;
			this.settingsBtn.Click += new EventHandler(this.settingsBtn_Click);
			this.legacyTooltip.AutoPopDelay = 500000;
			this.legacyTooltip.InitialDelay = 10;
			this.legacyTooltip.ReshowDelay = 5;
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			base.AutoScaleMode = AutoScaleMode.Font;
			base.ClientSize = new Size(422, 140);
			base.Controls.Add(this.settingsBtn);
			base.Controls.Add(this.updateLbl);
			base.Controls.Add(this.restartCheck);
			base.Controls.Add(this.betaCheck);
			base.Controls.Add(this.browseBtn);
			base.Controls.Add(this.label3);
			base.Controls.Add(this.statusLbl);
			base.Controls.Add(this.wipeCheck);
			base.Controls.Add(this.extractBtn);
			base.Controls.Add(this.label2);
			base.Controls.Add(this.targetBox);
			base.Controls.Add(this.downloadBtn);
			base.Controls.Add(this.label1);
			base.Controls.Add(this.sourceBox);
			base.Icon = (Icon)componentResourceManager.GetObject("$this.Icon");
			base.MaximizeBox = false;
			this.MaximumSize = new Size(2000, 179);
			this.MinimumSize = new Size(438, 179);
			base.Name = "Form1";
			this.Text = "PixelVision² Updater v1.0";
			base.FormClosing += new FormClosingEventHandler(this.Form1_FormClosing);
			base.Load += new EventHandler(this.Form1_Load);
			base.Resize += new EventHandler(this.Form1_Resize);
			base.ResumeLayout(false);
			base.PerformLayout();
		}
	}
}
