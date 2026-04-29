<template>
    <div>
        <!-- Header -->
        <div class="flex flex-column pb-6 align-items-center md:flex-row md:justify-content-between">
            <h4 class="font-bold mb-0">{{ t('body.sampleRequest.warehouseFee.title') }}</h4>
            <div class="flex gap-2">
                <Button v-if="selectedFeeData.length !== 0" severity="help" :label="t('body.sampleRequest.warehouseFee.export_button')" icon="pi pi-file-export"
                    @click="onExportFile" />
                <Button v-if="selectedFeeData.length !== 0" :label="t('Login.buttons.send') || 'Gửi'" severity="info" icon="pi pi-send"
                    @click="onSendFeeData" />
                <Button :label="t('body.sampleRequest.warehouseFee.calculate')" @click="dialogVisible = true" icon="pi pi-plus" />
            </div>
        </div>
        <!-- Body -->
        <div class="grid card p-2">
            <div class="col-12 m-0">
                <!-- Table  -->
                <DataTable :value="feeDataList" dataKey="id" showGridlines paginator :alwaysShowPaginator="true"
                    :totalRecords="paging.total" :page="paging.skip" :rows="paging.limit" :rowsPerPageOptions="Array.from({ length: 10 }, (_, i) => (i + 1) * 10)
                        "
                    paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
                    @page="onPageChange" lazy v-model:filters="filters" filterDisplay="menu" :filterLocale="'vi'"
                    v-model:selection="selectedFeeData" selectionMode="multiple" @update:filters="onFilter" scrollable>
                    <!-- Khung tìm kiêm -->
                    <template #header>
                        <div class="flex flex-column md:flex-row md: justify-content-between">
                            <IconField iconPosition="left">
                                <InputText @input="debounceF()" :placeholder="t('body.sampleRequest.warehouseFee.searchPlaceholder')" class="w-full"
                                    v-model="inputSearch" />
                                <InputIcon>
                                    <i class="pi pi-search" />
                                </InputIcon>
                            </IconField>
                            <Button type="button" icon="pi pi-filter-slash" v-tooltip.bottom="t('body.OrderApproval.clear')"
                                severity="danger" outlined @click="onClearFilters" />
                        </div>
                        <div v-if="totalFiltersChipList.length !== 0" class="p-2 flex align-items-center gap-2">
                            <Chip v-for="(total, index) in totalFiltersChipList" :key="total.label + index"
                                :label="total.label" removable @remove="onTotalChipRemove(index)" />
                        </div>
                        <div v-if="totalWithVatFilters.length !== 0" class="p-2 flex align-items-center gap-2">
                            <Chip v-for="(totalWithVat, index) in totalWithVatFilters" :key="totalWithVat.label + index"
                                :label="totalWithVat.label" removable @remove="onTotalWithVatChipRemove(index)" />
                        </div>
                        <div v-if="statusFiltersChipList.length !== 0" class="p-2 flex align-items-center gap-2">
                            <div>{{ t('body.systemSetting.status_label') }}:</div>
                            <Chip v-for="(status, index) in statusFiltersChipList" :key="status + index" :label="status"
                                removable @remove="onStatusFilterChipRemove(index)" />
                        </div>
                    </template>
                    <!-- Cột chọn data -->
                    <Column selectionMode="multiple"></Column>
                    <!-- # -->
                    <Column header="#" :pt="{
                        headerTitle: { class: 'w-full text-center' },
                        bodycell: { class: 'text-center' },
                    }">
                        <template #body="sp">
                            <div>
                                {{ sp.index + 1 }}
                            </div>
                        </template>
                    </Column>
                    <!-- Mã chứng từ -->
                    <Column :header="t('body.sampleRequest.warehouseFee.documentCode')" field="feebyCustomerCode" :pt="{
                        headerTitle: { class: 'w-full text-left' },
                        bodycell: { class: 'text-left' },
                    }" />
                    <!-- Tên chứng từ -->
                    <Column :header="t('body.sampleRequest.warehouseFee.documentName')" field="feebyCustomerName" :pt="{
                        headerTitle: { class: 'w-full text-left' },
                        bodycell: { class: 'text-left' },
                    }" />
                    <!-- Mã khách hàng -->
                    <Column :header="t('body.sampleRequest.warehouseFee.customerCode')" field="cardCode" :pt="{
                        headerTitle: { class: 'w-full text-left' },
                        bodycell: { class: 'text-left' },
                    }" />
                    <!-- Tên khách hàng -->
                    <Column :header="t('body.sampleRequest.warehouseFee.customerName')" field="cardName" :pt="{
                        headerTitle: { class: 'w-full text-left' },
                        bodycell: { class: 'text-left' },
                    }" />
                    <!-- Phí lưu kho chưa VAT -->
                    <Column :header="t('body.sampleRequest.warehouseFee.feeWithoutVAT')" field="total" :pt="{
                        headerTitle: { class: 'w-full text-right' },
                        bodycell: { class: 'text-right' },
                    }" :showFilterOperator="false" :showFilterMatchModes="false">
                        <template #filter="{ filterModel }">
                            <div class="flex flex-column gap-2">
                                <Dropdown v-model="filterModel.matchMode" :options="matchModeList" optionLabel="name"
                                    optionValue="code" />
                                <InputNumber v-model="filterModel.value" />
                            </div>
                        </template>
                        <template #body="{ data }">
                            <span>{{
                                format.FormatCurrency(data.total - data.vat)
                                }}</span>
                        </template>
                    </Column>
                    <!-- Phí lưu kho có VAT(8%) -->
                    <Column :header="t('body.sampleRequest.warehouseFee.feeWithVAT')" field="totalWithVat" :pt="{
                        headerTitle: { class: 'w-full text-right' },
                        bodycell: { class: 'text-right' },
                    }" :showFilterOperator="false" :showFilterMatchModes="false">
                        <template #filter="{ filterModel }">
                            <div class="flex flex-column gap-2">
                                <Dropdown v-model="filterModel.matchMode" :options="matchModeList" optionLabel="name"
                                    optionValue="code" />
                                <InputNumber v-model="filterModel.value" />
                            </div>
                        </template>
                        <template #body="{ data }">
                            <span>{{ format.FormatCurrency(data.total) }}</span>
                        </template>
                    </Column>
                    <!-- Trạng thái gửi-->
                    <Column :header="t('body.sampleRequest.warehouseFee.sendStatus')" field="status" :pt="{
                        headerTitle: { class: 'w-full text-center' },
                        bodycell: { class: 'text-center' },
                    }" :showFilterOperator="false" :showFilterMatchModes="false" :showAddButton="false">
                        <template #filter="{ filterModel }">
                            <Dropdown v-model="filterModel.value" :options="sendStatusList" optionLabel="label"
                                optionValue="value" placeholder="Chọn Trạng thái" class="p-column-filter" showClear>
                                <template #option="slotProps">
                                    <Tag :value="slotProps.option.label" :severity="slotProps.option.severity" />
                                </template>
                            </Dropdown>
                        </template>
                        <template #body="{ data }">
                            <Tag :severity="data.sendStatusSeverity">{{
                                data.sendStatusMessage
                                }}</Tag>
                            <Tag :severity="data.sendStatusSeverity" v-if="data.sendedmDateAvailable">{{
                                data.sendedmDate }}</Tag>
                        </template>
                    </Column>
                    <!-- Ngày gửi hidden -->
                    <Column header="Ngày gửi" field="sendedmDate" hidden />
                    <!-- Trạng thái xác nhận -->
                    <Column :header="t('body.sampleRequest.warehouseFee.confirmStatus')" field="confirmStatus" :pt="{
                        headerTitle: { class: 'w-full text-center' },
                        bodycell: { class: 'text-center' },
                    }" :showFilterOperator="false" :showFilterMatchModes="false" :showAddButton="false">
                        <template #filter="{ filterModel }">
                            <Dropdown v-model="filterModel.value" :options="confirmStatusList" optionLabel="label"
                                optionValue="value" placeholder="Chọn Trạng thái" class="p-column-filter" showClear>
                                <template #option="slotProps">
                                    <Tag :value="slotProps.option.label" :severity="slotProps.option.severity" />
                                </template>
                            </Dropdown>
                        </template>
                        <template #body="{ data }">
                            <Tag :severity="data.confirmStatusSeverity">{{
                                data.confirmStatusMessage
                                }}</Tag>
                            <Tag :severity="data.confirmStatusSeverity" v-if="data.confirmedDateAvailable">{{
                                data.confirmedDate }}</Tag>
                        </template>
                    </Column>
                    <!-- Ngày xác nhận hidden -->
                    <Column header="Ngày xác nhận" field="confirmedDate" hidden />
                    <!-- Trạng thái thanh toán -->
                    <Column v-if="0" header="Trạng thái thanh toán" field="payStatus" :pt="{
                        headerTitle: { class: 'w-full text-center' },
                        bodycell: { class: 'text-center' },
                    }" :showFilterOperator="false" :showFilterMatchModes="false" :showAddButton="false">
                        <template #filter="{ filterModel }">
                            <Dropdown v-model="filterModel.value" :options="payStatusList" optionLabel="label"
                                optionValue="value" placeholder="Chọn Trạng thái" class="p-column-filter" showClear>
                                <template #option="slotProps">
                                    <Tag :value="slotProps.option.label" :severity="slotProps.option.severity" />
                                </template>
                            </Dropdown>
                        </template>
                        <template #body="{ data }">
                            <Tag :severity="data.payStatusSeverity">{{
                                data.payStatusMessage
                                }}</Tag>
                            <Tag :severity="data.payStatusSeverity" v-if="data.payedDateAvailable">{{ data.payedDate }}
                            </Tag>
                        </template>
                    </Column>
                    <!-- Ngày thanh toán hidden -->
                    <Column header="Ngày thanh toán" field="payedDate" hidden />
                    <!-- Hành động -->
                    <Column :header="t('body.sampleRequest.warehouseFee.actions')" :pt="{
                        headerTitle: { class: 'w-full text-center' },
                        bodycell: { class: 'text-center' },
                    }">
                        <template #body="{ data }">
                            <Button icon="pi pi-eye" severity="info" text @click="onDetailShow(data.id)" />
                        </template>
                    </Column>
                    <!-- Khi dataTable không có dữ liệu -->
                    <template #empty>
                        <div class="text-center py-5 my-5 text-500 font-italic">
                            {{ t('body.systemSetting.no_data_to_display') }}
                        </div>
                    </template>
                </DataTable>
            </div>
        </div>
        <!-- Loading -->
        <Loading v-if="loading"></Loading>
        <DetailWarehouseStorageFee v-model:visible="detailVisible" :feeData="feeData321"
            @onSendFeeDataDetail="handleSendFeeDataDetail" @onExportFileDetail="handleExportFileDetail" />
        <StorageFeeDateSelectingDlg v-model:visible="dialogVisible" v-if="dialogVisible"
            v-model:selectedYear="searchYear" :selectedQuarter="searchQuarter"
            @yearQuarterSaved="handleYearQuarterSaved" @yearQuarterCanceled="handleYearQuarterCancel" />
    </div>
