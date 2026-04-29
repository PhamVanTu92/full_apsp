import { defineStore } from 'pinia';
import { FilterMatchMode, FilterOperator } from 'primevue/api';
export const FilterStore = defineStore('FilterStorePurchaseOrder', {
    state: () => ({
        filters: {
            global: { value: null, matchMode: FilterMatchMode.CONTAINS },
            total: { value: null, matchMode: FilterMatchMode.GREATER_THAN_OR_EQUAL_TO },
            status: { value: null, matchMode: FilterMatchMode.EQUALS },
            docStatus: { value: null, matchMode: FilterMatchMode.EQUALS },
            docType: { value: "", matchMode: FilterMatchMode.EQUALS },
            docDate: {
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
                docStatus: { value: null, matchMode: FilterMatchMode.EQUALS },
                docType: { value: "", matchMode: FilterMatchMode.EQUALS },
                docDate: {
                    operator: FilterOperator.AND,
                    constraints: [{ value: null, matchMode: FilterMatchMode.DATE_AFTER }]
                }, 
            };
        }
    }
});
