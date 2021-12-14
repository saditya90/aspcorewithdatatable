namespace WebDataTableApp.Models
{
    public class EmployeeViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Position { get; set; }
        public string Office { get; set; }
        public EmployeeGender? Gender { get; set; }
        public int Age { get; set; }
    }
}
