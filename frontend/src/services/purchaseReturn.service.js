import API from "../api/api-main";

class PurchaseReturnService {
    async getAll(params = '') {
        try {
            const res = await API.get(`PurchaseReturn${params ? '?' + params : ''}`);
            return res.data;
        } catch (error) {
            console.error('Error fetching all purchase returns:', error);
            throw error;
        }
    }

    async getAllMe(params = '') {
        try {
            const res = await API.get(`me/PurchaseReturn${params ? '?' + params : ''}`);
            return res.data;
        } catch (error) {
            console.error('Error fetching my purchase returns:', error);
            throw error;
        }
    }

    async getById(id) {
        try {
            const res = await API.get(`PurchaseReturn/${id}`);
            return res.data;
        } catch (error) {
            console.error(`Error fetching purchase return ${id}:`, error);
            throw error;
        }
    }

    async add(data) {
        try {
            const res = await API.add('PurchaseReturn/add', data);
            return res.data;
        } catch (error) {
            console.error('Error adding purchase return:', error);
            throw error;
        }
    }

    async update(id, data) {
        try {
            const res = await API.update(`PurchaseReturn/${id}`, data);
            return res.data;
        } catch (error) {
            console.error(`Error updating purchase return ${id}:`, error);
            throw error;
        }
    }

    async changeStatus(id, status, options = {}) {
        try {
            const { reasonForCancellation = '', attachFiles = [] } = options;
            const query = reasonForCancellation ? `?reasonForCancellation=${encodeURIComponent(reasonForCancellation)}` : '';
            const formData = new FormData();

            attachFiles.forEach((file) => {
                if (file) {
                    formData.append('AttachFile', file);
                }
            });

            const res = await API.update(`PurchaseReturn/${id}/change-status/${status}${query}`, formData);
            return res.data;
        } catch (error) {
            console.error(`Error changing status for purchase return ${id}:`, error);
            throw error;
        }
    }

    async search(query) {
        try {
            const res = await API.get(`PurchaseReturn/search/${query}`);
            return res.data;
        } catch (error) {
            console.error(`Error searching purchase returns with ${query}:`, error);
            throw error;
        }
    }

    async getOrder(params = '') {
        try {
            const res = await API.get(`PurchaseReturn/getOrder${params ? '?' + params : ''}`);
            return res.data;
        } catch (error) {
            console.error('Error fetching orders for return:', error);
            throw error;
        }
    }
}

export default new PurchaseReturnService();
