<script setup>
import Dropdown from "primevue/dropdown";
import { ref, onMounted, reactive, markRaw, computed } from "vue";
import API from "@/api/api-main";
import { useI18n } from 'vue-i18n';
const { t } = useI18n();

const condition = ref({
    region: [],
    industry: [],
    itemType: [],
    size: [],
});

// O(1) Map lookup thay vì O(n) .find() trên 10k items mỗi lần render
const regionMap = computed(() => new Map(condition.value.region.map(r => [r.id, r])));
const industryMap = computed(() => new Map(condition.value.industry.map(i => [i.id, i])));
const props = defineProps({
    setup: {
        API: {
            type: Object,
            required: true,
        },
        modelStates: {
            type: Object,
            required: true,
        },
        toast: {
            type: Object,
            required: true,
        },
    },
});
const enableEdit = ref(false);
const DATA = reactive({
    BPArea: [],
    BPSize: [],
});

const selection = reactive({
    area: [],
    size: [],
});

const modelStates = reactive({
    isAllBPArea: null,
    bpArea: null,
    isAllBPSize: null,
    bpSize: null,
});

const payLoad = ref();

const initData = () => {
    payLoad.value = [
        {
            id: 0,
            regionId: null,
            areaId: null,
            industryId: null,
            brandIds: [],
            bpSizeIds: [],
        },
    ];
};
initData();
const onClickEnableEdit = () => {
    enableEdit.value = true;
};

const onClickCancelEdit = () => {
    enableEdit.value = false;
};

const initialComponent = () => {
    props.setup.API.get("BPArea")
        .then((res) => {
            DATA.BPArea = res.data;
        })
        .catch((error) => {
            props.setup.toast.add({
                severity: "error",
                summary: t('body.report.error_occurred_message'),
                detail: error.message,
                life: 3000,
            });
        });
    props.setup.API.get("BPSize/getall")
        .then((res) => {
            DATA.BPSize = res.data;
        })
        .catch((error) => {
            props.setup.toast.add({
                severity: "error",
                summary: t('body.report.error_occurred_message'),
                detail: error.message,
                life: 3000,
            });
        });

    Object.keys(modelStates).forEach((key) => {
        modelStates[key] = props.setup.modelStates[key];
    });
};
const getValueCondition = async () => {
    const endpoints = {
        region: "regions?skip=0&limit=10000",
        industry: "Industry/getallHiarchy",
        size: "BPSize/getall",
    };
    try {
        const conditionData = {};
        for (const [key, endpoint] of Object.entries(endpoints)) {
            const response = await API.get(endpoint);
            // markRaw + freeze: Vue không proxy, V8 tối ưu bộ nhớ cho object bất biến
            conditionData[key] = Object.freeze(markRaw(response.data?.items || response.data || []));
        }
        return conditionData;
    } catch (error) {
        return { region: [], industry: [], size: [] };
    }
};

const Save = async () => {
    try {
        const dataNew = payLoad.value.filter((e) => !e.id);
        const dataUpdate = payLoad.value.filter((e) => e.id && e.status == "U");
        const dataDelete = payLoad.value.filter((e) => e.id && e.status == "D");

        if (dataDelete.length) {
            await API.delete(
                `Customer/${props.setup.modelStates.id}/classify`,
                dataDelete.map((e) => e.id)
            );
        }
        if (dataNew.length) {
            await API.add(`Customer/${props.setup.modelStates.id}/classify`, dataNew);
        }
        if (dataUpdate.length) {
            await API.update(
                `Customer/${props.setup.modelStates.id}/classify`,
                dataUpdate
            );
        }
        props.setup.toast.add({
            severity: "success",
            summary: t('body.systemSetting.success_label'),
            detail: t('body.systemSetting.update_account_success_message'),
            life: 3000,
        });
    } catch (error) {
        props.setup.toast.add({
            severity: "error",
            summary: t('body.report.error_occurred_message'),
            detail: error,
            life: 3000,
        });
    } finally {
        enableEdit.value = false;
    }
};

const addRow = () => {
    const data = {
        id: 0,
        regionId: null,
        areaId: null,
        industryId: null,
        brandIds: [],
        bpSizeIds: [],
    };
    payLoad.value.push(data);
};

const removeRow = (data, index) => {
    if (data.id) {
        data.status = "D";
        return;
    }
    payLoad.value.splice(index, 1);
};

const getIndustry = (data) => {
    // Dùng industryMap (O(1)) thay vì .find() O(n) trên toàn bộ mảng
    const brands = industryMap.value.get(data.industryId)?.brands;
    if (!brands) return [];
    const brandIdSet = new Set(data.brandIds);
    const itemTypeMap = new Map();
    for (const brand of brands) {
        if (brandIdSet.has(brand.id)) {
            for (const itemType of brand.itemTypes) {
                itemTypeMap.set(itemType.id, itemType);
            }
        }
    }
    return Array.from(itemTypeMap.values());
};

