import { defineStore } from 'pinia';
import { FilterMatchMode ,FilterOperator} from 'primevue/api';
export const FilterStore = defineStore('FilterPromotionData', {
    state: () => ({
        filters: {
            global: { value: null, matchMode: FilterMatchMode.CONTAINS },
            promotionStatus: { value: null, matchMode: FilterMatchMode.EQUALS }, 
            endDate: {
                operator: FilterOperator.AND,
                constraints: [{ value: null, matchMode: FilterMatchMode.DATE_AFTER }]
            },
            startDate: {
                operator: FilterOperator.AND,
                constraints: [{ value: null, matchMode: FilterMatchMode.DATE_AFTER }]
            }
        }
    }),

    actions: {
        resetFilters() {
            this.filters = {
                global: { value: null, matchMode: FilterMatchMode.CONTAINS },
                promotionStatus: { value: null, matchMode: FilterMatchMode.EQUALS }, 
                endDate: {
                    operator: FilterOperator.AND,
                    constraints: [{ value: null, matchMode: FilterMatchMode.DATE_AFTER }]
                },
                startDate: {
                    operator: FilterOperator.AND,
                    constraints: [{ value: null, matchMode: FilterMatchMode.DATE_AFTER }]
                }
            };
        }
    }
});
