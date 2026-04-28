namespace TravelPlanner.Common.Mapping;

public interface IModelMapper<TSource, TDestination>
{
    TDestination Map(TSource source);
}

