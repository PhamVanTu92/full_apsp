<template>
    <div class="flex flex-column gap-4">
        <!-- Header -->
        <h4 class="font-bold">{{t('body.sampleRequest.warehouseFee.title')}}</h4>
        <!-- Body -->
        <div class="grid card p-2">
            <div class="col-12 m-0">
                <DataTable
                    :value="feeDataList"
                    showGridlines
                    paginator
                    :totalRecords="dataTable.total"
                    :rows="query.limit"
                    :rowsPerPageOptions="[10, 20, 30, 50]"
                    @page="onChangePage"
                >
                    <!-- v-model:filters="filters" filterDisplay="menu -->
                    <!-- Khung tìm kiêm -->
                    <template #header>
                        <div
                            class="flex flex-column md:flex-row md: justify-content-between"
                        >
                            <IconField iconPosition="left">
                                <InputText
                                    @input="onSearch"
                                    v-model="query.search"
                                    :placeholder="t('client.search_keyword')"
                                    class="w-full"
                                />
                                <InputIcon>
                                    <i class="pi pi-search" />
                                </InputIcon>
                            </IconField>
                            <Button
                                type="button"
                                icon="pi pi-filter-slash"
                                :label="t('client.clear_filter')"
                                outlined
                            />
                        </div>
                    </template>
                    <!-- # -->
                    <Column>
                        <template #header>
                            <div class="w-full text-center">#</div>
                        </template>
                        <template #body="{ index }">
                            <div class="w-full text-center">
                                {{ index + 1 }}
                            </div>
                        </template>
                    </Column>
                    <!-- Mã chứng từ -->
                    <Column>
                        <template #header>
                            <div class="w-full text-left">{{ t('client.certificate_code') }}</div>
                        </template>
                        <template #body="{ data }">
                            <div
                                class="w-full text-left text-primary font-bold hover:underline cursor-pointer"
                                @click="openDetailClick(data.id)"
                            >
                                {{ data.feebyCustomerCode }}
                            </div>
                        </template>
                        <template #filter="{ filterModel }">
                            <InputText
                                v-model="filterModel.value"
                                type="text"
                                class="p-column-filter"
                            />
                        </template>
                    </Column>
                    <!-- Tên chứng từ -->
                    <Column>
                        <template #header>
                            <div class="w-full text-left">{{ t('client.certificate_name') }}</div>
                        </template>
                        <template #body="{ data }">
                            <div class="w-full text-left">
                                {{ data.feebyCustomerName }}
                            </div>
                        </template>
                    </Column>
                    <!-- Phí lưu kho chưa VAT -->
                    <Column>
                        <template #header>
                            <div class="w-full text-right">{{ t('client.storage_fee_no_vat') }}</div>
                        </template>
                        <template #body="{ data }">
                            <div class="w-full text-right">
                                {{ format.FormatCurrency(data.total) }}
                            </div>
                        </template>
                    </Column>
                    <!-- Phí lưu kho có VAT -->
                    <Column>
                        <template #header>
                            <div class="w-full text-right">{{ t('client.storage_fee_with_vat') }}</div>
                        </template>
                        <template #body="{ data }">
                            <div class="w-full text-right">
                                {{ format.FormatCurrency(data.total + data.vat) }}
                            </div>
                        </template>
                    </Column>
                    <!-- Trạng thái -->
                    <Column>
                        <template #header>
                            <div class="w-full text-center">{{ t('client.status') }}</div>
                        </template>
                        <template #body="{ data }">
                            <div class="flex flex-column gap-2 w-full">
                                <Tag
                                    severity="warning"
                                    v-if="data.confirmStatus === STATUS.NOT_CONFIRMED"
                                    >{{ STATUS_MESSAGE.NOT_CONFIRMED }}</Tag
                                >
                                <Tag
                                    severity="success"
                                    v-if="data.confirmStatus === STATUS.CONFIRMED"
                                    >{{ STATUS_MESSAGE.CONFIRMED }}
                                </Tag>
                                <Tag
                                    severity="success"
                                    v-if="data.confirmStatus === STATUS.CONFIRMED"
                                    >{{ format.formatDate(data.confirmedDate) }}</Tag
                                >
                            </div>
                        </template>
                    </Column>
                    <!-- Ngày nhận -->
                    <Column>
                        <template #header>
                            <div class="w-full text-center">{{ t('client.date_received') }}</div>
                        </template>
                        <template #body="{ data }">
                            <div class="w-full text-center">
                                {{ format.formatDate(data.sendedmDate) }}
                            </div>
                        </template>
                    </Column>
                    <template #empty>
                        <div class="py-5 my-5 text-center">
                            {{ t('client.no_data') }}
                        </div>
                    </template>
                </DataTable>
            </div>
        </div>
    </div>
    <DetailWarehouseStorageFee
        v-model:visible="detailVisible"
        :feeData="feeData"
        :NPP="true"
        @onConfirmFeeDataDetail="handleConfirmFeeDataDetail"
    />
    <Loading v-if="loading"></Loading>
