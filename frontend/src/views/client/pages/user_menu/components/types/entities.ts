import { ItemMasterData } from './item.type';
import { PromotionInCheckPricePayload } from './script';
import cloneDeep from 'lodash/cloneDeep';
import { PromotionResponse } from './promotionOrderLine.type';

export type PaymentMethodCode = 'PayNow' | 'PayCredit' | 'PayGuarantee';
export class ItemDetail {
    itemId!: number;
    itemCode!: string;
    itemName!: string;
    quantity!: number;
    price!: number;
    priceAfterDist!: number;
    discount!: number | null;
    distcountAmount!: number;
    vatCode!: string;
    vat!: number;
    vatAmount!: number;
    lineTotal!: number;
    note!: string;
    ouomId!: number;
    packing!: object;
    exchangePoint!: number;
    uomCode!: string;
    uomName!: string;
    numInSale: number = 0;
    paymentMethodCode!: PaymentMethodCode;
    _volumn!: number;
    _lastDiscount: number | null = null;
    _isPromotion?: boolean;
    _promotionQuanlity: number = 0;
    discountType: 'P' | 'C' = 'P';

    constructor(init?: ItemMasterData, paymentMethodCode: PaymentMethodCode = 'PayNow') {
        if (init) {
            for (const key of Object.keys(this)) {
                this[key as keyof this] = init[key as keyof ItemMasterData];
            }
            this.itemId = init.id;
            this.quantity = 1;
            this.paymentMethodCode = paymentMethodCode;
            (this.vatCode = init.taxGroups?.code), (this.vat = init.taxGroups?.rate), (this.ouomId = init.ougp?.baseUom);
            this.discount = 0;
            this.exchangePoint = init.exchangePoint;
            this.packing = init.packing;
            (this.vatAmount = 0), //????
                (this.distcountAmount = 0); //????
            this.priceAfterDist = 0; //????
            this.ouomId = init.ougp?.baseUom;
            this.uomCode = init.ougp?.ouom.uomCode;
            this.uomName = init.ougp?.ouom.uomName;
            this._volumn = init.packing.volumn;
            this._promotionQuanlity = 0;
            // this.discountType = 'P';
        }
    }
}

export class Address {
    address!: string;
    locationId!: number;
    locationName!: string;
    areaId!: number;
    areaName!: string;
    email!: string;
    phone!: string | null;
    person!: string;
    note!: string | null;
    type!: string;
}

export class PaymentMethod {
    paymentMethodID!: number;
    paymentMethodCode!: string;
    paymentMethodName!: string;
}

export class Customer {
    id!: number;
    cardCode!: string;
    cardName!: string;
    frgnName!: string;
    cardType!: string;
    licTradNum!: string;
    isBusinessHouse!: boolean;
    avatar!: string;
    gender!: string;
    locationId!: number;
    locationName!: string;
    areaId!: number;
    areaName!: string;
    address!: string;
    email!: string;
    phone!: string;
    person!: string;
    customerPoints!: [
        {
            id: number;
            customerId: number;
            poitnSetupId: number;
            name: string;
            startDate: string;
            endDate: string;
            expiryDate: string;
            earnedPoint: number;
            redeemedPoint: number;
            remainingPoint: number;
            status: number;
        }
    ];
    note!: string;
    status!: string;
    isInterCom!: boolean;
    crD1: {
        id: number;
        locationId: number;
        type: 'S' | 'B';
        locationName: string | null;
        areaId: number;
        areaName: string | null;
        address: string | null;
        email: string | null;
        phone: string | null;
        vehiclePlate: string | null;
        cccd: string | null;
        person: string | null;
        note: string | null;
        bpId: number;
        locationType: string;
        default: 'Y' | 'N';
        status: null;
        // getFullAddress?: string
    }[] = [];
    crD2: any[] = [];
    crD3: any[] = [];
    crD4: any[] = [];
    crD5: any[] = [];
    crD6: any[] = [];

    getCrD1Default(type: 'S' | 'B') {
        var _default = this.crD1.find((item) => item.type === type && item.default === 'Y') || null;
        return {
            default: _default,
            fullAddress: [_default?.address, _default?.locationName, _default?.areaName].filter((a) => a).join(', ')
        };
    }

    getAddress(): string {
        return [this.address, this.locationName, this.areaName].filter((item) => item).join(', ');
    }

    constructor(init?: any) {
        if (init) Object.assign(this, init);
    }
}

