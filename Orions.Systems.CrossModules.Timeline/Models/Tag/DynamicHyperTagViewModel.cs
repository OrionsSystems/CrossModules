using Newtonsoft.Json;
using Orions.Common;
using Orions.Infrastructure.HyperMedia;
using Orions.Infrastructure.HyperSemantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Text.RegularExpressions;
using Orions.SDK.Utilities;

namespace Orions.Systems.CrossModules.Timeline.Models
{
	/// <summary>
	///  High level array elements:
	///   ScreenCoordinates
	///   Title
	///   TaggingTitle
	///   CreatedAtUTC
	///   Tag
	///     CreatedAt
	///     Elements (not key value, only int array of values)
	///       ReferenceId
	///       HyperId
	///         BlockType
	///         StoreId
	///         AssetId
	///           Guid
	///           Domain
	///         DomainId
	///         TrackId
	///           TrackKey
	///           TrackIndex
	///         FragmentId
	///         SliceId
	///         ...
	///       SemanticQuery
	///       TimeType
	///       TaskId
	///       UserId
	///       ExploitationDuration
	///     Id
	///   StreamPosition
	/// </summary>
	public class DynamicHyperTagViewModel : HyperStoreModel
	{
		[JsonIgnore]
		public dynamic DynamicData { get; set; }

		[JsonIgnore]
		public HyperTagExportView TagView { get; set; }

		public string ToSimpleJSON(string indent)
		{
			string initPath = "";
			StringBuilder ans = new StringBuilder();
			ans.Append(indent + "{ \"InitPath\":\"REPLACEINITPATHHERE\", ");
			ans.AppendLine(indent + "\"StreamPosition\":\"" + StreamPosition + "\", ");
			string preindent = indent;
			indent += "  ";
			if (FinishedPaths != null)
			{
				ans.AppendLine(indent + "\"Paths\":[");
				int pathcount = 0;
				foreach (var path in FinishedPaths)
				{

					if (pathcount > 0) ans.AppendLine(",");
					pathcount++;

					var iscomplete = SafeGetElement(path, "IsComplete");
					var steps = SafeGetElement(path, "Steps");

					if (steps != null)
					{
						int idx = 0;
						ans.Append(indent + " { \"Path\":[");
						foreach (var step in steps)
						{
							var nodename = SafeGetElement(step, "OptionalTargetNodeName").ToString().Trim();
							if (idx > 0) ans.Append(", ");
							ans.Append("\"" + nodename + "\"");
							idx++;

							if (pathcount == 1)
							{
								if (initPath.Length > 0) initPath += "->";
								initPath += nodename;
							}

							var actions = SafeGetElement(step, "Actions");
							if (actions != null && actions.Count > 0)
							{
								foreach (var action in actions)
								{
									//var actionnodename = action["NodeName"];
									//var actionresult = action["Result"];
									//if (actionnodename != null && actionresult != null)
									//{
									//	try
									//	{
									//		ans.Append(", \"" + actionnodename.ToString() + ":" + actionnodename.Value + "\"");
									//	}
									//	catch (Exception ex)
									//	{

									//	}
									//}
									//else if (actionresult != null)
									//{
									//	var ischecked = actionresult["IsChecked"];
									//	ans.Append(", \"" + ischecked.ToString() + "\"");
									//}
								}
							}
						}
						ans.Append("] }");
					}
				}

				ans.AppendLine();
				if ((Children != null && Children.Count > 0) || BasicMLTags != null)
				{
					ans.AppendLine(indent + "], ");
				}
				else
				{
					ans.AppendLine(indent + "]");
				}
			}

			if (Children != null && Children.Count > 0)
			{
				ans.AppendLine(indent + "\"Children\":[");
				int childcount = 0;
				foreach (DynamicHyperTagViewModel child in Children)
				{
					var childContent = child.ToSimpleJSON(indent + " ");
					childcount++;
					if (childcount > 1)
					{
						ans.AppendLine(", ");
					}
					ans.Append(childContent);
				}
				ans.AppendLine();
				if (BasicMLTags != null)
				{
					ans.AppendLine(indent + "], ");
				}
				else
				{
					ans.AppendLine(indent + "]");
				}
			}

			if (BasicMLTags != null)
			{
				ans.AppendLine(indent + "\"BasicMLTags\":[");
				int labelcount = 0;
				foreach (string labeltext in BasicMLTags.Labels)
				{
					if (labelcount > 0) ans.AppendLine(", ");

					string jsonTagArray = "";
					string[] tokens = labeltext.Split(',');
					bool firstToken = true;
					foreach (string token in tokens)
					{
						if (firstToken == false) jsonTagArray += ", ";
						jsonTagArray += "\"" + token.Trim() + "\"";
						firstToken = false;
					}

					ans.AppendLine(indent + " {");

					ans.AppendLine(indent + "   \"TagType\":\"TextConfidenceTag\", ");
					ans.AppendLine(indent + "   \"TagArray\":[" + jsonTagArray + "], ");

					string ifmore = "\" ";
					if (!string.IsNullOrEmpty(ImageGenerated))
						ifmore = "\", ";

					ans.AppendLine(indent + "   \"Confidence\":\"" + BasicMLTags.Confidences[labelcount] + ifmore);
					if (!string.IsNullOrEmpty(ImageGenerated))
					{
						string fixpath = ImageGenerated.Replace(@"\", @"/").Replace("\"", "\\\"");

						ans.AppendLine(indent + "   \"Thumbnail\":\"" + fixpath + "\"");
					}

					ans.Append(indent + " }");
					labelcount++;
				}
				ans.AppendLine();
				ans.AppendLine(indent + "]");

				if (string.IsNullOrEmpty(initPath)) initPath = "OrionsAI";
			}

			ans.Append(preindent + "}");
			return (ans.ToString().Replace("REPLACEINITPATHHERE", initPath));
		}

