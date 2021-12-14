using Bogus;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WebDataTableApp.Models;

namespace WebDataTableApp.Services
{
    public class DataService : IDataService
    {
        private readonly IMemoryCache _memoryCache;
        private const string DefaultErrorMessage = "Something is wrong, Please try after some time.";

        public DataService(IMemoryCache memoryCache)
        => _memoryCache = memoryCache;

        /// <summary> 
        ///NOTE: Filtering ordering and pagination on data should apply at database level in order to acheive good performance.
        /// </summary> 
        public (int recordsTotal, int recordsFiltered, IEnumerable<EmployeeViewModel> data) GetEmployeesViewModel(DataTableParameters dataTableParameters)
        {
            var employees = GetEmployees();

            employees = ApplyEmpFilterIfAny(dataTableParameters, employees);

            string orderPropertyName, dir = "asc";

            orderPropertyName = SetupNameAndDir(dataTableParameters, ref dir);

            employees = ApplyEmpOrder(employees, orderPropertyName, dir, Flags);

            var total = employees.Count();

            var paginationData = employees.Skip(dataTableParameters.Start).Take(dataTableParameters.Length);

            return (recordsTotal: total, recordsFiltered: total, data: paginationData);
        }


        private static IEnumerable<EmployeeViewModel> ApplyEmpFilterIfAny(DataTableParameters dataTableParameters, IEnumerable<EmployeeViewModel> employees)
        {
            if (dataTableParameters.Search != null && !string.IsNullOrWhiteSpace(dataTableParameters.Search.Value))
            {
                var searchKey = dataTableParameters.Search.Value;

                employees = employees.Where(q => q.Name.Contains(searchKey, StringComparison.InvariantCultureIgnoreCase) ||
                    q.Office.Contains(searchKey, StringComparison.InvariantCultureIgnoreCase) ||
                    q.Age.ToString().Contains(searchKey, StringComparison.InvariantCultureIgnoreCase) ||
                    q.Position.Contains(searchKey, StringComparison.InvariantCultureIgnoreCase));
            }

            return employees;
        }

        private static IEnumerable<EmployeeViewModel> ApplyEmpOrder(IEnumerable<EmployeeViewModel> employees, string orderPropertyName, string dir, BindingFlags bindingFlags)
        => dir.Equals("asc", StringComparison.InvariantCultureIgnoreCase) ?
                employees.OrderBy(o => o.GetType().GetProperty(orderPropertyName, bindingFlags).GetValue(o)) :
                employees.OrderByDescending(od => od.GetType().GetProperty(orderPropertyName, bindingFlags).GetValue(od));


        private IEnumerable<EmployeeViewModel> GetEmployees()
        {
            if (_memoryCache.TryGetValue(DataTableAppKeys.EmpDataKey, out IEnumerable<EmployeeViewModel> employees))
                return employees;

            var designations = new List<string> { "CTO", "CIO", "VP", "Chief Architect", "Software Architect", "Engineering Manager", "Director of Engineering", "Software Engineer" };
            int id = 1;
            var employee = new Faker<EmployeeViewModel>()
                .CustomInstantiator(ci => new EmployeeViewModel { Id = id++ })
                .RuleFor(r => r.Gender, f => f.PickRandom<EmployeeGender>())
                .RuleFor(r => r.Name, f => f.Name.FullName())
                .RuleFor(r => r.Office, f => f.Company.CompanyName())
                .RuleFor(r => r.Position, f => f.PickRandom(designations))
                .RuleFor(r => r.Age, f =>
                {
                    var dob = f.Person.DateOfBirth;
                    return dob.CompareTo(DateTime.Now) < 0 ?
                    DateTime.Now.Year - dob.Year : dob.Year - DateTime.Now.Year;
                });
            employees = new List<EmployeeViewModel>();
            for (int i = 0; i < 50; i++)
            {
                employees = employees.Append(employee.Generate());
            }
            _memoryCache.Set(DataTableAppKeys.EmpDataKey, employees);
            return employees;
        }

        private static string SetupNameAndDir(DataTableParameters dataTableParameters, ref string dir)
        {
            string orderPropertyName;
            if (dataTableParameters.Order != null && dataTableParameters.Order.Length > 0)
            {
                var order = dataTableParameters.Order[0];
                orderPropertyName = dataTableParameters.Columns[order.Column].Data;
                dir = order.Dir;
            }
            else
                orderPropertyName = dataTableParameters.Columns[0].Data;
            return orderPropertyName;
        }

        public string DeleteEmployee(int id)
        {
            string response = default;
            if (_memoryCache.TryGetValue(DataTableAppKeys.EmpDataKey, out IEnumerable<EmployeeViewModel> employees))
            {
                try
                {
                    var data = employees.ToList();
                    var employee = data.FirstOrDefault(q => q.Id == id);
                    if (employee != null)
                    {
                        data.Remove(employee);
                        _memoryCache.Set(DataTableAppKeys.EmpDataKey, data);
                    }
                    else
                        response = "Unable to find record in cache.";
                }
                catch (Exception ex)
                {
                    response = ex.Message;
                }
            }
            else
                response = DefaultErrorMessage;

            return response;
        }

