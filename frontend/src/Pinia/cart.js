import { defineStore } from 'pinia';
import API from '../api/api-main'; 

export const useCartStore = defineStore('cart', {
    state: () => ({
        items: JSON.parse(localStorage.getItem('cart')) || [],
        itemQuantity: JSON.parse(localStorage.getItem('cart'))?.length || 0,
    }),
    getters: {
        totalQuantity(state) {
            return state.items.reduce((total, item) => total + item.quantity, 0);
        },
        getItems(state) {
            return state.items;
        },
        getCartQuantity(state) {
            return state.itemQuantity;
        }
    },
    actions: {
        async getCart() {
            const response = await API.get('Cart/me');
            this.items = response.data.items;
            this.itemQuantity = response.data.items.length;
            localStorage.setItem('cart', JSON.stringify(this.items));
            return response;
        },
        async addToCart(data) {
            const response = await API.add('Cart/me/items', data);
            localStorage.setItem('cart', JSON.stringify(this.items));
            return response;
        },
        async updateQuantity(data) {
            const response = await API.update('Cart/me', data);
            localStorage.setItem('cart', JSON.stringify(response.data.items));
            return response;
        },
        async deleteItemFromCart(id) {
            await API.delete(`Cart/me/${id}`);
            localStorage.setItem('cart', JSON.stringify(this.items));
            return 1;
        },
        async deleteItemsFromCart(ids) {
            const res = await API.delete(`Cart/me/bulk`, ids);
            return res;
        },
        async clearCart() {
            const ids = this.items?.map(itm => itm.id);
            const res = await API.delete(`Cart/me/bulk`, ids);
            this.itemQuantity = 0;
            this.items = [];
            return res;
        }
    }
});