		public dynamic Title { get { return (SafeGetElement(DynamicData, "Title")); } }
		public dynamic TaggingTitle { get { return (SafeGetElement(DynamicData, "TaggingTitle")); } }
		public dynamic MissionName { get { return (SafeGetElement(DynamicData, "MissionName")); } }
		public dynamic MissionId { get { return (SafeGetElement(DynamicData, "MissionId")); } }
		public DateTime RealWorldContentTimeUTC
		{
			get
			{
				DateTime ans = DateTime.MaxValue;
				var timeObj = SafeGetElement(DynamicData, "RealWorldContentTimeUTC");
				if (timeObj != null && DateTime.TryParse(timeObj.ToString(), out ans)) return (ans);
				return (DateTime.MaxValue);
			}
		}

		public string ServerHost { get; set; }
		public int ServerPort { get; set; }
		public string HlsServerHost { get; set; }

		[JsonIgnore]
		public dynamic Tag { get { return (SafeGetElement(DynamicData, "Tag")); } }

		[JsonIgnore]
		public HyperTag HyperTag { get; set; }

		public dynamic TagId { get { return (SafeGetElement(Tag, "Id")); } }
		[JsonIgnore]
		public dynamic Elements
		{
			get
			{
				return SafeGetElement(Tag, "Elements");
			}
		}

		public List<string> ElementLabels
		{
			get
			{
				var el = TagView.TagInstance?.Elements;
				if (el != null && el.Any()) {
					return el.Select(it => it.GetType().Name).ToList();
				}

				return new List<string>();
			}
		}

		public HyperTagBasicML BasicMLTags { get { return (HyperTag?.GetElement<HyperTagBasicML>()); } }
		public dynamic HyperId { get { return (SafeGetArrayElement(Elements, "HyperId")); } }
		[JsonIgnore]
		public dynamic Result { get { return (SafeGetArrayElement(Elements, "Result")); } }
		public dynamic FinishedPaths { get { return (SafeGetElement(Result, "FinishedPaths")); } }
		[JsonIgnore]
		public dynamic FragmentId { get { return (SafeGetElement(HyperId, "FragmentId")); } }
		[JsonIgnore]
		public string FragmentIndex { get { return (SafeGetElement(FragmentId, "Index")); } }
		public string StreamPosition { get { return (SafeGetElement(DynamicData, "StreamPosition")); } }
		public string WorkflowInstanceId { get { return SafeGetElement(Elements, "WorkflowInstanceId"); } }


		[JsonIgnore]
		public string ImageGenerated { get; set; }

		public string AssetGuid
		{
			get
			{
				var assetid = SafeGetElement(HyperId, "AssetId"); if (assetid == null) return (string.Empty);
				return (assetid["Guid"]);
			}
		}

		public List<DynamicHyperTagViewModel> Children = new List<DynamicHyperTagViewModel>();

		public static string ParseIndexFromId(string TagId, out string outIndex)
		{
			outIndex = "0";
			string[] components = TagId.Split('-', '_');
			foreach (string ele in components)
			{
				if (ele.Length <= 3) outIndex = ele;
			}
			return (outIndex);
		}