</template>

<script setup>
// Import
import { ref, onMounted, watch, computed } from "vue";
import { useRoute, useRouter } from "vue-router";
import API from "@/api/api-main";
import format from "@/helpers/format.helper";
import DetailWarehouseStorageFee from "@/components/DetailWarehouseStorageFee.vue";
import StorageFeeDateSelectingDlg from "./components/StorageFeeDateSelectingDlg.vue";
import { useGlobal } from "@/services/useGlobal";
import ExcelJS from "exceljs";
import { useDebounce } from "@vueuse/core";
import { FilterMatchMode, FilterOperator } from "primevue/api";
import { debounce, keyBy } from "lodash";

// Router
const router = useRouter();
const route = useRoute();
import { useI18n } from "vue-i18n";
const { t } = useI18n();

// CONSTANT + ENUM
const STATUS = {
    SENT: "SD",
    NOT_SENT: "NS",
    CONFIRMED: "CF",
    NOT_CONFIRMED: "NC",
    PAID: "PD",
    NOT_PAID: "NP",
};
const STATUS_MESSAGE = {
    SENT: t('body.status.sent'),
    NOT_SENT: t('body.status.not_sent') ,
    CONFIRMED: t('body.status.confirmed') ,
    PAID: t('body.status.paid') ,
    NOT_PAID: t('body.status.not_paid') ,
};
const STATUS_SEVERITY = {
    SUCCESS: "success",
    WARNING: "warning",
};

