using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GComFuelManager.Shared.Modelos
{
    public class CRMVendedorOriginador : IEquatable<CRMVendedorOriginador>
    {
        public int VendedorId { get; set; }
        public int OriginadorId { get; set; }

        public CRMVendedor? Vendedor { get; set; } = null!;
        public CRMOriginador? Originador { get; set; } = null!;

        public bool Equals(CRMVendedorOriginador? cRM)
        {
            if(cRM is null) { return false; }
            return this.VendedorId == cRM.VendedorId && this.OriginadorId == cRM.OriginadorId;
        }

        public override bool Equals(object? obj) => Equals(obj as CRMVendedorOriginador);
        public override int GetHashCode() => (VendedorId, OriginadorId).GetHashCode();
    }
}