</template>

<script setup>
import { reactive, ref, watch } from "vue";
import format from "@/helpers/format.helper";
import DetailWarehouseStorageFee from "@/components/DetailWarehouseStorageFee.vue";
import { onBeforeMount } from "vue";
import API from "@/api/api-main";
import { useGlobal } from "@/services/useGlobal";
import { useRoute } from "vue-router";
import { useI18n } from 'vue-i18n';

const { t } = useI18n();
// import StepHistory from '../../../../../components/StepHistory.vue';

const route = useRoute();
// ENUM
const STATUS = {
    SENT: "SD",
    NOT_SENT: "NS",
    CONFIRMED: "CF",
    NOT_CONFIRMED: "NC",
};
const STATUS_MESSAGE = {
    SENT: "Đã gửi",
    NOT_SENT: "Chưa gửi",
    CONFIRMED: "Đã xác nhận",
    NOT_CONFIRMED: "Chưa xác nhận",
};

// Composables
const { toast, FunctionGlobal } = useGlobal();

// Internal States
const loading = ref(false);
const detailVisible = ref(false);
const feeData = ref("");
const feeDataList = ref([]);
const dataTable = ref({
    limit: 10,
    skip: 0,
    total: 0,
});

const openDetailClick = async (id) => {
    await fetchfeeDataById(id);
    detailVisible.value = true;
};

// Life Cycle Function
onBeforeMount(() => {
    fetchFeeDatas(query.toQueryString());
    if (route.query.id) {
        openDetailClick(route.query.id);
    }
});
watch(route, (newVal, oldVal) => {
    if (route.query.id) {
        openDetailClick(route.query.id);
    }
});
// API Functions

const query = reactive({
    skip: 0,
    limit: 10,
    search: null,
    toQueryString: () => {
        let result = [];
        Object.keys(query)
            .filter((key) => key != "toQueryString")
            .forEach((key) => {
                if (query[key] != null) {
                    result.push(`${key}=${query[key]}`);
                }
            });
        return result.length > 0 ? "?" + result.join("&") : "";
    },
});

const onChangePage = (event) => {
    query.skip = event.page;
    query.limit = event.rows;
    fetchFeeDatas(query.toQueryString());
};

import debounce from "lodash/debounce";

const onSearch = debounce(() => {
    query.skip = 0;
    fetchFeeDatas(query.toQueryString());
}, 500);

const fetchFeeDatas = async (querryParams) => {
    loading.value = true;
    try {
        // const queryParams = `skip=${dataTable.value.skip}&limit=${dataTable.value.limit}`;
        const res = await API.get(`Fee/feeByCus/pagination${querryParams}`);
        if (res.data) {
            // Lấy các thông tin config cho DataTable
            Object.assign(dataTable.value, {
                limit: res.data.limit,
                skip: res.data.skip,
                total: res.data.total,
            });
            // // chỉ lấy các data đẫ được gửi từ SG petrol
            // let dataList = res.data.items;
            // dataList = dataList.filter((data) => data.sendedmDate !== null);
            // // sắp xếp lại dataList theo ngày nhận
            // dataList = dataList.sort(
            //     (dataA, dataB) =>
            //         new Date(dataB.sendedmDate) - new Date(dataA.sendedmDate)
            // );
            // chốt data hiển thị
            feeDataList.value = res.data.items;
        }
    } catch (error) {
        console.error(error);
    } finally {
        loading.value = false;
    }
};

// API Functions
const fetchfeeDataById = async (feeDataId) => {
    loading.value = true;
    try {
        const querryParams = `${feeDataId}`;
        const res = await API.get(`Fee/feeByCus/${querryParams}`);
        if (res.data && res.data.items) {
            // Lấy feeData
            feeData.value = editFeeData(res.data.items);
        }
    } catch (error) {
        console.error(error);
    } finally {
        loading.value = false;
    }
};

