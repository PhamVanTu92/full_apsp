<template>
    <div class="flex justify-content-between align-items-center mb-3">
        <div class="text-2xl font-bold">
            {{ t('client.import_plan_details') }} - {{ modelView?.planCode }}
        </div>
        <div>
            <Button
                class="mr-2"
                icon="pi pi-arrow-left"
                :label="t('body.home.back_button')"
                severity="secondary"
                @click="router.back()"
            />
            <Button
                icon="pi pi-check"
                :label="t('body.OrderApproval.confirm')"
                v-if="
                    modelView?.status === 'P' &&
                    authStore.userType === 'APSP'
                "
                @click="onConfirmStatus()"
            />
        </div>
    </div>
    <div class="card">
        <div class="grid m-0 mb-3 border-1 border-200">
            <div class="col-7">
                <div class="grid">
                    <div class="col-3 font-bold">{{ t('body.sampleRequest.importPlan.plan_id_label') }}</div>
                    <div class="col px-0">{{ modelView?.planCode }}</div>
                </div>
                <div class="grid">
                    <div class="col-3 font-bold">{{ t('body.sampleRequest.importPlan.plan_name_label') }}</div>
                    <div class="col px-0">{{ modelView?.planName }}</div>
                </div>
                <div class="grid">
                    <div class="col-3 font-bold">{{ t('body.sampleRequest.importPlan.customer_label') }}</div>
                    <div class="col px-0">{{ modelView?.customerName }}</div>
                </div>
                <div class="grid">
                    <div class="col-3 font-bold">{{ t('body.home.time_label') }}</div>

                    <div class="col px-0">
                        {{" "}}
                        {{
                            modelView.startDate
                                ? format(parseISO(modelView.startDate), "dd/MM/yyyy")
                                : "N/A"
                        }}
                        -
                        {{
                            modelView.endDate
                                ? format(parseISO(modelView.endDate), "dd/MM/yyyy")
                                : "N/A"
                        }}
                        {{
                            viewTotalDate(
                                modelView.startDate,
                                modelView.endDate,
                                modelView.periodType
                            )
                        }}
                    </div>
                </div>
            </div>
            <div class="col-5">
                <div class="grid">
                    <div class="col-5 font-bold">{{ t('client.creator') }}</div>
                    <div class="col px-0">{{ modelView?.author?.fullName }}</div>
                </div>
                <div class="grid">
                    <div class="col-5 font-bold">{{ t('client.status') }}</div>
                    <div class="col px-0">
                        <Tag
                            :value="getStatusLabel(modelView.status).label"
                            :severity="getStatusLabel(modelView.status).severity"
                        ></Tag>
                    </div>
                </div>
                <div class="grid">
                    <div class="col-5 font-bold">{{ t('body.sampleRequest.importPlan.plan_by_label') }}</div>
                    <div class="col px-0">
                        {{
                            modelView?.periodType === "T"
                                ? t('body.sampleRequest.importPlan.plan_by_placeholder') /* fallback for 'Tuần' not present in en.json */
                                : modelView?.periodType === "M"
                                ? t('body.sampleRequest.importPlan.plan_by_placeholder')
                                : t('client.quarter')
                        }}
                    </div>
                </div>
                <div class="flex">
                    <div class="col-5 p-0 font-bold">{{ t('client.note') }}</div>
                    <div class="flex-grow-1">{{ modelView?.notes }}</div>
                </div>
            </div>
        </div>
        <DataTable :value="modelView?.saleForecastItems || []" showGridlines>
            <ColumnGroup type="header">
                <Row>
                    <Column header="#" :rowspan="2" style="min-width: 5rem" />
                    <Column :header="t('body.sampleRequest.importPlan.table_header_product_name')" style="min-width: 30rem" :rowspan="2">
                    </Column>
                    <Column
                        :header="t('body.sampleRequest.importPlan.table_header_unit')"
                        style="min-width: 10rem"
                        :rowspan="2"
                    ></Column>
                    <Column
                        v-for="(row, i) in modelView.saleForecastItems &&
                        modelView.saleForecastItems.length > 0
                            ? modelView.saleForecastItems[0].periods
                            : []"
                        :key="i"
                        :colspan="3"
                        class="bg-gray-100"
                    >
                        <template #header>
                            <div class="flex justify-content-center w-full">
                                <span>{{ row.periodName }}</span>
                            </div>
                        </template>
                    </Column>
                </Row>
                <Row>
                    <template
                        v-for="(row, i) in modelView.saleForecastItems &&
                        modelView.saleForecastItems.length > 0
                            ? modelView.saleForecastItems[0].periods
                            : []"
                        :key="i"
                    >
                        <Column
                            style="min-width: 7rem"
                            class="text-right"
                            :header="t('client.forecast')"
                        ></Column>
                        <Column
                            style="min-width: 7rem"
                            class="text-right"
                            :header="t('client.actual')"
                        ></Column>
                        <Column
                            style="min-width: 9rem"
                            class="text-right"
                            :header="t('client.completion_rate')"
                        ></Column>
                    </template>
                </Row>
            </ColumnGroup>
            <Column style="width: 3rem">
                <template #body="{ index }">
                    {{ index + 1 }}
                </template>
            </Column>
            <Column field="itemName" :header="t('body.sampleRequest.importPlan.table_header_product_name')"></Column>
            <Column field="ugpName" :header="t('body.sampleRequest.importPlan.table_header_unit')"></Column>
            <template
                v-for="(row, i) in modelView.saleForecastItems &&
                modelView.saleForecastItems.length > 0
                    ? modelView.saleForecastItems[0].periods
                    : []"
                :key="i"
            >
                <Column :header="t('client.forecast')" class="text-right">
                    <template #body="sp">
                        {{ formatNumber(sp.data.periods[i].quantity) }}
                    </template>
                </Column>
                <Column :header="t('client.actual')" class="text-right">
                    <template #body="sp">
                        {{ formatNumber(sp.data.periods[i].actualQuantity) }}
                    </template>
                </Column>
                <Column :header="t('client.completion_rate')" class="text-right">
                    <template #body="sp">
                        {{
                            sp.data.periods[i].quantity !== 0
                                ? (
                                      (sp.data.periods[i].actualQuantity /
                                          sp.data.periods[i].quantity) *
                                      100
                                  ).toFixed(2)
                                : "0.00"
                        }}%
                    </template>
                </Column>
            </template>
        </DataTable>
    </div>
    <loading v-if="isLoading"></loading>
