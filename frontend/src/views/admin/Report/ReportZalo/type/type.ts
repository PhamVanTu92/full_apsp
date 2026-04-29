export interface ZaloTokenData {
    id: number;
    access_token: string;
    refresh_token: string;
    expires_in: number;
    templateConfirmed: string;
    templateCompleted: string;
}

export const zaloTokenData: ZaloTokenData = {
    id: 0,
    access_token: '', 
    refresh_token: '',
    expires_in: 0,
    templateConfirmed: "",
    templateCompleted: "",
};