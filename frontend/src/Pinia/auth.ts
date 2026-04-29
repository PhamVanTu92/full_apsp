import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import AuthService from '@/services/LoginService.js';
import { startReferenceDataHub, stopReferenceDataHub } from '@/services/referenceDataHub';
import { clearAllCache } from '@/services/referenceDataCache';

interface AuthStatus {
    loggedIn: boolean;
    _2FA: boolean;
    username: string;
}

interface AppUser {
    userType: string;
    cardId: number | null;
    [key: string]: any;
}

interface AuthUser {
    token: string;
    refreshToken?: string;
    appUser: AppUser;
    'access-exp'?: number;
    [key: string]: any;
}

export const useAuthStore = defineStore('auth', () => {
    const _stored = localStorage.getItem('user');
    const _initial: AuthUser | null = _stored ? JSON.parse(_stored) : null;

    const user = ref<AuthUser | null>(_initial);
    const status = ref<AuthStatus>({
        loggedIn: !!_initial,
        _2FA: false,
        username: ''
    });

    const isLoggedIn = computed(() => status.value.loggedIn);
    const userType = computed(() => user.value?.appUser?.userType ?? '');
    const token = computed(() => user.value?.token ?? '');

    async function login(credentials: { username: string; password: string }) {
        try {
            const response = await AuthService.login(credentials);
            const userData: AuthUser = response.data;

            if (userData.token === '2FA') {
                status.value._2FA = true;
                status.value.username = credentials.username;
                return { status: false, _2FA: true, username: credentials.username };
            }

            if (userData) {
                const currentDate = new Date();
                const expireTime = userData['access-exp'] ? userData['access-exp'] * 60 * 1000 : 0;
                const expireTokenTime = new Date(currentDate.getTime() + expireTime).toString();
                const jToken = { ExpireToken: expireTokenTime, ...userData };
                localStorage.setItem('user', JSON.stringify(jToken));
            }

            user.value = userData;
            status.value.loggedIn = true;
            status.value._2FA = false;
            // Khởi động SignalR hub sau khi token đã có trong localStorage
            startReferenceDataHub();
            return { status: true, data: userData };
        } catch (error: any) {
            status.value.loggedIn = false;
            user.value = null;
            const errorMessage = error.response?.data?.errors || 'An error occurred during login.';
            return { status: false, message: errorMessage };
        }
    }

    async function logout() {
        await AuthService.logout();
        user.value = null;
        status.value.loggedIn = false;
        status.value._2FA = false;
        // Ngắt SignalR + xoá cache reference data của user cũ
        await stopReferenceDataHub();
        await clearAllCache();
    }

    async function idleLogout() {
        await AuthService.logout();
        user.value = null;
        status.value.loggedIn = false;
        await stopReferenceDataHub();
        await clearAllCache();
    }

    async function register(credentials: object) {
        try {
            const response = await AuthService.register(credentials);
            const userData: AuthUser = response.data;
            user.value = userData;
            status.value.loggedIn = true;
            return { status: true, data: userData };
        } catch (error: any) {
            status.value.loggedIn = false;
            const errorMessage = error.response?.data?.error?.message || 'An error occurred during registration.';
            return { status: false, message: errorMessage };
        }
    }

    return { user, status, isLoggedIn, userType, token, login, logout, idleLogout, register };
});
