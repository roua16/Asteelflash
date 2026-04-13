using AutoMapper;
using ITStockM.Application.Features.Common.DTOs;
using ITStockM.Domain.Entities;

namespace ITStockM.Application.Common.Mapping
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
