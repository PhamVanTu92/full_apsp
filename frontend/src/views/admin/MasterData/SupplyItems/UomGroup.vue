<template>
    <div class="flex flex-column gap-4">
        <div class="flex justify-content-between align-items-center">
            <h4 class="font-bold m-0">{{ t('body.sampleRequest.warehouseFee.table_header_unit_group') }}</h4>
            <div class="flex gap-2">
                <!-- <Button
                    @click="openAddDialog()"
                    :label="t('body.productManagement.add_new_button')"
                    icon="pi pi-plus"
                    to=""
                /> -->
            </div>
        </div>
        <div class="card grid mt-2 p-2">
            <div class="col-12">
                <DataTable
                    v-model:selection="selectedUnitGrp"
                    paginator
                    :rows="rows"
                    :page="page"
                    :totalRecords="totalRecords"
                    lazy
                    @page="onPageChange($event)"
                     :currentPageReportTemplate="`${t('body.productManagement.display')} {first} - {last} ${t('body.productManagement.total_of')} {totalRecords} ${t('body.systemSetting.orders')}`"
                    scrollable
                    scrollHeight="70vh"
                    stripedRows
                    showGridlines
                    paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
                    :rowsPerPageOptions="[10, 20, 50]"
                    :value="UnitGroup"
                    tableStyle="min-width: 50rem"
                >
                    <template #empty>
                        <div class="text-center p-2">{{ t('body.systemSetting.no_data_to_display') }}</div>
                    </template>
                    <template #header>
                        <div class="flex justify-content-between m-0 mb-3">
                            <Button
                                v-if="false"
                                type="button"
                                icon="pi pi-filter-slash"
                                label="Xóa bộ lọc"
                                outlined
                                @click="clearFilter()"
                            />
                            <IconField iconPosition="left">
                                <InputText
                                    :placeholder="t('body.productManagement.search_placeholder')"
                                    v-model="searhKey"
                                    @input="fetchAllUnitGroup(searhKey)"
                                />
                                <InputIcon>
                                    <i class="pi pi-search" />
                                </InputIcon>
                            </IconField>
                        </div>
                    </template>
                    <!-- columns -->
                    <Column
                        v-for="col of UnitColumn"
                        :key="col.field"
                        :field="col.field"
                        :header="col.header"
                    >
                    </Column>
                    <Column header=" " headerClass="w-10rem" v-if="0">
                        <template #body="slotProps">
                            <div class="flex gap-2">
                                <Button
                                    @click="openEditUnitDialog(slotProps.data)"
                                    text
                                    icon="pi pi-pencil"
                                />
                                <Button
                                    @click="openDeleteDialog(slotProps.data)"
                                    text
                                    icon="pi pi-trash"
                                    severity="danger"
                                />
                            </div>
                        </template>
                    </Column>
                </DataTable>
            </div>
        </div>
    </div>

    <!-- Thêm mới đơn vị tính -->
    <Dialog
        v-model:visible="addNewModal"
        modal
        :header="dataEdit.status ? (t('body.promotion.update_button') + ' ' + (dataEdit.ugpName || '')) : t('body.sampleRequest.warehouseFee.table_header_unit_group')"
        :style="{ width: '1200px' }"
    >
        <div>
            <div class="flex w-full gap-2 mb-4">
                <div class="flex flex-column gap-2 mb-2 w-full">
                    <label for=""
                        >{{ t('body.productManagement.unit_group_title') }}
                        <span
                            style="
                                color: #eb5757;
                                font-size: 16px;
                                font-family: Open Sans;
                                font-weight: 400;
                                word-wrap: break-word;
                                margin-left: 3px;
                            "
                            >*</span
                        ></label
                    >
                    <InputText
                        :invalid="check == false || unitGrpData.ugpCode != '' ? false : true"
                        v-model="unitGrpData.ugpCode"
                    ></InputText>
                    <small v-if="er != null" class="text-red-500 mt-1">{{ t('body.systemSetting.already_exists_label') }}</small>
                    <small v-else-if="!(check == false || unitGrpData.ugpCode != '')" class="text-red-500 mt-1">{{ t('body.productManagement.validate_enter_product_type_name') }}</small>
                </div>
                <div class="flex flex-column gap-2 mb-2 w-full">
                    <label for=""
                        >Tên nhóm đơn vị tính
                        <span
                            style="
                                color: #eb5757;
                                font-size: 16px;
                                font-family: Open Sans;
                                font-weight: 400;
                                word-wrap: break-word;
                                margin-left: 3px;
                            "
                            >*</span
                        ></label
                    >
                    <InputText
                        :invalid="check == false || unitGrpData.ugpName != '' ? false : true"
                        v-model="unitGrpData.ugpName"
                    ></InputText>
                    <small v-if="check == false || unitGrpData.ugpName != '' ? false : true" class="text-red-500 mt-1">{{ t('body.productManagement.validate_enter_product_type_name') }}</small>
                </div>
                <div class="flex flex-column gap-2 mb-2 w-full">
                    <label for=""
                        >Đơn vị tính cơ sở
                        <span
                            style="
                                color: #eb5757;
                                font-size: 16px;
                                font-family: Open Sans;
                                font-weight: 400;
                                word-wrap: break-word;
                                margin-left: 3px;
                            "
                            >*</span
                        ></label
                    >
                    <InputGroup>
                        <Dropdown
                            :invalid="check == false || unitGrpData.baseUom != '' ? false : true"
                            v-model="unitGrpData.baseUom"
                            filter
                            @change="chooseUnit($event)"
                            :options="Units.filter((item) => { return checkDvt(unitGrpData.baseUom, item); })"
                            optionValue="id"
                            optionLabel="uomName"
                        >
                        </Dropdown>
                        <Button icon="pi pi-plus" @click="onClickOpenDlPacking"/>
                    </InputGroup>
                    <small v-if="check == false || unitGrpData.baseUom != '' ? false : true" class="text-red-500 mt-1">{{ t('body.productManagement.validate_enter_product_type_name') }}</small>
                </div>
            </div>
            <div>
                <DataTable
                    stripedRows
                    :value="unitGrpData.ugP1.filter((val) => { return val.status != 'D'; })"
                    v-if="unitGrpData.baseUom != ''"
                >
                    <Column field="baseQty" :header="t('body.sampleRequest.warehouseFee.table_header_price')">
                        <template #body="{ index, ...slotProps }">
                            <div>
                                <InputNumber
                                    v-model="slotProps.data.baseQty"
                                    :disabled="index == 0"
                                    :min="1"
                                >
                                </InputNumber>
                            </div>
                        </template>
                    </Column>
                    <Column :header="t('body.productManagement.unit_name_column')">
                        <template #body="{ index, ...slotProps }">
                            <div>
                                <Dropdown
                                    :disabled="index == 0"
                                    v-model="slotProps.data.uomId"
                                    optionLabel="uomName"
                                    optionValue="id"
                                    :options="Units.filter((val) => { return checkDvt(slotProps.data.uomId, val); })"
                                    class="w-full"
                                ></Dropdown>
                            </div>
                        </template>
                    </Column>
                    <Column field="altQty" :header="t('body.sampleRequest.warehouseFee.table_header_price')">
                        <template #body="{ index, ...slotProps }">
                            <div>
                                <InputNumber
                                    v-model="slotProps.data.altQty"
                                    :disabled="index == 0"
                                    :min="1"
                                >
                                </InputNumber>
                            </div>
                        </template>
                    </Column>
                    <Column :header="t('body.productManagement.unit_group_title')">
                        <template #body>
                            <InputText
                                disabled
                                v-model="Units.filter((val) => { return val.id == unitGrpData.baseUom; })[0].uomName"
                            ></InputText>
                        </template>
                    </Column>
                    <Column header=" " >
                        <template #body="{ index, ...slotProps }">
                            <div class="flex align-items-center gap-2">
                                <Button
                                    :disabled="index == 0"
                                    @click="removeUnits(slotProps.data)"
                                    text
                                    icon="pi pi-trash"
                                />
                            </div>
                        </template>
                    </Column>
                </DataTable>
                <div class="my-2">
                    <Button
                        @click="addNewUnits()"
                        text
                        :label="t('body.promotion.addPromotionCondition')"
                        icon="pi pi-plus-circle"
                    />
                </div>
            </div>
            <div class="flex justify-content-start">
                <div class="flex flex-column gap-2">
                    <label for="Status">{{ t('body.systemSetting.status_label') }}</label>
                    <InputSwitch inputId="Status" v-model="unitGrpData.status"></InputSwitch>
                </div>
            </div>
            <div class="flex justify-content-end">
                <div class="flex gap-2">
                    <Button outlined :label="t('body.importPlan.cancel_button')" @click="addNewModal = false"/>
                    <Button v-if="dataEdit.status" @click="updUnitGrp" :label="t('body.promotion.update_button')"/>
                    <Button v-else @click="addNewUnitGrp" :label="t('body.systemSetting.save_button')"/>
                </div>
            </div>
        </div>
    </Dialog>

    <!-- Xóa nhóm đơn vị -->
    <Dialog
        position="top"
        :draggable="false"
        v-model:visible="removeUnitGrpModal"
        :style="{ width: '450px' }"
        :header="t('body.OrderApproval.confirm')"
        :modal="true"
    >
        <div class="flex align-items-center justify-content-center">
            <i class="pi pi-exclamation-triangle mr-3" style="font-size: 2rem" />
            <span v-if="selectedUnitGrp">
                {{ t('body.productManagement.delete') || 'Are you sure to delete' }} <b>{{ selectedUnitGrp[0].ugpName }}</b> ?
            </span>
        </div>
        <template #footer>
            <Button label="Hủy" icon="pi pi-times" outlined @click="openDeleteDialog(null)"/>
            <Button label="{{ t('body.SaleSchannel.delete') }}" icon="pi pi-check" @click="confirmRemove(selectedUnitGrp[0].id)"/>
        </template>
    </Dialog>
    <Loadding v-if="isLoading"></Loadding>

    <Dialog
        @hide="onHideOuomDl"
        v-model:visible="visibleOuom"
        modal
        :header="t('body.sampleRequest.customer.unit_group_title')"
        class="w-2"
    >
        <div class="flex flex-column field">
            <label for="uomCode" class="font-semibold">{{ t('body.productManagement.packaging_code_column') }}<sup class="text-red-500">*</sup></label>
            <InputText v-model="ouomState.uomCode" id="uomCode" :invalid="ouomErrorMsg.uomCode"></InputText>
            <small class="text-red-500" v-if="ouomErrorMsg.uomCode">{{ ouomErrorMsg.uomCode }}</small>
        </div>
        <div class="flex flex-column field">
            <label for="uomName" class="font-semibold">{{ t('body.productManagement.packaging_name_column') }}<sup class="text-red-500">*</sup></label>
            <InputText v-model="ouomState.uomName" id="uomName" :invalid="ouomErrorMsg.uomName"></InputText>
            <small class="text-red-500" v-if="ouomErrorMsg.uomName">{{ ouomErrorMsg.uomName }}</small>
        </div>
        <div class="flex gap-3">
            <InputSwitch v-model="ouomState.status" inputId="ouomStatus" />
            <label for="ouomStatus">{{ t('body.systemSetting.status_label') }}</label>
        </div>
        <template #footer>
            <Button :label="t('body.importPlan.cancel_button')" icon="pi pi-times" severity="secondary" @click="visibleOuom = false"/>
            <Button :label="t('body.systemSetting.save_button')" icon="pi pi-save" @click="onClickSaveOuom"/>
        </template>
    </Dialog>
