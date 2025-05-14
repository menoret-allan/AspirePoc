using AutoMapper;
using TestAspire.ApiService.DataTransferObjects;
using TestAspire.ApiService.Entities;

namespace TestAspire.ApiService
{
    public class AutomapperProfile : Profile
    {
        public AutomapperProfile()
        {
            CreateMap<Result, ResultDto>().ReverseMap();
            CreateMap<Dataset, DatasetDto>().ReverseMap();
            CreateMap<Algo, AlgoDto>().ReverseMap();
        }
    }
}