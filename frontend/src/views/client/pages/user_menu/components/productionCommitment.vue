<template>
    <div>
        <div class="flex gap-3">
            <div class="col-9">
                <h4 class="font-bold m-0">{{ t('client.production_commitment_list') }}</h4>
            </div>
        </div>
        <div class="card flex flex-column gap-4 mt-3 p-2">
            <DataTable
                :value="dataCommited"
                v-model:filters="filterStore.filters"
                showGridlines
                scrollable
                tableStyle="min-width: 50rem; max-width: 100%"
                header="surface-200"
                lazy
                stripedRows
                paginator
                :rows="dataTable?.limit"
                :page="dataTable?.skip"
                :totalRecords="dataTable?.total"
                @page="onPageChange($event)"
                :rowsPerPageOptions="[10, 20, 30]"
                paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
                :currentPageReportTemplate="`${t('body.productManagement.display')} {first} - {last} ${t('body.productManagement.total_of')} {totalRecords} ${t('body.systemSetting.orders')}`"
                filterDisplay="menu"
                :filterLocale="'vi'"
                @filter="onFilter"
            >
                <template #empty>
                    <div class="py-5 my-5 text-center">
                        {{ t('client.no_matching_request') }}
                    </div>
                </template>
                <template #header>
                    <div class="flex justify-content-between">
                        <Button
                            type="button"
                            icon="pi pi-filter-slash"
                            :label="t('client.clear_filter')"
                            outlined
                            @click="clearFilter()"
                        />
                        <IconField iconPosition="left">
                            <InputText
                                :placeholder="t('client.enter_search_keywords')"
                                v-model="filterStore.filters['global'].value"
                                @keyup.enter="onFilter()"
                            />
                            <InputIcon>
                                <i class="pi pi-search" @click="onFilter()" />
                            </InputIcon>
                        </IconField>
                    </div>
                </template>
                <Column header="#" class="w-3rem">
                    <template #body="{ index }">
                        <span>{{ index + 1 }}</span>
                    </template>
                </Column>
                <Column field="committedCode" :header="t('client.commitment_code_header')">
                    <template #body="sp">
                        <div
                            @click="OpenDetail(sp.data.id)"
                            class="text-primary cursor-pointer font-semibold hover:underline"
                        >
                            <span>{{ sp.data.committedCode }}</span>
                        </div>
                    </template>
                </Column>
                <Column field="committedName" :header="t('client.commitment_name_header')"></Column>
                <Column field="committedYear" :header="t('client.commitment_period_header')" class="w-8rem">
                    <template #body="sp">{{
                        new Date(sp.data.committedYear).getFullYear()
                    }}</template>
                </Column>
                <Column
                    field="docStatus"
                    :header="t('client.status_header')"
                    :showFilterMatchModes="false"
                    class="w-10rem"
                >
                    <template #body="sp">
                        <Tag
                            :value="setStatus(sp.data.docStatus).label"
                            :severity="setStatus(sp.data.docStatus).type"
                        />
                    </template>
                    <template #filter="{ filterModel }">
                        <MultiSelect
                            v-model="filterModel.value"
                            :options="[ 
                                { name: t('body.status.pending'), code: 'P' },
                                { name:  t('body.status.draft'), code: 'D' },
                                { name: t('body.status.DH'), code: 'R' },
                                { name: t('body.status.DXN'), code: 'A' },
                            ]"
                            optionLabel="name"
                            optionValue="code"
                            :placeholder="t('body.promotion.select_status')"
                            class="p-column-filter"
                            showClear
                        >
                        </MultiSelect>
                    </template>
                </Column>
                <Column v-if="0" field="id" :header="t('client.actions')" style="width: 5rem">
                    <template #body="slotProp">
                        <div class="flex gap-2">
                            <Button
                                :icon="
                                    slotProp.data.docStatus == 'A'
                                        ? 'pi pi-eye'
                                        : 'pi pi-pencil'
                                "
                                @click="OpenDetail(slotProp.data.id)"
                                text
                            />
                        </div>
                    </template>
                </Column>
            </DataTable>
        </div>
    </div>
    <Dialog
        v-model:visible="confirmModal"
        modal
        :draggable="false"
        @hide="onModalHide"
        :header="PayloadData.id ? t('body.sampleRequest.commitment.page_title') : t('body.sampleRequest.commitment.title')"
        :style="{ width: '85%' }"
        maximizable
    >
        <div class="flex flex-column gap-4">
            <div class="card m-0 grid">
                <div class="col-6 flex flex-column gap-2">
                    <div class="flex">
                        <div class="w-20rem">
                            <strong>{{ t('client.commitment_code') }}</strong>
                        </div>
                        <div class="w-full">
                            {{ PayloadData.committedCode }}
                        </div>
                    </div>
                    <div class="flex">
                        <div class="w-20rem">
                            <strong>{{ t('body.sampleRequest.commitment.name_label') }}</strong>
                        </div>
                        <div class="w-full">
                            {{ PayloadData.committedName }}
                        </div>
                    </div>
                    <div class="flex">
                        <div class="w-20rem"><strong>{{ t('body.sampleRequest.commitment.notes_label') }}</strong></div>
                        <div class="w-full">
                            {{ PayloadData.committedDescription }}
                        </div>
                    </div>
                </div>
                <div class="col-6 flex flex-column gap-4">
                    <div class="flex justify-content-between">
                        <div class="w-20rem">
                            <strong>{{ t('body.sampleRequest.commitment.applicable_object_label') }}</strong>
                        </div>
                        <div class="flex flex-column gap-2 w-full">
                            <div class="flex align-items-center gap-2">
                                {{ PayloadData.cardName }}
                            </div>
                        </div>
                    </div>
                    <div class="flex justify-content-between">
                        <label class="w-20rem">
                            <strong>{{ t('body.sampleRequest.commitment.time_label') }}</strong>
                        </label>
                        <div class="flex gap-3 w-full">
                            <div class="flex flex-column w-full gap-2">
                                {{ new Date(PayloadData.committedYear).getFullYear() }}
                            </div>
                        </div>
                    </div>
                    <div class="flex justify-content-between">
                        <label class="w-20rem">
                            <strong>{{ t('body.sampleRequest.commitment.status_label') }}</strong>
                        </label>
                        <div class="flex gap-3 w-full">
                            <div class="flex flex-column gap-2">
                                <Tag
                                    :value="setStatus(PayloadData.docStatus).label"
                                    :severity="setStatus(PayloadData.docStatus).type"
                                />
                            </div>
                        </div>
                    </div>
                    <div
                        v-if="PayloadData.docStatus == 'R'"
                        class="flex justify-content-between"
                    >
                        <label class="w-20rem">
                            <strong>{{ t('client.reason') || 'Lý do' }}</strong>
                        </label>
                        <div class="w-full">
                            {{ PayloadData.rejectReason }}
                        </div>
                    </div>
                </div>
            </div>
            <div
                class="card flex flex-column gap-3"
                v-for="item of PayloadData.committedLine"
                :key="item"
            >
                <div class="flex gap-2 align-items-center">
                    <strong class="text-xl">{{ t('body.sampleRequest.commitment.content_section_title') }}</strong>
                </div>
                <div class="grid">
                    <div class="col-6 gap-2">
                        <div class="flex gap-3">
                            <label class=""><strong> {{ t('body.sampleRequest.commitment.application_method_label') }}</strong> </label>
                            <div>
                                <Tag
                                    :value="
                                        dataCommittedType.find(
                                            (el) => el.code == item.committedType
                                        )?.name
                                    "
                                    class="px-3"
                                />
                            </div>
                        </div>
                    </div>
                    <div class="col-12 p-2">
                        <DataTable
                            :value="item?.committedLineSub || []"
                            tableStyle="min-width: 50rem"
                            resizableColumns
                            columnResizeMode="fit"
                            v-model:expandedRows="expandedRows"
                        >
                            <Column expander style="width: 5rem" />
                            <Column
                                v-for="col of DataStruct.structTable.filter(
                                    (el) =>
                                        !el.committedType ||
                                        el.committedType?.includes(item.committedType)
                                )"
                                :key="col.field"
                                :field="col.field"
                                :header="col.header"
                            >
                                <template #body="{ data }">
                                    <span v-if="col.typeValue == 'Dropdown'">
                                        {{ data[col.field]?.name }}
                                    </span>
                                    <div
                                        v-if="col.typeValue == 'MultiSelect'"
                                        class="flex gap-2"
                                    >
                                        <Tag
                                            v-for="(i, idx) in data[col.field]"
                                            :key="idx"
                                            severity="info"
                                            >{{ i?.name }}
                                        </Tag>
                                    </div>
                                    <span v-if="col.typeValue == 'InputNumber'">{{
                                        Intl.NumberFormat().format(data[col.field])
                                    }}</span>
                                    <div
                                        class="flex align-items-center gap-2"
                                        v-if="col.typeValue == 'Mixed'"
                                    >
                                        <span>{{ data[col.field] || 0 }}%</span>
                                        <div class="flex gap-2">
                                            <Checkbox
                                                disabled
                                                v-model="data[col.sub_field]"
                                                binary
                                            ></Checkbox>
                                            <span>{{ t('client.exceeded_production') || 'Quy ra hàng' }}</span>
                                        </div>
                                    </div>
                                </template>
                            </Column>
                            <template #expansion="sp">
                                <DataTable
                                    :value="sp.data.committedLineSubSub"
                                    tableStyle="max-width: 50rem"
                                    resizableColumns
                                    columnResizeMode="fit"
                                >
                                    <Column field="code" header="#">
                                        <template #body="{ index }">
                                            <span>{{ index + 1 }}</span>
                                        </template>
                                    </Column>
                                    <Column :header="t('body.sampleRequest.commitment.table_header_total_volume')" field="name">
                                        <template #body="{ data }">
                                            <span>{{
                                                Intl.NumberFormat().format(data.outPut)
                                            }}</span>
                                        </template>
                                    </Column>
                                    <Column field="category" :header="t('body.sampleRequest.commitment.discount_quarterly')">
                                        <template #body="{ data }">
                                            <div class="flex gap-2 align-items-center">
                                                <span> {{ data.discount }}%</span>
                                                <div class="flex gap-2">
                                                    <Checkbox
                                                        binary
                                                        v-model="data.isConvert"
                                                        disabled
                                                    ></Checkbox>
                                                    <span>{{ t('client.exceeded_production') || 'Quy ra hàng' }}</span>
                                                </div>
                                            </div>
                                        </template>
                                    </Column>
                                </DataTable>
                            </template>
                        </DataTable>
                    </div>
                </div>
            </div>
        </div>
        <template #footer>
            <div class="flex gap-2">
                <Button
                    :label="t('body.OrderList.close')"
                    icon="pi pi-times"
                    severity="secondary"
                    @click="confirmModal = false"
                />
                <Button
                    v-if="['P'].includes(PayloadData.docStatus)"
                    :label="t('body.OrderApproval.rejected')"
                    icon="pi pi-ban"
                    severity="danger"
                    @click="onClickShowConfirm"
                />
                <Button
                    v-if="['P'].includes(PayloadData.docStatus)"
                    :label="t('body.home.confirm_button')"
                    @click="saveCommited('confirm')"
                    icon="pi pi-check"
                />
            </div>
        </template>
    </Dialog>
    <Dialog
        v-model:visible="visibleConfirm"
        class="w-3"
        :closable="false"
        :header="t('client.confirm') || 'Xác nhận từ chối'"
        modal
    >
        <div class="flex flex-column w-full gap-1">
            <Textarea
                v-model="rejectReason"
                :invalid="reasonError"
                class="w-full"
                rows="3"
                :placeholder="t('body.sampleRequest.commitment.reject_placeholder') || 'Nhập lý do từ chối'"
            ></Textarea>
            <small class="text-red-500">{{ reasonError }}</small>
        </div>
        <template #footer>
            <Button @click="visibleConfirm = false" :label="t('body.OrderList.close')" severity="secondary" />
            <Button
                :loading="loadingReject"
                @click="onClickConfirm()"
                :label="t('body.OrderApproval.rejected')"
                severity="danger"
            />
        </template>
    </Dialog>
    <Loading v-if="loadding"></Loading>
