using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace AirConServicingManagementSystem.Models;

public partial class DBContext : DbContext
{
    public DBContext()
    {
    }

    public DBContext(DbContextOptions<DBContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AirConBrand> AirConBrands { get; set; }

    public virtual DbSet<AirConModel> AirConModels { get; set; }

    public virtual DbSet<AirConUnit> AirConUnits { get; set; }

    public virtual DbSet<Appointment> Appointments { get; set; }

    public virtual DbSet<AppointmentReassign> AppointmentReassigns { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<CustomerLocation> CustomerLocations { get; set; }

    public virtual DbSet<CustomerQrToken> CustomerQrTokens { get; set; }

    public virtual DbSet<MaintenanceSchedule> MaintenanceSchedules { get; set; }

    public virtual DbSet<MonthlyServiceReport> MonthlyServiceReports { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<ServicePhoto> ServicePhotos { get; set; }

    public virtual DbSet<ServiceRecord> ServiceRecords { get; set; }

    public virtual DbSet<ServiceReminder> ServiceReminders { get; set; }

    public virtual DbSet<ServiceRequest> ServiceRequests { get; set; }

    public virtual DbSet<ServiceTechnician> ServiceTechnicians { get; set; }

    public virtual DbSet<ServiceWarranty> ServiceWarranties { get; set; }

    public virtual DbSet<TbStateDivision> TbStateDivisions { get; set; }

    public virtual DbSet<TbTownship> TbTownships { get; set; }

    public virtual DbSet<Technician> Technicians { get; set; }

    public virtual DbSet<TechnicianBonuse> TechnicianBonuses { get; set; }

    public virtual DbSet<TechnicianSchedulePlan> TechnicianSchedulePlans { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Warranty> Warranties { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=203.81.89.218;Database=AirConServicingDB;User Id=internadmin;Password=intern@dmin123;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AirConBrand>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AirConBr__3214EC072286C9BB");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<AirConModel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AirConMo__3214EC0765AD51E4");

            entity.Property(e => e.IsDeleted).HasDefaultValue(false);

            entity.HasOne(d => d.Brand).WithMany(p => p.AirConModels)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__AirConMod__Brand__403A8C7D");
        });

        modelBuilder.Entity<AirConUnit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AirConUn__3214EC075ADF1A1A");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Brand).WithMany(p => p.AirConUnits)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__AirConUni__Brand__45F365D3");

            entity.HasOne(d => d.Customer).WithMany(p => p.AirConUnits)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__AirConUni__Custo__44FF419A");

            entity.HasOne(d => d.Model).WithMany(p => p.AirConUnits)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__AirConUni__Model__46E78A0C");
        });

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.AppointmentId).HasName("PK__Appointm__8ECDFCC2B7D35939");

            entity.Property(e => e.Status).HasDefaultValue("Pending");

            entity.HasOne(d => d.Customer).WithMany(p => p.Appointments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Appointments_Customers");

            entity.HasOne(d => d.Technician).WithMany(p => p.Appointments).HasConstraintName("FK_Appointments_Technicians");
        });

        modelBuilder.Entity<AppointmentReassign>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Appointm__3214EC07C4607E13");

            entity.Property(e => e.ChangedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Appointment).WithMany(p => p.AppointmentReassigns)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Appointme__Appoi__7E02B4CC");

            entity.HasOne(d => d.NewTechnician).WithMany(p => p.AppointmentReassignNewTechnicians)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Appointme__NewTe__7FEAFD3E");

            entity.HasOne(d => d.OldTechnician).WithMany(p => p.AppointmentReassignOldTechnicians).HasConstraintName("FK__Appointme__OldTe__7EF6D905");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Customer__3214EC077FC05282");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<CustomerLocation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Customer__3214EC07B153C8FC");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Customer).WithMany(p => p.CustomerLocations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CustomerL__Custo__68487DD7");

            entity.HasOne(d => d.StateDivisionPk).WithMany(p => p.CustomerLocations).HasConstraintName("FK_CustomerLocation_StateDivision");

            entity.HasOne(d => d.TownshipPk).WithMany(p => p.CustomerLocations).HasConstraintName("FK_CustomerLocation_Township");
        });

        modelBuilder.Entity<CustomerQrToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Customer__3214EC07593BF303");

            entity.HasOne(d => d.Customer).WithMany(p => p.CustomerQrTokens)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CustomerQrToken_Customer");
        });

        modelBuilder.Entity<MaintenanceSchedule>(entity =>
        {
            entity.HasKey(e => e.MaintenanceId).HasName("PK__Maintena__E60542D568160DCF");

            entity.Property(e => e.Status).HasDefaultValue("Pending");

            entity.HasOne(d => d.AirCon).WithMany(p => p.MaintenanceSchedules)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MaintenanceSchedules_AirConUnits");

            entity.HasOne(d => d.Technician).WithMany(p => p.MaintenanceSchedules).HasConstraintName("FK_MaintenanceSchedule_Technicians");
        });

        modelBuilder.Entity<MonthlyServiceReport>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MonthlyS__3214EC0717C3BD42");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__Payments__9B556A38E774B46B");

            entity.Property(e => e.PaidAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Service).WithOne(p => p.Payment)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Payments_ServiceRequests");

            entity.HasOne(d => d.Technician).WithMany(p => p.Payments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Payments_Technicians");
        });

        modelBuilder.Entity<ServicePhoto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ServiceP__3214EC078E197418");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);

            entity.HasOne(d => d.ServiceRecord).WithMany(p => p.ServicePhotos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ServicePh__Servi__6C190EBB");
        });

        modelBuilder.Entity<ServiceRecord>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ServiceR__3214EC07ECB24023");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.AirConUnit).WithMany(p => p.ServiceRecords)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ServiceRe__AirCo__5165187F");

            entity.HasOne(d => d.Customer).WithMany(p => p.ServiceRecords)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ServiceRe__Custo__5070F446");

            entity.HasOne(d => d.ServiceRequest).WithMany(p => p.ServiceRecords).HasConstraintName("FK_ServiceRecords_ServiceRequests");

            entity.HasOne(d => d.Technician).WithMany(p => p.ServiceRecords)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ServiceRecords_Technicians");
        });

        modelBuilder.Entity<ServiceReminder>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ServiceR__3214EC0798EC3F7D");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.SentStatus).HasDefaultValue(false);

            entity.HasOne(d => d.AirConUnit).WithMany(p => p.ServiceReminders)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ServiceRe__AirCo__619B8048");

            entity.HasOne(d => d.Customer).WithMany(p => p.ServiceReminders)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ServiceRe__Custo__60A75C0F");
        });

        modelBuilder.Entity<ServiceRequest>(entity =>
        {
            entity.HasKey(e => e.ServiceId).HasName("PK__ServiceR__C51BB00A318214CF");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.PaymentStatus).HasDefaultValue("Unpaid");
            entity.Property(e => e.RequestedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Status).HasDefaultValue("Pending");

            entity.HasOne(d => d.AirCon).WithMany(p => p.ServiceRequests)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_ServiceRequests_AirConUnits");

            entity.HasOne(d => d.Appointment).WithMany(p => p.ServiceRequests).HasConstraintName("FK_ServiceRequests_Appointments");

            entity.HasOne(d => d.Customer).WithMany(p => p.ServiceRequests)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ServiceRequests_Customers");

            entity.HasOne(d => d.Technician).WithMany(p => p.ServiceRequests)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_ServiceRequests_Technicians");
        });

        modelBuilder.Entity<ServiceTechnician>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ServiceT__3214EC07843FD1BF");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<ServiceWarranty>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ServiceW__3214EC07866C283A");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.ServiceRecord).WithMany(p => p.ServiceWarranties)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ServiceWa__Servi__571DF1D5");
        });

        modelBuilder.Entity<TbTownship>(entity =>
        {
            entity.Property(e => e.TownshipEn).IsFixedLength();

            entity.HasOne(d => d.StateDivisionPk).WithMany(p => p.TbTownships).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Technician>(entity =>
        {
            entity.HasKey(e => e.TechnicianId).HasName("PK__Technici__301F812179B040B3");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsAvailable).HasDefaultValue(true);
            entity.Property(e => e.JoinDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.TechnicianRole).HasDefaultValue("Technician");
        });

        modelBuilder.Entity<TechnicianBonuse>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Technici__3214EC077E748D2E");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Technician).WithMany(p => p.TechnicianBonuses)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Technicia__Techn__5BE2A6F2");
        });

        modelBuilder.Entity<TechnicianSchedulePlan>(entity =>
        {
            entity.HasKey(e => e.PlanId).HasName("PK__Technici__755C22B7CCEE96D0");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Customer).WithMany(p => p.TechnicianSchedulePlans).HasConstraintName("FK_TechnicianSchedulePlans_Customers");

            entity.HasOne(d => d.ServiceRequest).WithMany(p => p.TechnicianSchedulePlans).HasConstraintName("FK_Plan_ServiceRequest");

            entity.HasOne(d => d.Technician).WithMany(p => p.TechnicianSchedulePlans)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Plan_Technician");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC07AD8446F7");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);

            entity.HasOne(d => d.Technician).WithMany(p => p.Users).HasConstraintName("FK__Users__Technicia__70DDC3D8");
        });

        modelBuilder.Entity<Warranty>(entity =>
        {
            entity.HasKey(e => e.WarrantyId).HasName("PK__Warranti__2ED3181363C9D683");

            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.AirCon).WithOne(p => p.Warranty)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Warranties_AirConUnits");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
