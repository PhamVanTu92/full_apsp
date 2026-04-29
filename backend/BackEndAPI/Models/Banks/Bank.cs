using BackEndAPI.Models.Promotion;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.OpenApi.Any;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BackEndAPI.Models.Banks
{
    public class Bank
    { 
        public int Id { get; set; }
        [MaxLength(50)]
        public string BankCode { get; set; }
        [MaxLength(254)]
        public string BankName { get; set; }
        [MaxLength(254)]
        public string GlobalName { get; set; }
        [MaxLength (2)]
        public string BankType { get; set; }
        [MaxLength(2)]
        public string Status { get; set; }
        [MaxLength(254)]
        public string SearchText { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        [MaxLength(50)]
        public string? Creator { get; set; }
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
        [MaxLength(50)]
        public string? Updator { get; set; }
    }
    public class BankView
    {
        public int Id { get; set; }
        [MaxLength(50)]
        public string BankCode { get; set; }
        [MaxLength(254)]
        public string BankName { get; set; }
        [MaxLength(254)]
        public string GlobalName { get; set; }
    }
    public class BankCard
    {
        public int Id { get; set; }
        public int BankId { get; set; }
        [MaxLength(50)]
        public string BankCode { get; set; }
        [MaxLength(254)]
        public string BankName { get; set; }
        [MaxLength(50)]
        public string BankCardCode { get; set; }
        [MaxLength(254)]
        public string Cardholder { get; set; }
        [MaxLength(254)]
        public string? Remarks { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        [MaxLength(50)]
        public string? Creator { get; set; }
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
        [MaxLength(50)]
        public string? Updator { get; set; }
        [JsonIgnore]
        public Bank? Bank {get; set; }
    }
    public class BankCardCreate
    {
        public int Id { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Nhập Id ngân hàng")]
        public int BankId { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Nhập mã ngân hàng")]
        [MaxLength(50)]
        public string BankCode { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Nhập têm ngân hàng")]
        [MaxLength(254)]
        public string BankName { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Nhập số tài khoản ngân hàng")]
        [MaxLength(50)]
        public string BankCardCode { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Nhập tên chủ tài khoản ngân hàng")]
        [MaxLength(254)]
        public string Cardholder { get; set; }
        [MaxLength(254)]
        public string? Remarks { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Nhập người cập tạo")]
        [MaxLength(50)]
        public string? Creator { get; set; }
    }
    public class BankCardUpdate
    {
        public int Id { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Nhập Id ngân hàng")]
        public int BankId { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Nhập mã ngân hàng")]
        [MaxLength(50)]
        public string BankCode { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Nhập têm ngân hàng")]
        [MaxLength(254)]
        public string BankName { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Nhập số tài khoản ngân hàng")]
        [MaxLength(50)]
        public string BankCardCode { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Nhập tên chủ tài khoản ngân hàng")]
        [MaxLength(254)]
        public string Cardholder { get; set; }
        [MaxLength(254)]
        public string? Remarks { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Nhập người cập nhập")]
        [MaxLength(50)]
        public string? Updator { get; set; }
    }
    public static class BankList
    {
        public static List<Bank> GetBanks()
        {

            List<Bank> listBank = new List<Bank>
            {
            new Bank { Id = 1, BankCode = "Vietcombank",BankType ="B",BankName = "Ngân hàng TMCP Ngoại thương Việt Nam",GlobalName = "Bank for Foreign trade of Vietnam"
                    ,SearchText = "vietcombank ngan hang tmcp ngoai thuong viet nam",Status = "A"},
             new Bank { Id = 2, BankCode = "VietinBank",BankType ="B",BankName = "Ngân hàng TMCP Công Thương Việt Nam",GlobalName = "VietNam Joint Stock Commercial Bank for Industry and Trade"
                    ,SearchText = "vietinbank ngan hang tmcp cong thuong viet nam",Status = "A"},
             new Bank {Id = 3,BankCode = "BIDV",BankType ="B",BankName = "Ngân hàng TMCP Đầu tư và Phát triển Việt Nam",GlobalName = "Joint Stock Commercial Bank for Investment and Development of Vietnam",
                    Status = "A",SearchText = "bidv ngan hang tmcp dau tu va phat trien viet nam"},
             new Bank {Id = 4,BankCode = "Techcombank",BankType ="B",BankName = "Ngân hàng TMCP Kỹ Thương Việt Nam",GlobalName = "Vietnam Technological and Commercial Joint- stock Bank",
                    Status = "A",SearchText = "techcombank ngan hang tmcp ky thuong viet nam"},
             new Bank {Id = 5,BankCode = "ACB",BankType ="B",BankName = "Ngân hàng Á Châu",GlobalName = "Asia Commercial Joint Stock Bank",
                    Status = "A",SearchText = "acb ngan hang a chau"},
              new Bank {Id = 6,BankCode = "MB",BankType ="B",BankName = "Ngân hàng TMCP Quân Đội",GlobalName = "Military Commercial Joint Stock Bank",
                    Status = "A",SearchText = "mb ngan hang tmcp quan doi"},
              new Bank {Id = 7,BankCode = "VPBank",BankType ="B",BankName = "Ngân hàng TMCP Việt Nam Thịnh Vượng",GlobalName = "Vietnam Prosperity Joint-Stock Commercial Bank",
                    Status = "A",SearchText = "vpbank ngan hang tmcp viet nam thinh vuong"},
              new Bank{Id = 8,BankCode = "Agribank",BankType ="B",BankName = "Ngân hàng Nông nghiệp và Phát triển Nông thôn Việt Nam",GlobalName = "Vietnam Bank for Agriculture and Rural Development",Status = "A",SearchText = "agribank ngan hang nong nghiep va phat trien nong thon viet nam"},
              new Bank{Id = 9,BankCode = "SHB",BankType ="B",BankName = "Ngân hàng TMCP Sài Gòn – Hà Nội",GlobalName = "Saigon – Hanoi Commercial Joint Stock Bank",Status = "A",SearchText = "shb ngan hang tmcp sai gon – ha noi"
              },
              new Bank{Id = 10,BankCode = "Sacombank",BankType ="B",BankName = "Ngân hàng TMCP Sài Gòn Thương Tín",GlobalName = "Saigon Thuong Tin Commercial Joint Stock Bank",Status = "A",SearchText = "sacombank ngan hang tmcp sai gon thuong tin"
              },
              new Bank{Id = 11,BankCode = "HDB",BankType ="B",BankName = "Ngân hàng TMCP phát triển Tp.HCM",GlobalName = "Development Joint Stock Commercial Bank",Status = "A",SearchText = "hdb ngan hang tmcp phat trien tp.hcm"
              },
              new Bank{Id = 12,BankCode = "PVCombank",BankType ="B",BankName = "Ngân hàng TMCP Đại Chúng Việt Nam",GlobalName = "Vietnam Public Joint-stock Commercial Bank",Status = "A",SearchText = "pvcombank ngan hang tmcp dai chung viet nam"
              },
              new Bank
              {
                  Id = 13,
                  BankCode = "Oceanbank",BankType ="B",
                  BankName = "Ngân hàng TM TNHH Một Thành Viên Đại Dương",
                  GlobalName = "Ocean Commercial One Member Limited Liability Bank",
                  Status = "A",
                  SearchText = "oceanbank ngan hang tm tnhh mot thanh vien dai duong"
              },
              new Bank
              {
                  Id = 14,
                  BankCode = "LienVietPostBank",BankType ="B",
                  BankName = "Ngân hàng TMCP Bưu điện Liên Việt",
                  GlobalName = "Lien Viet Post Joint Stock Commercial Bank",
                  Status = "A",
                  SearchText = "lienvietpostbank ngan hang tmcp buu dien lien viet"
              },
              new Bank
              {
                  Id = 15,
                  BankCode = "TPBank",BankType ="B",
                  BankName = "Ngân hàng TMCP Tiên Phong",
                  GlobalName = "Tien Phong Commercial Joint Stock Bank",
                  Status = "A",
                  SearchText = "tpbank ngan hang tmcp tien phong"
              },
              new Bank
              {
                  Id = 16,
                  BankCode = "VIB",BankType ="B",
                  BankName = "Ngân hàng TMCP Quốc Tế Việt Nam",
                  GlobalName = "Vietnam International Commercial Joint Stock Bank",
                  Status = "A",
                  SearchText = "vib ngan hang tmcp quoc te viet nam"
              },
              new Bank
              {
                  Id = 17,
                  BankCode = "MSB Maritime Bank",BankType ="B",
                  BankName = "Ngân hàng TMCP Hàng Hải Việt Nam",
                  GlobalName = "Vietnam Maritime Commercial Joint Stock Bank",
                  Status = "A",
                  SearchText = "msb maritime bank ngan hang tmcp hang hai viet nam"
              },
              new Bank
              {
                  Id = 18,
                  BankCode = "Eximbank",BankType ="B",
                  BankName = "Ngân hàng TMCP Xuất nhập khẩu Việt Nam",
                  GlobalName = "Vietnam Export Import Bank",
                  Status = "A",
                  SearchText = "eximbank ngan hang tmcp xuat nhap khau viet nam"
              },
              new Bank
              {
                  Id = 19,
                  BankCode = "ABBank",BankType ="B",
                  BankName = "Ngân hàng TMCP An Bình",
                  GlobalName = "An Binh Commercial Joint Stock Bank",
                  Status = "A",
                  SearchText = "abbank ngan hang tmcp an binh"
              },
              new Bank
              {
                  Id = 20,
                  BankCode = "OCB",BankType ="B",
                  BankName = "Ngân hàng TMCP Phương Đông",
                  GlobalName = "Orient Commercial Joint Stock Bank",
                  Status = "A",
                  SearchText = "ocb ngan hang tmcp phuong dong"
              },
              new Bank
              {
                  Id = 21,
                  BankCode = "NASB",BankType ="B",
                  BankName = "Ngân hàng TMCP Bắc Á",
                  GlobalName = "North American State Bank",
                  Status = "A",
                  SearchText = "nasb ngan hang tmcp bac a"
              },
              new Bank
              {
                  Id = 22,
                  BankCode = "DongA Bank",BankType ="B",
                  BankName = "Ngân hàng TMCP Đông Á",
                  GlobalName = "DongA Joint Stock Commercial Bank",
                  Status = "A",
                  SearchText = "donga bank ngan hang tmcp dong a"
              },
              new Bank
              {
                  Id = 23,
                  BankCode = "VietABank",BankType ="B",
                  BankName = "Ngân hàng TMCP Việt Á",
                  GlobalName = "VietABank",
                  Status = "A",
                  SearchText = "vietabank ngan hang tmcp viet a"
              },
              new Bank
              {
                  Id = 24,
                  BankCode = "BAOVIET Bank",BankType ="B",
                  BankName = "Ngân hàng TMCP Bảo Việt",
                  GlobalName = "BAOVIET Bank",
                  Status = "A",
                  SearchText = "baoviet bank ngan hang tmcp bao viet"
              },
              new Bank
              {
                  Id = 25,
                  BankCode = "SAIGONBANK",BankType ="B",
                  BankName = "NGÂN HÀNG TMCP SÀI GÒN CÔNG THƯƠNG",
                  GlobalName = "SAIN BANK FOR INDUSTRY AND TRADE",
                  Status = "A",
                  SearchText = "saigonbank ngan hang tmcp sai gon cong thuong"
              },
              new Bank
              {
                  Id = 26,
                  BankCode = "NamABank",BankType ="B",
                  BankName = "Ngân hàng TMCP Nam Á",
                  GlobalName = "Nam A Commercial Joint Stock Bank",
                  Status = "A",
                  SearchText = "namabank ngan hang tmcp nam a"
              },
              new Bank
              {
                  Id = 27,
                  BankCode = "GPBank",BankType ="B",
                  BankName = "Ngân hàng TM TNHH MTV Dầu Khí Toàn Cầu.",
                  GlobalName = "Global Petro Sole Member Limited Commercial Bank",
                  Status = "A",
                  SearchText = "gpbank ngan hang tm tnhh mtv dau khi toan cau."
              },
              new Bank
              {
                  Id = 28,
                  BankCode = "KienLongBank",BankType ="B",
                  BankName = "Kiên Long",
                  GlobalName = "Kien Long Bank",
                  Status = "A",
                  SearchText = "kienlongbank kien long"
              },
              new Bank
              {
                  Id = 29,
                  BankCode = "VCCB",BankType ="B",
                  BankName = "Bản Việt",
                  GlobalName = "VIET CAPITAL BANK",
                  Status = "A",
                  SearchText = "vccb ban viet"
              },
              new Bank
              {
                  Id = 30,
                  BankCode = "PG Bank",BankType ="B",
                  BankName = "Xăng dầu Petrolimex",
                  GlobalName = "Petrolimex Group Bank",
                  Status = "A",
                  SearchText = "pg bank xang dau petrolimex"
              },
              new Bank{Id = 33,BankCode = "Co-opBank",BankType ="B",BankName = "Ngân hàng Hợp tác xã Việt Nam",GlobalName = "Co-operative bank of VietNam",Status = "A",SearchText = "co-opbank ngan hang hop tac xa viet nam"
              },
              new Bank{Id = 35,BankCode = "NCB",BankType ="B",BankName = "NH TMCP Quốc Dân",GlobalName = "National Citizen Commercial Joint Stock Bank",Status = "A",SearchText = "ncb nh tmcp quoc dan"
              },
              new Bank{Id = 37,BankCode = "SCB",BankType ="B",BankName = "Ngân hàng TMCP Sài Gòn",GlobalName = "Sai Gon Commercial Bank",Status = "A",SearchText = "scb ngan hang tmcp sai gon"
              },
              new Bank{Id = 38,BankCode = "VietBank",BankType ="B",BankName = "Ngân Hàng TMCP Việt Nam Thương Tín",GlobalName = "Vietnam Thuong Tin Commercial Joint Stock Bank",Status = "A",SearchText = "vietbank ngan hang tmcp viet nam thuong tin"
              },
              new Bank{Id = 41,BankCode = "Deutsche Bank",BankType ="B",BankName = "Deutsche Bank Việt Nam",GlobalName = "Deutsche Bank AG, Vietnam",Status = "A",SearchText = "deutsche bank deutsche bank viet nam"
              },
              new Bank{Id = 42,BankCode = "Citibank",BankType ="B",BankName = "Ngân hàng Citibank Việt Nam",GlobalName = "Citibank VietNam",Status = "A",SearchText = "citibank ngan hang citibank viet nam"
              },
              new Bank{Id = 43,BankCode = "HSBC",BankType ="B",BankName = "Ngân hàng TNHH MTV HSBC Việt Nam",GlobalName = "HSBC Bank Vietnam Limited",Status = "A",SearchText = "hsbc ngan hang tnhh mtv hsbc viet nam"
              },
              new Bank{Id = 44,BankCode = "Standard Chartered",BankType ="B",BankName = "Standard Chartered",GlobalName = "Standard Chartered Bank (Vietnam) Limited",Status = "A",SearchText = "standard chartered standard chartered"
              },
              new Bank{Id = 45,BankCode = "SHBVN",BankType ="B",BankName = "Ngân hàng TNHH MTV Shinhan Việt Nam",GlobalName = "Shinhan Vietnam Bank Limited",Status = "A",SearchText = "shbvn ngan hang tnhh mtv shinhan viet nam"
              },
              new Bank{Id = 46,BankCode = "HLBVN",BankType ="B",BankName = "Ngân hàng Hong Leong Việt Nam",GlobalName = "Hong Leong Bank Vietnam Limited ",Status = "A",SearchText = "hlbvn ngan hang hong leong viet nam"
              },
              new Bank{Id = 49,BankCode = "Mizuho",BankType ="B",BankName = "Ngân hàng Mizuho",GlobalName = "Mizuho bank",Status = "A",SearchText = "mizuho ngan hang mizuho"
              },
              new Bank{Id = 50,BankCode = "MUFG",BankType ="B",BankName = "MUFG Bank",GlobalName = "MUFG Bank",Status = "A",SearchText = "mufg mufg bank"
              },
              new Bank{Id = 51,BankCode = "SMBC",BankType ="B",BankName = "Ngân hàng Sumitomo Mitsui",GlobalName = "Sumitomo Mitsui Banking Corporation",Status = "A",SearchText = "smbc ngan hang sumitomo mitsui"
              },
              new Bank{Id = 52,BankCode = "IVB",BankType ="B",BankName = "Ngân hàng TNHH Indovina",GlobalName = "Indovina Bank Ltd",Status = "A",SearchText = "ivb ngan hang tnhh indovina"
              },
              new Bank{Id = 53,BankCode = "VRB",BankType ="B",BankName = "Ngân hàng Liên doanh Việt - Nga",GlobalName = "Vietnam - Russia Joint Venture Bank",Status = "A",SearchText = "vrb ngan hang lien doanh viet - nga"
              },
              new Bank{Id = 55,BankCode = "PBVN",BankType ="B",BankName = "Ngân hàng Public Việt Nam",GlobalName = "Public Bank Vietnam",Status = "A",SearchText = "pbvn ngan hang public viet nam"
              },
              new Bank{Id = 59,BankCode = "SEAB",BankType ="B",BankName = "TMCP Đông Nam Á",GlobalName = "SouthEast Asia Commercial Joint Stock Bank",Status = "A",SearchText = "seab tmcp dong nam a"
              },
              new Bank{Id = 60,BankCode = "CBB",BankType ="B",BankName = "TM TNHH MTV Xây Dựng Việt Nam",GlobalName = "Construction Commercial One Member Limited Liability Bank",Status = "A",SearchText = "cbb tm tnhh mtv xay dung viet nam"
              },
              new Bank{Id = 61,BankCode = "CIMB",BankType ="B",BankName = "Ngân hàng CIMB Việt Nam",GlobalName = "CIMB Bank Vietnam Limited",Status = "A",SearchText = "cimb ngan hang cimb viet nam"
              },
              new Bank{Id = 62,BankCode = "UOB",BankType ="B",BankName = "Ngân hàng UOB Việt Nam",GlobalName = "UOB Vietnam Limited",Status = "A",SearchText = "uob ngan hang uob viet nam"
              },
              new Bank{Id = 63,BankCode = "WVN",BankType ="B",BankName = "TNHH MTV Woori Việt Nam",GlobalName = "Woori Bank Vietnam Limited",Status = "A",SearchText = "wvn tnhh mtv woori viet nam"
              },
              new Bank{Id = 68,BankCode = "SGICB",BankType ="B",BankName = "TMCP Sài Gòn Công Thương",GlobalName = "",Status = "A",SearchText = "sgicb tmcp sai gon cong thuong"
              },
              new Bank{Id = 69,BankCode = "IBK HN",BankType ="B",BankName = "IBK - chi nhánh Hà Nội",GlobalName = "",Status = "A",SearchText = "ibk hn ibk - chi nhanh ha noi"
              },
              new Bank{Id = 70,BankCode = "IBK HCM",BankType ="B",BankName = "IBK - chi nhánh HCM",GlobalName = "",Status = "A",SearchText = "ibk hcm ibk - chi nhanh hcm"
              },
              new Bank
              {
                  Id = 71,
                  BankCode = "CAKE",BankType ="B",
                  BankName = "Ngân hàng số CAKE by VPBank",
                  GlobalName = "",
                  Status = "A",
                  SearchText = "cake ngan hang so cake by vpbank"
              },
              new Bank
              {
                  Id = 72,
                  BankCode = "UBANK",BankType ="B",
                  BankName = "Ngân hàng số Ubank by VPBank",
                  GlobalName = "",
                  Status = "A",
                  SearchText = "ubank ngan hang so ubank by vpbank"
              },
              new Bank
              {
                  Id = 73,
                  BankCode = "KBANK",BankType ="B",
                  BankName = "Đại chúng TNHH Kasikornbank",
                  GlobalName = "",
                  Status = "A",
                  SearchText = "kbank dai chung tnhh kasikornbank"
              },
              new Bank
              {
                  Id = 74,
                  BankCode = "CTBC",BankType ="B",
                  BankName = "Ngân hàng TNHH CTBC",
                  GlobalName = "CTBC Bank",
                  Status = "A",
                  SearchText = "ctbc ngan hang tnhh ctbc"
              }
            };
            return listBank;    
        }
    }
}