</template>

<script setup>
import { ref, watch, onBeforeMount, onMounted, watchEffect, reactive } from "vue";
import { useGlobal } from "@/services/useGlobal";
import API from "@/api/api-main";
import { useRouter, useRoute } from "vue-router";
import { FilterStore } from "@/Pinia/Filter/FilterStoreCommitted";
import { inject } from "vue";
import ExcelJS from "exceljs";
import { map } from "lodash";
import { useI18n } from "vue-i18n";
const { t } = useI18n();

// Biến function
const conditionHandler = inject("conditionHandler");
const router = useRouter();
const route = useRoute();
const { toast, FunctionGlobal } = useGlobal();
// END

// Biến trạng thái
const loadding = ref(false);
const visibleDelete = ref(false);
const confirmModal = ref(false);
const submited = ref(false);
// END

// Biến lưu trữ data
const expandedRows = ref();
const Hierarchy = ref();
// const dataCustomer = ref();
const dataCommited = ref();
const dataTable = ref({
    limit: route.query.page ? route.query.page : 10,
    skip: route.query.size ? route.query.size : 0,
});
const dataCommittedType = ref();
const DataDelete = ref();
const committedLine = ref();
const PayloadData = ref();
const Industry = ref();
const IndustryClone = ref();
const filterStore = FilterStore() || {};
const DataStruct = ref({
    structTable: [
        {
            field: "industry",
            header: t('body.sampleRequest.commitment.table_header_category'),
            require: true,
            typeValue: "Dropdown",
            placeholder: t('client.enter_search_keywords'),
        },
        {
            field: "brand",
            header: t('body.sampleRequest.commitment.table_header_brand'),
            require: true,
            typeValue: "MultiSelect",
            placeholder: t('client.enter_search_keywords'),
        },
        {
            field: "itemTypes",
            header: t('body.sampleRequest.commitment.table_header_product_type'),
            require: true,
            typeValue: "MultiSelect",
            placeholder: t('client.enter_search_keywords'),
        },
        {
            field: "quarter1",
            header: t('client.quarter_1'),
            typeValue: "InputNumber",
            placeholder: t('client.quarter_1'),
            committedType: ["Q"],
        },
        {
            field: "quarter2",
            header: t('client.quarter_2'),
            typeValue: "InputNumber",
            placeholder: t('client.quarter_2'),
            committedType: ["Q"],
        },
        {
            field: "quarter3",
            header: t('client.quarter_3'),
            typeValue: "InputNumber",
            placeholder: t('client.quarter_3'),
            committedType: ["Q"],
        },
        {
            field: "quarter4",
            header: t('client.quarter_4'),
            typeValue: "InputNumber",
            placeholder: t('client.quarter_4'),
            committedType: ["Q"],
        },
        {
            field: "package",
            header: t('body.sampleRequest.commitment.table_header_total_volume'),
            typeValue: "InputNumber",
            placeholder: t('body.sampleRequest.commitment.table_header_total_volume'),
            committedType: ["P"],
        },
        {
            field: "month1",
            header: "Tháng 1",
            typeValue: "InputNumber",
            placeholder: "Nhập tháng 1",
            committedType: ["Y"],
        },
        {
            field: "month2",
            header: "Tháng 2",
            typeValue: "InputNumber",
            placeholder: "Nhập tháng 2",
            committedType: ["Y"],
        },
        {
            field: "month3",
            header: "Tháng 3",
            typeValue: "InputNumber",
            placeholder: "Nhập tháng 3",
            committedType: ["Y"],
        },
        {
            field: "month4",
            header: "Tháng 4",
            typeValue: "InputNumber",
            placeholder: "Nhập tháng 4",
            committedType: ["Y"],
        },
        {
            field: "month5",
            header: "Tháng 5",
            typeValue: "InputNumber",
            placeholder: "Nhập tháng 5",
            committedType: ["Y"],
        },
        {
            field: "month6",
            header: "Tháng 6",
            typeValue: "InputNumber",
            placeholder: "Nhập tháng 6",
            committedType: ["Y"],
        },
        {
            field: "month7",
            header: "Tháng 7",
            typeValue: "InputNumber",
            placeholder: "Nhập tháng 7",
            committedType: ["Y"],
        },
        {
            field: "month8",
            header: "Tháng 8",
            typeValue: "InputNumber",
            placeholder: "Nhập tháng 8",
            committedType: ["Y"],
        },
        {
            field: "month9",
            header: "Tháng 9",
            typeValue: "InputNumber",
            placeholder: "Nhập tháng 9",
            committedType: ["Y"],
        },
        {
            field: "month10",
            header: "Tháng 10",
            typeValue: "InputNumber",
            placeholder: "Nhập tháng 10",
            committedType: ["Y"],
        },
        {
            field: "month11",
            header: "Tháng 11",
            typeValue: "InputNumber",
            placeholder: "Nhập tháng 11",
            committedType: ["Y"],
        },
        {
            field: "month12",
            header: "Tháng 12",
            typeValue: "InputNumber",
            placeholder: "Nhập tháng 12",
            committedType: ["Y"],
        },
        {
            field: "total",
            header: t('body.sampleRequest.commitment.table_header_total_volume'),
            typeValue: "InputNumber",
            disabled: true,
            placeholder: t('body.sampleRequest.commitment.table_header_total_volume'),
            committedType: ["Y", "Q"],
        },
        {
            field: "discountMonth",
            sub_field: "isCvMonth",
            header: t('body.sampleRequest.commitment.discount_quarterly'),
            typeValue: "Mixed",
            placeholder: t('body.sampleRequest.commitment.discount_quarterly'),
            committedType: ["Y"],
        },
        {
            field: "discount",
            sub_field: "isConvert",
            header: t('body.sampleRequest.commitment.discount_quarterly'),
            typeValue: "Mixed",
            placeholder: t('body.sampleRequest.commitment.discount_quarterly'),
            committedType: ["Q"],
        },
        {
            field: "threeMonthDiscount",
            sub_field: "isCvThreeMonth",
            header: t('body.sampleRequest.commitment.discount_9_months'),
            typeValue: "Mixed",
            placeholder: t('body.sampleRequest.commitment.discount_9_months'),
            committedType: ["Y"],
        },
        {
            field: "sixMonthDiscount",
            sub_field: "isCvSixMonth",
            header: t('body.sampleRequest.commitment.discount_6_months'),
            typeValue: "Mixed",
            placeholder: t('body.sampleRequest.commitment.discount_6_months'),
            committedType: ["Q", "Y"],
        },
        {
            field: "nineMonthDiscount",
            sub_field: "isCvNineMonth",
            header: t('body.sampleRequest.commitment.discount_9_months'),
            typeValue: "Mixed",
            placeholder: t('body.sampleRequest.commitment.discount_9_months'),
            committedType: ["Q"],
        },
        {
            field: "discountYear",
            sub_field: "isCvYear",
            header: t('body.sampleRequest.commitment.discount_yearly'),
            typeValue: "Mixed",
            placeholder: t('body.sampleRequest.commitment.discount_yearly'),
            committedType: ["Q", "Y"],
        },
        {
            field: "discount",
            sub_field: "isConvert",
            header: t('body.sampleRequest.commitment.discount_quarterly'),
            typeValue: "Mixed",
            placeholder: t('body.sampleRequest.commitment.discount_quarterly'),
            committedType: ["P"],
        },
    ],
    structTableChill: [
        {
            field: "outPut",
            header: "Sản lượng vượt",
            typeValue: "InputNumber",
            placeholder: "Nhập sản lượng vượt",
        },
        {
            field: "discount",
            sub_field: "isConvert",
            header: "Chiết khấu ",
            typeValue: "Mixed",
            placeholder: "Nhập chiết khấu",
        },
    ],
});
// END

