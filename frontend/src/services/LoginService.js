import axios from 'axios';
import cookie from '../helpers/cookies.helper';
const { VITE_APP_API } = import.meta.env;

class AuthService {
    login(user) {
        const data = {
            username: user.username,
            password: user.password
        };
        return axios.post(VITE_APP_API + 'account/login', data);
    }

    register(user) {
        return axios.post(VITE_APP_API + 'register', user);
    }

    async logout() {
        try {
            const stored = localStorage.getItem('user');
            const user = stored ? JSON.parse(stored) : null;
            if (user?.token && user?.refreshToken) {
                await axios.post(
                    VITE_APP_API + 'Account/logout',
                    { refreshToken: user.refreshToken },
                    { headers: { Authorization: `Bearer ${user.token}` } }
                );
            }
        } catch {
            // silent — vẫn xóa local dù backend lỗi
        } finally {
            localStorage.removeItem('user');
            cookie.clear('user_type');
        }
    }
}

export default new AuthService();
