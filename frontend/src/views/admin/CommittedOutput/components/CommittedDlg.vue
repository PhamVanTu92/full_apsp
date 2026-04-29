<template>
    <div>
        <Dialog v-model:visible="visible" @hide="onHide" :header="t('body.sampleRequest.commitment.page_title')" modal
            class="w-10 md:w-9" style="max-width: 100vw" maximizable>
            <div>
                <div class="card p-3 grid m-0 mb-3">
                    <div class="col-6">
                        <div class="grid">
                            <div class="col-12 pt-0 flex flex-column flex-row gap-2">
                                <div class="font-semibold">{{ t('body.sampleRequest.commitment.code_label') }}</div>
                                <span v-if="isReadOnly" class="flex-grow-1 font-medium">{{ model.committedCode || '—'
                                }}</span>
                                <InputText v-else v-model="model.committedCode" class="flex-grow-1" :disabled="id"
                                    :placeholder="t('body.sampleRequest.sampleProposal.auto_generated_code')" />
                                <small v-if="errMsg.committedCode"> {{ errMsg.committedCode }}</small>
                            </div>
                            <div class="col-12 pt-0 flex flex-column flex-row gap-2">
                                <div class="font-semibold">
                                    {{ t('body.sampleRequest.commitment.name_label') }}
                                    <sup v-if="!isReadOnly" class="font-bold text-red-500">*</sup>
                                </div>
                                <span v-if="isReadOnly" class="font-medium">{{ model.committedName || '—' }}</span>
                                <InputText v-else :invalid="errMsg.committedName ? true : false"
                                    v-model="model.committedName" class="flex-grow-1" autofocus />
                                <small v-if="errMsg.committedName"> {{ errMsg.committedName }}</small>
                            </div>
                            <div class="col-12 pt-0 flex flex-column flex-row gap-2">
                                <div class="font-semibold">
                                    {{ t('body.sampleRequest.commitment.applicable_object_label') }}
                                    <sup v-if="!isReadOnly" class="font-bold text-red-500">*</sup>
                                </div>
                                <span v-if="isReadOnly" class="font-medium">{{ model.cardName || '—' }}</span>
                                <CustomerSelectorId v-else v-model="model.cardId"
                                    :placeholder="model.cardId ? model.cardName : t('body.OrderList.select_customer_placeholder')"
                                    @item-select="onSelectCustomer">
                                </CustomerSelectorId>
                            </div>
                        </div>
                    </div>
                    <div class="col-6">
                        <div class="grid flex-column h-full">
                            <div class="col-12 pt-0 flex flex-column flex-row gap-2">
                                <div class="font-semibold">
                                    {{ t('body.sampleRequest.commitment.time_label') }}
                                    <sup v-if="!isReadOnly" class="font-bold text-red-500">*</sup>
                                </div>
                                <span v-if="isReadOnly" class="font-medium">
                                    {{ model.committedYear ? new Date(model.committedYear).getFullYear() : '—' }}
                                </span>
                                <Calendar v-else v-model="model.committedYear"
                                    :invalid="errMsg.committedYear ? true : false" :minDate="new Date()" view="year"
                                    dateFormat="yy"></Calendar>
                                <small v-if="errMsg.committedYear"> {{ errMsg.committedYear }}</small>
                            </div>
                            <div class="col-12 pt-0 pb-0 flex flex-column flex-row gap-2 flex-grow-1">
                                <div class="font-semibold">{{ t('body.sampleRequest.commitment.notes_label') }}</div>
                                <div class="flex-grow-1">
                                    <span v-if="isReadOnly" class="font-medium white-space-pre-wrap">{{
                                        model.committedDescription || '—' }}</span>
                                    <Textarea v-else v-model="model.committedDescription" class="w-full h-full" />
                                </div>
                            </div>
                            <div class="col-12 pb-1 align-items-center flex flex-row gap-3">
                                <span class="font-semibold">{{ t('body.sampleRequest.commitment.status_label') }}
                                </span>
                                <Tag :value="getStatusLabel(model.docStatus).label"
                                    :severity="getStatusLabel(model.docStatus).severity"></Tag>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="card p-0">
                    <div class="flex justify-content-between p-3">
                        <span class="font-bold text-lg my-auto">{{
                            t('body.sampleRequest.commitment.content_section_title') }}</span>
                        <Button @click="model.addCommittedLine()" :disabled="model.committedLine.length >= 2"
                            v-if="!isReadOnly" :label="t('body.sampleRequest.commitment.application_method_label')"
                            size="small" />
                    </div>
                    <hr class="my-0" />
                    <div class="p-3">
                        <div v-for="(line, lineIndex) in model.getCommittedLine()" :key="lineIndex"
                            class="card p-3 border-noround">
                            <div class="mb-3 flex justify-content-between">
                                <div class="flex align-items-center gap-2">
                                    <span class="mr-3">{{ t('body.sampleRequest.commitment.application_method_label')
                                    }}</span>
                                    <!-- Chỉ xem: hiện label text -->
                                    <span v-if="isReadOnly" class="font-medium">
                                        {{model.getCmtLineTypeOptions().find(o => o.value === line.committedType)?.name
                                            || line.committedType}}
                                    </span>
                                    <Dropdown v-else v-model="line.committedType"
                                        :options="model.getCmtLineTypeOptions()" optionLabel="name" optionValue="value">
                                    </Dropdown>
                                </div>
                                <Button v-if="!isReadOnly && model.committedLine.length > 1"
                                    @click="model.removeCommittedLine(lineIndex)" :label="t('body.SaleSchannel.delete')"
                                    severity="danger" class="ml-3" outlined />
                            </div>
                            <!-- line.committedLineSub -->
                            <DataTable v-model:expandedRows="expandedRows" :value="line.getCommittedLineSub()"
                                showGridlines size="small" scrollable class="z-5 relative" resizableColumns
                                columnResizeMode="fit" :pt="{
                                    rowexpansioncell: {
                                        class: ' p-0'
                                    }
                                }">
                                <Column class="w-1rem" v-if="!isReadOnly">
                                    <template #body="sp">
                                        <Button :disabled="line.getCommittedLineSub().length <= 1" icon="pi pi-trash"
                                            text severity="danger" @click="line.removeSubLine(sp.index)" />
                                    </template>
                                </Column>
                                <Column expander class="w-1rem"> </Column>
                                <Column v-for="(col, col_index) in line?.getColumns(line.committedType)"
                                    :key="col_index" :field="col.field" :header="col.header">
                                    <template #body="sp">
                                        <div class="flex gap-2 align-items-center">
                                            <!-- Chỉ xem: hiện text thuần -->
                                            <template v-if="isReadOnly">
                                                <span v-if="col.isPercent" class="text-right w-full">
                                                    {{ sp.data[col.field] != null ? sp.data[col.field] + '%' : '—' }}
                                                </span>
                                                <span
                                                    v-else-if="col.componentName === 'Dropdown' || col.componentName === 'MultiSelect'"
                                                    class="w-full">
                                                    <!-- itemTypeIds: ≤2 hiện tên tag, >2 hiện số lượng -->
                                                    <template
                                                        v-if="col.componentName === 'MultiSelect' && col.field === 'itemTypeIds'">
                                                        <div
                                                            class="flex flex-wrap align-items-center justify-content-center gap-1">
                                                            <template v-if="(sp.data[col.field] || []).length">
                                                                <!-- ≤2 loại: hiện tag tên trực tiếp -->
                                                                <template v-if="(sp.data[col.field] || []).length <= 2">
                                                                    <Tag v-for="(itemId, tIdx) in (sp.data[col.field] || [])"
                                                                        :key="tIdx"
                                                                        :value="getOptionLabel(sp.data, col, itemId)"
                                                                        severity="secondary"
                                                                        class="text-xs cursor-pointer"
                                                                        @click="toggleMultiSelectPanel($event, sp.data, col)" />
                                                                </template>
                                                                <!-- >2 loại: hiện tag số lượng, click xem danh sách -->
                                                                <Tag v-else
                                                                    :value="`${(sp.data[col.field] || []).length} ${col.header.toLowerCase()}`"
                                                                    severity="info" class="cursor-pointer text-xs"
                                                                    @click="toggleMultiSelectPanel($event, sp.data, col)" />
                                                            </template>
                                                            <span v-else class="text-color-secondary">—</span>
                                                        </div>
                                                    </template>
                                                    <!-- Các cột MultiSelect khác (brandIds...): hiện text thuần join comma -->
                                                    <template v-else-if="col.componentName === 'MultiSelect'">
                                                        {{ getMultiSelectLabel(sp.data, col) || '—' }}
                                                    </template>
                                                    <template v-else>
                                                        {{ getOptionLabel(sp.data, col, sp.data[col.field]) || '—' }}
                                                    </template>
                                                </span>
                                                <span v-else class="text-right w-full">
                                                    {{ sp.data[col.field] != null ? sp.data[col.field].toLocaleString()
                                                        : '—' }}
                                                </span>
                                            </template>
                                            <!-- Chỉnh sửa: hiện component input -->
                                            <component v-else :is="col.componentName" v-model="sp.data[col.field]"
                                                :invalid="errMsgLine?.[`${lineIndex}`]?.[sp.index]?.[col.field]"
                                                :min="0" :max="col.isPercent ? 100 : undefined"
                                                :options="sp.data.getSelectionOptions(col.field, sp.data['industryId'], sp.data['brandIds'], model.cardId)"
                                                @change="sp.data?.onChangeSelect(col.field)"
                                                :optionLabel="col.optionLabel" :optionValue="col.optionValue"
                                                @update:modelValue="onInputNumber(sp.data, line.committedType, col.field, $event)"
                                                x----------x----------x----------x----------x----------x----------x----------x
                                                class="w-10rem" :class="col.class"
                                                :pt="getPassThrough(col.componentName)"
                                                :suffix="col.isPercent ? '%' : null" />
                                        </div>
                                    </template>
                                </Column>
                                <template #expansion="subLine">
                                    <div class="flex p-3">
                                        <DataTable scrollable scrollHeight="250px"
                                            :value="subLine.data?.getCommittedLineSubSub()">
                                            <Column header="#" class="w-3rem">
                                                <template #body="{ index }">
                                                    {{ index + 1 }}
                                                </template>
                                            </Column>
                                            <Column :header="t('client.exceeded_production')">
                                                <template #body="{ data, index }">
                                                    <span v-if="isReadOnly" class="font-medium">
                                                        {{ data.outPut != null ? data.outPut.toLocaleString() : '—' }}
                                                    </span>
                                                    <InputNumber v-else v-model="data.outPut"
                                                        :invalid="errMsgLine?.[`${lineIndex}`]?.[subLine.index]?.[index]?.['outPut']"
                                                        class="w-10rem"
                                                        :min="getMinOverValue(subLine.data?.total || 0, subLine.data?.committedLineSubSub, index)"
                                                        :pt="getPassThrough('InputNumber')" />
                                                </template>
                                            </Column>
                                            <Column :header="t('body.report.discount')" class="w-12rem">
                                                <template #body="sp">
                                                    <div class="flex gap-2 align-items-center">
                                                        <span v-if="isReadOnly" class="font-medium">
                                                            {{ sp.data.discount != null ? sp.data.discount + '%' : '—'
                                                            }}
                                                        </span>
                                                        <InputNumber v-else
                                                            :invalid="errMsgLine?.[`${lineIndex}`]?.[subLine.index]?.[sp.index]?.['discount']"
                                                            v-model="sp.data.discount" :min="0" :max="100"
                                                            :pt="getPassThrough('InputNumber')" suffix="%" />
                                                    </div>
                                                </template>
                                            </Column>
                                            <Column v-if="!isReadOnly" class="w-1rem">
                                                <template #body="sp">
                                                    <Button @click="subLine.data?.removeSubSubLine(sp.index)"
                                                        icon="pi pi-trash" text severity="danger" />
                                                </template>
                                            </Column>
                                            <template #empty>
                                                <div class="text-center p-3 w-30rem mx-3">{{
                                                    t('body.sampleRequest.importPlan.empty_state_message') }}</div>
                                            </template>
                                            <template #footer v-if="!isReadOnly">
                                                <div class="p-1">
                                                    <Button
                                                        :label="t('body.report.table_header_exceeding_volume_bonus')"
                                                        icon="pi pi-plus" size="small"
                                                        @click="subLine.data?.addSubSubLine()" outlined />
                                                </div>
                                            </template>
                                        </DataTable>
                                    </div>
                                </template>
                                <template #footer v-if="!isReadOnly">
                                    <div class="p-2">
                                        <Button :label="t('body.promotion.addPromotionCondition')" icon="pi pi-plus"
                                            size="small" @click="line.addSubLine()" />
                                    </div>
                                </template>
                            </DataTable>
                        </div>
                    </div>
                </div>
                <div v-if="props.showPayload" class="card p-3 relative surface-700 text-200">
                    <span
                        class="flex align-items-center gap-2 absolute px-3 pt-2 pb-0 surface-700 card border-none font-bold text-green-200"
                        style="top: -16px">
                        Payload
                        <i class="fa-solid fa-code"></i>
                    </span>
                    <div class="h-30rem overflow-y-scroll">
                        <code>
            {{ model }}
            <!-- {{ errMsgLine }} -->
        </code>
                    </div>
                    <div class="flex justify-content-end gap-2">
                        <Button @click="onClickCopy" outlined icon="pi pi-copy" class="text-200" />
                    </div>
                </div>
            </div>
            <template #footer>
                <Button @click="visible = false" :label="t('body.OrderList.close')" severity="secondary" />
                <template v-if="['D', 'R'].includes(model.docStatus) || !id">
                    <Button :disabled="loadingBtnSent" :loading="loadingBtnDrawp" @click="onClickSave('D')"
                        :label="t('body.sampleRequest.sampleProposal.save_draft_button')" severity="info" />
                    <Button :disabled="loadingBtnDrawp" :loading="loadingBtnSent" @click="onClickSave('P')"
                        :label="t('body.sampleRequest.sampleProposal.send_for_approval_button')" />
                </template>
            </template>
        </Dialog>
        <Loading v-if="loading" />
        <!-- OverlayPanel: danh sách chi tiết (thương hiệu, loại sản phẩm...) -->
        <OverlayPanel ref="multiSelectPanel">
            <div class="p-2" style="min-width: 220px; max-width: 340px">
                <div class="font-semibold mb-2 text-primary">{{ overlayTitle }}</div>
                <div v-if="!overlayLabels.length" class="text-color-secondary text-sm">
                    {{ t('body.systemSetting.no_data_to_display') }}
                </div>
                <div v-else class="flex flex-wrap gap-2">
                    <Tag v-for="(item, idx) in overlayLabels" :key="idx" :value="item" severity="secondary"
                        class="text-xs" />
                </div>
            </div>
        </OverlayPanel>
    </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
