using System;
using System.Configuration;
using System.Xml;

namespace PeerMessenger
{
	/// <summary>
	/// Summary description for ConfigurationManager.
	/// </summary>
	public sealed class ConfigurationManager
	{
		private static string _ConfigFile = "PeerMessenger.exe.config";
		private static string _UserName;
		private static string _LogFile;
		private static bool _DisablePeerMessengerSupport;

		static ConfigurationManager()
		{
			_UserName = _GetConfigValue("UserName");
			_DisablePeerMessengerSupport = false;
			string disablePeerMessengerSupport = _GetConfigValue("DisablePeerMessengerSupport");
			if(disablePeerMessengerSupport != null)
			{
				_DisablePeerMessengerSupport = bool.Parse(disablePeerMessengerSupport);
			}

			XmlDocument doc = new XmlDocument();
			doc.Load(_ConfigFile);
			XmlElement messageAppender = doc.SelectSingleNode("//configuration/log4net/appender[@name='MessageAppender']/param[@name='File']") as XmlElement;
			if(messageAppender != null)
			{
				_LogFile = messageAppender.Attributes["value"].Value;
			}
		}

		private ConfigurationManager()
		{
		}

		private static string _GetConfigValue(string key)
		{
			return ConfigurationSettings.AppSettings[key];
		}

		private static void _SetConfigValue(string configKey, object val)
		{
			XmlDocument doc = new XmlDocument();
			doc.Load(_ConfigFile);

			XmlNode n = doc.SelectSingleNode("//configuration/appSettings/add[@key='" + configKey + "']");
			if(n == null)
			{
				XmlElement appSettings = doc.SelectSingleNode("//configuration/appSettings") as XmlElement;
				XmlElement add = doc.CreateElement("add");						
				XmlAttribute key = doc.CreateAttribute("key");
				key.Value = configKey;

				XmlAttribute v = doc.CreateAttribute("value");

				add.Attributes.Append(key);
				add.Attributes.Append(v);
				appSettings.AppendChild(add);

				n = add as XmlNode;
			}

			n.Attributes["value"].Value = val.ToString();
			doc.Save(_ConfigFile);
		}

		public static void SetLogFile(string logFile)
		{
			XmlDocument doc = new XmlDocument();
			doc.Load(_ConfigFile);
			XmlElement messageAppender = doc.SelectSingleNode("//configuration/log4net/appender[@name='MessageAppender']/param[@name='File']") as XmlElement;
			if(messageAppender != null)
			{
				messageAppender.Attributes["value"].Value = logFile.Replace("\\", "\\\\");
			}
			doc.Save(_ConfigFile);
		}

		public static void SetUserName(string userName)
		{
			_SetConfigValue("UserName", userName);
		}

		public static void SetDisablePeerMessengerSupport(bool val)
		{
			_SetConfigValue("DisablePeerMessengerSupport", val.ToString());
		}

		public static string UserName
		{
			get
			{
				return _UserName;
			}
		}

		public static string LogFile
		{
			get
			{
				return _LogFile;
			}
		}

		public static bool DisablePeerMessengerSupport
		{
			get
			{
				return _DisablePeerMessengerSupport;
			}
		}
	}
}
