<script setup>
import { ref, onBeforeMount } from 'vue';
import API from '@/api/api-main';
import SelectDistributor from './components/SelectDistributor.vue';
import { useRouter, useRoute } from 'vue-router';
import { useGlobal } from '@/services/useGlobal';
import { useI18n } from 'vue-i18n';
import format from '@/helpers/format.helper';
import { useHierarchyStore } from "@/Pinia/hierarchyStore";

const { t } = useI18n();
const { toast } = useGlobal();
const checkboxTime = ref(false);
const checkboxNoti = ref(false);
const dataCallApi = ref({
    id: 0,
    name: '',
    fromDate: format.formatDate(new Date()),
    toDate: format.formatDate(new Date()),
    extendedToDate: null,
    isAllCustomer: true,
    notifyBeforeDays: 0,
    isActive: true,
    note: '',
    customers: [],
    lines: [
        {
            id: 0,
            brandIds: [],
            industryIds: [],
            itemType: [],
            packingIds: [],
            point: 0
        }
    ]
});
const router = useRouter();
const route = useRoute();
const submited = ref(false);
const loading = ref(false);
const Hierarchy = ref();
const ItemData = ref([]);
const lineAddProduct = ref();
const selectedItemType = ref([]);
const hierarchyStore = useHierarchyStore();

const ConvertCustomerLocal = async (data) => {
    if (Array.isArray(data) && data.length > 0) {
        const customer = [
            {
                type: data[0]?.type || '',
                items: data.map((e) => ({
                    id: e.customerId,
                    cardCode: e.customerCode,
                    cardName: e.customerName
                }))
            }
        ];
        return customer;
    }
    return [{ type: '', items: [] }];
};


const ConvertCustomer = (data) => {
    if (data?.length)
        data = data[0];

    if (data?.items?.length) {
        return data?.items.map((el) => ({
            type: data.type,
            customerId: el.id,
            CustomerCode: el.cardCode || '',
            customerName: el.cardName || el.groupName,
            itemCode: el.itemCode,
            itemId: el.itemId,
            itemName: el.itemName,
            status: 'A'
        }));
    }
    return [];
};
const convertDateCallApi = (date) => {
    const [d, m, y] = date.split('/');
    return new Date(`${y}-${m}-${d}`).toISOString().slice(0, 10);
};

const validate = () => {
    submited.value = true;
    if (!dataCallApi.value.name || !dataCallApi.value.fromDate || !dataCallApi.value.toDate) {
        toast.add({
            severity: 'error',
            summary: t('Notification.input_required'),
            life: 3000
        });
        return false;
    }
    if (dataCallApi.value.lines.length === 0) {
        toast.add({
            severity: 'error',
            summary: t('Notification.input_required_promotion'),
            life: 3000
        });
        return false;
    }

    // Validate từng dòng trong lines
    let hasError = false;
    for (const line of dataCallApi.value.lines) {
        if (!line.brandIds || line.brandIds.length === 0) {
            hasError = true;
        }
        if (!line.industryIds || line.industryIds.length === 0) {
            hasError = true;
        }
        if (!line.itemType || line.itemType.length === 0) {
            hasError = true;
        }

        if (!line.packingIds || line.packingIds.length === 0) {
            if (line.itemType[0]?.itemType !== 'I')
                hasError = true;
        }
        if (!line.point || line.point === 0) {
            hasError = true;
        }
    }

    if (hasError) {
        toast.add({
            severity: 'error',
            summary: t('Notification.input_required_promotion'),
            life: 3000
        });
        return false;
    }

    dataCallApi.value.fromDate = convertDateCallApi(dataCallApi.value.fromDate);
    dataCallApi.value.toDate = convertDateCallApi(dataCallApi.value.toDate);
    if (dataCallApi.value.extendedToDate) dataCallApi.value.extendedToDate = convertDateCallApi(dataCallApi.value.extendedToDate);
    return true;
};

