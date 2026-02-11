using AutoMapper;
using Pepegov.UnitOfWork.Entityes;

namespace AegisForge.PL.Definitions.Mapping;

/// <inheritdoc />
public class PagedListMappingProfile : Profile
{
    /// <inheritdoc />
    public PagedListMappingProfile()
    {
        CreateMap(typeof(IPagedList<>), typeof(IPagedList<>))
            .ConvertUsing(typeof(PagedListConvert<,>));
        CreateMap(typeof(PagedList<>), typeof(IPagedList<>))
            .ConvertUsing(typeof(PagedListToIPagedListConvert<,>));
    }
}

/// <inheritdoc />
public class PagedListConvert<TMapFrom, TMapTo> : ITypeConverter<IPagedList<TMapFrom>, IPagedList<TMapTo>>
{
    /// <inheritdoc />
    public IPagedList<TMapTo> Convert(IPagedList<TMapFrom> source, IPagedList<TMapTo> destination,
        ResolutionContext context) =>
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        source == null
            ? PagedList.Empty<TMapTo>()
            : PagedList.From(source, items => context.Mapper.Map<List<TMapTo>>(items));
}

/// <inheritdoc />
public class PagedListToIPagedListConvert<TMapFrom, TMapTo> : ITypeConverter<PagedList<TMapFrom>, IPagedList<TMapTo>>
{
    /// <inheritdoc />
    public IPagedList<TMapTo> Convert(PagedList<TMapFrom> source, IPagedList<TMapTo> destination,
        ResolutionContext context) =>
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        source == null
            ? PagedList.Empty<TMapTo>()
            : PagedList.From(source, items => context.Mapper.Map<List<TMapTo>>(items));
}