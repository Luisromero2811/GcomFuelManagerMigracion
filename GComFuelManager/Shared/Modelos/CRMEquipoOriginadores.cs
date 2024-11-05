using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace GComFuelManager.Shared.Modelos
{
	public class CRMEquipoOriginadores:IEquatable<CRMEquipoOriginadores>
	{
        public int EquipoId { get; set; }
        public int OriginadorId { get; set; }

        [NotMapped]
        public CRMOriginador? Originador { get; set; } = null!;
        [NotMapped]
        public CRMEquipo? Equipo { get; set; } = null!;

        public bool Equals(CRMEquipoOriginadores? other)
        {
            if (other is null) { return false; }
            return this.OriginadorId == other.OriginadorId && this.EquipoId == other.EquipoId;
        }

        public override bool Equals(object? obj) => Equals(obj as CRMEquipo);

        public override int GetHashCode() => (OriginadorId, EquipoId).GetHashCode();

    }
}

