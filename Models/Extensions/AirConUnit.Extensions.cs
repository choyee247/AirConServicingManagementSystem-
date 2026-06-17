namespace AirConServicingManagementSystem.Models
{
    public partial class AirConUnit
    {
        public DateTime? NextServiceDate
        {
            get
            {
                if (!InstallationDate.HasValue || !NextServiceOption.HasValue)
                    return null;

                return InstallationDate.Value.AddMonths(NextServiceOption.Value);
            }
        }

        public bool IsNextServiceActive =>
            NextServiceDate.HasValue && NextServiceDate >= DateTime.Now;
    }
}