using Bunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncfusion.Blazor.Tests.Grids
{
    public class GridTestBase: TestComponentBase
    {
        public List<Order> Orders = new List<Order>();
        public List<Order> Data = new List<Order>();
        public List<Employee> Employees = new List<Employee>();
        public List<Order> GenerateOrderData()
        {
            return Orders = Enumerable.Range(1, 20).Select(x => new Order()
            {
                OrderID = 1000 + x,
                CustomerID = (new string[] { "ALFKI",
                "ANANTR", "ANTON", "BLONP", "BOLID" })[x % 5],
                Freight = 25.7 + x,
                PriceValue = 34.3 + x,
                OrderDate = (new DateTime[] { new DateTime(2010, 5, 1), new DateTime(2010, 5, 2), new DateTime(2010, 5, 3) })[new Random().Next(3)],
                OrderDateNullable = (new DateTime[] { new DateTime(2014, 5, 1), new DateTime(2014, 5, 2), new DateTime(2014, 5, 3) })[new Random().Next(3)],
                OrderTime = (new TimeOnly[] { new TimeOnly(10, 00, 00), new TimeOnly(10, 30, 00), new TimeOnly(11, 00, 00)})[new Random().Next(3)],
                Date = (new DateOnly[] { new DateOnly(2012, 5, 1), new DateOnly(2012, 5, 2), new DateOnly(2012, 5, 3) })[new Random().Next(3)],
                DateNullable = (new DateOnly[] { new DateOnly(2018, 5, 1), new DateOnly(2018, 5, 2), new DateOnly(2018, 5, 3) })[new Random().Next(3)],
                OrderDateOff =(new DateTime[] { new DateTime(2015, 4, 1), new DateTime(2015, 4, 2), new DateTime(2015, 4, 3) })[new Random().Next(3)],
                OrderDateOffNullable = (new DateTime[] { new DateTime(2020, 4, 1), new DateTime(2020, 4, 2), new DateTime(2020, 4, 3) })[new Random().Next(3)],
                EmployeeID = new Random().Next(1, 9),
                Quantity = (x % 5 + 1) * 100,
                TotalItemCount = 15L + x,
                TotalItemCountNullable = 12L + x,
                ShippingCost = x * 20.6f,
                ShippingCostNullable = x * 23.2f,
                TotalPrice = 25M + x,
                TotalPriceNullable = 23M + x,
                StockQuantity = (uint)(1+x),
                StockQuantityNullable = (uint?)(5+x),
                ContactNumber = (ulong)(1334567 + x),
                ContactNumberNullable = (ulong?)(1233567 + x),
                ShipCountryCode =(short)(1000 +x),
                ShipCountryCodeNullable = (short?)(2000 + x),
                SalesCode = (ushort)(1100+ x),
                SalesCodeNullable =(ushort?)(2200 + x),
                ProductID = (byte)(11 + x),
                ProductIDNullable = (byte?)(22 + x),
                SalesID = (sbyte)(6 + x),
                SalesIDNullabele = (sbyte?)(4 + x),
                Verified = (new bool?[] { true, false, null })[x % 3],
                Country = (new string[] { "USA", "UK", "INDIA", "CHINA", "RUSSIA" })[x % 5],
            }).ToList();
        }
        public List<Order> GenerateOrderNullData()
        {
            Orders.Add(new Order() { OrderID = 10001, CustomerID = "ALFKI", Freight = 25.7 * 2 });
            Orders.Add(new Order() { OrderID = 10002, CustomerID = "ANANTR", Freight = 26.7 * 2 });
            Orders.Add(new Order() { OrderID = 10003, CustomerID = "BLONP", Freight = 27.7 * 2 });
            Orders.Add(new Order() { OrderID = 10004, CustomerID = "ANTON", Freight = 28.7 * 2 });
            Orders.Add(new Order() { OrderID = 10005, CustomerID = "BOLID", Freight = 29.7 * 2 });
            Orders.Add(new Order() { OrderID = 10006, CustomerID = null, Freight = 41.3 * 2 });
            return Orders;
        }

        public List<Order> GenerateData() {

            for (var i = 1; i < 28; i++) {

                Data.Add(new Order() { OrderID = 1000 + i, CustomerID = "ALfki" + i, OrderDate = new DateTime(2010, 5, i), Freight = 25.62 + i  });
            }

            return Data;
        }

        public List<Employee> GenerateEmployeeData()
        {            
            Employees.Add(new Employee() { FirstName = "Nancy", LastName = "Davolio", HireDate = new DateTime(1992, 01, 05), Country = "USA", City = "Seattle", EmployeeID = 1, Title = "Sales Representative" });
            Employees.Add(new Employee() { FirstName = "Andrew", LastName = "Fuller", HireDate = new DateTime(1993, 03, 08), Country = "USA", City = "Tacoma", EmployeeID = 2, Title = "Vice President, Sales " });
            Employees.Add(new Employee() { FirstName = "Janet", LastName = "Leverling", HireDate = new DateTime(1992, 11, 04), Country = "UK", City = "Kirkland", EmployeeID = 3, Title = "Sales Representative" });
            Employees.Add(new Employee() { FirstName = "Steven", LastName = "Peacock", HireDate = new DateTime(1992, 05, 05), Country = "USA", City = "Redmond", EmployeeID = 4, Title = "Sales Representative" });
            Employees.Add(new Employee() { FirstName = "Margaret", LastName = "Buchanan", HireDate = new DateTime(1993, 07, 05), Country = "UK", City = "London", EmployeeID = 5, Title = "Sales Manager" });
            Employees.Add(new Employee() { FirstName = "Micheal", LastName = "Suyama", HireDate = new DateTime(1993, 08, 05), Country = "UK", City = "London", EmployeeID = 6, Title = "Sales Representative" });
            Employees.Add(new Employee() { FirstName = "Robert", LastName = "King", HireDate = new DateTime(1993, 09, 05), Country = "USA", City = "London", EmployeeID = 7, Title = "Sales Representative" });
            Employees.Add(new Employee() { FirstName = "Laura", LastName = "Callahan", HireDate = new DateTime(1993, 10, 05), Country = "UK", City = "Seattle", EmployeeID = 8, Title = "Inside Sales Coordinator" });
            Employees.Add(new Employee() { FirstName = "Anne", LastName = "Dodsworth", HireDate = new DateTime(1993, 11, 05), Country = "USA", City = "London", EmployeeID = 9, Title = "Sales Representative" });
            return Employees;
        }

        public List<Order> ForeignData = new List<Order>();

        public List<Order> GenerateForeignData()
        {
            return ForeignData = Enumerable.Range(1, 9).Select(x => new Order()
            {
                OrderID = 1000 + x,
                CustomerID = (new string[] { "ALFKI",
                "ANANTR", "ANTON", "BLONP", "BOLID" })[x % 5],
                Freight = 25.7 + x,
                OrderDate = new DateTime(2019, 01, 01),
                EmployeeID = x,
                Verified = (new bool?[] { true, false, null })[x % 3],
                NullableValue = (new int?[] { 1, 2, null })[x % 3],
            }).ToList();
        }

        public List<Order> GetVirtualData()
        {
            int Code = 1;
            for (var i = 0; i <= 100; i++)
            {
                Orders.Add(new Order() { OrderID = Code + i, CustomerID = "ALFKI" + i, OrderDate = new DateTime(2010, 11, 30), Freight = 25.62 + i, EmployeeID = i, Verified = true, PriceValue = 2.1, ShipCountry="USA", ShipCity = "Seattle", ShipAddress = "123 Main St", ShippedDate = new DateTime(2010, 12, 01) });
                Orders.Add(new Order() { OrderID = Code + i, CustomerID = "ANANTR" + i, OrderDate = new DateTime(2010, 12, 30), Freight = 25.62 + i, EmployeeID = i, Verified = false, PriceValue = 2, ShipCountry="UK", ShipCity = "Tacoma", ShipAddress = "456 Elm St", ShippedDate = new DateTime(2010, 12, 31) });
                Orders.Add(new Order() { OrderID = Code + i, CustomerID = "ANTON" + i, OrderDate = new DateTime(2010, 3, 4), Freight = 25.62 + i, EmployeeID = i, Verified = true, PriceValue = 3, ShipCountry = "France", ShipCity = "London", ShipAddress = "789 Oak St", ShippedDate = new DateTime(2010, 3, 5) });
                Orders.Add(new Order() { OrderID = Code + i, CustomerID = "BLONP" + i, OrderDate = new DateTime(2010, 2, 19), Freight = 25.62 + i, EmployeeID = i, Verified = false, PriceValue = 4, ShipCountry = "Canada", ShipCity = "Redmond", ShipAddress = "101 Pine St", ShippedDate = new DateTime(2010, 2, 20) });
                Orders.Add(new Order() { OrderID = Code + i, CustomerID = "BOLID" + i, OrderDate = new DateTime(2010, 1, 9), Freight = 25.62 + i, EmployeeID = i, Verified = false, PriceValue = 6, ShipCountry = "Brazil", ShipCity = "Kirkland", ShipAddress = "202 Maple St", ShippedDate = new DateTime(2010, 1, 10) });
            }
            return Orders;
        }

        public List<Order> GetAllRecords()
        {
            return Orders = Enumerable.Range(1, 50).Select(x => new Order()
            {
                OrderID = 1000 + x,
                CustomerID = (new string[] { "ALFKI", "ANANTR", "ANTON", "BLONP", "BOLID" })[new Random().Next(5)],
                Freight = 2.1 * x,
                OrderDate = DateTime.Now.AddDays(-x),
            }).ToList();
        }

        public List<OrderData> GenerateInfiniteData()
        {
            List<OrderData> data = new List<OrderData>();
            int count = 1000;

            for (int i = 0; i < 500; i++)
            {
                data.Add(new OrderData() { OrderID = count + 1, CustomerID = "Alfki", OrderDate = new DateTime(1995, 05, 15), Freight = 25.7 * 2 });
                data.Add(new OrderData() { OrderID = count + 2, CustomerID = "Anantr", OrderDate = new DateTime(1994, 04, 04), Freight = 26.7 * 2 });
                data.Add(new OrderData() { OrderID = count + 3, CustomerID = "Blonp", OrderDate = new DateTime(1993, 03, 10), Freight = 27.7 * 2 });
                data.Add(new OrderData() { OrderID = count + 4, CustomerID = "Anton", OrderDate = new DateTime(1992, 02, 14), Freight = 28.7 * 2 });
                data.Add(new OrderData() { OrderID = count + 5, CustomerID = "Bolid", OrderDate = new DateTime(1991, 01, 18), Freight = 29.7 * 2 });
                count += 5;
            }
            return data;
        }

        public List<Order> GenerateAggregateOrderData()
        {
            List<Order> Orders = new List<Order>();
            int[] orderIDs = { 10248, 10249, 10250, 10251, 10252, 10253, 10254, 10255, 10256 };
            int[] employeeIDs = { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            string[] customerIDs = { "ERNSH", "SUPRD", "WELLI", "HANAR", "WELLI", "HANAR", "QUEDE", "RICSU", "WELLI" };
            string[] countries = { "Austria", "Belgium", "Brazil", "France", "Germany", "Mexico", "Switzerland", "Austria", "Belgium" };
            DateTime[] dates =
            {
                new DateTime(1996, 07, 17),
                new DateTime(1996, 09, 07),
                new DateTime(1996, 07, 08),
                new DateTime(1996, 07, 10),
                new DateTime(1996, 10, 17),
                new DateTime(1996, 07, 19),
                new DateTime(1996, 07, 04),
                new DateTime(1996, 07, 08),
                new DateTime(1996, 07, 05)
            };
            double[] freight = { 140.51, 51.30, 65.83, 58.17, 13.97, 3.05, 32.38, 41.34, 11.61 };
            double[] price = { 1.1, 2.2, 3.3, 4.4, 9.9, 8.8, 1.1, 9.9, 9.8 };

            for (int i = 0; i < orderIDs.Length; i++)
            {
                Orders.Add(new Order()
                {
                    OrderID = orderIDs[i],
                    EmployeeID = employeeIDs[i],
                    CustomerID = customerIDs[i],
                    ShipCountry = countries[i],
                    OrderDate = dates[i],
                    Freight = freight[i],
                    PriceValue = price[i],
                    Verified = employeeIDs[i] == 1 || employeeIDs[i] == 2 || employeeIDs[i] == 3 || employeeIDs[i] == 4 ? true : false
                });
            }
            return Orders;
        }

        public List<Employee> GenerateFilteringEmployeeData()
        {
            List<Employee> Employees = new List<Employee>();

            int[] ids = { 123, 234, 345, 456 };
            string[] lastNames = { "Davolio", "Fuller", "Leverling", "Peacock" };
            string[] countries = { "USA", "UK", "USA", "Canada" };
            DateTime[] hireDates =
            {
                new DateTime(2023, 8, 1),
                new DateTime(2023, 8, 2),
                new DateTime(2023, 8, 3),
                new DateTime(2001, 10, 1)
            };

            // Optional mapping (since original data doesn't include these fields)
            string[] firstNames = { "Nancy", "Andrew", "Janet", "Steven" };
            string[] cities = { "Seattle", "Tacoma", "Kirkland", "Redmond" };
            string[] titles = { "Sales Rep", "VP Sales", "Sales Rep", "Sales Rep" };

            for (int i = 0; i < ids.Length; i++)
            {
                Employees.Add(new Employee()
                {
                    EmployeeID = ids[i],
                    FirstName = firstNames[i],
                    LastName = lastNames[i],
                    HireDate = hireDates[i],
                    Country = countries[i],
                    City = cities[i],
                    Title = titles[i]
                });
            }
            return Employees;
        }
    }
}
