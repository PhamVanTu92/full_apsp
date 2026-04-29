using BackEndAPI.Models.ConfirmationSystem;
using BackEndAPI.Models.Other;
using Gridify;
using Microsoft.AspNetCore.Mvc;

namespace BackEndAPI.Service.ConfirmationSystems
{
    public interface IConfirmationDocumentService
    {
        public Task<(ConfirmationDocument, Mess)> Create(ConfirmationDocumentNew doc, int? userId);
        public Task<(List<ConfirmationDocument>, int, Mess)> GetList(GridifyQuery gridifyQuery, int cardId = 0);
        public Task<(ConfirmationDocument,Mess)> GetDetail(int id);
        public Task<(bool, Mess)> Send(ActionRequest request, int userId);
        public Task<(bool, Mess)> Approve(ActionRequest request, int userId);
        public Task<(bool, Mess)> Reject([FromBody] ActionRequest request, int userId);
    }
}
