using Microsoft.JSInterop;
using System;
namespace GComFuelManager.Client.Helpers
{
	public static class IJSRuntimeExtensionMethods
	{
		public static ValueTask<object> SetItemLocalStorage(this IJSRuntime js, string key, string value) 
			=> js.InvokeAsync<object>("localStorage.setItem",key,value);

		public static ValueTask<string> RemoveItemLocalStorage(this IJSRuntime js, string key)
			=> js.InvokeAsync<string>("localStorage.removeItem", key);

		public static ValueTask<string> GetItemLocalStorage(this IJSRuntime js, string key)
			=> js.InvokeAsync<string>("localStorage.getItem", key);
	}
}