import { useToast } from 'primevue/usetoast';
import OverlayPanel from 'primevue/overlaypanel';
import CustomerSelectorId from '@/components/CustomerSelectorId.vue';
import API from '@/api/api-main';
import { Committed, refreshIndustryOptions } from '../entities/Committed';
import { useHierarchyStore } from '@/Pinia/hierarchyStore';

// --- Props & Emits ---
const props = defineProps({
    showPayload: { type: Boolean, default: false }
});
const emits = defineEmits(['refresh']);

// --- Services ---
const { t } = useI18n();
const toast = useToast();
const router = useRouter();
const route = useRoute();

// --- State ---
const visible = ref(false);
const id = ref<number | null>(null);
const loading = ref(false);
const loadingBtnDrawp = ref(false);
const loadingBtnSent = ref(false);
const expandedRows = ref([]);
const model = ref<Committed>(new Committed());
const errMsg = ref<Record<string, string>>({});
const errMsgLine = ref<Record<string, any>>({});

// --- OverlayPanel State (Read-only UI) ---
const multiSelectPanel = ref();
const overlayLabels = ref<string[]>([]);
const overlayTitle = ref('');

// --- Computed ---
/** Chế độ chỉ xem: true khi phiếu đã được xác nhận (docStatus = 'A') */
const isReadOnly = computed(() => model.value.docStatus === 'A');

