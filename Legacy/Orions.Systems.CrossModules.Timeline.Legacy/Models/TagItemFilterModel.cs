using System;

using Orions.SDK.Utilities;

namespace Orions.Systems.CrossModules.Timeline.Models
{
	public class TagItemFilterModel : SuperFilterModel
	{
		public TagItemFilterModel(string id)
		{
			if (string.IsNullOrWhiteSpace(id))
				throw new ArgumentException(nameof(id));

			Id = id;
		}

		public string RealmId { get; set; }

		public string Id { get; set; }
	}
}
