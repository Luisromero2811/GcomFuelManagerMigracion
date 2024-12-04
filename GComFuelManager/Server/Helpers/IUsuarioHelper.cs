using GComFuelManager.Server.Identity;

namespace GComFuelManager.Server.Helpers
{
    public interface IUsuarioHelper
    {
        public Task<short> GetTerminalId();
        public Task<string> GetUserId();
        public Task<IdentityUsuario> GetUsuario();
    }
}