onMounted(() => {
    if (route.query.id) {
        OpenDetail(route.query.id);
    }
});
watch(route, () => {
    if (route.query.id) {
        OpenDetail(route.query.id);
    }
});
// Begin Function
onBeforeMount(async () => {
    dataCommited.value = await GetCommited();
    // dataCustomer.value = await GetCustomer();
});

// Khởi tạo dữ liệu
const InitData = () => {
    confirmModal.value = false;
    visibleDelete.value = false;
    submited.value = false;
    expandedRows.value = [];

    dataCommittedType.value = [
        {
            code: "Q",
            name: t('client.quarter') || "Quý",
        },
        {
            code: "P",
            name: "Gói",
        },
        {
            code: "Y",
            name: t('client.year') || "Năm",
        },
    ];

    PayloadData.value = {
        id: 0,
        committedCode: "",
        committedName: "",
        committedDescription: "",
        cardId: 0,
        committedYear: "",
        docStatus: "",
        committedLine: [
            {
                id: 0,
                fatherId: 0,
                committedType: "",
                status: "",
                committedLineSub: [
                    {
                        id: 0,
                        fatherId: 0,
                        industryId: 0,
                        brandIds: 0,
                        quarter1: 0,
                        quarter2: 0,
                        quarter3: 0,
                        quarter4: 0,
                        package: 0,
                        month1: 0,
                        month2: 0,
                        month3: 0,
                        month4: 0,
                        month5: 0,
                        month6: 0,
                        month7: 0,
                        month8: 0,
                        month9: 0,
                        month10: 0,
                        month11: 0,
                        month12: 0,
                        total: 0,
                        discount: 0,
                        isConvert: false,
                        discountMonth: 0,
                        isCvMonth: false,
                        nineMonthDiscount: 0,
                        isCvNineMonth: false,
                        discountYear: 0,
                        isCvYear: false,
                        status: "",
                        committedLineSubSub: [
                            {
                                id: 0,
                                fatherId: 0,
                                outPut: 0,
                                discount: 0,
                                isConvert: false,
                                status: "",
                            },
                        ],
                    },
                ],
            },
        ],
    };

    committedLine.value = JSON.stringify(PayloadData.value.committedLine[0]);
};
InitData();

