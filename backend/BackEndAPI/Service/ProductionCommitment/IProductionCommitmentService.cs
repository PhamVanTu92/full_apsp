using BackEndAPI.Models.Other;
using Gridify;

namespace BackEndAPI.Service.ProductionCommitment;

public interface IProductionCommitmentService
{
    public Task<(Models.ProductionCommitmentModel.ProductionCommitment?, Mess?)> Create(Models.ProductionCommitmentModel.ProductionCommitment productionCommitment);

    Task<(List<Models.ProductionCommitmentModel.ProductionCommitment>?, Mess?, int)>
        GetProductionCommitments(int skip, int limit, GridifyQuery q, string? search);
}