const sendStatusList = [
    { label: STATUS_MESSAGE.SENT, value: STATUS.SENT, severity: STATUS_SEVERITY.SUCCESS },
    {
        label: STATUS_MESSAGE.NOT_SENT,
        value: STATUS.NOT_SENT,
        severity: STATUS_SEVERITY.WARNING,
    },
];
const confirmStatusList = [
    {
        label: STATUS_MESSAGE.CONFIRMED,
        value: STATUS.CONFIRMED,
        severity: STATUS_SEVERITY.SUCCESS,
    },
    {
        label: STATUS_MESSAGE.NOT_CONFIRMED,
        value: STATUS.NOT_CONFIRMED,
        severity: STATUS_SEVERITY.WARNING,
    },
];
const payStatusList = [
    { label: STATUS_MESSAGE.PAID, value: STATUS.PAID, severity: STATUS_SEVERITY.SUCCESS },
    {
        label: STATUS_MESSAGE.NOT_PAID,
        value: STATUS.NOT_PAID,
        severity: STATUS_SEVERITY.WARNING,
    },
];

const statusMap = {
    [STATUS.SENT]: STATUS_MESSAGE.SENT,
    [STATUS.NOT_SENT]: STATUS_MESSAGE.NOT_SENT,
    [STATUS.CONFIRMED]: STATUS_MESSAGE.CONFIRMED,
    [STATUS.NOT_CONFIRMED]: STATUS_MESSAGE.NOT_CONFIRMED,
    [STATUS.PAID]: STATUS_MESSAGE.PAID,
    [STATUS.NOT_PAID]: STATUS_MESSAGE.NOT_PAID,
};

