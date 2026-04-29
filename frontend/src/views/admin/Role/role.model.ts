import { cloneDeep } from "lodash";

export interface RoleClaim {
    privilegesId: number;
    checked: boolean;
    partialChecked: boolean;
}

export class RoleClaim {
    constructor(roleClaim : any){
        this.checked = roleClaim.checked;
        this.partialChecked = roleClaim.partialChecked;
        this.privilegesId = roleClaim.privilegesId;
        return this
    }
}

export interface Role {
    id: number | null;
    name: string | null;
    normalizedName: string | null;
    concurrencyStamp: string | null;
    isActive: boolean;
    notes: string | null;
    roleClaims: Array<RoleClaim>;
    privilegeIds: Array<number>;
    selectClaims: any;
    isSaleRole: boolean;
    roleFillCustomerGroups: any[]
    isFillCustomerGroup: boolean;
    toJSON() : object
}

const convertClaimToSelect = (claims: Array<any>): Object => {
    const result = {} as any;
    claims.forEach((item) => {
        let { checked, partialChecked } = item;
        result[`${item.privilegesId}`] = {
            checked,
            partialChecked,
        };
    });
    return result;
};

export class Role {
    constructor(role? : any) {
        if(!role){
            this.roleClaims = Array<RoleClaim>();
            this.privilegeIds = Array<number>();
            this.name = null;
            this.isSaleRole = true;
            this.isFillCustomerGroup = false;
            this.roleFillCustomerGroups = [];
            this.isActive = true;
        }
        else{
            Object.assign(this, role);
            this.selectClaims = convertClaimToSelect(this.roleClaims);
            if(this.isSaleRole === undefined) {
                this.isSaleRole = false;
            }
            this.roleFillCustomerGroups = this.roleFillCustomerGroups?.map(item => item.customerGroupId);
        }
        return this;
    }
    toJSON() {
        const payload = cloneDeep(this);
        const roleClaims = [] as Array<RoleClaim>;
        const privilegeIds = [] as Array<number>;
        for (const key in payload.selectClaims) {
            const selectClaim = new RoleClaim(payload.selectClaims?.[key]);
            const keyNumber = parseInt(key);
            selectClaim.privilegesId = keyNumber;
            roleClaims.push(selectClaim);
            privilegeIds.push(keyNumber);
        }
        payload.roleClaims = roleClaims;
        payload.privilegeIds = privilegeIds;
        payload.isFillCustomerGroup = !payload.isSaleRole;
        payload.roleFillCustomerGroups = payload.roleFillCustomerGroups?.map(id => ({
            customerGroupId: id
        }))
        const { selectClaims, ...role } = payload;
        return role;
    }

}
