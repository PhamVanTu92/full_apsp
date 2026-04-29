using BackEndAPI.Models.Other;
using Gridify;

namespace BackEndAPI.Service.LocationService;

public interface ILocaltion
{
    Task<(List<Models.LocationModels.Area>?, int, Mess?)> GetAreas(int skip, int limit, GridifyQuery q, string? search);

    Task<(List<Models.LocationModels.Region>?, int, Mess?)> GetRegions(int skip, int limit, GridifyQuery q,
        string? search);
}