const SavePromotion = async () => {
    if (!validate()) return;
    try {
        if (dataCallApi.value.extendedToDate === '') dataCallApi.value.extendedToDate = null;
        const dataCall = {
            ...dataCallApi.value,
            customers: ConvertCustomer(dataCallApi.value.customers)
        };
        const res = dataCall.id ? await API.update('PointSetup/' + dataCall.id, dataCall) : await API.add('PointSetup', dataCall);
        if (res) {
            toast.add({
                severity: 'success',
                summary: dataCallApi.value.id ? t('client.update_success') : t('PromotionalItems.SetupPurchases.addnewSuccess'),
                life: 3000
            });
            router.back();
        }
    } catch (error) {
        // ApiError: envelope từ backend (có message + errors[] đã được parse)
        // AxiosError: fallback khi endpoint chưa theo format envelope
        const status = error?.statusCode ?? error?.response?.status;
        const detail = error?.errors?.length
            ? error.errors.join('\n')
            : error?.message || t('body.report.error_occurred_message');

        if (status === 403) {
            toast.add({
                severity: 'error',
                summary: t('common.error'),
                detail: t('PromotionalItems.SetupPurchases.youDontAction'),
                life: 3000
            });
        } else {
            toast.add({
                severity: 'error',
                summary: t('common.error'),
                detail,
                life: status === 400 ? 5000 : 3000
            });
        }
    } finally {
        loading.value = false;
    }
};

const getHierarchy = async () => {
    try {
        const res = await API.get('Item/hierarchy?cardId=');
        if (res.data.items) Hierarchy.value = res.data.items;
    } catch (error) {
        console.error(error);
        Hierarchy.value = [];
    }
};

const addNewProduct = () => {
    dataCallApi.value.lines.unshift({
        id: 0,
        brandIds: [],
        industryIds: [],
        itemType: [],
        packingIds: [],
        point: 0
    });
};
onBeforeMount(async () => {
    try {
        await getHierarchy();
        if (route?.params?.id)
            await getById(route?.params?.id);

    } catch (error) {
        console.error(error);
        toast.add({
            severity: 'error',
            summary: t('body.report.error_occurred_message'),
            life: 3000
        });
    } finally {
        loading.value = false;
    }
});
const getById = async (id) => {
    loading.value = true;
    try {
        const res = await API.get(`PointSetup/${id}`);
        if (res.data) {
            let resq = res.data.items;
            dataCallApi.value = {
                ...resq,
                fromDate: format.formatDate(resq.fromDate),
                extendedToDate: format.formatDate(resq.extendedToDate),
                toDate: format.formatDate(resq.toDate)
            };
            dataCallApi.value.customers = await ConvertCustomerLocal(resq.customers);
        }
    } catch (error) {
        console.error(error);
        toast.add({
            severity: 'error',
            summary: t('body.report.error_occurred_message'),
            life: 3000
        });
    }
    loading.value = true;
};
const getDataIndustry = (dataId) => {
    const result = [];
    if (!dataId || !Array.isArray(dataId) || dataId.length === 0 || !Hierarchy.value) {
        return [];
    }
    for (const id of dataId) {
        const brand = Hierarchy.value.find(b => b.brandId === id);
        if (brand && brand.industry) {
            for (const industry of brand.industry) {
                if (!result.some(existingIndustry => existingIndustry.industryId === industry.industryId))
                    result.push(industry);
            }
        } else
            result.push(...brand.industry);
    }
    return result;
};
const visible = ref(false);
const ItemType = ref([]);
const openSelectProduct = async (data, index) => {
    //Set filters cho hierarchy store
    hierarchyStore.setBrandFilter(data.brandIds);
    hierarchyStore.setIndustryFilter(data.industryIds);

    try {
        visible.value = true;
        let dataVal = Hierarchy.value.filter(brand => data.brandIds.includes(brand.brandId));
        let getDataIndustry = [];
        let getDataItemType = [];
        dataVal.map(brand => {
            brand.industry.map(industry => {
                if (data.industryIds.includes(industry.industryId)) {
                    getDataIndustry.push(industry);
                }
            });
        });
        getDataIndustry.map(industry => {
            industry.itemType.map(itemType => {
                if (!getDataItemType.some(existing => existing.itemTypeId === itemType.itemTypeId))
                    getDataItemType.push(itemType);
            });
        });
        if (data.itemType.length > 0 && data.itemType[0].itemType === "I") {
            ItemData.value = data.itemType;
        } else {
            ItemData.value = [];
        }
        ItemType.value = getDataItemType;

        selectedItemType.value = getDataItemType.filter(itemType =>
            data.itemType.some(selectedItem => selectedItem.itemId === itemType.itemTypeId)
        );
        lineAddProduct.value = index;
    } catch (error) {
        console.error('Error in openSelectProduct:', error);
        visible.value = true;
    }
};

