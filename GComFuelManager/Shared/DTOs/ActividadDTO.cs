using System;
using GComFuelManager.Shared.Modelos;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;

namespace GComFuelManager.Shared.DTOs
{
    public class ActividadDTO
    {
        //EntityEntry, que proporciona acceso para realizar un seguimiento de los cambios dentro del contexto
        public ActividadDTO(EntityEntry entry)
        {
            Entry = entry;
        }
        public EntityEntry Entry { get; }
        public string UserId { get; set; }
        public string TableName { get; set; }
        public Dictionary<string, object> KeyValues { get; } = new Dictionary<string, object>();
        public Dictionary<string, object> OldValues { get; } = new Dictionary<string, object>();
        public Dictionary<string, object> NewValues { get; } = new Dictionary<string, object>();
        public int? AuditType { get; set; }
        public List<string> ChangedColumns { get; } = new List<string>();

        //convirtiendo AuditEntry a la clase Audit
        public ActividadRegistrada ToAudit()
        {
            var audit = new ActividadRegistrada();
            audit.UserId = UserId;
            audit.Type = AuditType;
            audit.TableName = TableName;
            audit.DateTime = DateTime.Now;//se puede usar UtcNow

            //serializamos los valores antiguos y nuevos para que se guarden en la base de datos como cadenas JSON
            audit.PrimaryKey = JsonConvert.SerializeObject(KeyValues);
            audit.OldValues = OldValues.Count == 0 ? null : JsonConvert.SerializeObject(OldValues);
            audit.NewValues = NewValues.Count == 0 ? null : JsonConvert.SerializeObject(NewValues);
            audit.AffectedColumns = ChangedColumns.Count == 0 ? null : JsonConvert.SerializeObject(ChangedColumns);
            return audit;
        }
    }
}

