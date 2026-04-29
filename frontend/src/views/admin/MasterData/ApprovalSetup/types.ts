export interface Pagable {
    rows: number;
    page: number;
    totalRecords: number;
}

export interface DocItem {
    name: string;
    transType: number;
    status: string;
    selected: number | null;
}

export interface ApprovalProcess {
    id?: number;
    wtsId: number;
    fatherId?: number;
    sort: number;
    selectedAppLv: boolean;
    status: string;
    name: string;
    description: string;
    isCheck: boolean;
}

export interface UserRole {
    id?: number;
    name?: string;
}

export interface UserInfo {
    id: number;
    fullName: string;
    email: string;
    role: UserRole;
}

export interface FormData {
    id: number;
    approvalSampleName: string;
    description: string;
    isActive: boolean;
    isDebtLimit?: boolean;
    isOverdueDebt?: boolean;
    isOther?: boolean;
    approvalSampleProcessesLines: ApprovalProcess[];
    approvalSampleDocumentsLines: number[];
    approvalSampleMembersLines: number[];
    rUsers?: UserInfo[];
}

export interface ApprovalTemplate {
    id: number;
    approvalSampleName: string;
    description: string;
    isActive: boolean;
}

export interface ApiResponse<T> {
    result?: T;
    data?: T;
    message?: string;
    total?: number;
}

export interface SubmitPayload {
    approvalSampleName: string;
    description: string;
    isActive: boolean;
    isDebtLimit?: boolean;
    isOverdueDebt?: boolean;
    isOther?: boolean;
    approvalSampleDocumentsLines: number[];
    approvalSampleProcessesLines: number[];
    approvalSampleMembersLines: number[];
}

export interface ApprovalMember {
    id?: number;
    creatorId: number;
    creator?: UserInfo;
}

export interface ApprovalDocument {
    id?: number;
    document: number;
}

export interface ApprovalLevelInfo {
    approvalLevelId: number;
    approvalLevelName?: string;
    description?: string;
}

export interface ApprovalProcessLine {
    id?: number;
    approvalLevelId: number;
    fatherId?: number;
    approvalLevel?: ApprovalLevelInfo;
}

export interface ApprovalSampleDetail {
    id: number;
    approvalSampleName: string;
    description: string;
    isActive: boolean;
    isDebtLimit?: boolean;
    isOverdueDebt?: boolean;
    isOther?: boolean;
    approvalSampleMembersLines?: ApprovalMember[];
    approvalSampleDocumentsLines?: ApprovalDocument[];
    approvalSampleProcessesLines?: ApprovalProcessLine[];
}
