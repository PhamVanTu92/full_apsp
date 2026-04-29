import { Validator, ValidateResult } from '../../../helpers/validate';
import sampleData from './data/OrgData.json';
import API from '@/api/api-main';
import { ref } from 'vue';
import { AxiosResponse } from 'axios';


export interface Employee {
    id: number;
    fullName: string | null;
    userName: string | null;
    email: string | null;
    isLeader: boolean | null;
    isDelete?: string | null;
}

export interface OrgStruct {
    id: number | null;
    code: string;
    name: string;
    key?: string | undefined;
    label?: string;
    parentId: number | null | object | any;
    parent?: OrgStruct | null;
    children: Array<OrgStruct> | null;
    employees: Array<Employee> | null;
    isActive: boolean;
    description: string | null;
    managerUserId: number | null;
    managerUser: Employee | null;
    employeesCount: number | null;
}

const orgStructValidateOption = {
    // code: { validators: { type: String, required: true, name: 'mã cơ cấu tổ chức' } },
    name: { validators: { type: String, required: true, name: 'tên cơ cấu tổ chức' } },
    fatherId: { validators: { type: Number, required: true, nullMessage: 'Vui lòng chọn cơ cấu tổ chức cha' } }
};

export class OrgStruct {
    constructor(data?: any) {
        if (data) {
            Object.assign(this, data);
            return this;
        }
        this.id = null;
        this.code = '';
        this.name = '';
        this.parentId = 0;
        this.children = null;
        this.isActive = true;
        this.description = null;
    }
    validate(): ValidateResult {
        return Validator(this.toJSON(), orgStructValidateOption);
    }

    toJSON(): object {
        const result = {
            name: this.name?.trim(),
            parentId: this.parentId,
            code: this.code?.trim(),
            description: this.description?.trim(),
            isActive: this.isActive,
        };
        if(this.id){
            result['id'] = this.id;
        }
        if(typeof result.parentId === 'object') {
            result.parentId = Object.keys(result.parentId)[0] || null
            if(result.parentId == "null"){
                result.parentId = null;
            }
        }
        return result;
    }

    dispose(): void {
        this.id = null;
        this.code = '';
        this.name = '';
        this.parentId = null;
        this.children = null;
        this.description = null;
    }
}

const findLeader = (array : Array<Employee> | null, employeeLeaderId: number | null ) : Array<Employee> | null => {
    if(employeeLeaderId && array){
        for(const epl of array){
            if(epl.id === employeeLeaderId){
                epl.isLeader = true;
                // break;
            }
            else{
                epl.isLeader = false;
            }
        }
    }
    return array;
}

const convertToOrgStructTree = (data: Array<OrgStruct> | Array<any>): Array<OrgStruct> => {
    const result: Array<OrgStruct> = [];
    for (const item of data) {
        const orgStruct = new OrgStruct();
        Object.assign(orgStruct, item);
        orgStruct.key = `${item.id}`;
        orgStruct.label = item.name;
        orgStruct.children = convertToOrgStructTree(item.children || []);
        result.push(orgStruct);
    }
    return result;
};

// OrgStruct API methods
export async function getOrgStruct(): Promise<Array<OrgStruct>> {
    try {
        const res = await API.get('OrganizationUnit/tree');
        return convertToOrgStructTree(res.data);
    } catch (error) {
        console.error(error);
        return [];
    }
}

export async function getOrgStructDetail(id: number): Promise<OrgStruct | null> {
    const orgStruct = new OrgStruct();
    const res = await API.get(`OrganizationUnit/${id}`) ;
    if(res.data){
        Object.assign(orgStruct, res.data);
        orgStruct.employees = findLeader(orgStruct.employees,orgStruct.managerUserId);
        return orgStruct;
    }
    else{
        throw new Error('Không tìm thấy cơ cấu tổ chức');
    }
}
export async function addOrgStruct(payload: OrgStruct): Promise<AxiosResponse> {
    return await API.add('OrganizationUnit', payload.toJSON());
}
export async function updateOrgStruct(id :number ,payload: OrgStruct): Promise<AxiosResponse> {
    return await API.update(`OrganizationUnit/${id}`, payload.toJSON());
}

export async function deleteOrgStruct(id: number | null): Promise<AxiosResponse> {
    return await API.delete(`OrganizationUnit/${id}`);
}

//Employee API methods
export async function saveChoseLeader(orgId: number | null, leaderId: number): Promise<AxiosResponse> {
    return await API.update(`OrganizationUnit/${orgId}/set-manager/${leaderId}`);
}
export async function deleteEmployee(orgId: number | null, deleteIds: Array<number>): Promise<AxiosResponse> {
    return await API.delete(`OrganizationUnit/${orgId}/employees`, deleteIds);
}

export async function addEmployee(orgId: number | null, employeeIds: Array<number>): Promise<AxiosResponse> {
    return await API.add(`OrganizationUnit/${orgId}/employees`, employeeIds);
}