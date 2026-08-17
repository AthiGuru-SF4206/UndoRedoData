using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Syncfusion.Blazor.Tests.Grids
{
    public class Order
    {
        public int OrderField { get; set; } = 0;
        [Required]
        public int OrderID { get; set; }
        [Required]
        public string CustomerID { get; set; }
        public DateTime? OrderDate { get; set; }
        public DateTime OrderDateNullable { get; set; }
        public TimeOnly? OrderTime { get; set; }
        public DateOnly? Date { get; set; }
        public DateOnly DateNullable { get; set; }
        public TimeOnly TimeNonNullable { get; set; }
        public double? Freight { get; set; }
	    public double PriceValue { get; set; }
        public int EmployeeID { get; set; }
        public bool? Verified { get; set; }
         public int Quantity { get; set; }
        public long TotalItemCount { get; set; }
        public long? TotalItemCountNullable { get; set; }
        public float ShippingCost { get; set; }
        public float? ShippingCostNullable { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal? TotalPriceNullable { get; set; }
        public ulong ContactNumber { get; set; }
        public ulong? ContactNumberNullable { get; set; }
        public uint StockQuantity { get; set; }
        public uint? StockQuantityNullable { get; set; }
        public short ShipCountryCode { get; set; }
        public short? ShipCountryCodeNullable { get; set; }
        public ushort SalesCode { get; set; }
        public ushort? SalesCodeNullable { get; set; }
        public byte ProductID { get; set; }
        public string ProductName { get; set; }
        public byte? ProductIDNullable { get; set; }
        public sbyte  SalesID { get; set; }
        public sbyte? SalesIDNullabele { get; set; }
        public DateTimeOffset OrderDateOff { get; set; }
        public DateTimeOffset? OrderDateOffNullable { get; set; }
        public object Country { get; set; }
        public Guid MyGUIDNullable { get; set; } = Guid.NewGuid();
        public Guid? MyGUID { get; set; } = Guid.NewGuid();
        public string ShipCountry { get; set; }
        public string ShipCity { get; set; }
        public string ShipAddress { get; set; }
        public DateTime? ShippedDate { get; set; }
        public Employee Employees { get; set; }
        public Status Status { get; set; }
        public int? NullableValue { get; set; }
    }

    public class EmployeeData
    {
        public int? EmployeeID { get; set; }
        public string FirstName { get; set; }
    }

    public enum Status
    {
        Dispatched,
        Packed,
        YetToShip,
        Completed
    }

    public class OrderData
    {
        public int? OrderID { get; set; }
        public string CustomerID { get; set; }
        public DateTime? OrderDate { get; set; }
        public double? Freight { get; set; }
    }
}
