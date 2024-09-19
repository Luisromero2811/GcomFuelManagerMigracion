using System;
using System.ComponentModel.DataAnnotations.Schema;
using GComFuelManager.Shared.Filtro;
using GComFuelManager.Shared.Modelos;

namespace GComFuelManager.Shared.DTOs.CRM
{
	public class CRMUsuarioDTO : Parametros_Busqueda_Gen
	{
        public int Id { get; set; }
        public string Id_Asp { get; set; } = string.Empty;
        //Usuario ASP
        public string UserName { get; set; } = string.Empty;
        //Contraseña
        public string Password { get; set; } = string.Empty;
        //Divisiones
        public int? IDDivision { get; set; }
        //Roles
        public int? IDRolCRM { get; set; }
        public List<CRMRol> Roles { get; set; } = new List<CRMRol>();
        public List<int> RolesAsignados { get; set; } = new List<int>();
        //Vendedor
        public int? IDVendedor { get; set; }
        //Originador
        public int? IDOriginador { get; set; }

        public string NombreUsuario { get; set; } = string.Empty;
        public string Division { get; set; } = string.Empty;
        public string TipoUsuario { get; set; } = string.Empty;

        //Helpers//
        public bool Activo { get; set; } = true;
        //Vista de contraseña
        [NotMapped]
        public bool passwordView { get; set; } = true;
        //Bool para casilla de vendedor
        public bool IsVendedor { get; set; } = false;
        //Bool para casilla de comercial
        public bool IsComercial { get; set; } = false;

        public int pagina { get; set; } = 1;
        public int tamanopagina { get; set; } = 10;
    }
}

