<template>
    <div class="border-1 border-200 p-3 flex flex-column gap-3">
        <div class="font-bold text-xl">{{ t('common.payment_info') }}</div>
        <hr />
        <div class="flex justify-content-between">
            <div>{{ t('body.client.order_before_discount') }}:</div>
            <div class="font-bold">
                {{ formatNumber(props.paymentInfo?.totalBeforeVat) }}
            </div>
        </div>
        <div class="flex justify-content-between">
            <div>{{ t('body.client.other_promotions') }}:</div>
            <div class="font-bold">
                {{ formatNumber(0) }}
            </div>
        </div>
        <div class="flex justify-content-between">
            <div>
                {{ t('body.client.commitment_bonus') }}:
                <i
                    v-if="props.showInfoButton"
                    class="pi pi-info-circle"
                    @click="onClickOpen(DialogOption.SLCK, t('body.client.commitment_bonus'))"
                />
            </div>
            <div class="font-bold">
                {{ formatNumber(props.paymentInfo?.bonusCommited) }}
            </div>
        </div>
        <div class="flex justify-content-between">
            <div>
                {{ t('body.client.ttn_bonus') }}:
                <i
                    v-if="props.showInfoButton"
                    class="pi pi-info-circle"
                    @click="onClickOpen(DialogOption.TTN, t('body.client.ttn_bonus'))"
                />
            </div>
            <div class="font-bold">
                {{ formatNumber(props.paymentInfo?.bonusAmount) }}
            </div>
        </div>
        <hr />
        <div class="flex justify-content-between">
            <div>
                {{ t('body.client.order_after_discount') }}:
                <i
                    v-if="props.showInfoButton"
                    class="pi pi-info-circle"
                    @click="onClickOpen(DialogOption.SCK, t('body.client.order_after_discount'))"
                />
            </div>
            <div class="font-bold">
                {{
                    formatNumber(
                        (props.paymentInfo?.totalBeforeVat || 0) -
                            (props.paymentInfo?.bonusCommited || 0)
                    )
                }}
            </div>
        </div>
        <div class="flex justify-content-between">
            <div>
                {{ t('body.client.total_tax') }}:
                <i
                    v-if="props.showInfoButton"
                    class="pi pi-info-circle"
                    @click="onClickOpen(DialogOption.THUE, t('body.client.total_tax'))"
                />
            </div>
            <div class="font-bold">
                {{ formatNumber(props.paymentInfo?.vatAmount) }}
            </div>
        </div>
        <hr />
        <div class="flex justify-content-between">
            <div>{{ t('body.client.pay_now') }}:</div>
            <div class="font-bold">
                {{ formatNumber(props.paymentInfo?.totalPayNow) }}
            </div>
        </div>
        <div class="flex justify-content-between">
            <div>{{ t('body.client.credit_debt') }}:</div>
            <div class="font-bold">
                {{ formatNumber(props.paymentInfo?.totalDebt) }}
            </div>
        </div>
        <div class="flex justify-content-between">
            <div>{{ t('body.client.guarantee_debt') }}:</div>
            <div class="font-bold">
                {{ formatNumber(props.paymentInfo?.totalDebtGuarantee) }}
            </div>
        </div>
        <hr />
        <div class="flex justify-content-between font-bold">
            <div>{{ t('body.client.total_invoice') }}:</div>
            <div class="font-bold">
                <span>
                    {{ formatNumber(props.paymentInfo?.totalAfterVat) }}
                </span>
            </div>
        </div>
        <div class="flex justify-content-between font-bold">
            <div>{{ t('body.client.total_payment') }}:</div>
            <div class="font-bold">
                <span>
                    {{
                        formatNumber(
                            (props.paymentInfo?.totalAfterVat || 0) -
                                (props.paymentInfo?.bonusAmount || 0)
                        )
                    }}
                </span>
            </div>
        </div>
        <div>
            <DataTable :value="props.promotion" showGridlines>
                <Column :header="t('common.promo_code')" field="promotionName"></Column>
                <Column :header="t('common.description')" field="promotionDesc"></Column>
                <template #empty>
                    <div class="py-5 text-center text-500 font-italic">
                        {{ t('common.no_promotion') }}
                    </div>
                </template>
            </DataTable>
        </div>
        <Dialog v-model:visible="visible" :header="dlgHeader" class="w-30rem" modal>
            <DataTable :value="detailTable" showGridlines>
                <Column field="label"></Column>
                <Column field="value" :header="t('common.value_vnd')" class="w-13rem text-right">
                    <template #body="{ data }">
                        {{ formatNumber(data?.value) }}
                    </template>
                </Column>
                <ColumnGroup type="footer">
                    <Row>
                        <Column :footer="t('common.total') + ':'"></Column>
                        <Column
                            :footer="formatNumber(sumValueDetailTable)"
                            class="text-right"
                        >
                        </Column>
                    </Row>
                </ColumnGroup>
            </DataTable>
            <template #footer>
                <Button
                    :label="t('common.btn_close')"
                    @click="visible = false"
                    severity="secondary"
                />
            </template>
        </Dialog>
    </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, watch, onMounted } from "vue";
