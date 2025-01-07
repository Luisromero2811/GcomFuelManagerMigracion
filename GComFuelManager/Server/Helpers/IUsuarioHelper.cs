using GComFuelManager.Server.Identity;

namespace GComFuelManager.Server.Helpers
{
    public interface IUsuarioHelper
    {
        public Task<short> GetTerminalIdAsync();
        public short GetTerminalId();
        public Task<string> GetUserId();
        public Task<IdentityUsuario> GetUsuario();
    }
}
