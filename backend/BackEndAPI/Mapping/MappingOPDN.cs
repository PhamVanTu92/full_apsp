using AutoMapper;
using BackEndAPI.Models.Document;
using BackEndAPI.Models.ItemMasterData;

namespace BackEndAPI.Mapping
{
    public class MappingOPDN : Profile
    {
        public MappingOPDN()
        {
            CreateMap<DocumentOPDN, ODOC>();
        }
    }
}