        public string UpdateEmployee(EmployeeViewModel employeeViewModel)
        {
            string response = default;
            if (_memoryCache.TryGetValue(DataTableAppKeys.EmpDataKey, out IEnumerable<EmployeeViewModel> employees))
            {
                try
                {
                    var data = employees.ToList();
                    var employee = data.FirstOrDefault(q => q.Id == employeeViewModel.Id);
                    if (employee != null)
                    {
                        var index = employees.ToList().IndexOf(employee);
                        var updatedEmployee = new EmployeeViewModel
                        {
                            Id = employeeViewModel.Id,
                            Name = employeeViewModel.Name,
                            Gender = employee.Gender,
                            Age = employeeViewModel.Age,
                            Office = employeeViewModel.Office,
                            Position = employeeViewModel.Position
                        };
                        data.Remove(employee); data.Insert(index, updatedEmployee);
                        _memoryCache.Set(DataTableAppKeys.EmpDataKey, data);
                    }
                    else
                        response = "Unable to find record in cache.";
                }
                catch (Exception ex)
                {
                    response = ex.Message;
                }
            }
            else
                response = DefaultErrorMessage;

            return response;
        }

        public string AddEmployee(EmployeeViewModel employeeViewModel)
        {
            string response = default;
            if (_memoryCache.TryGetValue(DataTableAppKeys.EmpDataKey, out IEnumerable<EmployeeViewModel> employees))
            {
                try
                {
                    var data = employees.ToList();

                    var employee = new EmployeeViewModel
                    {
                        Id = employees.Max(q => q.Id) + 1,
                        Name = employeeViewModel.Name,
                        Gender = EmployeeGender.Other,
                        Age = employeeViewModel.Age,
                        Office = employeeViewModel.Office,
                        Position = employeeViewModel.Position
                    };
                    data.Add(employee);
                    _memoryCache.Set(DataTableAppKeys.EmpDataKey, data);
                }
                catch (Exception ex)
                {
                    response = ex.Message;
                }
            }
            else
                response = DefaultErrorMessage;

            return response;
        }

        public (int recordsTotal, int recordsFiltered, IEnumerable<CustomerViewModel> data) GetCustomersViewModel(DataTableParameters dataTableParameters)
        {
            var customers = GetCustomersViewModel();

            customers = ApplyCustFilterIfAny(dataTableParameters, customers);

            string orderPropertyName, dir = "asc";

            orderPropertyName = SetupNameAndDir(dataTableParameters, ref dir);

            customers = ApplyCustOrder(customers, orderPropertyName, dir, Flags);

            var total = customers.Count();

            var paginationData = customers.Skip(dataTableParameters.Start).Take(dataTableParameters.Length);

            return (recordsTotal: total, recordsFiltered: total, data: paginationData);
        }

        private IEnumerable<CustomerViewModel> ApplyCustFilterIfAny(DataTableParameters dataTableParameters, IEnumerable<CustomerViewModel> customers)
        {
            if (dataTableParameters.Search != null && !string.IsNullOrWhiteSpace(dataTableParameters.Search.Value))
            {
                var searchKey = dataTableParameters.Search.Value;

                customers = customers.Where(q => q.Name.Contains(searchKey, StringComparison.InvariantCultureIgnoreCase) ||
                    q.Email.Contains(searchKey, StringComparison.InvariantCultureIgnoreCase) ||
                    q.AccountNumber.ToString().Contains(searchKey, StringComparison.InvariantCultureIgnoreCase) ||
                    q.AccountName.Contains(searchKey, StringComparison.InvariantCultureIgnoreCase) ||
                    q.Country.Contains(searchKey, StringComparison.InvariantCultureIgnoreCase));
            }

            return customers;
        }

        private IEnumerable<CustomerViewModel> ApplyCustOrder(IEnumerable<CustomerViewModel> customers, string orderPropertyName, string dir, BindingFlags bindingFlags)
        => dir.Equals("asc", StringComparison.InvariantCultureIgnoreCase) ?
                customers.OrderBy(o => o.GetType().GetProperty(orderPropertyName, bindingFlags).GetValue(o)) :
                customers.OrderByDescending(od => od.GetType().GetProperty(orderPropertyName, bindingFlags).GetValue(od));

        private IEnumerable<CustomerViewModel> GetCustomersViewModel()
        {
            if (_memoryCache.TryGetValue(DataTableAppKeys.CustDataKey, out IEnumerable<CustomerViewModel> customers))
                return customers;

            int id = 1;

            var products = new Faker<ProductDetail>()
                .RuleFor(r => r.Name, f => f.Commerce.ProductName())
                .RuleFor(r => r.Description, f => f.Commerce.ProductDescription())
                .RuleFor(r => r.Department, f => f.Commerce.Department())
                .RuleFor(r => r.Price, f => f.Commerce.Price());

            var customer = new Faker<CustomerViewModel>()
                .CustomInstantiator(ci => new CustomerViewModel { Id = id++ })
                .RuleFor(r => r.Name, f => f.Name.FullName())
                .RuleFor(r => r.Email, f => f.Internet.Email())
                .RuleFor(r => r.AccountNumber, f => f.Finance.Account())
                .RuleFor(r => r.Country, f => f.Address.Country())
                .RuleFor(r => r.AccountName, f => f.Finance.AccountName())
                .RuleFor(r => r.Avatar, f => f.Internet.Avatar())
                .RuleFor(r => r.ProductDetails, f => products.GenerateBetween(1, 10));

            customers = new List<CustomerViewModel>();
            for (int i = 0; i < 50; i++)
            {
                customers = customers.Append(customer.Generate());
            }
            _memoryCache.Set(DataTableAppKeys.CustDataKey, customers);

            return customers;
        }

        private BindingFlags Flags
        {
            get { return BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance; }
        }
    }
}
