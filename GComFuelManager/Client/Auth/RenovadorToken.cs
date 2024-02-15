using System;
using System.Timers;
using Timer = System.Timers.Timer;

namespace GComFuelManager.Client.Auth
{
	public class RenovadorToken : IDisposable
	{
        private readonly ILoginService loginService;

        public RenovadorToken(ILoginService loginService)
		{
            this.loginService = loginService;
        }

        Timer? timer;

        public void Iniciar()
        {
            timer = new Timer();
            //timer.Interval = 1000 * 60 * 50;
            timer.Interval = 1000 * 60 * 25;
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            loginService.ManejarRenovacionToken();
        }

        public void Dispose()
        {
            timer?.Dispose();
        }

        public void CheckLogin()
        {
            loginService.CheckLoginApp();
        }
    }
}

