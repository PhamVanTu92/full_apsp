import {Validator, ValidateResult, ValidateOption, PATTERN} from '../../../helpers/validate'
export interface IAddressInfo {
    id?: number;
    locationId: number | null;
    type: 'S' | 'B';
    locationName: string | null;
    areaId: number | null;
    areaName: string | null;
    address: string;
    email: string;
    phone: string;
    vehiclePlate: string;
    cccd: string;
    person: string;
    note: string;
    bpId: 0 | number;
    default: 'N' | 'Y';
    status: '' | string;
    locationType:'DM' | 'INT';
    validate(type: 'S'| 'B'): ValidateResult;
}

export class AddressInfo implements IAddressInfo {
    id = 0;
    locationId = null;
    type: 'S' | 'B' = 'S';
    locationName = '';
    areaId = null;
    areaName = '';
    address = '';
    email = '';
    phone = '';
    vehiclePlate = '';
    cccd = '';
    person = '';
    note = '';
    bpId = 0;
    default: 'N' = 'N';
    status = '';
    locationType: 'DM' | 'INT'  = 'DM';
    constructor(source?: object) {
        if (source) {
            Object.assign(this, source);
        }
        return this;
    }
    
    validate(type: 'S'|'B'): ValidateResult {
        let vresult: ValidateResult;
        let option:ValidateOption = {}
        option = type === 'S' ?deliveryOption : billingOption
        if(this.locationType == 'DM'){
            option = {...option, ...area_location_require_option}
        }
        vresult = Validator(this, option);
        return vresult;
    }
}
const billingOption: ValidateOption = {
    person: {
        validators: {
            type: String,
            required: true,
            nullMessage: 'Tên người liên hệ không được để trống',
        },
    },
    phone: {
        validators: {
            type: String,
            required: true,
            pattern: PATTERN.phone,
            patternMessage: 'Sai định dạng số điện thoại',
            name: 'Số điện thoại',
            nullMessage: 'Số điện thoại không được để trống',
        },
    },
    email: {
        validators: {
            type: String,
            required: true,
            pattern: PATTERN.email,
            patternMessage: 'Email sai định dạng',
            name: 'Email',
            nullMessage: 'Email không được để trống',
        },
    },
    address: {
        validators: {
            type: String,
            required: true,
            nullMessage: 'Vui lòng nhập địa chỉ',
        },
    },
}

const deliveryOption: ValidateOption = {
    person: {
        validators: {
            type: String,
            required: true,
            nullMessage: 'Tên người liên hệ không được để trống',
        },
    },
    phone: {
        validators: {
            type: String,
            required: true,
            pattern: PATTERN.phone,
            patternMessage: 'Sai định dạng số điện thoại',
            name: 'Số điện thoại',
            nullMessage: 'Số điện thoại không được để trống',
        },
    },
    vehiclePlate: {
        validators: {
            type: String,
            required: true,
            pattern: PATTERN.vehiclePlate,
            patternMessage: 'Biển số xe sai định dạng',
            name: 'Biển số xe',
            nullMessage: 'Biển số xe không được để trống',
        },
    },
    cccd: {
        validators: {
            type: String,
            required: true,
            pattern: PATTERN.identityId,
            patternMessage: 'CCCD sai định dạng',
            name: 'CCCD',
            nullMessage: 'CCCD không được để trống',
        },
    },
    // email: {
    //     validators: {
    //         type: String,
    //         required: true,
    //         pattern: PATTERN.email,
    //         patternMessage: 'Email sai định dạng',
    //         name: 'Email',
    //         nullMessage: 'Email không được để trống',
    //     },
    // },
    address: {
        validators: {
            type: String,
            required: true,
            nullMessage: 'Vui lòng nhập địa chỉ',
        },
    },
}

const area_location_require_option: ValidateOption = {
    areaId: {
        validators: {
            type: Number,
            required: true,
            nullMessage: 'Vui lòng chọn khu vực',
        },
    },
    locationId: {
        validators: {
            type: Number,
            required: true,
            nullMessage: 'Vui lòng chọn phường xã',
        },
    },
}

export interface ResponeLocation {
    id: number;
    name: string;
    normalName: string;
}
