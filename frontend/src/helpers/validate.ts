export class ValidateResult {
    result: boolean;
    errors: Record<string, string>;

    constructor() {
        this.result = true;
        this.errors = {};
    }
}

import { phoneRegex, emailRegex, cccdRegex, vehiclePlateRegex } from '../helpers/regex';

export const PATTERN = {
    phone: phoneRegex,
    email: emailRegex,
    identityId: cccdRegex,
    vehiclePlate: vehiclePlateRegex,
    password: /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!#%*?&])[A-Za-z\d@$!#%*?&]{8,}$/
};

type ValidatorConfig = {
    type?: Array<any> | string | String | number | Number | Date | BinaryType | Boolean | any;
    name?: string;
    required?: boolean;
    nullMessage?: string;
    pattern?: string | RegExp;
    patternMessage?: string;
    minLength?: number;
    maxLength?: number;
    min?: number;
    max?: number;
    minDate?: Date;
    maxDate?: Date;
};

export type ValidateOption<T = any> = {
    [K in keyof T]?: {
        validators?: ValidatorConfig;
    };
};

/**
 * Kiểm tra một đối tượng dựa trên một tập hợp các tùy chọn xác thực.
 *
 * @param srcObject - Đối tượng nguồn cần kiểm tra.
 * @param options - Một đối tượng chứa các quy tắc xác thực cho mỗi trường trong đối tượng nguồn.
 * @returns {
 *            result: boolean, // true nếu kiểm tra thành công, false nếu không
 *            errors: object   // chứa các thông báo lỗi cho mỗi kiểm tra không thành công
 *          }
 *
 * @example options = {name: {validators: { required: true, name: "Tên người dùng", partern }}}
 *
 *
 *
 */
export function Validator(srcObject: object, options: ValidateOption): ValidateResult {
    let result = new ValidateResult();
    if (options) {
        for (let key in srcObject) {
            const value = srcObject[key as keyof typeof srcObject];
            const validators = options[key]?.validators; 
            // check for validators exist
            if (validators) {
                //Kiểm tra có tồn tại rằng buộc kiểu không, nếu có thì kiểm tra xem có đúng kiểu dữ liệu hay không
                const valueType = (typeof value).toLowerCase(); // tên kiểu dữ liệu của dữ liệu nguồn
                const validateType = options[key]?.validators?.type?.name?.toLowerCase() || ''; // tên kiểu dữ liệu của dữ liệu rằng buộc
                if (validators.type !== undefined) {
                    //Trường hợp đúng với kiểu dữ liệu
                    if (valueType == validateType || value instanceof validators.type) {
                        switch (valueType) {
                            case 'number':
                                if ((validators.min != undefined || validators.max != undefined) && !checkLength(value, validators.min, validators.max)) {
                                    result.errors[key] = 'Độ dài không hợp lệ';
                                }
                                break;
                            case 'string':
                                if ((validators.minLength != undefined || validators.maxLength != undefined) && !checkLength(value, validators.min, validators.max)) {
                                    result.errors[key] = result.errors[key] = 'Độ dài không hợp lệ';
                                }
                                break;
                            // case 'date':
                            //     if(value < validators.minDate || value > validators.maxDate){
                            //         result.errors[key] = validators.minDate? `${validators.name} phải l��n hơn hoặc b��ng ${validators.minDate.toLocaleDateString()}` : `${validators.name} phải nh�� hơn hoặc b��ng ${validators.maxDate.toLocaleDateString()}`;
                            //     }
                            //     break;
                            case 'array':
                                if ((validators.minLength != undefined || validators.maxLength != undefined) && !checkLength(value, validators.min, validators.max)) {
                                    result.errors[key] = `Phải có ít nhất ${validators.minLength} ${validators.name}`;
                                }
                                break;
                            case 'object':
                                // kiểu mảng | Array
                                if (Array.isArray(value)) {
                                    if ((validators.minLength != undefined || validators.maxLength != undefined) && !checkLength(value, validators.minLength, validators.maxLength)) {
                                        result.errors[key] = `Phải có ít nhất ${validators.minLength} ${validators.name}`;
                                    }
                                }
                                break;
                            default:
                                result.errors[key] = `Kiểu dữ liệu ${valueType} không hỗ trợ`;
                                break;
                        }
                    } else {
                        result.errors[key] = `Kiểu dữ liệu${validators.name ? ` ${validators.name} ` : ' '}không đúng định dạng`;
                    }
                }
                if (validators.required && !value) {
                    if (!value) {
                        result.errors[key] = validators.nullMessage || `Nhập ${validators.name}`;
                    }
                    if (!srcObject.hasOwnProperty(key)) {
                        result.errors[key] = validators.nullMessage || `Nhập ${validators.name}`;
                    }
                } else if (value && validators.pattern) {
                    const regex = new RegExp(validators.pattern);
                    if (!regex.test(value)) {
                        result.errors[key] = validators.patternMessage || `${validators.name} không hợp lệ`;
                    }
                }
            }
        }
        if (Object.keys(result.errors).length) result.result = false;
    }
    return result;
}

export function checkLength(value: string | Array<any>, min: number | undefined, max: number | undefined): boolean | string {
    const minLength = min ?? 0;
    const maxLength = max ?? Number.MAX_SAFE_INTEGER;
    if (Array.isArray(value)) {
        return value.length >= minLength && value.length <= maxLength;
    }
    const result = value.length >= minLength && value.length <= maxLength;
    return result;
}
