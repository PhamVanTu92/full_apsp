import { defineStore } from 'pinia';
import { FilterMatchMode, FilterOperator } from 'primevue/api';
export const FilterStore = defineStore('FilterPromotionDataNEW', {
    state: () => ({
        filters: {
            global: { value: null, matchMode: FilterMatchMode.CONTAINS },
            promotionStatus: { value: null, matchMode: FilterMatchMode.EQUALS },
            toDate: {
                operator: FilterOperator.AND,
                constraints: [{ value: null, matchMode: FilterMatchMode.DATE_AFTER}]
            },
            fromDate: {
                operator: FilterOperator.AND,
                constraints: [{ value: null, matchMode: FilterMatchMode.DATE_AFTER  }]
            }
        }
    }),

    actions: {
        resetFilters() {
            this.filters = {
                global: { value: null, matchMode: FilterMatchMode.CONTAINS },
                promotionStatus: { value: null, matchMode: FilterMatchMode.EQUALS },
                toDate: {
                    operator: FilterOperator.AND,
                    constraints: [{ value: null, matchMode: FilterMatchMode.DATE_AFTER }]
                },
                fromDate: {
                    operator: FilterOperator.AND,
                    constraints: [{ value: null, matchMode: FilterMatchMode.DATE_AFTER  }]
                }
            };
        }
    }
});