const GetCustomer = async () => {
    try {
        const res = await API.get("Customer?skip=0&limit=1000");
        return res.data.items;
    } catch (error) {
        return [];
    }
};

const GetCommited = async (filters = "") => {
    try {
        loadding.value = true;
        const queryParams = `?skip=${dataTable.value.skip}&limit=${dataTable.value.limit}${filters}`;
        const res = await API.get("Commited/me" + queryParams);
        Object.assign(dataTable.value, {
            limit: res.data.limit,
            skip: res.data.skip,
            total: res.data.total,
        });
        router.push(queryParams);
        return res.data.data;
    } catch (error) {
        return [];
    } finally {
        loadding.value = false;
    }
};

const saveCommited = async (type) => {
    submited.value = true;
    try {
        loadding.value = true;
        PayloadData.value.docStatus = type;
        const ENDPOINT = API.update(`Commited/${PayloadData.value.id}/${type}`);
        const res = await ENDPOINT;
        if (res.status === 200) {
            const mess = t('body.systemSetting.success_label') || "Xác nhận thành công";
            FunctionGlobal.$notify("S", mess, toast);
            dataCommited.value = await GetCommited();
            InitData();
        }
    } catch (error) {
        FunctionGlobal.$notify("E", error.response.data.errors, toast);
    } finally {
        loadding.value = false;
    }
};

