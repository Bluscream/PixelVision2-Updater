using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Pixelvision_Updater
{
	public class SettingsForm : Form
	{
		public class Setting
		{
			public string text
			{
				get;
				set;
			}

			public string code
			{
				get;
				set;
			}

			public bool enabled
			{
				get;
				set;
			}
		}

		private List<CheckBox> settingBoxes;

		private SettingsForm.Setting[] settings;

		private Process steamProcess;

		private string steamProcessPath;

		private int counter = 0;

		private IContainer components = null;

		private Button cancelBtn;

		private Button saveBtn;

		private Label label1;

		private Button saveRestartBtn;

		private Timer restartTimer;

		public string settingsPath
		{
			get;
			set;
		}

		public SettingsForm()
		{
			this.InitializeComponent();
		}

		private void SettingsForm_Load(object sender, EventArgs e)
		{
			base.Visible = false;
			int num = 0;
			int num2 = 0;
			this.settingBoxes = new List<CheckBox>();
			List<SettingsForm.Setting> list = new List<SettingsForm.Setting>();
			string text = "";
			string text2 = "";
			bool enabled = true;
			foreach (string current in File.ReadLines(this.settingsPath))
			{
				bool flag = num == 1 && (!text.ToLower().Contains("settings.ini") || !current.Contains("{")) && (!text.ToLower().Contains("settings.ini") || !text.Contains("{"));
				if (flag)
				{
					MessageBox.Show("Settings.ini is malformed, please download a new copy.");
					base.Close();
				}
				bool flag2 = current.Contains("-----");
				if (flag2)
				{
					while (string.IsNullOrWhiteSpace(text.Substring(0, 1)))
					{
						text = text.Substring(1, text.Length - 1);
					}
					bool flag3 = text.Substring(0, 2) == "//";
					if (flag3)
					{
						text = text.Substring(2, text.Length - 2);
					}
					text2 = text;
				}
				bool flag4 = text2 != "" && current.Contains("include \"");
				if (flag4)
				{
					string text3 = current;
					while (string.IsNullOrWhiteSpace(text3.Substring(0, 1)))
					{
						text3 = text3.Substring(1, text3.Length - 1);
					}
					bool flag5 = text3.Substring(0, 2) == "//";
					if (flag5)
					{
						text3 = text3.Substring(2, text3.Length - 2);
						enabled = false;
					}
					list.Add(new SettingsForm.Setting
					{
						text = text2,
						code = text3,
						enabled = enabled
					});
					text2 = "";
					enabled = true;
				}
				bool flag6 = !string.IsNullOrWhiteSpace(current);
				if (flag6)
				{
					text = current;
				}
				num++;
			}
			bool flag7 = !text.Contains("}");
			if (flag7)
			{
				MessageBox.Show("Settings.ini is malformed, please download a new copy.");
				base.Close();
			}
			this.settings = list.ToArray();
			bool flag8 = this.settings.Length == 0;
			if (flag8)
			{
				MessageBox.Show("No usable settings found, please download a new copy.");
				base.Close();
			}
			int num3 = 0;
			SettingsForm.Setting[] array = this.settings;
			for (int i = 0; i < array.Length; i++)
			{
				SettingsForm.Setting setting = array[i];
				CheckBox checkBox = new CheckBox();
				checkBox.Text = this.settings[num2].text;
				checkBox.Checked = this.settings[num2].enabled;
				checkBox.Location = new Point(12, 12 + 23 * num2);
				checkBox.AutoSize = true;
				checkBox.Name = num2.ToString();
				checkBox.Click += new EventHandler(this.settingsBox_Click);
				this.settingBoxes.Add(checkBox);
				base.Controls.Add(checkBox);
				bool flag9 = checkBox.Width > num3;
				if (flag9)
				{
					num3 = checkBox.Width;
				}
				num2++;
			}
			base.Size = new Size(num3 + 50, 12 + 23 * num2 + 80);
			foreach (CheckBox current2 in this.settingBoxes)
			{
				foreach (CheckBox current3 in this.settingBoxes)
				{
					bool flag10 = current2.Name != current3.Name;
					if (flag10)
					{
						bool flag11 = this.checkExclusive(current2.Text, current3.Text) && current3.Checked;
						if (flag11)
						{
							current2.Checked = false;
							this.settings[int.Parse(current2.Name)].enabled = current2.Checked;
						}
					}
				}
			}
			base.Visible = true;
			this.label1.Focus();
		}

		private void settingsBox_Click(object sender, EventArgs e)
		{
			CheckBox checkBox = (CheckBox)sender;
			foreach (CheckBox current in this.settingBoxes)
			{
				bool flag = this.checkExclusive(checkBox.Text, current.Text);
				if (flag)
				{
					current.Checked = false;
					this.settings[int.Parse(current.Name)].enabled = current.Checked;
				}
			}
			this.settings[int.Parse(checkBox.Name)].enabled = checkBox.Checked;
		}

		private bool checkExclusive(string origin, string target)
		{
			bool flag = origin != target;
			bool result;
			if (flag)
			{
				origin = this.stripBrackets(origin);
				target = this.stripBrackets(target);
				bool flag2 = origin == target;
				if (flag2)
				{
					result = true;
					return result;
				}
			}
			result = false;
			return result;
		}

		private string stripBrackets(string input)
		{
			string result = "";
			bool flag = input.Contains('[');
			if (flag)
			{
				result = input.Substring(0, input.IndexOf('['));
			}
			bool flag2 = input.Contains('(');
			if (flag2)
			{
				result = input.Substring(0, input.IndexOf('('));
			}
			return result;
		}

		private bool checkSetting(string line)
		{
			while (string.IsNullOrWhiteSpace(line.Substring(0, 1)))
			{
				line = line.Substring(1, line.Length - 1);
			}
			bool flag = line.Substring(0, 2) == "//";
			return !flag;
		}

		private void saveBtn_Click(object sender, EventArgs e)
		{
			this.saveSettings();
			base.Close();
		}

		private void saveSettings()
		{
			try
			{
				string[] array = File.ReadAllLines(this.settingsPath);
				int num = 0;
				for (int i = 0; i < array.Length; i++)
				{
					bool flag = array[i].ToLower().Contains(this.settings[num].code.ToLower());
					if (flag)
					{
						bool enabled = this.settings[num].enabled;
						if (enabled)
						{
							array[i] = this.settings[num].code;
						}
						else
						{
							array[i] = "//" + this.settings[num].code;
						}
						bool flag2 = num < this.settings.Length - 1;
						if (flag2)
						{
							num++;
						}
					}
				}
				File.WriteAllLines(this.settingsPath, array);
			}
			catch (FileNotFoundException)
			{
				MessageBox.Show("Settings.ini could not be found, please download a new copy.");
			}
			catch (UnauthorizedAccessException)
			{
				MessageBox.Show("Could not edit settings.ini, please ensure the file is not read-only and you have permission to edit the file.");
			}
		}

		private void cancelBtn_Click(object sender, EventArgs e)
		{
			base.Close();
		}

		private void SettingsForm_SizeChanged(object sender, EventArgs e)
		{
			this.saveBtn.Location = new Point(base.Width - 184, base.Height - 74);
			this.cancelBtn.Location = new Point(base.Width - 103, base.Height - 74);
			this.saveRestartBtn.Location = new Point(base.Width - 323, base.Height - 74);
		}

		private void saveRestartBtn_Click(object sender, EventArgs e)
		{
			this.saveSettings();
			Process[] processes = Process.GetProcesses();
			for (int i = 0; i < processes.Length; i++)
			{
				Process process = processes[i];
				bool flag = process.ProcessName == "Steam";
				if (flag)
				{
					this.steamProcess = process;
					this.steamProcessPath = process.MainModule.FileName;
					process.Kill();
					this.restartTimer.Start();
					this.counter = 0;
				}
			}
			base.Close();
		}

		private void restartTimer_Tick(object sender, EventArgs e)
		{
			this.counter++;
			bool flag = this.counter > 15;
			if (flag)
			{
				MessageBox.Show("Steam could not be restarted.");
				this.restartTimer.Stop();
			}
			try
			{
				bool hasExited = this.steamProcess.HasExited;
				if (hasExited)
				{
					Process.Start(this.steamProcessPath);
					this.restartTimer.Stop();
				}
			}
			catch (Exception var_2_64)
			{
				Process.Start(this.steamProcessPath);
				this.restartTimer.Stop();
			}
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
			ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(SettingsForm));
			this.cancelBtn = new Button();
			this.saveBtn = new Button();
			this.label1 = new Label();
			this.saveRestartBtn = new Button();
			this.restartTimer = new Timer(this.components);
			base.SuspendLayout();
			this.cancelBtn.Location = new Point(247, 267);
			this.cancelBtn.Name = "cancelBtn";
			this.cancelBtn.Size = new Size(75, 23);
			this.cancelBtn.TabIndex = 2;
			this.cancelBtn.Text = "Cancel";
			this.cancelBtn.UseVisualStyleBackColor = true;
			this.cancelBtn.Click += new EventHandler(this.cancelBtn_Click);
			this.saveBtn.Location = new Point(166, 267);
			this.saveBtn.Name = "saveBtn";
			this.saveBtn.Size = new Size(75, 23);
			this.saveBtn.TabIndex = 1;
			this.saveBtn.Text = "Save";
			this.saveBtn.UseVisualStyleBackColor = true;
			this.saveBtn.Click += new EventHandler(this.saveBtn_Click);
			this.label1.AutoSize = true;
			this.label1.Location = new Point(298, 9);
			this.label1.Name = "label1";
			this.label1.Size = new Size(0, 13);
			this.label1.TabIndex = 0;
			this.saveRestartBtn.Location = new Point(27, 267);
			this.saveRestartBtn.Name = "saveRestartBtn";
			this.saveRestartBtn.Size = new Size(133, 23);
			this.saveRestartBtn.TabIndex = 3;
			this.saveRestartBtn.Text = "Save and restart Steam";
			this.saveRestartBtn.UseVisualStyleBackColor = true;
			this.saveRestartBtn.Click += new EventHandler(this.saveRestartBtn_Click);
			this.restartTimer.Interval = 1000;
			this.restartTimer.Tick += new EventHandler(this.restartTimer_Tick);
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			base.AutoScaleMode = AutoScaleMode.Font;
			base.ClientSize = new Size(332, 302);
			base.Controls.Add(this.saveRestartBtn);
			base.Controls.Add(this.label1);
			base.Controls.Add(this.saveBtn);
			base.Controls.Add(this.cancelBtn);
			base.FormBorderStyle = FormBorderStyle.FixedToolWindow;
			base.Icon = (Icon)componentResourceManager.GetObject("$this.Icon");
			base.MaximizeBox = false;
			base.Name = "SettingsForm";
			base.StartPosition = FormStartPosition.CenterParent;
			this.Text = "PixelVision² Settings";
			base.Load += new EventHandler(this.SettingsForm_Load);
			base.SizeChanged += new EventHandler(this.SettingsForm_SizeChanged);
			base.ResumeLayout(false);
			base.PerformLayout();
		}
	}
}
