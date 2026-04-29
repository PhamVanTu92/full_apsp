import { defineStore } from 'pinia';
import { ref } from 'vue';
import API from '../api/api-main';

interface Brand {
    id: number;
    code: string;
    name: string;
    sapCode: null | string;
}

export const useBrandStore = defineStore('Brand', () => {
    const data = ref<Brand[]>([]);
    const loading = ref(false);

    async function fetchAll() {
        loading.value = true;
        try {
            const result = await API.get('Brand/getall');
            data.value = result.data ?? [];
        } catch (error) {
            console.error('Error getting all Brand:', error);
            data.value = [];
        } finally {
            loading.value = false;
        }
    }

    return { data, loading, fetchAll };
});
