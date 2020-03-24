using System;
using System.Threading.Tasks;
using Orions.Systems.Desi.Common.Services;

namespace Orions.Systems.CrossModules.Desi.Util
{
	public class BlazorPlatform: NativePlatform
	{
		private IInputHelper _inputHelper;

		public BlazorPlatform(IInputHelper inputHelper)
		{
			_inputHelper = inputHelper;
		}

        public override async Task<bool> ReadLineAsync(string caption, Func<bool?, string, string> callback)
        {
            var userInput = string.Empty;
            var dialogResult = true;
            try
            {
                userInput = await _inputHelper.ReadLineAsync(caption);
            }
            catch (Exception)
            {
                dialogResult = false;
            }

            callback?.Invoke(dialogResult, userInput);
            return true;
        }

        public override Task<bool> ShowConfirmation(string text)
        {
            return _inputHelper.ConfirmAsync(text);
        }
    }
}