</template>

<script setup>
import { onMounted, ref, reactive } from "vue";
import API from "@/api/api-main";
import { useGlobal } from "@/services/useGlobal";
import merge from "lodash/merge";
import { useRouter } from "vue-router";
import { useI18n } from "vue-i18n";

const { t } = useI18n();

const onHideOuomDl = () => {
    ouomErrorMsg.uomCode = null;
    ouomErrorMsg.uomName = null;
    Object.assign(ouomState, {
        uomCode: "",
        uomName: "",
        status: true,
    });
};

const ouomErrorMsg = reactive({
    uomCode: null,
    uomName: null,
});

const validateOuom = () => {
    ouomErrorMsg.uomCode = null;
    ouomErrorMsg.uomName = null;
    if (!ouomState.uomCode?.trim())
        ouomErrorMsg.uomCode = "Mã đơn vị tính không được để trống";
    if (!ouomState.uomName?.trim())
        ouomErrorMsg.uomName = "Tên đơn vị tính không được để trống";
    return ouomState.uomCode?.trim() && ouomState.uomName?.trim();
};

const onClickSaveOuom = () => {
    if (validateOuom()) {
        API.add("ouom/add", ouomState)
            .then((res) => {
                toast.add({
                    severity: "success",
                    summary: "Tạo thành công",
                    detail: "Đã tạo đơn vị tính",
                    life: 3000,
                });
                fetchAllUnits();
                visibleOuom.value = false;
            })
            .catch((error) => {
                toast.add({
                    severity: "error",
                    summary: "Tạo thất bại",
                    detail: "Đã có lỗi xảy ra",
                    life: 3000,
                });
            });
    }
};

