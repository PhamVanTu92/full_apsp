import axios from 'axios';
import authHeader from '../service/auth-header';
const API_URL = import.meta.env.VITE_APP_API;

class API {
    async get_all(api) {
        return await axios.get(API_URL + api, { headers: authHeader() });
    }

    async get_data(api, data) {
        return await axios.get(API_URL + api, { params: data.data, headers: authHeader() });
    }

    async add_new_v2(data, api) {
        return await axios.post(API_URL + api, data, { headers: authHeader() });
    }

    async update_v2(data, api) {
        return await axios.put(API_URL + api, data, { headers: authHeader() });
    }

    async delete_v2(id, api) {
        return await axios.delete(API_URL + api, { headers: authHeader(), data: id });
    }
}

export default new API();
