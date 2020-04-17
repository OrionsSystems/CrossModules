using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using System.Collections.Generic;
using System.Globalization;

namespace Orions.Systems.CrossModules.Components.Model
{

	public interface IFlowDesignData 
	{

		public string FlowName { get; set; }

		public string FlowId { get; set; }
	}


	public class FlowDesignData : IFlowDesignData
	{
		private static long version = 223;

		[JsonProperty("tabs")]
		public List<FlowTab> Tabs { get; set; }

		[JsonProperty("components")]
		public List<FlowDesignComponent> Components { get; set; }

		[JsonProperty("version")]
		public long Version { get; set; }

		[JsonProperty("variables")]
		public string Variables { get; set; }

		[JsonProperty("flowName")]
		public string FlowName { get; set; }

		[JsonProperty("flowId")]
		public string FlowId { get; set; }

		[JsonProperty("defComponents")]
		public List<FlowDesignNodeConfiguration> NodeConfigurations { get; set; }

		public FlowDesignData()
		{
			Tabs = new List<FlowTab>();
			Tabs.Add(new FlowTab());
			Components = new List<FlowDesignComponent>();
			Version = version;
			Variables = string.Empty;
			NodeConfigurations = new List<FlowDesignNodeConfiguration>();
		}

		//public static WorkflowDesignData FromJson(string json) => JsonConvert.DeserializeObject<WorkflowDesignData>(json, Converter.Settings);
	}

	public class FlowDesignComponent
	{
		[JsonProperty("component")]
		public string Name { get; set; }

		[JsonProperty("typeFull")]
		public string Type { get; set; }

		[JsonProperty("state")]
		public FlowState State { get; set; }

		[JsonProperty("group")]
		public string Group { get; set; }

		[JsonProperty("x")]
		public long X { get; set; }

		[JsonProperty("y")]
		public long Y { get; set; }

		[JsonProperty("tab")]
		public string Tab { get; set; }

		[JsonProperty("connections")]
		public List<FlowConnection> Connections { get; set; }

		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("isnew")]
		public bool IsNew { get; set; }

		public FlowDesignComponent()
		{
			Connections = new List<FlowConnection>();
		}
	}

	public class FlowConnection
	{
		[JsonProperty("index")]
		public string Index { get; set; }

		[JsonProperty("id")]
		public string Id { get; set; }
	}

	public class FlowState
	{
		[JsonProperty("text")]
		public string Text { get; set; }

		[JsonProperty("color")]
		public string Color { get; set; }
	}

	public class FlowTab
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
		public FlowTab()
		{
			Name = "Default";
			Id = "1516287215994";
			Icon = "fa-object-ungroup";
			Linker = "main";
			Index = 0;
		}
	}

	public class FlowDesignNodeStatus
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

	public class FlowDesignNodeConfiguration
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

	internal class FlowDesignConverter
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
