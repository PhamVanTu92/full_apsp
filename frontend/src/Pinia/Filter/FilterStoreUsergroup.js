import { defineStore } from 'pinia';
import { FilterMatchMode, FilterOperator } from 'primevue/api';
export const FilterStore = defineStore('filter', {
    state: () => ({
        filters: {
            global: { value: null, matchMode: FilterMatchMode.CONTAINS },
            createdAt: {
                operator: FilterOperator.AND,
                constraints: [{ value: null, matchMode: FilterMatchMode.DATE_AFTER }]
            }, updatedAt: {
                operator: FilterOperator.AND,
                constraints: [{ value: null, matchMode: FilterMatchMode.DATE_AFTER }]
            },
            isActive: { value: null, matchMode: FilterMatchMode.EQUALS },
        }
    }),

    actions: {
        resetFilters() {
            this.filters = {
                global: { value: null, matchMode: FilterMatchMode.CONTAINS },
                createdAt: {
                    operator: FilterOperator.AND,
                    constraints: [{ value: null, matchMode: FilterMatchMode.DATE_AFTER }]
                }, updatedAt: {
                    operator: FilterOperator.AND,
                    constraints: [{ value: null, matchMode: FilterMatchMode.DATE_AFTER }]
                },
                isActive: { value: null, matchMode: FilterMatchMode.EQUALS },
            };
        }
    }
});
