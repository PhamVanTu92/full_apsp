import axios from 'axios';
import authHeader from '../services/auth-header';

const API_URL = import.meta.env.VITE_APP_API_PM;

class API {
    async getUsers(limit, skip, fields) {
        const users = await axios.get(API_URL + 'user/getall?size=0&page=0', { headers: authHeader() });
        return users.data.data;
    }

    get_all(api) {
        return axios.get(API_URL + api, { headers: authHeader() });
    }

    add_new(data, api) {
        return axios.post(API_URL + api, data, { headers: authHeader() });
    }

    update(data, api) {
        return axios.put(API_URL + api, data, { headers: authHeader() });
    }

    delete(card_code, api) {
        return axios.delete(API_URL + api, { headers: authHeader(), data: card_code });
    }
}

export default new API();