const ouomState = reactive({
    uomCode: "",
    uomName: "",
    status: true,
});

const visibleOuom = ref(false);
const onClickOpenDlPacking = () => {
    visibleOuom.value = true;
};

const router = useRouter();
const rows = ref(10);
const page = ref(0);
const totalRecords = ref();
const isLoading = ref(true);
const { toast, FunctionGlobal } = useGlobal();

const API_URL = ref("OUGP");
const selectedUnitGrp = ref([]);
const removeUnitGrpModal = ref(false);
const addNewModal = ref(false);
const dataEdit = ref({});
const Units = ref([]);
const UnitGroup = ref([]);
const er = ref(null);
const check = ref(false);
const searhKey = ref();

const unitGrpData = ref({
    ugpCode: "",
    ugpName: "",
    baseUom: "",
    ugP1: [],
    status: true,
});
const UnitColumn = ref([
    {
        field: "ugpCode",
        header: t('body.productManagement.unit_code_column'),
        style: "text-align: start;",
        headerStyle: "min-width: 10rem;",
        type: "text",
    },
    {
        field: "ugpName",
        header: t('body.productManagement.unit_name_column'),
        style: "text-align: start;",
        headerStyle: "min-width: 10rem;",
        type: "text",
    },
]);

onMounted(() => {
    fetchAllUnitGroup();
});

