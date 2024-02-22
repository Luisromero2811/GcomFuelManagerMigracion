using System;
using System.Timers;
using Timer = System.Timers.Timer;

namespace GComFuelManager.Client.Auth
{
	public class RenovadorToken : IDisposable
	{
        private readonly ILoginService loginService;
        private Timer timer = new();

        public RenovadorToken(ILoginService loginService)
		{
            this.loginService = loginService;
        }

        public void Iniciar()
        {
            timer = new Timer
            {
                //timer.Interval = 1000 * 60 * 50;
                Interval = 1000 * 60 * 25
            };
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

