using System.Collections.Generic;
using WebDataTableApp.Models;

namespace WebDataTableApp.Services
{
    public interface IDataService
    {
        (int recordsTotal, int recordsFiltered, IEnumerable<EmployeeViewModel> data) GetEmployeesViewModel(DataTableParameters dataTableParameters);
        string DeleteEmployee(int id);
        string UpdateEmployee(EmployeeViewModel employeeViewModel);
        string AddEmployee(EmployeeViewModel employeeViewModel);
        (int recordsTotal, int recordsFiltered, IEnumerable<CustomerViewModel> data) GetCustomersViewModel(DataTableParameters dataTableParameters);
    }
}
