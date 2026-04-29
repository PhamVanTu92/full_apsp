<template>
    <Loading v-if="loading" />
    <div class="flex justify-content-between align-items-center mb-4">
        <h4 class="font-bold m-0">{{ t('body.productManagement.product_type') }}</h4>
        <div class="flex gap-2">
            <!-- <Button label="Thêm mới" icon="fa-solid fa-plus" @click="getItemType()"/> -->
            <!-- <Button icon="fa-solid fa-sync" @click="getAllType()"/> -->
        </div>
    </div>

    <div class="grid mt-3 card p-2">
        <div class="col-12 bg-white">
            <DataTable
                :value="dataTable"
                showGridlines
                paginator
                lazy
                :rows="paginator.rows"
                :totalRecords="paginator.total"
                :rowsPerPageOptions="paginator.rowsPerPageOptions"
                @page="changePage"
            >
                <template #header>
                    <div class="flex justify-content-between m-0">
                        <!-- <Button type="button" icon="pi pi-filter-slash" label="Xóa bộ lọc" outlined @click="clearFilter()" /> -->
                        <div class="w-4 flex gap-4">
                            <IconField iconPosition="left">
                                <InputText
                                    :placeholder="t('body.productManagement.search_placeholder')"
                                    class="w-full"
                                    v-model="keySearch"
                                    @input="getAllType(keySearch)"
                                />
                                <InputIcon>
                                    <i class="pi pi-search" />
                                </InputIcon>
                            </IconField>
                        </div>
                        <Button
                            v-if="0"
                            icon="fa-solid fa-sync"
                            @click="getAllType()"
                        />
                    </div>
                </template>
                <template #empty>{{ t('body.systemSetting.no_data_to_display') }}</template>
                <Column field="code" :header="t('body.productManagement.product_type_code')"></Column>
                <Column field="name" :header="t('body.productManagement.product_type_name')"></Column>
                <!-- <Column field="id" style="width: 4rem">
                    <template #body="{ data }">
                        <div class="flex gap-1">
                            <Button text @click="getItemType(data.id)" icon="pi pi-pencil" />
                        </div>
                    </template>
                </Column> -->
            </DataTable>
        </div>
    </div>

    <Dialog
        v-model:visible="showDialogSave"
        :header="payload.id ? `Chỉnh loại sửa sản phẩm` : 'Thêm mới loại sản phẩm'"
        modal
        :style="{ width: '33rem' }"
    >
        <div class="flex flex-column mb-5">
            <label for="code" class="font-semibold mb-1"
                >Mã
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
                id="code"
                v-model="payload.code"
                class="flex-auto"
                autocomplete="off"
                :invalid="submited && !payload.code"
            />
            <small v-if="submited && !payload.code" class="text-red-500"
                >Vui lòng nhập mã!</small
            >
        </div>
        <div class="flex flex-column mb-">
            <label for="name" class="font-semibold mb-1"
                >Tên loại sản phẩm
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
                id="name"
                v-model="payload.name"
                class="flex-auto"
                autocomplete="off"
                :invalid="submited && !payload.name"
            />
            <small v-if="submited && !payload.name" class="text-red-500"
                >{{t('body.productManagement.validate_enter_product_type_name')}}</small
            >
        </div>
        <template #footer>
            <div class="flex justify-end gap-2">
                <Button
                    type="button"
                    :label="t('body.sampleRequest.importPlan.cancel_button')"
                    severity="secondary"
                    @click="showDialogSave = false"
                />
                <Button type="button" :label="t('body.systemSetting.save_button')" @click="SaveItem()"/>
            </div>
        </template>
    </Dialog>
</template>

<style scoped></style>
<script setup>
import { ref, onBeforeMount, watch, reactive } from "vue";
import API from "@/api/api-main";
import { useGlobal } from "@/services/useGlobal";
import { useI18n } from "vue-i18n";

//Trang chính
const API_URL = "ItemType";
const loading = ref(false);
const { toast, FunctionGlobal } = useGlobal();
const { t } = useI18n();

const paginator = reactive({
    total: 0,
    page: 0,
    rows: 10,
    rowsPerPageOptions: [10, 20, 50],
});
const keySearch = ref();
const dataTable = ref([]);

const debounce = (func, wait) => {
    let timeout;
    return (...args) => {
        const context = this;
        clearTimeout(timeout);
        timeout = setTimeout(() => func.apply(context, args), wait);
    };
};

const getAllType = debounce(async (keySearched = "") => {
    loading.value = true;
    try {
        keySearched == "" ? (keySearch.value = "") : "";
        const url =
            keySearched != ""
                ? `${API_URL}/search/${encodeURIComponent(keySearched)}`
                : `${API_URL}/GetPagination?skip=${paginator.page}&limit=${paginator.rows}`;
        const res = await API.get(url);
        if (res.data && keySearched) {
            res.data?.items
                ? (dataTable.value = res.data.items)
                : (dataTable.value = res.data);
        }
        if (res.data.items) {
            if (!keySearched) {
                dataTable.value = res.data.items;
                Object.assign(paginator, {
                    total: res.data.total,
                    page: res.data.skip,
                    rows: res.data.limit,
                });
            }
        }
    } catch (error) {
        FunctionGlobal.$notify("E", error, toast);
    } finally {
        loading.value = false;
    }
}, 200);

const changePage = async (event) => {
    paginator.page = event.page;
    paginator.rows = event.rows;
    await getAllType();
};

//Các chức năng: Thêm, Sửa, Xoá
const submited = ref(false);
const payload = ref({});

const clearPayload = () => {
    submited.value = false;
    payload.value = {};
};

const validateDate = () => {
    const { code, name } = payload.value;
    return Boolean(code?.trim() && name?.trim());
};
const showDialogSave = ref(false);
const getItemType = async (key) => {
    try {
        clearPayload();
        if (key) {
            const res = await API.get(`${API_URL}/${key}`);
            payload.value = res.data;
        }
        showDialogSave.value = true;
    } catch (error) {
        FunctionGlobal.$notify("E", error, toast);
    } finally {
    }
};

const SaveItem = async () => {
    const id = payload.value.id;
    submited.value = true;
    if (!validateDate()) return;
    loading.value = true;
    try {
        const url = id
            ? API.update(`${API_URL}/${id}`, payload.value)
            : API.add(`${API_URL}/add`, payload.value);
        const res = await url;
        id
            ? FunctionGlobal.$notify("S", "Cập nhật loại sản phẩm thành công!", toast)
            : FunctionGlobal.$notify("S", "Thêm loại sản phẩm thành công!", toast);
        await getAllType();
        showDialogSave.value = false;
    } catch (error) {
        FunctionGlobal.$notify("E", error, toast);
    } finally {
        loading.value = false;
    }
};

onBeforeMount(() => {
    getAllType();
});
</script>
