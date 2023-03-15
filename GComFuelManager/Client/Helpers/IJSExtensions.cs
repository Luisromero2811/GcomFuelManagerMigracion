using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using System.Collections.Generic;
using System.Linq;

namespace GComFuelManager.Client.Helpers
{
	public static class IJSExtensions
	{
		public static ValueTask<object> GuardarComo(this IJSRuntime js, string nombreArchivo, byte[] archivo)
		{
			
			return js.InvokeAsync<object>("saveAsFile",
				nombreArchivo,
				Convert.ToBase64String(archivo));
		}
	}
}

