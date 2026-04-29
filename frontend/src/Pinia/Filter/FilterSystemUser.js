import { defineStore } from 'pinia';
import { FilterMatchMode, FilterOperator } from 'primevue/api';
export const useFilterSystemUserStore = defineStore('FilterSystemUser', {
    state: () => ({
        filters: {
            status: { value: null, matchMode: FilterMatchMode.EQUALS },
            'role.name': { value: null, matchMode: FilterMatchMode.EQUALS }
        }
    }),

    actions: {
        resetFilters() {
            this.filters = {
                status: { value: null, matchMode: FilterMatchMode.EQUALS },
                'role.name': { value: null, matchMode: FilterMatchMode.EQUALS }
            };
        },
        // getQuery () {
        //     return {
        //         status: this.filters.status.value,
        //         roles: this.filters['role.name'].value
        //     }
        // }
    }
});