		public string BaseTagId
		{
			get
			{
				string id = CleanId((string)SafeGetElement(Tag, "Id"));
				return (id);
			}
		}

		public string BaseTagIdIndex
		{
			get
			{
				string id = CleanId((string)SafeGetElement(Tag, "Id"));
				string ans;
				ParseIndexFromId(id, out ans);
				return (ans);
			}
		}

		public string ParentTagId
		{
			get
			{
				var fullId = SafeGetArrayElement(Elements, "ReferenceId");
				if (fullId != null)
				{
					var cleanId = CleanId(fullId.ToString());
					return (cleanId);
				}
				return (string.Empty);
			}
		}

		public string CleanId(string URI)
		{
			if (URI == null) return ("na");
			if (URI.Contains("/"))
			{
				int x = URI.LastIndexOf("/");
				return (URI.Substring(x + 1));
			}
			return (URI);
		}


		// WARNING: This is connected to StreamPosition for the moment, but we need to fix this eventually!! (Multi angle stuff, etc.)
		public TimeSpan WallClockTime
		{
			get
			{
				var pos = SafeGetElement(DynamicData, "StreamPosition");
				if (pos == null) return (new TimeSpan(0));
				return (pos);
			}
		}

		public string TimeLabel
		{
			get
			{
				return string.Format("{0:hh\\:mm\\:ss\\.ff}", WallClockTime);
			}
		}

		public List<string> TagPaths
		{
			get
			{
				var result =  new List<string>();
				if (TaggingTitle == null) return result;

				string data = TaggingTitle.ToString();
				if (!string.IsNullOrEmpty(data)) {
					data = Regex.Replace(data, @"^\s*$\n|\r", string.Empty, RegexOptions.Multiline).TrimEnd();
					var items = data.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
					result.AddRange(items);
				}
				return result;
			}
		}

		public dynamic SafeGetElement(dynamic Object, string elementName)
		{

			if (Object == null) return (null);
			var ans = Object.SelectToken(elementName);
			if (ans == null)
			{
				foreach (var element in Object)
				{
					var value = element.SelectToken(elementName);
					if (value != null) return value;
				}
			}

			if (ans == null) return null;

			if (ans.SelectToken("$values") != null) return (ans.SelectToken("$values"));

			return (ans);
		}

		/// <summary>
		/// Use this method when tags come from the cloud, because they don't contain type infomation
		/// </summary>
		/// <returns></returns>
		public List<HyperTagElement> SafeGetElementsFromJson()
		{
			var type = typeof(HyperTagElement);

			var list = ReflectionHelper.Instance.GetTypeChildrenTypes(type, type.Assembly);

			var elements = SafeGetElement(Tag, "Elements");

			var result = new List<HyperTagElement>();

			foreach (var typeElement in list)
			{
				foreach (var element in elements)
				{
					var props = typeElement
						.GetProperties()//BindingFlags.Public)
						.Where(it => it.GetCustomAttribute(typeof(DocumentDescriptorAttribute)) != null).ToList();

					var compatible = props.Any();

					foreach (var prop in props)
					{
						if (element.SelectToken(prop.Name) == null)
						{
							compatible = false;
							break;
						}
					}

					if (compatible)
					{
						try
						{
							var jsonElement = JsonConvert.SerializeObject(element);

							var elementInstance = Activator.CreateInstance(typeElement);

							JsonHelper.Populate(jsonElement, elementInstance);

							result.Add(elementInstance as HyperTagElement);
						}
						catch
						{
							// ignore
						}
					}
				}
			}

			return result;
		}

		public HyperTagMission GetTagMission()
		{
			var result = HyperTag.Elements.FirstOrDefault(it => it.GetType() == typeof(HyperTagMission));
			return result as HyperTagMission;
		}

		public dynamic SafeGetArrayElement(dynamic Object, string elementName)
		{
			if (Object == null) return (null);
			try
			{
				var values = Object;
				//                if (values["$values"] != null) values = values["$values"];
				foreach (var ele in values)
				{
					try
					{
						if (ele[elementName] != null) return (ele[elementName]);
					}
					catch { }
				}
				return (null);
			}
			catch
			{
				return (null);
			}
		}

		public List<dynamic> SafeGetArrayElements(dynamic Object, string elementName)
		{
			List<dynamic> ans = new List<dynamic>();
			if (Object == null) return (null);
			try
			{
				foreach (var ele in Object)
				{
					try
					{
						if (ele[elementName] != null) ans.Add(ele[elementName]);
					}
					catch { }
				}
			}
			catch
			{
				return (null);
			}
			return (ans);
		}
	}
}
