import { defineStore } from 'pinia';
import { FilterMatchMode } from 'primevue/api';
export const FilterStore = defineStore('FilterStoreAgencyCategory', {
    state: () => ({
        filters: {
            global: { value: null, matchMode: FilterMatchMode.CONTAINS },
            phone: { value: null, matchMode: FilterMatchMode.CONTAINS },
            licTradNum: { value: null, matchMode: FilterMatchMode.CONTAINS },
            status: { value: null, matchMode: FilterMatchMode.EQUALS },
            saleStaffId: { value: null, matchMode: FilterMatchMode.EQUALS }
        }
    }),

    actions: {
        resetFilters() {
            this.filters = {
                global: { value: null, matchMode: FilterMatchMode.CONTAINS },
                phone: { value: null, matchMode: FilterMatchMode.CONTAINS },
                licTradNum: { value: null, matchMode: FilterMatchMode.CONTAINS },
                status: { value: null, matchMode: FilterMatchMode.EQUALS },
                saleStaffId: { value: null, matchMode: FilterMatchMode.EQUALS }
            };
        }
    }
});