const loadingReject = ref(false);
const visibleConfirm = ref(false);
const reasonError = ref(null);
const rejectReason = ref("");
const onClickShowConfirm = () => {
    visibleConfirm.value = true;
    reasonError.value = null;
};
const onClickConfirm = () => {
    const reason = rejectReason.value?.trim() || "";
    if (!reason.length) {
        reasonError.value = t('client.please_enter_reason') || "Vui lòng nhập lý do";
        return;
    }
    reasonError.value = null;
    loadingReject.value = true;
    API.update(`Commited/${PayloadData.value.id}/reject`, {
        rejectReason: rejectReason.value,
    })
        .then(async (res) => {
            loadingReject.value = false;
            toast.add({
                severity: "success",
                summary: t('body.systemSetting.success_label'),
                detail: t('body.sampleRequest.commitment.reject_success') || "Đã gửi yêu cầu thành công",
                life: 3000,
            });
            visibleConfirm.value = false;
            confirmModal.value = false;
            dataCommited.value = await GetCommited();
            InitData();
        })
        .catch((error) => {
            loadingReject.value = false;
            toast.add({
                severity: "danger",
                summary: t('body.report.error_occurred_message'),
                detail: t('body.report.error_occurred_message'),
                life: 3000,
            });
        });
};

const getByIdCommeted = async (id) => {
    try {
        const res = await API.get(`Commited/${id}`);
        return res.data;
    } catch (error) {
        return [];
    }
};