const updateSelectedProduct = (data) => {
    for (const el of data) {
        ItemData.value.push({
            id: 0,
            itemType: 'I',
            itemId: el.itemId,
            itemCode: el.itemCode,
            itemName: el.itemName,
            packingId: el.packing.id,
            packingName: el.packing.name,
            point: 0
        });
    }
};
const confirmDataProduct = () => {
    dataCallApi.value.lines[lineAddProduct.value].itemType = ItemData.value;
    dataCallApi.value.lines[lineAddProduct.value].packingIds = [];
    visible.value = false;
};

const confirmDataTypeItem = (data) => {
    ItemData.value = [];
    for (const el of data) {
        ItemData.value.push({
            id: 0,
            fatherId: 0,
            itemType: 'G',
            itemCode: el.itemTypeId.toString(),
            itemId: el.itemTypeId,
            itemName: el.itemTypeName,
            packing: el.packing,
            status: 'A',
            brandId: el.brandId || 0,
            industryIds: el.industryIds || []
        });
    }
    dataCallApi.value.lines[lineAddProduct.value].itemType = ItemData.value;
    dataCallApi.value.lines[lineAddProduct.value].packingIds = [];
    ItemType.value = [];
    selectedItemType.value = [];
    visible.value = false;
};

const renderLabel = (data) => {
    if (!data.length) return '';
    const type = data[0].itemType == 'G' ? t('body.systemSetting.product_types') : t('body.systemSetting.products');
    const text = t('body.productManagement.selected_label') + ` ${data.length} ${type}`;
    return text;
};

