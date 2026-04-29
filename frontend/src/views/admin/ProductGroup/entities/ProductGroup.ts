import API from '@/api/api-main';
import { AxiosError, AxiosResponse } from 'axios';
import { Validator, ValidateResult, ValidateOption } from '../../../../helpers/validate';
import { cloneDeep } from 'lodash';

export interface IProductGroupResponse {
    itemGroup: Array<IProductGroup>;
    limit: number;
    skip: number;
    total: number;
}

export const TypeConditionOption = [
    {
        label: 'Ngành hàng',
        value: 'industry'
    },
    {
        label: 'Thương hiệu',
        value: 'brand'
    },
    {
        label: 'Loại sản phẩm',
        value: 'item_type'
    },
    {
        label: 'Ứng dụng',
        value: 'product_applications'
    },
    {
        label: 'Nhóm hàng hóa',
        value: 'product_group'
    },
    {
        label: 'Quy cách bao bì',
        value: 'packing'
    }
];

export interface ICondValues {
    id?: number | string;
    condId?: 0;
    value: string;
    valueName: string;
}

export interface IConditionItemGroups {
    id?: number;
    groupId?: number;
    isEqual: true;
    typeCondition: string | null;
    condValues: Array<ICondValues|number|string|undefined>;
    values?: Array<number | string>;
}

export interface IProductGroup {
    id?: number;
    itmsGrpName: string | null;
    items: Array<any>; // Product
    itemIds: Array<number>;
    conditionItemGroups: Array<IConditionItemGroups>;
    isOneOfThem: boolean;
    isSelected: boolean;
    isActive: boolean;
    description: string;
    toPayload(): IProductGroup;
    AddCond(): void;
    validate(): ValidateResult;
}

const stringTypes = ['product_group', 'product_applications']

export class ProductGroup implements IProductGroup {
    id?: number;
    itmsGrpName: string | null = null;
    items: Array<any> = [];
    itemIds: Array<number>;
    conditionItemGroups: Array<IConditionItemGroups> = [];
    isSelected: boolean = true; // select
    isOneOfThem: boolean = false; // second select
    isActive: boolean = true;
    description: string;
    constructor(source?: object) {
        if (source) {
            Object.assign(this, source);
            this.conditionItemGroups?.forEach((cig) => {
                //typeof cv === 'object' ? (parseInt(cv.value)||cv.value) : cv
                cig.condValues = cig.condValues.map((cv) => {
                    if(stringTypes.includes(cig.typeCondition || '')){
                        return typeof cv === 'object' ? cv.value : cv;
                    }
                    else{
                        return typeof cv === 'object' ? (parseInt(cv.value)||cv.value) : cv
                    }
                });
            })
            return this;
        }
    }

    AddCond() {
        this.conditionItemGroups.push({
            typeCondition: null,
            isEqual: true,
            condValues: []
        });
    }

    toPayload(): IProductGroup {
        const payload = cloneDeep(this) as IProductGroup;
        if (payload.isSelected) {
            if (payload.items.length) {
                payload.itemIds = payload.items.map((prodct) => prodct.id);
            }
            payload.conditionItemGroups = [];
            payload.items = [];
        } else {
            if (payload.conditionItemGroups.length) {
                payload.conditionItemGroups?.forEach((cig) => {
                    cig.values = cig.condValues.filter((value): value is string | number => value !== undefined).map((value) => value.toString());
                    cig.condValues = [];
                });
            }
            payload.items = [];
            payload.itemIds = [];
        }
        return payload;
    }
    validate(): ValidateResult {
        const vresult = Validator(this, validateOption);
        if(this.isSelected){
            if(this.items.length < 1){
                vresult.result = false;
                vresult.errors['items'] = 'Danh sách sản phẩm không được để trống';
            }
        }
        else{
            if(this.conditionItemGroups.length < 1){
                vresult.result = false;
                vresult.errors['list'] = 'Danh sách điều kiện không được để trống'
                
            }
            else{
                this.conditionItemGroups.forEach(row => {
                    if(!row.typeCondition || !row.values?.length){
                        vresult.errors['list'] = true;
                    }
                })
            }
        }
        return vresult;
    }
}

const validateOption: ValidateOption = {
    itmsGrpName: {
        validators: {
            required: true,
            type: String,
            name: 'tên nhóm sản phẩm'
        }
    },
    // items: {
    //     validators: {
    //         type: Array,
    //         required: true,
    //         name: 'danh sách sản phẩm',
    //         minLength: 1
    //     }
    // }
};

export async function fetchProductGroup(skip: number = 0, limit: number = 10, text: string | null = null): Promise<AxiosResponse<IProductGroupResponse>> {
    let uri = `ItemGroup`;
    if(text){
        uri += `ItemGroup/search/${text}`
    }
    uri += '?';
    if (skip && limit) {
        uri += `skip=${skip}&limit=${limit}&`;
    }
    uri += `OrderBy=id desc`
    return await API.get(uri);
}

export async function postProductGroup(payload: IProductGroup): Promise<AxiosResponse<IProductGroupResponse>> {
    return await API.add('ItemGroup/add', payload);
}

export async function getProductGroupById(id: number): Promise<AxiosResponse<IProductGroupResponse>> {
    return await API.get(`ItemGroup/${id}`);
}

export async function deleteProductGroupById(id: number): Promise<AxiosResponse<IProductGroupResponse>> {
    return await API.delete(`ItemGroup/${id}`);
}

export async function updateProductGroup(payload: IProductGroup): Promise<AxiosResponse<IProductGroupResponse>> {
    return await API.update(`ItemGroup/${payload.id}`, payload);
}