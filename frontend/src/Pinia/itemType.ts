import { defineStore } from 'pinia';
import { ref } from 'vue';
import API from '../api/api-main';

interface ItemType {
    id: number;
    code: string;
    name: string;
    sapCode: null | string;
}

export const useItemTypeStore = defineStore('itemType', () => {
    const data = ref<ItemType[]>([]);
    const loading = ref(false);

    async function fetchAll() {
        const isLoggedIn = !!localStorage.getItem('user');
        const endpoint = isLoggedIn ? 'ItemType/getall' : 'ItemType/bypass';
        loading.value = true;
        try {
            const result = await API.get(endpoint);
            data.value = result.data.items ?? [];
        } catch (error) {
            console.error('Error getting all itemTypes:', error);
            data.value = [];
        } finally {
            loading.value = false;
        }
    }

    return { data, loading, fetchAll };
});
