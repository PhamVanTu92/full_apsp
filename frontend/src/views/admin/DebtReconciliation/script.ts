import { IDebtRec, DebtRec, DebtRecResponse } from './entities/DebtRec';
import { CustomerResponse } from './entities/Customer';
import API from '@/api/api-main';
import { AxiosResponse } from 'axios';

/**
 * @param payload type IDebtRec
 * @returns Array of DebtRec
 */
export async function getDebtRecData(skip: number = 0, limit: number = 10, search?: string): Promise<DebtRecResponse> {
    //AxiosResponse<IDebtRec>
    let uri = `DebtReconciliation?Page=${skip}&PageSize=${limit}&OrderBy=createdAt desc`;
    if (search) {
        uri += `&search=${search?.trim()}`;
    }
    const res = (await API.get(uri)) as AxiosResponse<DebtRecResponse>;
    return res.data;
}
export async function getDebtRecDetail(id: number): Promise<AxiosResponse<IDebtRec>> {
    //AxiosResponse<IDebtRec>
    let uri = `DebtReconciliation/${id}`;
    return (await API.get(uri));
}

export async function handleCreate(payload: IDebtRec): Promise<AxiosResponse<IDebtRec>> {
    //AxiosResponse<IDebtRec>
    let uri = `DebtReconciliation`;
    return await API.add(uri, payload);
}

export async function handleUpdate(payload: IDebtRec): Promise<AxiosResponse<IDebtRec>> {
    //AxiosResponse<IDebtRec>
    let uri = `DebtReconciliation/${payload.id}`;
    return await API.update(uri, payload);
}

export async function handleSaveFiles(id: number, files: Array<any>): Promise<AxiosResponse | null> {
    //AxiosResponse<IDebtRec>
    if (files.length < 1) {
        return null;
    }
    let uri = `DebtReconciliation/${id}/attachments`;
    const formdata = new FormData();
    files.forEach((row) => {
        if(!row.id){
            formdata.append('files', row);
        }
    });
    return await API.add(uri, formdata);
}
export async function handleChangeStatus(id: number, status: string): Promise<AxiosResponse | null> {
    //AxiosResponse<IDebtRec>
    let uri = `DebtReconciliation/${id}/change-status/${status}`;
    return await API.update(uri);
}

export async function searchCustomer(text: string): Promise<AxiosResponse<CustomerResponse>> {
    // delay 1000ms
    return await API.get(`customer?search=${text?.trim()}`);
}

const status = {
    P: {
        severity: 'warning',
        label: 'Chờ xác nhận'
    },
    A: {
        severity: 'success',
        label: 'Đã xác nhận'
    },
    R: {
        severity: 'danger',
        label: 'Từ chối'
    },
    C: {
        severity: 'secondary',
        label: 'Đã hủy'
    }
};

export function getStatus(str: string): { severity: string; label: string } {
    return (
        status[str] || {
            severity: 'contrast',
            label: 'Unknown'
        }
    );
}

export function getActionDisable(str: string | null): boolean {
    return (['P', 'A', 'C'] as Array<any>).includes(str);
}
