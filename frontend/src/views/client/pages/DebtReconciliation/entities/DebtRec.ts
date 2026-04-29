import { Validator, ValidateResult, ValidateOption } from '../../../../../helpers/validate';

export interface IDebtRec {
    id?: number;
    code: string;
    name: string;
    customerId: number | null;
    customerName?: string | null;
    userId?: number | null;
    creatorName?: string | null;
    createdAt?: string | Date | null;
    confirmDate?: string | Date | null;
    status: 'P' | 'C' | 'R' | 'A' | null ;
    attachments?: Array<any> | [];
    bpAttachments?: Array<any> | [];
    note?: string | null;
    systemNote: string | null;
    reason?: string | null;
    rejectReason?: string | null;

    toJSON(): IDebtRec;
    validate(): ValidateResult;
}

export class DebtRec implements IDebtRec {
    id?: number = 0;
    code: string = '';
    name: string = 'Biên bản đối chiếu công nợ ';
    customerId: number | null = null;
    // customerName: string | null = null;
    // confirmDate: string | Date | null = null;
    // senderId: number | null = null;
    // sender: object | null = null;
    status: 'P' | 'C' | 'R' | null = null;
    attachments? = [];
    // clientNote: string | null = null;
    systemNote: string | null = null;

    constructor(source?: Partial<IDebtRec>) {
        if (source) {
            Object.keys(this).forEach((key) => {
                if (source.hasOwnProperty(key)) {
                    (this as any)[key] = source[key as keyof IDebtRec];
                }
            });
        }
    }
    toJSON(): IDebtRec {
        if (!this.id) {
            delete this.id;
        }
        let result = {};
        Object.keys(this).forEach((key) => {
            if (this.hasOwnProperty(key)) {
                if (typeof (this as any)[key] === 'string') {
                    result[key] = (this as any)[key].trim();
                } else {
                    result[key] = (this as any)[key];
                }
            }
        });
        return result as IDebtRec;
    }

    validate(): ValidateResult {
        let result = Validator(this, validateOption);
        return result;
    }
}

export interface DebtRecResponse {
    items: Array<IDebtRec>;
    total: number;
    page: number;
    pageSize: number;
}

const validateOption = {
    name: {
        validators: {
            type: String,
            required: true,
            name: 'tên biên bản',
        }
    },
    customerId: {
        validators: {
            type: Number,
            required: true,
            nullMessage: 'Vui lòng chọn khách hàng'
        }
    },
    attachments: {
        validators: {
            type: Array,
            required: true,
            minLength: 1,
            name: 'tài liệu'
        }
    }
};