import { useI18n } from "vue-i18n";
const { t } = useI18n();

interface Payment {
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
interface Promotion {
    id: number;
    fatherId: number;
    lineId: number;
    promotionId: number;
    price: number;
    promotionCode: string | null;
    promotionName: string | null;
    promotionDesc: string | null;
    itemGroup: any;
    itemId: any;
    itemCode: any;
    itemName: any;
    packingId: any;
    packingName: any;
    volumn: any;
    quantityAdd: any;
    note: any;
    discount: any;
    discountType: any;
    document: any;
}
interface Props {
    paymentInfo?: Payment;
    promotion?: Promotion;
    showInfoButton?: boolean;
    currency?: string;
}
enum DialogOption {
    SLCK = "SLCK",
    TTN = "TTN",
    SCK = "SCK",
    THUE = "THUE",
}
const visible = ref(false);
const props = withDefaults(defineProps<Props>(), {
    showInfoButton: () => true,
    currency: () => "VND",
});
const rowOption: { [key: string]: Array<{ label: string; propName: string }> } = {
    [DialogOption.SLCK]: [],
    [DialogOption.SCK]: [
        {
            propName: "totalPayNowBeforeVat", // "TotalPayNowBeforeVat",
            label: "Thanh toán ngay",
        },
        {
            propName: "totalDebtBeforeVat", //"totalDebtBeforeVat",
            label: "Tín chấp",
        },
        {
            propName: "totalDebtGuaranteeBeforeVat", //"totalDebtGuaranteeBeforeVat",
            label: "Bảo lãnh",
        },
    ],
    [DialogOption.THUE]: [
        {
            propName: "paynowVATAmount", // "TotalPayNowBeforeVat",
            label: "Thanh toán ngay",
        },
        {
            propName: "debtVATAmount", //"totalDebtBeforeVat",
            label: "Tín chấp",
        },
        {
            propName: "debtGuaranteeVATAmount", //"totalDebtGuaranteeBeforeVat",
            label: "Bảo lãnh",
        },
    ],
};
const sumValueDetailTable = computed(() =>
    detailTable.value.reduce((a, b) => a + b.value, 0)
);
const detailTable = ref<Array<{ label: string; value: number | any }>>([]);
const dlgHeader = ref("");
const onClickOpen = (option: DialogOption, header: string = "") => {
    detailTable.value = rowOption[option].map((item) => {
        return {
            label: item.label,
            value: props.paymentInfo?.[item.propName] || 0,
        };
    });
    dlgHeader.value = header;
    visible.value = true;
};

const formatNumber = (input: number | string | any) => {
    if (!input) return 0;
    const number = parseFloat(input.toString().replace(/,/g, ""));
    return Intl.NumberFormat().format(number);
};

const initialComponent = () => {
    // code here
};

onMounted(function () {
    initialComponent();
});
</script>

<style scoped>
.bill-detail {
    padding: 1rem;
}

hr {
    margin: 0;
}
i {
    color: var(--primary-color);
    cursor: pointer;
}
i:hover {
    color: rgb(23, 172, 47);
    font-weight: 500;
}
</style>
