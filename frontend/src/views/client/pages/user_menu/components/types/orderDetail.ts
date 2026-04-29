export interface ItemDetail {
    id: number;
    fatherId: number;
    type: string | null;
    promotionId: number | null;
    itemId: number;
    isSync: boolean;
    lineId: number;
    itemCode: string;
    itemName: string;
    baseId: number;
    baseObj: number;
    parentItemId: number | null;
    serialBatchNumber: string | null;
    paymentStatus: string | null;
    quantity: number;
    price: number;
    priceAfterDist: number;
    discount: number;
    distcountAmount: number;
    itemVolume: number;
    vat: number;
    vatId: number | null;
    vatCode: string;
    vatName: string | null;
    vatAmount: number;
    note: string;
    result: string | null;
    ouomId: number;
    uomCode: string;
    uomName: string;
    paymentMethodCode: string;
    item: any;
    numInSale: number;
    document: any;
    createdAt: string;
}

export interface Promotion {
    id: number;
    fatherId: number;
    lineId: number;
    listLineId: number[];
    promotionId: number;
    price: number;
    promotionCode: string;
    promotionName: string;
    promotionDesc: string;
    isOtherPromotion: boolean;
    isOtherDist: boolean;
    isOtherPay: boolean;
    hasException: boolean;
    isOtherPromotionExc: boolean;
    isOtherDistExc: boolean;
    isOtherPayExc: boolean;
    itemGroup: any;
    itemId: number;
    addAccumulate: boolean;
    itemCode: string;
    itemName: string;
    packingId: number | null;
    packingName: string;
    volumn: number | null;
    quantityAdd: number;
    note: string | null;
    discount: number | null;
    discountType: string | null;
    document: any;
}

export interface Address {
    id: number; 
    bpId: number;
    address: string;
    locationId: number;
    locationName: string;
    areaId: number;
    areaName: string;
    email: string;
    phone: string;
    vehiclePlate: string;
    cccd: string;
    person: string;
    type: 'S' | 'B';
}

export interface PaymentMethod {
    id: number;
    fatherId: number;
    paymentMethodID: number;
    paymentMethodCode: string;
    paymentMethodName: string;
    document: any;
    status?: string;
}

export interface PaymentInfo {
    id: number;
    docId: number;
    vatAmount: number;
    paynowVATAmount: number;
    debtVATAmount: number;
    debtGuaranteeVATAmount: number;
    bonusCommited: number;
    totalBeforeVat: number;
    totalPayNowBeforeVat: number;
    totalDebtBeforeVat: number;
    totalDebtGuaranteeBeforeVat: number;
    totalAfterVat: number;
    bonusPercent: number;
    bonusAmount: number;
    totalPayNow: number;
    totalDebt: number;
    totalDebtGuarantee: number;
}

export interface Author {
    isSupperUser: boolean;
    roleId: number;
    personRoleId: number;
    personRole: any;
    role: any;
    userRoles: any;
    fullName: string;
    phone: string;
    userType: string;
    note: string;
    dob: string;
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

export interface AttachFile {
    id?: number;
    fileName?: string;
    filePath?: string;
    note?: string;
    memo?: string;
    fatherId?: number;
    status?: string | null;
    document?: any;
    uploadFileAt?: string;
    authorId?: number;
    authorName?: string;
    author?: Author;
    _file?: File;
  }
export interface Order {
    id: number;
    docType: string;
    invoiceCode: string;
    internalCust: boolean;
    externalCust: boolean;
    cardId: number;
    currency: string;
    sapDocEntry: string | null;
    cardCode: string;
    cardName: string;
    isIncoterm: boolean;
    incotermType: string;
    docDate: string;
    dueDate: string;
    confirmAt: string;
    discount: number;
    distcountAmount: number;
    discountByPromotion: number | null;
    distcountAmountPromotion: number | null;
    vatAmount: number;
    totalBeforeVat: number;
    totalAfterVat: number | null;
    total: number;
    bonus: number;
    bonusAmount: number;
    totalPayment: number | null;
    payingAmount: number | null;
    note: string;
    sellerNote: string | null;
    objType: number;
    userId: number;
    year: number | null;
    subPeriods: string | null;
    noPeriods: string | null;
    status: string;
    deliveryTime: string;
    wddStatus: string;
    limitRequire: number | null;
    limitOverDue: number | null;
    other: string | null;
    memo: string | null;
    author: Author;
    itemDetail: ItemDetail[];
    promotion: Promotion[];
    address: Address[];
    paymentMethod: PaymentMethod[];
    payments?: PaymentMethod[];
    tracking: any[];
    attachFile: AttachFile[];
    attDocuments: AttachFile[];
    approval: any;
    bp: any;
    quarterlyCommitmentBonus: number;
    yearCommitmentBonus: number;
    isSync: boolean;
    paymentInfo: PaymentInfo;
    vnPayStatus: boolean;
}
