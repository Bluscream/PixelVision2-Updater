using System;
using System.CodeDom.Compiler;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Pixelvision_Updater.Properties
{
	[GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "11.0.0.0"), CompilerGenerated]
	internal sealed class Settings : ApplicationSettingsBase
	{
		private static Settings defaultInstance = (Settings)SettingsBase.Synchronized(new Settings());

		public static Settings Default
		{
			get
			{
				return Settings.defaultInstance;
			}
		}

		[DefaultSettingValue("False"), UserScopedSetting, DebuggerNonUserCode]
		public bool beta
		{
			get
			{
				return (bool)this["beta"];
			}
			set
			{
				this["beta"] = value;
			}
		}

		[DefaultSettingValue("False"), UserScopedSetting, DebuggerNonUserCode]
		public bool restart
		{
			get
			{
				return (bool)this["restart"];
			}
			set
			{
				this["restart"] = value;
			}
		}

		[DefaultSettingValue(""), UserScopedSetting, DebuggerNonUserCode]
		public string targetFolder
		{
			get
			{
				return (string)this["targetFolder"];
			}
			set
			{
				this["targetFolder"] = value;
			}
		}
	}
}
