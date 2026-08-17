using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Syncfusion.Blazor.Tests.Grids
{
    public class Employee
    {
        public int EmployeeID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Title { get; set; }
        public DateTime HireDate { get; set; }
        public string City { get; set; }
        public string Country { get; set; }

        public DateOnly Date { get; set; }
        public TimeOnly Time { get; set; }
        public DateTimeOffset DateTimeOffset { get; set; }
    }
}