const debounce = (func, wait) => {
    let timeout;
    return (...args) => {
        const context = this;
        clearTimeout(timeout);
        timeout = setTimeout(() => func.apply(context, args), wait);
    };
};

const fetchAllUnitGroup = debounce(async (searhKey = "") => {
    isLoading.value = true;
    const uri =
        searhKey == ""
            ? `${API_URL.value}?skip=${page.value}&limit=${rows.value}`
            : `${API_URL.value}/?search=${searhKey}`;
    try {
        const res = await API.get(uri);
        UnitGroup.value = res.data.items;
        totalRecords.value = res.data.total;
        router.push(`?skip=${page.value}&limit=${rows.value}`);
    } catch (e) {

    } finally {
        isLoading.value = false;
    }
}, 1000);
const onPageChange = (event) => {
    rows.value = event.rows;
    page.value = event.page;
    fetchAllUnitGroup();
};
const fetchAllUnits = async () => {
    try {
        const res = await API.get(`OUOM`);
        Units.value = res.data.items;
    } catch (e) {

    }
};
const chooseUnit = (event) => {
    if (unitGrpData.value.ugP1.length < 1) {
        unitGrpData.value.ugP1.push({
            id: 0,
            ugpId: 0,
            uomId: event.value,
            baseQty: 1,
            altQty: 1,
            type: "",
        });
    } else {
        unitGrpData.value.ugP1[0].uomId = event.value;
        unitGrpData.value.ugP1[0].altQty = 1;
        unitGrpData.value.ugP1[0].baseQty = 1;
    }
};

