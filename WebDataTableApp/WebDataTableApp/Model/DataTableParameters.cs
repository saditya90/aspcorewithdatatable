namespace WebDataTableApp.Models
{
    public class DataTableParameters
    {
        public int Draw { get; set; }
        public int Length { get; set; }
        public int Start { get; set; }
        public DataTableSearchParameters Search { get; set; }
        public DataTableOrderParameters[] Order { get; set; }
        public DataTableColumn[] Columns { get; set; }
    }
}
