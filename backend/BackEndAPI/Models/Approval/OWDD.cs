using BackEndAPI.Models.Unit;
using System.Text.Json.Serialization;

namespace BackEndAPI.Models.Approval
{
    public class OWDD
    {
        public int Id { get; set; }
        public int WtmId { get; set; }
        [JsonIgnore] public OWTM? OWTM { get; set; }

        public int OwnerId { get; set; }
        public int DocId { get; set; }
        public int ObjType { get; set; }

        public DateTime DocDate { get; set; }
        public int CurrStep { get; set; }
        public string Status { get; set; }
        public int MaxReqr { get; set; }
        public int MaxRejReqr { get; set; }
        public ICollection<WDD1>? WDD1 { get; set; }
    }

    public class WDD1
    {
        public int Id { get; set; }
        public int WddId { get; set; }

        public int StepCode { get; set; }
        public int UserId { get; set; }
        public string Status { get; set; }
        public DateTime CreateDate { get; set; }
        public int SortId { get; set; }
        [JsonIgnore] public OWDD? OWDD { get; set; }
    }
}