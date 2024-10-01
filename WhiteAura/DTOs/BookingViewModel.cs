using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace WhiteAura.DTOs
{
    public class BookingViewModel
    {
        public Nullable<int> ServiceID { get; set; }

        public string ServiceName { get; set; }
        public decimal Price { get; set; }
        public string Place { get; set; }
        public int NumberOfGuests { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime? WeddingDate { get; set; }
        public TimeSpan? ReservedHours { get; set; }
        public bool IsPaid { get; set; }

    }
}
