using System;
using System.Collections.Generic;

namespace NBTIS.Data.Models;

public partial class Stage_BridgeInspection
{
    public long ID { get; set; }

    public long SubmitId { get; set; }

    public byte StateCode_BL01 { get; set; }

    public string BridgeNo_BID01 { get; set; } = null!;

    public string SubmittedBy { get; set; } = null!;

    public string? InspectionType_BIE01 { get; set; }

    public DateOnly? BeginDate_BIE02 { get; set; }

    public DateOnly? CompletionDate_BIE03 { get; set; }

    public string? NC_BridgeInspector_BIE04 { get; set; }

    public byte? InspectInterval_BIE05 { get; set; }

    public DateOnly? InspectDueDate_BIE06 { get; set; }

    public string? RBI_Method_BIE07 { get; set; }

    public DateOnly? QltyControlDate_BIE08 { get; set; }

    public DateOnly? QltyAssuranceDate_BIE09 { get; set; }

    public DateOnly? InspectDataUpdateDate_BIE10 { get; set; }

    public string? InspectionNote_BIE11 { get; set; }

    public string RecordStatus { get; set; } = null!;

    public string? InspectEquipment_BIE12 { get; set; }

    public virtual Stage_BridgePrimary Stage_BridgePrimary { get; set; } = null!;
}
