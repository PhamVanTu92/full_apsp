import { defineStore } from 'pinia';
import { FilterMatchMode, FilterOperator } from 'primevue/api';
export const FilterStore = defineStore('filter', {
    state: () => ({
        filters: {
            global: { value: null, matchMode: FilterMatchMode.CONTAINS },
            docStatus: { value: null, matchMode: FilterMatchMode.EQUALS },
            'committedYear.Year': { value: null, matchMode: FilterMatchMode.EQUALS },
        }
    }),

    actions: {
        resetFilters() {
            this.filters = {
                global: { value: null, matchMode: FilterMatchMode.CONTAINS },
                docStatus: { value: null, matchMode: FilterMatchMode.EQUALS },
                'committedYear.Year': { value: null, matchMode: FilterMatchMode.EQUALS },
            };
        }
    }
});
