using System;
using System.Collections.Generic;

namespace NBTIS.Data.Models;

public partial class activity_log
{
    public int ActivityId { get; set; }

    public string? TransactionId { get; set; }

    public string? Entity { get; set; }

    public string? TypeOfChange { get; set; }

    public int? Id { get; set; }

    public string? FieldName { get; set; }

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public string? UpdatedBy { get; set; }
}