export class PurchaseOrder {
    id: number = 0;
    docType: string = 'NET';
    invoiceCode: string = '';
    cardId!: number;
    cardCode!: string;
    cardName!: string;
    _customer!: Customer;
    isIncoterm!: boolean;
    incotermType: string = '';
    docDate!: Date | string;
    deliveryTime!: Date;
    discount: number = 0;
    distcountAmount: number = 0;
    vatAmount: number = 0;
    total: number = 0;
    note: string = '';
    userId!: number;
    status: string = '';
    itemDetail: ItemDetail[] = [];
    address!: Address[];
    paymentMethod: PaymentMethod[] = [
        {
            paymentMethodID: 1,
            paymentMethodCode: 'PayNow',
            paymentMethodName: 'Thanh toán ngay'
        }
    ];
    currency!: 'VND' | 'USD';
    bonus: number = 0;
    bonusAmount!: number;
    totalBeforeVat!: number;
    promotion: PromotionInCheckPricePayload[] = [];
    quarterlyCommitmentBonus: number = 0;
    yearCommitmentBonus: number = 0;
    Promotions!: PromotionResponse;

    setCustomer(customer: Customer) {
        this._customer = customer;
        this.cardId = customer.id;
        this.cardCode = customer.cardCode;
        this.cardName = customer.cardName;
        this.isIncoterm = customer.isInterCom;
        if (customer.isInterCom) {
            this.incotermType = '';
            this.currency = 'USD';
        } else {
            this.currency = 'VND';
        }
    }

    getTotalVolumn() {
        return this.itemDetail.reduce((sum, item) => {
            return sum + item._volumn * item.quantity;
        }, 0);
    }

    ADDRESS_FIELDS_2_DELETE = ['bpId', 'default', 'id', 'locationType', 'status'];
    /**
     * @param bonus Thưởng khuyến mãi thanh toán ngay
     * @param bonusAmount Số tiền thưởng khuyến mãi thanh toán ngay
     * @param total Tổng thanh toán
     * @param totalBeforeVatnumber  Tổng thanh toán trước thuế
     * @param vatAmount Tổng tiền thuế
     * @returns Dữ liệu đơn hàng để gửi lên server
     */
    toPayload(bonus: number = 0, bonusAmount: number = 0, total: number = 0, totalBeforeVatnumber: number = 0, vatAmount: number = 0): PurchaseOrder {
        const _payload = cloneDeep(this) as PurchaseOrder;
        _payload.bonus = bonus;
        _payload.bonusAmount = bonusAmount;
        _payload.total = total;
        _payload.totalBeforeVat = totalBeforeVatnumber;
        _payload.vatAmount = vatAmount;
        _payload.docDate = new Date();
        _payload.quarterlyCommitmentBonus = 0;
        _payload.yearCommitmentBonus = 0;
        const user = JSON.parse(localStorage.getItem('user') || '{}');
        _payload.userId = user?.appUser?.id || 0;

        
        const shipAddress = this._customer
            ? cloneDeep(this._customer.getCrD1Default('S')?.default)
            : null;
        const billAddress = this._customer
            ? cloneDeep(this._customer.getCrD1Default('B')?.default)
            : null;
        this.ADDRESS_FIELDS_2_DELETE.forEach((key) => {
            if (shipAddress) {
                delete shipAddress[key as keyof typeof shipAddress];
            }
            if (billAddress) {
                delete billAddress[key as keyof typeof shipAddress];
            }
        });
        _payload.address = [shipAddress as Address, billAddress as Address].filter((item) => item);
        for (let key in _payload) {
            if (key.includes('_')) {
                delete _payload[key as keyof PurchaseOrder];
            }
        }
        let taxRate = 0;
        _payload.itemDetail = _payload.itemDetail.map((item) => {
            taxRate = (item.vat ?? 0) / 100;
            item.priceAfterDist = item.price - (item.discountType == 'C' ? item.discount || 0 : (item.price * (item.discount || 0)) / 100); // [priceAfterDist] Giảm giá trên giá gốc
            item.distcountAmount = item.discountType == 'C' ? (item.discount || 0) * item.quantity : (item.price * (item.discount || 0)) / 100;
            item.vatAmount = item.priceAfterDist * taxRate * item.quantity; // [vatAmount] Thuế trên giá sau giảm
            item.lineTotal = item.priceAfterDist * item.quantity;
            item.numInSale = item._volumn * item.quantity; // [numInSale] Chả biết trường này để làm gì :D?, chắc là tổng sản lượng
            Object.keys(item).forEach((key) => {
                if (key.startsWith('_')) {
                    delete item[key as keyof ItemDetail];
                }
            });
            item.discountType = item.discountType || 'P';
            return item;
        });
        return _payload;
    }

    constructor(customer?: Customer) {
        this.deliveryTime = new Date();
        this.currency = 'VND';
        if (customer) {
            this.setCustomer(customer);
        }
    }
}
