using BackEndAPI.Models.Other;
using Gridify;

namespace BackEndAPI.Service.SaleForecast;

public interface ISaleForecast
{
    Task<(Models.SaleForecastModel.SaleForecast?, Mess?)> CreateSaleForecast(
        Models.SaleForecastModel.SaleForecast saleForecast);

    Task<(List<Models.SaleForecastModel.SaleForecast>?, Mess?, int)> GetSaleForecast(int skip, int limit,
        string? search, GridifyQuery filter, int cardId);

    Task<(Models.SaleForecastModel.SaleForecast?, Mess?)> GetSaleForecastById(int planId);

    Task<(Models.SaleForecastModel.SaleForecast?, Mess?)> UpdateSaleForecast(int planId,
        Models.SaleForecastModel.SaleForecast saleForecast, int userId);

    Task<(Models.SaleForecastModel.SaleForecast?, Mess?)> AddItemToSaleForecast(int id,
        ICollection<Models.SaleForecastModel.SaleForecastItem> orderPlanItems);

    Task<(Models.SaleForecastModel.SaleForecast?, Mess?)> RemoveItemFromSaleForecast(int id, List<int> orderPlanIds);


    Task<Mess?> UpdateStatusSaleForecast(int planId,  string status, string userType);
}