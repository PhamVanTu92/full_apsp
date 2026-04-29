import axios from 'axios';
// import authHeader from '../service/auth-header';
import {authHeader, authHeader_new} from '../helpers/auth-header.helper';


const api = import.meta.env.VITE_APP_API_1;

class API {
    async get(patch) {
        const url = api + patch;
        try {
            const res = await axios.get(url, {
                headers: authHeader(),
                timeout: 10000 // Set the timeout to 5000 milliseconds (5 seconds)
            });
            return res;
        } catch (error) {
            console.error(error)
            if (error.code === 'ECONNABORTED') {
                console.error('Request timed out');
            } else {
                console.error('An error occurred:', error.message);
            }
            throw error; // Rethrow the error to handle it in the calling function if necessary
        }
    }

    async add(patch, params) {
        const url = api + patch;
        try {
            const res = await axios.post(url, params, {
                headers: authHeader(),
                timeout: 5000 // Set the timeout to 5000 milliseconds (5 seconds)
            });
            return res;
        } catch (error) {
            if (error.code === 'ECONNABORTED') {
                console.error('Request timed out');
            } else {
                console.error('An error occurred:', error.message);
            }
            throw error; // Rethrow the error to handle it in the calling function if necessary
        }
    }

    async update(patch, params) {
        const url = api + patch;
        try {
            const res = await axios.put(url, params, {
                headers: authHeader(),
                timeout: 5000 // Set the timeout to 5000 milliseconds (5 seconds)
            });
            return res;
        } catch (error) {
            if (error.code === 'ECONNABORTED') {
                console.error('Request timed out');
            } else {
                console.error('An error occurred:', error.message);
            }
            throw error; // Rethrow the error to handle it in the calling function if necessary
        }
    }

    async delete(patch, params) {
        const url = api + patch;
        try {
            const res = await axios.delete(url, {
                headers: authHeader(),
                data: params, // Include params in the data property for DELETE request
                timeout: 5000 // Set the timeout to 5000 milliseconds (5 seconds)
            });
            return res;
        } catch (error) {
            if (error.code === 'ECONNABORTED') {
                console.error('Request timed out');
            } else {
                console.error('An error occurred:', error.message);
            }
            throw error; // Rethrow the error to handle it in the calling function if necessary
        }
    }

}
export default new API();