const OpenDetail = async (id) => {
    InitData();
    await getHierarchy();
    loadding.value = true;
    PayloadData.value = await getByIdCommeted(id);
    PayloadData.value.committedLine.forEach((el) => {
        el.committedLineSub.forEach((element) => {
            element.brandIds = element.brand.map((b) => b.id);
        });
    });
    PayloadData.value.committedYear = new Date(PayloadData.value.committedYear);
    if (
        PayloadData.value?.committedLine[0]?.committedLineSub[0].committedLineSubSub
            .length
    )
        expandedRows.value =
            [PayloadData.value?.committedLine[0]?.committedLineSub[0]] || [];
    loadding.value = false;
    confirmModal.value = true;
};

const onFilter = async () => {
    const queryString = conditionHandler.getQuery(filterStore.filters);
    dataCommited.value = await GetCommited(queryString);
};

const onPageChange = (event) => {
    dataTable.value.skip = event.page;
    dataTable.value.limit = event.rows;
    onFilter();
};

const clearFilter = () => {
    filterStore.resetFilters();
    onFilter();
};

const onModalHide = () => {
    router.replace({
        name: router.name,
        query: null,
    });
};

const getHierarchy = async () => {
    try {
        const { data } = await API.get("Item/hierarchy?cardId=");
        if (data) Hierarchy.value = data.items;
    } catch (error) {
        Hierarchy.value = [];
    }
};

const getIndustry = async () => {
    try {
        const { data } = await API.get("Industry/getall");
        Industry.value = data;
    } catch (error) {
        Industry.value = [];
    } finally {
        IndustryClone.value = JSON.stringify(Industry.value);
    }
};

getIndustry();

const setStatus = (key) => {
    const data = [
        {
            label: t('body.status.DXN'),
            code: "A",
            type: "primary",
        },
        {
            label: t('body.status.CXN'),
            code: "P",
            type: "warning",
        },
        {
            label: t('body.status.HUY'),
            code: "R",
            type: "danger",
        },
        {
            label: t('body.status.DH'),
            code: "C",
            type: "secondary",
        },
    ];
    return (
        data.find((el) => el.code == key) || {
            label: "Unknown",
            code: "",
            type: "constrast",
        }
    );
};
</script>

<style scoped>
.bg_ {
    background: none;
    border: none;
}

:deep(.p-inputnumber-input) {
    width: 100%;
}
</style>
