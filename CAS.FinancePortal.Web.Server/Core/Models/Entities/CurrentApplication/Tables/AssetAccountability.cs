#nullable disable
using System;
using System.Collections.Generic;


public partial class AssetAccountability
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public bool NewIssuance { get; set; }

    public bool Replacement { get; set; }

    public bool Borrowed { get; set; }

    public bool AdditionalEquipment { get; set; }

    public bool Returned { get; set; }

    public bool IsActive { get; set; }

    public DateOnly? DateIssued { get; set; }

    public DateOnly? DateReturned { get; set; }

    public int? Admin { get; set; }

    public int? AdminSignatureId { get; set; }

    public DateTime? AdminAcknowledgementDate { get; set; }

    public string AdminRemarks { get; set; }

    public int? Supervisor { get; set; }

    public int? SupervisorSignatureId { get; set; }

    public DateTime? SupervisorAcknowledgementDate { get; set; }

    public string SupervisorRemarks { get; set; }

    public int? Technical { get; set; }

    public int? TechnicalSignatureId { get; set; }

    public DateTime? TechnicalAcknowledgementDate { get; set; }

    public string TechnicalRemarks { get; set; }

    public int? EmployeeSignatureId { get; set; }

    public DateTime? EmployeeAcknowledgementDate { get; set; }

    public string EmployeeRemarks { get; set; }

    public bool? IsEmployeeApproved { get; set; }

    public virtual Signature AdminSignature { get; set; }

    public virtual ICollection<AssetAccountabilityItem> AssetAccountabilityItems { get; set; } = new List<AssetAccountabilityItem>();

    public virtual Signature EmployeeSignature { get; set; }

    public virtual Signature SupervisorSignature { get; set; }

    public virtual Signature TechnicalSignature { get; set; }
}
