using System;

namespace AirConServicingManagementSystem.Models
{
    public partial class CustomerQrToken
    {
        public bool IsExpired => DateTime.Now > ExpiredAt;

        public bool IsValid => !IsUsed && !IsExpired;
    }
}