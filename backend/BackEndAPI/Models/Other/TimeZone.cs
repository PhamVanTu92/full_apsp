using BackEndAPI.Models.Banks;

namespace BackEndAPI.Models.Other
{
    public class TimeZones
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string TimeZone { get; set; }
    }
    public static class TimeZonesList
    {
        public static List<TimeZones> GetTimeZones()
        {
            List<TimeZones> listTimeZones = new List<TimeZones>
            {
                 new TimeZones{
                    Id =  1,
                    Name =  "(UTC-12:00) International Date Line West",
                    TimeZone =  "Etc/GMT+12"
                  },
                  new TimeZones{
                    Id =  2,
                    Name =  "(UTC-11:00) Coordinated Universal Time-11",
                    TimeZone =  "Etc/GMT+11"
                  },
                  new TimeZones{
                    Id =  3,
                    Name =  "(UTC-10:00) Aleutian Islands",
                    TimeZone =  "America/Adak"
                  },
                  new TimeZones{
                    Id =  4,
                    Name =  "(UTC-10:00) Hawaii",
                    TimeZone =  "Pacific/Honolulu"
                  },
                  new TimeZones{
                    Id =  5,
                    Name =  "(UTC-09:30) Marquesas Islands",
                    TimeZone =  "Pacific/Marquesas"
                  },
                  new TimeZones{
                    Id =  6,
                    Name =  "(UTC-09:00) Alaska",
                    TimeZone =  "America/Anchorage"
                  },
                  new TimeZones{
                    Id =  7,
                    Name =  "(UTC-09:00) Coordinated Universal Time-09",
                    TimeZone =  "Etc/GMT+9"
                  },
                  new TimeZones{
                    Id =  8,
                    Name =  "(UTC-08:00) Baja California",
                    TimeZone =  "America/Tijuana"
                  },
                  new TimeZones{
                    Id =  9,
                    Name =  "(UTC-08:00) Coordinated Universal Time-08",
                    TimeZone =  "Etc/GMT+8"
                  },
                  new TimeZones{
                    Id =  10,
                    Name =  "(UTC-08:00) Pacific Time (US & Canada)",
                    TimeZone =  "America/Los_Angeles"
                  },
                  new TimeZones{
                    Id =  11,
                    Name =  "(UTC-07:00) Arizona",
                    TimeZone =  "America/Phoenix"
                  },
                  new TimeZones{
                    Id =  12,
                    Name =  "(UTC-07:00) La Paz, Mazatlan",
                    TimeZone =  "America/Mazatlan"
                  },
                  new TimeZones{
                    Id =  13,
                    Name =  "(UTC-07:00) Mountain Time (US & Canada)",
                    TimeZone =  "America/Boise"
                  },
                  new TimeZones{
                    Id =  14,
                    Name =  "(UTC-07:00) Yukon",
                    TimeZone =  "America/Whitehorse"
                  },
                  new TimeZones{
                    Id =  15,
                    Name =  "(UTC-06:00) Central America",
                    TimeZone =  "America/Merida"
                  },
                  new TimeZones{
                    Id =  16,
                    Name =  "(UTC-06:00) Central Time (US & Canada)",
                    TimeZone =  "America/Chicago"
                  },
                  new TimeZones{
                    Id =  17,
                    Name =  "(UTC-06:00) Easter Island",
                    TimeZone =  "Pacific/Easter"
                  },
                  new TimeZones{
                    Id =  18,
                    Name =  "(UTC-06:00) Guadalajara, Mexico City, Monterrey",
                    TimeZone =  "America/Mexico_City"
                  },
                  new TimeZones{
                    Id =  19,
                    Name =  "(UTC-06:00) Saskatchewan",
                    TimeZone =  "America/Regina"
                  },
                  new TimeZones{
                    Id =  20,
                    Name =  "(UTC-05:00) Bogota, Lima, Quito, Rio Branco",
                    TimeZone =  "America/Bogota"
                  },
                  new TimeZones{
                    Id =  21,
                    Name =  "(UTC-05:00) Chetumal",
                    TimeZone =  "America/Cancun"
                  },
                  new TimeZones{
                    Id =  22,
                    Name =  "(UTC-05:00) Eastern Time (US & Canada)",
                    TimeZone =  "America/Detroit"
                  },
                  new TimeZones{
                    Id =  23,
                    Name =  "(UTC-05:00) Haiti",
                    TimeZone =  "America/Port-au-Prince"
                  },
                  new TimeZones{
                    Id =  24,
                    Name =  "(UTC-05:00) Havana",
                    TimeZone =  "America/Havana"
                  },
                  new TimeZones{
                    Id =  25,
                    Name =  "(UTC-05:00) Indiana (East)",
                    TimeZone =  "America/Indiana/Indianapolis"
                  },
                  new TimeZones{
                    Id =  26,
                    Name =  "(UTC-05:00) Turks and Caicos",
                    TimeZone =  "America/Grand_Turk"
                  },
                  new TimeZones{
                    Id =  27,
                    Name =  "(UTC-04:00) Asuncion",
                    TimeZone =  "America/Asuncion"
                  },
                  new TimeZones{
                    Id =  28,
                    Name =  "(UTC-04:00) Atlantic Time (Canada)",
                    TimeZone =  "America/Glace_Bay"
                  },
                  new TimeZones{
                    Id =  29,
                    Name =  "(UTC-04:00) Caracas",
                    TimeZone =  "America/Caracas"
                  },
                  new TimeZones{
                    Id =  30,
                    Name =  "(UTC-04:00) Cuiaba",
                    TimeZone =  "America/Cuiaba"
                  },
                  new TimeZones{
                    Id =  31,
                    Name =  "(UTC-04:00) Georgetown, La Paz, Manaus, San Juan",
                    TimeZone =  "America/Guyana"
                  },
                  new TimeZones{
                    Id =  32,
                    Name =  "(UTC-04:00) Santiago",
                    TimeZone =  "America/Santiago"
                  },
                  new TimeZones{
                    Id =  33,
                    Name =  "(UTC-03:30) Newfoundland",
                    TimeZone =  "America/St_Johns"
                  },
                  new TimeZones{
                    Id =  34,
                    Name =  "(UTC-03:00) Araguaina",
                    TimeZone =  "America/Araguaina"
                  },
                  new TimeZones{
                    Id =  35,
                    Name =  "(UTC-03:00) Brasilia",
                    TimeZone =  "America/Araguaina"
                  },
                  new TimeZones{
                    Id =  36,
                    Name =  "(UTC-03:00) Cayenne, Fortaleza",
                    TimeZone =  "America/Cayenne"
                  },
                  new TimeZones{
                    Id =  37,
                    Name =  "(UTC-03:00) Buenos Aires",
                    TimeZone =  "America/Argentina/Buenos_Aires"
                  },
                  new TimeZones{
                    Id =  38,
                    Name =  "(UTC-03:00) Greenland",
                    TimeZone =  "America/Nuuk"
                  },
                  new TimeZones{
                    Id =  39,
                    Name =  "(UTC-03:00) Montevideo",
                    TimeZone =  "America/Montevideo"
                  },
                  new TimeZones{
                    Id =  40,
                    Name =  "(UTC-03:00) Punta Arenas",
                    TimeZone =  "America/Punta_Arenas"
                  },
                  new TimeZones{
                    Id =  41,
                    Name =  "(UTC-03:00) Saint Pierre and Miquelon",
                    TimeZone =  "America/Miquelon"
                  },
                  new TimeZones{
                    Id =  42,
                    Name =  "(UTC-03:00) Salvador",
                    TimeZone =  "America/Bahia"
                  },
                  new TimeZones{
                    Id =  43,
                    Name =  "(UTC-02:00) Coordinated Universal Time-02",
                    TimeZone =  "Etc/GMT+2"
                  },
                  new TimeZones{
                    Id =  44,
                    Name =  "(UTC-01:00) Azores",
                    TimeZone =  "Atlantic/Azores"
                  },
                  new TimeZones{
                    Id =  45,
                    Name =  "(UTC-01:00) Cabo Verde Is.",
                    TimeZone =  "Atlantic/Cape_Verde"
                  },
                  new TimeZones{
                    Id =  46,
                    Name =  "(UTC) Coordinated Universal Time",
                    TimeZone =  "Etc/UTC"
                  },
                  new TimeZones{
                    Id =  47,
                    Name =  "(UTC+00:00) Dublin, Edinburgh, Lisbon, London",
                    TimeZone =  "Europe/Dublin"
                  },
                  new TimeZones{
                    Id =  48,
                    Name =  "(UTC+00:00) Monrovia, Reykjavik",
                    TimeZone =  "Africa/Monrovia"
                  },
                  new TimeZones{
                    Id =  49,
                    Name =  "(UTC+00:00) Sao Tome",
                    TimeZone =  "Africa/Sao_Tome"
                  },
                  new TimeZones{
                    Id =  50,
                    Name =  "(UTC+01:00) Casablanca",
                    TimeZone =  "Africa/Casablanca"
                  },
                  new TimeZones{
                    Id =  51,
                    Name =  "(UTC+01:00) Amsterdam, Berlin, Bern, Rome, Stockholm, Vienna",
                    TimeZone =  "Europe/Amsterdam"
                  },
                  new TimeZones{
                    Id =  52,
                    Name =  "(UTC+01:00) Belgrade, Bratislava, Budapest, Ljubljana, Prague",
                    TimeZone =  "Europe/Belgrade"
                  },
                  new TimeZones{
                    Id =  53,
                    Name =  "(UTC+01:00) Brussels, Copenhagen, Madrid, Paris",
                    TimeZone =  "Europe/Brussels"
                  },
                  new TimeZones{
                    Id =  54,
                    Name =  "(UTC+01:00) Sarajevo, Skopje, Warsaw, Zagreb",
                    TimeZone =  "Europe/Sarajevo"
                  },
                  new TimeZones{
                    Id =  55,
                    Name =  "(UTC+01:00) West Central Africa",
                    TimeZone =  "Africa/Algiers"
                  },
                  new TimeZones{
                    Id =  56,
                    Name =  "(UTC+02:00) Athens, Bucharest",
                    TimeZone =  "Europe/Athens"
                  },
                  new TimeZones{
                    Id =  57,
                    Name =  "(UTC+02:00) Beirut",
                    TimeZone =  "Asia/Beirut"
                  },
                  new TimeZones{
                    Id =  58,
                    Name =  "(UTC+02:00) Cairo",
                    TimeZone =  "Africa/Cairo"
                  },
                  new TimeZones{
                    Id =  59,
                    Name =  "(UTC+02:00) Chisinau",
                    TimeZone =  "Europe/Chisinau"
                  },
                  new TimeZones{
                    Id =  60,
                    Name =  "(UTC+02:00) Damascus",
                    TimeZone =  "Asia/Damascus"
                  },
                  new TimeZones{
                    Id =  61,
                    Name =  "(UTC+02:00) Gaza, Hebron",
                    TimeZone =  "Africa/Maputo"
                  },
                  new TimeZones{
                    Id =  62,
                    Name =  "(UTC+02:00) Harare, Pretoria",
                    TimeZone =  "Africa/Harare"
                  },
                  new TimeZones{
                    Id =  63,
                    Name =  "(UTC+02:00) Helsinki, Kyiv, Riga, Sofia, Tallinn, Vilnius",
                    TimeZone =  "Europe/Helsinki"
                  },
                  new TimeZones{
                    Id =  64,
                    Name =  "(UTC+02:00) Jerusalem",
                    TimeZone =  "Asia/Jerusalem"
                  },
                  new TimeZones{
                    Id =  65,
                    Name =  "(UTC+02:00) Juba",
                    TimeZone =  "Africa/Juba"
                  },
                  new TimeZones{
                    Id =  66,
                    Name =  "(UTC+02:00) Kaliningrad",
                    TimeZone =  "Europe/Kaliningrad"
                  },
                  new TimeZones{
                    Id =  67,
                    Name =  "(UTC+02:00) Khartoum",
                    TimeZone =  "Africa/Khartoum"
                  },
                  new TimeZones{
                    Id =  68,
                    Name =  "(UTC+02:00) Tripoli",
                    TimeZone =  "Africa/Tripoli"
                  },
                  new TimeZones{
                    Id =  69,
                    Name =  "(UTC+02:00) Windhoek",
                    TimeZone =  "Africa/Windhoek"
                  },
                  new TimeZones{
                    Id =  70,
                    Name =  "(UTC+03:00) Amman",
                    TimeZone =  "Asia/Amman"
                  },
                  new TimeZones{
                    Id =  71,
                    Name =  "(UTC+03:00) Baghdad",
                    TimeZone =  "Asia/Baghdad"
                  },
                  new TimeZones{
                    Id =  72,
                    Name =  "(UTC+03:00) Istanbul",
                    TimeZone =  "Europe/Istanbul"
                  },
                  new TimeZones{
                    Id =  73,
                    Name =  "(UTC+03:00) Kuwait, Riyadh",
                    TimeZone =  "Asia/Kuwait"
                  },
                  new TimeZones{
                    Id =  74,
                    Name =  "(UTC+03:00) Minsk",
                    TimeZone =  "Europe/Minsk"
                  },
                  new TimeZones{
                    Id =  75,
                    Name =  "(UTC+03:00) Moscow, St. Petersburg",
                    TimeZone =  "Europe/Moscow"
                  },
                  new TimeZones{
                    Id =  76,
                    Name =  "(UTC+03:00) Nairobi",
                    TimeZone =  "Africa/Nairobi"
                  },
                  new TimeZones{
                    Id =  77,
                    Name =  "(UTC+03:00) Volgograd",
                    TimeZone =  "Europe/Volgograd"
                  },
                  new TimeZones{
                    Id =  78,
                    Name =  "(UTC+03:30) Tehran",
                    TimeZone =  "Asia/Tehran"
                  },
                  new TimeZones{
                    Id =  79,
                    Name =  "(UTC+04:00) Abu Dhabi, Muscat",
                    TimeZone =  "Asia/Dubai"
                  },
                  new TimeZones{
                    Id =  80,
                    Name =  "(UTC+04:00) Astrakhan, Ulyanovsk",
                    TimeZone =  "Europe/Samara"
                  },
                  new TimeZones{
                    Id =  81,
                    Name =  "(UTC+04:00) Baku",
                    TimeZone =  "Asia/Baku"
                  },
                  new TimeZones{
                    Id =  82,
                    Name =  "(UTC+04:00) Izhevsk, Samara",
                    TimeZone =  "Europe/Samara"
                  },
                  new TimeZones{
                    Id =  83,
                    Name =  "(UTC+04:00) Port Louis",
                    TimeZone =  "Indian/Mauritius"
                  },
                  new TimeZones{
                    Id =  84,
                    Name =  "(UTC+04:00) Saratov",
                    TimeZone =  "Europe/Saratov"
                  },
                  new TimeZones{
                    Id =  85,
                    Name =  "(UTC+04:00) Tbilisi",
                    TimeZone =  "Asia/Tbilisi"
                  },
                  new TimeZones{
                    Id =  86,
                    Name =  "(UTC+04:00) Yerevan",
                    TimeZone =  "Asia/Yerevan"
                  },
                  new TimeZones{
                    Id =  87,
                    Name =  "(UTC+04:30) Kabul",
                    TimeZone =  "Asia/Kabul"
                  },
                  new TimeZones{
                    Id =  88,
                    Name =  "(UTC+05:00) Ashgabat, Tashkent",
                    TimeZone =  "Asia/Ashgabat"
                  },
                  new TimeZones{
                    Id =  89,
                    Name =  "(UTC+05:00) Ekaterinburg",
                    TimeZone =  "Asia/Yekaterinburg"
                  },
                  new TimeZones{
                    Id =  90,
                    Name =  "(UTC+05:00) Islamabad, Karachi",
                    TimeZone =  "Asia/Karachi"
                  },
                  new TimeZones{
                    Id =  91,
                    Name =  "(UTC+05:00) Qyzylorda",
                    TimeZone =  "Asia/Qyzylorda"
                  },
                  new TimeZones{
                    Id =  92,
                    Name =  "(UTC+05:30) Chennai, Kolkata, Mumbai, New Delhi",
                    TimeZone =  "Asia/Kolkata"
                  },
                  new TimeZones{
                    Id =  93,
                    Name =  "(UTC+05:30) Sri Jayawardenepura",
                    TimeZone =  "Asia/Colombo"
                  },
                  new TimeZones{
                    Id =  94,
                    Name =  "(UTC+05:45) Kathmandu",
                    TimeZone =  "Asia/Kathmandu"
                  },
                  new TimeZones{
                    Id =  95,
                    Name =  "(UTC+06:00) Astana",
                    TimeZone =  "Asia/Almaty"
                  },
                  new TimeZones{
                    Id =  96,
                    Name =  "(UTC+06:00) Dhaka",
                    TimeZone =  "Asia/Dhaka"
                  },
                  new TimeZones{
                    Id =  97,
                    Name =  "(UTC+06:00) Omsk",
                    TimeZone =  "Asia/Omsk"
                  },
                  new TimeZones{
                    Id =  98,
                    Name =  "(UTC+06:30) Yangon (Rangoon)",
                    TimeZone =  "Asia/Yangon"
                  },
                  new TimeZones{
                    Id =  99,
                    Name =  "(UTC+07:00) Bangkok, Hanoi, Jakarta",
                    TimeZone =  "Asia/Bangkok"
                  },
                  new TimeZones{
                    Id =  100,
                    Name =  "(UTC+07:00) Barnaul, Gorno-Altaysk",
                    TimeZone =  "Asia/Krasnoyarsk"
                  },
                  new TimeZones{
                    Id =  101,
                    Name =  "(UTC+07:00) Hovd",
                    TimeZone =  "Asia/Hovd"
                  },
                  new TimeZones{
                    Id =  102,
                    Name =  "(UTC+07:00) Krasnoyarsk",
                    TimeZone =  "Asia/Krasnoyarsk"
                  },
                  new TimeZones{
                    Id =  103,
                    Name =  "(UTC+07:00) Novosibirsk",
                    TimeZone =  "Asia/Novosibirsk"
                  },
                  new TimeZones{
                    Id =  104,
                    Name =  "(UTC+07:00) Tomsk",
                    TimeZone =  "Asia/Omsk"
                  },
                  new TimeZones{
                    Id =  105,
                    Name =  "(UTC+08:00) Beijing, Chongqing, Hong Kong, Urumqi",
                    TimeZone =  "Asia/Shanghai"
                  },
                  new TimeZones{
                    Id =  106,
                    Name =  "(UTC+08:00) Irkutsk",
                    TimeZone =  "Asia/Irkutsk"
                  },
                  new TimeZones{
                    Id =  107,
                    Name =  "(UTC+08:00) Kuala Lumpur, Singapore",
                    TimeZone =  "Asia/Kuala_Lumpur"
                  },
                  new TimeZones{
                    Id =  108,
                    Name =  "(UTC+08:00) Manila",
                    TimeZone =  "Asia/Manila"
                  },
                  new TimeZones{
                    Id =  109,
                    Name =  "(UTC+08:00) Perth",
                    TimeZone =  "Australia/Perth"
                  },
                  new TimeZones{
                    Id =  110,
                    Name =  "(UTC+08:00) Taipei",
                    TimeZone =  "Asia/Taipei"
                  },
                  new TimeZones{
                    Id =  111,
                    Name =  "(UTC+08:00) Ulaanbaatar",
                    TimeZone =  "Asia/Ulaanbaatar"
                  },
                  new TimeZones{
                    Id =  112,
                    Name =  "(UTC+08:45) Eucla",
                    TimeZone =  "Australia/Eucla"
                  },
                  new TimeZones{
                    Id =  113,
                    Name =  "(UTC+09:00) Chita",
                    TimeZone =  "Asia/Chita"
                  },
                  new TimeZones{
                    Id =  114,
                    Name =  "(UTC+09:00) Osaka, Sapporo, Tokyo",
                    TimeZone =  "Asia/Tokyo"
                  },
                  new TimeZones{
                    Id =  115,
                    Name =  "(UTC+09:00) Pyongyang",
                    TimeZone =  "Asia/Pyongyang"
                  },
                  new TimeZones{
                    Id =  116,
                    Name =  "(UTC+09:00) Seoul",
                    TimeZone =  "Asia/Seoul"
                  },
                  new TimeZones{
                    Id =  117,
                    Name =  "(UTC+09:00) Yakutsk",
                    TimeZone =  "Asia/Yakutsk"
                  },
                  new TimeZones{
                    Id =  118,
                    Name =  "(UTC+09:30) Adelaide",
                    TimeZone =  "Australia/Adelaide"
                  },
                  new TimeZones{
                    Id =  119,
                    Name =  "(UTC+09:30) Darwin",
                    TimeZone =  "Australia/Darwin"
                  },
                  new TimeZones{
                    Id =  120,
                    Name =  "(UTC+10:00) Brisbane",
                    TimeZone =  "Australia/Brisbane"
                  },
                  new TimeZones{
                    Id =  121,
                    Name =  "(UTC+10:00) Canberra, Melbourne, Sydney",
                    TimeZone =  "Australia/Sydney"
                  },
                  new TimeZones{
                    Id =  122,
                    Name =  "(UTC+10:00) Guam, Port Moresby",
                    TimeZone =  "Pacific/Guam"
                  },
                  new TimeZones{
                    Id =  123,
                    Name =  "(UTC+10:00) Hobart",
                    TimeZone =  "Australia/Hobart"
                  },
                  new TimeZones{
                    Id =  124,
                    Name =  "(UTC+10:00) Vladivostok",
                    TimeZone =  "Asia/Vladivostok"
                  },
                  new TimeZones{
                    Id =  125,
                    Name =  "(UTC+10:30) Lord Howe Island",
                    TimeZone =  "Australia/Lord_Howe"
                  },
                  new TimeZones{
                    Id =  126,
                    Name =  "(UTC+11:00) Bougainville Island",
                    TimeZone =  "Pacific/Bougainville"
                  },
                  new TimeZones{
                    Id =  127,
                    Name =  "(UTC+11:00) Chokurdakh",
                    TimeZone =  "Asia/Magadan"
                  },
                  new TimeZones{
                    Id =  128,
                    Name =  "(UTC+11:00) Magadan",
                    TimeZone =  "Asia/Magadan"
                  },
                  new TimeZones{
                    Id =  129,
                    Name =  "(UTC+11:00) Norfolk Island",
                    TimeZone =  "Pacific/Norfolk"
                  },
                  new TimeZones{
                    Id =  130,
                    Name =  "(UTC+11:00) Sakhalin",
                    TimeZone =  "Asia/Sakhalin"
                  },
                  new TimeZones{
                    Id =  131,
                    Name =  "(UTC+11:00) Solomon Is., New Caledonia",
                    TimeZone =  "Pacific/Guadalcanal"
                  },
                  new TimeZones{
                    Id =  132,
                    Name =  "(UTC+12:00) Anadyr, Petropavlovsk-Kamchatsky",
                    TimeZone =  "Asia/Anadyr"
                  },
                  new TimeZones{
                    Id =  133,
                    Name =  "(UTC+12:00) Auckland, Wellington",
                    TimeZone =  "Pacific/Auckland"
                  },
                  new TimeZones{
                    Id =  134,
                    Name =  "(UTC+12:00) Coordinated Universal Time+12",
                    TimeZone =  "Etc/GMT-12"
                  },
                  new TimeZones{
                    Id =  135,
                    Name =  "(UTC+12:00) Fiji",
                    TimeZone =  "Pacific/Fiji"
                  },
                  new TimeZones{
                    Id =  136,
                    Name =  "(UTC+12:45) Chatham Islands",
                    TimeZone =  "Pacific/Chatham"
                  },
                  new TimeZones{
                    Id =  137,
                    Name =  "(UTC+13:00) Coordinated Universal Time+13",
                    TimeZone =  "Etc/GMT-13"
                  },
                  new TimeZones{
                    Id =  138,
                    Name =  "(UTC+13:00) Nuku'alofa",
                    TimeZone =  "Pacific/Tongatapu"
                  },
                  new TimeZones{
                    Id =  139,
                    Name =  "(UTC+13:00) Samoa",
                    TimeZone =  "Pacific/Apia"
                  },
                  new TimeZones{
                    Id =  140,
                    Name =  "(UTC+14:00) Kiritimati Island",
                    TimeZone =  "Pacific/Kiritimati"
                  }
            };
            return listTimeZones;
        }
    }
}
