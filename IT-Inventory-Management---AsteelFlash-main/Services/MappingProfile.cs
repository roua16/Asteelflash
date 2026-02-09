using AutoMapper;
using ITStockM.DTOs;
using ITStockM.Models.ITStockManagment;

namespace ITStockM.Services
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Materiel, MaterielDto>().ReverseMap();
            CreateMap<Assignment, AssignmentDto>().ReverseMap();
            CreateMap<Request, RequestDto>().ReverseMap();
            CreateMap<DeliveryOrder, DeliveryOrderDto>().ReverseMap();
        }
    }
}
