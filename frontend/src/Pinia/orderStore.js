import { defineStore } from 'pinia';
import { ref, computed } from 'vue';

export const useOrderStore = defineStore('orders', () => {
    const orders = ref([]);
    const gifts = ref([]);
    const coupon = ref(null);   
    const discount = ref(0);
    const note = ref(null);
    const address = ref({});
    const totalItemPrice = computed(() => {
        return orders.value.reduce((a, b) => {
            return a + (b.f_price)
        }, 0);
    });

    return { orders , gifts, totalItemPrice, coupon, discount, note, address };
});
