using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orions.Infrastructure.HyperMedia;
using Orions.Systems.CrossModules.Common;

namespace Orions.Systems.CrossModules.Desi
{
	public class Program
	{
		static AppHostHelper<Startup> _helper;
		public static CrossModuleInstanceHost CrossModuleInstanceHost => _helper?.CrossModuleInstanceHost;

		/// <summary>
		/// Main entry point for the application.
		/// </summary>
		public static void Main(string[] args)
		{
			_helper = new AppHostHelper<Startup>();

			_helper.StartWithArgs(args);
		}
	}
}