const optionPacking = (data) => {
    // Kiểm tra dữ liệu đầu vào: nếu không có itemType hoặc Hierarchy thì trả về mảng rỗng
    if (!data.itemType?.length || !Hierarchy.value) return [];
    const packingMap = new Map();
    const selectedItemIds = new Set(data.itemType.map(item => item.itemId));
    // Duyệt qua tất cả brandIds đã được chọn
    if (data.brandIds?.length) {
        for (const brandId of data.brandIds) {
            // Tìm brand tương ứng trong Hierarchy
            const brand = Hierarchy.value.find(b => b.brandId === brandId);
            if (!brand?.industry) continue;
            // Duyệt qua tất cả industry của brand
            for (const industry of brand.industry) {
                if (!industry.itemType) continue;
                // Duyệt qua tất cả itemType của industry
                for (const type of industry.itemType) {
                    // Kiểm tra nếu itemType được chọn và có packing
                    if (selectedItemIds.has(type.itemTypeId) && type.packing) {
                        // Thêm tất cả packing vào Map
                        for (const packing of type.packing) {
                            packingMap.set(packing.packingId, packing);
                        }
                    }
                }
            }
        }
    }
    return Array.from(packingMap.values());
};
</script>
<template>
    <div class="flex justify-content-between align-items-center mb-4 sticky top-0">
        <h4 class="font-bold m-0">
            {{ dataCallApi.id ? t('PromotionalItems.SetupPurchases.update_promotion_page_title') :
                t('PromotionalItems.SetupPurchases.create_promotion_page_title') }}
        </h4>
        <div class="flex gap-2">
            <Button @click="router.back()" :label="t('body.promotion.back_button')" icon="pi pi-arrow-left"
                severity="secondary" />
            <Button :label="t('body.systemSetting.save_button')" icon="pi pi-save" @click="SavePromotion()" />
        </div>
    </div>
    <div class="grid">
        <div class="col-8">
            <div class="card">
                <h6 class="m-0 font-bold">{{ t('body.promotion.info_section_title') }}</h6>
                <div class="grid mt-2">
                    <div class="col-12 flex flex-column gap-3">
                        <div class="flex justify-content-between">
                            <span>{{ t('body.promotion.promotion_name_label') }} <sup
                                    class="text-red-500">*</sup></span>
                            <InputText :invalid="submited && !dataCallApi.name" v-model="dataCallApi.name" class="w-8">
                            </InputText>
                        </div>
                    </div>
                    <div class="col-12 flex flex-column gap-3">
                        <div class="flex justify-content-between">
                            <span>{{ t('body.promotion.status_column') }}</span>
                            <div class="flex w-8 gap-6">
                                <div class="flex align-items-center">
                                    <RadioButton inputId="A" v-model="dataCallApi.isActive" :value="true" />
                                    <label for="A" class="ml-2">{{ t('body.sampleRequest.customer.active_status')
                                        }}</label>
                                </div>
                                <div class="flex align-items-center">
                                    <RadioButton inputId="I" v-model="dataCallApi.isActive" :value="false" />
                                    <label for="I" class="ml-2">{{ t('body.sampleRequest.customer.inactive_status')
                                        }}</label>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="grid mt-2">
                    <div class="col-12 flex flex-column gap-3">
                        <h6 class="m-0 font-bold">{{ t('body.promotion.applicable_object_section_title') }}</h6>
                        <div class="flex justify-content-between">
                            <div class="flex align-items-center">
                                <RadioButton inputId="All" v-model="dataCallApi.isAllCustomer" :value="true"
                                    name="pizza" />
                                <label for="All" class="ml-2">{{ t('body.promotion.applicable_object_all') }}</label>
                            </div>
                        </div>
                        <div class="flex">
                            <div class="flex w-4 align-items-center">
                                <RadioButton inputId="Cus" v-model="dataCallApi.isAllCustomer" :value="false"
                                    name="pizza" />
                                <label for="Cus" class="ml-2">{{ t('body.promotion.applicable_object_custom') }}</label>
                            </div>
                            <div class="flex w-8 flex-column gap-2">
                                <SelectDistributor v-if="!loading" v-model:selection="dataCallApi.customers"
                                    :disabled="dataCallApi.isAllCustomer" />
                            </div>
                        </div>
                    </div>
                    <div class="col-12 flex flex-column gap-3">
                        <h6 class="m-0 font-bold">{{ t('body.promotion.applicable_time_section_title') }}</h6>
                        <div class="align-items-center flex">
                            <span class="w-4">{{ t('body.promotion.start_date_label') }}<sup
                                    class="text-red-500">*</sup></span>
                            <Calendar showIcon :invalid="submited && !dataCallApi.fromDate"
                                v-model="dataCallApi.fromDate" class="w-8"
                                :placeholder="t('body.promotion.start_date_placeholder')" dateFormat="dd/mm/yy"
                                @update:modelValue="(val) => (dataCallApi.fromDate = format.formatDate(val))">
                            </Calendar>
                        </div>
                        <div class="align-items-center flex">
                            <span class="w-4">{{ t('body.promotion.end_date_label') }}<sup
                                    class="text-red-500">*</sup></span>
                            <Calendar showIcon :invalid="submited && !dataCallApi.toDate"
                                :minDate="format.toDate(dataCallApi.fromDate)" v-model="dataCallApi.toDate"
                                :placeholder="t('body.promotion.end_date_placeholder')" dateFormat="dd/mm/yy"
                                class="w-8" @update:modelValue="(val) => (dataCallApi.toDate = format.formatDate(val))">
                            </Calendar>
                        </div>
                        <div class="align-items-center flex">
                            <span class="w-4">
                                <Checkbox v-model="checkboxTime" binary inputId="ingredient1" @change="
                                    () => {
                                        checkboxTime == false ? (dataCallApi.extendedToDate = null) : (dataCallApi.extendedToDate = format.formatDate(new Date()));
                                    }
                                " />
                                <label for="ingredient1" class="pl-2">{{
                                    t('PromotionalItems.SetupPurchasesPoint.timeOther') }}</label>
                            </span>
                            <Calendar showIcon v-model="dataCallApi.extendedToDate" class="w-8"
                                :disabled="!checkboxTime" :placeholder="t('body.promotion.start_date_placeholder')"
                                :dateFormat="'dd/mm/yy'" :minDate="format.toDate(dataCallApi.toDate)"
                                @update:modelValue="(val) => (dataCallApi.extendedToDate = format.formatDate(val))">
                            </Calendar>
                        </div>
                        <div class="flex align-items-center gap-2">
                            <Checkbox v-model="checkboxNoti" binary inputId="ingredient2" @change="
                                () => {
                                    if (checkboxNoti == false) dataCallApi.notifyBeforeDays = 0;
                                }
                            " />
                            <label for="ingredient2">
                                {{ t('PromotionalItems.SetupPurchasesPoint.SendMes') }}
                                <InputNumber v-model="dataCallApi.notifyBeforeDays" :disabled="!checkboxNoti"
                                    style="width: 60px" />
                                {{ t('PromotionalItems.SetupPurchasesPoint.beforeSendMes') }}
                            </label>
                        </div>
                    </div>
                    <div class="col-12">
                        <h6 class="m-0 font-bold">{{ t('body.PurchaseRequestList.note_label') }}</h6>
                        <div class="flex justify-content-between mt-3">
                            <Textarea v-model="dataCallApi.note" rows="2" class="w-full" />
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <div class="col-4">
            <div class="card">
                <h6 class="m-0 font-bold mb-3">{{ t('PromotionalItems.SetupPurchasesPoint.summary_section_title') }}
                </h6>
                <div style="line-height: 2">
                    <span>{{ t('body.promotion.promotion_name_label') }} : {{ dataCallApi.name }}</span> <br />
                    <span>{{ t('body.sampleRequest.customer.applicable_object_column') }} : {{ dataCallApi.isAllCustomer
                        == true ? t('body.promotion.applicable_object_all') :
                        t('body.promotion.applicable_object_custom') }}</span><br />
                    <span>{{ t('body.promotion.applicable_time_section_title') }} : {{ dataCallApi.fromDate
                        }}</span><br />
                    <span>{{ t('PromotionalItems.SetupPurchasesPoint.pointsExpirationDate') }} : {{ dataCallApi.toDate
                        }}</span>
                    <br />
                    <span v-if="dataCallApi.extendedToDate">{{
                        t('PromotionalItems.SetupPurchasesPoint.timeReservePoints') }} : {{ dataCallApi.extendedToDate
                        }}</span> <br />
                    <span v-if="dataCallApi.notifyBeforeDays">{{
                        t('PromotionalItems.SetupPurchasesPoint.sendAdvanceNotice') }} : {{ dataCallApi.notifyBeforeDays
                        }} {{ t('body.promotion.voucher_expiry_date_label2') }}</span>
                </div>
            </div>
        </div>
        <div class="col-12">
            <div class="card">
                <div class="flex align-items-center justify-content-between">
                    <h5 class="m-0 font-bold">{{ t('body.promotion.promotion') }}</h5>
                    <Button icon="pi pi-plus" class="p-button-rounded p-button-text"
                        style="background-color: #0d733d; color: #fff" @click="addNewProduct()" />
                </div>
                <DataTable :value="dataCallApi.lines" responsiveLayout="scroll" stripedRows>
                    <Column header="#">
                        <template #body="{ index }">
                            {{ index + 1 }}
                        </template>
                    </Column>
                    <Column :header="t('body.productManagement.brand') + ' *'">
                        <template #body="{ data }">
                            <MultiSelect :options="Hierarchy" optionLabel="brandName" optionValue="brandId"
                                class="w-full" :placeholder="t('client.input_brand')" v-model="data.brandIds"
                                :invalid="submited && (!data.brandIds || data.brandIds.length === 0)"
                                @change="() => { data.industryIds = [], data.packingIds = [], data.itemType = [] }" />
                        </template>
                    </Column>
                    <Column :header="t('body.productManagement.category') + ' *'">
                        <template #body="{ data }">
                            <MultiSelect :options="getDataIndustry(data.brandIds)" optionLabel="industryName"
                                optionValue="industryId" class="w-full"
                                :disabled="!data.brandIds || data.brandIds.length === 0"
                                :placeholder="t('body.sampleRequest.customer.select_category')"
                                :invalid="submited && (!data.industryIds || data.industryIds.length === 0)"
                                v-model="data.industryIds"
                                @change="() => { data.packingIds = [], data.itemType = [] }" />
                        </template>
                    </Column>
                    <Column :header="t('body.systemSetting.product_types') + ' *'">
                        <template #body="{ data, index }">
                            <div
                                :class="{ 'disabled-group': !data.brandIds || data.brandIds.length === 0 || !data.industryIds || data.industryIds.length === 0 }">
                                <InputGroup class="w-full" @click="openSelectProduct(data, index)"
                                    @change="data.packingIds = []">
                                    <InputText :placeholder="t('body.promotion.selectProductOrType')"
                                        :disabled="!data.brandIds || data.brandIds.length === 0 || !data.industryIds || data.industryIds.length === 0"
                                        :value="renderLabel(data.itemType)"
                                        :invalid="submited && (!data.itemType || data.itemType.length === 0)"
                                        style="min-width: 15rem;" />
                                    <InputGroupAddon>
                                        <i class="pi pi-list-check"></i>
                                    </InputGroupAddon>
                                </InputGroup>
                            </div>
                        </template>
                    </Column>
                    <Column :header="t('body.home.packaging_column') + ' *'" bodyClass="w-20rem">
                        <template #body="{ data }">
                            <MultiSelect :disabled="!data.itemType || !data.itemType.some((el) => el.itemType !== 'I')"
                                :options="optionPacking(data)" optionLabel="packingName" optionValue="packingId"
                                class="w-20rem" :placeholder="t('PromotionalItems.SetupPurchases.chooseUnit')"
                                :invalid="submited && (!data.packingIds || data.packingIds.length === 0)"
                                v-model="data.packingIds" :maxSelectedLabels="3"
                                :selectedItemsLabel="t('validation.quantity_product_choose') + (data.packingIds?.length || 0) + t('validation.quantity_product_choose2')" />
                        </template>
                    </Column>
                    <Column :header="t('PromotionalItems.SetupPurchasesPoint.promotion_type') + ' *'">
                        <template #body="{ data }">
                            <InputNumber v-model="data.point" class="w-10rem" :minFractionDigits="1" :min="0"
                                :invalid="submited && (!data.point || data.point === 0)" :maxFractionDigits="1" />
                        </template>
                    </Column>
                    <Column :header="t('body.systemSetting.action')" bodyClass="text-left w-10rem">
                        <template #body="{ data, index }">
                            <Button icon="pi pi-copy" class="p-button-rounded p-button-text mr-2"
                                :title="t('body.promotion.copy_button')"
                                @click="dataCallApi.lines.push({ ...data, id: data.id + 1 })" />
                            <Button icon="pi pi-trash" class="p-button-rounded p-button-text" severity="danger"
                                :title="t('body.OrderList.delete')" @click="dataCallApi.lines.splice(index, 1)" />
                        </template>
                    </Column>
                </DataTable>
            </div>
        </div>
    </div>
    <Dialog v-model:visible="visible" modal :header="t('body.sampleRequest.sampleProposal.choose_product_button')"
        :style="{ width: '45rem' }">
        <TabView>
            <TabPanel :header="t('body.home.product_label')">
                <DataTable :value="ItemData">
                    <Column header="#">
                        <template #body="{ index }">{{ index + 1 }}</template>
                    </Column>
                    <Column field="itemName" :header="t('body.home.product_name_column')"></Column>
                    <Column>
                        <template #body="{ index }">
                            <Button icon="pi pi-trash" text severity="danger" @click="ItemData.splice(index, 1)" />
                        </template>
                    </Column>
                </DataTable>
                <div class="flex justify-content-between w-full mt-5">
                    <ProductSelector icon="pi pi-plus" :label="t('body.PurchaseRequestList.find_product_button')"
                        outlined @confirm="updateSelectedProduct($event)" :disabledHeader="true" />
                    <div class="flex justify-content-end gap-2">
                        <Button type="button" :label="t('body.home.cancel_button')" severity="secondary"
                            @click="visible = false" />
                        <Button type="button" :label="t('body.home.confirm_button')" @click="confirmDataProduct" />
                    </div>
                </div>
            </TabPanel>
            <TabPanel :header="t('body.productManagement.product_type')">
                <DataTable :value="ItemType" v-model:selection="selectedItemType" dataKey="itemTypeId" scrollable
                    scrollHeight="400px">
                    <Column selectionMode="multiple" headerStyle="width: 3rem"></Column>
                    <Column field="itemTypeName" :header="t('body.productManagement.typeName')"></Column>
                </DataTable>
                <div class="flex justify-content-end gap-2 mt-5">
                    <Button type="button" :label="t('body.home.cancel_button')" severity="secondary"
                        @click="visible = false" />
                    <Button type="button" :label="t('body.home.confirm_button')"
                        @click="confirmDataTypeItem(selectedItemType)" />
                </div>
            </TabPanel>
        </TabView>
    </Dialog>
    <Loading v-if="loading"></Loading>
</template>

<style scoped>
.p-highlight::before {
    background: var(--green-200);
}

.p-highlight>.p-button-label {
    color: var(--green-800);
}

:deep(.p-inputnumber-input) {
    width: 100%;
}

.disabled-group {
    pointer-events: none;
}

/* Style cho input invalid */
:deep(.p-invalid) {
    border-color: #ef4444 !important;
}

:deep(.p-multiselect.p-invalid) {
    border-color: #ef4444 !important;
}

:deep(.p-inputnumber.p-invalid > .p-inputnumber-input) {
    border-color: #ef4444 !important;
}
</style>
