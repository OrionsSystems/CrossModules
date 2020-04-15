using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using System.Collections.Generic;
using System.Globalization;

namespace Orions.Systems.CrossModules.Components.Model
{
	public class WorkflowDesignData
	{
		private static long version = 223;

		[JsonProperty("tabs")]
		public List<Tab> Tabs { get; set; }

		[JsonProperty("components")]
		public List<DesignComponent> Components { get; set; }

		[JsonProperty("version")]
		public long Version { get; set; }

		[JsonProperty("variables")]
		public string Variables { get; set; }

		[JsonProperty("workflowName")]
		public string WorkflowName { get; set; }

		[JsonProperty("workflowId")]
		public string WorkflowId { get; set; }

		[JsonProperty("defComponents")]
		public List<DesignNodeConfiguration> NodeConfigurations { get; set; }

		public WorkflowDesignData()
		{
			Tabs = new List<Tab>();
			Tabs.Add(new Tab());
			Components = new List<DesignComponent>();
			Version = version;
			Variables = string.Empty;
			NodeConfigurations = new List<DesignNodeConfiguration>();
		}

		//public static WorkflowDesignData FromJson(string json) => JsonConvert.DeserializeObject<WorkflowDesignData>(json, Converter.Settings);
	}

	public class DesignComponent
	{
		[JsonProperty("component")]
		public string Name { get; set; }

		[JsonProperty("typeFull")]
		public string Type { get; set; }

		[JsonProperty("state")]
		public State State { get; set; }

		[JsonProperty("group")]
		public string Group { get; set; }

		[JsonProperty("x")]
		public long X { get; set; }

		[JsonProperty("y")]
		public long Y { get; set; }

		[JsonProperty("tab")]
		public string Tab { get; set; }

		[JsonProperty("connections")]
		public List<Connection> Connections { get; set; }

		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("isnew")]
		public bool IsNew { get; set; }

		public DesignComponent()
		{
			Connections = new List<Connection>();
		}
	}

	public class Connection
	{
		[JsonProperty("index")]
		public string Index { get; set; }

		[JsonProperty("id")]
		public string Id { get; set; }
	}

	public class State
	{
		[JsonProperty("text")]
		public string Text { get; set; }

		[JsonProperty("color")]
		public string Color { get; set; }
	}

	public class Tab
	{
		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("icon")]
		public string Icon { get; set; }

		[JsonProperty("linker")]
		public string Linker { get; set; }

		[JsonProperty("index")]
		public long Index { get; set; }

		//TODO
		public Tab()
		{
			Name = "Default";
			Id = "1516287215994";
			Icon = "fa-object-ungroup";
			Linker = "main";
			Index = 0;
		}
	}

	public class WorkflowDesignNodeStatus
	{
		[JsonProperty("nodeId")]
		public string NodeId { get; set; }

		[JsonProperty("statusMessage")]
		public string StatusMessage { get; set; }

		[JsonProperty("systemStatusMessage")]
		public string SystemStatusMessage { get; set; }

		[JsonProperty("defaultStatusMessage")]
		public string DefaultStatusMessage { get; set; }

		[JsonProperty("loggerStatusMessage")]
		public string LoggerStatusMessage { get; set; }

		[JsonProperty("isActive")]
		public bool IsActive { get; set; }

		[JsonProperty("state")]
		public string State { get; set; }
	}

	public class DesignNodeConfiguration
	{
		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("typeFull")]
		public string TypeFull { get; set; }

		[JsonProperty("title")]
		public string Title { get; set; }

		[JsonProperty("group")]
		public string Group { get; set; }

		[JsonProperty("color")]
		public string Color { get; set; }

		[JsonProperty("input")]
		public int Input { get; set; }

		[JsonProperty("output")]
		public int Output { get; set; }

		[JsonProperty("author")]
		public string author { get; set; }

		[JsonProperty("icon")]
		public string Icon { get; set; }

		[JsonProperty("html")]
		public string Html { get; set; }

		[JsonProperty("readme")]
		public string Readme { get; set; }
	}

	internal class WorkflowDesignConverter
	{
		public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
		{
			MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
			DateParseHandling = DateParseHandling.None,
			Converters = {
					 new IsoDateTimeConverter()
					 {
						  DateTimeStyles = DateTimeStyles.AssumeUniversal,
					 },
				},
		};
	}
}
