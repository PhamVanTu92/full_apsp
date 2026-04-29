import { defineStore } from 'pinia';
export const FilterStore = defineStore('filter', {
    state: () => ({
        filters: {
            search: "",
            brand: [],
            industry: [],
            itemtype: [],
            packing: [],
          }
    }),

    actions: {
        resetFilters() {
            this.filters = {
                search: "",
                brand: [],
                industry: [],
                itemtype: [],
                packing: [],
            };
        }
    }
});
