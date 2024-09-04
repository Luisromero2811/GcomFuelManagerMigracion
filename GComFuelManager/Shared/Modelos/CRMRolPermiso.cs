namespace GComFuelManager.Shared.Modelos
{
    public class CRMRolPermiso : IEquatable<CRMRolPermiso>
    {
        public int RolId { get; set; }
        public string PermisoId { get; set; } = string.Empty;

        public bool Equals(CRMRolPermiso? other)
        {
            if (other is null) return false;
            return this.RolId == other.RolId && this.PermisoId == other.PermisoId;
        }

        public override bool Equals(object? obj) => Equals(obj as CRMRolPermiso);

        public override int GetHashCode() => (RolId, PermisoId).GetHashCode();
    }
}