// Eager load condition data ngay từ mount để Dropdown trong READ mode có options
// → map được regionId/industryId/bpSizeIds → tên hiển thị (Dropdown disabled vẫn cần options).
// Các endpoint này nằm trong whitelist của referenceDataCache → lần 2 trở đi chỉ là 304
// Not Modified (vài chục byte). Khi admin update master data, SignalR push → invalidate cache.
const loadConditionData = async () => {
    const dataCondition = await getValueCondition();
    condition.value.region = dataCondition.region;
    condition.value.industry = dataCondition.industry;
    condition.value.size = dataCondition.size;
};

onMounted(() => {
    initialComponent();
    loadConditionData();
    // JSON.parse/stringify: clone sạch, tránh lodash merge traverse Vue reactive proxy
    const classify = props.setup.modelStates.classify;
    if (classify?.length) {
        payLoad.value = JSON.parse(JSON.stringify(classify));
    }
});

const changeUpdate = (data) => {
    if (data.id) data.status = "U";
};
</script>
<template>
    <div id="section2" class="card">
        <div class="flex justify-content-between mb-3">
            <div class="py-2 text-green-700 text-xl font-semibold">
                {{ t('body.sampleRequest.customer.classification_title') }}</div>
            <div class="flex gap-3">
                <Button v-if="!enableEdit" @click="onClickEnableEdit" icon="pi pi-pencil"
                    :label="t('body.OrderList.edit')" text />
                <template v-else>
                    <Button @click="onClickCancelEdit" icon="pi pi-times" :label="t('body.OrderList.close')"
                        severity="secondary" text />
                    <Button icon="pi pi-save" :label="t('body.systemSetting.save_button')" @click="Save()" />
                </template>
            </div>
        </div>
        <hr class="m-0" />
        <DataTable :value="payLoad.filter((e) => e.status != 'D')" tableStyle="min-width: 50rem">
            <Column header="#">
                <template #body="{ index }">{{ index + 1 }}</template>
            </Column>
            <Column field="name" :header="t('client.area')">
                <template #body="{ data }">
                    <Dropdown @change="changeUpdate(data)" class="w-full" filter :placeholder="t('client.select_area')"
                        :options="condition.region" optionValue="id" optionLabel="name" v-model="data.regionId"
                        :disabled="!enableEdit" :virtualScrollerOptions="{ itemSize: 38 }" />
                </template>
            </Column>
            <Column field="category" :header="t('body.sampleRequest.customer.province_column')">
                <template #body="{ data }">
                    <!-- regionMap.get() O(1) thay vì condition.region.find() O(n) -->
                    <Dropdown @change="changeUpdate(data)" class="w-full" filter
                        :placeholder="t('body.sampleRequest.customer.select_province')"
                        :options="regionMap.get(data.regionId)?.areas" optionValue="id"
                        optionLabel="name" v-model="data.areaId" :disabled="!data.regionId || !enableEdit" />
                </template>
            </Column>
            <Column :header="t('body.sampleRequest.customer.category_column')">
                <template #body="{ data }">
                    <Dropdown class="w-full" :options="condition.industry" v-model="data.industryId" optionValue="id"
                        optionLabel="name" filter @change="() => { data.brandIds = []; data.itemTypeIds = []; changeUpdate(data); }"
                        :placeholder="t('body.sampleRequest.customer.select_category')" :disabled="!enableEdit" />
                </template>
            </Column>
            <Column :header="t('client.brand')">
                <template #body="{ data }">
                    <!-- industryMap.get() O(1) thay vì condition.industry.find() O(n) -->
                    <MultiSelect class="w-full"
                        :options="industryMap.get(data.industryId)?.brands" optionValue="id"
                        optionLabel="name" filter :placeholder="t('client.select_brand')" v-model="data.brandIds"
                        :disabled="!data.industryId || !enableEdit" :maxSelectedLabels="2"
                        selectedItemsLabel="Đã chọn {0} thương hiệu" @change="changeUpdate(data)" />
                </template>
            </Column>
            <Column :header="t('body.sampleRequest.commitment.table_header_product_type')">
                <template #body="{ data }">
                    <MultiSelect class="w-full" :options="getIndustry(data)" optionValue="id" optionLabel="name" filter
                        :placeholder="t('client.select_product_type')" v-model="data.itemTypeIds"
                        :disabled="!data.industryId || !enableEdit" :maxSelectedLabels="2"
                        selectedItemsLabel="Đã chọn {0} loại sản phẩm" @change="changeUpdate(data)" />
                </template>
            </Column>
            <Column :header="t('body.sampleRequest.customer.scale_column')">
                <template #body="{ data }">
                    <MultiSelect @change="changeUpdate(data)" class="w-full" :options="condition.size" optionValue="id"
                        optionLabel="name" filter :placeholder="t('body.sampleRequest.customer.select_scale')"
                        v-model="data.bpSizeIds" :maxSelectedLabels="2" selectedItemsLabel="Đã chọn {0} quy mô"
                        :disabled="!enableEdit" />
                </template>
            </Column>

            <Column>
                <template #body="{ index, data }">
                    <Button v-if="enableEdit" text icon="pi pi-trash" severity="danger"
                        @click="removeRow(data, index)" />
                </template>
            </Column>
            <template #footer>
                <Button outlined v-if="enableEdit" icon="pi pi-plus" @click="addRow()" :label="t('client.add_line')" />
            </template>
        </DataTable>
    </div>
</template>
