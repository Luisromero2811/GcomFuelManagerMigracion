using GComFuelManager.Shared.DTOs;
using System;
namespace GComFuelManager.Client.Auth
{
	public interface ILoginService
	{
		Task Login(UserTokenDTO token);
		Task Logoute();
	}
}

