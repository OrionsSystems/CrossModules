﻿using Orions.Infrastructure.HyperMedia.MapOverlay;
using System;
using System.Collections.Generic;
using System.Text;

namespace Orions.Systems.CrossModules.Components.Components.SVGMapEditor.JsModel
{
	public abstract class BaseOverlayEntryJsModel
	{
		public string Id { get; set; }
		public string Name { get; set; }

		public string Color { get; set; }

		public Dictionary<string, string> EventHandlerMappings { get; } = new Dictionary<string, string>();

		public abstract string EntryType { get; }

		protected static void MapBasePropertiesFromDomainModel(OverlayEntry domain, BaseOverlayEntryJsModel model)
		{
			model.Color = domain.Color;
			model.Id = domain.Id;
			model.Name = domain.Name;
		}

		protected static void MapBasePropertiesToDomainModel(OverlayEntry domain, BaseOverlayEntryJsModel zoneOverlayEntryJsModel)
		{
			if(zoneOverlayEntryJsModel.Id != null)
			{
				domain.Id = zoneOverlayEntryJsModel.Id;
			}
			domain.Name = zoneOverlayEntryJsModel.Name;
			domain.Color = zoneOverlayEntryJsModel.Color;
		}
	}
}