</template>

<script setup>
import { ref, reactive, onBeforeMount } from "vue";
import { useRouter, useRoute } from "vue-router";
import API from "@/api/api-main";
import { format, parseISO } from "date-fns";
import { useAuthStore } from "@/Pinia/auth";
import { useGlobal } from "@/services/useGlobal";
import { useI18n } from "vue-i18n";

const { toast, FunctionGlobal } = useGlobal();
const router = useRouter();
const route = useRoute();
const { t} = useI18n();

const isLoading = ref(false);
const modelView = ref({});
const formatNumber = (value) => {
    return new Intl.NumberFormat("vi-VN").format(value);
};
const authStore = useAuthStore();
const viewTotalDate = (startDate, endDate, periodType) => {
    if (startDate && endDate) {
        const start = parseISO(startDate);
        const end = parseISO(endDate);

        if (periodType === "T") {
            const daysDifference = Math.ceil((end - start) / (1000 * 60 * 60 * 24));
            const weeks = Math.ceil(daysDifference / 7);
            return `(${weeks} ${t('body.sampleRequest.importPlan.plan_by_placeholder')})`;
        } else {
            const months =
                (end.getFullYear() - start.getFullYear()) * 12 +
                (end.getMonth() - start.getMonth());
            return `(${months} ${t('body.sampleRequest.importPlan.plan_by_placeholder')})`;
        }
    }
    return "";
};

const labels = {
    P: {
        severity: "warning",
        label: t('body.OrderApproval.pending'),
    },
    A: {
        severity: "success",
        label: t('body.OrderList.confirmed'),
    },
    R: {
        severity: "danger",
        label: t('body.OrderList.cancelled'),
    },
};

const getStatusLabel = (str) => {
    return labels[str] || { severity: "warning", label: t('body.OrderApproval.pending') };
};
const getDataById = async () => {
    isLoading.value = true;
    try {
        const res = await API.get(`sale-forecast/${route.params.id}`);
        modelView.value = res.data;
    } catch (err) {
        console.error(error);
    } finally {
        isLoading.value = false;
    }
};

const onConfirmStatus = async () => {
    try {
        await API.update(`sale-forecast/${route.params.id}/confirm`);
        toast.add({
            severity: "success",
            summary: t('body.systemSetting.success_label'),
            detail: "Cập nhật trạng thái kế hoạch nhập hàng thành công",
            life: 3000,
        });
        router.push("/order-planning/list");
        getDataById();
    } catch (error) {
        console.error(error);
    }
};
onBeforeMount(async () => {
    await getDataById();
});
</script>

<style>
.p-datatable-wrapper {
    padding-bottom: 10px !important;
}

.p-datatable-wrapper::-webkit-scrollbar {
    height: 10px !important;
}

.p-datatable-wrapper::-webkit-scrollbar-thumb:hover {
    background-color: #555;
    /* Color of the scrollbar thumb on hover */
}

.p-datatable-wrapper::-webkit-scrollbar-track {
    background: #f1f1f1;
    /* Color of the scrollbar track */
}
</style>