const openDeleteDialog = (data) => {
    if (data != null) {
        selectedUnitGrp.value = [data];
    } else {
        selectedUnitGrp.value = [];
    }
    removeUnitGrpModal.value = !removeUnitGrpModal.value;
};
const confirmRemove = async (id) => {
    try {
        const res = await API.delete(`${API_URL.value}/${id}`);
        fetchAllUnitGroup();
        selectedUnitGrp.value = [];
        removeUnitGrpModal.value = !removeUnitGrpModal.value;
        FunctionGlobal.$notify("S", res.data, toast);
    } catch (e) {
        FunctionGlobal.$notify("E", e, toast);
    }
};
const addNewUnits = () => {
    if (unitGrpData.value.baseUom == "") {
        FunctionGlobal.$notify("E", "Vui lòng chọn đơn vị tính cơ sở", toast);
        return;
    }
    unitGrpData.value.ugP1.push({
        id: 0,
        ugpId: 0,
        uomId: "",
        baseQty: 0,
        altQty: 0,
        status: "",
    });
};
const removeUnits = (data) => {
    data.status = "D";
};
const addNewUnitGrp = async () => {
    unitGrpData.value.ugP1.shift();
    const data = { ...unitGrpData.value };
    validate(data);
    if (validate(data)) {
        try {
            const res = await API.add(`${API_URL.value}/add`, data);
            fetchAllUnitGroup();
            FunctionGlobal.$notify("S", `Tạo thành công ${res.data.ugpName}`, toast);
            er.value = null;
            addNewModal.value = false;
        } catch (e) {
            er.value = e.response.data.status;
            FunctionGlobal.$notify("E", e.response.data.errors, toast);
        }
    }
};
const updUnitGrp = async () => {
    unitGrpData.value.ugP1.shift();
    const data = { ...unitGrpData.value };
    try {
        const res = await API.update(`${API_URL.value}/${unitGrpData.value.id}`, data);
        FunctionGlobal.$notify("S", `Cập nhật ${res.data.ugpName} thành công!`, toast);
    } catch (e) {
        FunctionGlobal.$notify("E", e, toast);
    } finally {
        addNewModal.value = false;
        fetchAllUnitGroup();
    }
};
const openAddDialog = () => {
    er.value = null;
    check.value = false;
    dataEdit.value = {};
    unitGrpData.value = {
        id: 0,
        ugpCode: "",
        ugpName: "",
        baseUom: "",
        ugP1: [],
        status: true,
    };
    fetchAllUnits();
    addNewModal.value = true;
};
const openEditUnitDialog = async (data) => {
    er.value = null;
    check.value = false;
    dataEdit.value = { ...data };
    dataEdit.value.status = "update";
    await fetchAllUnits();
    await fetchIDUnits(data.id);
    addNewModal.value = true;
};

const checkDvt = (id, data) => {
    if (id == data.id) return true;
    const arrCheck = unitGrpData.value.ugP1;
    const check = arrCheck.filter((val) => {
        return val.uomId == data.id && val.status != "D";
    });
    if (check.length) {
        return false;
    } else {
        return true;
    }
};

const fetchIDUnits = async (id) => {
    try {
        const res = await API.get(`${API_URL.value}/${id}`);
        res.data.ugP1.unshift({
            id: 0,
            ugpId: res.data.id,
            uomId: res.data.baseUom,
            baseQty: 1,
            altQty: 1,
        });
        unitGrpData.value = merge({}, unitGrpData.value, res.data);
        const ugP1 = unitGrpData.value.ugP1;
        ugP1.forEach((item) => {
            item.status = "U";
        });
    } catch (error) {
        console.error(error);
    }
};

const validate = (data) => {
    check.value = true;
    if (data.ugpCode == "") {
        return false;
    } else if (data.ugpName == "") {
        return false;
    } else if (data.baseUom == "") {
        return false;
    }

    return true;
};
</script>

<style></style>
