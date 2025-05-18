using AutoMapper;
using TestAspire.ApiService.DataTransferObjects;
using TestAspire.ApiService.Entities;

namespace TestAspire.ApiService;

public class AutomapperProfile : Profile
{
    public AutomapperProfile()
    {
        CreateMap<Result, ResultDto>().ReverseMap();
        CreateMap<Dataset, DatasetDto>().ForMember(dest => dest.ImageFile, opt => opt.Ignore());
        CreateMap<DatasetDto, Dataset>().ConvertUsing(new CustomTypeConverterX());
        CreateMap<Dataset, DatasetReadDto>();
        CreateMap<Algo, AlgoDto>().ReverseMap();
    }
}

public class CustomTypeConverterX : ITypeConverter<DatasetDto, Dataset>
{
    public Dataset Convert(DatasetDto source, Dataset destination, ResolutionContext context)
    {
        using var ms = new MemoryStream();
        source.ImageFile.CopyTo(ms);
        return new Dataset
        {
            Id = source.Id,
            Name = source.Name,
            Image = ms.ToArray()
        };
    }
}