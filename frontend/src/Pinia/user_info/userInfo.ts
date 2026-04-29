import API from '@/api/api-main';
import { AxiosResponse } from 'axios';
import { defineStore } from 'pinia';
import { ref } from 'vue';
import { UserInfo } from './userInfo.interface';

export const useUserInfoStore = defineStore('UserInfo', () => {
    const userInfo = ref<UserInfo>();
    let initPromise: Promise<void> | null = null;

    async function initUserInfoStore(): Promise<void> {
        if (initPromise) {
            return initPromise;
        }

        initPromise = API.get('account/me').then((res: AxiosResponse<UserInfo>) => {
            userInfo.value = res.data;
        })
        .catch(err => {
            console.error(err);
            throw err;
        });

        return initPromise;
    }

    // Khởi tạo ngay khi store được tạo; lỗi sẽ surface khi caller gọi lại initUserInfoStore()
    initUserInfoStore().catch(() => {});

    return {
        userInfo,
        initUserInfoStore
    };
});