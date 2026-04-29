import { defineStore } from 'pinia';
import { FilterMatchMode, FilterOperator } from 'primevue/api';
export const FilterStore = defineStore('FilterStoreApproval', {
    state: () => ({
        filters: {
            global: { value: null, matchMode: FilterMatchMode.CONTAINS },
            total: { value: null, matchMode: FilterMatchMode.GREATER_THAN_OR_EQUAL_TO },
            status: { value: null, matchMode: FilterMatchMode.EQUALS },
            approvalStatus: { value: null, matchMode: FilterMatchMode.EQUALS },
            createdAt: {
                operator: FilterOperator.AND,
                constraints: [{ value: null, matchMode: FilterMatchMode.DATE_AFTER }]
            },
           
        }
    }),

    actions: {
        resetFilters() {
            this.filters = {
                global: { value: null, matchMode: FilterMatchMode.CONTAINS },
                total: { value: null, matchMode: FilterMatchMode.GREATER_THAN_OR_EQUAL_TO },
                status: { value: null, matchMode: FilterMatchMode.EQUALS },
                approvalStatus: { value: null, matchMode: FilterMatchMode.EQUALS },
                createdAt: {
                    operator: FilterOperator.AND,
                    constraints: [{ value: null, matchMode: FilterMatchMode.DATE_AFTER }]
                },
                
            };
        }
    }
});
