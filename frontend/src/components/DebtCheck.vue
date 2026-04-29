<template>
    <div>
        <Button
            v-if="type === 'icon'"
            @click="openDebtDialog()"
            size="small"
            :label="t('common.btn_check_debt')"
            icon="fa-solid fa-circle-info"
            :severity="checkDebt() ? 'primary' : 'danger'"
        />
        <Button
            v-else
            class="w-full"
            @click="openDebtDialog()"
            icon="pi pi-dollar"
            :label="t('common.btn_check_debt')"
            :severity="checkDebt() ? 'primary' : 'danger'"
        />

        <Dialog
            v-model:visible="DebtModal"
            modal
            :header="t('common.dialog_debt_detail')"
            class="md:w-6 w-9"
        >
            <DataTable
                :value="DebtData"
                selectionMode="single"
                dataKey="invoiceNumber"
                v-model:selection="selectedDebt"
                sortMode="single"
                sortField="totalDebt"
                :sortOrder="1"
                showGridlines
                rowGroupMode="rowspan"
                groupRowsBy="totalDebt"
                stripedRows
                :pt="{
                    rowExpansionCell: {
                        class: 'p-0 ',
                    },
                }"
            >
                <template #empty>
                    <div class="p-2 text-center">{{ t('common.no_debt_found') }}</div>
                </template>
                <Column style="width: 3rem">
                    <template #body="sp">
                        <Button
                            v-if="sp.data.payments && sp.data.payments.length"
                            :icon="sp.data.checkOpen ? 'pi pi-minus' : 'pi pi-plus'"
                            size="small"
                            text
                            @click="GetInvoid(sp.data, sp.index)"
                        />
                    </template>
                </Column>
                <Column
                    field="invoiceNumber"
                    :header="t('common.order_invoice_code')"
                    style="width: 15rem"
                >
                </Column>
                <Column
                    field="paymentMethodName"
                    :header="t('common.debt_type')"
                    style="width: 15rem"
                >
                    <template #body="slotProps">
                        <span :class="{ 'text-primary': slotProps.data.type }">{{
                            slotProps.data.paymentMethodName
                        }}</span>
                    </template>
                </Column>
                <Column field="daysOverdue" :header="t('common.overdue')" class="w-10rem"></Column>
                <Column field="amountOverdue" :header="t('common.value')">
                    <template #body="slotProps">
                        <span :class="{ 'text-primary': slotProps.data.type }">{{
                            Intl.NumberFormat().format(slotProps.data.amountOverdue)
                        }}</span>
                    </template>
                </Column>
                <Column field="totalDebt" :header="t('common.total_debt')" style="width: 300px">
                    <template #body>
                        <div class="flex flex-column gap-3">
                            <span class="flex flex-column gap-2">
                                {{ t('common.debt_unsecured') }}:<br />
                                <strong :class="!checkDebtCredit ? 'text-red-500' : ''"
                                    >{{
                                        Intl.NumberFormat().format(
                                            TotalDebtData.filter((val) => {
                                                return (
                                                    val.paymentMethodCode == "PayCredit"
                                                );
                                            })[0]?.balance + SetTotal("PayCredit")
                                        )
                                    }}
                                    /
                                    {{
                                        Intl.NumberFormat().format(
                                            TotalDebtData.filter((val) => {
                                                return (
                                                    val.paymentMethodCode == "PayCredit"
                                                );
                                            })[0]?.balanceLimit
                                        )
                                    }}</strong
                                >
                            </span>
                            <span class="flex flex-column gap-2">
                                {{ t('common.debt_guaranteed') }}: <br />
                                <strong :class="!checkDebtPayBank ? 'text-red-500' : ''">
                                    {{
                                        Intl.NumberFormat().format(
                                            TotalDebtData.filter((val) => {
                                                return (
                                                    val.paymentMethodCode ==
                                                    "PayGuarantee"
                                                );
                                            })[0]?.balance + SetTotal("PayGuarantee")
                                        )
                                    }}
                                    /
                                    {{
                                        Intl.NumberFormat().format(
                                            TotalDebtData.filter((val) => {
                                                return (
                                                    val.paymentMethodCode ==
                                                    "PayGuarantee"
                                                );
                                            })[0]?.balanceLimit
                                        )
                                    }}</strong
                                >
                            </span>
                            <span></span>
                        </div>
                    </template>
                </Column>
            </DataTable>
            <template #footer>
                <div>
                    <Button
                        @click="DebtModal = false"
                        :label="t('common.btn_close')"
                        severity="secondary"
                    />
                </div>
            </template>
        </Dialog>
    </div>
