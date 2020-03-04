using Orions.Common;
using Orions.Infrastructure.HyperMedia;
using Orions.Node.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Orions.Systems.CrossModules
{
	public interface ITagReviewContext
	{
		int DashApiPort { get; set; }

		IHyperArgsSink HyperStore { get; set; }

		bool ExtractMode { get; set; }

		string FabricServiceId { get; set; }

		bool ShowFragmentAndSlice { get; set; }

		ViewModelProperty<List<HyperTag>> HyperTags { get; }

		int ColumnsNumber { get; set; }

		int InitialRowsNumber { get; set; }
	}
}