/** Style trạng thái của văn bản */
const statusConfig = {
    P: { label: 'Đang chờ', severity: 'warning' },
    D: { label: 'Nháp', severity: 'info' },
    C: { label: 'Đã hủy', severity: 'danger' },
    A: { label: 'Đã xác nhận', severity: 'primary' },
    R: { label: 'Từ chối', severity: 'danger' }
};

// --- UI Helpers ---

/** Lấy thông tin nhãn & độ ưu tiên của trạng thái */
function getStatusLabel(statusVal: string) {
    return statusConfig[statusVal as keyof typeof statusConfig] || statusConfig.P;
}

/** Lấy label của một option đơn lẻ (dùng cho Tag trong read-only mode) */
function getOptionLabel(rowData: any, col: any, itemId: any): string {
    return rowData.getSelectionOptions(col.field, rowData['industryId'], rowData['brandIds'], model.value.cardId)
        ?.find((o: any) => o[col.optionValue] === itemId)?.[col.optionLabel] || '?';
}

/** Ghép label của nhiều options thành chuỗi (dùng cho MultiSelect text mode) */
function getMultiSelectLabel(rowData: any, col: any): string {
    const ids = rowData[col.field] || [];
    const options = rowData.getSelectionOptions(col.field, rowData['industryId'], rowData['brandIds'], model.value.cardId) || [];
    return ids.map((idVal: any) => options.find((o: any) => o[col.optionValue] === idVal)?.[col.optionLabel])
        .filter(Boolean)
        .join(', ');
}