</template>
<script setup>
import { computed, ref, watchEffect } from "vue";
import { useI18n } from "vue-i18n";
const { t } = useI18n();

const emit = defineEmits(["checkDebt"]);
const selectedDebt = ref();
const props = defineProps({
    styleClass: {
        type: String,
        default: "",
    },
    type: {
        type: String,
        default: "",
    },
    DebtData: {
        type: Array,
        default: [],
    },
    TotalDebtData: {
        type: Array,
        default: [],
    },
    TotalPayment: {
        type: Number,
    },
    paymentMethod: {
        type: String,
    },
    ItemDebt: {
        type: Object,
    },
});

const DebtModal = ref(false);

const openDebtDialog = () => {
    DebtModal.value = true;
};

const checkDebt = () => {
    const getBalance = (method) =>
        props.TotalDebtData.find((val) => val.paymentMethodCode === method) || {};

    let { balance: balanceCredit, balanceLimit: balanceLimitCredit } = getBalance(
        "PayCredit"
    );
    let { balance: balancePayBank, balanceLimit: balanceLimitPayBank } = getBalance(
        "PayGuarantee"
    );

    balanceCredit += props.TotalPayment;
    if (balanceCredit > balanceLimitCredit) {
        emit("checkDebt", false);
        return false;
    }
    balancePayBank += props.TotalPayment;
    if (balancePayBank > balanceLimitPayBank) {
        emit("checkDebt", false);
        return false;
    }
    emit("checkDebt", true);
    return true;
};
const checkDebtCredit = computed(() => {
    let { balance, balanceLimit } =
        props.TotalDebtData.find((val) => val.paymentMethodCode === "PayCredit") || {};
    balance += SetTotal("PayCredit");

    return balance <= balanceLimit;
});

const checkDebtPayBank = computed(() => {
    let { balance, balanceLimit } =
        props.TotalDebtData.find((val) => val.paymentMethodCode === "PayGuarantee") || {};
    balance += SetTotal("PayGuarantee");
    return balance <= balanceLimit;
});

const SetTotal = (type) => {
    if (props.paymentMethod == type) return props.TotalPayment;
    return 0;
};

const GetInvoid = (data, index) => {
    if (data.checkOpen) {
        data.checkOpen = false;
        const dataDelete = props.DebtData.filter(
            (el) => el.docCode === data.invoiceNumber
        );
        if (dataDelete.length) {
            dataDelete.forEach((el) => {
                let i = props.DebtData.findIndex(
                    (item) => item.invoiceNumber === el.invoiceNumber
                );
                props.DebtData.splice(i, 1);
            });
        }
        selectedDebt.value = {};
    } else {
        data.checkOpen = true;
        selectedDebt.value = data;
    }
    if (data.payments && data.payments.length && data.checkOpen) {
        const objTemPlate = data.payments.map((el) => ({
            invoiceNumber: el.paymentCode,
            paymentMethodName: el.paymentMethodName,
            amountOverdue: el.remainingAmount,
            docCode: el.docCode,
        }));
        objTemPlate.forEach((item) => {
            props.DebtData.splice(index + 1, 0, item);
        });
    }
};

watchEffect(() => {
    const dataDelete = props.DebtData.filter((el) => el.type === "draft");
    if (dataDelete.length) {
        dataDelete.forEach((el) => {
            let i = props.DebtData.findIndex((item) => item.type === el.type);
            props.DebtData.splice(i, 1);
        });
    }
    if (props.ItemDebt != null) {
        props.DebtData.unshift(props.ItemDebt);
    }
});
</script>
<style></style>
