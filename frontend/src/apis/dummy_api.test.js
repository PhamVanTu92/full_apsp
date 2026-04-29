import axios from 'axios';

const API_URL = import.meta.env.VITE_APP_API_PM;

class API {
    async getUsers(limit, skip, fields) {
        const users = await axios.get(`https://dummyjson.com/users?limit=${limit}&skip=${skip}&select=${fields}`);
        return users.data.users;
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