/** Mở/đóng OverlayPanel danh sách chi tiết (item type) */
async function toggleMultiSelectPanel(event: Event, rowData: any, col: any) {
    overlayTitle.value = col.header;
    if (col.field === 'itemTypeIds') {
        const hierarchyStore = useHierarchyStore();
        await hierarchyStore.loadHierarchies(model.value.cardId);
    }
    const ids = rowData[col.field] || [];
    const options = rowData.getSelectionOptions(col.field, rowData['industryId'], rowData['brandIds'], model.value.cardId) || [];
    overlayLabels.value = ids.map((idVal: any) => options.find((o: any) => o[col.optionValue] === idVal)?.[col.optionLabel]).filter(Boolean);
    multiSelectPanel.value?.toggle(event);
}

/** Cấu hình passThrough cho PrimeVue components */
function getPassThrough(type: string) {
    if (type === 'InputNumber') {
        return { input: { root: { class: 'w-10rem text-right' } } };
    }
    return null;
}

/** Giá trị tối thiểu cho sản lượng vượt (lấy từ dòng liền trước) */
function getMinOverValue(total: number, data: any[], index: number): number {
    if (index === 0) return total ? total + 1 : 0;
    return (data[index - 1]?.outPut || 0) + 1;
}

// --- Event Handlers ---

