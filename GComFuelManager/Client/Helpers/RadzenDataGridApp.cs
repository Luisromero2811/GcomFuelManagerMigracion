using Radzen.Blazor;

namespace GComFuelManager.Client.Helpers
{
    public class RadzenDataGridApp<TItem> : RadzenDataGrid<TItem>
    {
        public RadzenDataGridApp():base()
        {
            base.AndOperatorText = "Y";
            base.EqualsText = "Igual a";
            base.NotEqualsText = "No es igual a";
            base.LessThanText = "Menor qué";
            base.LessThanOrEqualsText = "Menor que o igual";
            base.GreaterThanText = "Mayor qué";
            base.GreaterThanOrEqualsText = "Mayor que o Igual";
            base.IsNullText = "Es nulo";
            base.IsNotNullText = "No es nulo";
            base.AndOperatorText = "Y";
            base.OrOperatorText = "O";
            base.ContainsText = "Contiene";
            base.DoesNotContainText = "No Contiene";
            base.StartsWithText = "Inicia Con";
            base.EndsWithText = "Termina Con";
            base.ClearFilterText = "Limpiar";
            base.ApplyFilterText = "Aplicar";
            base.FilterText = "Filtrar";
        }
    }
}
