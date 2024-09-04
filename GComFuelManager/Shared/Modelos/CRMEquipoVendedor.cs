namespace GComFuelManager.Shared.Modelos
{
    public class CRMEquipoVendedor:IEquatable<CRMEquipoVendedor>
    {
        public int EquipoId { get; set; }
        public int VendedorId { get; set; }

        public CRMVendedor? Vendedor { get; set; } = null!;
        public CRMEquipo? Equipo { get; set; } = null!;

        public bool Equals(CRMEquipoVendedor? other)
        {
            if(other is null) { return false; }
            return this.VendedorId == other.VendedorId && this.EquipoId == other.EquipoId;
        }

        public override bool Equals(object? obj) => Equals(obj as CRMEquipo);

        public override int GetHashCode() => (VendedorId, EquipoId).GetHashCode();
    }
}
