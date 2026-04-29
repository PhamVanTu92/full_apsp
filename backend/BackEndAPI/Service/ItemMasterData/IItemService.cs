using BackEndAPI.Models.ItemMasterData;
using BackEndAPI.Models.Other;
using BackEndAPI.Models.Unit;
using Gridify;

namespace BackEndAPI.Service.ItemMasterData
{
    public interface IItemService : IService<Item>
    {
        Task<(IEnumerable<Item>, Mess, int total)> GetAllItemsAsync(GridifyQuery gridifyQuery);
        Task<(IEnumerable<Item>, Mess, int total)> GetAllItemsAsync(GridifyQuery gridifyQuery, ItemQuery query);
        Task<(IEnumerable<Item>, Mess, int total)> GetAllItemsAsync(GridifyQuery gridifyQuery, ItemQuery query, int CardId);
        Task<(IEnumerable<Item>, Mess, int total)> GetAllItemsIgAsync(GridifyQuery gridifyQuery, ItemQuery query, int CardId);
        Task<(IEnumerable<Item>, Mess)> GetItemAsync(string search);
        Task<(IEnumerable<Item>, Mess)> GetItemAsync(int brand, int industry, int itemType, int packing, string CardCode, string search);
        Task<(IEnumerable<Item>, Mess)> GetItemOrBarCodeAsync(string search, int branchId);
        Task SyncFromSap(List<ItemSync> list);
        Task<Mess> SyncFromSap();
        Task<(IEnumerable<ItemListView>, Mess)> GetItemOrBarCode1Async(string search, int branchId);
        Task<(Item, Mess)> CreateItemAsync(ItemView model);
        Task<(Item,Mess)> UpdateItemAsync(int id, ItemView model);
        Task<(Item,Mess)> GetItemByIdAsync(int id);
        Task<(bool, Mess)> UpdateItemAsync(string itemCode, ItemOnhand model);
        Task<(bool,Mess)> DeleteItemAsync(int id);
        Task<(IEnumerable<ItemSpecHierarchy>, Mess)> GetHierarchySpecAsync(int? CardId);




        Task<(IEnumerable<ItemPromotionView>, Mess, int total)> GetItemPromotion(GridifyQuery gridifyQuery, int? CardId,string? search);
        Task<(IEnumerable<ItemPromotionView>, Mess, int total)> GetItemPromotions(GridifyQuery gridifyQuery, int? CardId, string? search);
        Task<(IEnumerable<ItemImport>, Mess)> GetImportPricelist(List<ItemImportView> view);
    }
}
