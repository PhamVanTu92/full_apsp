import API from "../api/api-main";
class Promotion {
    async getPromotion(data) {
        try {
            const res = await API.add('Promotion/getPromotion', data);
            return res.data.items;
        } catch (error) {
            return {promotionOrderLine: []}
        }
    } 
}
export default new Promotion();