const matchModeList = [
    {
        code: "gte",
        name: t('body.OrderList.greater_or_equal'),
    },
    {
        code: "lte",
        name: t('body.OrderList.less_or_equal'),
    },
];

const matchModeMap = {
    gte: { label: t('body.OrderList.greater_or_equal'), operator: ">=" },
    lte: { label: t('body.OrderList.less_or_equal'), operator: "<=" },
};

// Composables
const { toast, FunctionGlobal } = useGlobal();

// Status
const loading = ref(false);
// Data State
const feeDataList = ref([]);
const feeData321 = ref({});
const selectedFeeData = ref([]);
// Visible State
const dialogVisible = ref(false);
const detailVisible = ref(false);
// Paging State
const paging = ref({
    limit: route.query.page ? route.query.page : 10,
    skip: route.query.size ? route.query.size : 0,
    total: 0,
});
// Filter State
const searchYear = ref("");
const searchQuarter = ref("");
const inputSearch = ref("");
const debounceInputValue = useDebounce(inputSearch, 500);

const filters = ref({
    status: { value: null },
    confirmStatus: { value: null },
    payStatus: { value: null },
    total: {
        operator: FilterOperator.AND,
        constraints: [
            { value: null, matchMode: FilterMatchMode.GREATER_THAN_OR_EQUAL_TO },
        ],
    },
    totalWithVat: {
        operator: FilterOperator.AND,
        constraints: [
            { value: null, matchMode: FilterMatchMode.GREATER_THAN_OR_EQUAL_TO },
        ],
    },
});

// Computed States
const statusFiltersChipList = computed(() => {
    const resultArray = [];
    const allowedKeys = ["status", "confirmStatus", "payStatus"];
    for (const key in filters.value) {
        if (allowedKeys.includes(key) && filters.value[key]?.value !== null) {
            resultArray.push(statusMap[filters.value[key].value]);
        }
    }
    return resultArray;
});

const totalFiltersChipList = computed(() => {
    let resultArray = [];
    filters.value.total.constraints
        .filter((constraint) => constraint.value !== null) // Filter out constraints with null values
        .map((constraint) => {
            const matchModeText = matchModeMap[constraint.matchMode].label;
            resultArray.push({
                label: `Phí lưu kho chưa VAT ${matchModeText} ${format.FormatCurrency(
                    constraint.value
                )}`,
            });
        });
    return resultArray;
});

const totalWithVatFilters = computed(() => {
    let resultArray = [];
    filters.value.totalWithVat.constraints
        .filter((constraint) => constraint.value !== null) // Filter out constraints with null values
        .map((constraint) => {
            const matchModeText = matchModeMap[constraint.matchMode].label;
            resultArray.push({
                label: `Phí lưu kho có VAT ${matchModeText} ${format.FormatCurrency(
                    constraint.value
                )}`,
            });
        });
    return resultArray;
});

// API Functions
const fetchFeeDataList = async () => {
    loading.value = true;
    try {
        // Build câu query
        const queryParams = `${editSearchYear(searchYear.value)}/${searchQuarter.value}`;
        const pagingParams = `?skip=${paging.value.skip}&limit=${paging.value.limit}`;
        const inputSearchParams = `&search=${inputSearch.value}`;
        const filtersParams = `${editFilters(filters.value)}`;
        const queryString = `${queryParams}${pagingParams}${inputSearchParams}${filtersParams}`;
        const { data } = await API.get(`Fee/feePeriod/${queryString}`);
        if (data) {
            // Lấy data paging
            paging.value.skip = data.skip;
            paging.value.limit = data.limit;
            paging.value.total = data.total;
            // chỉnh sửa và Lấy data danh sách biên bản tính phí
            feeDataList.value = editFeeDataList(data.items);
        }
        router.push(pagingParams + inputSearchParams + filtersParams);
    } catch (error) {
        console.error(error);
    } finally {
        loading.value = false;
    }
};

