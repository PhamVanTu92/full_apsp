<template>
    <div class="flex flex-column gap-4">
        <div class="flex justify-content-between align-items-center">
            <strong class="text-2xl">{{ t('body.productManagement.packaging') }}</strong>
            <div class="flex gap-2">
                <!-- <Button @click="openAddDialog()" label="Thêm mới" icon="pi pi-plus" to="" /> -->
                <!-- <Button label="Import" icon="pi pi-file-import" /> -->
            </div>
        </div>
        <div class="card shadow-1">
            <DataTable
                v-model:selection="selectedUnits"
                paginator
                :rows="rows"
                :currentPageReportTemplate="`${t('body.productManagement.display')} {first} - {last} ${t('body.productManagement.total_of')} {totalRecords} ${t('body.systemSetting.orders')}`"
                stripedRows
                showGridlines
                paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
                :rowsPerPageOptions="[5, 10, 20, 50]"
                :value="Units"
                tableStyle="min-width: 50rem"
            >
                <template #empty>
                    <div class="text-center p-2">{{ t('body.systemSetting.no_data_to_display') }}</div>
                </template>
                <template #header>
                    <div class="flex justify-content-between m-0 gap-3">
                        <IconField iconPosition="left">
                            <InputIcon class="pi pi-search"> </InputIcon>
                            <InputText
                                v-model="keySearch"
                                type="text"
                                :placeholder="t('body.productManagement.packaging_search_placeholder')"
                                class="w-30rem"
                                @input="fetchAllUnits(keySearch)"
                            />
                        </IconField>
                        <Button v-if="0" icon="pi pi-sync" @click="fetchAllUnits()" />
                    </div>
                </template>
                <!-- <Column selectionMode="multiple" headerStyle="width: 3rem"></Column> -->
                <Column field="code" :header="t('body.productManagement.packaging_code_column')"></Column>
                <Column field="name" :header="t('body.productManagement.packaging_name_column')"></Column>
                <Column field="volumn" :header="t('body.productManagement.capacity_column')"></Column>
                <Column :header="t('body.sampleRequest.warehouseFee.actions')" headerStyle="width:7rem" v-if="0">
                    <template #body="slotProps">
                        <div class="flex justify-content-center">
                            <Button
                                @click="openEditUnitDialog(slotProps.data)"
                                text
                                icon="pi pi-pencil"
                            />
                        </div>
                    </template>
                </Column>
            </DataTable>
        </div>
    </div>
    <Dialog
        v-model:visible="UnitModal"
        modal
        :header="unitsData.id ? `${t('body.promotion.update_button')} ${t('body.productManagement.packaging')}` : `${t('body.productManagement.add_new_button')} ${t('body.productManagement.packaging')}`"
        :style="{ width: '700px' }"
    >
        <div class="flex flex-column w-full gap-2 mb-4">
            <div class="flex flex-column gap-2 mb-2 w-full">
                <label for=""
                    >{{ t('body.productManagement.packaging_code_column') }} <sup class="p-important text-red-500">*</sup>
                </label>
                <InputText
                    v-model="unitsData.code"
                    :invalid="(submited && !unitsData.code) || checkDuplicate != null"
                >
                </InputText>
                <small
                    v-if="(submited && !unitsData.code) || checkDuplicate != null"
                    class="text-red-500"
                    >{{
                        checkDuplicate != null
                            ? t('body.systemSetting.already_exists_label') || "Duplicate code"
                            : (t('body.productManagement.validate_enter_product_type_name') || "Please enter code")
                    }}</small
                >
            </div>
            <div class="flex flex-column gap-2 mb-2 w-full">
                <label for=""
                    >{{ t('body.productManagement.packaging_name_column') }}<sup class="p-important text-red-500"
                        >*</sup
                    ></label
                >
                <InputText
                    v-model="unitsData.name"
                    :invalid="(submited && !unitsData.name) || checkDuplicate != null"
                >
                </InputText>
                <small
                    v-if="(submited && !unitsData.name) || checkDuplicate != null"
                    class="text-red-500"
                    >{{
                        checkDuplicate != null
                            ? t('body.systemSetting.already_exists_label') || "Duplicate name"
                            : (t('body.productManagement.validate_enter_product_type_name') || "Please enter name")
                    }}</small
                >
            </div>
            <div class="flex flex-column gap-2 mb-2 w-full">
                <label for=""
                    >{{ t('body.productManagement.capacity_column') }}<sup class="p-important text-red-500">*</sup></label
                >
                <InputNumber
                    v-model="unitsData.volumn"
                    :invalid="(submited && !unitsData.volumn) || unitsData.volumn <= 0"
                    mode="decimal"
                    decimalSeparator="."
                    :maxFractionDigits="5"
                ></InputNumber>
                <small v-if="submited && !unitsData.volumn" class="text-red-500"
                    >{{ t('body.productManagement.validate_enter_product_type_name') || 'Please enter volume' }}</small
                >
                <small v-if="submited && unitsData.volumn <= 0" class="text-red-500"
                    >{{ t('body.promotion.validation_denomination_required') || 'Please enter greater than 0' }}</small
                >
            </div>
            <!-- <div class="flex flex-column gap-2 mb-2 w-full">
        <label for="">Trạng thái</label>
        <Dropdown v-model="unitsData.status" optionLabel="label" optionValue="val" :options="unitDataStatus"></Dropdown>
      </div> -->
        </div>
        <template #footer>
            <div class="flex gap-2">
                <Button outlined @click="UnitModal = false" :label="t('body.importPlan.cancel_button')"/>
                <Button @click="confirmSaveUnit()" :label="t('body.systemSetting.save_button')"/>
            </div>
        </template>
    </Dialog>
    <!-- Xóa đơn vị tính -->
    <Dialog
        v-model:visible="removeUnitModal"
        :style="{ width: '450px' }"
        :header="t('body.OrderApproval.confirm')"
        :modal="true"
    >
        <div class="flex align-items-center justify-content-center">
            <i class="pi pi-exclamation-triangle mr-3" style="font-size: 2rem" />
            <span
                >{{ t('body.productManagement.delete') || 'Are you sure to delete' }} <b>{{ unitsData.name }}</b> ?</span
            >
        </div>
        <template #footer>
            <Button
                :label="t('body.importPlan.cancel_button')"
                icon="pi pi-times"
                outlined
                @click="removeUnitModal = false"
            />
            <Button :label="t('body.SaleSchannel.delete')" icon="pi pi-check" @click="confirmRemove()" />
        </template>
    </Dialog>
    <loadding v-if="isLoading"></loadding>
