<template>
    <div class="card">
        <div class="grid">
            <div class="col-12 md:col-6 py-0">
                <div class="field grid">
                    <label class="col-fixed">{{ t('body.report.table_header_customer_code_1') }}</label>
                    <div class="col">
                        <Button @click="onClickCustomerDetail" :label="odStore.order?.cardCode" text
                            class="p-0 font-bold"/>
                    </div>
                </div>
            </div>
            <div class="col-12 md:col-6 py-0">
                <div class="field grid">
                    <label class="col-fixed">{{ t('body.PurchaseRequestList.requestCode') }}</label>
                    <div class="col">{{ odStore.order?.invoiceCode }}</div>
                </div>
            </div>
            <div class="col-12 md:col-6 py-0">
                <div class="field grid">
                    <label class="col-fixed">{{ t('client.tax_code') }}</label>
                    <div class="col">{{ odStore.customer?.licTradNum }}</div>
                </div>
            </div>
            <div class="col-12 md:col-6 py-0">
                <div class="field grid">
                    <label class="col-fixed">{{ t('client.creator') }}</label>
                    <div class="col">{{ odStore.order?.author?.fullName }}</div>
                </div>
            </div>
            <div class="col-12 md:col-6 py-0">
                <div class="field grid">
                    <label class="col-fixed">{{ t('client.customerName') }}</label>
                    <div class="col">{{ odStore.order?.cardName }}</div>
                </div>
            </div>

            <div class="col-12 md:col-6 py-0">
                <div class="field grid">
                    <label class="col-fixed">{{ t('client.created_date') }}</label>
                    <div class="col">
                        {{
                            odStore.order?.docDate
                                ? format(
                                    odStore.order?.docDate,
                                    "HH:mm - dd/MM/yyyy"
                                )
                                : ""
                        }}
                    </div>
                </div>
            </div>
            <div class="col-12 md:col-6 py-0">
                <div class="field grid">
                    <label class="col-fixed">{{ t('body.PurchaseRequestList.status') }}</label>
                    <div class="col">
                        <Tag :class="formatStatus(odStore.order?.status || '')?.class
                            " :value="formatStatus(odStore.order?.status || '')?.label
                                        " />
                    </div>
                </div>
            </div>

            <div class="col-12 md:col-6 py-0">
                <div class="field grid">
                    <label class="col-fixed">{{ t('body.OrderList.order_date_label') }}</label>
                    <div class="col">
                        {{
                            odStore.order?.deliveryTime
                                ? format(
                                    odStore.order?.deliveryTime,
                                    "HH:mm - dd/MM/yyyy"
                                )
                                : ""
                        }}
                    </div>
                </div>
            </div> 
        </div>
    </div>
</template>

<script setup lang="ts">
import { format } from "date-fns";
import {  onMounted } from "vue";
import { formatStatus } from "../script";
import { useOrderDetailStore } from "../store/orderDetail";
import router from "@/router";
import { useI18n } from "vue-i18n";

const { t } = useI18n();

const odStore = useOrderDetailStore();

const onClickCustomerDetail = () => {
    router.push({
        name: "agencyCategory-detail",
        params: {
            id: odStore.order?.cardId,
        },
    });
};

const initialComponent = () => {
    // code here
};

onMounted(function () {
    initialComponent();
});
</script>

<style scoped>
.field.grid {
    border-bottom: 1px solid var(--surface-200);
    margin-left: 4px;
}

label {
    width: 13rem;
    font-weight: bold;
}
</style>