const debounceF = debounce(fetchFeeDataList, 1000);

const fetchfeeDataById = async (feeDataId) => {
    loading.value = true;
    try {
        const queryParams = `${feeDataId}`;
        const res = await API.get(`Fee/feeByCus/${queryParams}`);
        if (res.data && res.data.items) {
            // Lấy feeData
            feeData321.value = editFeeData(res.data.items);
            // feeData321.value = res.data.items;
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
        const queryParams = `${status}`;
        const res = await API.update(`Fee/feeByCus/${queryParams}`, idList);
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

// Life Cycle Functions
onMounted(() => {
    if (route.query.id) {
        fetchfeeDataById(route.query.id);
        detailVisible.value = true;
    }
    searchYear.value = new Date();
    searchQuarter.value = "1";
    fetchFeeDataList();
});

// Watcher
// watch(debounceInputValue, () => {
//     paging.value.skip = 0;
//     paging.value.limit = 10;
//     fetchFeeDataList();
// });
watch(route, () => {
    if (route.query.id) {
        fetchfeeDataById(route.query.id);
        detailVisible.value = true;
    }
});
// Event Function
const onPageChange = (event) => {
    paging.value.skip = event.page;
    paging.value.limit = event.rows;
    fetchFeeDataList();
};

const onFilter = () => {
    fetchFeeDataList();
};

const onClearFilters = () => {
    inputSearch.value = "";
    filters.value = {
        status: { value: null },
        confirmStatus: { value: null },
        payStatus: { value: null },
        total: {
            operator: FilterOperator.AND,
            constraints: [
                { value: null, matchMode: FilterMatchMode.GREATER_THAN_OR_EQUAL_TO },
            ],
        },
        totalWithVat: {
            operator: FilterOperator.AND,
            constraints: [
                { value: null, matchMode: FilterMatchMode.GREATER_THAN_OR_EQUAL_TO },
            ],
        },
    };
    fetchFeeDataList();
};

const onTotalChipRemove = (index) => {
    filters.value.total.constraints[index].value = null;
    fetchFeeDataList();
};

const onTotalWithVatChipRemove = (index) => {
    filters.value.totalWithVat.constraints[index].value = null;
    fetchFeeDataList();
};

const onStatusFilterChipRemove = (index) => {
    const { status, confirmStatus, payStatus } = filters.value;
    switch (index) {
        case 0:
            status.value = null;
        case 1:
            confirmStatus.value = null;
            break;
        case 2:
            payStatus.value = null;
            break;
        default:
            break;
    }
    fetchFeeDataList();
};

const onDetailShow = (id) => {
    fetchfeeDataById(id);
    detailVisible.value = true;
};

const onExportFile = () => {
    try {
        selectedFeeData.value.forEach(async (data) => {
            await fetchfeeDataById(data.id);
            const exportSucces = exportFile(feeData321.value);
            if (!exportSucces) {
                FunctionGlobal.$notify("E", "Xuất file thất bại", toast);
                return;
            }
        });
        FunctionGlobal.$notify("S", "Xuất file thành công", toast);
        detailVisible.value = false;
    } catch (error) {
        FunctionGlobal.$notify("E", "Xuất file thất bại", toast);
    }
};

const onSendFeeData = async () => {
    const messages = {
        successMessage: "Gửi thành công",
        errorMessage: "Gửi thất bại",
        validateMessage:
            "Danh sách gửi không được bao gồm biên bản với trạng thái Đã gửi",
    };
    try {
        // Lấy danh sách Id
        let idList = [];
        for (const data of selectedFeeData.value) {
            // Nếu trong các data được select có data đã được gửi thì thông báo lỗi
            if (data.status === STATUS.SENT) {
                FunctionGlobal.$notify("E", messages.validateMessage, toast);
                return; // Dừng update
            }
            idList.push(data.id);
        }
        const sendSucces = await updateDataStatus(idList, STATUS.SENT);
        if (sendSucces) {
            FunctionGlobal.$notify("S", messages.successMessage, toast);
            detailVisible.value = false;
            fetchFeeDataList();
        }
    } catch (error) {
        FunctionGlobal.$notify("E", messages.errorMessage, toast);
    }
    //Gửi data
};

const handleYearQuarterSaved = (payload) => {
    const { selectedYear, selectedQuarter } = payload;
    resetCurrentState();
    searchYear.value = selectedYear;
    searchQuarter.value = selectedQuarter;
    fetchFeeDataList();
    dialogVisible.value = false;
};

const handleYearQuarterCancel = () => {
    dialogVisible.value = false;
};

const handleSendFeeDataDetail = async () => {
    const messages = {
        successMessage: "Gửi thành công",
        errorMessage: "Gửi thất bại",
    };
    try {
        let idList = [];
        idList.push(feeData321.value.id);
        const sendSuccess = await updateDataStatus(idList, STATUS.SENT);
        if (sendSuccess) {
            FunctionGlobal.$notify("S", messages.successMessage, toast);
            detailVisible.value = false;
            fetchFeeDataList();
        }
    } catch (error) {
        FunctionGlobal.$notify("E", messages.errorMessage, toast);
    }
};

const handleExportFileDetail = async () => {
    try {
        await fetchfeeDataById(feeData321.value.id);
        const exportSuccess = exportFile(feeData321.value);
        if (exportSuccess) {
            FunctionGlobal.$notify("S", "Xuất file thành công", toast);
            detailVisible.value = false;
        }
    } catch (error) {
        FunctionGlobal.$notify("E", "Xuất file thất bại", toast);
    }
};

// Function
const editFeeDataList = (data) => {
    return (
        data?.map((item) => ({
            // Các trường giữ nguyên
            ...item,

            // Các trường chỉnh sửa
            // total: format.FormatCurrency(item.total),
            //date
            sendedmDate: format.formatDate(item.sendedmDate),
            confirmedDate: format.formatDate(item.confirmedDate),
            payedDate: format.formatDate(item.payedDate),

            // Các trường thêm mới
            selected: false,
            totalWithVat: format.FormatCurrency(item.total + item.vat),
            // send
            sendedmDateAvailable: item.status === STATUS.SENT,
            sendStatusMessage:
                item.status === STATUS.SENT
                    ? STATUS_MESSAGE.SENT
                    : STATUS_MESSAGE.NOT_SENT,
            sendStatusSeverity:
                item.status === STATUS.SENT
                    ? STATUS_SEVERITY.SUCCESS
                    : STATUS_SEVERITY.WARNING,
            // confirm
            confirmedDateAvailable: item.confirmStatus === STATUS.CONFIRMED,
            confirmStatusMessage:
                item.confirmStatus === STATUS.CONFIRMED
                    ? STATUS_MESSAGE.CONFIRMED
                    : STATUS_MESSAGE.NOT_CONFIRMED,
            confirmStatusSeverity:
                item.confirmStatus === STATUS.CONFIRMED
                    ? STATUS_SEVERITY.SUCCESS
                    : STATUS_SEVERITY.WARNING,
            // pay
            payedDateAvailable: item.payStatus === STATUS.PAID,
            payStatusMessage:
                item.payStatus === STATUS.PAID
                    ? STATUS_MESSAGE.PAID
                    : STATUS_MESSAGE.NOT_PAID,
            payStatusSeverity:
                item.payStatus === STATUS.PAID
                    ? STATUS_SEVERITY.SUCCESS
                    : STATUS_SEVERITY.WARNING,
        })) || []
    );
};

const exportFile = async (data) => {
    loading.value = true;
    let success = false;
    try {
        const { feebyCustomerLine, feebyCustomerName, fromDate, toDate } = data;
        // Tên file Exp
        const workbookName = `${feebyCustomerName} (${fromDate} - ${toDate})`;
        // Create a workbook and a worksheet
        const workbook = new ExcelJS.Workbook();
        const worksheet = workbook.addWorksheet("Biên bản chi tiết");

        // Định nghĩa các cột + style
        worksheet.columns = [
            {
                header: "#",
                key: "lineId",
                width: 10,
                style: { alignment: { horizontal: "center", vertical: "middle" } },
            },
            {
                header: "Mã hàng",
                key: "itemCode",
                width: 22,
                style: { alignment: { horizontal: "left", vertical: "middle" } },
            },
            {
                header: "Tên hàng",
                key: "itemName",
                width: 20,
                style: {
                    alignment: { horizontal: "left", vertical: "middle", wrapText: true },
                },
            },
            {
                header: "Đơn vị tính",
                key: "ugpName",
                width: 15,
                style: {
                    alignment: { horizontal: "left", vertical: "middle", wrapText: true },
                },
            },
            {
                header: "Số lô nhập",
                key: "batchNum",
                width: 18,
                style: { alignment: { horizontal: "left", vertical: "middle" } },
            },
            {
                header: "Ngày nhập (1)",
                key: "receiptDate",
                width: 13,
                style: { alignment: { horizontal: "center", vertical: "middle" } },
            },
            {
                header: "Ngày tính phí gửi kho (2)",
                key: "feeStartDate",
                width: 25,
                style: { alignment: { horizontal: "center", vertical: "middle" } },
            },
            {
                header: "Ngày xuất (3)",
                key: "issueDate",
                width: 15,
                style: { alignment: { horizontal: "center", vertical: "middle" } },
            },
            {
                header: "SL xuất (4)",
                key: "quantity",
                width: 13,
                style: { alignment: { horizontal: "center", vertical: "middle" } },
            },
            {
                header: "Số ngày gửi kho (5)= (3)-(2)",
                key: "day",
                width: 25,
                style: { alignment: { horizontal: "center", vertical: "middle" } },
            },
            {
                header: "Số ngày gửi kho tính phí (6)",
                key: "dayToFee",
                width: 25,
                style: { alignment: { horizontal: "center", vertical: "middle" } },
            },
            {
                header: "Đơn giá (7)",
                key: "price",
                width: 11,
                style: { alignment: { horizontal: "right", vertical: "middle" } },
            },
            {
                header: "Biểu phí áp dụng (8)",
                key: "feeLevelName",
                width: 20,
                style: { alignment: { horizontal: "center", vertical: "middle" } },
            },
            {
                header: "Tổng tiền chưa VAT 8% (9) = (4)*(6)*(7)",
                key: "lineTotal",
                width: 35,
                style: { alignment: { horizontal: "right", vertical: "middle" } },
            },
        ];

        //Style header
        const headerRowIndex = 1;
        const headerRow = worksheet.getRow(headerRowIndex);
        headerRow.font = { bold: true };
        //Freeze Header
        worksheet.views = [{ state: "frozen", ySplit: headerRowIndex }];

        // row bắt đầu làm việc là row sau row header (1)
        let startRow = headerRowIndex + 1;
        // item trong data -> 1 hoặc nhiều row tùy vào số phần tử của các item.field là mảng
        feebyCustomerLine.forEach((item) => {
            let index = 0
            // Số dòng cần add = số phần tử của item.field là mảng
            const rowCount = item.dayToFee.length; // dayToFee là 1 field mảng
            // Các công việc cần xử lý cho mỗi row
            const rowData = {};
            for (let i = 0; i < rowCount; i++) {
                // Làm đầy giá trị cho row bằng item.field
                for (const key in item) {  
                    if (Array.isArray(item[key]) && key != "lineTotal") {
                        // Các item.field là mảng thì lấy giá trị của phần tử thêm vào từng row
                        rowData[key] = item[key][i];
                    } else if(key != "lineTotal") { 
                        // Các phần tử item.field không phải là mảng thì chỉ thêm cho row đầu tiên các row sau sẽ để trống
                        rowData[key] = i === 0 ? item[key] : null;
                    }
                } 
                rowData["lineTotal"] = item["lineTotal"][index]  - item["lineVAT"][index]
                worksheet.addRow(rowData);
                index++;
            }

            const endRow = startRow + rowCount - 1;
            //Nếu số dòng lớn hơn 1 thì sẽ cần merge cell
            if (rowCount > 1) {
                for (const key in item) {
                    if (!Array.isArray(item[key])) {
                        const startColumn = worksheet.getColumn(key).number;
                        const endColumn = startColumn;
                        worksheet.mergeCells(startRow, startColumn, endRow, endColumn);
                    }
                }
            }
            // Chuyển đến nhóm dòng tiếp theo
            startRow += rowCount; 
        });
        

        
        //Tính Tổng cộng cho cột SL xuất, Số ngày gửi kho tính phí, Tổng tiền chưa VAT 8%
        // Control của các cột SL xuất, Số ngày gửi kho tính phí, Tổng tiền chưa VAT 8%
        const quantityColumn = worksheet.getColumn("quantity");
        const dayToFeeColumn = worksheet.getColumn("dayToFee");
        const lineTotalColumn = worksheet.getColumn("lineTotal");
        // Index của các cột SL xuất, Số ngày gửi kho tính phí, Tổng tiền chưa VAT 8%
        const quantityColumnIndex = quantityColumn.number;
        const dayToFeeColumnIndex = dayToFeeColumn.number;
        const lineTotalColumnIndex = lineTotalColumn.number;
        // Index của dòng sau dòng cuối cùng của cột
        const lastRowIndex = worksheet.rowCount + 1;
        // Control của các cell cuối cùng của cột SL xuất, Số ngày gửi kho tính phí, Tổng tiền chưa VAT 8%
        const quantityColumnLastCell = worksheet.getCell(
            lastRowIndex,
            quantityColumnIndex
        );
        const dayToFeeColumnLastCell = worksheet.getCell(
            lastRowIndex,
            dayToFeeColumnIndex
        );
        const lineTotalColumnLastCell = worksheet.getCell(
            lastRowIndex,
            lineTotalColumnIndex
        );
        // Thêm công thức tính tổng vào các cell
        quantityColumnLastCell.value = {
            // Tính tông từ dòng dưới header đến dòng cuối cùng trên dòng tính tổng
            formula: `SUM(${quantityColumn.letter}${headerRowIndex + 1}:${quantityColumn.letter
                }${lastRowIndex - 1})`,
        };
        dayToFeeColumnLastCell.value = {
            // Tính tông từ dòng dưới header đến dòng cuối cùng trên dòng tính tổng
            formula: `SUM(${dayToFeeColumn.letter}${headerRowIndex + 1}:${dayToFeeColumn.letter
                }${lastRowIndex - 1})`,
        };
        lineTotalColumnLastCell.value = {
            // Tính tông từ dòng dưới header đến dòng cuối cùng trên dòng tính tổng
            formula: `SUM(${lineTotalColumn.letter}${headerRowIndex + 1}:${lineTotalColumn.letter
                }${lastRowIndex - 1})`,
        };

        // Style dòng tính tổng
        const lastRow = worksheet.getRow(lastRowIndex);
        lastRow.font = { bold: true };
        // Thêm label Tổng cộng cho dòng tính tổng
        const totalRowLabelCell = lastRow.getCell(1, lastRowIndex);
        totalRowLabelCell.value = "Tổng cộng";

        // Generate and download the Excel file
        const buffer = await workbook.xlsx.writeBuffer();
        const blob = new Blob([buffer], {
            type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        });
        const url = window.URL.createObjectURL(blob);

        const a = document.createElement("a");
        a.href = url;
        a.download = workbookName;
        a.click();
        window.URL.revokeObjectURL(url);
        success = true;
    } catch (error) {
        console.error(error);
    } finally {
        loading.value = false;
        return success;
    }
};

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

const editSearchYear = (year) => {
    if (!year) return 0; //searchYear trên màn hình nhập rỗng thì trả về null nếu chuyển Date sẽ cho ra năm 1970 NG -> chuyển thành 0 OK
    const date = new Date(year); //searchYear ở đây là 1 Date string literal -> chuyển về Date để lấy năm
    return isNaN(date.getFullYear()) ? 0 : date.getFullYear(); //giữ là số để so sánh, khi gán vào payload sẽ tự động chuyển thành string
};

const editFilters = (filters) => {
    const { status, confirmStatus, payStatus, total, totalWithVat } = filters;
    const statusFilters = status.value ? `status=${status.value}` : "";
    const confirmStatusFilters = confirmStatus.value
        ? `confirmStatus=${confirmStatus.value}`
        : "";
    const payStatusFilters = payStatus.value ? `payStatus=${payStatus.value}` : "";
    const totalFilters = total.constraints
        .filter((condition) => condition.value)
        .map(
            (condition) =>
                `total${matchModeMap[condition.matchMode].operator}${condition.value}`
        )
        .join(",");
    const totalWithVatFilters = totalWithVat.constraints
        .filter((condition) => condition.value)
        .map(
            (condition) =>
                `totalWithVat${matchModeMap[condition.matchMode].operator}${condition.value
                }`
        )
        .join(",");
    const filterParams = `&filter=${[
        statusFilters,
        confirmStatusFilters,
        payStatusFilters,
        totalFilters,
        totalWithVatFilters,
    ]
        .filter(Boolean)
        .join(",")}`;
    return filterParams;
};

const resetCurrentState = () => {
    // Selected Data State
    selectedFeeData.value = [];
    // Paging State
    paging.value = {
        limit: route.query.page ? route.query.page : 10,
        skip: route.query.size ? route.query.size : 0,
        total: 0,
    };
    // Filter State
    searchYear.value = "";
    searchQuarter.value = "";
    inputSearch.value = "";
    filters.value = {
        status: { value: null },
        confirmStatus: { value: null },
        payStatus: { value: null },
        total: {
            operator: FilterOperator.AND,
            constraints: [
                { value: null, matchMode: FilterMatchMode.GREATER_THAN_OR_EQUAL_TO },
            ],
        },
        totalWithVat: {
            operator: FilterOperator.AND,
            constraints: [
                { value: null, matchMode: FilterMatchMode.GREATER_THAN_OR_EQUAL_TO },
            ],
        },
    };
};
</script>
<style scoped></style>
