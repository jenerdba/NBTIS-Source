using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace NBTIS.Data.Models;

public partial class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ActivityLog> ActivityLogs { get; set; }

    public virtual DbSet<Announcement> Announcements { get; set; }

    public virtual DbSet<BridgeElement> BridgeElements { get; set; }

    public virtual DbSet<BridgeFeature> BridgeFeatures { get; set; }

    public virtual DbSet<BridgeInspection> BridgeInspections { get; set; }

    public virtual DbSet<BridgeNoChange> BridgeNoChanges { get; set; }

    public virtual DbSet<BridgePostingEvaluation> BridgePostingEvaluations { get; set; }

    public virtual DbSet<BridgePostingStatus> BridgePostingStatuses { get; set; }

    public virtual DbSet<BridgePrimary> BridgePrimaries { get; set; }

    public virtual DbSet<BridgeRoute> BridgeRoutes { get; set; }

    public virtual DbSet<BridgeSpanSet> BridgeSpanSets { get; set; }

    public virtual DbSet<BridgeSubstructureSet> BridgeSubstructureSets { get; set; }

    public virtual DbSet<BridgeWork> BridgeWorks { get; set; }

    public virtual DbSet<Email> Emails { get; set; }

    public virtual DbSet<LookupValue> LookupValues { get; set; }

    public virtual DbSet<Lookup_County> Lookup_Counties { get; set; }

    public virtual DbSet<Lookup_DataItem> Lookup_DataItems { get; set; }

    public virtual DbSet<Lookup_Element> Lookup_Elements { get; set; }

    public virtual DbSet<Lookup_ErrorMessage> Lookup_ErrorMessages { get; set; }

    public virtual DbSet<Lookup_HPMSRoute> Lookup_HPMSRoutes { get; set; }

    public virtual DbSet<Lookup_State> Lookup_States { get; set; }

    public virtual DbSet<Lookup_Status> Lookup_Statuses { get; set; }

    public virtual DbSet<Report> Reports { get; set; }

    public virtual DbSet<Stage_BridgeElement> Stage_BridgeElements { get; set; }

    public virtual DbSet<Stage_BridgeFeature> Stage_BridgeFeatures { get; set; }

    public virtual DbSet<Stage_BridgeInspection> Stage_BridgeInspections { get; set; }

    public virtual DbSet<Stage_BridgePostingEvaluation> Stage_BridgePostingEvaluations { get; set; }

    public virtual DbSet<Stage_BridgePostingStatus> Stage_BridgePostingStatuses { get; set; }

    public virtual DbSet<Stage_BridgePrimary> Stage_BridgePrimaries { get; set; }

    public virtual DbSet<Stage_BridgeRoute> Stage_BridgeRoutes { get; set; }

    public virtual DbSet<Stage_BridgeSpanSet> Stage_BridgeSpanSets { get; set; }

    public virtual DbSet<Stage_BridgeSubstructureSet> Stage_BridgeSubstructureSets { get; set; }

    public virtual DbSet<Stage_BridgeWork> Stage_BridgeWorks { get; set; }

    public virtual DbSet<SubmittalComment> SubmittalComments { get; set; }

    public virtual DbSet<SubmittalCount> SubmittalCounts { get; set; }

    public virtual DbSet<SubmittalError> SubmittalErrors { get; set; }

    public virtual DbSet<SubmittalFile> SubmittalFiles { get; set; }

    public virtual DbSet<SubmittalLog> SubmittalLogs { get; set; }

    public virtual DbSet<TriggerChangeLog> TriggerChangeLogs { get; set; }

    public virtual DbSet<VSubmittalLog> VSubmittalLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ActivityLog>(entity =>
        {
            entity.HasKey(e => e.ActivityId);

            entity.ToTable("ActivityLogs", "NBI");

            entity.Property(e => e.ActivityDate).HasColumnType("datetime");
            entity.Property(e => e.EventDescription)
                .HasMaxLength(1000)
                .IsUnicode(false);
            entity.Property(e => e.IPAddress)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.LoginId)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.OfficeCode)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.ScreenName)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Announcement>(entity =>
        {
            entity.HasKey(e => e.AnnouncementId).HasName("PK_Announcement");

            entity.ToTable("Announcements", "NBI");

            entity.Property(e => e.AnnouncementDate).HasColumnType("datetime");
            entity.Property(e => e.EmailingTime)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.LoginId)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<BridgeElement>(entity =>
        {
            entity.HasKey(e => new { e.StateCode_BL01, e.BridgeNo_BID01, e.SubmittedBy, e.ElementNo_BE01, e.ElementParentNo_BE02 });

            entity
                .ToTable("BridgeElements", "NBI")
                .ToTable(tb => tb.IsTemporal(ttb =>
                    {
                        ttb.UseHistoryTable("BridgeElements_History", "NBI");
                        ttb
                            .HasPeriodStart("SysStartTime")
                            .HasColumnName("SysStartTime");
                        ttb
                            .HasPeriodEnd("SysEndTime")
                            .HasColumnName("SysEndTime");
                    }));

            entity.Property(e => e.BridgeNo_BID01)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.SubmittedBy)
                .HasMaxLength(4)
                .IsUnicode(false);
            entity.Property(e => e.ElementNo_BE01)
                .HasMaxLength(4)
                .IsUnicode(false);
            entity.Property(e => e.ElementParentNo_BE02)
                .HasMaxLength(4)
                .IsUnicode(false);

            entity.HasOne(d => d.ElementNo_BE01Navigation).WithMany(p => p.BridgeElements)
                .HasForeignKey(d => d.ElementNo_BE01)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BridgeElements_Lookup_Elements");

            entity.HasOne(d => d.BridgePrimary).WithMany(p => p.BridgeElements)
                .HasForeignKey(d => new { d.StateCode_BL01, d.BridgeNo_BID01, d.SubmittedBy })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BridgeElements_Bridges");
        });

        modelBuilder.Entity<BridgeFeature>(entity =>
        {
            entity.HasKey(e => new { e.StateCode_BL01, e.BridgeNo_BID01, e.SubmittedBy, e.FeatureType_BF01 });

            entity
                .ToTable("BridgeFeatures", "NBI")
                .ToTable(tb => tb.IsTemporal(ttb =>
                    {
                        ttb.UseHistoryTable("BridgeFeatures_History", "NBI");
                        ttb
                            .HasPeriodStart("SysStartTime")
                            .HasColumnName("SysStartTime");
                        ttb
                            .HasPeriodEnd("SysEndTime")
                            .HasColumnName("SysEndTime");
                    }));

            entity.Property(e => e.BridgeNo_BID01)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.SubmittedBy)
                .HasMaxLength(4)
                .IsUnicode(false);
            entity.Property(e => e.FeatureType_BF01)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.AADTT_BH10).HasColumnType("numeric(8, 0)");
            entity.Property(e => e.AADT_BH09).HasColumnType("numeric(8, 0)");
            entity.Property(e => e.BypassDetourLength_BH17).HasColumnType("numeric(3, 0)");
            entity.Property(e => e.CrossingBridgeNo_BH18)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.FeatureLocation_BF02)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.FeatureName_BF03)
                .HasMaxLength(300)
                .IsUnicode(false);
            entity.Property(e => e.FuncClass_BH01)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.HwyMaxUsableSurfaceWidth_BH16).HasColumnType("numeric(3, 1)");
            entity.Property(e => e.HwyMaxVertClearance_BH12).HasColumnType("numeric(3, 1)");
            entity.Property(e => e.HwyMinHorizClearanceLeft_BH14).HasColumnType("numeric(3, 1)");
            entity.Property(e => e.HwyMinHorizClearanceRight_BH15).HasColumnType("numeric(3, 1)");
            entity.Property(e => e.HwyMinVertClearance_BH13).HasColumnType("numeric(3, 1)");
            entity.Property(e => e.LRSMilePoint_BH07).HasColumnType("numeric(8, 3)");
            entity.Property(e => e.LRSRouteID_BH06)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.MovableMaxNavVertClearance_BN03).HasColumnType("numeric(4, 1)");
            entity.Property(e => e.NHSDesig_BH03)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.NatHwyFreightNet_BH04)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.NavChannelMinHorizClearance_BN05).HasColumnType("numeric(5, 1)");
            entity.Property(e => e.NavChannelWidth_BN04).HasColumnType("numeric(5, 1)");
            entity.Property(e => e.NavMinVertClearance_BN02).HasColumnType("numeric(4, 1)");
            entity.Property(e => e.NavWaterway_BN01)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.RailroadMinHorizOffset_BRR03).HasColumnType("numeric(3, 1)");
            entity.Property(e => e.RailroadMinVertClearance_BRR02).HasColumnType("numeric(3, 1)");
            entity.Property(e => e.RailroadServiceType_BRR01)
                .HasMaxLength(2)
                .IsUnicode(false);
            entity.Property(e => e.STRAHNETDesig_BH05)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.SubstructNavProtection_BN06)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.UrbanCode_BH02)
                .HasMaxLength(5)
                .IsUnicode(false);

            entity.HasOne(d => d.BridgePrimary).WithMany(p => p.BridgeFeatures)
                .HasForeignKey(d => new { d.StateCode_BL01, d.BridgeNo_BID01, d.SubmittedBy })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BridgeFeatures_Bridges");
        });

        modelBuilder.Entity<BridgeInspection>(entity =>
        {
            entity.HasKey(e => new { e.StateCode_BL01, e.BridgeNo_BID01, e.SubmittedBy, e.InspectionType_BIE01 });

            entity
                .ToTable("BridgeInspections", "NBI")
                .ToTable(tb => tb.IsTemporal(ttb =>
                    {
                        ttb.UseHistoryTable("BridgeInspections_History", "NBI");
                        ttb
                            .HasPeriodStart("SysStartTime")
                            .HasColumnName("SysStartTime");
                        ttb
                            .HasPeriodEnd("SysEndTime")
                            .HasColumnName("SysEndTime");
                    }));

            entity.Property(e => e.BridgeNo_BID01)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.SubmittedBy)
                .HasMaxLength(4)
                .IsUnicode(false);
            entity.Property(e => e.InspectionType_BIE01)
                .HasMaxLength(1)
                .IsUnicode(false);
            entity.Property(e => e.InspectEquipment_BIE12).HasMaxLength(120);
            entity.Property(e => e.InspectionNote_BIE11).HasMaxLength(300);
            entity.Property(e => e.NC_BridgeInspector_BIE04)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.RBI_Method_BIE07)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();

            entity.HasOne(d => d.BridgePrimary).WithMany(p => p.BridgeInspections)
                .HasForeignKey(d => new { d.StateCode_BL01, d.BridgeNo_BID01, d.SubmittedBy })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BridgeInspections_Bridges");
        });

        modelBuilder.Entity<BridgeNoChange>(entity =>
        {
            entity.HasKey(e => new { e.StateCode, e.SubmittedBy, e.OldBridgeNo, e.NewBridgeNo });

            entity.ToTable("BridgeNoChanges", "NBI");

            entity.Property(e => e.SubmittedBy)
                .HasMaxLength(4)
                .IsUnicode(false);
            entity.Property(e => e.OldBridgeNo)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.NewBridgeNo)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.ChangeDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<BridgePostingEvaluation>(entity =>
        {
            entity.HasKey(e => new { e.StateCode_BL01, e.BridgeNo_BID01, e.SubmittedBy, e.LegalLoadConfig_BEP01 });

            entity
                .ToTable("BridgePostingEvaluations", "NBI")
                .ToTable(tb => tb.IsTemporal(ttb =>
                    {
                        ttb.UseHistoryTable("BridgePostingEvaluations_History", "NBI");
                        ttb
                            .HasPeriodStart("SysStartTime")
                            .HasColumnName("SysStartTime");
                        ttb
                            .HasPeriodEnd("SysEndTime")
                            .HasColumnName("SysEndTime");
                    }));

            entity.Property(e => e.BridgeNo_BID01)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.SubmittedBy)
                .HasMaxLength(4)
                .IsUnicode(false);
            entity.Property(e => e.LegalLoadConfig_BEP01)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.LegalLoadRatingFactor_BEP02).HasColumnType("numeric(4, 2)");
            entity.Property(e => e.PostingType_BEP03)
                .HasMaxLength(17)
                .IsUnicode(false);
            entity.Property(e => e.PostingValue_BEP04)
                .HasMaxLength(15)
                .IsUnicode(false);

            entity.HasOne(d => d.BridgePrimary).WithMany(p => p.BridgePostingEvaluations)
                .HasForeignKey(d => new { d.StateCode_BL01, d.BridgeNo_BID01, d.SubmittedBy })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BridgePostingEvaluations_Bridges");
        });

        modelBuilder.Entity<BridgePostingStatus>(entity =>
        {
            entity.HasKey(e => new { e.StateCode_BL01, e.BridgeNo_BID01, e.SubmittedBy, e.PostingStatusChangeDate_BPS02 });

            entity
                .ToTable("BridgePostingStatuses", "NBI")
                .ToTable(tb => tb.IsTemporal(ttb =>
                    {
                        ttb.UseHistoryTable("BridgePostingStatuses_History", "NBI");
                        ttb
                            .HasPeriodStart("SysStartTime")
                            .HasColumnName("SysStartTime");
                        ttb
                            .HasPeriodEnd("SysEndTime")
                            .HasColumnName("SysEndTime");
                    }));

            entity.Property(e => e.BridgeNo_BID01)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.SubmittedBy)
                .HasMaxLength(4)
                .IsUnicode(false);
            entity.Property(e => e.LoadPostingStatus_BPS01)
                .HasMaxLength(5)
                .IsUnicode(false);

            entity.HasOne(d => d.BridgePrimary).WithMany(p => p.BridgePostingStatuses)
                .HasForeignKey(d => new { d.StateCode_BL01, d.BridgeNo_BID01, d.SubmittedBy })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BridgePostingStatuses_Bridges");
        });

        modelBuilder.Entity<BridgePrimary>(entity =>
        {
            entity.HasKey(e => new { e.StateCode_BL01, e.BridgeNo_BID01, e.SubmittedBy }).HasName("PK_BridgeInventory");

            entity
                .ToTable("BridgePrimary", "NBI")
                .ToTable(tb => tb.IsTemporal(ttb =>
                    {
                        ttb.UseHistoryTable("BridgeInventory_History", "NBI");
                        ttb
                            .HasPeriodStart("SysStartTime")
                            .HasColumnName("SysStartTime");
                        ttb
                            .HasPeriodEnd("SysEndTime")
                            .HasColumnName("SysEndTime");
                    }));

            entity.Property(e => e.BridgeNo_BID01)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.SubmittedBy)
                .HasMaxLength(4)
                .IsUnicode(false);
            entity.Property(e => e.ApproachRoadwayAlign_BAP01)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.ApproachRoadwayWidth_BG09).HasColumnType("numeric(4, 1)");
            entity.Property(e => e.BorderBridgeInspectResp_BL09)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.BorderBridgeLeadState_BL10)
                .HasMaxLength(2)
                .IsUnicode(false);
            entity.Property(e => e.BorderBridgeNo_BL07)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.BorderBridgeStateCode_BL08)
                .HasMaxLength(2)
                .IsUnicode(false);
            entity.Property(e => e.BridgeBearingCondRate_BC07)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.BridgeCondClass_BC12)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.BridgeJointCondRate_BC08)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.BridgeLocation_BL11).HasMaxLength(300);
            entity.Property(e => e.BridgeMedian_BG10)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.BridgeName_BID02).HasMaxLength(100);
            entity.Property(e => e.BridgeRailCondRate_BC05)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.BridgeRailTransitCondRate_BC06)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.BridgeRailings_BRH01)
                .HasMaxLength(4)
                .IsUnicode(false);
            entity.Property(e => e.BridgeWidthCurb_BG06).HasColumnType("numeric(4, 1)");
            entity.Property(e => e.BridgeWidthOut_BG05).HasColumnType("numeric(4, 1)");
            entity.Property(e => e.CalcDeckArea_BG16).HasColumnType("numeric(10, 1)");
            entity.Property(e => e.ChannelCondRate_BC09)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.ChannelProtectCondRate_BC10)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.ComplexFeature_BIR04)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.ControllingLegalLRFactor_BLR07).HasColumnType("numeric(4, 2)");
            entity.Property(e => e.CulvertCondRate_BC04)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.CurvedBridge_BG12)
                .HasMaxLength(2)
                .IsUnicode(false);
            entity.Property(e => e.DeckCondRate_BC01)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.DesignLoad_BLR01)
                .HasMaxLength(8)
                .IsUnicode(false);
            entity.Property(e => e.DesignMethod_BLR02)
                .HasMaxLength(4)
                .IsUnicode(false);
            entity.Property(e => e.EmergencyEvacDesig_BCL06)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.FatigueDetails_BIR02)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.FedTribalAccess_BCL03)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.HighwayDist_BL04)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.HistSignificance_BCL04)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.InventoryLRFactor_BLR05).HasColumnType("numeric(4, 2)");
            entity.Property(e => e.IrregularDeckArea_BG15).HasColumnType("numeric(10, 1)");
            entity.Property(e => e.Latitude_BL05).HasColumnType("numeric(9, 6)");
            entity.Property(e => e.LeftCurbWidth_BG07).HasColumnType("numeric(3, 1)");
            entity.Property(e => e.LoadRatingMethod_BLR04)
                .HasMaxLength(4)
                .IsUnicode(false);
            entity.Property(e => e.Longitude_BL06).HasColumnType("numeric(10, 6)");
            entity.Property(e => e.LowestCondRateCode_BC13)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.MPO_BL12).HasMaxLength(300);
            entity.Property(e => e.MaintResp_BCL02)
                .HasMaxLength(4)
                .IsUnicode(false);
            entity.Property(e => e.MaxSpanLength_BG03).HasColumnType("numeric(5, 1)");
            entity.Property(e => e.MinSpanLength_BG04).HasColumnType("numeric(5, 1)");
            entity.Property(e => e.NBISBridgeLength_BG01).HasColumnType("numeric(7, 1)");
            entity.Property(e => e.NSTMInspectCond_BC14)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.NSTMInspectReq_BIR01)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.OperatingLRFactor_BLR06).HasColumnType("numeric(4, 2)");
            entity.Property(e => e.OvertopLikelihood_BAP02)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.Owner_BCL01)
                .HasMaxLength(4)
                .IsUnicode(false);
            entity.Property(e => e.PrevBridgeNo_BID03)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.RightCurbWidth_BG08).HasColumnType("numeric(3, 1)");
            entity.Property(e => e.RoutinePermitLoads_BLR08)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.ScourCondRate_BC11)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.ScourPOA_BAP04)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.ScourVulnerability_BAP03)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.SeismicVulnerability_BAP05)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.SidehillBridge_BG14)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.SubstructCondRate_BC03)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.SuperstrCondRate_BC02)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Toll_BCL05)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.TotalBridgeLength_BG02).HasColumnType("numeric(7, 1)");
            entity.Property(e => e.Transitions_BRH02)
                .HasMaxLength(4)
                .IsUnicode(false);
            entity.Property(e => e.UnderwaterInspectCond_BC15)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.UnderwaterInspectReq_BIR03)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();

            entity.HasOne(d => d.Lookup_County).WithMany(p => p.BridgePrimaries)
                .HasForeignKey(d => new { d.StateCode_BL01, d.CountyCode_BL02 })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Bridges_CountyCodes");
        });

        modelBuilder.Entity<BridgeRoute>(entity =>
        {
            entity.HasKey(e => new { e.StateCode_BL01, e.BridgeNo_BID01, e.SubmittedBy, e.FeatureType_BF01, e.RouteDesignation_BRT01 });

            entity
                .ToTable("BridgeRoutes", "NBI")
                .ToTable(tb => tb.IsTemporal(ttb =>
                    {
                        ttb.UseHistoryTable("BridgeRoutes_History", "NBI");
                        ttb
                            .HasPeriodStart("SysStartTime")
                            .HasColumnName("SysStartTime");
                        ttb
                            .HasPeriodEnd("SysEndTime")
                            .HasColumnName("SysEndTime");
                    }));

            entity.Property(e => e.BridgeNo_BID01)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.SubmittedBy)
                .HasMaxLength(4)
                .IsUnicode(false);
            entity.Property(e => e.FeatureType_BF01)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.RouteDesignation_BRT01)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.RouteDirection_BRT03)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.RouteNumber_BRT02)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.RouteType_BRT04)
                .HasMaxLength(1)
                .IsUnicode(false);
            entity.Property(e => e.ServiceType_BRT05)
                .HasMaxLength(1)
                .IsUnicode(false);
        });

        modelBuilder.Entity<BridgeSpanSet>(entity =>
        {
            entity.HasKey(e => new { e.StateCode_BL01, e.BridgeNo_BID01, e.SubmittedBy, e.SpanConfigDesig_BSP01 }).HasName("PK_BridgeSuperstructures");

            entity
                .ToTable("BridgeSpanSets", "NBI")
                .ToTable(tb => tb.IsTemporal(ttb =>
                    {
                        ttb.UseHistoryTable("BridgeSpanSets_History", "NBI");
                        ttb
                            .HasPeriodStart("SysStartTime")
                            .HasColumnName("SysStartTime");
                        ttb
                            .HasPeriodEnd("SysEndTime")
                            .HasColumnName("SysEndTime");
                    }));

            entity.Property(e => e.BridgeNo_BID01)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.SubmittedBy)
                .HasMaxLength(4)
                .IsUnicode(false);
            entity.Property(e => e.SpanConfigDesig_BSP01)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.DeckInteraction_BSP08)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.DeckMaterial_BSP09)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.DeckProtectSystem_BSP11)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.DeckReinforcSystem_BSP12)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.DeckStayInPlaceForms_BSP13)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.NumberOfBeamLines_BSP03).HasColumnType("numeric(3, 0)");
            entity.Property(e => e.NumberOfSpans_BSP02).HasColumnType("numeric(4, 0)");
            entity.Property(e => e.SpanContinuity_BSP05)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.SpanMaterial_BSP04)
                .HasMaxLength(4)
                .IsUnicode(false);
            entity.Property(e => e.SpanProtectSystem_BSP07)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.SpanType_BSP06)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.WearingSurface_BSP10)
                .HasMaxLength(5)
                .IsUnicode(false);

            entity.HasOne(d => d.BridgePrimary).WithMany(p => p.BridgeSpanSets)
                .HasForeignKey(d => new { d.StateCode_BL01, d.BridgeNo_BID01, d.SubmittedBy })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SpanSets_Bridges");
        });

        modelBuilder.Entity<BridgeSubstructureSet>(entity =>
        {
            entity.HasKey(e => new { e.StateCode_BL01, e.BridgeNo_BID01, e.SubmittedBy, e.SubstructConfigDesig_BSB01 }).HasName("PK_BridgeSubstructures");

            entity
                .ToTable("BridgeSubstructureSets", "NBI")
                .ToTable(tb => tb.IsTemporal(ttb =>
                    {
                        ttb.UseHistoryTable("BridgeSubstructureSets_History", "NBI");
                        ttb
                            .HasPeriodStart("SysStartTime")
                            .HasColumnName("SysStartTime");
                        ttb
                            .HasPeriodEnd("SysEndTime")
                            .HasColumnName("SysEndTime");
                    }));

            entity.Property(e => e.BridgeNo_BID01)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.SubmittedBy)
                .HasMaxLength(4)
                .IsUnicode(false);
            entity.Property(e => e.SubstructConfigDesig_BSB01)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.FoundationProtectSystem_BSB07)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.FoundationType_BSB06)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.NoSubstructUnits_BSB02).HasColumnType("numeric(3, 0)");
            entity.Property(e => e.SubstructMaterial_BSB03)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.SubstructProtectSystem_BSB05)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.SubstructType_BSB04)
                .HasMaxLength(3)
                .IsUnicode(false);

            entity.HasOne(d => d.BridgePrimary).WithMany(p => p.BridgeSubstructureSets)
                .HasForeignKey(d => new { d.StateCode_BL01, d.BridgeNo_BID01, d.SubmittedBy })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SubstructureSets_Bridges");
        });

        modelBuilder.Entity<BridgeWork>(entity =>
        {
            entity.HasKey(e => new { e.StateCode_BL01, e.BridgeNo_BID01, e.SubmittedBy, e.YearWorkPerformed_BW02 });

            entity
                .ToTable("BridgeWorks", "NBI")
                .ToTable(tb => tb.IsTemporal(ttb =>
                    {
                        ttb.UseHistoryTable("BridgeWorks_History", "NBI");
                        ttb
                            .HasPeriodStart("SysStartTime")
                            .HasColumnName("SysStartTime");
                        ttb
                            .HasPeriodEnd("SysEndTime")
                            .HasColumnName("SysEndTime");
                    }));

            entity.Property(e => e.BridgeNo_BID01)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.SubmittedBy)
                .HasMaxLength(4)
                .IsUnicode(false);
            entity.Property(e => e.WorkPerformed_BW03)
                .HasMaxLength(120)
                .IsUnicode(false);

            entity.HasOne(d => d.BridgePrimary).WithMany(p => p.BridgeWorks)
                .HasForeignKey(d => new { d.StateCode_BL01, d.BridgeNo_BID01, d.SubmittedBy })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Works_Bridges");
        });

        modelBuilder.Entity<Email>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Email");

            entity.ToTable("Emails", "NBI");

            entity.Property(e => e.EmailDate).HasColumnType("datetime");
            entity.Property(e => e.From)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Message)
                .HasMaxLength(4000)
                .IsUnicode(false);
        });

        modelBuilder.Entity<LookupValue>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LookupVa__3214EC07E5470E28");

            entity.ToTable("LookupValues", "NBI");

            entity.Property(e => e.Code).HasMaxLength(100);
            entity.Property(e => e.FieldName).HasMaxLength(100);
        });

        modelBuilder.Entity<Lookup_County>(entity =>
        {
            entity.HasKey(e => new { e.St, e.Code }).HasName("PK_Counties");

            entity.ToTable("Lookup_Counties", "NBI");

            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Lookup_DataItem>(entity =>
        {
            entity.HasKey(e => e.NBI_Id).HasName("PK_Lookup_DataItems_new");

            entity.ToTable("Lookup_DataItems", "NBI");

            entity.Property(e => e.NBI_Id)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.DataSet)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Format)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Identifier)
                .HasMaxLength(100)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.ItemId)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.ItemName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Section)
                .HasMaxLength(500)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Lookup_Element>(entity =>
        {
            entity.HasKey(e => e.ElementNo).HasName("PK_Elements");

            entity.ToTable("Lookup_Elements", "NBI");

            entity.Property(e => e.ElementNo)
                .HasMaxLength(4)
                .IsUnicode(false);
            entity.Property(e => e.ElementName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ElementSubType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ElementType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MeasureUnit)
                .HasMaxLength(10)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Lookup_ErrorMessage>(entity =>
        {
            entity.HasKey(e => e.ErrorCode).HasName("PK_ErrorMessages");

            entity.ToTable("Lookup_ErrorMessages", "NBI");

            entity.Property(e => e.ErrorCode)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.ErrorDescription)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.ErrorType)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.ItemId)
                .HasMaxLength(4)
                .IsUnicode(false)
                .IsFixedLength();
        });

        modelBuilder.Entity<Lookup_HPMSRoute>(entity =>
        {
            entity.HasKey(e => new { e.YearRecord, e.RouteID, e.StateCode }).HasName("PK_HPMSRoutes");

            entity.ToTable("Lookup_HPMSRoutes", "NBI");

            entity.Property(e => e.RouteID)
                .HasMaxLength(120)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Lookup_State>(entity =>
        {
            entity.HasKey(e => e.Code);

            entity.ToTable("Lookup_States", "NBI");

            entity.Property(e => e.Abbreviation)
                .HasMaxLength(2)
                .IsUnicode(false);
            entity.Property(e => e.Description).HasMaxLength(100);
        });

        modelBuilder.Entity<Lookup_Status>(entity =>
        {
            entity.HasKey(e => e.StatusCode).HasName("PK_Status");

            entity.ToTable("Lookup_Statuses", "NBI");

            entity.Property(e => e.StatusDescription)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Report>(entity =>
        {
            entity.ToTable("Reports", "NBI");

            entity.Property(e => e.FileName)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.LoginId)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ReportDate).HasColumnType("datetime");
            entity.Property(e => e.ReportType)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.SubmittedBy)
                .HasMaxLength(4)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Stage_BridgeElement>(entity =>
        {
            entity.HasKey(e => e.ID).HasName("PK_StageBridgeElements");

            entity.ToTable("Stage_BridgeElements", "NBI", tb => tb.HasTrigger("trgr_StageBridgeElements"));

            entity.Property(e => e.BridgeNo_BID01)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.ElementNo_BE01)
                .HasMaxLength(4)
                .IsUnicode(false);
            entity.Property(e => e.ElementParentNo_BE02)
                .HasMaxLength(4)
                .IsUnicode(false);
            entity.Property(e => e.RecordStatus)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Active");
            entity.Property(e => e.SubmittedBy)
                .HasMaxLength(4)
                .IsUnicode(false);

            entity.HasOne(d => d.Stage_BridgePrimary).WithMany(p => p.Stage_BridgeElements)
                .HasForeignKey(d => new { d.SubmitId, d.StateCode_BL01, d.BridgeNo_BID01, d.SubmittedBy })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Stage_BridgeElements_Bridges");
        });

        modelBuilder.Entity<Stage_BridgeFeature>(entity =>
        {
            entity.HasKey(e => e.ID).HasName("PK_StageBridgeFeatures");

            entity.ToTable("Stage_BridgeFeatures", "NBI", tb => tb.HasTrigger("trgr_StageBridgeFeatures"));

            entity.Property(e => e.AADTT_BH10).HasColumnType("numeric(8, 0)");
            entity.Property(e => e.AADT_BH09).HasColumnType("numeric(8, 0)");
            entity.Property(e => e.BridgeNo_BID01)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.BypassDetourLength_BH17).HasColumnType("numeric(3, 0)");
            entity.Property(e => e.CrossingBridgeNo_BH18)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.FeatureLocation_BF02)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.FeatureName_BF03)
                .HasMaxLength(300)
                .IsUnicode(false);
            entity.Property(e => e.FeatureType_BF01)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.FuncClass_BH01)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.HwyMaxUsableSurfaceWidth_BH16).HasColumnType("numeric(3, 1)");
            entity.Property(e => e.HwyMaxVertClearance_BH12).HasColumnType("numeric(3, 1)");
            entity.Property(e => e.HwyMinHorizClearanceLeft_BH14).HasColumnType("numeric(3, 1)");
            entity.Property(e => e.HwyMinHorizClearanceRight_BH15).HasColumnType("numeric(3, 1)");
            entity.Property(e => e.HwyMinVertClearance_BH13).HasColumnType("numeric(3, 1)");
            entity.Property(e => e.LRSMilePoint_BH07).HasColumnType("numeric(8, 3)");
            entity.Property(e => e.LRSRouteID_BH06)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.MovableMaxNavVertClearance_BN03).HasColumnType("numeric(4, 1)");
            entity.Property(e => e.NHSDesig_BH03)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.NatHwyFreightNet_BH04)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.NavChannelMinHorizClearance_BN05).HasColumnType("numeric(5, 1)");
            entity.Property(e => e.NavChannelWidth_BN04).HasColumnType("numeric(5, 1)");
            entity.Property(e => e.NavMinVertClearance_BN02).HasColumnType("numeric(4, 1)");
            entity.Property(e => e.NavWaterway_BN01)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.RailroadMinHorizOffset_BRR03).HasColumnType("numeric(3, 1)");
            entity.Property(e => e.RailroadMinVertClearance_BRR02).HasColumnType("numeric(3, 1)");
            entity.Property(e => e.RailroadServiceType_BRR01)
                .HasMaxLength(2)
                .IsUnicode(false);
            entity.Property(e => e.RecordStatus)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Active");
            entity.Property(e => e.STRAHNETDesig_BH05)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.SubmittedBy)
                .HasMaxLength(4)
                .IsUnicode(false);
            entity.Property(e => e.SubstructNavProtection_BN06)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.UrbanCode_BH02)
                .HasMaxLength(5)
                .IsUnicode(false);

            entity.HasOne(d => d.Stage_BridgePrimary).WithMany(p => p.Stage_BridgeFeatures)
                .HasForeignKey(d => new { d.SubmitId, d.StateCode_BL01, d.BridgeNo_BID01, d.SubmittedBy })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Stage_BridgeFeatures_Stage_Bridges");
        });

        modelBuilder.Entity<Stage_BridgeInspection>(entity =>
        {
            entity.ToTable("Stage_BridgeInspections", "NBI", tb => tb.HasTrigger("trgr_StageBridgeInspections"));

            entity.Property(e => e.BridgeNo_BID01)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.InspectEquipment_BIE12).HasMaxLength(120);
            entity.Property(e => e.InspectionNote_BIE11).HasMaxLength(300);
            entity.Property(e => e.InspectionType_BIE01)
                .HasMaxLength(1)
                .IsUnicode(false);
            entity.Property(e => e.NC_BridgeInspector_BIE04)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.RBI_Method_BIE07)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.RecordStatus)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Active");
            entity.Property(e => e.SubmittedBy)
                .HasMaxLength(4)
                .IsUnicode(false);

            entity.HasOne(d => d.Stage_BridgePrimary).WithMany(p => p.Stage_BridgeInspections)
                .HasForeignKey(d => new { d.SubmitId, d.StateCode_BL01, d.BridgeNo_BID01, d.SubmittedBy })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StageInspections_Bridges");
        });

        modelBuilder.Entity<Stage_BridgePostingEvaluation>(entity =>
        {
            entity.HasKey(e => e.ID).HasName("PK_StageBridgePostingEvaluations");

            entity.ToTable("Stage_BridgePostingEvaluations", "NBI", tb => tb.HasTrigger("trgr_StageBridgePostingEvaluations"));

            entity.Property(e => e.BridgeNo_BID01)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.LegalLoadConfig_BEP01)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.LegalLoadRatingFactor_BEP02).HasColumnType("numeric(4, 2)");
            entity.Property(e => e.PostingType_BEP03)
                .HasMaxLength(17)
                .IsUnicode(false);
            entity.Property(e => e.PostingValue_BEP04)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.RecordStatus)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Active");
            entity.Property(e => e.SubmittedBy)
                .HasMaxLength(4)
                .IsUnicode(false);

            entity.HasOne(d => d.Stage_BridgePrimary).WithMany(p => p.Stage_BridgePostingEvaluations)
                .HasForeignKey(d => new { d.SubmitId, d.StateCode_BL01, d.BridgeNo_BID01, d.SubmittedBy })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StagePostingEvaluations_Bridges");
        });

        modelBuilder.Entity<Stage_BridgePostingStatus>(entity =>
        {
            entity.HasKey(e => e.ID).HasName("PK_StageBridgePostingStatuses");

            entity.ToTable("Stage_BridgePostingStatuses", "NBI", tb => tb.HasTrigger("trgr_StageBridgePostingStatuses"));

            entity.Property(e => e.BridgeNo_BID01)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.LoadPostingStatus_BPS01)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.RecordStatus)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Active");
            entity.Property(e => e.SubmittedBy)
                .HasMaxLength(4)
                .IsUnicode(false);

            entity.HasOne(d => d.Stage_BridgePrimary).WithMany(p => p.Stage_BridgePostingStatuses)
                .HasForeignKey(d => new { d.SubmitId, d.StateCode_BL01, d.BridgeNo_BID01, d.SubmittedBy })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StagePostingStatuses_Bridges");
        });

        modelBuilder.Entity<Stage_BridgePrimary>(entity =>
        {
            entity.HasKey(e => new { e.SubmitId, e.StateCode_BL01, e.BridgeNo_BID01, e.SubmittedBy }).HasName("PK_StageBridges");

            entity.ToTable("Stage_BridgePrimary", "NBI", tb => tb.HasTrigger("trgr_StageBridgePrimary"));

            entity.Property(e => e.BridgeNo_BID01)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.SubmittedBy)
                .HasMaxLength(4)
                .IsUnicode(false);
            entity.Property(e => e.ApproachRoadwayAlign_BAP01)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.ApproachRoadwayWidth_BG09).HasColumnType("numeric(4, 1)");
            entity.Property(e => e.BorderBridgeInspectResp_BL09)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.BorderBridgeLeadState_BL10)
                .HasMaxLength(2)
                .IsUnicode(false);
            entity.Property(e => e.BorderBridgeNo_BL07)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.BorderBridgeStateCode_BL08)
                .HasMaxLength(2)
                .IsUnicode(false);
            entity.Property(e => e.BridgeBearingCondRate_BC07)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.BridgeCondClass_BC12)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.BridgeJointCondRate_BC08)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.BridgeLocation_BL11).HasMaxLength(300);
            entity.Property(e => e.BridgeMedian_BG10)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.BridgeName_BID02).HasMaxLength(100);
            entity.Property(e => e.BridgeRailCondRate_BC05)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.BridgeRailTransitCondRate_BC06)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.BridgeRailings_BRH01)
                .HasMaxLength(4)
                .IsUnicode(false);
            entity.Property(e => e.BridgeWidthCurb_BG06).HasColumnType("numeric(4, 1)");
            entity.Property(e => e.BridgeWidthOut_BG05).HasColumnType("numeric(4, 1)");
            entity.Property(e => e.CalcDeckArea_BG16).HasColumnType("numeric(10, 1)");
            entity.Property(e => e.ChannelCondRate_BC09)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.ChannelProtectCondRate_BC10)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.ComplexFeature_BIR04)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.ControllingLegalLRFactor_BLR07).HasColumnType("numeric(4, 2)");
            entity.Property(e => e.CulvertCondRate_BC04)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.CurvedBridge_BG12)
                .HasMaxLength(2)
                .IsUnicode(false);
            entity.Property(e => e.DeckCondRate_BC01)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.DesignLoad_BLR01)
                .HasMaxLength(8)
                .IsUnicode(false);
            entity.Property(e => e.DesignMethod_BLR02)
                .HasMaxLength(4)
                .IsUnicode(false);
            entity.Property(e => e.EmergencyEvacDesig_BCL06)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.FatigueDetails_BIR02)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.FedTribalAccess_BCL03)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.HighwayDist_BL04)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.HistSignificance_BCL04)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.InventoryLRFactor_BLR05).HasColumnType("numeric(4, 2)");
            entity.Property(e => e.IrregularDeckArea_BG15).HasColumnType("numeric(10, 1)");
            entity.Property(e => e.Latitude_BL05).HasColumnType("numeric(9, 6)");
            entity.Property(e => e.LeftCurbWidth_BG07).HasColumnType("numeric(3, 1)");
            entity.Property(e => e.LoadRatingMethod_BLR04)
                .HasMaxLength(4)
                .IsUnicode(false);
            entity.Property(e => e.Longitude_BL06).HasColumnType("numeric(10, 6)");
            entity.Property(e => e.LowestCondRateCode_BC13)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.MPO_BL12).HasMaxLength(300);
            entity.Property(e => e.MaintResp_BCL02)
                .HasMaxLength(4)
                .IsUnicode(false);
            entity.Property(e => e.MaxSpanLength_BG03).HasColumnType("numeric(5, 1)");
            entity.Property(e => e.MinSpanLength_BG04).HasColumnType("numeric(5, 1)");
            entity.Property(e => e.NBISBridgeLength_BG01).HasColumnType("numeric(7, 1)");
            entity.Property(e => e.NSTMInspectCond_BC14)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.NSTMInspectReq_BIR01)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.OperatingLRFactor_BLR06).HasColumnType("numeric(4, 2)");
            entity.Property(e => e.OvertopLikelihood_BAP02)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.Owner_BCL01)
                .HasMaxLength(4)
                .IsUnicode(false);
            entity.Property(e => e.PrevBridgeNo_BID03)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.RightCurbWidth_BG08).HasColumnType("numeric(3, 1)");
            entity.Property(e => e.RoutinePermitLoads_BLR08)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.ScourCondRate_BC11)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.ScourPOA_BAP04)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.ScourVulnerability_BAP03)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.SeismicVulnerability_BAP05)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.SidehillBridge_BG14)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.SubstructCondRate_BC03)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.SuperstrCondRate_BC02)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Toll_BCL05)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.TotalBridgeLength_BG02).HasColumnType("numeric(7, 1)");
            entity.Property(e => e.Transitions_BRH02)
                .HasMaxLength(4)
                .IsUnicode(false);
            entity.Property(e => e.UnderwaterInspectCond_BC15)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.UnderwaterInspectReq_BIR03)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();

            entity.HasOne(d => d.StateCode_BL01Navigation).WithMany(p => p.Stage_BridgePrimaries)
                .HasForeignKey(d => d.StateCode_BL01)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Stage_Bridges_Lookup_States");

            entity.HasOne(d => d.Submit).WithMany(p => p.Stage_BridgePrimaries)
                .HasForeignKey(d => d.SubmitId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StageBridges_SubmittalLogs");

            entity.HasOne(d => d.Lookup_County).WithMany(p => p.Stage_BridgePrimaries)
                .HasForeignKey(d => new { d.StateCode_BL01, d.CountyCode_BL02 })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StageBridges_CountyCodes");
        });

        modelBuilder.Entity<Stage_BridgeRoute>(entity =>
        {
            entity.HasKey(e => e.ID).HasName("PK_StageBridgeRoutes");

            entity.ToTable("Stage_BridgeRoutes", "NBI", tb => tb.HasTrigger("trgr_StageBridgeRoutes"));

            entity.Property(e => e.BridgeNo_BID01)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.RecordStatus)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Active");
            entity.Property(e => e.RouteDesignation_BRT01)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.RouteDirection_BRT03)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.RouteNumber_BRT02)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.RouteType_BRT04)
                .HasMaxLength(1)
                .IsUnicode(false);
            entity.Property(e => e.ServiceType_BRT05)
                .HasMaxLength(1)
                .IsUnicode(false);
            entity.Property(e => e.SubmittedBy)
                .HasMaxLength(4)
                .IsUnicode(false);

            entity.HasOne(d => d.Feature).WithMany(p => p.Stage_BridgeRoutes)
                .HasForeignKey(d => d.FeatureID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Stage_BridgeRoutes_Features");
        });

        modelBuilder.Entity<Stage_BridgeSpanSet>(entity =>
        {
            entity.ToTable("Stage_BridgeSpanSets", "NBI", tb => tb.HasTrigger("trgr_StageBridgeSpanSets"));

            entity.Property(e => e.BridgeNo_BID01)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.DeckInteraction_BSP08)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.DeckMaterial_BSP09)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.DeckProtectSystem_BSP11)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.DeckReinforcSystem_BSP12)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.DeckStayInPlaceForms_BSP13)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.NumberOfBeamLines_BSP03).HasColumnType("numeric(3, 0)");
            entity.Property(e => e.NumberOfSpans_BSP02).HasColumnType("numeric(4, 0)");
            entity.Property(e => e.RecordStatus)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Active");
            entity.Property(e => e.SpanConfigDesig_BSP01)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.SpanContinuity_BSP05)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.SpanMaterial_BSP04)
                .HasMaxLength(4)
                .IsUnicode(false);
            entity.Property(e => e.SpanProtectSystem_BSP07)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.SpanType_BSP06)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.SubmittedBy)
                .HasMaxLength(4)
                .IsUnicode(false);
            entity.Property(e => e.WearingSurface_BSP10)
                .HasMaxLength(5)
                .IsUnicode(false);

            entity.HasOne(d => d.Stage_BridgePrimary).WithMany(p => p.Stage_BridgeSpanSets)
                .HasForeignKey(d => new { d.SubmitId, d.StateCode_BL01, d.BridgeNo_BID01, d.SubmittedBy })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StageSpanSets_Bridges");
        });

        modelBuilder.Entity<Stage_BridgeSubstructureSet>(entity =>
        {
            entity.ToTable("Stage_BridgeSubstructureSets", "NBI", tb => tb.HasTrigger("trgr_StageBridgeSubStructureSets"));

            entity.Property(e => e.BridgeNo_BID01)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.FoundationProtectSystem_BSB07)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.FoundationType_BSB06)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.NoSubstructUnits_BSB02).HasColumnType("numeric(3, 0)");
            entity.Property(e => e.RecordStatus)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Active");
            entity.Property(e => e.SubmittedBy)
                .HasMaxLength(4)
                .IsUnicode(false);
            entity.Property(e => e.SubstructConfigDesig_BSB01)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.SubstructMaterial_BSB03)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.SubstructProtectSystem_BSB05)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.SubstructType_BSB04)
                .HasMaxLength(3)
                .IsUnicode(false);

            entity.HasOne(d => d.Stage_BridgePrimary).WithMany(p => p.Stage_BridgeSubstructureSets)
                .HasForeignKey(d => new { d.SubmitId, d.StateCode_BL01, d.BridgeNo_BID01, d.SubmittedBy })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StageSubstructureSets_Bridges");
        });

        modelBuilder.Entity<Stage_BridgeWork>(entity =>
        {
            entity.ToTable("Stage_BridgeWorks", "NBI", tb => tb.HasTrigger("trgr_StageBridgeWorks"));

            entity.Property(e => e.BridgeNo_BID01)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.RecordStatus)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Active");
            entity.Property(e => e.SubmittedBy)
                .HasMaxLength(4)
                .IsUnicode(false);
            entity.Property(e => e.WorkPerformed_BW03)
                .HasMaxLength(120)
                .IsUnicode(false);

            entity.HasOne(d => d.Stage_BridgePrimary).WithMany(p => p.Stage_BridgeWorks)
                .HasForeignKey(d => new { d.SubmitId, d.StateCode_BL01, d.BridgeNo_BID01, d.SubmittedBy })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StageWorks_Bridges");
        });

        modelBuilder.Entity<SubmittalComment>(entity =>
        {
            entity.ToTable("SubmittalComments", "NBI");

            entity.Property(e => e.CommentText).IsUnicode(false);
            entity.Property(e => e.CommentType)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasDefaultValue("ALL");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(200)
                .IsUnicode(false);

            entity.HasOne(d => d.Submit).WithMany(p => p.SubmittalComments)
                .HasForeignKey(d => d.SubmitId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SubmittalComments_SubmittalLogs");
        });

        modelBuilder.Entity<SubmittalCount>(entity =>
        {
            entity.HasKey(e => new { e.CountId, e.SubmitId });

            entity.ToTable("SubmittalCounts", "NBI");

            entity.Property(e => e.CountId).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<SubmittalError>(entity =>
        {
            entity.HasKey(e => e.ErrorId);

            entity.ToTable("SubmittalErrors", "NBI");

            entity.Property(e => e.BridgeNo)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Comments)
                .HasMaxLength(1000)
                .IsUnicode(false);
            entity.Property(e => e.DataSet)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ErrorCode)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.ErrorDescription)
                .HasMaxLength(1000)
                .IsUnicode(false);
            entity.Property(e => e.ErrorType)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.IgnoredBy)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.ItemId)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Owner)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ReviewedBy)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.SubmittedBy)
                .HasMaxLength(4)
                .IsUnicode(false);
            entity.Property(e => e.SubmittedValue)
                .HasMaxLength(1000)
                .IsUnicode(false);
            entity.Property(e => e.Submitter)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.Submit).WithMany(p => p.SubmittalErrors)
                .HasForeignKey(d => d.SubmitId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SubmittalErrors_SubmittalLogs");
        });

        modelBuilder.Entity<SubmittalFile>(entity =>
        {
            entity.HasKey(e => new { e.FileId, e.SubmitId });

            entity.ToTable("SubmittalFiles", "NBI");

            entity.Property(e => e.FileName)
                .HasMaxLength(200)
                .IsUnicode(false);

            entity.HasOne(d => d.Submit).WithMany(p => p.SubmittalFiles)
                .HasForeignKey(d => d.SubmitId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SubmittalFiles_SubmittalLogs");
        });

        modelBuilder.Entity<SubmittalLog>(entity =>
        {
            entity.HasKey(e => e.SubmitId).HasName("PK_NBI_SubmittalLogs");

            entity.ToTable("SubmittalLogs", "NBI");

            entity.HasIndex(e => e.MergedIntoSubmitId, "IX_SubmittalLogs_MergedIntoSubmitId").HasFilter("([MergedIntoSubmitId] IS NOT NULL)");

            entity.HasIndex(e => e.UploadId, "UQ_SubmittalLogs_UploadId").IsUnique();

            entity.Property(e => e.ApproveRejectDate).HasPrecision(0);
            entity.Property(e => e.Approver)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ReviewDate).HasPrecision(0);
            entity.Property(e => e.Reviewer)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.SubmitDate).HasPrecision(0);
            entity.Property(e => e.SubmittedBy)
                .HasMaxLength(4)
                .IsUnicode(false);
            entity.Property(e => e.Submitter)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.UploadDate)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.UploadedBy)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.MergedIntoSubmit).WithMany(p => p.InverseMergedIntoSubmit).HasForeignKey(d => d.MergedIntoSubmitId);
        });

        modelBuilder.Entity<TriggerChangeLog>(entity =>
        {
            entity.HasKey(e => e.ActivityId).HasName("PK_activity_log");

            entity.ToTable("TriggerChangeLog", "NBI");
        });

        modelBuilder.Entity<VSubmittalLog>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("VSubmittalLogs", "NBI");

            entity.Property(e => e.ApproveRejectDate).HasPrecision(0);
            entity.Property(e => e.Approver)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Lookup_States_Abbreviation).HasMaxLength(100);
            entity.Property(e => e.Lookup_Statuses_StatusDescription)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ReportContent)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.ReviewDate).HasPrecision(0);
            entity.Property(e => e.Reviewer)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.SubmitDate).HasPrecision(0);
            entity.Property(e => e.SubmittedBy)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Submitter)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.UploadDate).HasPrecision(0);
            entity.Property(e => e.UploadedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
