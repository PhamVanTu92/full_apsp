<template>
    <Loading v-if="loading" />
    <TabView class="card p-0 overflow-hidden">
        <TabPanel :header="t('body.productManagement.product_list_title')">
            <div class="w-12 flex gap-2 justify-content-end">
                <Button @click="visible = true" :label="t('body.productManagement.add_new_button')" icon="pi pi-plus"
                    v-if="0" />
                <Button v-if="0" icon="fa-solid fa-file-import" label="Nhập file Excel"/>
            </div>
            <DataTable :value="data.items?.filter((el) => el.itemType.code !== 'VPKM')" showGridlines paginator lazy
                :first="paginator.page * paginator.rows" :rows="paginator.rows" :totalRecords="data.total"
                @page="changePage" resizableColumns columnResizeMode="fit"
                :rowsPerPageOptions="paginator.rowsPerPageOptions" scrollable scrollHeight="72vh">
                <template #header>
                    <div class="flex justify-content-between gap-2">
                        <div class="flex-grow-1">
                            <ProductFilter @change="onChangeFilter" :clearable="true"> </ProductFilter>
                        </div>
                        <div class="w-8rem flex">
                            <Button class="flex-grow-1" :label="t('body.productManagement.sync_button')"
                                icon="pi pi-sync" @click="onClickSync" :loading="loadingSync" />
                        </div>
                    </div>
                </template>
                <Column :header="t('body.productManagement.image_column')">
                    <template #body="{ data }">
                        <Image :src="data.itM1[0]?.filePath || 'https://placehold.co/40x40'" width="40px" height="40px"
                            :preview="!!data.itM1[0]?.filePath"></Image>
                    </template>
                </Column>
                <Column field="itemCode" :header="t('body.productManagement.product_code_column')"></Column>
                <Column field="itemName" :header="t('body.productManagement.product_name_column')"></Column>
                <Column field="brand.name" :header="t('body.productManagement.brand')"></Column>
                <Column field="industry.name" :header="t('body.productManagement.category')"></Column>
                <Column field="itemType.name" :header="t('body.productManagement.product_type')"> </Column>
                <!-- Thêm mới sau -->
                <Column field="productGroup.name" :header="t('body.productManagement.product_group')"></Column>
                <Column field="productQualityLevel.name" :header="t('body.productManagement.quality_level')"></Column>
                <Column field="productApplications.name" :header="t('body.productManagement.application')"></Column>
                <!--  end-->
                <Column field="packing.name" :header="t('body.productManagement.packaging')"></Column>
                <Column field="price" :header="t('body.productManagement.price_before_vat')">
                    <template #body="{ data }">
                        <span class="text-primary">
                            {{
                                data.currency === "USD"
                                    ? Intl.NumberFormat('vi-VI', {
                                        minimumFractionDigits: 2, maximumFractionDigits: 2
                                    }).format(data.price) + ' ' + (data.currency || '')
                                    : Intl.NumberFormat('vi-VI').format(data.price) + ' ' + (data.currency || '')
                            }}
                        </span>
                    </template>
                </Column>
                <Column field="industry.name" :header="t('body.productManagement.price_with_vat')">
                    <template #body="{ data }">
                        <span class="text-primary">
                            {{
                                data.currency === "USD"
                                    ? Intl.NumberFormat('vi-VI', { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(
                                        data.price_after_tax = data.price + (data.taxGroups.rate * data.price) / 100
                                    ) + ' ' + (data.currency || '')
                                    : Intl.NumberFormat('vi-VI').format(
                                        data.price_after_tax = data.price + (data.taxGroups.rate * data.price) / 100
                                    ) + ' ' + (data.currency || '')
                            }}
                            ({{ data.taxGroups.name }})
                        </span>
                    </template>
                </Column>
                <Column style="width: 5rem">
                    <template #body="{ data }">
                        <div class="flex gap-1">
                            <Button text @click="onClickEditData(data)" icon="pi pi-pencil" />
                            <Button v-if="0" text @click="onClickDeleteItem(data)" severity="danger"
                                icon="pi pi-trash" />
                        </div>
                    </template>
                </Column>

                <template #empty>
                    <div class="p-5 text-center">{{ t('body.systemSetting.no_data_to_display') }}</div>
                </template>
            </DataTable>
        </TabPanel>
        <TabPanel :header="t('body.promotion.gift_item')">
            <PromotionProducts :isFilter="isFilter" :clickEdit="onClickEditData" :clickDelete="onClickDeleteItem" />
        </TabPanel>
    </TabView>

    <Dialog @hide="onCloseDialog" v-model:visible="visible"
        :header="payload.id == 0 ? t('body.productManagement.add_new_button') : `${t('body.OrderList.edit')}: ${payload.itemCode}`"
        modal style="width: 70rem">
        <div class="grid">
            <div class="col-3 field pb-0 mb-0">
                <label for="">{{ t('body.productManagement.product_code_column') }} <sup
                        class="text-red-500 font-bold">*</sup></label>
                <InputText :disabled="payload.id" :readonly="payload.id"
                    :invalid="errorMsg.itemCode && !payload.itemCode" v-model.trim="payload.itemCode" class="w-full" />
                <small v-if="submited && !payload.itemCode" class="text-red-500">{{
                    t('body.productManagement.product_code_column') }} {{ t('body.systemSetting.invalid_data_message')
                    }}</small>
            </div>
            <div class="col-6 field pb-0 mb-0">
                <label for="">{{ t('body.productManagement.product_name_column') }} <sup
                        class="text-red-500 font-bold">*</sup></label>
                <InputText :disabled="payload.id" v-model.trim="payload.itemName" class="w-full" />
                <small v-if="submited && !payload.itemName" class="text-red-500">{{
                    t('body.productManagement.product_name_column') }} {{ t('body.systemSetting.invalid_data_message')
                    }}</small>
            </div>
            <div class="col-3 field pb-0 mb-0">
                <label for="">{{ t('body.productManagement.unit_group_title') }} <sup
                        class="text-red-500 font-bold">*</sup></label>
                <Dropdown v-model="payload.ugpEntry" :options="dataGlobal.UnitGroups" optionValue="id"
                    optionLabel="ugpName" filter class="w-full" dropdownMode="current" :disabled="payload.id" />
                <small v-if="submited && !payload.ugpEntry" class="text-red-500">{{
                    t('body.productManagement.unit_group_title') }} {{ t('body.systemSetting.invalid_data_message')
                    }}</small>
            </div>
            <div class="col-3 field pb-0 mb-0">
                <label for="">{{ t('client.price_sell') }}<sup class="text-red-500 font-bold">*</sup></label>
                <InputNumber v-if="payload.currency != 'USD'" :disabled="payload.id" v-model="payload.price"
                    suffix=" VNĐ" :pt:input:root:class="'w-full'" />
                <InputNumber v-else :disabled="payload.id" v-model="payload.price" suffix=" USD"
                    :pt:input:root:class="'w-full'" :minFractionDigits="2" :maxFractionDigits="2" />
                <small v-if="submited && (payload.price === null || payload.price < 0)" class="text-red-500">Vui lòng
                    nhập
                    giá bán hợp lệ</small>
            </div>
            <div class="col-3 field pb-0 mb-0">
                <label for="">{{ t('body.sampleRequest.warehouseFee.table_header_tax') }} <sup
                        class="text-red-500 font-bold">*</sup></label>
                <Dropdown :disabled="payload.id" v-model="payload.taxGroupsId" :options="dataGlobal.Taxs"
                    optionValue="id" optionLabel="name" class="w-full" />
                <small v-if="submited && !payload.taxGroupsId" class="text-red-500">Vui lòng chọn thuế</small>
            </div>
            <div class="col-3 field pb-0 mb-.0">
                <label for="">{{ t('body.productManagement.brand') }} <sup
                        class="text-red-500 font-bold">*</sup></label>
                <InputGroup>
                    <Dropdown v-model="payload.brandId" optionLabel="name" optionValue="id"
                        :options="dataGlobal.BrandContent" class="w-full" filter />
                    <Button @click="OpenDialog('B')" icon="pi pi-plus" />
                </InputGroup>
                <small v-if="submited && !payload.brandId" class="text-red-500">{{ t('body.productManagement.brand') }}
                    {{
                        t('body.systemSetting.invalid_data_message') || 'Vui lòng chọn thương hiệu' }}</small>
            </div>
            <div class="col-3 field pb-0 mb-0">
                <label for="">{{ t('body.productManagement.category') }} <sup
                        class="text-red-500 font-bold">*</sup></label>
                <InputGroup>
                    <Dropdown v-model="payload.industryId" optionLabel="name" optionValue="id"
                        :options="dataGlobal.IndustryContent" class="w-full" filter />
                    <Button @click="OpenDialog('I')" icon="pi pi-plus" />
                </InputGroup>
                <small v-if="submited && !payload.industryId" class="text-red-500">{{
                    t('body.productManagement.category')
                }} {{ t('body.systemSetting.invalid_data_message') || 'Vui lòng chọn ngành hàng' }}</small>
            </div>
            <div class="col-3 field py-0 mb-0">
                <label for="">{{ t('body.productManagement.product_type') }} <sup
                        class="text-red-500 font-bold">*</sup></label>
                <InputGroup>
                    <Dropdown v-model="payload.itemTypeId" optionLabel="name" optionValue="id"
                        :options="dataGlobal.ItemTypeContent" class="w-full" filter />
                    <Button @click="OpenDialog('IT')" icon="pi pi-plus" />
                </InputGroup>
                <small v-if="submited && !payload.itemTypeId" class="text-red-500">{{
                    t('body.productManagement.product_type') }} {{ t('body.systemSetting.invalid_data_message')
                    }}</small>
            </div>
            <div class="col-3 field py-0 mb-0">
                <label for="">{{ t('body.productManagement.packaging') }} <sup
                        class="text-red-500 font-bold">*</sup></label>
                <InputGroup>
                    <Dropdown v-model="payload.packingId" optionLabel="name" optionValue="id"
                        :options="dataGlobal.PackingContent" class="w-full" filter />
                    <Button @click="OpenDialog('P')" icon="pi pi-plus"/>
                </InputGroup>
                <small v-if="submited && !payload.packingId" class="text-red-500">{{
                    t('body.productManagement.packaging')
                }} {{ t('body.systemSetting.invalid_data_message') }}</small>
            </div>
            <div class="col field py-0 mb-0 justify-content-between align-items-end">
                <label>{{ t('body.systemSetting.status') }} <sup class="text-red-500 font-bold"></sup></label>
                <div class="flex gap-3 p-2">
                    <RadioButton v-model="payload.isActive" inputId="ingredient1" :value="true" />
                    <label for="ingredient1">{{ t('body.promotion.status_active') }}</label>
                    <RadioButton v-model="payload.isActive" inputId="ingredient2" :value="false" />
                    <label for="ingredient2">{{ t('body.promotion.status_inactive') }}</label>
                </div>
            </div>
            <div class="col-12 field pb-0 mb-0">
                <label>{{ t('body.productManagement.description_column') }}</label>
                <Editor v-model="payload.note" editorStyle="min-height: 140px;" />
            </div>
            <div class="col-12 field pb-0 mb-0">
                <FileUpload name="images[]" url="/api/upload" @select="onSelectFile($event)" :multiple="true"
                    accept="image/*" :maxFileSize="1000000">
                    <template #header="{ chooseCallback }">
                        <div class="flex gap-2 justify-content-between align-items-center w-full">
                            <div class="font-bold">{{ t('body.systemSetting.choose_file') }}</div>
                            <Button @click="chooseCallback()" icon="pi pi-images"
                                :label="t('body.systemSetting.choose_file')"/>
                        </div>
                    </template>

                    <template #content="{ files, removeFileCallback }">
                        <div class="mb-3" v-if="payload.id && payload.itM1.length > 0">
                            <div class="flex flex-column p-0 gap-3">
                                <div v-for="item of payload.itM1.filter((el) => el.status != 'D' && el.id)"
                                    :key="item.id"
                                    class="m-0 p-3 flex border-1 surface-border justify-content-between align-items-center gap-3">
                                    <div class="flex-grow-1 flex">
                                        <img role="presentation" :alt="item.fileName" :src="item.filePath" width="100"
                                            height="100" />
                                        <div class="px-3">
                                            <span class="font-semibold text-lg">{{ item.fileName }}</span>
                                            <br />
                                            <Tag class="mt-2" :value="t('client.uploaded_label')" />
                                        </div>
                                    </div>
                                    <Button icon="pi pi-times" @click="item.status = 'D'" severity="danger" />
                                </div>
                            </div>
                        </div>

                        <div v-if="files.length > 0">
                            <div class="flex flex-column p-0 gap-3">
                                <div v-for="(file, index) of files" :key="file.name + file.type + file.size"
                                    class="m-0 p-3 flex border-1 surface-border justify-content-between align-items-center gap-3">
                                    <div class="flex-grow-1 flex">
                                        <img role="presentation" :alt="file.name" :src="file.objectURL" width="100"
                                            height="100" />
                                        <div class="px-3">
                                            <span class="font-semibold text-lg">{{ file.name }}</span>
                                            <div>{{ formatSize(file.size) }}</div>
                                            <Tag class="mt-2" :value="t('body.productManagement.pending_label')"
                                                severity="warning" />
                                        </div>
                                    </div>
                                    <Button icon="pi pi-times"
                                        @click="onRemoveTemplatingFile(file, removeFileCallback, index)"
                                        severity="danger" />
                                </div>
                            </div>
                        </div>
                    </template>

                    <template #empty>
                        <p class="p-6 text-center">{{ t('client.drag_drop_image') }}</p>
                    </template>
                </FileUpload>
            </div>
        </div>
        <template #footer>
            <Button @click="visible = false" :label="t('body.OrderList.close')" severity="secondary"/>
            <Button :loading="loading1" :label="t('body.systemSetting.save_button')" @click="onClickSaveItem"/>
        </template>
    </Dialog>

    <Dialog v-model:visible="visibleProdBrand" header="Thêm thương hiệu " :style="{ width: '25%' }" position="top"
        :modal="true" :draggable="false" @hide="onCloseSubDlg">
        <div class="card">
            <div class="grid">
                <div class="col-12">
                    <div class="flex flex-column gap-2">
                        <label>{{ t('body.productManagement.brand_code_label') }}<sup
                                class="text-red-500">*</sup></label>
                        <InputText v-model="subFormState.code"
                            :placeholder="t('body.productManagement.enter_brand_code')"
                            :invalid="errMgSubForm['thương hiệu'].code" />
                        <small v-if="errMgSubForm['thương hiệu'].code" class="text-red-500">{{ errMgSubForm['thương hiệu'].code }}</small>
                    </div>
                </div>
                <div class="col-12">
                    <div class="flex flex-column gap-2">
                        <label>{{ t('body.productManagement.brand_name_label') }}<sup
                                class="text-red-500">*</sup></label>
                        <InputText v-model="subFormState.name"
                            :placeholder="t('body.productManagement.enter_brand_name')"
                            :invalid="errMgSubForm['thương hiệu'].name" />
                        <small v-if="errMgSubForm['thương hiệu'].name" class="text-red-500">{{ errMgSubForm['thương hiệu'].name }}</small>
                    </div>
                </div>
            </div>
        </div>
        <template #footer>
            <Button :label="t('body.OrderList.close')" icon="pi pi-times" outlined @click="visibleProdBrand = false" />
            <Button :label="t('body.productManagement.add_new_button')" icon="pi pi-check" @click="SaveBrand()" />
        </template>
    </Dialog>

    <Dialog v-model:visible="visibleProdIndustry" header="Thêm ngành hàng " :style="{ width: '25%' }" position="top"
        :modal="true" :draggable="false" @hide="onCloseSubDlg">
        <div class="card">
            <div class="grid">
                <div class="col-12">
                    <div class="flex flex-column gap-2">
                        <label>{{ t('body.productManagement.industry_code_label') }}<sup
                                class="text-red-500">*</sup></label>
                        <InputText v-model="subFormState.code"
                            :placeholder="t('body.productManagement.enter_industry_code')"
                            :invalid="errMgSubForm['ngành hàng'].code" />
                        <small v-if="errMgSubForm['ngành hàng'].code" class="text-red-500">{{ errMgSubForm['ngành hàng'].code }}</small>
                    </div>
                </div>
                <div class="col-12">
                    <div class="flex flex-column gap-2">
                        <label>{{ t('body.productManagement.industry_name_label') }}<sup
                                class="text-red-500">*</sup></label>
                        <InputText v-model="subFormState.name"
                            :placeholder="t('body.productManagement.enter_industry_name')"
                            :invalid="errMgSubForm['ngành hàng'].name" />
                        <small v-if="errMgSubForm['ngành hàng'].name" class="text-red-500">{{ errMgSubForm['ngành hàng'].name }}</small>
                    </div>
                </div>
            </div>
        </div>
        <template #footer>
            <Button :label="t('body.OrderList.close')" icon="pi pi-times" outlined
                @click="visibleProdIndustry = false" />
            <Button :label="t('client.confirm')" icon="pi pi-check" @click="SaveIndustry()" />
        </template>
    </Dialog>

    <Dialog v-model:visible="visibleItmType" header="Thêm loại sản phẩm" :style="{ width: '25%' }" position="top"
        :modal="true" :draggable="false" @hide="onCloseSubDlg">
        <div class="card">
            <div class="grid">
                <div class="col-12">
                    <div class="flex flex-column gap-2">
                        <label>{{ t('body.productManagement.item_type_code_label') }}<sup
                                class="text-red-500">*</sup></label>
                        <InputText v-model="subFormState.code"
                            :placeholder="t('body.productManagement.enter_itemtype_code')"
                            :invalid="errMgSubForm['loại sản phẩm'].code" />
                        <small v-if="errMgSubForm['loại sản phẩm'].code" class="text-red-500">{{ errMgSubForm['loại sản phẩm'].code }}</small>
                    </div>
                </div>
                <div class="col-12">
                    <div class="flex flex-column gap-2">
                        <label>{{ t('body.productManagement.item_type_name_label') }}<sup
                                class="text-red-500">*</sup></label>
                        <InputText v-model="subFormState.name"
                            :placeholder="t('body.productManagement.enter_itemtype_name')"
                            :invalid="errMgSubForm['loại sản phẩm'].name" />
                        <small v-if="errMgSubForm['loại sản phẩm'].name" class="text-red-500">{{ errMgSubForm['loại sản phẩm'].name }}</small>
                    </div>
                </div>
            </div>
        </div>
        <template #footer>
            <Button :label="t('body.OrderList.close')" icon="pi pi-times" outlined @click="visibleItmType = false" />
            <Button :label="t('client.confirm')" icon="pi pi-check" @click="SaveItmType()" />
        </template>
    </Dialog>

    <Dialog v-model:visible="visiblePacking" header="Thêm quy cách bao bì" :style="{ width: '25%' }" position="top"
        :modal="true" :draggable="false" @hide="onCloseSubDlg">
        <div class="card">
            <div class="grid">
                <div class="col-12">
                    <div class="flex flex-column gap-2">
                        <label>{{ t('body.productManagement.packing_code_label') }}<sup
                                class="text-red-500">*</sup></label>
                        <InputText v-model="subFormState.code"
                            :placeholder="t('body.productManagement.enter_packing_code')"
                            :invalid="errMgSubForm['quy cách bao bì'].code" />
                        <small v-if="errMgSubForm['quy cách bao bì'].code" class="text-red-500">
                            {{ errMgSubForm['quy cách bao bì'].code }}
                        </small>
                    </div>
                </div>
                <div class="col-12">
                    <div class="flex flex-column gap-2">
                        <label>{{ t('body.productManagement.packing_name_label') }}<sup
                                class="text-red-500">*</sup></label>
                        <InputText v-model="subFormState.name"
                            :placeholder="t('body.productManagement.enter_packing_name')"
                            :invalid="errMgSubForm['quy cách bao bì'].name" />
                        <small v-if="errMgSubForm['quy cách bao bì'].name" class="text-red-500">
                            {{ errMgSubForm['quy cách bao bì'].name }}
                        </small>
                    </div>
                </div>
            </div>
        </div>
        <template #footer>
            <Button :label="t('body.OrderList.close')" icon="pi pi-times" outlined @click="visiblePacking = false" />
            <Button :label="t('client.confirm')" icon="pi pi-check" @click="SavePacking()" />
        </template>
    </Dialog>
    <ConfirmDialog>
        <template #message="slotProps">
            <div class="flex flex-column w-full gap-3">
                <div class="flex align-items-center justify-content-center gap-5">
                    <i :class="slotProps.message.icon" class="text-6xl text-primary-500"></i>
                    <div>{{ t('body.OrderApproval.confirm') }}</div>
                </div>
                <ul class="m-0">
                    <li class="py-1">{{ t('body.productManagement.product_code_column') }}: {{
                        slotProps.message.message.code }}</li>
                    <li class="py-1">{{ t('body.productManagement.product_name_column') }}: {{
                        slotProps.message.message.name }}</li>
                </ul>
            </div>
        </template>
    </ConfirmDialog>
</template>

<script setup>
import { ref, onBeforeMount, reactive, watch } from 'vue';
import API from '@/api/api-main';
import { useConfirm } from 'primevue/useconfirm';
import { useGlobal } from '@/services/useGlobal';
import PromotionProducts from './promotionProducts.vue';
import { useI18n } from 'vue-i18n';
import { usePrimeVue } from 'primevue/config';
import Dropdown from 'primevue/dropdown';
const { t } = useI18n();

const loadingSync = ref(false);
const onClickSync = () => {
    loadingSync.value = true;
    API.add('Item/sync')
        .then(() => {
            toast.add({
                severity: 'success',
                summary: t('body.systemSetting.success_label'),
                detail: t('body.sampleRequest.customer.sync_button'),
                life: 3000
            });
        })
        .catch((error) => {
            toast.add({
                severity: 'error',
                summary: t('body.report.error_occurred_message'),
                detail: t('body.report.error_occurred_message'),
                life: 3000
            });
        })
        .finally(() => {
            loadingSync.value = false;
        });
};

const errMgSubForm = reactive({
    'thương hiệu': {
        name: null,
        code: null
    },
    'ngành hàng': {
        name: null,
        code: null
    },
    'loại sản phẩm': {
        name: null,
        code: null
    },
    'quy cách bao bì': {
        name: null,
        code: null
    }
});

const onCloseSubDlg = () => {
    Object.keys(errMgSubForm).forEach((key) => {
        errMgSubForm[key].name = null;
        errMgSubForm[key].code = null;
    });
};

const validateSubForm = (form) => {
    if (!subFormState.value.code?.trim()) errMgSubForm[form].code = `Hãy nhập mã ${form}`;
    if (!subFormState.value.name?.trim()) errMgSubForm[form].name = `Hãy nhập tên ${form}`;
    return subFormState.value.code?.trim() && subFormState.value.name?.trim();
};

const filterLayer = reactive({
    search: null,
    brand: [],
    industry: [],
    itemtype: [],
    packing: []
});

var timeLimiter = null;
const generateQuery = () => {
    let arr = [];
    Object.keys(filterLayer).forEach((key) => {
        if (filterLayer[key]) {
            if (typeof filterLayer[key] == 'object') {
                if (filterLayer[key].length > 0) arr.push(`${key}=,${filterLayer[key].toString()},`);
            } else if (key == 'search') {
                arr.push(`${key}=${encodeURIComponent(filterLayer[key])}`);
            }
        }
    });
    return arr.join('&');
};
const onFilterData = () => {
    clearTimeout(timeLimiter);
    timeLimiter = setTimeout(async () => {
        try {
            paginator.page = 0;
            const res = await API.get(`Item?Page=${paginator.page + 1}&PageSize=${paginator.rows}&${generateQuery()}`);
            Object.assign(data, res.data);
        } catch (error) {
            console.error(error);
        }
    }, 1000);
};

watch(filterLayer, () => {
    onFilterData();
    0;
});

const isFilter = ref(false);
const onClickEditData = async (item) => {
    loading.value = true;
    try {
        const res = await API.get('Item/' + item.id);
        payload.value = res.data;
        loading.value = false;
        visible.value = true;
    } catch (e) {

        loading.value = false;
    }
};

const { toast, FunctionGlobal } = useGlobal();
const confirm = useConfirm();
const onClickDeleteItem = (item) => {
    confirm.require({
        message: {
            code: item.itemCode,
            name: item.itemName
        },
        header: t('body.OrderApproval.confirm'),
        icon: 'pi pi-exclamation-triangle',
        rejectClass: 'p-button-secondary',
        rejectlabel: t('body.status.DONG'),
        acceptClass: 'p-button-danger',
        acceptLabel: t('body.OrderList.delete'),
        accept: async () => {
            try {
                await API.delete('Item/' + item.id);
                toast.add({
                    severity: 'success',
                    summary: t('body.systemSetting.success_label'),
                    detail: t('body.productManagement.deleted_success_detail'),
                    life: 3000
                });
                isFilter.value = !isFilter.value;
                GetItemsData();
            } catch (error) {
                toast.add({
                    severity: 'error',
                    summary: t('body.report.error_occurred_message'),
                    detail: error.response.data.errors,
                    life: 3000
                });
            }
        },
        reject: () => { }
    });
};

const paginator = reactive({
    page: 0,
    rows: 10,
    rowsPerPageOptions: Array.from({ length: 10 }, (_, i) => (i + 1) * 10)
});

const changePage = async (event) => {
    paginator.page = event.page;
    paginator.rows = event.rows;
    await GetItemsData(filterQuery);
};

const data = reactive({
    items: [],
    total: null,
    skip: 0,
    limit: 10
});

const errorMsg = reactive({
    itemCode: null,
    itemName: null
});

const onCloseDialog = () => {
    Object.keys(errorMsg).forEach((key) => {
        errorMsg[key] = null;
    });
    submited.value = false;
    payload.value = { ...defaultPayload };
    images.value = [];
};
const submited = ref(false);
const validatePayload = () => {
    const { itemCode, itemName, ugpEntry, price, taxGroupsId, brandId, industryId, itemTypeId, packingId } = payload.value;

    // Kiểm tra xem tất cả các trường bắt buộc có giá trị hay không
    if (!itemCode || !itemName || !ugpEntry || price === null || price < 0 || !taxGroupsId || !brandId || !industryId || !itemTypeId || !packingId) {
        return false;
    }
    return true;
};
const loading1 = ref(false);
const onClickSaveItem = async () => {
    loading1.value = true;
    submited.value = true;
    if (!validatePayload()) {
        loading1.value = false;
        return;
    }

    let formData = new FormData();
    if (payload.value.id == 0) {
        images.value.forEach((file) => {
            payload.value.itM1.push({
                id: 0,
                fileName: file.name,
                status: 'A'
            });
            formData.append('images', file);
        });
        formData.append('item', JSON.stringify(payload.value));

        try {
            const res = await API.add('Item/add', formData);
            FunctionGlobal.$notify('S', 'Tìm sản phẩm thành công', toast);
            GetItemsData();
            visible.value = false;
            submited.value = false;
        } catch (e) {
            FunctionGlobal.$notify('E', e.response.data.errors, toast);

            if (e.response.status == 400) {
                errorMsg.itemCode = e.response.data.errors;
            }
        }
    } else {
        let { brand, industry, itemType, packing, ougp, taxGroups, ..._payload } = payload.value;
        images.value.forEach((file) => {
            _payload.itM1.push({
                id: 0,
                fileName: file.name,
                status: 'A'
            });
            formData.append('images', file);
        });
        formData.append('item', JSON.stringify(_payload));
        loading1.value = true;
        try {
            await API.update('Item/' + _payload.id, formData);
            FunctionGlobal.$notify('S', 'Cập nhật sản phẩm thành công', toast);
            GetItemsData();
            visible.value = false;
        } catch (e) {
            FunctionGlobal.$notify('E', e.response.data.errors, toast);

        }
    }
    isFilter.value = !isFilter.value;
    loading1.value = false;
};

const visible = ref(false);
const dataGlobal = ref({
    Taxs: [],
    UnitGroups: [],
    BrandContent: [],
    IndustryContent: [],
    ItemTypeContent: [],
    PackingContent: []
});

const visibleProdBrand = ref(false);
const visibleProdIndustry = ref(false);
const visibleItmType = ref(false);
const visiblePacking = ref(false);
const subFormState = ref({});
const defaultPayload = {
    id: 0,
    itemCode: '',
    itemName: '',
    ugpEntry: '',
    note: '',
    price: null,
    itM1: [
        // {
        //     id: 0,
        //     fileName: "",
        //     filePath: "",
        // }
    ],
    brandId: '',
    industryId: '',
    itemTypeId: '',
    packingId: '',
    isActive: true,
    taxGroupsId: ''
};
const payload = ref({ ...defaultPayload });
const loading = ref(false);
const GetItemsData = async (query = null) => {
    // debugger
    try {
        // Get data from API
        loading.value = true;
        let url = `Item`;
        if (query) {
            url += query + `&Page=${paginator.page + 1}&PageSize=${paginator.rows}`;
        } else {
            url += `?Page=${paginator.page + 1}&PageSize=${paginator.rows}`;
        }
        const response = await API.get(url);
        Object.assign(data, response.data);
        submited.value = false;
    } catch (error) {
        FunctionGlobal.$notify('E', e.message, toast);
    }
    loading.value = false;
};

onBeforeMount(async () => {
    await GetItemsData();
    GetUnitGroupData();
    GetTaxData();
    GetBrandContent();
    GetIndustryContent();
    GetItemType();
    GetPacking();
});

var filterQuery = null;
const onChangeFilter = async (query) => {
    paginator.page = 0;
    filterQuery = query;
    await GetItemsData(query);
};

const SaveBrand = async () => {
    if (validateSubForm('thương hiệu'))
        try {
            const res = await API.add('Brand/add', subFormState.value);
            if (res.status == 200) {
                visibleProdBrand.value = false;
                subFormState.value = { id: 0, code: '', name: '' };
                await GetBrandContent();
                FunctionGlobal.$notify('S', t('body.productManagement.brand_added_success'), toast);
            }
        } catch (e) {
            FunctionGlobal.$notify('E', e.response.data.errors, toast);
        }
};

const OpenDialog = (type) => {
    if (type == 'P') {
        subFormState.value = { id: 0, code: '', name: '' };
        visiblePacking.value = true;
    }
    if (type == 'I') {
        subFormState.value = { id: 0, code: '', name: '' };
        visibleProdIndustry.value = true;
    }
    if (type == 'B') {
        subFormState.value = { id: 0, code: '', name: '' };
        visibleProdBrand.value = true;
    }
    if (type == 'IT') {
        subFormState.value = { id: 0, code: '', name: '' };
        visibleItmType.value = true;
    }
};

const GetUnitGroupData = async () => {
    try {
        const res = await API.get('OUGP?skip=0&limit=10000');
        if (res.data) {
            dataGlobal.value.UnitGroups = res.data?.items ? res.data.items : res.data;
        }
    } catch (e) {
        FunctionGlobal.$notify('E', e.response.data.errors, toast);
    }
};

const GetTaxData = async () => {
    try {
        const res = await API.get('TaxGroups/getall');
        if (res.data) {
            dataGlobal.value.Taxs = res.data?.items ? res.data.items : res.data;
        }
    } catch (e) {
        FunctionGlobal.$notify('E', e.response.data.errors, toast);
    }
};

const GetBrandContent = async () => {
    try {
        const res = await API.get('Brand/getall');
        if (res.data) {
            dataGlobal.value.BrandContent = res.data?.items ? res.data.items : res.data;
        }
    } catch (e) {
        FunctionGlobal.$notify('E', e.response.data.errors, toast);
    }
};
const SaveIndustry = async () => {
    if (validateSubForm('ngành hàng'))
        try {
            const res = await API.add('Industry/add', subFormState.value);
            if (res.status == 200) {
                visibleProdIndustry.value = false;
                subFormState.value = { id: 0, code: '', name: '' };
                GetBrandContent();
                FunctionGlobal.$notify('S', t('body.productManagement.industry_added_success'), toast);
            }
        } catch (e) {
            FunctionGlobal.$notify('E', e.response.data.errors, toast);
        }
};
const GetIndustryContent = async () => {
    try {
        const res = await API.get('Industry/getall');
        if (res.data) {
            dataGlobal.value.IndustryContent = res.data?.items ? res.data.items : res.data;
        }
    } catch (e) {
        FunctionGlobal.$notify('E', e.response.data.errors, toast);
    }
};
const SaveItmType = async () => {
    if (validateSubForm('loại sản phẩm'))
        try {
            const res = await API.add(`ItemType/add`, subFormState.value);
            if (res.data) {
                subFormState.value = { id: 0, code: '', name: '' };
                visibleItmType.value = false;
                GetItemType();
                FunctionGlobal.$notify('S', t('body.productManagement.itemtype_added_success'), toast);
            }
        } catch (error) {
            FunctionGlobal.$notify('E', error.response.data.errors, toast);
        }
};
const GetItemType = async () => {
    try {
        const res = await API.get('ItemType/getall');
        if (res.data) {
            dataGlobal.value.ItemTypeContent = res.data?.items ? res.data.items : res.data;

        }
    } catch (e) {
        FunctionGlobal.$notify('E', e.response.data.errors, toast);
    }
};
const SavePacking = async () => {
    if (validateSubForm('quy cách bao bì'))
        try {
            const res = await API.add(`Packing/add`, subFormState.value);
            if (res.data) {
                subFormState.value = { id: 0, code: '', name: '' };
                visiblePacking.value = false;
                FunctionGlobal.$notify('S', t('body.productManagement.packing_added_success'), toast);
            }
        } catch (e) {
            FunctionGlobal.$notify('E', e.response.data.errors, toast);
        }
};
const GetPacking = async () => {
    try {
        const res = await API.get('Packing/getall');
        if (res.data) {
            dataGlobal.value.PackingContent = res.data?.items ? res.data.items : res.data;
        }
    } catch (e) {
        FunctionGlobal.$notify('E', e.response.data.errors, toast);
    }
};

const $primevue = usePrimeVue();

const images = ref([]);

const onSelectFile = (e) => {

    images.value = e.files;
};

const onRemoveTemplatingFile = (file, removeFileCallback, index) => {
    removeFileCallback(index);
    images.value.splice(index, 1);
};

const formatSize = (bytes) => {
    const k = 1024;
    const dm = 3;
    const sizes = $primevue.config.locale.fileSizeTypes;

    if (bytes === 0) {
        return `0 ${sizes[0]}`;
    }

    const i = Math.floor(Math.log(bytes) / Math.log(k));
    const formattedSize = parseFloat((bytes / Math.pow(k, i)).toFixed(dm));

    return `${formattedSize} ${sizes[i]}`;
};

const autoCompleteData = reactive({
    ugp: []
});

const onCompleteUGP = ({ query }) => {
    autoCompleteData.ugp = dataGlobal.value.UnitGroups.filter((el) => el.ugpName?.toLowerCase().includes(query.toLowerCase()) || el.ugpCode?.toLowerCase().includes(query.toLowerCase()));
};
</script>

<style scoped></style>