const updateDataStatus = async (idList, status) => {
    // Thông báo cho các trường hợp
    let success = false;
    loading.value = true;
    try {
        // update status cho các data được chọn
        const querryParams = `${status}`;
        const res = await API.update(`Fee/feeByCus/${querryParams}`, idList);
        if (res.status === 200) {
            success = true;
        }
    } catch (error) {
        console.error(error);
    } finally {
        loading.value = false;
        return success;
    }
};

// Event Functions
const handleConfirmFeeDataDetail = async () => {
    try {
        if (feeData.value.confirmStatus === STATUS.CONFIRMED) {
            FunctionGlobal.$notify(
                "E",
                "Không thể xác nhận các chứng từ có trạng thái Đã xác nhận",
                toast
            );
            return; // Dừng xác nhận
        }
        const idList = [];
        idList.push(feeData.value.id);
        const confirmSuccess = await updateDataStatus(idList, STATUS.CONFIRMED);
        if (confirmSuccess) {
            await fetchFeeDatas(query.toQueryString());
            detailVisible.value = false;
            FunctionGlobal.$notify("S", "Xác nhận thành công!", toast);
        }
    } catch (error) {
        FunctionGlobal.$notify("E", "Xác nhận thất bại", toast);
    }
};

// Function

const groupObjectsByFields = (array, groupingFields) => {
    const grouped = {};

    // Group objects by the specified fields
    array.forEach((obj) => {
        // Create a key based on the grouping fields
        const key = groupingFields.map((field) => obj[field]).join("-");
        if (!grouped[key]) {
            // Initialize the group with the grouping fields as primitive values
            grouped[key] = {};
            groupingFields.forEach((field) => {
                grouped[key][field] = obj[field];
            });

            // Initialize other fields as arrays
            Object.keys(obj).forEach((field) => {
                if (!groupingFields.includes(field)) {
                    grouped[key][field] = [obj[field]];
                }
            });
        } else {
            // Append values to arrays for other fields
            Object.keys(obj).forEach((field) => {
                if (!groupingFields.includes(field)) {
                    grouped[key][field].push(obj[field]);
                }
            });
        }
    });

    // Convert grouped objects to an array and add `id` field
    return Object.values(grouped).map((group, index) => {
        return { lineId: index + 1, ...group };
    });
};

const getQuarterDates = (year, quarter) => {
    if (quarter < 1 || quarter > 4) {
        throw new Error("Quarter must be between 1 and 4.");
    }

    // Define the start and end months for each quarter
    const startMonth = (quarter - 1) * 3;
    const endMonth = startMonth + 2;

    // Create date objects for the first and last day of the quarter
    const quarterFirstDay = new Date(year, startMonth, 1);
    const quarterLastDay = new Date(year, endMonth + 1, 0); // Day 0 gives the last day of the previous month

    return {
        quarterFirstDay, // Date object for the first day
        quarterLastDay, // Date object for the last day
    };
};

const editFeeData = (data) => {
    // Lấy ngày Từ Đến
    const { quarterFirstDay, quarterLastDay } = getQuarterDates(data.year, data.period);
    return {
        // Các trường giữ nguyên
        ...data,
        // Các trường chỉnh sửa
        feebyCustomerLine: editFeeDataDetail(data.feebyCustomerLine),
        // Các trường thêm mới
        fromDate: format.formatDate(quarterFirstDay),
        toDate: format.formatDate(quarterLastDay),
    };
};

const editFeeDataDetail = (dataDetail) => {
    dataDetail = dataDetail.map((item) => {
        const updatedItem = {
            // Các trường giữ nguyên
            ...item,
            // Các trường chỉnh sửa
            receiptDate: format.formatDate(item.receiptDate),
            issueDate: format.formatDate(item.issueDate),
            // Các trường thêm mới
            feeStartDate: format.formatDate(item.receiptDate),
        };
        // Trường bị xóa vì không cần thiết và ảnh hưởng đến việc nhóm
        delete updatedItem.fatherId;
        delete updatedItem.ugpCode;
        return updatedItem;
    });
    const groupingFields = [
        "itemCode",
        "itemName",
        "ugpName",
        "batchNum",
        "receiptDate",
        "feeStartDate",
        "issueDate",
        "quantity",
        "day",
    ];
    return groupObjectsByFields(dataDetail, groupingFields);
};
</script>
<style scoped></style>
