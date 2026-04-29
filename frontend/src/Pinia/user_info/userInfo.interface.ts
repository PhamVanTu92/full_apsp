export interface Privilege {
    children: Privilege[];
    privilegesId: number;
    partialChecked: boolean;
    privilegeCode: string;
    privilegeName: string;
    numberOrder: number;
    checked: boolean;
    id: number;
    roleId: number;
    claimType: string | null;
    claimValue: string | null;
}

export interface PersonRole {
    roleClaims: Privilege[];
    partialChecked: boolean;
    isPersonRole: boolean;
    privilegeIds: number[];
    isActive: boolean;
    countUserInRole: number;
    notes: string | null;
    id: number;
    name: string;
    normalizedName: string;
    concurrencyStamp: string | null;
}

export interface UserInfo {
    claims: any[];
    isSupperUser: boolean;
    roleId: number;
    personRoleId: number;
    personRole: PersonRole;
    userRoles: any;
    fullName: string;
    phone: string;
    userType: string;
    note: string;
    dob: string; // ISO date string
    emailConfirmed: boolean;
    status: string;
    cardId: number | null;
    bpInfo: any;
    userGroup: any;
    lastLogin: string;
    organizationId: number;
    directStaff: any[];
    id: number;
    userName: string;
    normalizedUserName: string;
    email: string;
    normalizedEmail: string;
    phoneNumber: string | null;
}