using BackEndAPI.Models.Committed;
using BackEndAPI.Models.Document;
using BackEndAPI.Models.Other;
using Gridify;

namespace BackEndAPI.Service.Committeds
{
    public interface ICommittedService : IService<Committed>
    {
        Task<(Committed?, Mess?)> CreateCommited(Committed model);
        Task<(Committed?, Mess?)> UpdateCommitted(int id, Committed model);
        Task<(Committed?, Mess?)> GetCommitedById(int id);
        Task<(IEnumerable<Committed>?, int total, Mess?)> GetCommitted(int skip, int limit, string? search, GridifyQuery q, int cardId);
        Task<Mess?> UpdateStatus(int cmmId, string status, string note);
        Task<Mess?> DeleteCommited(int cmId);
        Task<Committed> GetCommitedDiscount(int bpId, List<ItemChecking> items);
        Task UpdateVolumn(int bpId, List<DOC1> items);
        Task<(CommittedTracking?, Mess?)> CreateCommitedTracking(CommittedTracking model);
    }
}