</template>
<script setup>
import { onMounted, ref } from "vue";
import API from "@/api/api-main";
import { getCurrentInstance } from "vue";
import { useToast } from "primevue/usetoast";
import { useRouter } from "vue-router";
import { useI18n } from "vue-i18n";

const { t } = useI18n();

const keySearch = ref();
const checkDuplicate = ref(null);
const submited = ref(false);
const isLoading = ref(false);
const router = useRouter();
const rows = ref(10);
const page = ref(0);
const totalRecords = ref();
const toast = useToast();
const { proxy } = getCurrentInstance();
const UnitModal = ref(false);
const removeUnitModal = ref(false);
const selectedColumn = ref([]);
const selectedUnits = ref([]);
const Units = ref([]);
const unitsData = ref({});
const clearUnitsData = JSON.stringify(unitsData.value);
onMounted(() => {
    fetchAllUnits();
});

const openAddDialog = () => {
    checkDuplicate.value = null;
    FuncClearUnitsData();
    UnitModal.value = true;
};
const FuncClearUnitsData = () => {
    submited.value = false;
    unitsData.value = JSON.parse(clearUnitsData);
};
const removeUnitsDialog = (data) => {
    unitsData.value = data;
    removeUnitModal.value = true;
};
const openEditUnitDialog = async (data) => {
    checkDuplicate.value = null;
    unitsData.value = { ...data };
    UnitModal.value = !UnitModal.value;
};

const debounce = (func, wait) => {
    let timeout;
    return (...args) => {
        const context = this;
        clearTimeout(timeout);
        timeout = setTimeout(() => func.apply(context, args), wait);
    };
};

const fetchAllUnits = debounce(async (keySearched = "") => {
    isLoading.value = true;
    const searchKey = keySearched ? keySearched.trim() : "";
    searchKey == "" ? (keySearch.value = "") : "";
    const uri =
        searchKey.trim() != "" ? `Packing/search/${searchKey.trim()}` : `packing/getall`;
    try {
        const res = await API.get(uri);
        Units.value = res.data.items ? res.data.items : res.data;

        totalRecords.value = res.data.total;
    } catch (e) {
        proxy.$notify("E", e.message, toast);
    } finally {
        isLoading.value = false;
        // router.push(`?skip=${page.value}&limit=${rows.value}`);
    }
}, 1000);

const validateDate = () => {
    let status = true;
    if (unitsData.value.code == null || unitsData.value.code == "") status = false;
    if (unitsData.value.name == null || unitsData.value.name == "") status = false;
    if (
        unitsData.value.volumn == null ||
        unitsData.value.volumn == "" ||
        unitsData.value.volumn <= 0
    )
        status = false;
    return status;
};
const confirmSaveUnit = async () => {
    const ENDPOINT_API = unitsData.value.id
        ? `packing/${unitsData.value.id}`
        : `packing/add`;
    submited.value = true;
    if (!validateDate()) {
        checkDuplicate.value = null;
        return;
    }
    try {
        const FUNC_API = unitsData.value.id
            ? API.update(ENDPOINT_API, unitsData.value)
            : API.add(ENDPOINT_API, { id: 0, ...unitsData.value });
        const res = await FUNC_API;
        if (res)
            proxy.$notify(
                "S",
                unitsData.value.id
                    ? t('body.systemSetting.update_account_success_message') || `Updated successfully!`
                    : t('body.systemSetting.success_label') || `Created packaging!`,
                toast
            );
        UnitModal.value = false;
    } catch (error) {
        checkDuplicate.value = error.response.data.status;

        //proxy.$notify('E', error.response.data.status == 400 ? 'Quy cách bao bì bị trùng' : error.response.data.errors, toast);
    } finally {
        fetchAllUnits();
        // UnitModal.value = false;
    }
};
const confirmRemove = async () => {
    isLoading.value = true;
    try {
        const res = await API.delete(`packing/${unitsData.value.id}`);
        if (res) proxy.$notify("S", t('body.systemSetting.success_label') || `Deleted successfully`, toast);
    } catch (e) {
        proxy.$notify("E", e.message, toast);
    } finally {
        isLoading.value = false;
        removeUnitModal.value = false;
        fetchAllUnits();
    }
};

// const searchUint = async () => {}
</script>
<style></style>
