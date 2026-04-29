export interface ItemMasterData {
    id: number;
    series: any;
    itemCode: string;
    itemName: string;
    frgnName: string | null;
    ugpEntry: number;
    price: number;
    note: string;
    createdDate: string;
    creator: any | null;
    updatedDate: string;
    updator: any | null;
    quantity: number;
    itM1: Array<{
        id: number;
        fileName: string;
        filePath: string;
        note: string | null;
        itemId: number;
        status: any | null;
    }>;
    brandId: number;
    brand: {
        id: number;
        code: string;
        name: string;
        sapCode: string;
    };
    industryId: number;
    industry: {
        id: number;
        code: string;
        name: string;
        brands: any | null;
        sapCode: string;
    };
    itemTypeId: number;
    itemType: {
        id: number;
        code: string;
        name: string;
        sapCode: string | null;
    };
    packingId: number;
    packing: {
        id: number;
        code: string;
        name: string;
        sapId: number;
        volumn: number;
        type: string;
    };
    ougp: {
        id: number;
        ugpCode: string;
        ugpName: string;
        baseUom: number;
        sapId: number;
        ugP1: any[];
        ouom: {
            id: number;
            uomCode: string;
            uomName: string;
            sapId: number;
            status: boolean;
        };
        status: boolean;
    };
    taxGroupsId: number;
    taxGroups: {
        id: number;
        code: string;
        name: string;
        rate: number;
        effDate: string;
        type: string;
        locked: boolean;
    };
    onHand: any | null;
    onOrder: any | null;
    isActive: boolean;
    productGroupCode: any | null;
    productApplicationsCode: any | null;
    productQualityLevelCode: any | null;
    itemGroupId: number;
    productGroup: any | null;
    productApplications: any | null;
    productQualityLevel: any | null;
    pendingOndHand: number;
    canBePlacedOnHand: number;
}
