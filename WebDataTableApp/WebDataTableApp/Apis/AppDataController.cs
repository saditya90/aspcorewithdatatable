using Microsoft.AspNetCore.Mvc;
using WebDataTableApp.Models;
using WebDataTableApp.Services;

namespace WebDataTableApp.Apis
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AppDataController : ControllerBase
    {
        private readonly IDataService _dataService;

        public AppDataController(IDataService dataService)
        {
            _dataService = dataService;
        }

        [HttpPost]
        [ActionName("employess")]
        public IActionResult Employees([FromBody] DataTableParameters dataTableParameters)
        {
            var (recordsTotal, recordsFiltered, data) = _dataService.GetEmployeesViewModel(dataTableParameters);
            
            return Ok(new
            {
                draw = dataTableParameters.Draw,
                recordsTotal,
                recordsFiltered,
                data
            });
        }

        [HttpPost]
        [ActionName("addemp")]
        public IActionResult AddEmployee([FromBody] EmployeeViewModel employeeViewModel)
        {
            return Ok(new ApiResponse
            {
                Message = _dataService.AddEmployee(employeeViewModel)
            });
        }

        [HttpDelete]
        [ActionName("deleteemp"), Route("{id:int}")]
        public IActionResult DeleteEmployee(int id)
        {
            return Ok(new ApiResponse
            {
                Message = _dataService.DeleteEmployee(id)
            });
        }

        [HttpPut]
        [ActionName("updateemp")]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateEmployee([FromBody] EmployeeViewModel employeeViewModel)
        {
            return Ok(new ApiResponse
            {
                Message = _dataService.UpdateEmployee(employeeViewModel)
            });
        }

        [HttpPost]
        [ActionName("customers")]
        public IActionResult Customers([FromBody] DataTableParameters dataTableParameters)
        {
            var (recordsTotal, recordsFiltered, data) = _dataService.GetCustomersViewModel(dataTableParameters);

            return Ok(new
            {
                draw = dataTableParameters.Draw,
                recordsTotal,
                recordsFiltered,
                data
            });
        }
    }

    public class ApiResponse
    {
        public string Message { get; set; }
    } 
}