/** Khi chọn khách hàng, tự điền tên vào tên cam kết nếu đang trống */
function onSelectCustomer(data: any) {
    if (!model.value.committedName) {
        model.value.committedName = data.cardName + ' - ';
    }
    refreshIndustryOptions(data.id);
}

/** Xử lý tự động chia sản lượng hoặc tính tổng khi nhập số liệu */
function onInputNumber(data: any, committedType: 'Q' | 'Y' | 'P', field: string, value: number) {
    if (field === 'total') {
        if (committedType === 'Q') {
            data.quarter1 = data.quarter2 = data.quarter3 = data.quarter4 = 0;
        } else if (committedType === 'Y') {
            for (let i = 1; i <= 12; i++) data[`month${i}`] = 0;
        }
    } else if (field.includes('quarter') || field.includes('month')) {
        data.total = 0;
        if (committedType === 'Q') {
            for (let i = 1; i <= 4; i++) data.total += data['quarter' + i] || 0;
        } else if (committedType === 'Y') {
            for (let i = 1; i <= 12; i++) data.total += data['month' + i] || 0;
        }
    }
}

function onClickCopy() {
    navigator.clipboard.writeText(JSON.stringify(model.value));
}

// --- API Actions ---

/** Lưu dữ liệu (Nháp hoặc Gửi phê duyệt) */
async function onClickSave(statusVal: 'P' | 'D') {
    if (statusVal === 'D') loadingBtnDrawp.value = true;
    else loadingBtnSent.value = true;

    try {
        const result = await model.value.save(statusVal, toast);
        errMsg.value = result.errors || {};
        errMsgLine.value = result.linesError || {};
        if (result.success) {
            onHide();
            emits('refresh', true);
        }
    } finally {
        loadingBtnDrawp.value = loadingBtnSent.value = false;
    }
}

/** Tải dữ liệu khi mở dialog */
function open(recordId?: number) {
    if (recordId) {
        loading.value = true;
        id.value = recordId;
        API.get(`commited/${recordId}`)
            .then((res) => {
                visible.value = true;
                model.value = new Committed(res.data);
                refreshIndustryOptions(model.value.cardId);
                useHierarchyStore().loadHierarchies(model.value.cardId);
            })
            .catch(() => {
                toast.add({
                    severity: 'error',
                    summary: t('body.report.error_occurred_message'),
                    detail: t('body.report.error_occurred_message'),
                    life: 3000
                });
            })
            .finally(() => {
                loading.value = false;
            });
    } else {
        visible.value = true;
    }
}

/** Reset dữ liệu khi đóng dialog */
function onHide() {
    model.value = new Committed();
    expandedRows.value = [];
    id.value = null;
    errMsg.value = {};
    errMsgLine.value = {};
    visible.value = false;
    router.replace({ name: route.name });
}

// --- Public APIs ---
defineExpose({ open });

</script>

<style scoped>
small {
    color: #ff3d32;
    margin-top: -4px;
}
</style>
