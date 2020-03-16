using Orions.Desi.Forms.Core.Services;
using Orions.Systems.Desi.Common.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orions.Systems.CrossModules.Desi.Debug.Infrastructure
{
	public class BlazorDependencyResolver : DependencyResolver, IDependencyResolver
	{
		public override IApiHelper GetApiHelper()
		{
			return null;
			throw new NotImplementedException();
		}

		public override IDialogService GetDialogService()
		{
			return null;
			throw new NotImplementedException();
		}

		public override IImageService GetImageService()
		{
			return null;
			throw new NotImplementedException();
		}

		public override IDispatcher GetDispatcher()
		{
			return new BlazorDispatcher();
		}
	}
}
