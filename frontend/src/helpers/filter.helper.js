import format from './format.helper';
export default class ConditionHandler {
    constructor() {
        this.operatorLocale = {
            startsWith: '^',
            contains: '=*',
            notContains: '!*',
            endsWith: '$',
            equals: '=',
            notEquals: '=!',
            lt: '<',
            lte: '<=',
            gt: '>',
            gte: '>=',
            dateIs: '=',
            dateIsNot: '!=',
            dateBefore: '<=',
            dateAfter: '>=',
            matchAll: 'AND',
            matchAny: 'OR',
            and: ',',
            or: '|'
        };

        this.labelLocale = {
            startsWith: 'bắt đầu với',
            contains: 'chứa',
            notContains: 'không chứa',
            endsWith: 'kết thúc với',
            equals: 'bằng',
            notEquals: 'không bằng',
            noFilter: 'không có bộ lọc',
            lt: 'nhỏ hơn',
            lte: 'nhỏ hơn hoặc bằng',
            gt: 'lớn hơn',
            gte: 'lớn hơn hoặc bằng',
            dateIs: 'Ngày',
            dateIsNot: 'Ngày không phải là',
            dateBefore: 'Đến ngày',
            dateAfter: 'Từ ngày',
            clear: 'Xóa',
            apply: 'Áp dụng',
            matchAll: 'Tất cả',
            matchAny: 'Bất kỳ',
            addRule: 'Thêm quy tắc',
            removeRule: 'Xóa quy tắc',
            accept: 'Đồng ý',
            reject: 'Hủy',
            choose: 'Chọn',
            upload: 'Tải lên',
            cancel: 'Hủy',
            and: 'và',
            or: 'hoặc'
        };
    }

    getOperatorCondition(key) {
        return this.operatorLocale[key] || null;
    }

    getLabelCondition(key) {
        return this.labelLocale[key] || null;
    }

    getQuery(filters) {
        const conditions = [];
        const searchGlobal = [];
        for (const [field, filter] of Object.entries(filters)) {
            if (field === 'global' && filter.value) {
                searchGlobal.push(`&search=${filter.value.trim()}`);
            } else if (filter.value != null && filter.value !== undefined) {
                if (Array.isArray(filter.value)) {
                    conditions.push(`(${field}=${filter.value.join(`|${field}=`)})`);
                } else if (filter.matchMode) {
                    conditions.push(`(${field}${this.getOperatorCondition(filter.matchMode)}${filter.value})`);
                }
            }
            if (filter.constraints) {
                if (filter.constraints[0].value) {
                    const constraintConditions = filter.constraints
                        .map((el, index) => `${index ? ` ${this.getOperatorCondition(filter.operator)} ` : ''}${field}${this.getOperatorCondition(el.matchMode)}${format.DateTime(el.value).date_format}`)
                        .join('');
                    if (constraintConditions) {
                        conditions.push(`(${constraintConditions})`);
                    }
                }
            }
        }
        const search = searchGlobal.length ? searchGlobal[0] : '';
        return `&filter=${conditions.filter(Boolean).join(',')}${search}`;
    }
}
