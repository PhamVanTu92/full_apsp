import { GridifyQueryBuilder, ConditionalOperator } from 'gridify-client';
import { FilterMatchModeOptions } from 'primevue/api';

enum MatchMode {
    STARTS_WITH = ConditionalOperator.StartsWith,
    CONTAINS = ConditionalOperator.Contains,
    NOT_CONTAINS = ConditionalOperator.NotContains,
    ENDS_WITH = ConditionalOperator.EndsWith,
    EQUALS = ConditionalOperator.Equal,
    NOT_EQUALS = ConditionalOperator.NotEqual,
    IN = ConditionalOperator.Contains,
    LESS_THAN = ConditionalOperator.LessThan,
    LESS_THAN_OR_EQUAL_TO = ConditionalOperator.LessThanOrEqual,
    GREATER_THAN = ConditionalOperator.GreaterThan,
    GREATER_THAN_OR_EQUAL_TO = ConditionalOperator.GreaterThanOrEqual,
    BETWEEN = ConditionalOperator.Contains,
    DATE_IS = ConditionalOperator.Equal,
    DATE_IS_NOT = ConditionalOperator.NotEqual,
    DATE_BEFORE = ConditionalOperator.LessThan,
    DATE_AFTER = ConditionalOperator.GreaterThan
}

interface Filters {
    first: number;
    rows: number;
    sortField: any;
    sortOrder: any;
    multiSortMeta: any[];
    filters: {
        [key: string]: {
            value: any;
            matchMode: MatchMode;
        };
    };
}

export function toQueryString(filters: Filters): string {
    const gridify = new GridifyQueryBuilder();
    const filter = filters.filters;
    const filterKeys = Object.keys(filter);
    filterKeys.forEach((key, index) => {
        const filterValue = filter[key].value;
        const matchMode = filter[key].matchMode;
        const op = MatchMode[matchMode.toUpperCase()]; 
        if (filterValue) {
            if (Array.isArray(filterValue)) {
                filterValue.forEach((value: any, index2: number) => {
                    if (index2 === 0) gridify.startGroup();
                    gridify.addCondition(key, op, value);
                    if (index2 > 0 && index2 < filterValue.length - 1) {
                        gridify.or();  
                    }
                    if (index2 === filterValue.length - 1) gridify.endGroup();
                });
            } else {
                gridify.addCondition(key, op, filterValue);
            }
            if (filter[filterKeys[index + 1]]?.value) {
                gridify.and();
            }
        }
    });
    return gridify.build().filter || '';
}
