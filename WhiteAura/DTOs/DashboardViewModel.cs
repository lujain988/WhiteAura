using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WhiteAura.Models;

namespace WhiteAura.DTOs
{
    public class DashboardViewModel
    {
        public int TotalBookings { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<object> MonthlyBookings { get; set; }
        public List<object> MonthlyRevenue { get; set; }

    }
}