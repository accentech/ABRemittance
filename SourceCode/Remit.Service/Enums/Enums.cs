using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Remit.Service.Enums
{
    public enum EntryStatusEnum : int
    {
        COMPONENT_CREATE = 11,
        COMPONENT_UPDATE = 12,
        COMPONENT_DELETE = 13,
        COMPONENT_UPDATECREATE = 14,
        COMPONENT_UPDATEDELETE = 15,
        OPEN_CREATE = 21,
        OPEN_UPDATE = 22,
        OPEN_DELETE = 23,
        OPEN_UPDATECREATE = 24,
        OPEN_UPDATEDELETE = 25,
        ISSUE_CREATE = 31,
        ISSUE_UPDATE = 32,
        ISSUE_DELETE = 33,
        ISSUE_UPDATECREATE = 34,
        ISSUE_UPDATEDELETE = 35,
        SCRAP_CREATE = 41,
        SCRAP_UPDATE = 42,
        SCRAP_DELETE = 43,
        SCRAP_UPDATECREATE = 44,
        SCRAP_UPDATEDELETE = 45,
        UNUSABLERETURN_CREATE = 51,
        UNUSABLERETURN_UPDATE = 52,
        UNUSABLERETURN_DELETE = 53,
        UNUSABLERETURN_UPDATECREATE = 54,
        UNUSABLERETURN_UPDATEDELETE = 55,
        LOAN_CREATE = 61,
        LOAN_UPDATE = 62,
        LOAN_DELETE = 63,
        LOAN_UPDATECREATE = 64,
        LOAN_UPDATEDELETE = 65,
        LOANRETURN_CREATE = 71,
        LOANRETURN_UPDATE = 72,
        LOANRETURN_DELETE = 73,
        LOANRETURN_UPDATECREATE = 74,
        LOANRETURN_UPDATEDELETE = 75,
        GOODRECIEVE_CREATE = 81,
        GOODRECIEVE_UPDATE = 82,
        GOODRECIEVE_DELETE = 83,
        GOODRECIEVE_UPDATECREATE = 84,
        GOODRECIEVE_UPDATEDELETE = 85,
        Inventory_Opening_Approve = 90
    }

    public enum PriorityEnum : int
    {
        CRITICAL = 1,
        NORMAL,
        AOG,
        IOR
    }
  
    public enum WayToReceiveEnum : int
    {
        Purchase = 1,
        Exchange,
        Service,
        Loan,
        ByAircraft,
        Other
    }
   
    public enum WorkFlowActionEnum : int
    {
        Approve = 1,
        Review,
        ApproveReject,
        HardApprove,
        APPROVAL
    }

    public enum StatusEnum : int
    {
        Running = 1,
        Complete,
        Cancelled
    }

    public enum AgeTypeEnum : int
    {
        Day = 1,
        Month,
        Year
    }
   
    public enum CommonEnum : int
    {
        Pending = 1,
        Review,
        ReviewRejected,
        Approved,
        Rejected,
        Issued,
    }

    public enum actionEnum : int
    {
        Create = 1,
        Update,
        Delete,
        Reject
    }

    public enum PaymentModeEnum : int
    {
        Cash = 1,
        CD,
        PDC,
        Online
    }


    public enum ItemGroupTypeEnum : int
    {
        RawMaterials = 1,
        SparepartsAndOthers
    }

    public enum PeriodTypeEnum : int
    {
        Monthly = 1,
        Quaterly,
        HalfYearly,
        Yearly
    }

    public enum MonthlyPeriodEnum : int
    {
        January = 1,
        February,
        March,
        April,
        May,
        June,
        July,
        August,
        September,
        October,
        November,
        December
    }
    public enum QuaterlyPeriodEnum : int
    {
        JanuaryToMarch =3,
        AprilToJune = 6,
        JulyToSeptember = 9,
        OctoberToNovember = 12
    }

    public enum HalfYearlyPeriodEnum : int
    {
        FirstHalf = 6,
        SecondHalf = 12
    }
    public enum YearlyPeriodEnum : int
    {
        FullYear = 12
    }
}
