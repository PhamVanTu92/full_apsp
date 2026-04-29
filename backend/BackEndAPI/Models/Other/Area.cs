using BackEndAPI.Models.Other;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BackEndAPI.Models.Other
{
    public class Area
    {
        public int Id { get; set; }
        [MaxLength(254)] public string Name { get; set; }
        [MaxLength(254)] public string NormalName { get; set; }
        public int KMSId { get; set; }
        public int PharmacyWardId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? ParentId { get; set; }
    }

    public static class AreaList
    {
        public static List<Area> GetArea()
        {
            List<Area> listArea = new List<Area>
            {
                new Area
                {
                    Id = 1,
                    Name = "An Giang - Huyện Chợ Mới",
                    NormalName = "An Giang - Huyen Cho Moi",

                    KMSId = 893
                },
                new Area
                {
                    Id = 2,
                    Name = "An Giang - Huyện Châu Phú",
                    NormalName = "An Giang - Huyen Chau Phu",

                    KMSId = 889
                },
                new Area
                {
                    Id = 3,
                    Name = "An Giang - Huyện Châu Thành",
                    NormalName = "An Giang - Huyen Chau Thanh",

                    KMSId = 892
                },
                new Area
                {
                    Id = 4,
                    Name = "An Giang - Huyện Tri Tôn",
                    NormalName = "An Giang - Huyen Tri Ton",

                    KMSId = 891
                },
                new Area
                {
                    Id = 5,
                    Name = "An Giang - Huyện Tịnh Biên",
                    NormalName = "An Giang - Huyen Tinh Bien",

                    KMSId = 890
                },
                new Area
                {
                    Id = 6,
                    Name = "An Giang - Thành phố Châu Đốc",
                    NormalName = "An Giang - Thanh pho Chau Doc",

                    KMSId = 884
                },
                new Area
                {
                    Id = 7,
                    Name = "An Giang - Huyện An Phú",
                    NormalName = "An Giang - Huyen An Phu",

                    KMSId = 886
                },
                new Area
                {
                    Id = 8,
                    Name = "An Giang - Thị xã Tân Châu",
                    NormalName = "An Giang - Thi xa Tan Chau",

                    KMSId = 887
                },
                new Area
                {
                    Id = 9,
                    Name = "An Giang - Huyện Phú Tân",
                    NormalName = "An Giang - Huyen Phu Tan",

                    KMSId = 888
                },
                new Area
                {
                    Id = 10,
                    Name = "An Giang - Thành phố Long Xuyên",
                    NormalName = "An Giang - Thanh pho Long Xuyen",

                    KMSId = 883
                },
                new Area
                {
                    Id = 11,
                    Name = "An Giang - Huyện Thoại Sơn",
                    NormalName = "An Giang - Huyen Thoai Son",

                    KMSId = 894
                },
                new Area
                {
                    Id = 12,
                    Name = "Bà Rịa - Vũng Tàu - Thị xã Phú Mỹ",
                    NormalName = "Ba Ria - Vung Tau - Thi xa Phu My",

                    KMSId = 754
                },
                new Area
                {
                    Id = 13,
                    Name = "Bà Rịa - Vũng Tàu - Thành phố Vũng Tàu",
                    NormalName = "Ba Ria - Vung Tau - Thanh pho Vung Tau",

                    KMSId = 747
                },
                new Area
                {
                    Id = 14,
                    Name = "Bà Rịa - Vũng Tàu - Huyện Châu Đức",
                    NormalName = "Ba Ria - Vung Tau - Huyen Chau Duc",

                    KMSId = 750
                },
                new Area
                {
                    Id = 15,
                    Name = "Bà Rịa - Vũng Tàu - Huyện Đất Đỏ",
                    NormalName = "Ba Ria - Vung Tau - Huyen Dat Do",

                    KMSId = 753
                },
                new Area
                {
                    Id = 16,
                    Name = "Bà Rịa - Vũng Tàu - Huyện Long Điền",
                    NormalName = "Ba Ria - Vung Tau - Huyen Long Dien",

                    KMSId = 752
                },
                new Area
                {
                    Id = 17,
                    Name = "Bà Rịa - Vũng Tàu - Thành phố Bà Rịa",
                    NormalName = "Ba Ria - Vung Tau - Thanh pho Ba Ria",

                    KMSId = 748
                },
                new Area
                {
                    Id = 18,
                    Name = "Bà Rịa - Vũng Tàu - Huyện Xuyên Mộc",
                    NormalName = "Ba Ria - Vung Tau - Huyen Xuyen Moc",

                    KMSId = 751
                },
                new Area
                {
                    Id = 19,
                    Name = "Bà Rịa - Vũng Tàu - Huyện Côn Đảo",
                    NormalName = "Ba Ria - Vung Tau - Huyen Con Dao",

                    KMSId = 755
                },
                new Area
                {
                    Id = 20,
                    Name = "Bạc Liêu - Huyện Đông Hải",
                    NormalName = "Bac Lieu - Huyen Dong Hai",

                    KMSId = 960
                },
                new Area
                {
                    Id = 21,
                    Name = "Bạc Liêu - Thị xã Giá Rai",
                    NormalName = "Bac Lieu - Thi xa Gia Rai",

                    KMSId = 959
                },
                new Area
                {
                    Id = 22,
                    Name = "Bạc Liêu - Huyện Hồng Dân",
                    NormalName = "Bac Lieu - Huyen Hong Dan",

                    KMSId = 956
                },
                new Area
                {
                    Id = 23,
                    Name = "Bạc Liêu - Thành phố Bạc Liêu",
                    NormalName = "Bac Lieu - Thanh pho Bac Lieu",

                    KMSId = 954
                },
                new Area
                {
                    Id = 24,
                    Name = "Bạc Liêu - Huyện Hòa Bình",
                    NormalName = "Bac Lieu - Huyen Hoa Binh",

                    KMSId = 961
                },
                new Area
                {
                    Id = 25,
                    Name = "Bạc Liêu - Huyện Phước Long",
                    NormalName = "Bac Lieu - Huyen Phuoc Long",

                    KMSId = 957
                },
                new Area
                {
                    Id = 26,
                    Name = "Bạc Liêu - Huyện Vĩnh Lợi",
                    NormalName = "Bac Lieu - Huyen Vinh Loi",

                    KMSId = 958
                },
                new Area
                {
                    Id = 27,
                    Name = "Bắc Giang - Huyện Lục Nam",
                    NormalName = "Bac Giang - Huyen Luc Nam",

                    KMSId = 218
                },
                new Area
                {
                    Id = 28,
                    Name = "Bắc Giang - Huyện Lục Ngạn",
                    NormalName = "Bac Giang - Huyen Luc Ngan",

                    KMSId = 219
                },
                new Area
                {
                    Id = 29,
                    Name = "Bắc Giang - Huyện Hiệp Hòa",
                    NormalName = "Bac Giang - Huyen Hiep Hoa",

                    KMSId = 223
                },
                new Area
                {
                    Id = 30,
                    Name = "Bắc Giang - Huyện Lạng Giang",
                    NormalName = "Bac Giang - Huyen Lang Giang",

                    KMSId = 217
                },
                new Area
                {
                    Id = 31,
                    Name = "Bắc Giang - Huyện Sơn Động",
                    NormalName = "Bac Giang - Huyen Son Dong",

                    KMSId = 220
                },
                new Area
                {
                    Id = 32,
                    Name = "Bắc Giang - Huyện Tân Yên",
                    NormalName = "Bac Giang - Huyen Tan Yen",

                    KMSId = 216
                },
                new Area
                {
                    Id = 33,
                    Name = "Bắc Giang - Huyện Việt Yên",
                    NormalName = "Bac Giang - Huyen Viet Yen",

                    KMSId = 222
                },
                new Area
                {
                    Id = 34,
                    Name = "Bắc Giang - Huyện Yên Dũng",
                    NormalName = "Bac Giang - Huyen Yen Dung",

                    KMSId = 221
                },
                new Area
                {
                    Id = 35,
                    Name = "Bắc Giang - Huyện Yên Thế",
                    NormalName = "Bac Giang - Huyen Yen The",

                    KMSId = 215
                },
                new Area
                {
                    Id = 36,
                    Name = "Bắc Giang - Thành phố Bắc Giang",
                    NormalName = "Bac Giang - Thanh pho Bac Giang",

                    KMSId = 213
                },
                new Area
                {
                    Id = 37,
                    Name = "Bắc Kạn - Huyện Bạch Thông",
                    NormalName = "Bac Kan - Huyen Bach Thong",

                    KMSId = 63
                },
                new Area
                {
                    Id = 38,
                    Name = "Bắc Kạn - Huyện Ba Bể",
                    NormalName = "Bac Kan - Huyen Ba Be",

                    KMSId = 61
                },
                new Area
                {
                    Id = 39,
                    Name = "Bắc Kạn - Thành phố Bắc Kạn",
                    NormalName = "Bac Kan - Thanh pho Bac Kan",

                    KMSId = 58
                },
                new Area
                {
                    Id = 40,
                    Name = "Bắc Kạn - Huyện Chợ Đồn",
                    NormalName = "Bac Kan - Huyen Cho Don",

                    KMSId = 64
                },
                new Area
                {
                    Id = 41,
                    Name = "Bắc Kạn - Huyện Chợ Mới",
                    NormalName = "Bac Kan - Huyen Cho Moi",

                    KMSId = 65
                },
                new Area
                {
                    Id = 42,
                    Name = "Bắc Kạn - Huyện Na Rì",
                    NormalName = "Bac Kan - Huyen Na Ri",

                    KMSId = 66
                },
                new Area
                {
                    Id = 43,
                    Name = "Bắc Kạn - Huyện Ngân Sơn",
                    NormalName = "Bac Kan - Huyen Ngan Son",

                    KMSId = 62
                },
                new Area
                {
                    Id = 44,
                    Name = "Bắc Kạn - Huyện Pác Nặm",
                    NormalName = "Bac Kan - Huyen Pac Nam",

                    KMSId = 60
                },
                new Area
                {
                    Id = 45,
                    Name = "Bắc Ninh - Thành phố Bắc Ninh",
                    NormalName = "Bac Ninh - Thanh pho Bac Ninh",

                    KMSId = 256
                },
                new Area
                {
                    Id = 46,
                    Name = "Bắc Ninh - Huyện Quế Võ",
                    NormalName = "Bac Ninh - Huyen Que Vo",

                    KMSId = 259
                },
                new Area
                {
                    Id = 47,
                    Name = "Bắc Ninh - Huyện Tiên Du",
                    NormalName = "Bac Ninh - Huyen Tien Du",

                    KMSId = 260
                },
                new Area
                {
                    Id = 48,
                    Name = "Bắc Ninh - Thành phố Từ Sơn",
                    NormalName = "Bac Ninh - Thanh pho Tu Son",

                    KMSId = 261
                },
                new Area
                {
                    Id = 49,
                    Name = "Bắc Ninh - Huyện Gia Bình",
                    NormalName = "Bac Ninh - Huyen Gia Binh",

                    KMSId = 263
                },
                new Area
                {
                    Id = 50,
                    Name = "Bắc Ninh - Huyện Thuận Thành",
                    NormalName = "Bac Ninh - Huyen Thuan Thanh",

                    KMSId = 262
                },
                new Area
                {
                    Id = 51,
                    Name = "Bắc Ninh - Huyện Yên Phong",
                    NormalName = "Bac Ninh - Huyen Yen Phong",

                    KMSId = 258
                },
                new Area
                {
                    Id = 52,
                    Name = "Bắc Ninh - Huyện Lương Tài",
                    NormalName = "Bac Ninh - Huyen Luong Tai",

                    KMSId = 264
                },
                new Area
                {
                    Id = 53,
                    Name = "Bến Tre - Huyện Giồng Trôm",
                    NormalName = "Ben Tre - Huyen Giong Trom",

                    KMSId = 834
                },
                new Area
                {
                    Id = 54,
                    Name = "Bến Tre - Huyện Mỏ Cày Bắc",
                    NormalName = "Ben Tre - Huyen Mo Cay Bac",

                    KMSId = 838
                },
                new Area
                {
                    Id = 55,
                    Name = "Bến Tre - Huyện Mỏ Cày Nam",
                    NormalName = "Ben Tre - Huyen Mo Cay Nam",

                    KMSId = 833
                },
                new Area
                {
                    Id = 56,
                    Name = "Bến Tre - Huyện Châu Thành",
                    NormalName = "Ben Tre - Huyen Chau Thanh",

                    KMSId = 831
                },
                new Area
                {
                    Id = 57,
                    Name = "Bến Tre - Huyện Bình Đại",
                    NormalName = "Ben Tre - Huyen Binh Dai",

                    KMSId = 835
                },
                new Area
                {
                    Id = 58,
                    Name = "Bến Tre - Huyện Ba Tri",
                    NormalName = "Ben Tre - Huyen Ba Tri",

                    KMSId = 836
                },
                new Area
                {
                    Id = 59,
                    Name = "Bến Tre - Thành phố Bến Tre",
                    NormalName = "Ben Tre - Thanh pho Ben Tre",

                    KMSId = 829
                },
                new Area
                {
                    Id = 60,
                    Name = "Bến Tre - Huyện Thạnh Phú",
                    NormalName = "Ben Tre - Huyen Thanh Phu",

                    KMSId = 837
                },
                new Area
                {
                    Id = 61,
                    Name = "Bến Tre - Huyện Chợ Lách",
                    NormalName = "Ben Tre - Huyen Cho Lach",

                    KMSId = 832
                },
                new Area
                {
                    Id = 62,
                    Name = "Bình Dương - Huyện Bàu Bàng",
                    NormalName = "Binh Duong - Huyen Bau Bang",

                    KMSId = 719
                },
                new Area
                {
                    Id = 63,
                    Name = "Bình Dương - Thị xã Bến Cát",
                    NormalName = "Binh Duong - Thi xa Ben Cat",

                    KMSId = 721
                },
                new Area
                {
                    Id = 64,
                    Name = "Bình Dương - Huyện Dầu Tiếng",
                    NormalName = "Binh Duong - Huyen Dau Tieng",

                    KMSId = 720
                },
                new Area
                {
                    Id = 65,
                    Name = "Bình Dương - Thành phố Thủ Dầu Một",
                    NormalName = "Binh Duong - Thanh pho Thu Dau Mot",

                    KMSId = 718
                },
                new Area
                {
                    Id = 66,
                    Name = "Bình Dương - Thành phố Dĩ An",
                    NormalName = "Binh Duong - Thanh pho Di An",

                    KMSId = 724
                },
                new Area
                {
                    Id = 67,
                    Name = "Bình Dương - Thành phố Thuận An",
                    NormalName = "Binh Duong - Thanh pho Thuan An",

                    KMSId = 725
                },
                new Area
                {
                    Id = 68,
                    Name = "Bình Dương - Thị xã Tân Uyên",
                    NormalName = "Binh Duong - Thi xa Tan Uyen",

                    KMSId = 723
                },
                new Area
                {
                    Id = 69,
                    Name = "Bình Dương - Huyện Phú Giáo",
                    NormalName = "Binh Duong - Huyen Phu Giao",

                    KMSId = 722
                },
                new Area
                {
                    Id = 70,
                    Name = "Bình Dương - Huyện Bắc Tân Uyên",
                    NormalName = "Binh Duong - Huyen Bac Tan Uyen",

                    KMSId = 726
                },
                new Area
                {
                    Id = 71,
                    Name = "Bình Định - Thành phố Quy Nhơn",
                    NormalName = "Binh Dinh - Thanh pho Quy Nhon",

                    KMSId = 540
                },
                new Area
                {
                    Id = 72,
                    Name = "Bình Định - Thị xã An Nhơn",
                    NormalName = "Binh Dinh - Thi xa An Nhon",

                    KMSId = 549
                },
                new Area
                {
                    Id = 73,
                    Name = "Bình Định - Huyện Phù Cát",
                    NormalName = "Binh Dinh - Huyen Phu Cat",

                    KMSId = 548
                },
                new Area
                {
                    Id = 74,
                    Name = "Bình Định - Thị xã Hoài Nhơn",
                    NormalName = "Binh Dinh - Thi xa Hoai Nhon",

                    KMSId = 543
                },
                new Area
                {
                    Id = 75,
                    Name = "Bình Định - Huyện An Lão",
                    NormalName = "Binh Dinh - Huyen An Lao",

                    KMSId = 542
                },
                new Area
                {
                    Id = 76,
                    Name = "Bình Định - Huyện Tuy Phước",
                    NormalName = "Binh Dinh - Huyen Tuy Phuoc",

                    KMSId = 550
                },
                new Area
                {
                    Id = 77,
                    Name = "Bình Định - Huyện Hoài Ân",
                    NormalName = "Binh Dinh - Huyen Hoai An",

                    KMSId = 544
                },
                new Area
                {
                    Id = 78,
                    Name = "Bình Định - Huyện Phù Mỹ",
                    NormalName = "Binh Dinh - Huyen Phu My",

                    KMSId = 545
                },
                new Area
                {
                    Id = 79,
                    Name = "Bình Định - Huyện Tây Sơn",
                    NormalName = "Binh Dinh - Huyen Tay Son",

                    KMSId = 547
                },
                new Area
                {
                    Id = 80,
                    Name = "Bình Định - Huyện Vân Canh",
                    NormalName = "Binh Dinh - Huyen Van Canh",

                    KMSId = 551
                },
                new Area
                {
                    Id = 81,
                    Name = "Bình Định - Huyện Vĩnh Thạnh",
                    NormalName = "Binh Dinh - Huyen Vinh Thanh",

                    KMSId = 546
                },
                new Area
                {
                    Id = 82,
                    Name = "Bình Phước - Huyện Bù Đăng",
                    NormalName = "Binh Phuoc - Huyen Bu Dang",

                    KMSId = 696
                },
                new Area
                {
                    Id = 83,
                    Name = "Bình Phước - Huyện Đồng Phú",
                    NormalName = "Binh Phuoc - Huyen Dong Phu",

                    KMSId = 695
                },
                new Area
                {
                    Id = 84,
                    Name = "Bình Phước - Huyện Lộc Ninh",
                    NormalName = "Binh Phuoc - Huyen Loc Ninh",

                    KMSId = 692
                },
                new Area
                {
                    Id = 85,
                    Name = "Bình Phước - Thị Xã Chơn Thành",
                    NormalName = "Binh Phuoc - Thi Xa Chon Thanh",

                    KMSId = 697
                },
                new Area
                {
                    Id = 86,
                    Name = "Bình Phước - Huyện Hớn Quản",
                    NormalName = "Binh Phuoc - Huyen Hon Quan",

                    KMSId = 694
                },
                new Area
                {
                    Id = 87,
                    Name = "Bình Phước - Thị xã Bình Long",
                    NormalName = "Binh Phuoc - Thi xa Binh Long",

                    KMSId = 690
                },
                new Area
                {
                    Id = 88,
                    Name = "Bình Phước - Thị xã Phước Long",
                    NormalName = "Binh Phuoc - Thi xa Phuoc Long",

                    KMSId = 688
                },
                new Area
                {
                    Id = 89,
                    Name = "Bình Phước - Thành phố Đồng Xoài",
                    NormalName = "Binh Phuoc - Thanh pho Dong Xoai",

                    KMSId = 689
                },
                new Area
                {
                    Id = 90,
                    Name = "Bình Phước - Huyện Bù Gia Mập",
                    NormalName = "Binh Phuoc - Huyen Bu Gia Map",

                    KMSId = 691
                },
                new Area
                {
                    Id = 91,
                    Name = "Bình Phước - Huyện Bù Đốp",
                    NormalName = "Binh Phuoc - Huyen Bu Dop",

                    KMSId = 693
                },
                new Area
                {
                    Id = 731,
                    Name = "Bình Phước - Huyện Phú Riềng",
                    NormalName = "Binh Phuoc - Huyen Phu Rieng",

                    KMSId = 698
                },
                new Area
                {
                    Id = 92,
                    Name = "Bình Thuận - Thành phố Phan Thiết",
                    NormalName = "Binh Thuan - Thanh pho Phan Thiet",

                    KMSId = 593
                },
                new Area
                {
                    Id = 93,
                    Name = "Bình Thuận - Huyện Hàm Thuận Nam",
                    NormalName = "Binh Thuan - Huyen Ham Thuan Nam",

                    KMSId = 598
                },
                new Area
                {
                    Id = 94,
                    Name = "Bình Thuận - Huyện Hàm Thuận Bắc",
                    NormalName = "Binh Thuan - Huyen Ham Thuan Bac",

                    KMSId = 597
                },
                new Area
                {
                    Id = 95,
                    Name = "Bình Thuận - Thị xã La Gi",
                    NormalName = "Binh Thuan - Thi xa La Gi",

                    KMSId = 594
                },
                new Area
                {
                    Id = 96,
                    Name = "Bình Thuận - Huyện Đức Linh",
                    NormalName = "Binh Thuan - Huyen Duc Linh",

                    KMSId = 600
                },
                new Area
                {
                    Id = 97,
                    Name = "Bình Thuận - Huyện Bắc Bình",
                    NormalName = "Binh Thuan - Huyen Bac Binh",

                    KMSId = 596
                },
                new Area
                {
                    Id = 98,
                    Name = "Bình Thuận - Huyện Tuy Phong",
                    NormalName = "Binh Thuan - Huyen Tuy Phong",

                    KMSId = 595
                },
                new Area
                {
                    Id = 99,
                    Name = "Bình Thuận - Huyện Tánh Linh",
                    NormalName = "Binh Thuan - Huyen Tanh Linh",

                    KMSId = 599
                },
                new Area
                {
                    Id = 100,
                    Name = "Bình Thuận - Huyện Hàm Tân",
                    NormalName = "Binh Thuan - Huyen Ham Tan",

                    KMSId = 601
                },
                new Area
                {
                    Id = 101,
                    Name = "Bình Thuận - Huyện Phú Quí",
                    NormalName = "Binh Thuan - Huyen Phu Qui",

                    KMSId = 602
                },
                new Area
                {
                    Id = 102,
                    Name = "Cà Mau - Huyện U Minh",
                    NormalName = "Ca Mau - Huyen U Minh",

                    KMSId = 966
                },
                new Area
                {
                    Id = 103,
                    Name = "Cà Mau - Huyện Trần Văn Thời",
                    NormalName = "Ca Mau - Huyen Tran Van Thoi",

                    KMSId = 968
                },
                new Area
                {
                    Id = 104,
                    Name = "Cà Mau - Thành phố Cà Mau",
                    NormalName = "Ca Mau - Thanh pho Ca Mau",

                    KMSId = 964
                },
                new Area
                {
                    Id = 105,
                    Name = "Cà Mau - Huyện Cái Nước",
                    NormalName = "Ca Mau - Huyen Cai Nuoc",

                    KMSId = 969
                },
                new Area
                {
                    Id = 106,
                    Name = "Cà Mau - Huyện Phú Tân",
                    NormalName = "Ca Mau - Huyen Phu Tan",

                    KMSId = 972
                },
                new Area
                {
                    Id = 107,
                    Name = "Cà Mau - Huyện Thới Bình",
                    NormalName = "Ca Mau - Huyen Thoi Binh",

                    KMSId = 967
                },
                new Area
                {
                    Id = 108,
                    Name = "Cà Mau - Huyện Năm Căn",
                    NormalName = "Ca Mau - Huyen Nam Can",

                    KMSId = 971
                },
                new Area
                {
                    Id = 109,
                    Name = "Cà Mau - Huyện Đầm Dơi",
                    NormalName = "Ca Mau - Huyen Dam Doi",

                    KMSId = 970
                },
                new Area
                {
                    Id = 110,
                    Name = "Cà Mau - Huyện Ngọc Hiển",
                    NormalName = "Ca Mau - Huyen Ngoc Hien",

                    KMSId = 973
                },
                new Area
                {
                    Id = 111,
                    Name = "Cao Bằng - Huyện Hòa An",
                    NormalName = "Cao Bang - Huyen Hoa An",

                    KMSId = 51
                },
                new Area
                {
                    Id = 112,
                    Name = "Cao Bằng - Huyện Hà Quảng",
                    NormalName = "Cao Bang - Huyen Ha Quang",

                    KMSId = 45
                },
                new Area
                {
                    Id = 113,
                    Name = "Cao Bằng - Thành phố Cao Bằng",
                    NormalName = "Cao Bang - Thanh pho Cao Bang",

                    KMSId = 40
                },
                new Area
                {
                    Id = 114,
                    Name = "Cao Bằng - Huyện Bảo Lâm",
                    NormalName = "Cao Bang - Huyen Bao Lam",

                    KMSId = 42
                },
                new Area
                {
                    Id = 116,
                    Name = "Cao Bằng - Huyện Trùng Khánh",
                    NormalName = "Cao Bang - Huyen Trung Khanh",

                    KMSId = 47
                },
                new Area
                {
                    Id = 117,
                    Name = "Cao Bằng - Huyện Bảo Lạc",
                    NormalName = "Cao Bang - Huyen Bao Lac",

                    KMSId = 43
                },
                new Area
                {
                    Id = 118,
                    Name = "Cao Bằng - Huyện Hạ Lang",
                    NormalName = "Cao Bang - Huyen Ha Lang",

                    KMSId = 48
                },
                new Area
                {
                    Id = 119,
                    Name = "Cao Bằng - Huyện Nguyên Bình",
                    NormalName = "Cao Bang - Huyen Nguyen Binh",

                    KMSId = 52
                },
                new Area
                {
                    Id = 121,
                    Name = "Cao Bằng - Huyện Thạch An",
                    NormalName = "Cao Bang - Huyen Thach An",

                    KMSId = 53
                },
                new Area
                {
                    Id = 738,
                    Name = "Cao Bằng - Huyện Quảng Hòa",
                    NormalName = "Cao Bang - Huyen Quang Hoa",

                    KMSId = 49
                },
                new Area
                {
                    Id = 124,
                    Name = "Cần Thơ - Quận Ninh Kiều",
                    NormalName = "Can Tho - Quan Ninh Kieu",

                    KMSId = 916
                },
                new Area
                {
                    Id = 125,
                    Name = "Cần Thơ - Quận Bình Thủy",
                    NormalName = "Can Tho - Quan Binh Thuy",

                    KMSId = 918
                },
                new Area
                {
                    Id = 126,
                    Name = "Cần Thơ - Quận Cái Răng",
                    NormalName = "Can Tho - Quan Cai Rang",

                    KMSId = 919
                },
                new Area
                {
                    Id = 127,
                    Name = "Cần Thơ - Quận Ô Môn",
                    NormalName = "Can Tho - Quan O Mon",

                    KMSId = 917
                },
                new Area
                {
                    Id = 128,
                    Name = "Cần Thơ - Quận Thốt Nốt",
                    NormalName = "Can Tho - Quan Thot Not",

                    KMSId = 923
                },
                new Area
                {
                    Id = 129,
                    Name = "Cần Thơ - Huyện Vĩnh Thạnh",
                    NormalName = "Can Tho - Huyen Vinh Thanh",

                    KMSId = 924
                },
                new Area
                {
                    Id = 130,
                    Name = "Cần Thơ - Huyện Thới Lai",
                    NormalName = "Can Tho - Huyen Thoi Lai",

                    KMSId = 927
                },
                new Area
                {
                    Id = 131,
                    Name = "Cần Thơ - Huyện Phong Điền",
                    NormalName = "Can Tho - Huyen Phong Dien",

                    KMSId = 926
                },
                new Area
                {
                    Id = 132,
                    Name = "Cần Thơ - Huyện Cờ Đỏ",
                    NormalName = "Can Tho - Huyen Co Do",

                    KMSId = 925
                },
                new Area
                {
                    Id = 134,
                    Name = "Đà Nẵng - Huyện Hòa Vang",
                    NormalName = "Da Nang - Huyen Hoa Vang",

                    KMSId = 497
                },
                new Area
                {
                    Id = 135,
                    Name = "Đà Nẵng - Quận Hải Châu",
                    NormalName = "Da Nang - Quan Hai Chau",

                    KMSId = 492
                },
                new Area
                {
                    Id = 136,
                    Name = "Đà Nẵng - Quận Thanh Khê",
                    NormalName = "Da Nang - Quan Thanh Khe",

                    KMSId = 491
                },
                new Area
                {
                    Id = 137,
                    Name = "Đà Nẵng - Quận Sơn Trà",
                    NormalName = "Da Nang - Quan Son Tra",

                    KMSId = 493
                },
                new Area
                {
                    Id = 138,
                    Name = "Đà Nẵng - Quận Ngũ Hành Sơn",
                    NormalName = "Da Nang - Quan Ngu Hanh Son",

                    KMSId = 494
                },
                new Area
                {
                    Id = 139,
                    Name = "Đà Nẵng - Quận Liên Chiểu",
                    NormalName = "Da Nang - Quan Lien Chieu",

                    KMSId = 490
                },
                new Area
                {
                    Id = 140,
                    Name = "Đà Nẵng - Quận Cẩm Lệ",
                    NormalName = "Da Nang - Quan Cam Le",

                    KMSId = 495
                },
                new Area
                {
                    Id = 141,
                    Name = "Đà Nẵng - Huyện Hoàng Sa",
                    NormalName = "Da Nang - Huyen Hoang Sa",

                    KMSId = 498
                },
                new Area
                {
                    Id = 142,
                    Name = "Đắk Lắk - Thành phố Buôn Ma Thuột",
                    NormalName = "Dak Lak - Thanh pho Buon Ma Thuot",

                    KMSId = 643
                },
                new Area
                {
                    Id = 143,
                    Name = "Đắk Lắk - Huyện Krông A Na",
                    NormalName = "Dak Lak - Huyen Krong A Na",

                    KMSId = 655
                },
                new Area
                {
                    Id = 144,
                    Name = "Đắk Lắk - Huyện Ea Kar",
                    NormalName = "Dak Lak - Huyen Ea Kar",

                    KMSId = 651
                },
                new Area
                {
                    Id = 145,
                    Name = "Đắk Lắk - Huyện Krông Pắc",
                    NormalName = "Dak Lak - Huyen Krong Pac",

                    KMSId = 654
                },
                new Area
                {
                    Id = 146,
                    Name = "Đắk Lắk - Huyện Buôn Đôn",
                    NormalName = "Dak Lak - Huyen Buon Don",

                    KMSId = 647
                },
                new Area
                {
                    Id = 147,
                    Name = "Đắk Lắk - Huyện Cư M'gar",
                    NormalName = "Dak Lak - Huyen Cu Mgar",

                    KMSId = 648
                },
                new Area
                {
                    Id = 148,
                    Name = "Đắk Lắk - Huyện Ea H'leo",
                    NormalName = "Dak Lak - Huyen Ea Hleo",

                    KMSId = 645
                },
                new Area
                {
                    Id = 149,
                    Name = "Đắk Lắk - Huyện Krông Năng",
                    NormalName = "Dak Lak - Huyen Krong Nang",

                    KMSId = 650
                },
                new Area
                {
                    Id = 150,
                    Name = "Đắk Lắk - Thị xã Buôn Hồ",
                    NormalName = "Dak Lak - Thi xa Buon Ho",

                    KMSId = 644
                },
                new Area
                {
                    Id = 151,
                    Name = "Đắk Lắk - Huyện Krông Bông",
                    NormalName = "Dak Lak - Huyen Krong Bong",

                    KMSId = 653
                },
                new Area
                {
                    Id = 152,
                    Name = "Đắk Lắk - Huyện M'Đrắk",
                    NormalName = "Dak Lak - Huyen MDrak",

                    KMSId = 652
                },
                new Area
                {
                    Id = 153,
                    Name = "Đắk Lắk - Huyện Cư Kuin",
                    NormalName = "Dak Lak - Huyen Cu Kuin",

                    KMSId = 657
                },
                new Area
                {
                    Id = 154,
                    Name = "Đắk Lắk - Huyện Ea Súp",
                    NormalName = "Dak Lak - Huyen Ea Sup",

                    KMSId = 646
                },
                new Area
                {
                    Id = 155,
                    Name = "Đắk Lắk - Huyện Krông Búk",
                    NormalName = "Dak Lak - Huyen Krong Buk",

                    KMSId = 649
                },
                new Area
                {
                    Id = 156,
                    Name = "Đắk Lắk - Huyện Lắk",
                    NormalName = "Dak Lak - Huyen Lak",

                    KMSId = 656
                },
                new Area
                {
                    Id = 157,
                    Name = "Đắk Nông - Thành phố Gia Nghĩa",
                    NormalName = "Dak Nong - Thanh pho Gia Nghia",

                    KMSId = 660
                },
                new Area
                {
                    Id = 158,
                    Name = "Đắk Nông - Huyện Đắk R'Lấp",
                    NormalName = "Dak Nong - Huyen Dak RLap",

                    KMSId = 666
                },
                new Area
                {
                    Id = 159,
                    Name = "Đắk Nông - Huyện Đăk Glong",
                    NormalName = "Dak Nong - Huyen Dak Glong",

                    KMSId = 661
                },
                new Area
                {
                    Id = 160,
                    Name = "Đắk Nông - Huyện Đắk Mil",
                    NormalName = "Dak Nong - Huyen Dak Mil",

                    KMSId = 663
                },
                new Area
                {
                    Id = 161,
                    Name = "Đắk Nông - Huyện Cư Jút",
                    NormalName = "Dak Nong - Huyen Cu Jut",

                    KMSId = 662
                },
                new Area
                {
                    Id = 162,
                    Name = "Đắk Nông - Huyện Đắk Song",
                    NormalName = "Dak Nong - Huyen Dak Song",

                    KMSId = 665
                },
                new Area
                {
                    Id = 163,
                    Name = "Đắk Nông - Huyện Krông Nô",
                    NormalName = "Dak Nong - Huyen Krong No",

                    KMSId = 664
                },
                new Area
                {
                    Id = 164,
                    Name = "Đắk Nông - Huyện Tuy Đức",
                    NormalName = "Dak Nong - Huyen Tuy Duc",

                    KMSId = 667
                },
                new Area
                {
                    Id = 165,
                    Name = "Điện Biên - Huyện Tủa Chùa",
                    NormalName = "Dien Bien - Huyen Tua Chua",

                    KMSId = 98
                },
                new Area
                {
                    Id = 166,
                    Name = "Điện Biên - Huyện Tuần Giáo",
                    NormalName = "Dien Bien - Huyen Tuan Giao",

                    KMSId = 99
                },
                new Area
                {
                    Id = 167,
                    Name = "Điện Biên - Thị xã Mường Lay",
                    NormalName = "Dien Bien - Thi xa Muong Lay",

                    KMSId = 95
                },
                new Area
                {
                    Id = 168,
                    Name = "Điện Biên - Thành phố Điện Biên Phủ",
                    NormalName = "Dien Bien - Thanh pho Dien Bien Phu",

                    KMSId = 94
                },
                new Area
                {
                    Id = 169,
                    Name = "Điện Biên - Huyện Mường Chà",
                    NormalName = "Dien Bien - Huyen Muong Cha",

                    KMSId = 97
                },
                new Area
                {
                    Id = 170,
                    Name = "Điện Biên - Huyện Mường Nhé",
                    NormalName = "Dien Bien - Huyen Muong Nhe",

                    KMSId = 96
                },
                new Area
                {
                    Id = 171,
                    Name = "Điện Biên - Huyện Điện Biên",
                    NormalName = "Dien Bien - Huyen Dien Bien",

                    KMSId = 100
                },
                new Area
                {
                    Id = 172,
                    Name = "Điện Biên - Huyện Điện Biên Đông",
                    NormalName = "Dien Bien - Huyen Dien Bien Dong",

                    KMSId = 101
                },
                new Area
                {
                    Id = 720,
                    Name = "Điện Biên - Huyện Mường Ảng",
                    NormalName = "Dien Bien - Huyen Muong Ang",

                    KMSId = 102
                },
                new Area
                {
                    Id = 721,
                    Name = "Điện Biên - Huyện Nậm Pồ",
                    NormalName = "Dien Bien - Huyen Nam Po",

                    KMSId = 103
                },
                new Area
                {
                    Id = 173,
                    Name = "Đồng Nai - Huyện Vĩnh Cửu",
                    NormalName = "Dong Nai - Huyen Vinh Cuu",

                    KMSId = 735
                },
                new Area
                {
                    Id = 174,
                    Name = "Đồng Nai - Huyện Cẩm Mỹ",
                    NormalName = "Dong Nai - Huyen Cam My",

                    KMSId = 739
                },
                new Area
                {
                    Id = 175,
                    Name = "Đồng Nai - Huyện Xuân Lộc",
                    NormalName = "Dong Nai - Huyen Xuan Loc",

                    KMSId = 741
                },
                new Area
                {
                    Id = 176,
                    Name = "Đồng Nai - Huyện Thống Nhất",
                    NormalName = "Dong Nai - Huyen Thong Nhat",

                    KMSId = 738
                },
                new Area
                {
                    Id = 177,
                    Name = "Đồng Nai - Huyện Nhơn Trạch",
                    NormalName = "Dong Nai - Huyen Nhon Trach",

                    KMSId = 742
                },
                new Area
                {
                    Id = 178,
                    Name = "Đồng Nai - Huyện Trảng Bom",
                    NormalName = "Dong Nai - Huyen Trang Bom",

                    KMSId = 737
                },
                new Area
                {
                    Id = 179,
                    Name = "Đồng Nai - Huyện Tân Phú",
                    NormalName = "Dong Nai - Huyen Tan Phu",

                    KMSId = 734
                },
                new Area
                {
                    Id = 180,
                    Name = "Đồng Nai - Thành phố Biên Hòa",
                    NormalName = "Dong Nai - Thanh pho Bien Hoa",

                    KMSId = 731
                },
                new Area
                {
                    Id = 181,
                    Name = "Đồng Nai - Huyện Long Thành",
                    NormalName = "Dong Nai - Huyen Long Thanh",

                    KMSId = 740
                },
                new Area
                {
                    Id = 182,
                    Name = "Đồng Nai - Thành phố Long Khánh",
                    NormalName = "Dong Nai - Thanh pho Long Khanh",

                    KMSId = 732
                },
                new Area
                {
                    Id = 183,
                    Name = "Đồng Nai - Huyện Định Quán",
                    NormalName = "Dong Nai - Huyen Dinh Quan",

                    KMSId = 736
                },
                new Area
                {
                    Id = 184,
                    Name = "Đồng Tháp - Huyện Tháp Mười",
                    NormalName = "Dong Thap - Huyen Thap Muoi",

                    KMSId = 872
                },
                new Area
                {
                    Id = 185,
                    Name = "Đồng Tháp - Huyện Thanh Bình",
                    NormalName = "Dong Thap - Huyen Thanh Binh",

                    KMSId = 874
                },
                new Area
                {
                    Id = 186,
                    Name = "Đồng Tháp - Huyện Tân Hồng",
                    NormalName = "Dong Thap - Huyen Tan Hong",

                    KMSId = 869
                },
                new Area
                {
                    Id = 187,
                    Name = "Đồng Tháp - Huyện Tam Nông",
                    NormalName = "Dong Thap - Huyen Tam Nong",

                    KMSId = 871
                },
                new Area
                {
                    Id = 188,
                    Name = "Đồng Tháp - Thành phố Hồng Ngự",
                    NormalName = "Dong Thap - Thanh pho Hong Ngu",

                    KMSId = 868
                },
                new Area
                {
                    Id = 189,
                    Name = "Đồng Tháp - Thành phố Cao Lãnh",
                    NormalName = "Dong Thap - Thanh pho Cao Lanh",

                    KMSId = 866
                },
                new Area
                {
                    Id = 190,
                    Name = "Đồng Tháp - Huyện Cao Lãnh",
                    NormalName = "Dong Thap - Huyen Cao Lanh",

                    KMSId = 873
                },
                new Area
                {
                    Id = 191,
                    Name = "Đồng Tháp - Huyện Lai Vung",
                    NormalName = "Dong Thap - Huyen Lai Vung",

                    KMSId = 876
                },
                new Area
                {
                    Id = 192,
                    Name = "Đồng Tháp - Thành phố Sa Đéc",
                    NormalName = "Dong Thap - Thanh pho Sa Dec",

                    KMSId = 867
                },
                new Area
                {
                    Id = 193,
                    Name = "Đồng Tháp - Huyện Lấp Vò",
                    NormalName = "Dong Thap - Huyen Lap Vo",

                    KMSId = 875
                },
                new Area
                {
                    Id = 194,
                    Name = "Đồng Tháp - Huyện Châu Thành",
                    NormalName = "Dong Thap - Huyen Chau Thanh",

                    KMSId = 877
                },
                new Area
                {
                    Id = 200,
                    Name = "Đồng Tháp - Huyện Hồng Ngự",
                    NormalName = "Dong Thap - Huyen Hong Ngu",

                    KMSId = 870
                },
                new Area
                {
                    Id = 279,
                    Name = "Hải Dương - Thành phố Chí Linh",
                    NormalName = "Hai Duong - Thanh pho Chi Linh",

                    KMSId = 290
                },
                new Area
                {
                    Id = 280,
                    Name = "Hải Dương - Huyện Nam Sách",
                    NormalName = "Hai Duong - Huyen Nam Sach",

                    KMSId = 291
                },
                new Area
                {
                    Id = 281,
                    Name = "Hải Dương - Thành phố Hải Dương",
                    NormalName = "Hai Duong - Thanh pho Hai Duong",

                    KMSId = 288
                },
                new Area
                {
                    Id = 282,
                    Name = "Hải Dương - Huyện Bình Giang",
                    NormalName = "Hai Duong - Huyen Binh Giang",

                    KMSId = 296
                },
                new Area
                {
                    Id = 283,
                    Name = "Hải Dương - Huyện Cẩm Giàng",
                    NormalName = "Hai Duong - Huyen Cam Giang",

                    KMSId = 295
                },
                new Area
                {
                    Id = 284,
                    Name = "Hải Dương - Thị xã Kinh Môn",
                    NormalName = "Hai Duong - Thi xa Kinh Mon",

                    KMSId = 292
                },
                new Area
                {
                    Id = 285,
                    Name = "Hải Dương - Huyện Kim Thành",
                    NormalName = "Hai Duong - Huyen Kim Thanh",

                    KMSId = 293
                },
                new Area
                {
                    Id = 286,
                    Name = "Hải Dương - Huyện Gia Lộc",
                    NormalName = "Hai Duong - Huyen Gia Loc",

                    KMSId = 297
                },
                new Area
                {
                    Id = 288,
                    Name = "Hải Dương - Huyện Ninh Giang",
                    NormalName = "Hai Duong - Huyen Ninh Giang",

                    KMSId = 299
                },
                new Area
                {
                    Id = 289,
                    Name = "Hải Dương - Huyện Thanh Hà",
                    NormalName = "Hai Duong - Huyen Thanh Ha",

                    KMSId = 294
                },
                new Area
                {
                    Id = 290,
                    Name = "Hải Dương - Huyện Thanh Miện",
                    NormalName = "Hai Duong - Huyen Thanh Mien",

                    KMSId = 300
                },
                new Area
                {
                    Id = 291,
                    Name = "Hải Dương - Huyện Tứ Kỳ",
                    NormalName = "Hai Duong - Huyen Tu Ky",

                    KMSId = 298
                },
                new Area
                {
                    Id = 292,
                    Name = "Hải Phòng - Quận Dương Kinh",
                    NormalName = "Hai Phong - Quan Duong Kinh",

                    KMSId = 309
                },
                new Area
                {
                    Id = 293,
                    Name = "Hải Phòng - Huyện An Dương",
                    NormalName = "Hai Phong - Huyen An Duong",

                    KMSId = 312
                },
                new Area
                {
                    Id = 294,
                    Name = "Hải Phòng - Huyện An Lão",
                    NormalName = "Hai Phong - Huyen An Lao",

                    KMSId = 313
                },
                new Area
                {
                    Id = 295,
                    Name = "Hải Phòng - Huyện Tiên Lãng",
                    NormalName = "Hai Phong - Huyen Tien Lang",

                    KMSId = 315
                },
                new Area
                {
                    Id = 296,
                    Name = "Hải Phòng - Huyện Vĩnh Bảo",
                    NormalName = "Hai Phong - Huyen Vinh Bao",

                    KMSId = 316
                },
                new Area
                {
                    Id = 297,
                    Name = "Hải Phòng - Quận Ngô Quyền",
                    NormalName = "Hai Phong - Quan Ngo Quyen",

                    KMSId = 304
                },
                new Area
                {
                    Id = 298,
                    Name = "Hải Phòng - Quận Lê Chân",
                    NormalName = "Hai Phong - Quan Le Chan",

                    KMSId = 305
                },
                new Area
                {
                    Id = 299,
                    Name = "Hải Phòng - Quận Kiến An",
                    NormalName = "Hai Phong - Quan Kien An",

                    KMSId = 307
                },
                new Area
                {
                    Id = 300,
                    Name = "Hải Phòng - Quận Hải An",
                    NormalName = "Hai Phong - Quan Hai An",

                    KMSId = 306
                },
                new Area
                {
                    Id = 301,
                    Name = "Hải Phòng - Quận Đồ Sơn",
                    NormalName = "Hai Phong - Quan Do Son",

                    KMSId = 308
                },
                new Area
                {
                    Id = 302,
                    Name = "Hải Phòng - Quận Hồng Bàng",
                    NormalName = "Hai Phong - Quan Hong Bang",

                    KMSId = 303
                },
                new Area
                {
                    Id = 303,
                    Name = "Hải Phòng - Huyện Thủy Nguyên",
                    NormalName = "Hai Phong - Huyen Thuy Nguyen",

                    KMSId = 311
                },
                new Area
                {
                    Id = 304,
                    Name = "Hải Phòng - Huyện Bạch Long Vĩ",
                    NormalName = "Hai Phong - Huyen Bach Long Vi",

                    KMSId = 318
                },
                new Area
                {
                    Id = 305,
                    Name = "Hải Phòng - Huyện Cát Hải",
                    NormalName = "Hai Phong - Huyen Cat Hai",

                    KMSId = 317
                },
                new Area
                {
                    Id = 306,
                    Name = "Hải Phòng - Huyện Kiến Thụy",
                    NormalName = "Hai Phong - Huyen Kien Thuy",

                    KMSId = 314
                },
                new Area
                {
                    Id = 307,
                    Name = "Hậu Giang - Huyện Châu Thành A",
                    NormalName = "Hau Giang - Huyen Chau Thanh A",

                    KMSId = 932
                },
                new Area
                {
                    Id = 308,
                    Name = "Hậu Giang - Thành phố Ngã Bảy",
                    NormalName = "Hau Giang - Thanh pho Nga Bay",

                    KMSId = 931
                },
                new Area
                {
                    Id = 309,
                    Name = "Hậu Giang - Huyện Phụng Hiệp",
                    NormalName = "Hau Giang - Huyen Phung Hiep",

                    KMSId = 934
                },
                new Area
                {
                    Id = 310,
                    Name = "Hậu Giang - Thành phố Vị Thanh",
                    NormalName = "Hau Giang - Thanh pho Vi Thanh",

                    KMSId = 930
                },
                new Area
                {
                    Id = 311,
                    Name = "Hậu Giang - Thị Xã Long Mỹ",
                    NormalName = "Hau Giang - Thi Xa Long My",

                    KMSId = 937
                },
                new Area
                {
                    Id = 312,
                    Name = "Hậu Giang - Huyện Vị Thủy",
                    NormalName = "Hau Giang - Huyen Vi Thuy",

                    KMSId = 935
                },
                new Area
                {
                    Id = 313,
                    Name = "Hậu Giang - Huyện Châu Thành",
                    NormalName = "Hau Giang - Huyen Chau Thanh",

                    KMSId = 933
                },
                new Area
                {
                    Id = 736,
                    Name = "Hậu Giang - Huyện Long Mỹ",
                    NormalName = "Hau Giang - Huyen Long My",

                    KMSId = 936
                },
                new Area
                {
                    Id = 314,
                    Name = "Hòa Bình - Huyện Tân Lạc",
                    NormalName = "Hoa Binh - Huyen Tan Lac",

                    KMSId = 155
                },
                new Area
                {
                    Id = 315,
                    Name = "Hòa Bình - Thành phố Hòa Bình",
                    NormalName = "Hoa Binh - Thanh pho Hoa Binh",

                    KMSId = 148
                },
                new Area
                {
                    Id = 316,
                    Name = "Hòa Bình - Huyện Lương Sơn",
                    NormalName = "Hoa Binh - Huyen Luong Son",

                    KMSId = 152
                },
                new Area
                {
                    Id = 318,
                    Name = "Hòa Bình - Huyện Đà Bắc",
                    NormalName = "Hoa Binh - Huyen Da Bac",

                    KMSId = 150
                },
                new Area
                {
                    Id = 319,
                    Name = "Hòa Bình - Huyện Cao Phong",
                    NormalName = "Hoa Binh - Huyen Cao Phong",

                    KMSId = 154
                },
                new Area
                {
                    Id = 320,
                    Name = "Hòa Bình - Huyện Kim Bôi",
                    NormalName = "Hoa Binh - Huyen Kim Boi",

                    KMSId = 153
                },
                new Area
                {
                    Id = 321,
                    Name = "Hòa Bình - Huyện Lạc Sơn",
                    NormalName = "Hoa Binh - Huyen Lac Son",

                    KMSId = 157
                },
                new Area
                {
                    Id = 322,
                    Name = "Hòa Bình - Huyện Lạc Thủy",
                    NormalName = "Hoa Binh - Huyen Lac Thuy",

                    KMSId = 159
                },
                new Area
                {
                    Id = 323,
                    Name = "Hòa Bình - Huyện Mai Châu",
                    NormalName = "Hoa Binh - Huyen Mai Chau",

                    KMSId = 156
                },
                new Area
                {
                    Id = 324,
                    Name = "Hòa Bình - Huyện Yên Thủy",
                    NormalName = "Hoa Binh - Huyen Yen Thuy",

                    KMSId = 158
                },
                new Area
                {
                    Id = 350,
                    Name = "Hưng Yên - Huyện Ân Thi",
                    NormalName = "Hung Yen - Huyen An Thi",

                    KMSId = 329
                },
                new Area
                {
                    Id = 351,
                    Name = "Hưng Yên - Huyện Khoái Châu",
                    NormalName = "Hung Yen - Huyen Khoai Chau",

                    KMSId = 330
                },
                new Area
                {
                    Id = 352,
                    Name = "Hưng Yên - Thị xã Mỹ Hào",
                    NormalName = "Hung Yen - Thi xa My Hao",

                    KMSId = 328
                },
                new Area
                {
                    Id = 353,
                    Name = "Hưng Yên - Huyện Yên Mỹ",
                    NormalName = "Hung Yen - Huyen Yen My",

                    KMSId = 327
                },
                new Area
                {
                    Id = 354,
                    Name = "Hưng Yên - Thành phố Hưng Yên",
                    NormalName = "Hung Yen - Thanh pho Hung Yen",

                    KMSId = 323
                },
                new Area
                {
                    Id = 355,
                    Name = "Hưng Yên - Huyện Kim Động",
                    NormalName = "Hung Yen - Huyen Kim Dong",

                    KMSId = 331
                },
                new Area
                {
                    Id = 356,
                    Name = "Hưng Yên - Huyện Văn Giang",
                    NormalName = "Hung Yen - Huyen Van Giang",

                    KMSId = 326
                },
                new Area
                {
                    Id = 357,
                    Name = "Hưng Yên - Huyện Văn Lâm",
                    NormalName = "Hung Yen - Huyen Van Lam",

                    KMSId = 325
                },
                new Area
                {
                    Id = 358,
                    Name = "Hưng Yên - Huyện Phù Cừ",
                    NormalName = "Hung Yen - Huyen Phu Cu",

                    KMSId = 333
                },
                new Area
                {
                    Id = 359,
                    Name = "Hưng Yên - Huyện Tiên Lữ",
                    NormalName = "Hung Yen - Huyen Tien Lu",

                    KMSId = 332
                },
                new Area
                {
                    Id = 384,
                    Name = "Khánh Hòa - Thị xã Ninh Hòa",
                    NormalName = "Khanh Hoa - Thi xa Ninh Hoa",

                    KMSId = 572
                },
                new Area
                {
                    Id = 385,
                    Name = "Khánh Hòa - Thành phố Cam Ranh",
                    NormalName = "Khanh Hoa - Thanh pho Cam Ranh",

                    KMSId = 569
                },
                new Area
                {
                    Id = 386,
                    Name = "Khánh Hòa - Thành phố Nha Trang",
                    NormalName = "Khanh Hoa - Thanh pho Nha Trang",

                    KMSId = 568
                },
                new Area
                {
                    Id = 387,
                    Name = "Khánh Hòa - Huyện Cam Lâm",
                    NormalName = "Khanh Hoa - Huyen Cam Lam",

                    KMSId = 570
                },
                new Area
                {
                    Id = 388,
                    Name = "Khánh Hòa - Huyện Diên Khánh",
                    NormalName = "Khanh Hoa - Huyen Dien Khanh",

                    KMSId = 574
                },
                new Area
                {
                    Id = 389,
                    Name = "Khánh Hòa - Huyện Vạn Ninh",
                    NormalName = "Khanh Hoa - Huyen Van Ninh",

                    KMSId = 571
                },
                new Area
                {
                    Id = 390,
                    Name = "Khánh Hòa - Huyện Khánh Sơn",
                    NormalName = "Khanh Hoa - Huyen Khanh Son",

                    KMSId = 575
                },
                new Area
                {
                    Id = 391,
                    Name = "Khánh Hòa - Huyện Khánh Vĩnh",
                    NormalName = "Khanh Hoa - Huyen Khanh Vinh",

                    KMSId = 573
                },
                new Area
                {
                    Id = 392,
                    Name = "Khánh Hòa - Huyện Trường Sa",
                    NormalName = "Khanh Hoa - Huyen Truong Sa",

                    KMSId = 576
                },
                new Area
                {
                    Id = 360,
                    Name = "Kiên Giang - Thành phố Hà Tiên",
                    NormalName = "Kien Giang - Thanh pho Ha Tien",

                    KMSId = 900
                },
                new Area
                {
                    Id = 361,
                    Name = "Kiên Giang - Huyện Hòn Đất",
                    NormalName = "Kien Giang - Huyen Hon Dat",

                    KMSId = 903
                },
                new Area
                {
                    Id = 362,
                    Name = "Kiên Giang - Huyện Kiên Lương",
                    NormalName = "Kien Giang - Huyen Kien Luong",

                    KMSId = 902
                },
                new Area
                {
                    Id = 363,
                    Name = "Kiên Giang - Huyện An Minh",
                    NormalName = "Kien Giang - Huyen An Minh",

                    KMSId = 909
                },
                new Area
                {
                    Id = 364,
                    Name = "Kiên Giang - Huyện Châu Thành",
                    NormalName = "Kien Giang - Huyen Chau Thanh",

                    KMSId = 905
                },
                new Area
                {
                    Id = 365,
                    Name = "Kiên Giang - Huyện Tân Hiệp",
                    NormalName = "Kien Giang - Huyen Tan Hiep",

                    KMSId = 904
                },
                new Area
                {
                    Id = 366,
                    Name = "Kiên Giang - Huyện Giồng Riềng",
                    NormalName = "Kien Giang - Huyen Giong Rieng",

                    KMSId = 906
                },
                new Area
                {
                    Id = 367,
                    Name = "Kiên Giang - Huyện An Biên",
                    NormalName = "Kien Giang - Huyen An Bien",

                    KMSId = 908
                },
                new Area
                {
                    Id = 368,
                    Name = "Kiên Giang - Thành phố Rạch Giá",
                    NormalName = "Kien Giang - Thanh pho Rach Gia",

                    KMSId = 899
                },
                new Area
                {
                    Id = 369,
                    Name = "Kiên Giang - Huyện Gò Quao",
                    NormalName = "Kien Giang - Huyen Go Quao",

                    KMSId = 907
                },
                new Area
                {
                    Id = 370,
                    Name = "Kiên Giang - Huyện Giang Thành",
                    NormalName = "Kien Giang - Huyen Giang Thanh",

                    KMSId = 914
                },
                new Area
                {
                    Id = 371,
                    Name = "Kiên Giang - Huyện Kiên Hải",
                    NormalName = "Kien Giang - Huyen Kien Hai",

                    KMSId = 912
                },
                new Area
                {
                    Id = 372,
                    Name = "Kiên Giang - Thành phố Phú Quốc",
                    NormalName = "Kien Giang - Thanh pho Phu Quoc",

                    KMSId = 911
                },
                new Area
                {
                    Id = 373,
                    Name = "Kiên Giang - Huyện Vĩnh Thuận",
                    NormalName = "Kien Giang - Huyen Vinh Thuan",

                    KMSId = 910
                },
                new Area
                {
                    Id = 374,
                    Name = "Kiên Giang - Huyện U Minh Thượng",
                    NormalName = "Kien Giang - Huyen U Minh Thuong",

                    KMSId = 913
                },
                new Area
                {
                    Id = 375,
                    Name = "Kon Tum - Huyện Kon Plông",
                    NormalName = "Kon Tum - Huyen Kon Plong",

                    KMSId = 613
                },
                new Area
                {
                    Id = 376,
                    Name = "Kon Tum - Huyện Đắk Hà",
                    NormalName = "Kon Tum - Huyen Dak Ha",

                    KMSId = 615
                },
                new Area
                {
                    Id = 377,
                    Name = "Kon Tum - Huyện Đắk Glei",
                    NormalName = "Kon Tum - Huyen Dak Glei",

                    KMSId = 610
                },
                new Area
                {
                    Id = 378,
                    Name = "Kon Tum - Thành phố Kon Tum",
                    NormalName = "Kon Tum - Thanh pho Kon Tum",

                    KMSId = 608
                },
                new Area
                {
                    Id = 379,
                    Name = "Kon Tum - Huyện Đắk Tô",
                    NormalName = "Kon Tum - Huyen Dak To",

                    KMSId = 612
                },
                new Area
                {
                    Id = 380,
                    Name = "Kon Tum - Huyện Kon Rẫy",
                    NormalName = "Kon Tum - Huyen Kon Ray",

                    KMSId = 614
                },
                new Area
                {
                    Id = 381,
                    Name = "Kon Tum - Huyện Ngọc Hồi",
                    NormalName = "Kon Tum - Huyen Ngoc Hoi",

                    KMSId = 611
                },
                new Area
                {
                    Id = 382,
                    Name = "Kon Tum - Huyện Sa Thầy",
                    NormalName = "Kon Tum - Huyen Sa Thay",

                    KMSId = 616
                },
                new Area
                {
                    Id = 383,
                    Name = "Kon Tum - Huyện Tu Mơ Rông",
                    NormalName = "Kon Tum - Huyen Tu Mo Rong",

                    KMSId = 617
                },
                new Area
                {
                    Id = 730,
                    Name = "Kon Tum - Huyện Ia H' Drai",
                    NormalName = "Kon Tum - Huyen Ia HDrai",

                    KMSId = 618
                },
                new Area
                {
                    Id = 3859,
                    Name = "Xã Cao Thắng",
                    NormalName = "Xa Cao Thang",

                    ParentId = 290,
                    PharmacyWardId = 1301,
                    KMSId = 11278
                },
                new Area
                {
                    Id = 3860,
                    Name = "Xã Chi Lăng Bắc",
                    NormalName = "Xa Chi Lang Bac",
                    ParentId = 290,
                    PharmacyWardId = 1001,
                    KMSId = 11281
                },
                new Area
                {
                    Id = 3861,
                    Name = "Xã Chi Lăng Nam",
                    NormalName = "Xa Chi Lang Nam",

                    ParentId = 290,
                    PharmacyWardId = 1001,
                    KMSId = 11284
                },
                new Area
                {
                    Id = 3854,
                    Name = "Xã Đoàn Kết",
                    NormalName = "Xa Doan Ket",

                    ParentId = 290,
                    PharmacyWardId = 1308,
                    KMSId = 11263
                },
                new Area
                {
                    Id = 3850,
                    Name = "Xã Đoàn Tùng",
                    NormalName = "Xa Doan Tung",

                    ParentId = 290,
                    PharmacyWardId = 1304,
                    KMSId = 11251
                },
                new Area
                {
                    Id = 3863,
                    Name = "Xã Hồng Phong",
                    NormalName = "Xa Hong Phong",

                    ParentId = 290,
                    PharmacyWardId = 1295,
                    KMSId = 11293
                },
                new Area
                {
                    Id = 3851,
                    Name = "Xã Hồng Quang",
                    NormalName = "Xa Hong Quang",

                    ParentId = 290,
                    PharmacyWardId = 1454,
                    KMSId = 11254
                },
                new Area
                {
                    Id = 3853,
                    Name = "Xã Lam Sơn",
                    NormalName = "Xa Lam Son",

                    ParentId = 290,
                    PharmacyWardId = 613,
                    KMSId = 11260
                },
                new Area
                {
                    Id = 3855,
                    Name = "Xã Lê Hồng",
                    NormalName = "Xa Le Hong",

                    ParentId = 290,
                    PharmacyWardId = 1307,
                    KMSId = 11266
                },
                new Area
                {
                    Id = 3849,
                    Name = "Xã Ngô Quyền",
                    NormalName = "Xa Ngo Quyen",

                    ParentId = 290,
                    PharmacyWardId = 815,
                    KMSId = 11248
                },
                new Area
                {
                    Id = 3858,
                    Name = "Xã Ngũ Hùng",
                    NormalName = "Xa Ngu Hung",

                    ParentId = 290,
                    PharmacyWardId = 1299,
                    KMSId = 11275
                },
                new Area
                {
                    Id = 3848,
                    Name = "Xã Phạm Kha",
                    NormalName = "Xa Pham Kha",

                    ParentId = 290,
                    PharmacyWardId = 1302,
                    KMSId = 11245
                },
                new Area
                {
                    Id = 3852,
                    Name = "Xã Tân Trào",
                    NormalName = "Xa Tan Trao",

                    ParentId = 290,
                    PharmacyWardId = 740,
                    KMSId = 11257
                },
                new Area
                {
                    Id = 3856,
                    Name = "Xã Tứ Cường",
                    NormalName = "Xa Tu Cuong",

                    ParentId = 290,
                    PharmacyWardId = 747,
                    KMSId = 11269
                },
                new Area
                {
                    Id = 3862,
                    Name = "Xã Thanh Giang",
                    NormalName = "Xa Thanh Giang",

                    ParentId = 290,
                    PharmacyWardId = 1294,
                    KMSId = 11287
                },
                new Area
                {
                    Id = 3847,
                    Name = "Xã Thanh Tùng",
                    NormalName = "Xa Thanh Tung",

                    ParentId = 290,
                    PharmacyWardId = 1306,
                    KMSId = 11242
                },
                new Area
                {
                    Id = 3625,
                    Name = "Xã Bắc An",
                    NormalName = "Xa Bac An",
                    ParentId = 279,
                    PharmacyWardId = 1101,
                    KMSId = 10558
                },
                new Area
                {
                    Id = 3624,
                    Name = "Xã Hoàng Hoa Thám",
                    NormalName = "Xa Hoang Hoa Tham",
                    ParentId = 279,
                    PharmacyWardId = 1444,
                    KMSId = 10555
                },
                new Area
                {
                    Id = 3626,
                    Name = "Xã Hưng Đạo",
                    NormalName = "Xa Hung Dao",
                    ParentId = 279,
                    PharmacyWardId = 1233,
                    KMSId = 10561
                },
                new Area
                {
                    Id = 3627,
                    Name = "Xã Lê Lợi",
                    NormalName = "Xa Le Loi",
                    ParentId = 279,
                    PharmacyWardId = 605,
                    KMSId = 10564
                },
                new Area
                {
                    Id = 3636,
                    Name = "Xã Nhân Huệ",
                    NormalName = "Xa Nhan Hue",
                    ParentId = 279,
                    PharmacyWardId = 1107,
                    KMSId = 10591
                },
                new Area
                {
                    Id = 3637,
                    Name = "Phường An Lạc",
                    NormalName = "Phuong An Lac",
                    ParentId = 279,
                    PharmacyWardId = 1113,
                    KMSId = 10594
                },
                new Area
                {
                    Id = 3623,
                    Name = "Phường Bến Tắm",
                    NormalName = "Phuong Ben Tam",
                    ParentId = 279,
                    PharmacyWardId = 1097,
                    KMSId = 10552
                },
                new Area
                {
                    Id = 3631,
                    Name = "Phường Cổ Thành",
                    NormalName = "Phuong Co Thanh",
                    ParentId = 279,
                    PharmacyWardId = 1103,
                    KMSId = 10576
                },
                new Area
                {
                    Id = 3629,
                    Name = "Phường Cộng Hòa",
                    NormalName = "Phuong Cong Hoa",
                    ParentId = 279,
                    PharmacyWardId = 394,
                    KMSId = 10570
                },
                new Area
                {
                    Id = 3633,
                    Name = "Phường Chí Minh",
                    NormalName = "Phuong Chi Minh",
                    ParentId = 279,
                    PharmacyWardId = 1109,
                    KMSId = 10582
                },
                new Area
                {
                    Id = 3639,
                    Name = "Phường Đồng Lạc",
                    NormalName = "Phuong Dong Lac",
                    ParentId = 279,
                    PharmacyWardId = 375,
                    KMSId = 10600
                },
                new Area
                {
                    Id = 3630,
                    Name = "Phường Hoàng Tân",
                    NormalName = "Phuong Hoang Tan",

                    ParentId = 279,
                    PharmacyWardId = 1105,
                    KMSId = 10573
                },
                new Area
                {
                    Id = 3628,
                    Name = "Phường Hoàng Tiến",
                    NormalName = "Phuong Hoang Tien",

                    ParentId = 279,
                    PharmacyWardId = 1106,
                    KMSId = 10567
                },
                new Area
                {
                    Id = 3621,
                    Name = "Phường Phả Lại",
                    NormalName = "Phuong Pha Lai",

                    ParentId = 279,
                    PharmacyWardId = 1098,
                    KMSId = 10546
                },
                new Area
                {
                    Id = 3622,
                    Name = "Phường Sao Đỏ",
                    NormalName = "Phuong Sao Do",

                    ParentId = 279,
                    PharmacyWardId = 1096,
                    KMSId = 10549
                },
                new Area
                {
                    Id = 3640,
                    Name = "Phường Tân Dân",
                    NormalName = "Phuong Tan Dan",

                    ParentId = 279,
                    PharmacyWardId = 130,
                    KMSId = 10603
                },
                new Area
                {
                    Id = 3635,
                    Name = "Phường Thái Học",
                    NormalName = "Phuong Thai Hoc",

                    ParentId = 279,
                    PharmacyWardId = 1110,
                    KMSId = 10588
                },
                new Area
                {
                    Id = 3632,
                    Name = "Phường Văn An",
                    NormalName = "Phuong Van An",

                    ParentId = 279,
                    PharmacyWardId = 961,
                    KMSId = 10579
                },
                new Area
                {
                    Id = 3634,
                    Name = "Phường Văn Đức",
                    NormalName = "Phuong Van Duc",

                    ParentId = 279,
                    PharmacyWardId = 194,
                    KMSId = 10585
                },
                new Area
                {
                    Id = 3649,
                    Name = "Xã An Bình",
                    NormalName = "Xa An Binh",

                    ParentId = 280,
                    PharmacyWardId = 1034,
                    KMSId = 10630
                },
                new Area
                {
                    Id = 3654,
                    Name = "Xã An Lâm",
                    NormalName = "Xa An Lam",

                    ParentId = 280,
                    PharmacyWardId = 1130,
                    KMSId = 10645
                },
                new Area
                {
                    Id = 3651,
                    Name = "Xã An Sơn",
                    NormalName = "Xa An Son",

                    ParentId = 280,
                    PharmacyWardId = 651,
                    KMSId = 10636
                },
                new Area
                {
                    Id = 3652,
                    Name = "Xã Cộng Hòa",
                    NormalName = "Xa Cong Hoa",

                    ParentId = 280,
                    PharmacyWardId = 394,
                    KMSId = 10639
                },
                new Area
                {
                    Id = 3658,
                    Name = "Xã Đồng Lạc",
                    NormalName = "Xa Dong Lac",

                    ParentId = 280,
                    PharmacyWardId = 375,
                    KMSId = 10657
                },
                new Area
                {
                    Id = 3645,
                    Name = "Xã Hiệp Cát",
                    NormalName = "Xa Hiep Cat",

                    ParentId = 280,
                    PharmacyWardId = 1122,
                    KMSId = 10618
                },
                new Area
                {
                    Id = 3657,
                    Name = "Xã Hồng Phong",
                    NormalName = "Xa Hong Phong",

                    ParentId = 280,
                    PharmacyWardId = 422,
                    KMSId = 10654
                },
                new Area
                {
                    Id = 3644,
                    Name = "Xã Hợp Tiến",
                    NormalName = "Xa Hop Tien",

                    ParentId = 280,
                    PharmacyWardId = 498,
                    KMSId = 10615
                },
                new Area
                {
                    Id = 3659,
                    Name = "Xã Minh Tân",
                    NormalName = "Xa Minh Tan",

                    ParentId = 280,
                    PharmacyWardId = 557,
                    KMSId = 10666
                },
                new Area
                {
                    Id = 3648,
                    Name = "Xã Nam Chính",
                    NormalName = "Xa Nam Chinh",

                    ParentId = 280,
                    PharmacyWardId = 1123,
                    KMSId = 10627
                },
                new Area
                {
                    Id = 3656,
                    Name = "Xã Nam Hồng",
                    NormalName = "Xa Nam Hong",

                    ParentId = 280,
                    PharmacyWardId = 161,
                    KMSId = 10651
                },
                new Area
                {
                    Id = 3642,
                    Name = "Xã Nam Hưng",
                    NormalName = "Xa Nam Hung",

                    ParentId = 280,
                    PharmacyWardId = 761,
                    KMSId = 10609
                },
                new Area
                {
                    Id = 3643,
                    Name = "Xã Nam Tân",
                    NormalName = "Xa Nam Tan",

                    ParentId = 280,
                    PharmacyWardId = 1118,
                    KMSId = 10612
                },
                new Area
                {
                    Id = 3650,
                    Name = "Xã Nam Trung",
                    NormalName = "Xa Nam Trung",

                    ParentId = 280,
                    PharmacyWardId = 1124,
                    KMSId = 10633
                },
                new Area
                {
                    Id = 3655,
                    Name = "Xã Phú Điền",
                    NormalName = "Xa Phu Dien",

                    ParentId = 280,
                    PharmacyWardId = 202,
                    KMSId = 10648
                },
                new Area
                {
                    Id = 3647,
                    Name = "Xã Quốc Tuấn",
                    NormalName = "Xa Quoc Tuan",

                    ParentId = 280,
                    PharmacyWardId = 698,
                    KMSId = 10624
                },
                new Area
                {
                    Id = 3653,
                    Name = "Xã Thái Tân",
                    NormalName = "Xa Thai Tan",

                    ParentId = 280,
                    PharmacyWardId = 1085,
                    KMSId = 10642
                },
                new Area
                {
                    Id = 3646,
                    Name = "Xã Thanh Quang",
                    NormalName = "Xa Thanh Quang",

                    ParentId = 280,
                    PharmacyWardId = 1119,
                    KMSId = 10621
                },
                new Area
                {
                    Id = 3615,
                    Name = "Phường Ái Quốc",
                    NormalName = "Phuong Ai Quoc",

                    ParentId = 281,
                    PharmacyWardId = 1091,
                    KMSId = 10660
                },
                new Area
                {
                    Id = 3601,
                    Name = "Phường Bình Hàn",
                    NormalName = "Phuong Binh Han",

                    ParentId = 281,
                    PharmacyWardId = 1082,
                    KMSId = 10510
                },
                new Area
                {
                    Id = 3600,
                    Name = "Phường Cẩm Thượng",
                    NormalName = "Phuong Cam Thuong",

                    ParentId = 281,
                    PharmacyWardId = 285,
                    KMSId = 10507
                },
                new Area
                {
                    Id = 3612,
                    Name = "Phường Hải Tân",
                    NormalName = "Phuong Hai Tan",

                    ParentId = 281,
                    PharmacyWardId = 1085,
                    KMSId = 10537
                },
                new Area
                {
                    Id = 3611,
                    Name = "Phường Lê Thanh Nghị",
                    NormalName = "Phuong Le Thanh Nghi",

                    ParentId = 281,
                    PharmacyWardId = 488,
                    KMSId = 10534
                },
                new Area
                {
                    Id = 3618,
                    Name = "Phường Nam Đồng",
                    NormalName = "Phuong Nam Dong",

                    ParentId = 281,
                    PharmacyWardId = 1090,
                    KMSId = 10672
                },
                new Area
                {
                    Id = 3602,
                    Name = "Phường Ngọc Châu",
                    NormalName = "Phuong Ngoc Chau",

                    ParentId = 281,
                    PharmacyWardId = 1083,
                    KMSId = 10513
                },
                new Area
                {
                    Id = 3605,
                    Name = "Phường Nguyễn Trãi",
                    NormalName = "Phuong Nguyen Trai",

                    ParentId = 281,
                    PharmacyWardId = 1449,
                    KMSId = 10519
                },
                new Area
                {
                    Id = 3603,
                    Name = "Phường Nhị Châu",
                    NormalName = "Phuong Nhi Chau",

                    ParentId = 281,
                    PharmacyWardId = 1094,
                    KMSId = 10514
                },
                new Area
                {
                    Id = 3606,
                    Name = "Phường Phạm Ngũ Lão",
                    NormalName = "Phuong Pham Ngu Lao",

                    ParentId = 281,
                    PharmacyWardId = 1459,
                    KMSId = 10522
                },
                new Area
                {
                    Id = 3604,
                    Name = "Phường Quang Trung",
                    NormalName = "Phuong Quang Trung",

                    ParentId = 281,
                    PharmacyWardId = 539,
                    KMSId = 10516
                },
                new Area
                {
                    Id = 3610,
                    Name = "Phường Tân Bình",
                    NormalName = "Phuong Tan Binh",

                    ParentId = 281,
                    PharmacyWardId = 1034,
                    KMSId = 10532
                },
                new Area
                {
                    Id = 3620,
                    Name = "Phường Tân Hưng",
                    NormalName = "Phuong Tan Hung",

                    ParentId = 281,
                    PharmacyWardId = 145,
                    KMSId = 11011
                },
                new Area
                {
                    Id = 3613,
                    Name = "Phường Tứ Minh",
                    NormalName = "Phuong Tu Minh",

                    ParentId = 281,
                    PharmacyWardId = 517,
                    KMSId = 10540
                },
                new Area
                {
                    Id = 3619,
                    Name = "Phường Thạch Khôi",
                    NormalName = "Phuong Thach Khoi",

                    ParentId = 281,
                    PharmacyWardId = 1093,
                    KMSId = 11002
                },
                new Area
                {
                    Id = 3609,
                    Name = "Phường Thanh Bình",
                    NormalName = "Phuong Thanh Binh",

                    ParentId = 281,
                    PharmacyWardId = 407,
                    KMSId = 10531
                },
                new Area
                {
                    Id = 3607,
                    Name = "Phường Trần Hưng Đạo",
                    NormalName = "Phuong Tran Hung Dao",

                    ParentId = 281,
                    PharmacyWardId = 1233,
                    KMSId = 10525
                },
                new Area
                {
                    Id = 3608,
                    Name = "Phường Trần Phú",
                    NormalName = "Phuong Tran Phu",

                    ParentId = 281,
                    PharmacyWardId = 1166,
                    KMSId = 10528
                },
                new Area
                {
                    Id = 3614,
                    Name = "Phường Việt Hòa",
                    NormalName = "Phuong Viet Hoa",

                    ParentId = 281,
                    PharmacyWardId = 1427,
                    KMSId = 10543
                },
                new Area
                {
                    Id = 11225,
                    Name = "Xã An Thượng",
                    NormalName = "Xa An Thuong",

                    ParentId = 281,
                    KMSId = 10663
                },
                new Area
                {
                    Id = 11229,
                    Name = "Xã Gia Xuyên",
                    NormalName = "Xa Gia Xuyen",

                    ParentId = 281,
                    KMSId = 11017
                },
                new Area
                {
                    Id = 11228,
                    Name = "Xã Liên Hồng",
                    NormalName = "Xa Lien Hong",

                    ParentId = 281,
                    KMSId = 11005
                },
                new Area
                {
                    Id = 11230,
                    Name = "Xã Ngọc Sơn",
                    NormalName = "Xa Ngoc Son",

                    ParentId = 281,
                    KMSId = 11077
                },
                new Area
                {
                    Id = 11226,
                    Name = "Xã Quyết Thắng",
                    NormalName = "Xa Quyet Thang",

                    ParentId = 281,
                    KMSId = 10822
                },
                new Area
                {
                    Id = 11227,
                    Name = "Xã Tiền Tiến",
                    NormalName = "Xa Tien Tien",

                    ParentId = 281,
                    KMSId = 10837
                },

                new Area
                {
                    Id = 3760,
                    Name = "Xã Bình Minh",
                    NormalName = "Xa Binh Minh",

                    ParentId = 282,
                    PharmacyWardId = 1412,
                    KMSId = 10975
                },
                new Area
                {
                    Id = 3767,
                    Name = "Xã Bình Xuyên",
                    NormalName = "Xa Binh Xuyen",

                    ParentId = 282,
                    PharmacyWardId = 1286,
                    KMSId = 10996
                },
                new Area
                {
                    Id = 3763,
                    Name = "Xã Cổ Bi",
                    NormalName = "Xa Co Bi",

                    ParentId = 282,
                    PharmacyWardId = 189,
                    KMSId = 10984
                },
                new Area
                {
                    Id = 3761,
                    Name = "Xã Hồng Khê",
                    NormalName = "Xa Hong Khe",

                    ParentId = 282,
                    PharmacyWardId = 1283,
                    KMSId = 10978
                },
                new Area
                {
                    Id = 3753,
                    Name = "Xã Hùng Thắng",
                    NormalName = "Xa Hung Thang",

                    ParentId = 282,
                    PharmacyWardId = 762,
                    KMSId = 10954
                },
                new Area
                {
                    Id = 3756,
                    Name = "Xã Long Xuyên",
                    NormalName = "Xa Long Xuyen",

                    ParentId = 282,
                    PharmacyWardId = 1164,
                    KMSId = 10963
                },
                new Area
                {
                    Id = 3764,
                    Name = "Xã Nhân Quyền",
                    NormalName = "Xa Nhan Quyen",

                    ParentId = 282,
                    PharmacyWardId = 1285,
                    KMSId = 10987
                },
                new Area
                {
                    Id = 3759,
                    Name = "Xã Tân Hồng",
                    NormalName = "Xa Tan Hong",

                    ParentId = 282,
                    PharmacyWardId = 1024,
                    KMSId = 10972
                },
                new Area
                {
                    Id = 3757,
                    Name = "Xã Tân Việt",
                    NormalName = "Xa Tan Viet",

                    ParentId = 282,
                    PharmacyWardId = 1398,
                    KMSId = 10966
                },
                new Area
                {
                    Id = 3765,
                    Name = "Xã Thái Dương",
                    NormalName = "Xa Thai Duong",

                    ParentId = 282,
                    PharmacyWardId = 1291,
                    KMSId = 10990
                },
                new Area
                {
                    Id = 3766,
                    Name = "Xã Thái Hòa",
                    NormalName = "Xa Thai Hoa",

                    ParentId = 282,
                    PharmacyWardId = 835,
                    KMSId = 10993
                },
                new Area
                {
                    Id = 3762,
                    Name = "Xã Thái Học",
                    NormalName = "Xa Thai Hoc",

                    ParentId = 282,
                    PharmacyWardId = 1110,
                    KMSId = 10981
                },
                new Area
                {
                    Id = 3758,
                    Name = "Xã Thúc Kháng",
                    NormalName = "Xa Thuc Khang",

                    ParentId = 282,
                    PharmacyWardId = 1292,
                    KMSId = 10969
                },
                new Area
                {
                    Id = 3755,
                    Name = "Xã Vĩnh Hồng",
                    NormalName = "Xa Vinh Hong",

                    ParentId = 282,
                    PharmacyWardId = 1279,
                    KMSId = 10960
                },
                new Area
                {
                    Id = 3752,
                    Name = "Xã Vĩnh Hưng",
                    NormalName = "Xa Vinh Hung",

                    ParentId = 282,
                    PharmacyWardId = 1278,
                    KMSId = 10951
                },

                new Area
                {
                    Id = 3744,
                    Name = "Xã Cao An",
                    NormalName = "Xa Cao An",

                    ParentId = 283,
                    PharmacyWardId = 1268,
                    KMSId = 10927
                },
                new Area
                {
                    Id = 3747,
                    Name = "Xã Cẩm Điền",
                    NormalName = "Xa Cam Dien",

                    ParentId = 283,
                    PharmacyWardId = 1273,
                    KMSId = 10936
                },
                new Area
                {
                    Id = 3749,
                    Name = "Xã Cẩm Đoài",
                    NormalName = "Xa Cam Doai",

                    ParentId = 283,
                    PharmacyWardId = 1269,
                    KMSId = 10942
                },
                new Area
                {
                    Id = 3748,
                    Name = "Xã Cẩm Đông",
                    NormalName = "Xa Cam Dong",

                    ParentId = 283,
                    PharmacyWardId = 1270,
                    KMSId = 10939
                },
                new Area
                {
                    Id = 3734,
                    Name = "Xã Cẩm Hoàng",
                    NormalName = "Xa Cam Hoang",

                    ParentId = 283,
                    PharmacyWardId = 1264,
                    KMSId = 10897
                },
                new Area
                {
                    Id = 3733,
                    Name = "Xã Cẩm Hưng",
                    NormalName = "Xa Cam Hung",

                    ParentId = 283,
                    PharmacyWardId = 1258,
                    KMSId = 10894
                },
                new Area
                {
                    Id = 3746,
                    Name = "Xã Cẩm Phúc",
                    NormalName = "Xa Cam Phuc",

                    ParentId = 283,
                    PharmacyWardId = 1272,
                    KMSId = 10933
                },
                new Area
                {
                    Id = 3735,
                    Name = "Xã Cẩm Văn",
                    NormalName = "Xa Cam Van",

                    ParentId = 283,
                    PharmacyWardId = 1266,
                    KMSId = 10900
                },
                new Area
                {
                    Id = 3738,
                    Name = "Xã Cẩm Vũ",
                    NormalName = "Xa Cam Vu",

                    ParentId = 283,
                    PharmacyWardId = 1265,
                    KMSId = 10909
                },
                new Area
                {
                    Id = 3741,
                    Name = "Xã Định Sơn",
                    NormalName = "Xa Dinh Son",

                    ParentId = 283,
                    PharmacyWardId = 298,
                    KMSId = 10918
                },
                new Area
                {
                    Id = 3739,
                    Name = "Xã Đức Chính",
                    NormalName = "Xa Duc Chinh",

                    ParentId = 283,
                    PharmacyWardId = 1267,
                    KMSId = 10912
                },
                new Area
                {
                    Id = 3743,
                    Name = "Xã Lương Điền",
                    NormalName = "Xa Luong Dien",

                    ParentId = 283,
                    PharmacyWardId = 1274,
                    KMSId = 10924
                },
                new Area
                {
                    Id = 3736,
                    Name = "Xã Ngọc Liên",
                    NormalName = "Xa Ngoc Lien",

                    ParentId = 283,
                    PharmacyWardId = 1259,
                    KMSId = 10903
                },
                new Area
                {
                    Id = 3745,
                    Name = "Xã Tân Trường",
                    NormalName = "Xa Tan Truong",

                    ParentId = 283,
                    PharmacyWardId = 1271,
                    KMSId = 10930
                },
                new Area
                {
                    Id = 3737,
                    Name = "Xã Thạch Lỗi",
                    NormalName = "Xa Thach Loi",

                    ParentId = 283,
                    PharmacyWardId = 1261,
                    KMSId = 10906
                },

                new Area
                {
                    Id = 3661,
                    Name = "Xã Bạch Đằng",
                    NormalName = "Xa Bach Dang",

                    ParentId = 284,
                    PharmacyWardId = 755,
                    KMSId = 10678
                },
                new Area
                {
                    Id = 3671,
                    Name = "Xã Hiệp Hòa",
                    NormalName = "Xa Hiep Hoa",

                    ParentId = 284,
                    PharmacyWardId = 775,
                    KMSId = 10708
                },
                new Area
                {
                    Id = 3664,
                    Name = "Xã Hoành Sơn",
                    NormalName = "Xa Hoanh Son",

                    ParentId = 284,
                    PharmacyWardId = 1180,
                    KMSId = 10687
                },
                new Area
                {
                    Id = 3675,
                    Name = "Xã Lạc Long",
                    NormalName = "Xa Lac Long",

                    ParentId = 284,
                    PharmacyWardId = 1170,
                    KMSId = 10720
                },
                new Area
                {
                    Id = 3663,
                    Name = "Xã Lê Ninh",
                    NormalName = "Xa Le Ninh",

                    ParentId = 284,
                    PharmacyWardId = 1173,
                    KMSId = 10684
                },
                new Area
                {
                    Id = 3684,
                    Name = "Xã Minh Hòa",
                    NormalName = "Xa Minh Hoa",

                    ParentId = 284,
                    PharmacyWardId = 1161,
                    KMSId = 10747
                },
                new Area
                {
                    Id = 11232,
                    Name = "Xã Quang Thành",
                    NormalName = "Xa Quang Thanh",

                    ParentId = 284,
                    KMSId = 10705
                },
                new Area
                {
                    Id = 3674,
                    Name = "Xã Thăng Long",
                    NormalName = "Xa Thang Long",

                    ParentId = 284,
                    PharmacyWardId = 1169,
                    KMSId = 10717
                },
                new Area
                {
                    Id = 3678,
                    Name = "Xã Thượng Quận",
                    NormalName = "Xa Thuong Quan",

                    ParentId = 284,
                    PharmacyWardId = 1167,
                    KMSId = 10729
                },

                new Area
                {
                    Id = 3660,
                    Name = "Phường An Lưu",
                    NormalName = "Phuong An Luu",

                    ParentId = 284,
                    PharmacyWardId = 1160,
                    KMSId = 10675
                },
                new Area
                {
                    Id = 3679,
                    Name = "Phường An Phụ",
                    NormalName = "Phuong An Phu",

                    ParentId = 284,
                    PharmacyWardId = 1166,
                    KMSId = 10732
                },
                new Area
                {
                    Id = 3676,
                    Name = "Phường An Sinh",
                    NormalName = "Phuong An Sinh",

                    ParentId = 284,
                    PharmacyWardId = 1177,
                    KMSId = 10723
                },
                new Area
                {
                    Id = 3667,
                    Name = "Phường Duy Tân",
                    NormalName = "Phuong Duy Tan",

                    ParentId = 284,
                    PharmacyWardId = 1181,
                    KMSId = 10696
                },
                new Area
                {
                    Id = 3683,
                    Name = "Phường Hiến Thành",
                    NormalName = "Phuong Hien Thanh",

                    ParentId = 284,
                    PharmacyWardId = 1162,
                    KMSId = 10744
                },
                new Area
                {
                    Id = 3680,
                    Name = "Phường Hiệp An",
                    NormalName = "Phuong Hiep An",

                    ParentId = 284,
                    PharmacyWardId = 1165,
                    KMSId = 10735
                },
                new Area
                {
                    Id = 3677,
                    Name = "Phường Hiệp Sơn",
                    NormalName = "Phuong Hiep Son",

                    ParentId = 284,
                    PharmacyWardId = 1179,
                    KMSId = 10726
                },
                new Area
                {
                    Id = 3681,
                    Name = "Phường Long Xuyên",
                    NormalName = "Phuong Long Xuyen",

                    ParentId = 284,
                    PharmacyWardId = 1164,
                    KMSId = 10738
                },
                new Area
                {
                    Id = 3669,
                    Name = "Phường Minh Tân",
                    NormalName = "Phuong Minh Tan",

                    ParentId = 284,
                    PharmacyWardId = 557,
                    KMSId = 10702
                },
                new Area
                {
                    Id = 11231,
                    Name = "Phường Phạm Thái",
                    NormalName = "Phuong Pham Thai",

                    ParentId = 284,
                    PharmacyWardId = 0,
                    KMSId = 10693
                },
                new Area
                {
                    Id = 3673,
                    Name = "Phường Phú Thứ",
                    NormalName = "Phuong Phu Thu",

                    ParentId = 284,
                    PharmacyWardId = 1166,
                    KMSId = 10714
                },
                new Area
                {
                    Id = 3668,
                    Name = "Phường Tân Dân",
                    NormalName = "Phuong Tan Dan",

                    ParentId = 284,
                    PharmacyWardId = 130,
                    KMSId = 10699
                },
                new Area
                {
                    Id = 3682,
                    Name = "Phường Thái Thịnh",
                    NormalName = "Phuong Thai Thinh",

                    ParentId = 284,
                    PharmacyWardId = 1163,
                    KMSId = 10741
                },
                new Area
                {
                    Id = 3662,
                    Name = "Phường Thất Hùng",
                    NormalName = "Phuong That Hung",

                    ParentId = 284,
                    PharmacyWardId = 1175,
                    KMSId = 10681
                },

                new Area
                {
                    Id = 3701,
                    Name = "Xã Bình Dân",
                    NormalName = "Xa Binh Dan",

                    ParentId = 285,
                    PharmacyWardId = 1200,
                    KMSId = 10798
                },
                new Area
                {
                    Id = 3689,
                    Name = "Xã Cổ Dũng",
                    NormalName = "Xa Co Dung",

                    ParentId = 285,
                    PharmacyWardId = 1189,
                    KMSId = 10762
                },
                new Area
                {
                    Id = 3687,
                    Name = "Xã Cộng Hòa",
                    NormalName = "Xa Cong Hoa",

                    ParentId = 285,
                    PharmacyWardId = 394,
                    KMSId = 10756
                },
                new Area
                {
                    Id = 3705,
                    Name = "Xã Đại Đức",
                    NormalName = "Xa Dai Duc",

                    ParentId = 285,
                    PharmacyWardId = 1204,
                    KMSId = 10810
                },
                new Area
                {
                    Id = 3703,
                    Name = "Xã Đồng Cẩm",
                    NormalName = "Xa Dong Cam",

                    ParentId = 285,
                    PharmacyWardId = 1203,
                    KMSId = 10804
                },
                new Area
                {
                    Id = 3695,
                    Name = "Xã Kim Anh",
                    NormalName = "Xa Kim Anh",

                    ParentId = 285,
                    PharmacyWardId = 1196,
                    KMSId = 10780
                },
                new Area
                {
                    Id = 3699,
                    Name = "Xã Kim Đính",
                    NormalName = "Xa Kim Dinh",

                    ParentId = 285,
                    PharmacyWardId = 1198,
                    KMSId = 10792
                },
                new Area
                {
                    Id = 3698,
                    Name = "Xã Kim Liên",
                    NormalName = "Xa Kim Lien",

                    ParentId = 285,
                    PharmacyWardId = 1195,
                    KMSId = 10783
                },
                new Area
                {
                    Id = 3697,
                    Name = "Xã Kim Tân",
                    NormalName = "Xa Kim Tan",

                    ParentId = 285,
                    PharmacyWardId = 1199,
                    KMSId = 10786
                },
                new Area
                {
                    Id = 3692,
                    Name = "Xã Kim Xuyên",
                    NormalName = "Xa Kim Xuyen",

                    ParentId = 285,
                    PharmacyWardId = 1192,
                    KMSId = 10771
                },
                new Area
                {
                    Id = 3686,
                    Name = "Xã Lai Vu",
                    NormalName = "Xa Lai Vu",

                    ParentId = 285,
                    PharmacyWardId = 1186,
                    KMSId = 10753
                },
                new Area
                {
                    Id = 3704,
                    Name = "Xã Liên Hòa",
                    NormalName = "Xa Lien Hoa",

                    ParentId = 285,
                    PharmacyWardId = 1202,
                    KMSId = 10807
                },
                new Area
                {
                    Id = 3694,
                    Name = "Xã Ngũ Phúc",
                    NormalName = "Xa Ngu Phuc",

                    ParentId = 285,
                    PharmacyWardId = 1197,
                    KMSId = 10777
                },
                new Area
                {
                    Id = 3693,
                    Name = "Xã Phúc Thành A",
                    NormalName = "Xa Phuc Thanh A",

                    ParentId = 285,
                    PharmacyWardId = 1172,
                    KMSId = 10774
                },
                new Area
                {
                    Id = 3702,
                    Name = "Xã Tam Kỳ",
                    NormalName = "Xa Tam Ky",

                    ParentId = 285,
                    PharmacyWardId = 1205,
                    KMSId = 10801
                },
                new Area
                {
                    Id = 3691,
                    Name = "Xã Tuấn Việt",
                    NormalName = "Xa Tuan Viet",

                    ParentId = 285,
                    PharmacyWardId = 1191,
                    KMSId = 10768
                },
                new Area
                {
                    Id = 3688,
                    Name = "Xã Thượng Vũ",
                    NormalName = "Xa Thuong Vu",

                    ParentId = 285,
                    PharmacyWardId = 1188,
                    KMSId = 10759
                },
                new Area
                {
                    Id = 3641,
                    Name = "Thị trấn Nam Sách",
                    NormalName = "Thi tran Nam Sach",

                    ParentId = 280,
                    PharmacyWardId = 1116,
                    KMSId = 10606
                },
                new Area
                {
                    Id = 3750,
                    Name = "Thị trấn Kẻ Sặt",
                    NormalName = "Thi tran Ke Sat",

                    ParentId = 282,
                    PharmacyWardId = 1275,
                    KMSId = 10945
                },
                new Area
                {
                    Id = 3731,
                    Name = "Thị trấn Cẩm Giàng",
                    NormalName = "Thi tran Cam Giang",

                    ParentId = 283,
                    PharmacyWardId = 1256,
                    KMSId = 10888
                },
                new Area
                {
                    Id = 3732,
                    Name = "Thị trấn Lai Cách",
                    NormalName = "Thi tran Lai Cach",

                    ParentId = 283,
                    PharmacyWardId = 1257,
                    KMSId = 10891
                },
                new Area
                {
                    Id = 3685,
                    Name = "Thị trấn Phú Thái",
                    NormalName = "Thi tran Phu Thai",

                    ParentId = 285,
                    PharmacyWardId = 1166,
                    KMSId = 10750
                },
                new Area
                {
                    Id = 3785,
                    Name = "Xã Đoàn Thượng",
                    NormalName = "Xa Doan Thuong",

                    ParentId = 286,
                    PharmacyWardId = 373,
                    KMSId = 11056
                },
                new Area
                {
                    Id = 3788,
                    Name = "Xã Đồng Quang",
                    NormalName = "Xa Dong Quang",

                    ParentId = 286,
                    PharmacyWardId = 387,
                    KMSId = 11065
                },
                new Area
                {
                    Id = 3790,
                    Name = "Xã Đức Xương",
                    NormalName = "Xa Duc Xuong",

                    ParentId = 286,
                    PharmacyWardId = 1224,
                    KMSId = 11071
                },
                new Area
                {
                    Id = 3778,
                    Name = "Xã Gia Khánh",
                    NormalName = "Xa Gia Khanh",

                    ParentId = 286,
                    PharmacyWardId = 1221,
                    KMSId = 11035
                },
                new Area
                {
                    Id = 3779,
                    Name = "Xã Gia Lương",
                    NormalName = "Xa Gia Luong",

                    ParentId = 286,
                    PharmacyWardId = 1220,
                    KMSId = 11038
                },
                new Area
                {
                    Id = 3776,
                    Name = "Xã Gia Tân",
                    NormalName = "Xa Gia Tan",

                    ParentId = 286,
                    PharmacyWardId = 1219,
                    KMSId = 11029
                },
                new Area
                {
                    Id = 3782,
                    Name = "Xã Hoàng Diệu",
                    NormalName = "Xa Hoang Dieu",

                    ParentId = 286,
                    PharmacyWardId = 416,
                    KMSId = 11047
                },
                new Area
                {
                    Id = 3783,
                    Name = "Xã Hồng Hưng",
                    NormalName = "Xa Hong Hung",

                    ParentId = 286,
                    PharmacyWardId = 1215,
                    KMSId = 11050
                },
                new Area
                {
                    Id = 3780,
                    Name = "Xã Lê Lợi",
                    NormalName = "Xa Le Loi",

                    ParentId = 286,
                    PharmacyWardId = 605,
                    KMSId = 11041
                },
                new Area
                {
                    Id = 3789,
                    Name = "Xã Nhật Tân",
                    NormalName = "Xa Nhat Tan",

                    ParentId = 286,
                    PharmacyWardId = 1226,
                    KMSId = 11068
                },
                new Area
                {
                    Id = 3784,
                    Name = "Xã Phạm Trấn",
                    NormalName = "Xa Pham Tran",

                    ParentId = 286,
                    PharmacyWardId = 1227,
                    KMSId = 11053
                },
                new Area
                {
                    Id = 3787,
                    Name = "Xã Quang Minh",
                    NormalName = "Xa Quang Minh",

                    ParentId = 286,
                    PharmacyWardId = 558,
                    KMSId = 11062
                },
                new Area
                {
                    Id = 3777,
                    Name = "Xã Tân Tiến",
                    NormalName = "Xa Tan Tien",

                    ParentId = 286,
                    PharmacyWardId = 411,
                    KMSId = 11032
                },
                new Area
                {
                    Id = 3781,
                    Name = "Xã Toàn Thắng",
                    NormalName = "Xa Toan Thang",

                    ParentId = 286,
                    PharmacyWardId = 757,
                    KMSId = 11044
                },
                new Area
                {
                    Id = 3786,
                    Name = "Xã Thống Kênh",
                    NormalName = "Xa Thong Kenh",

                    ParentId = 286,
                    PharmacyWardId = 1222,
                    KMSId = 11059
                },
                new Area
                {
                    Id = 3770,
                    Name = "Xã Thống Nhất",
                    NormalName = "Xa Thong Nhat",

                    ParentId = 286,
                    PharmacyWardId = 1208,
                    KMSId = 11008
                },
                new Area
                {
                    Id = 3773,
                    Name = "Xã Yết Kiêu",
                    NormalName = "Xa Yet Kieu",

                    ParentId = 286,
                    PharmacyWardId = 1210,
                    KMSId = 11020
                },
                new Area
                {
                    Id = 3768,
                    Name = "Thị trấn Gia Lộc",
                    NormalName = "Thi tran Gia Loc",

                    ParentId = 286,
                    PharmacyWardId = 1206,
                    KMSId = 10999
                },

                new Area
                {
                    Id = 3824,
                    Name = "Xã An Đức",
                    NormalName = "Xa An Duc",

                    ParentId = 288,
                    PharmacyWardId = 1325,
                    KMSId = 11173
                },
                new Area
                {
                    Id = 3834,
                    Name = "Xã Đồng Tâm",
                    NormalName = "Xa Dong Tam",

                    ParentId = 288,
                    PharmacyWardId = 816,
                    KMSId = 11203
                },
                new Area
                {
                    Id = 3829,
                    Name = "Xã Đông Xuyên",
                    NormalName = "Xa Dong Xuyen",

                    ParentId = 288,
                    PharmacyWardId = 1326,
                    KMSId = 11188
                },
                new Area
                {
                    Id = 3842,
                    Name = "Xã Hiệp Lực",
                    NormalName = "Xa Hiep Luc",

                    ParentId = 288,
                    PharmacyWardId = 1313,
                    KMSId = 11227
                },
                new Area
                {
                    Id = 3838,
                    Name = "Xã Hồng Dụ",
                    NormalName = "Xa Hong Du",

                    ParentId = 288,
                    PharmacyWardId = 1314,
                    KMSId = 11215
                },
                new Area
                {
                    Id = 3822,
                    Name = "Xã Hồng Đức",
                    NormalName = "Xa Hong Duc",

                    ParentId = 288,
                    PharmacyWardId = 1314,
                    KMSId = 11167
                },
                new Area
                {
                    Id = 3841,
                    Name = "Xã Hồng Phong",
                    NormalName = "Xa Hong Phong",

                    ParentId = 288,
                    PharmacyWardId = 422,
                    KMSId = 11224
                },
                new Area
                {
                    Id = 3843,
                    Name = "Xã Hồng Phúc",
                    NormalName = "Xa Hong Phuc",

                    ParentId = 288,
                    PharmacyWardId = 1331,
                    KMSId = 11230
                },
                new Area
                {
                    Id = 3844,
                    Name = "Xã Hưng Long",
                    NormalName = "Xa Hung Long",

                    ParentId = 288,
                    PharmacyWardId = 1381,
                    KMSId = 11233
                },
                new Area
                {
                    Id = 3836,
                    Name = "Xã Kiến Quốc",
                    NormalName = "Xa Kien Quoc",

                    ParentId = 288,
                    PharmacyWardId = 732,
                    KMSId = 11209
                },
                new Area
                {
                    Id = 3833,
                    Name = "Xã Ninh Hải",
                    NormalName = "Xa Ninh Hai",

                    ParentId = 288,
                    PharmacyWardId = 1327,
                    KMSId = 11200
                },
                new Area
                {
                    Id = 3821,
                    Name = "Xã Nghĩa An",
                    NormalName = "Xa Nghia An",

                    ParentId = 288,
                    PharmacyWardId = 1319,
                    KMSId = 11164
                },
                new Area
                {
                    Id = 3826,
                    Name = "Xã Tân Hương",
                    NormalName = "Xa Tan Huong",

                    ParentId = 288,
                    PharmacyWardId = 1318,
                    KMSId = 11179
                },
                new Area
                {
                    Id = 3832,
                    Name = "Xã Tân Phong",
                    NormalName = "Xa Tan Phong",

                    ParentId = 288,
                    PharmacyWardId = 739,
                    KMSId = 11197
                },
                new Area
                {
                    Id = 3835,
                    Name = "Xã Tân Quang",
                    NormalName = "Xa Tan Quang",

                    ParentId = 288,
                    PharmacyWardId = 1362,
                    KMSId = 11206
                },
                new Area
                {
                    Id = 3820,
                    Name = "Xã Ứng Hoè",
                    NormalName = "Xa Ung Hoe",

                    ParentId = 288,
                    PharmacyWardId = 1321,
                    KMSId = 11161
                },
                new Area
                {
                    Id = 3825,
                    Name = "Xã Vạn Phúc",
                    NormalName = "Xa Van Phuc",

                    ParentId = 288,
                    PharmacyWardId = 1166,
                    KMSId = 11176
                },
                new Area
                {
                    Id = 3839,
                    Name = "Xã Văn Hội",
                    NormalName = "Xa Van Hoi",

                    ParentId = 288,
                    PharmacyWardId = 871,
                    KMSId = 11218
                },
                new Area
                {
                    Id = 3828,
                    Name = "Xã Vĩnh Hòa",
                    NormalName = "Xa Vinh Hoa",

                    ParentId = 288,
                    PharmacyWardId = 1316,
                    KMSId = 11185
                },
                new Area
                {
                    Id = 3818,
                    Name = "Thị trấn Ninh Giang",
                    NormalName = "Thi tran Ninh Giang",

                    ParentId = 288,
                    PharmacyWardId = 1312,
                    KMSId = 11155
                },
                new Area
                {
                    Id = 3721,
                    Name = "Xã An Phượng",
                    NormalName = "Xa An Phuong",

                    ParentId = 289,
                    PharmacyWardId = 669,
                    KMSId = 10864
                },
                new Area
                {
                    Id = 3711,
                    Name = "Xã Cẩm Chế",
                    NormalName = "Xa Cam Che",

                    ParentId = 289,
                    PharmacyWardId = 1141,
                    KMSId = 10828
                },
                new Area
                {
                    Id = 3707,
                    Name = "Xã Hồng Lạc",
                    NormalName = "Xa Hong Lac",

                    ParentId = 289,
                    PharmacyWardId = 1136,
                    KMSId = 10816
                },
                new Area
                {
                    Id = 3716,
                    Name = "Xã Liên Mạc",
                    NormalName = "Xa Lien Mac",

                    ParentId = 289,
                    PharmacyWardId = 206,
                    KMSId = 10843
                },
                new Area
                {
                    Id = 3715,
                    Name = "Xã Tân An",
                    NormalName = "Xa Tan An",

                    ParentId = 289,
                    PharmacyWardId = 1145,
                    KMSId = 10840
                },
                new Area
                {
                    Id = 3710,
                    Name = "Xã Tân Việt",
                    NormalName = "Xa Tan Viet",

                    ParentId = 289,
                    PharmacyWardId = 1398,
                    KMSId = 10825
                },
                new Area
                {
                    Id = 3712,
                    Name = "Xã Thanh An",
                    NormalName = "Xa Thanh An",

                    ParentId = 289,
                    PharmacyWardId = 1138,
                    KMSId = 10831
                },
                new Area
                {
                    Id = 3729,
                    Name = "Xã Thanh Cường",
                    NormalName = "Xa Thanh Cuong",

                    ParentId = 289,
                    PharmacyWardId = 1157,
                    KMSId = 10882
                },
                new Area
                {
                    Id = 3717,
                    Name = "Xã Thanh Hải",
                    NormalName = "Xa Thanh Hai",

                    ParentId = 289,
                    PharmacyWardId = 1146,
                    KMSId = 10846
                },
                new Area
                {
                    Id = 3728,
                    Name = "Xã Thanh Hồng",
                    NormalName = "Xa Thanh Hong",

                    ParentId = 289,
                    PharmacyWardId = 1158,
                    KMSId = 10879
                },
                new Area
                {
                    Id = 3718,
                    Name = "Xã Thanh Khê",
                    NormalName = "Xa Thanh Khe",

                    ParentId = 289,
                    PharmacyWardId = 1149,
                    KMSId = 10849
                },
                new Area
                {
                    Id = 3713,
                    Name = "Xã Thanh Lang",
                    NormalName = "Xa Thanh Lang",

                    ParentId = 289,
                    PharmacyWardId = 1139,
                    KMSId = 10834
                },
                new Area
                {
                    Id = 3725,
                    Name = "Xã Thanh Quang",
                    NormalName = "Xa Thanh Quang",

                    ParentId = 289,
                    PharmacyWardId = 1154,
                    KMSId = 10876
                },
                new Area
                {
                    Id = 3724,
                    Name = "Xã Thanh Sơn",
                    NormalName = "Xa Thanh Son",

                    ParentId = 289,
                    PharmacyWardId = 735,
                    KMSId = 10867
                },
                new Area
                {
                    Id = 3722,
                    Name = "Xã Thanh Thủy",
                    NormalName = "Xa Thanh Thuy",

                    ParentId = 289,
                    PharmacyWardId = 1152,
                    KMSId = 10861
                },
                new Area
                {
                    Id = 3719,
                    Name = "Xã Thanh Xá",
                    NormalName = "Xa Thanh Xa",

                    ParentId = 289,
                    PharmacyWardId = 1150,
                    KMSId = 10852
                },
                new Area
                {
                    Id = 3720,
                    Name = "Xã Thanh Xuân",
                    NormalName = "Xa Thanh Xuan",

                    ParentId = 289,
                    PharmacyWardId = 131,
                    KMSId = 10855
                },
                new Area
                {
                    Id = 3708,
                    Name = "Xã Việt Hồng",
                    NormalName = "Xa Viet Hong",

                    ParentId = 289,
                    PharmacyWardId = 1137,
                    KMSId = 10819
                },
                new Area
                {
                    Id = 3730,
                    Name = "Xã Vĩnh Lập",
                    NormalName = "Xa Vinh Lap",

                    ParentId = 289,
                    PharmacyWardId = 1159,
                    KMSId = 10885
                },
                new Area
                {
                    Id = 3706,
                    Name = "Thị trấn Thanh Hà",
                    NormalName = "Thi tran Thanh Ha",

                    ParentId = 289,
                    PharmacyWardId = 1244,
                    KMSId = 10813
                },

                new Area
                {
                    Id = 10562,
                    Name = "Xã An Thạnh Trung",
                    NormalName = "Xa An Thanh Trung",

                    ParentId = 1,
                    PharmacyWardId = 819,
                    KMSId = 30670
                },
                new Area
                {
                    Id = 10561,
                    Name = "Xã Bình Phước Xuân",
                    NormalName = "Xa Binh Phuoc Xuan",

                    ParentId = 1,
                    PharmacyWardId = 349,
                    KMSId = 30667
                },
                new Area
                {
                    Id = 10565,
                    Name = "Xã Hòa An",
                    NormalName = "Xa Hoa An",

                    ParentId = 1,
                    PharmacyWardId = 6908,
                    KMSId = 30679
                },
                new Area
                {
                    Id = 10564,
                    Name = "Xã Hòa Bình",
                    NormalName = "Xa Hoa Binh",

                    ParentId = 1,
                    PharmacyWardId = 789,
                    KMSId = 30676
                },
                new Area
                {
                    Id = 10563,
                    Name = "Xã Hội An",
                    NormalName = "Xa Hoi An",

                    ParentId = 1,
                    PharmacyWardId = 10727,
                    KMSId = 30673
                },
                new Area
                {
                    Id = 10550,
                    Name = "Xã Kiến An",
                    NormalName = "Xa Kien An",

                    ParentId = 1,
                    PharmacyWardId = 10727,
                    KMSId = 30634
                },
                new Area
                {
                    Id = 10555,
                    Name = "Xã Kiến Thành",
                    NormalName = "Xa Kien Thanh",

                    ParentId = 1,
                    PharmacyWardId = 3422,
                    KMSId = 30649
                },
                new Area
                {
                    Id = 10552,
                    Name = "Xã Long Điền A",
                    NormalName = "Xa Long Dien A",

                    ParentId = 1,
                    PharmacyWardId = 10727,
                    KMSId = 30640
                },
                new Area
                {
                    Id = 10554,
                    Name = "Xã Long Điền B",
                    NormalName = "Xa Long Dien B",

                    ParentId = 1,
                    PharmacyWardId = 10727,
                    KMSId = 30646
                },
                new Area
                {
                    Id = 10559,
                    Name = "Xã Long Giang",
                    NormalName = "Xa Long Giang",

                    ParentId = 1,
                    PharmacyWardId = 8901,
                    KMSId = 30661
                },
                new Area
                {
                    Id = 10560,
                    Name = "Xã Long Kiến",
                    NormalName = "Xa Long Kien",

                    ParentId = 1,
                    PharmacyWardId = 10727,
                    KMSId = 30664
                },
                new Area
                {
                    Id = 10557,
                    Name = "Xã Mỹ An",
                    NormalName = "Xa My An",

                    ParentId = 1,
                    PharmacyWardId = 4050,
                    KMSId = 30655
                },
                new Area
                {
                    Id = 10556,
                    Name = "Xã Mỹ Hiệp",
                    NormalName = "Xa My Hiep",

                    ParentId = 1,
                    PharmacyWardId = 10727,
                    KMSId = 30652
                },
                new Area
                {
                    Id = 10551,
                    Name = "Xã Mỹ Hội Đông",
                    NormalName = "Xa My Hoi Dong",

                    ParentId = 1,
                    PharmacyWardId = 10159,
                    KMSId = 30637
                },
                new Area
                {
                    Id = 10558,
                    Name = "Xã Nhơn Mỹ",
                    NormalName = "Xa Nhon My",

                    ParentId = 1,
                    PharmacyWardId = 10921,
                    KMSId = 30658
                },
                new Area
                {
                    Id = 10553,
                    Name = "Xã Tấn Mỹ",
                    NormalName = "Xa Tan My",

                    ParentId = 1,
                    PharmacyWardId = 486,
                    KMSId = 30643
                },
                new Area
                {
                    Id = 10548,
                    Name = "Thị trấn Chợ Mới",
                    NormalName = "Thi tran Cho Moi",

                    ParentId = 1,
                    PharmacyWardId = 968,
                    KMSId = 30628
                },
                new Area
                {
                    Id = 10549,
                    Name = "Thị trấn Mỹ Luông",
                    NormalName = "Thi tran My Luong",

                    ParentId = 1,
                    PharmacyWardId = 419,
                    KMSId = 30631
                },
                new Area
                {
                    Id = 10493,
                    Name = "Thị trấn Cái Dầu",
                    NormalName = "Thi tran Cai Dau",

                    ParentId = 2,
                    PharmacyWardId = 9960,
                    KMSId = 30463
                },
                new Area
                {
                    Id = 10498,
                    Name = "Thị trấn Vĩnh Thạnh Trung",
                    NormalName = "Thi tran Vinh Thanh Trung",

                    ParentId = 2,
                    PharmacyWardId = 819,
                    KMSId = 30478
                },
                new Area
                {
                    Id = 10505,
                    Name = "Xã Bình Chánh",
                    NormalName = "Xa Binh Chanh",

                    ParentId = 2,
                    PharmacyWardId = 9972,
                    KMSId = 30499
                },
                new Area
                {
                    Id = 10500,
                    Name = "Xã Bình Long",
                    NormalName = "Xa Binh Long",

                    ParentId = 2,
                    PharmacyWardId = 2602,
                    KMSId = 30484
                },
                new Area
                {
                    Id = 10501,
                    Name = "Xã Bình Mỹ",
                    NormalName = "Xa Binh My",

                    ParentId = 2,
                    PharmacyWardId = 7196,
                    KMSId = 30487
                },
                new Area
                {
                    Id = 10504,
                    Name = "Xã Bình Phú",
                    NormalName = "Xa Binh Phu",

                    ParentId = 2,
                    PharmacyWardId = 349,
                    KMSId = 30496
                },
                new Area
                {
                    Id = 10502,
                    Name = "Xã Bình Thủy",
                    NormalName = "Xa Binh Thuy",

                    ParentId = 2,
                    PharmacyWardId = 9969,
                    KMSId = 30490
                },
                new Area
                {
                    Id = 10503,
                    Name = "Xã Đào Hữu Cảnh",
                    NormalName = "Xa Dao Huu Canh",

                    ParentId = 2,
                    PharmacyWardId = 9970,
                    KMSId = 30493
                },
                new Area
                {
                    Id = 10494,
                    Name = "Xã Khánh Hòa",
                    NormalName = "Xa Khanh Hoa",

                    ParentId = 2,
                    PharmacyWardId = 3360,
                    KMSId = 30466
                },
                new Area
                {
                    Id = 10495,
                    Name = "Xã Mỹ Đức",
                    NormalName = "Xa My Duc",

                    ParentId = 2,
                    PharmacyWardId = 722,
                    KMSId = 30469
                },
                new Area
                {
                    Id = 10496,
                    Name = "Xã Mỹ Phú",
                    NormalName = "Xa My Phu",

                    ParentId = 2,
                    PharmacyWardId = 9759,
                    KMSId = 30472
                },
                new Area
                {
                    Id = 10497,
                    Name = "Xã Ô Long Vỹ",
                    NormalName = "Xa O Long Vy",

                    ParentId = 2,
                    PharmacyWardId = 9964,
                    KMSId = 30475
                },
                new Area
                {
                    Id = 10499,
                    Name = "Xã Thạnh Mỹ Tây",
                    NormalName = "Xa Thanh My Tay",

                    ParentId = 2,
                    PharmacyWardId = 256,
                    KMSId = 30481
                },
                new Area
                {
                    Id = 10535,
                    Name = "Thị trấn An Châu",
                    NormalName = "Thi tran An Chau",

                    ParentId = 3,
                    PharmacyWardId = 1089,
                    KMSId = 30589
                },
                new Area
                {
                    Id = 10540,
                    Name = "Thị trấn Vĩnh Bình",
                    NormalName = "Thi tran Vinh Binh",

                    ParentId = 3,
                    PharmacyWardId = 10374,
                    KMSId = 30604
                },
                new Area
                {
                    Id = 10536,
                    Name = "Xã An Hòa",
                    NormalName = "Xa An Hoa",

                    ParentId = 3,
                    PharmacyWardId = 777,
                    KMSId = 30592
                },
                new Area
                {
                    Id = 10541,
                    Name = "Xã Bình Hòa",
                    NormalName = "Xa Binh Hoa",

                    ParentId = 3,
                    PharmacyWardId = 1771,
                    KMSId = 30607
                },
                new Area
                {
                    Id = 10539,
                    Name = "Xã Bình Thạnh",
                    NormalName = "Xa Binh Thanh",

                    ParentId = 3,
                    PharmacyWardId = 2093,
                    KMSId = 30601
                },
                new Area
                {
                    Id = 10537,
                    Name = "Xã Cần Đăng",
                    NormalName = "Xa Can Dang",

                    ParentId = 3,
                    PharmacyWardId = 6956,
                    KMSId = 30595
                },
                new Area
                {
                    Id = 10543,
                    Name = "Xã Hòa Bình Thạnh",
                    NormalName = "Xa Hoa Binh Thanh",

                    ParentId = 3,
                    PharmacyWardId = 789,
                    KMSId = 30613
                },
                new Area
                {
                    Id = 10546,
                    Name = "Xã Tân Phú",
                    NormalName = "Xa Tan Phu",

                    ParentId = 3,
                    PharmacyWardId = 391,
                    KMSId = 30622
                },
                new Area
                {
                    Id = 10542,
                    Name = "Xã Vĩnh An",
                    NormalName = "Xa Vinh An",

                    ParentId = 3,
                    PharmacyWardId = 773,
                    KMSId = 30610
                },
                new Area
                {
                    Id = 10538,
                    Name = "Xã Vĩnh Hanh",
                    NormalName = "Xa Vinh Hanh",

                    ParentId = 3,
                    PharmacyWardId = 10727,
                    KMSId = 30598
                },
                new Area
                {
                    Id = 10544,
                    Name = "Xã Vĩnh Lợi",
                    NormalName = "Xa Vinh Loi",

                    ParentId = 3,
                    PharmacyWardId = 9574,
                    KMSId = 30616
                },
                new Area
                {
                    Id = 10545,
                    Name = "Xã Vĩnh Nhuận",
                    NormalName = "Xa Vinh Nhuan",

                    ParentId = 3,
                    PharmacyWardId = 10727,
                    KMSId = 30619
                },
                new Area
                {
                    Id = 10547,
                    Name = "Xã Vĩnh Thành",
                    NormalName = "Xa Vinh Thanh",

                    ParentId = 3,
                    PharmacyWardId = 5288,
                    KMSId = 30625
                },
                new Area
                {
                    Id = 10521,
                    Name = "Thị trấn Ba Chúc",
                    NormalName = "Thi tran Ba Chuc",

                    ParentId = 4,
                    PharmacyWardId = 6683,
                    KMSId = 30547
                },
                new Area
                {
                    Id = 10532,
                    Name = "Thị trấn Cô Tô",
                    NormalName = "Thi tran Co To",

                    ParentId = 4,
                    PharmacyWardId = 9999,
                    KMSId = 30580
                },
                new Area
                {
                    Id = 10520,
                    Name = "Thị trấn Tri Tôn",
                    NormalName = "Thi tran Tri Ton",

                    ParentId = 4,
                    PharmacyWardId = 10727,
                    KMSId = 30544
                },
                new Area
                {
                    Id = 10531,
                    Name = "Xã An Tức",
                    NormalName = "Xa An Tuc",

                    ParentId = 4,
                    PharmacyWardId = 6962,
                    KMSId = 30577
                },
                new Area
                {
                    Id = 10526,
                    Name = "Xã Châu Lăng",
                    NormalName = "Xa Chau Lang",

                    ParentId = 4,
                    PharmacyWardId = 6951,
                    KMSId = 30562
                },
                new Area
                {
                    Id = 10522,
                    Name = "Xã Lạc Quới",
                    NormalName = "Xa Lac Quoi",

                    ParentId = 4,
                    PharmacyWardId = 10727,
                    KMSId = 30550
                },
                new Area
                {
                    Id = 10523,
                    Name = "Xã Lê Trì",
                    NormalName = "Xa Le Tri",

                    ParentId = 4,
                    PharmacyWardId = 10727,
                    KMSId = 30553
                },
                new Area
                {
                    Id = 10528,
                    Name = "Xã Lương An Trà",
                    NormalName = "Xa Luong An Tra",

                    ParentId = 4,
                    PharmacyWardId = 5811,
                    KMSId = 30568
                },
                new Area
                {
                    Id = 10527,
                    Name = "Xã Lương Phi",
                    NormalName = "Xa Luong Phi",

                    ParentId = 4,
                    PharmacyWardId = 10727,
                    KMSId = 30565
                },
                new Area
                {
                    Id = 10530,
                    Name = "Xã Núi Tô",
                    NormalName = "Xa Nui To",

                    ParentId = 4,
                    PharmacyWardId = 10727,
                    KMSId = 30574
                },
                new Area
                {
                    Id = 10534,
                    Name = "Xã Ô Lâm",
                    NormalName = "Xa O Lam",

                    ParentId = 4,
                    PharmacyWardId = 10001,
                    KMSId = 30586
                },
                new Area
                {
                    Id = 10529,
                    Name = "Xã Tà Đảnh",
                    NormalName = "Xa Ta Danh",

                    ParentId = 4,
                    PharmacyWardId = 9996,
                    KMSId = 30571
                },
                new Area
                {
                    Id = 10533,
                    Name = "Xã Tân Tuyến",
                    NormalName = "Xa Tan Tuyen",

                    ParentId = 4,
                    PharmacyWardId = 6962,
                    KMSId = 30583
                },
                new Area
                {
                    Id = 10524,
                    Name = "Xã Vĩnh Gia",
                    NormalName = "Xa Vinh Gia",

                    ParentId = 4,
                    PharmacyWardId = 10727,
                    KMSId = 30556
                },
                new Area
                {
                    Id = 10525,
                    Name = "Xã Vĩnh Phước",
                    NormalName = "Xa Vinh Phuoc",

                    ParentId = 4,
                    PharmacyWardId = 3825,
                    KMSId = 30559
                },
                new Area
                {
                    Id = 10507,
                    Name = "Thị trấn Chi Lăng",
                    NormalName = "Thi tran Chi Lang",

                    ParentId = 5,
                    PharmacyWardId = 1001,
                    KMSId = 30505
                },
                new Area
                {
                    Id = 10506,
                    Name = "Thị trấn Nhà Bàng",
                    NormalName = "Thi tran Nha Bang",

                    ParentId = 5,
                    PharmacyWardId = 350,
                    KMSId = 30502
                },
                new Area
                {
                    Id = 10512,
                    Name = "Thị trấn Tịnh Biên",
                    NormalName = "Thi tran Tinh Bien",

                    ParentId = 5,
                    PharmacyWardId = 7402,
                    KMSId = 30520
                },
                new Area
                {
                    Id = 10514,
                    Name = "Xã An Cư",
                    NormalName = "Xa An Cu",

                    ParentId = 5,
                    PharmacyWardId = 7559,
                    KMSId = 30526
                },
                new Area
                {
                    Id = 10518,
                    Name = "Xã An Hảo",
                    NormalName = "Xa An Hao",

                    ParentId = 5,
                    PharmacyWardId = 9985,
                    KMSId = 30538
                },
                new Area
                {
                    Id = 10515,
                    Name = "Xã An Nông",
                    NormalName = "Xa An Nong",

                    ParentId = 5,
                    PharmacyWardId = 9982,
                    KMSId = 30529
                },
                new Area
                {
                    Id = 10510,
                    Name = "Xã An Phú",
                    NormalName = "Xa An Phu",

                    ParentId = 5,
                    PharmacyWardId = 1166,
                    KMSId = 30514
                },
                new Area
                {
                    Id = 10508,
                    Name = "Xã Núi Voi",
                    NormalName = "Xa Nui Voi",

                    ParentId = 5,
                    PharmacyWardId = 9976,
                    KMSId = 30508
                },
                new Area
                {
                    Id = 10509,
                    Name = "Xã Nhơn Hưng",
                    NormalName = "Xa Nhon Hung",

                    ParentId = 5,
                    PharmacyWardId = 9977,
                    KMSId = 30511
                },
                new Area
                {
                    Id = 10519,
                    Name = "Xã Tân Lập",
                    NormalName = "Xa Tan Lap",

                    ParentId = 5,
                    PharmacyWardId = 329,
                    KMSId = 30541
                },
                new Area
                {
                    Id = 10517,
                    Name = "Xã Tân Lợi",
                    NormalName = "Xa Tan Loi",

                    ParentId = 5,
                    PharmacyWardId = 3607,
                    KMSId = 30535
                },
                new Area
                {
                    Id = 10511,
                    Name = "Xã Thới Sơn",
                    NormalName = "Xa Thoi Son",

                    ParentId = 5,
                    PharmacyWardId = 9979,
                    KMSId = 30517
                },
                new Area
                {
                    Id = 10513,
                    Name = "Xã Văn Giáo",
                    NormalName = "Xa Van Giao",

                    ParentId = 5,
                    PharmacyWardId = 9980,
                    KMSId = 30523
                },
                new Area
                {
                    Id = 10516,
                    Name = "Xã Vĩnh Trung",
                    NormalName = "Xa Vinh Trung",

                    ParentId = 5,
                    PharmacyWardId = 4258,
                    KMSId = 30532
                },
                new Area
                {
                    Id = 10446,
                    Name = "Xã Vĩnh Châu",
                    NormalName = "Xa Vinh Chau",

                    ParentId = 6,
                    PharmacyWardId = 9913,
                    KMSId = 30334
                },
                new Area
                {
                    Id = 10445,
                    Name = "Xã Vĩnh Tế",
                    NormalName = "Xa Vinh Te",

                    ParentId = 6,
                    PharmacyWardId = 9912,
                    KMSId = 30331
                },
                new Area
                {
                    Id = 10441,
                    Name = "Phường Châu Phú A",
                    NormalName = "Phuong Chau Phu A",

                    ParentId = 6,
                    PharmacyWardId = 9908,
                    KMSId = 30319
                },
                new Area
                {
                    Id = 10440,
                    Name = "Phường Châu Phú B",
                    NormalName = "Phuong Chau Phu B",

                    ParentId = 6,
                    PharmacyWardId = 9907,
                    KMSId = 30316
                },
                new Area
                {
                    Id = 10443,
                    Name = "Phường Núi Sam",
                    NormalName = "Phuong Nui Sam",

                    ParentId = 6,
                    PharmacyWardId = 9909,
                    KMSId = 30325
                },
                new Area
                {
                    Id = 10442,
                    Name = "Phường Vĩnh Mỹ",
                    NormalName = "Phuong Vinh My",

                    ParentId = 6,
                    PharmacyWardId = 3845,
                    KMSId = 30322
                },
                new Area
                {
                    Id = 10444,
                    Name = "Phường Vĩnh Ngươn",
                    NormalName = "Phuong Vinh Nguon",

                    ParentId = 6,
                    PharmacyWardId = 3845,
                    KMSId = 30328
                },

                new Area
                {
                    Id = 10460,
                    Name = "Xã Đa Phước",
                    NormalName = "Xa Da Phuoc",

                    ParentId = 7,
                    PharmacyWardId = 9927,
                    KMSId = 30373
                },
                new Area
                {
                    Id = 10448,
                    Name = "Xã Khánh An",
                    NormalName = "Xa Khanh An",

                    ParentId = 7,
                    PharmacyWardId = 9916,
                    KMSId = 30340
                },
                new Area
                {
                    Id = 10450,
                    Name = "Xã Khánh Bình",
                    NormalName = "Xa Khanh Binh",

                    ParentId = 7,
                    PharmacyWardId = 7737,
                    KMSId = 30343
                },
                new Area
                {
                    Id = 10452,
                    Name = "Xã Nhơn Hội",
                    NormalName = "Xa Nhon Hoi",

                    ParentId = 7,
                    PharmacyWardId = 7367,
                    KMSId = 30349
                },
                new Area
                {
                    Id = 10454,
                    Name = "Xã Phú Hội",
                    NormalName = "Xa Phu Hoi",

                    ParentId = 7,
                    PharmacyWardId = 6727,
                    KMSId = 30355
                },
                new Area
                {
                    Id = 10453,
                    Name = "Xã Phú Hữu",
                    NormalName = "Xa Phu Huu",

                    ParentId = 7,
                    PharmacyWardId = 9341,
                    KMSId = 30352
                },
                new Area
                {
                    Id = 10455,
                    Name = "Xã Phước Hưng",
                    NormalName = "Xa Phuoc Hung",

                    ParentId = 7,
                    PharmacyWardId = 9492,
                    KMSId = 30358
                },
                new Area
                {
                    Id = 10451,
                    Name = "Xã Quốc Thái",
                    NormalName = "Xa Quoc Thai",

                    ParentId = 7,
                    PharmacyWardId = 9918,
                    KMSId = 30346
                },
                new Area
                {
                    Id = 10457,
                    Name = "Xã Vĩnh Hậu",
                    NormalName = "Xa Vinh Hau",

                    ParentId = 7,
                    PharmacyWardId = 9924,
                    KMSId = 30364
                },
                new Area
                {
                    Id = 10459,
                    Name = "Xã Vĩnh Hội Đông",
                    NormalName = "Xa Vinh Hoi Dong",

                    ParentId = 7,
                    PharmacyWardId = 9926,
                    KMSId = 30370
                },
                new Area
                {
                    Id = 10456,
                    Name = "Xã Vĩnh Lộc",
                    NormalName = "Xa Vinh Loc",

                    ParentId = 7,
                    PharmacyWardId = 6272,
                    KMSId = 30361
                },
                new Area
                {
                    Id = 10458,
                    Name = "Xã Vĩnh Trường",
                    NormalName = "Xa Vinh Truong",

                    ParentId = 7,
                    PharmacyWardId = 9925,
                    KMSId = 30367
                },
                new Area
                {
                    Id = 10447,
                    Name = "Thị trấn An Phú",
                    NormalName = "Thi tran An Phu",

                    ParentId = 7,
                    PharmacyWardId = 1166,
                    KMSId = 30337
                },
                new Area
                {
                    Id = 10449,
                    Name = "Thị Trấn Long Bình",
                    NormalName = "Thi Tran Long Binh",

                    ParentId = 7,
                    PharmacyWardId = 9122,
                    KMSId = 30341
                },
                new Area
                {
                    Id = 10463,
                    Name = "Phường Long Châu",
                    NormalName = "Phuong Long Chau",

                    ParentId = 8,
                    PharmacyWardId = 978,
                    KMSId = 30378
                },
                new Area
                {
                    Id = 10462,
                    Name = "Phường Long Hưng",
                    NormalName = "Phuong Long Hung",

                    ParentId = 8,
                    PharmacyWardId = 1404,
                    KMSId = 30377
                },
                new Area
                {
                    Id = 10470,
                    Name = "Phường Long Phú",
                    NormalName = "Phuong Long Phu",

                    ParentId = 8,
                    PharmacyWardId = 6736,
                    KMSId = 30394
                },
                new Area
                {
                    Id = 10474,
                    Name = "Phường Long Sơn",
                    NormalName = "Phuong Long Son",

                    ParentId = 8,
                    PharmacyWardId = 6736,
                    KMSId = 30412
                },
                new Area
                {
                    Id = 10461,
                    Name = "Phường Long Thạnh",
                    NormalName = "Phuong Long Thanh",

                    ParentId = 8,
                    PharmacyWardId = 5922,
                    KMSId = 30376
                },
                new Area
                {
                    Id = 10471,
                    Name = "Xã Châu Phong",
                    NormalName = "Xa Chau Phong",

                    ParentId = 8,
                    PharmacyWardId = 994,
                    KMSId = 30397
                },
                new Area
                {
                    Id = 10473,
                    Name = "Xã Lê Chánh",
                    NormalName = "Xa Le Chanh",

                    ParentId = 8,
                    PharmacyWardId = 9938,
                    KMSId = 30403
                },
                new Area
                {
                    Id = 10469,
                    Name = "Xã Long An",
                    NormalName = "Xa Long An",

                    ParentId = 8,
                    PharmacyWardId = 9329,
                    KMSId = 30391
                },
                new Area
                {
                    Id = 10464,
                    Name = "Xã Phú Lộc",
                    NormalName = "Xa Phu Loc",

                    ParentId = 8,
                    PharmacyWardId = 139,
                    KMSId = 30379
                },
                new Area
                {
                    Id = 10472,
                    Name = "Xã Phú Vĩnh",
                    NormalName = "Xa Phu Vinh",

                    ParentId = 8,
                    PharmacyWardId = 9937,
                    KMSId = 30400
                },
                new Area
                {
                    Id = 10468,
                    Name = "Xã Tân An",
                    NormalName = "Xa Tan An",

                    ParentId = 8,
                    PharmacyWardId = 1145,
                    KMSId = 30388
                },
                new Area
                {
                    Id = 10467,
                    Name = "Xã Tân Thạnh",
                    NormalName = "Xa Tan Thanh",

                    ParentId = 8,
                    PharmacyWardId = 1244,
                    KMSId = 30387
                },
                new Area
                {
                    Id = 10466,
                    Name = "Xã Vĩnh Hòa",
                    NormalName = "Xa Vinh Hoa",

                    ParentId = 8,
                    PharmacyWardId = 1316,
                    KMSId = 30385
                },
                new Area
                {
                    Id = 10465,
                    Name = "Xã Vĩnh Xương",
                    NormalName = "Xa Vinh Xuong",

                    ParentId = 8,
                    PharmacyWardId = 9930,
                    KMSId = 30382
                },
                new Area
                {
                    Id = 10490,
                    Name = "Xã Bình Thạnh Đông",
                    NormalName = "Xa Binh Thanh Dong",

                    ParentId = 9,
                    PharmacyWardId = 2093,
                    KMSId = 30454
                },
                new Area
                {
                    Id = 10486,
                    Name = "Xã Hiệp Xương",
                    NormalName = "Xa Hiep Xuong",

                    ParentId = 9,
                    PharmacyWardId = 9953,
                    KMSId = 30442
                },
                new Area
                {
                    Id = 10482,
                    Name = "Xã Hòa Lạc",
                    NormalName = "Xa Hoa Lac",

                    ParentId = 9,
                    PharmacyWardId = 3176,
                    KMSId = 30430
                },
                new Area
                {
                    Id = 10477,
                    Name = "Xã Long Hòa",
                    NormalName = "Xa Long Hoa",

                    ParentId = 9,
                    PharmacyWardId = 9105,
                    KMSId = 30415
                },
                new Area
                {
                    Id = 10484,
                    Name = "Xã Phú An",
                    NormalName = "Xa Phu An",

                    ParentId = 9,
                    PharmacyWardId = 7970,
                    KMSId = 30436
                },
                new Area
                {
                    Id = 10487,
                    Name = "Xã Phú Bình",
                    NormalName = "Xa Phu Binh",

                    ParentId = 9,
                    PharmacyWardId = 3229,
                    KMSId = 30445
                },
                new Area
                {
                    Id = 10480,
                    Name = "Xã Phú Hiệp",
                    NormalName = "Xa Phu Hiep",

                    ParentId = 9,
                    PharmacyWardId = 6717,
                    KMSId = 30424
                },
                new Area
                {
                    Id = 10489,
                    Name = "Xã Phú Hưng",
                    NormalName = "Xa Phu Hung",

                    ParentId = 9,
                    PharmacyWardId = 9956,
                    KMSId = 30451
                },
                new Area
                {
                    Id = 10479,
                    Name = "Xã Phú Lâm",
                    NormalName = "Xa Phu Lam",

                    ParentId = 9,
                    PharmacyWardId = 1004,
                    KMSId = 30421
                },
                new Area
                {
                    Id = 10478,
                    Name = "Xã Phú Long",
                    NormalName = "Xa Phu Long",

                    ParentId = 9,
                    PharmacyWardId = 139,
                    KMSId = 30418
                },
                new Area
                {
                    Id = 10483,
                    Name = "Xã Phú Thành",
                    NormalName = "Xa Phu Thanh",

                    ParentId = 9,
                    PharmacyWardId = 5110,
                    KMSId = 30433
                },
                new Area
                {
                    Id = 10481,
                    Name = "Xã Phú Thạnh",
                    NormalName = "Xa Phu Thanh",

                    ParentId = 9,
                    PharmacyWardId = 5110,
                    KMSId = 30427
                },
                new Area
                {
                    Id = 10488,
                    Name = "Xã Phú Thọ",
                    NormalName = "Xa Phu Tho",

                    ParentId = 9,
                    PharmacyWardId = 9089,
                    KMSId = 30448
                },
                new Area
                {
                    Id = 10485,
                    Name = "Xã Phú Xuân",
                    NormalName = "Xa Phu Xuan",

                    ParentId = 9,
                    PharmacyWardId = 885,
                    KMSId = 30439
                },
                new Area
                {
                    Id = 10491,
                    Name = "Xã Tân Hòa",
                    NormalName = "Xa Tan Hoa",

                    ParentId = 9,
                    PharmacyWardId = 393,
                    KMSId = 30457
                },
                new Area
                {
                    Id = 10492,
                    Name = "Xã Tân Trung",
                    NormalName = "Xa Tan Trung",

                    ParentId = 9,
                    PharmacyWardId = 4008,
                    KMSId = 30460
                },
                new Area
                {
                    Id = 10476,
                    Name = "Thị trấn Chợ Vàm",
                    NormalName = "Thi tran Cho Vam",

                    ParentId = 9,
                    PharmacyWardId = 968,
                    KMSId = 30409
                },
                new Area
                {
                    Id = 10475,
                    Name = "Thị trấn Phú Mỹ",
                    NormalName = "Thi tran Phu My",

                    ParentId = 9,
                    PharmacyWardId = 1166,
                    KMSId = 30406
                },
                new Area
                {
                    Id = 10439,
                    Name = "Xã Mỹ Hòa Hưng",
                    NormalName = "Xa My Hoa Hung",

                    ParentId = 10,
                    PharmacyWardId = 9527,
                    KMSId = 30313
                },
                new Area
                {
                    Id = 10438,
                    Name = "Xã Mỹ Khánh",
                    NormalName = "Xa My Khanh",

                    ParentId = 10,
                    PharmacyWardId = 9905,
                    KMSId = 30310
                },

                new Area
                {
                    Id = 10570,
                    Name = "Xã An Bình",
                    NormalName = "Xa An Binh",

                    ParentId = 11,
                    PharmacyWardId = 1034,
                    KMSId = 30692
                },
                new Area
                {
                    Id = 10581,
                    Name = "Xã Bình Thành",
                    NormalName = "Xa Binh Thanh",

                    ParentId = 11,
                    PharmacyWardId = 2093,
                    KMSId = 30724
                },
                new Area
                {
                    Id = 10575,
                    Name = "Xã Định Mỹ",
                    NormalName = "Xa Dinh My",

                    ParentId = 11,
                    PharmacyWardId = 10727,
                    KMSId = 30706
                },
                new Area
                {
                    Id = 10576,
                    Name = "Xã Định Thành",
                    NormalName = "Xa Dinh Thanh",

                    ParentId = 11,
                    PharmacyWardId = 6694,
                    KMSId = 30709
                },
                new Area
                {
                    Id = 10577,
                    Name = "Xã Mỹ Phú Đông",
                    NormalName = "Xa My Phu Dong",

                    ParentId = 11,
                    PharmacyWardId = 183,
                    KMSId = 30712
                },
                new Area
                {
                    Id = 10573,
                    Name = "Xã Phú Thuận",
                    NormalName = "Xa Phu Thuan",

                    ParentId = 11,
                    PharmacyWardId = 1182,
                    KMSId = 30700
                },
                new Area
                {
                    Id = 10569,
                    Name = "Xã Tây Phú",
                    NormalName = "Xa Tay Phu",

                    ParentId = 11,
                    PharmacyWardId = 7462,
                    KMSId = 30691
                },
                new Area
                {
                    Id = 10580,
                    Name = "Xã Thoại Giang",
                    NormalName = "Xa Thoai Giang",

                    ParentId = 11,
                    PharmacyWardId = 10727,
                    KMSId = 30721
                },
                new Area
                {
                    Id = 10574,
                    Name = "Xã Vĩnh Chánh",
                    NormalName = "Xa Vinh Chanh",

                    ParentId = 11,
                    PharmacyWardId = 3778,
                    KMSId = 30703
                },
                new Area
                {
                    Id = 10579,
                    Name = "Xã Vĩnh Khánh",
                    NormalName = "Xa Vinh Khanh",

                    ParentId = 11,
                    PharmacyWardId = 10727,
                    KMSId = 30718
                },
                new Area
                {
                    Id = 10571,
                    Name = "Xã Vĩnh Phú",
                    NormalName = "Xa Vinh Phu",

                    ParentId = 11,
                    PharmacyWardId = 3825,
                    KMSId = 30694
                },
                new Area
                {
                    Id = 10572,
                    Name = "Xã Vĩnh Trạch",
                    NormalName = "Xa Vinh Trach",

                    ParentId = 11,
                    PharmacyWardId = 10727,
                    KMSId = 30697
                },
                new Area
                {
                    Id = 10578,
                    Name = "Xã Vọng Đông",
                    NormalName = "Xa Vong Dong",

                    ParentId = 11,
                    PharmacyWardId = 10730,
                    KMSId = 30715
                },
                new Area
                {
                    Id = 10582,
                    Name = "Xã Vọng Thê",
                    NormalName = "Xa Vong The",

                    ParentId = 11,
                    PharmacyWardId = 10730,
                    KMSId = 30727
                },
                new Area
                {
                    Id = 10566,
                    Name = "Thị trấn Núi Sập",
                    NormalName = "Thi tran Nui Sap",

                    ParentId = 11,
                    PharmacyWardId = 10727,
                    KMSId = 30682
                },
                new Area
                {
                    Id = 10568,
                    Name = "Thị Trấn Óc Eo",
                    NormalName = "Thi Tran Oc Eo",

                    ParentId = 11,
                    PharmacyWardId = 10727,
                    KMSId = 30688
                },
                new Area
                {
                    Id = 10567,
                    Name = "Thị trấn Phú Hòa",
                    NormalName = "Thi tran Phu Hoa",

                    ParentId = 11,
                    PharmacyWardId = 1166,
                    KMSId = 30685
                },
                new Area
                {
                    Id = 9215,
                    Name = "Xã Châu Pha",
                    NormalName = "Xa Chau Pha",

                    ParentId = 12,
                    PharmacyWardId = 9540,
                    KMSId = 26728
                },
                new Area
                {
                    Id = 9213,
                    Name = "Xã Sông Xoài",
                    NormalName = "Xa Song Xoai",

                    ParentId = 12,
                    PharmacyWardId = 9538,
                    KMSId = 26722
                },
                new Area
                {
                    Id = 9209,
                    Name = "Xã Tân Hải",
                    NormalName = "Xa Tan Hai",

                    ParentId = 12,
                    PharmacyWardId = 3188,
                    KMSId = 26710
                },
                new Area
                {
                    Id = 9208,
                    Name = "Xã Tân Hòa",
                    NormalName = "Xa Tan Hoa",

                    ParentId = 12,
                    PharmacyWardId = 393,
                    KMSId = 26707
                },
                new Area
                {
                    Id = 9216,
                    Name = "Xã Tóc Tiên",
                    NormalName = "Xa Toc Tien",

                    ParentId = 12,
                    PharmacyWardId = 9541,
                    KMSId = 26731
                },
                new Area
                {
                    Id = 9214,
                    Name = "Phường Hắc Dịch",
                    NormalName = "Phuong Hac Dich",

                    ParentId = 12,
                    PharmacyWardId = 9539,
                    KMSId = 26725
                },
                new Area
                {
                    Id = 9212,
                    Name = "Phường Mỹ  Xuân",
                    NormalName = "Phuong My Xuan",

                    ParentId = 12,
                    PharmacyWardId = 9537,
                    KMSId = 26719
                },
                new Area
                {
                    Id = 9207,
                    Name = "Phường Phú Mỹ",
                    NormalName = "Phuong Phu My",

                    ParentId = 12,
                    PharmacyWardId = 1166,
                    KMSId = 26704
                },
                new Area
                {
                    Id = 9210,
                    Name = "Phường Phước Hòa",
                    NormalName = "Phuong Phuoc Hoa",

                    ParentId = 12,
                    PharmacyWardId = 6926,
                    KMSId = 26713
                },
                new Area
                {
                    Id = 9211,
                    Name = "Phường Tân Phước",
                    NormalName = "Phuong Tan Phuoc",

                    ParentId = 12,
                    PharmacyWardId = 391,
                    KMSId = 26716
                },
                new Area
                {
                    Id = 9151,
                    Name = "Xã Long Sơn",
                    NormalName = "Xa Long Son",

                    ParentId = 13,
                    PharmacyWardId = 9490,
                    KMSId = 26545
                },

                new Area
                {
                    Id = 9135,
                    Name = "Phường 1",
                    NormalName = "Phuong 1",

                    ParentId = 13,
                    PharmacyWardId = 8623,
                    KMSId = 26506
                },
                new Area
                {
                    Id = 9148,
                    Name = "Phường 10",
                    NormalName = "Phuong 10",

                    ParentId = 13,
                    PharmacyWardId = 8623,
                    KMSId = 26536
                },
                new Area
                {
                    Id = 9149,
                    Name = "Phường 11",
                    NormalName = "Phuong 11",

                    ParentId = 13,
                    PharmacyWardId = 8623,
                    KMSId = 26539
                },
                new Area
                {
                    Id = 9150,
                    Name = "Phường 12",
                    NormalName = "Phuong 12",

                    ParentId = 13,
                    PharmacyWardId = 8623,
                    KMSId = 26542
                },
                new Area
                {
                    Id = 9137,
                    Name = "Phường 2",
                    NormalName = "Phuong 2",

                    ParentId = 13,
                    PharmacyWardId = 8624,
                    KMSId = 26509
                },
                new Area
                {
                    Id = 9138,
                    Name = "Phường 3",
                    NormalName = "Phuong 3",

                    ParentId = 13,
                    PharmacyWardId = 8625,
                    KMSId = 26512
                },
                new Area
                {
                    Id = 9139,
                    Name = "Phường 4",
                    NormalName = "Phuong 4",

                    ParentId = 13,
                    PharmacyWardId = 8626,
                    KMSId = 26515
                },
                new Area
                {
                    Id = 9140,
                    Name = "Phường 5",
                    NormalName = "Phuong 5",

                    ParentId = 13,
                    PharmacyWardId = 6572,
                    KMSId = 26518
                },
                new Area
                {
                    Id = 9142,
                    Name = "Phường 7",
                    NormalName = "Phuong 7",

                    ParentId = 13,
                    PharmacyWardId = 8629,
                    KMSId = 26524
                },
                new Area
                {
                    Id = 9144,
                    Name = "Phường 8",
                    NormalName = "Phuong 8",

                    ParentId = 13,
                    PharmacyWardId = 8630,
                    KMSId = 26527
                },
                new Area
                {
                    Id = 9145,
                    Name = "Phường 9",
                    NormalName = "Phuong 9",

                    ParentId = 13,
                    PharmacyWardId = 7520,
                    KMSId = 26530
                },
                new Area
                {
                    Id = 9143,
                    Name = "Phường Nguyễn An Ninh",
                    NormalName = "Phuong Nguyen An Ninh",

                    ParentId = 13,
                    PharmacyWardId = 1892,
                    KMSId = 26526
                },
                new Area
                {
                    Id = 9147,
                    Name = "Phường Rạch Dừa",
                    NormalName = "Phuong Rach Dua",

                    ParentId = 13,
                    PharmacyWardId = 9487,
                    KMSId = 26535
                },
                new Area
                {
                    Id = 9146,
                    Name = "Phường Thắng Nhất",
                    NormalName = "Phuong Thang Nhat",

                    ParentId = 13,
                    PharmacyWardId = 9485,
                    KMSId = 26533
                },
                new Area
                {
                    Id = 9141,
                    Name = "Phường Thắng Nhì",
                    NormalName = "Phuong Thang Nhi",

                    ParentId = 13,
                    PharmacyWardId = 9491,
                    KMSId = 26521
                },
                new Area
                {
                    Id = 9136,
                    Name = "Phường Thắng Tam",
                    NormalName = "Phuong Thang Tam",

                    ParentId = 13,
                    PharmacyWardId = 9476,
                    KMSId = 26508
                },

                new Area
                {
                    Id = 9163,
                    Name = "Xã Bàu Chinh",
                    NormalName = "Xa Bau Chinh",

                    ParentId = 14,
                    PharmacyWardId = 6960,
                    KMSId = 26574
                },
                new Area
                {
                    Id = 9165,
                    Name = "Xã Bình Ba",
                    NormalName = "Xa Binh Ba",

                    ParentId = 14,
                    PharmacyWardId = 6960,
                    KMSId = 26578
                },
                new Area
                {
                    Id = 9169,
                    Name = "Xã Bình Giã",
                    NormalName = "Xa Binh Gia",

                    ParentId = 14,
                    PharmacyWardId = 9508,
                    KMSId = 26590
                },
                new Area
                {
                    Id = 9170,
                    Name = "Xã Bình Trung",
                    NormalName = "Xa Binh Trung",

                    ParentId = 14,
                    PharmacyWardId = 7191,
                    KMSId = 26593
                },
                new Area
                {
                    Id = 9172,
                    Name = "Xã Cù Bị",
                    NormalName = "Xa Cu Bi",

                    ParentId = 14,
                    PharmacyWardId = 9512,
                    KMSId = 26599
                },
                new Area
                {
                    Id = 9177,
                    Name = "Xã Đá Bạc",
                    NormalName = "Xa Da Bac",

                    ParentId = 14,
                    PharmacyWardId = 6960,
                    KMSId = 26614
                },
                new Area
                {
                    Id = 9175,
                    Name = "Xã Kim Long",
                    NormalName = "Xa Kim Long",

                    ParentId = 14,
                    PharmacyWardId = 863,
                    KMSId = 26608
                },
                new Area
                {
                    Id = 9173,
                    Name = "Xã Láng Lớn",
                    NormalName = "Xa Lang Lon",

                    ParentId = 14,
                    PharmacyWardId = 6951,
                    KMSId = 26602
                },
                new Area
                {
                    Id = 9178,
                    Name = "Xã Nghĩa Thành",
                    NormalName = "Xa Nghia Thanh",

                    ParentId = 14,
                    PharmacyWardId = 6694,
                    KMSId = 26617
                },
                new Area
                {
                    Id = 9174,
                    Name = "Xã Quảng Thành",
                    NormalName = "Xa Quang Thanh",

                    ParentId = 14,
                    PharmacyWardId = 660,
                    KMSId = 26605
                },
                new Area
                {
                    Id = 9168,
                    Name = "Xã Sơn Bình",
                    NormalName = "Xa Son Binh",

                    ParentId = 14,
                    PharmacyWardId = 4435,
                    KMSId = 26587
                },
                new Area
                {
                    Id = 9166,
                    Name = "Xã Suối Nghệ",
                    NormalName = "Xa Suoi Nghe",

                    ParentId = 14,
                    PharmacyWardId = 9505,
                    KMSId = 26581
                },
                new Area
                {
                    Id = 9176,
                    Name = "Xã Suối Rao",
                    NormalName = "Xa Suoi Rao",

                    ParentId = 14,
                    PharmacyWardId = 9516,
                    KMSId = 26611
                },
                new Area
                {
                    Id = 9171,
                    Name = "Xã Xà Bang",
                    NormalName = "Xa Xa Bang",

                    ParentId = 14,
                    PharmacyWardId = 6960,
                    KMSId = 26596
                },
                new Area
                {
                    Id = 9167,
                    Name = "Xã Xuân Sơn",
                    NormalName = "Xa Xuan Son",

                    ParentId = 14,
                    PharmacyWardId = 253,
                    KMSId = 26584
                },
                new Area
                {
                    Id = 9164,
                    Name = "Thị trấn Ngãi Giao",
                    NormalName = "Thi tran Ngai Giao",

                    ParentId = 14,
                    PharmacyWardId = 9503,
                    KMSId = 26575
                },

                new Area
                {
                    Id = 9205,
                    Name = "Xã Láng Dài",
                    NormalName = "Xa Lang Dai",

                    ParentId = 15,
                    PharmacyWardId = 6951,
                    KMSId = 26698
                },
                new Area
                {
                    Id = 9202,
                    Name = "Xã Long Mỹ",
                    NormalName = "Xa Long My",

                    ParentId = 15,
                    PharmacyWardId = 9545,
                    KMSId = 26689
                },
                new Area
                {
                    Id = 9204,
                    Name = "Xã Long Tân",
                    NormalName = "Xa Long Tan",

                    ParentId = 15,
                    PharmacyWardId = 9107,
                    KMSId = 26695
                },
                new Area
                {
                    Id = 9206,
                    Name = "Xã Lộc An",
                    NormalName = "Xa Loc An",

                    ParentId = 15,
                    PharmacyWardId = 8427,
                    KMSId = 26701
                },
                new Area
                {
                    Id = 9201,
                    Name = "Xã Phước Hội",
                    NormalName = "Xa Phuoc Hoi",

                    ParentId = 15,
                    PharmacyWardId = 9366,
                    KMSId = 26686
                },
                new Area
                {
                    Id = 9200,
                    Name = "Xã Phước Long Thọ",
                    NormalName = "Xa Phuoc Long Tho",

                    ParentId = 15,
                    PharmacyWardId = 9343,
                    KMSId = 26683
                },
                new Area
                {
                    Id = 9199,
                    Name = "Thị trấn Đất Đỏ",
                    NormalName = "Thi tran Dat Do",

                    ParentId = 15,
                    PharmacyWardId = 9542,
                    KMSId = 26680
                },
                new Area
                {
                    Id = 9203,
                    Name = "Thị trấn Phước Hải",
                    NormalName = "Thi tran Phuoc Hai",

                    ParentId = 15,
                    PharmacyWardId = 1166,
                    KMSId = 26692
                },

                new Area
                {
                    Id = 9194,
                    Name = "Xã An Ngãi",
                    NormalName = "Xa An Ngai",

                    ParentId = 16,
                    PharmacyWardId = 9552,
                    KMSId = 26665
                },
                new Area
                {
                    Id = 9196,
                    Name = "Xã An Nhứt",
                    NormalName = "Xa An Nhut",

                    ParentId = 16,
                    PharmacyWardId = 9554,
                    KMSId = 26671
                },
                new Area
                {
                    Id = 9198,
                    Name = "Xã Phước Hưng",
                    NormalName = "Xa Phuoc Hung",

                    ParentId = 16,
                    PharmacyWardId = 9492,
                    KMSId = 26677
                },
                new Area
                {
                    Id = 9197,
                    Name = "Xã Phước Tỉnh",
                    NormalName = "Xa Phuoc Tinh",

                    ParentId = 16,
                    PharmacyWardId = 8902,
                    KMSId = 26674
                },
                new Area
                {
                    Id = 9195,
                    Name = "Xã Tam Phước",
                    NormalName = "Xa Tam Phuoc",

                    ParentId = 16,
                    PharmacyWardId = 6930,
                    KMSId = 26668
                },
                new Area
                {
                    Id = 9192,
                    Name = "Thị trấn Long Điền",
                    NormalName = "Thi tran Long Dien",

                    ParentId = 16,
                    PharmacyWardId = 9122,
                    KMSId = 26659
                },
                new Area
                {
                    Id = 9193,
                    Name = "Thị trấn Long Hải",
                    NormalName = "Thi tran Long Hai",

                    ParentId = 16,
                    PharmacyWardId = 9122,
                    KMSId = 26662
                },

                new Area
                {
                    Id = 9159,
                    Name = "Phường Kim Dinh",
                    NormalName = "Phuong Kim Dinh",

                    ParentId = 17,
                    PharmacyWardId = 1198,
                    KMSId = 26566
                },
                new Area
                {
                    Id = 9158,
                    Name = "Phường Long Hương",
                    NormalName = "Phuong Long Huong",

                    ParentId = 17,
                    PharmacyWardId = 6736,
                    KMSId = 26563
                },
                new Area
                {
                    Id = 9156,
                    Name = "Phường Long Tâm",
                    NormalName = "Phuong Long Tam",

                    ParentId = 17,
                    PharmacyWardId = 6736,
                    KMSId = 26558
                },
                new Area
                {
                    Id = 9155,
                    Name = "Phường Long Toàn",
                    NormalName = "Phuong Long Toan",

                    ParentId = 17,
                    PharmacyWardId = 6736,
                    KMSId = 26557
                },
                new Area
                {
                    Id = 9153,
                    Name = "Phường Phước Hiệp",
                    NormalName = "Phuong Phuoc Hiep",

                    ParentId = 17,
                    PharmacyWardId = 7103,
                    KMSId = 26551
                },
                new Area
                {
                    Id = 9152,
                    Name = "Phường Phước Hưng",
                    NormalName = "Phuong Phuoc Hung",

                    ParentId = 17,
                    PharmacyWardId = 9492,
                    KMSId = 26548
                },
                new Area
                {
                    Id = 9154,
                    Name = "Phường Phước Nguyên",
                    NormalName = "Phuong Phuoc Nguyen",

                    ParentId = 17,
                    PharmacyWardId = 9495,
                    KMSId = 26554
                },
                new Area
                {
                    Id = 9157,
                    Name = "Phường Phước Trung",
                    NormalName = "Phuong Phuoc Trung",

                    ParentId = 17,
                    PharmacyWardId = 8876,
                    KMSId = 26560
                },
                new Area
                {
                    Id = 9162,
                    Name = "Xã Hòa Long",
                    NormalName = "Xa Hoa Long",

                    ParentId = 17,
                    PharmacyWardId = 960,
                    KMSId = 26572
                },
                new Area
                {
                    Id = 9161,
                    Name = "Xã Long Phước",
                    NormalName = "Xa Long Phuoc",

                    ParentId = 17,
                    PharmacyWardId = 8898,
                    KMSId = 26569
                },
                new Area
                {
                    Id = 9160,
                    Name = "Xã Tân Hưng",
                    NormalName = "Xa Tan Hung",

                    ParentId = 17,
                    PharmacyWardId = 145,
                    KMSId = 26567
                },

                new Area
                {
                    Id = 9185,
                    Name = "Xã Bàu Lâm",
                    NormalName = "Xa Bau Lam",

                    ParentId = 18,
                    PharmacyWardId = 6960,
                    KMSId = 26638
                },
                new Area
                {
                    Id = 9191,
                    Name = "Xã Bình Châu",
                    NormalName = "Xa Binh Chau",

                    ParentId = 18,
                    PharmacyWardId = 9531,
                    KMSId = 26656
                },
                new Area
                {
                    Id = 9183,
                    Name = "Xã Bông Trang",
                    NormalName = "Xa Bong Trang",

                    ParentId = 18,
                    PharmacyWardId = 9523,
                    KMSId = 26632
                },
                new Area
                {
                    Id = 9190,
                    Name = "Xã Bưng Riềng",
                    NormalName = "Xa Bung Rieng",

                    ParentId = 18,
                    PharmacyWardId = 6712,
                    KMSId = 26653
                },
                new Area
                {
                    Id = 9186,
                    Name = "Xã Hòa Bình",
                    NormalName = "Xa Hoa Binh",

                    ParentId = 18,
                    PharmacyWardId = 789,
                    KMSId = 26641
                },
                new Area
                {
                    Id = 9188,
                    Name = "Xã Hòa Hiệp",
                    NormalName = "Xa Hoa Hiep",

                    ParentId = 18,
                    PharmacyWardId = 8232,
                    KMSId = 26647
                },
                new Area
                {
                    Id = 9189,
                    Name = "Xã Hòa Hội",
                    NormalName = "Xa Hoa Hoi",

                    ParentId = 18,
                    PharmacyWardId = 7580,
                    KMSId = 26650
                },
                new Area
                {
                    Id = 9187,
                    Name = "Xã Hòa Hưng",
                    NormalName = "Xa Hoa Hung",

                    ParentId = 18,
                    PharmacyWardId = 9527,
                    KMSId = 26644
                },
                new Area
                {
                    Id = 9181,
                    Name = "Xã Phước Tân",
                    NormalName = "Xa Phuoc Tan",

                    ParentId = 18,
                    PharmacyWardId = 7566,
                    KMSId = 26626
                },
                new Area
                {
                    Id = 9180,
                    Name = "Xã Phước Thuận",
                    NormalName = "Xa Phuoc Thuan",

                    ParentId = 18,
                    PharmacyWardId = 6691,
                    KMSId = 26623
                },
                new Area
                {
                    Id = 9184,
                    Name = "Xã Tân Lâm",
                    NormalName = "Xa Tan Lam",

                    ParentId = 18,
                    PharmacyWardId = 1130,
                    KMSId = 26635
                },
                new Area
                {
                    Id = 9182,
                    Name = "Xã Xuyên Mộc",
                    NormalName = "Xa Xuyen Moc",

                    ParentId = 18,
                    PharmacyWardId = 9522,
                    KMSId = 26629
                },
                new Area
                {
                    Id = 9179,
                    Name = "Thị trấn Phước Bửu",
                    NormalName = "Thi tran Phuoc Buu",

                    ParentId = 18,
                    PharmacyWardId = 1166,
                    KMSId = 26620
                },
                new Area
                {
                    Id = 11164,
                    Name = "Đảo Côn Sơn",
                    NormalName = "Dao Con Son",

                    ParentId = 19,
                    PharmacyWardId = 9557
                },
                new Area
                {
                    Id = 204,
                    Name = "Gia Lai - Huyện Ia Grai",
                    NormalName = "Gia Lai - Huyen Ia Grai",

                    KMSId = 628
                },
                new Area
                {
                    Id = 205,
                    Name = "Gia Lai - Huyện Đức Cơ",
                    NormalName = "Gia Lai - Huyen Duc Co",

                    KMSId = 631
                },
                new Area
                {
                    Id = 206,
                    Name = "Gia Lai - Huyện Chư Prông",
                    NormalName = "Gia Lai - Huyen Chu Prong",

                    KMSId = 632
                },
                new Area
                {
                    Id = 207,
                    Name = "Gia Lai - Huyện Chư Sê",
                    NormalName = "Gia Lai - Huyen Chu Se",

                    KMSId = 633
                },
                new Area
                {
                    Id = 208,
                    Name = "Gia Lai - Huyện Phú Thiện",
                    NormalName = "Gia Lai - Huyen Phu Thien",

                    KMSId = 638
                },
                new Area
                {
                    Id = 209,
                    Name = "Gia Lai - Thị xã Ayun Pa",
                    NormalName = "Gia Lai - Thi xa Ayun Pa",

                    KMSId = 624
                },
                new Area
                {
                    Id = 210,
                    Name = "Gia Lai - Huyện Ia Pa",
                    NormalName = "Gia Lai - Huyen Ia Pa",

                    KMSId = 635
                },
                new Area
                {
                    Id = 211,
                    Name = "Gia Lai - Thị xã An Khê",
                    NormalName = "Gia Lai - Thi xa An Khe",

                    KMSId = 623
                },
                new Area
                {
                    Id = 212,
                    Name = "Gia Lai - Huyện Chư Păh",
                    NormalName = "Gia Lai - Huyen Chu Pah",

                    KMSId = 627
                },
                new Area
                {
                    Id = 213,
                    Name = "Gia Lai - Thành phố Pleiku",
                    NormalName = "Gia Lai - Thanh pho Pleiku",

                    KMSId = 622
                },
                new Area
                {
                    Id = 214,
                    Name = "Gia Lai - Huyện Đăk Đoa",
                    NormalName = "Gia Lai - Huyen Dak Doa",

                    KMSId = 626
                },
                new Area
                {
                    Id = 215,
                    Name = "Gia Lai - Huyện Đăk Pơ",
                    NormalName = "Gia Lai - Huyen Dak Po",

                    KMSId = 634
                },
                new Area
                {
                    Id = 216,
                    Name = "Gia Lai - Huyện KBang",
                    NormalName = "Gia Lai - Huyen KBang",

                    KMSId = 625
                },
                new Area
                {
                    Id = 217,
                    Name = "Gia Lai - Huyện Kông Chro",
                    NormalName = "Gia Lai - Huyen Kong Chro",

                    KMSId = 630
                },
                new Area
                {
                    Id = 218,
                    Name = "Gia Lai - Huyện Krông Pa",
                    NormalName = "Gia Lai - Huyen Krong Pa",

                    KMSId = 637
                },
                new Area
                {
                    Id = 219,
                    Name = "Gia Lai - Huyện Mang Yang",
                    NormalName = "Gia Lai - Huyen Mang Yang",

                    KMSId = 629
                },
                new Area
                {
                    Id = 220,
                    Name = "Gia Lai - Huyện Chư Pưh",
                    NormalName = "Gia Lai - Huyen Chu Puh",

                    KMSId = 639
                },
                new Area
                {
                    Id = 221,
                    Name = "Hà Giang - Huyện Bắc Quang",
                    NormalName = "Ha Giang - Huyen Bac Quang",

                    KMSId = 34
                },
                new Area
                {
                    Id = 222,
                    Name = "Hà Giang - Thành phố Hà Giang",
                    NormalName = "Ha Giang - Thanh pho Ha Giang",

                    KMSId = 24
                },
                new Area
                {
                    Id = 223,
                    Name = "Hà Giang - Huyện Hoàng Su Phì",
                    NormalName = "Ha Giang - Huyen Hoang Su Phi",

                    KMSId = 32
                },
                new Area
                {
                    Id = 224,
                    Name = "Hà Giang - Huyện Đồng Văn",
                    NormalName = "Ha Giang - Huyen Dong Van",

                    KMSId = 26
                },
                new Area
                {
                    Id = 225,
                    Name = "Hà Giang - Huyện Mèo Vạc",
                    NormalName = "Ha Giang - Huyen Meo Vac",

                    KMSId = 27
                },
                new Area
                {
                    Id = 226,
                    Name = "Hà Giang - Huyện Xín Mần",
                    NormalName = "Ha Giang - Huyen Xin Man",

                    KMSId = 33
                },
                new Area
                {
                    Id = 227,
                    Name = "Hà Giang - Huyện Yên Minh",
                    NormalName = "Ha Giang - Huyen Yen Minh",

                    KMSId = 28
                },
                new Area
                {
                    Id = 228,
                    Name = "Hà Giang - Huyện Quang Bình",
                    NormalName = "Ha Giang - Huyen Quang Binh",

                    KMSId = 35
                },
                new Area
                {
                    Id = 229,
                    Name = "Hà Giang - Huyện Quản Bạ",
                    NormalName = "Ha Giang - Huyen Quan Ba",

                    KMSId = 29
                },
                new Area
                {
                    Id = 230,
                    Name = "Hà Giang - Huyện Bắc Mê",
                    NormalName = "Ha Giang - Huyen Bac Me",

                    KMSId = 31
                },
                new Area
                {
                    Id = 231,
                    Name = "Hà Giang - Huyện Vị Xuyên",
                    NormalName = "Ha Giang - Huyen Vi Xuyen",

                    KMSId = 30
                },

                new Area
                {
                    Id = 232,
                    Name = "Hà Nam - Huyện Thanh Liêm",
                    NormalName = "Ha Nam - Huyen Thanh Liem",

                    KMSId = 351
                },
                new Area
                {
                    Id = 233,
                    Name = "Hà Nam - Huyện Lý Nhân",
                    NormalName = "Ha Nam - Huyen Ly Nhan",

                    KMSId = 353
                },
                new Area
                {
                    Id = 234,
                    Name = "Hà Nam - Huyện Kim Bảng",
                    NormalName = "Ha Nam - Huyen Kim Bang",

                    KMSId = 350
                },
                new Area
                {
                    Id = 235,
                    Name = "Hà Nam - Thị xã Duy Tiên",
                    NormalName = "Ha Nam - Thi xa Duy Tien",

                    KMSId = 349
                },
                new Area
                {
                    Id = 236,
                    Name = "Hà Nam - Huyện Bình Lục",
                    NormalName = "Ha Nam - Huyen Binh Luc",

                    KMSId = 352
                },
                new Area
                {
                    Id = 237,
                    Name = "Hà Nam - Thành phố Phủ Lý",
                    NormalName = "Ha Nam - Thanh pho Phu Ly",

                    KMSId = 347
                },

                new Area
                {
                    Id = 238,
                    Name = "Hà Nội - Quận Hà Đông",
                    NormalName = "Ha Noi - Quan Ha Dong",

                    KMSId = 268
                },
                new Area
                {
                    Id = 239,
                    Name = "Hà Nội - Quận Ba Đình",
                    NormalName = "Ha Noi - Quan Ba Dinh",

                    KMSId = 1
                },
                new Area
                {
                    Id = 240,
                    Name = "Hà Nội - Quận Cầu Giấy",
                    NormalName = "Ha Noi - Quan Cau Giay",

                    KMSId = 5
                },
                new Area
                {
                    Id = 241,
                    Name = "Hà Nội - Quận Đống Đa",
                    NormalName = "Ha Noi - Quan Dong Da",

                    KMSId = 6
                },
                new Area
                {
                    Id = 242,
                    Name = "Hà Nội - Quận Hai Bà Trưng",
                    NormalName = "Ha Noi - Quan Hai Ba Trung",

                    KMSId = 7
                },
                new Area
                {
                    Id = 243,
                    Name = "Hà Nội - Quận Hoàn Kiếm",
                    NormalName = "Ha Noi - Quan Hoan Kiem",

                    KMSId = 2
                },
                new Area
                {
                    Id = 244,
                    Name = "Hà Nội - Quận Hoàng Mai",
                    NormalName = "Ha Noi - Quan Hoang Mai",

                    KMSId = 8
                },
                new Area
                {
                    Id = 245,
                    Name = "Hà Nội - Quận Long Biên",
                    NormalName = "Ha Noi - Quan Long Bien",

                    KMSId = 4
                },
                new Area
                {
                    Id = 246,
                    Name = "Hà Nội - Quận Tây Hồ",
                    NormalName = "Ha Noi - Quan Tay Ho",

                    KMSId = 3
                },
                new Area
                {
                    Id = 247,
                    Name = "Hà Nội - Quận Thanh Xuân",
                    NormalName = "Ha Noi - Quan Thanh Xuan",

                    KMSId = 9
                },
                new Area
                {
                    Id = 248,
                    Name = "Hà Nội - Huyện Mê Linh",
                    NormalName = "Ha Noi - Huyen Me Linh",

                    KMSId = 250
                },
                new Area
                {
                    Id = 249,
                    Name = "Hà Nội - Huyện Đông Anh",
                    NormalName = "Ha Noi - Huyen Dong Anh",

                    KMSId = 17
                },
                new Area
                {
                    Id = 250,
                    Name = "Hà Nội - Huyện Sóc Sơn",
                    NormalName = "Ha Noi - Huyen Soc Son",

                    KMSId = 16
                },
                new Area
                {
                    Id = 251,
                    Name = "Hà Nội - Quận Bắc Từ Liêm",
                    NormalName = "Ha Noi - Quan Bac Tu Liem",

                    KMSId = 21
                },
                new Area
                {
                    Id = 252,
                    Name = "Hà Nội - Huyện Gia Lâm",
                    NormalName = "Ha Noi - Huyen Gia Lam",

                    KMSId = 18
                },
                new Area
                {
                    Id = 253,
                    Name = "Hà Nội - Huyện Thanh Trì",
                    NormalName = "Ha Noi - Huyen Thanh Tri",

                    KMSId = 20
                },
                new Area
                {
                    Id = 254,
                    Name = "Hà Nội - Thị xã Sơn Tây",
                    NormalName = "Ha Noi - Thi xa Son Tay",

                    KMSId = 269
                },
                new Area
                {
                    Id = 255,
                    Name = "Hà Nội - Quận Nam Từ Liêm",
                    NormalName = "Ha Noi - Quan Nam Tu Liem",

                    KMSId = 19
                },
                new Area
                {
                    Id = 256,
                    Name = "Hà Nội - Huyện Phú Xuyên",
                    NormalName = "Ha Noi - Huyen Phu Xuyen",

                    KMSId = 280
                },
                new Area
                {
                    Id = 257,
                    Name = "Hà Nội - Huyện Ba Vì",
                    NormalName = "Ha Noi - Huyen Ba Vi",

                    KMSId = 271
                },
                new Area
                {
                    Id = 258,
                    Name = "Hà Nội - Huyện Đan Phượng",
                    NormalName = "Ha Noi - Huyen Dan Phuong",

                    KMSId = 273
                },
                new Area
                {
                    Id = 259,
                    Name = "Hà Nội - Huyện Hoài Đức",
                    NormalName = "Ha Noi - Huyen Hoai Duc",

                    KMSId = 274
                },
                new Area
                {
                    Id = 260,
                    Name = "Hà Nội - Huyện Mỹ Đức",
                    NormalName = "Ha Noi - Huyen My Duc",

                    KMSId = 282
                },
                new Area
                {
                    Id = 261,
                    Name = "Hà Nội - Huyện Phúc Thọ",
                    NormalName = "Ha Noi - Huyen Phuc Tho",

                    KMSId = 272
                },
                new Area
                {
                    Id = 262,
                    Name = "Hà Nội - Huyện Thạch Thất",
                    NormalName = "Ha Noi - Huyen Thach That",

                    KMSId = 276
                },
                new Area
                {
                    Id = 263,
                    Name = "Hà Nội - Huyện Thanh Oai",
                    NormalName = "Ha Noi - Huyen Thanh Oai",

                    KMSId = 278
                },
                new Area
                {
                    Id = 264,
                    Name = "Hà Nội - Huyện Ứng Hòa",
                    NormalName = "Ha Noi - Huyen Ung Hoa",

                    KMSId = 281
                },
                new Area
                {
                    Id = 265,
                    Name = "Hà Nội - Huyện Chương Mỹ",
                    NormalName = "Ha Noi - Huyen Chuong My",

                    KMSId = 277
                },
                new Area
                {
                    Id = 266,
                    Name = "Hà Nội - Huyện Quốc Oai",
                    NormalName = "Ha Noi - Huyen Quoc Oai",

                    KMSId = 275
                },
                new Area
                {
                    Id = 267,
                    Name = "Hà Nội - Huyện Thường Tín",
                    NormalName = "Ha Noi - Huyen Thuong Tin",

                    KMSId = 279
                },

                new Area
                {
                    Id = 268,
                    Name = "Hà Tĩnh - Huyện Lộc Hà",
                    NormalName = "Ha Tinh - Huyen Loc Ha",

                    KMSId = 448
                },
                new Area
                {
                    Id = 269,
                    Name = "Hà Tĩnh - Huyện Kỳ Anh",
                    NormalName = "Ha Tinh - Huyen Ky Anh",

                    KMSId = 447
                },
                new Area
                {
                    Id = 270,
                    Name = "Hà Tĩnh - Huyện Hương Khê",
                    NormalName = "Ha Tinh - Huyen Huong Khe",

                    KMSId = 444
                },
                new Area
                {
                    Id = 271,
                    Name = "Hà Tĩnh - Huyện Nghi Xuân",
                    NormalName = "Ha Tinh - Huyen Nghi Xuan",

                    KMSId = 442
                },
                new Area
                {
                    Id = 272,
                    Name = "Hà Tĩnh - Thị xã Hồng Lĩnh",
                    NormalName = "Ha Tinh - Thi xa Hong Linh",

                    KMSId = 437
                },
                new Area
                {
                    Id = 273,
                    Name = "Hà Tĩnh - Huyện Cẩm Xuyên",
                    NormalName = "Ha Tinh - Huyen Cam Xuyen",

                    KMSId = 446
                },
                new Area
                {
                    Id = 274,
                    Name = "Hà Tĩnh - Thành phố Hà Tĩnh",
                    NormalName = "Ha Tinh - Thanh pho Ha Tinh",

                    KMSId = 436
                },
                new Area
                {
                    Id = 275,
                    Name = "Hà Tĩnh - Huyện Đức Thọ",
                    NormalName = "Ha Tinh - Huyen Duc Tho",

                    KMSId = 440
                },
                new Area
                {
                    Id = 276,
                    Name = "Hà Tĩnh - Huyện Can Lộc",
                    NormalName = "Ha Tinh - Huyen Can Loc",

                    KMSId = 443
                },
                new Area
                {
                    Id = 277,
                    Name = "Hà Tĩnh - Huyện Thạch Hà",
                    NormalName = "Ha Tinh - Huyen Thach Ha",

                    KMSId = 445
                },
                new Area
                {
                    Id = 278,
                    Name = "Hà Tĩnh - Huyện Hương Sơn",
                    NormalName = "Ha Tinh - Huyen Huong Son",

                    KMSId = 439
                },
                new Area
                {
                    Id = 726,
                    Name = "Hà Tĩnh - Huyện Vũ Quang",
                    NormalName = "Ha Tinh - Huyen Vu Quang",

                    KMSId = 441
                },
                new Area
                {
                    Id = 727,
                    Name = "Hà Tĩnh - Thị xã Kỳ Anh",
                    NormalName = "Ha Tinh - Thi xa Ky Anh",

                    KMSId = 449
                },
            };
            return listArea;
        }
    }
}