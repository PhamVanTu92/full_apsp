<template>
    <div>
        <div class="mb-3 flex justify-content-center">
            <h4 class="font-bold mb-0">Báo cáo thống kê hóa đơn</h4>
        </div>
        <div class="flex gap-2 align-items-center">
            <span class="font-semibold">Thời gian:</span>
            <div class="flex flex-column gap-1">
                <Calendar
                    v-model="timeFilter.fromDate"
                    placeholder="Từ"
                    class="w-10rem"
                    :invalid="!!errMsg.fromDate"
                ></Calendar>
                <small>{{ errMsg.fromDate }}</small>
            </div>
            <span>đến</span>
            <div class="flex flex-column gap-1">
                <Calendar
                    v-model="timeFilter.ToDate"
                    placeholder="Đến"
                    class="w-10rem"
                ></Calendar>
                <small>{{ errMsg.ToDate }}</small>
            </div>
            <Button label="Áp dụng" @click="onClickOk"/>
        </div>
        <hr />
        <DataTable
            class="card p-3 mb-5"
            :value="dataTable.value"
            showGridlines
            stripedRows
        >
            <Column header="#" class="w-3rem">
                <template #body="{ index }">
                    {{ index + 1 }}
                </template>
            </Column>
            <Column field="docDate" header="Ngày hóa đơn">
                <template #body="{ data, field }">
                    {{ format(data[field], "dd/MM/yyyy") }}
                </template>
            </Column>
            <Column field="invoiceCode" header="Số hóa đơn"> </Column>
            <Column
                field="docTotal"
                header="Giá trị hóa đơn (VND)"
                body-class="text-right"
            >
                <template #body="{ data, field }">
                    {{ fnum(data[field]) }}
                </template>
            </Column>
            <Column field="linkInvoice" header="Mã tra cứu HDDT"> </Column>
            <template #header>
                <div class="flex">
                    <IconField iconPosition="left">
                        <InputIcon class="pi pi-search"></InputIcon>
                        <InputText placeholder="Tìm kiếm..."></InputText>
                    </IconField>
                </div>
            </template>
            <template #empty>
                <div class="py-5 my-5 text-center">Không có dữ liệu để hiển thị</div>
            </template>
        </DataTable>
        <Loading v-if="loading"></Loading>
    </div>
</template>
<script setup lang="ts">
import apiMain from "@/api/api-main";
import { ref, reactive, computed, onMounted } from "vue";
import { format } from "date-fns";
import { Validator, ValidateOption } from "@/helpers/validate";

const timeNow = new Date();
const timeFilter = reactive({
    fromDate: new Date(`${timeNow.getFullYear()}/01/01`),
    ToDate: timeNow,
});
const loading = ref(false);
const dataTable = reactive({
    value: [],
});

const toQueryString = () => {
    const params = new URLSearchParams();
    if (timeFilter.fromDate) params.append("fromDate", timeFilter.fromDate.toISOString());
    if (timeFilter.ToDate) params.append("ToDate", timeFilter.ToDate.toISOString());
    return params.toString();
};

const fetchData = () => {
    apiMain
        .get(`Report/linkInvoice?${toQueryString()}`)
        .then((res) => {
            dataTable.value = res.data;
        })
        .catch((error) => {
            console.error(error);
        })
        .finally(() => {
            loading.value = false;
        });
};
const errMsg = ref<any>({});
const vldOtp: ValidateOption = {
    fromDate: {
        validators: {
            required: true,
            nullMessage: "Vui lòng chọn từ ngày",
        },
    },
    ToDate: {
        validators: {
            required: true,
            nullMessage: "Vui lòng chọn đến ngày",
        },
    },
};
const onClickOk = () => {
    const vResult = Validator(timeFilter, vldOtp);
    errMsg.value = {};
    if (!vResult.result) {
        errMsg.value = vResult.errors;
        return;
    }
    loading.value = true;
    fetchData();
};

const fnum = (num: number): string => {
    if (!num) return "0";
    else return Intl.NumberFormat().format(num);
};

onMounted(function () {});
</script>

<style scoped>
small {
    color: var(--red-500);
}
</style>
