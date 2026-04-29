import { defineStore } from 'pinia';
import { FilterMatchMode ,FilterOperator} from 'primevue/api';
export const FilterStore = defineStore('FilterPromotionData', {
    state: () => ({
        filters: {
            global: { value: null, matchMode: FilterMatchMode.CONTAINS },
            promotionStatus: { value: null, matchMode: FilterMatchMode.EQUALS }, 
            fromDate: {
                operator: FilterOperator.AND,
                constraints: [{ value: null, matchMode: FilterMatchMode.DATE_AFTER }]
            },
            toDate: {
                operator: FilterOperator.AND,
                constraints: [{ value: null, matchMode: FilterMatchMode.DATE_AFTER }]
            },
            extendedToDate: {
                operator: FilterOperator.AND,
                constraints: [{ value: null, matchMode: FilterMatchMode.DATE_AFTER }]
            },
            isActive : { value: null, matchMode: FilterMatchMode.EQUALS }
        }
    }),

    actions: {
        resetFilters() {
            this.filters = {
                global: { value: null, matchMode: FilterMatchMode.CONTAINS },
                promotionStatus: { value: null, matchMode: FilterMatchMode.EQUALS }, 
                fromDate: {
                    operator: FilterOperator.AND,
                    constraints: [{ value: null, matchMode: FilterMatchMode.DATE_AFTER }]
                },
                toDate: {
                    operator: FilterOperator.AND,
                    constraints: [{ value: null, matchMode: FilterMatchMode.DATE_AFTER }]
                },
                extendedToDate: {
                    operator: FilterOperator.AND,
                    constraints: [{ value: null, matchMode: FilterMatchMode.DATE_AFTER }]
                },
                isActive : { value: null, matchMode: FilterMatchMode.EQUALS }
            };
        }
    }
});
