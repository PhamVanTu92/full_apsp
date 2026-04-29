<template>
    <div class="">
        <Dialog
            v-model:visible="visible"
            modal
            class="w-10"
            @hide="onHideDlg"
            v-if="!loading"
            maximizable
        >
            <!-- Vùng tiêu đề -->
            <template #header>
                <h4 class="font-bold m-0">{{t('Custom.warehouseStorageFeeRecord')}}</h4>
            </template>
            <!-- Vùng thông tin chứng từ + khách hàng + thời gian + ghi chú -->
            <div class="grid m-0 pb-1">
                <!-- Mã chứng từ -->
                <div class="grid m-0 pb-3 col-12 p-0 md:col-6 md:flex-order-0">
                    <div class="col-12 p-0 md:col-3">
                        <label class="font-bold pb-2">{{t('client.certificate_code')}}</label>
                    </div>
                    <div class="col-12 p-0 md:col-6">
                        <label class="w-full">{{
                            props.feeData?.feebyCustomerCode
                        }}</label>
                    </div>
                </div>
                <!-- Tên chứng từ -->
                <div class="grid m-0 pb-3 col-12 p-0 md:col-6 md:flex-order-2">
                    <div class="col-12 p-0 md:col-3">
                        <label class="font-bold pb-2">{{t('client.certificate_name')}}</label>
                    </div>
                    <div class="col-12 p-0 md:col-6">
                        <label class="w-full">{{
                            props.feeData?.feebyCustomerName
                        }}</label>
                    </div>
                </div>
                <!-- Từ ngày -->
                <!-- Đến ngày -->
                <div class="grid m-0 pb-3 col-12 p-0 md:col-6 md:flex-order-4">
                    <div class="col-12 p-0 md:col-3">
                        <label class="font-bold pb-3">{{t('client.time_period')}}</label>
                    </div>
                    <div class="grid m-0 gap-3 col-12 p-0 md:col-6">
                        <label class="col-2 p-0">{{t('Custom.from')}}</label>
                        <label class="col-8 p-0">{{ props.feeData?.fromDate }}</label>
                        <label class="col-2 p-0">{{t('Custom.to')}}</label>
                        <label class="col-8 p-0">{{ props.feeData?.toDate }}</label>
                    </div>
                </div>
                <!-- Mã khách hàng -->
                <div class="grid m-0 pb-3 col-12 p-0 md:col-6 md:flex-order-1">
                    <div class="col-12 p-0 md:col-3">
                        <label class="font-bold pb-2">{{t('body.systemSetting.table_header_customer_code')}}</label>
                    </div>
                    <div class="col-12 p-0 md:col-6">
                        <label class="w-full">{{ props.feeData?.cardCode }}</label>
                    </div>
                </div>
                <!-- Tên khách hàng -->
                <div class="grid m-0 pb-3 col-12 p-0 md:col-6 md:flex-order-3">
                    <div class="col-12 p-0 md:col-3">
                        <label class="font-bold">{{t('body.systemSetting.table_header_customer_name')}}</label>
                    </div>
                    <div class="col-12 p-0 md:col-6">
                        <label class="w-full">{{ props.feeData?.cardName }}</label>
                    </div>
                </div>
                <!-- Trạng thái của NPP -->
                <div
                    v-if="props.NPP"
                    class="grid m-0 pb-3 col-12 p-0 md:col-6 md:flex-order-5"
                >
                    <div class="col-12 p-0 md:col-3">
                        <label class="font-bold pb-2">{{t('body.systemSetting.status_label')}}</label>
                    </div>
                    <div class="col-12 p-0 md:col-6">
                        <Tag
                            severity="warning"
                            v-if="props.feeData?.confirmStatus === STATUS.NOT_CONFIRMED"
                            >{{ STATUS_MESSAGE.NOT_CONFIRMED }}</Tag
                        >
                        <Tag
                            severity="success"
                            v-if="props.feeData?.confirmStatus === STATUS.CONFIRMED"
                            >{{ STATUS_MESSAGE.CONFIRMED }}
                        </Tag>
                    </div>
                </div>
                <!-- Ghi chú -->
                <!-- của Nhà Phân phối -->
                <div v-if="NPP" class="grid m-0 pb-3 col-12 p-0 md:col-6 md:flex-order-6">
                    <div class="col-12 p-0 md:col-3">
                        <label class="font-bold pb-2">{{t('body.systemSetting.note')}}</label>
                    </div>
                    <div class="col-12 p-0 md:col-6">
                        <div class="w-full">{{ props.feeData?.note }}</div>
                    </div>
                </div>
                <!-- của SG Petrol -->
                <div v-else class="grid m-0 pb-3 col-12 p-0 md:col-6 md:flex-order-5">
                    <div class="col-12 p-0 md:col-3">
                        <label class="font-bold pb-2">{{t('body.systemSetting.note')}}</label>
                    </div>
                    <div class="col-12 p-0 md:col-6">
                        <Textarea
                            rows="2"
                            cols="40"
                            autoResize
                            class="w-full"
                            maxlength="80"
                            v-model="note"
                            @input="onChangeNote"
                        />
                    </div>
                </div>
            </div>
            <!-- Vùng Bảng thông tin-->
            <DataTable
                :value="props.feeData?.feebyCustomerLine"
                scrollable
                scrollHeight="39rem"
                showGridlines
                resizableColumns
                columnResizeMode="expand"
            >
                <!-- # -->
                <Column
                    header="#"
                    field="lineId"
                    :pt="{
                        headerTitle: { class: 'w-full text-center' },
                        bodycell: { class: 'text-center' },
                    }"
                />
                <!-- Mã hàng -->
                <Column
                    :header="t('body.home.product_code_column')"
                    field="itemCode"
                    :pt="{
                        headerTitle: { class: 'w-full text-left' },
                        bodycell: { class: 'text-left' },
                    }"
                />
                <!-- Tên hàng -->
                <Column
                    :header="t('body.home.product_name_column')"
                    field="itemName"
                    :pt="{
                        headerTitle: { class: 'w-full text-left' },
                        bodycell: { class: 'text-left' },
                    }"
                />
                <!-- Đơn vị tính -->
                <Column
                    :header="t('client.unit')"
                    field="ugpName"
                    :pt="{
                        headerTitle: { class: 'w-full text-left' },
                        bodycell: { class: 'text-left' },
                    }"
                />
                <!-- Số lô nhập -->
                <Column
                    :header="t('client.batch_number')"
                    field="batchNum"
                    :pt="{
                        headerTitle: { class: 'w-full text-left' },
                        bodycell: { class: 'text-left' },
                    }"
                />
                <!-- Ngày nhập -->
                <Column field="receiptDate" :pt="{ bodycell: { class: 'text-center' } }">
                    <template #header>
                        <div class="flex-column text-center">
                            <div>{{t('client.import_date')}}</div>
                            <div>(1)</div>
                        </div>
                    </template>
                </Column>
                <!-- Ngày tính phí gửi kho -->
                <Column field="receiptDate" :pt="{ bodycell: { class: 'text-center' } }">
                    <template #header>
                        <div class="flex-column text-center">
                            <div>{{t('client.warehouse_fee_date')}}</div>
                            <div>(2)</div>
                        </div>
                    </template>
                </Column>
                <!-- Ngày xuất -->
                <Column field="issueDate" :pt="{ bodycell: { class: 'text-center' } }">
                    <template #header>
                        <div class="flex-column text-center">
                            <div>{{t('client.export_date')}}</div>
                            <div>(3)</div>
                        </div>
                    </template>
                </Column>
                <!-- Số lượng xuất -->
                <Column field="quantity" :pt="{ bodycell: { class: 'text-center' } }">
                    <template #header>
                        <div class="flex-column text-center">
                            <div>{{t('client.export_quantity')}}</div>
                            <div>(4)</div>
                        </div>
                    </template>
                </Column>
                <!-- Số ngày gửi kho -->
                <Column field="day" :pt="{ bodycell: { class: 'text-center' } }">
                    <template #header>
                        <div class="flex-column text-center">
                            <div>{{t('client.days_in_storage')}}</div>
                            <div>(5)= (3)-(2)</div>
                        </div>
                    </template>
                </Column>
                <!-- Số ngày gửi kho tính phí -->
                <Column
                    field="dayToFee"
                    :pt="{ bodycell: { class: 'text-center' } }"
                    bodyClass="px-0"
                >
                    <template #header>
                        <div class="flex-column text-center">
                            <div>{{t('client.days_charged')}}</div>
                            <div>(6)</div>
                        </div>
                    </template>
                    <template #body="{ data }">
                        <div class="flex flex-column with-lines">
                            <div
                                v-for="(day, index) in data.dayToFee"
                                :key="day + index"
                                class="w-full text-center p-3"
                            >
                                {{ day }}
                            </div>
                        </div>
                    </template>
                </Column>
                <!-- Đơn giá -->
                <Column field="price" bodyClass="px-0">
                    <template #header>
                        <div class="w-full text-center">
                            <div>{{t('client.unit_price')}}</div>
                            <div>(7)</div>
                        </div>
                    </template>
                    <template #body="{ data }">
                        <div class="flex flex-column with-lines">
                            <div
                                v-for="(priceItem, index) in data.price"
                                :key="priceItem + index"
                                class="w-full text-right p-3"
                            >
                                {{ format.FormatCurrency(priceItem) }}
                            </div>
                        </div>
                    </template>
                </Column>
                <!-- Biểu phí áp dụng -->
                <Column field="feeLevelName" bodyClass="px-0">
                    <template #header>
                        <div class="w-full text-center">
                            <div>{{t('client.applied_fee')}}</div>
                            <div>(8)</div>
                        </div>
                    </template>
                    <template #body="{ data }">
                        <div class="flex flex-column with-lines">
                            <div
                                v-for="(name, index) in data.feeLevelName"
                                :key="name + index"
                                class="w-full text-left p-3"
                            >
                                {{ name }}
                            </div>
                        </div>
                    </template>
                </Column>
                <!-- Tổng tiền chưa VAT 8% -->
                <Column field="lineTotal" bodyClass="px-0">
                    <template #header>
                        <div class="w-full text-center">
                            <div>{{t('client.total_excl_vat')}}</div>
                            <div>(9) = (4)*(6)*(7)</div>
                        </div>
                    </template>
                    <template #body="{ data }">
                        <!-- <div class="flex flex-column with-lines">
                            <div
                                v-for="(total, index) in data.lineTotal"
                                :key="index"
                                class="w-full text-right p-3"
                            >
                                {{ format.FormatCurrency(total) }}
                            </div>
                        </div> -->
                        <div class="flex justify-content-end p-3">
                            <!-- {{ format.FormatCurrency(data.lineTotal - data.lineVAT) }} -->
                            {{
                                format.FormatCurrency(
                                    data.lineTotal.reduce((a, b) => a + b, 0) -
                                        data.lineVAT.reduce((a, b) => a + b, 0)
                                )
                            }}
                        </div>
                    </template>
                </Column>
                <ColumnGroup type="footer">
                    <Row>
                        <Column
                            footer="Tổng cộng:"
                            :colspan="8"
                            footerClass="w-full text-left"
                        />
                        <!-- Tổng số lượng xuất -->
                        <Column :footer="totalQuantity" footerClass="w-full text-center">
                        </Column>
                        <Column />
                        <!-- Tổng số lượng ngày gửi kho tính phí -->
                        <Column
                            :footer="totalDayToFee"
                            footerClass="w-full text-center"
                        />
                        <Column :colspan="2" />
                        <!-- Tổng tổng tiền -->
                        <Column
                            :footer="format.FormatCurrency(totalLineTotal)"
                            footerClass="w-full text-right"
                        />
                    </Row>
                </ColumnGroup>
                <template #empty>
                    <div class="text-red-400">{{ t('client.no_data') }}</div>
                </template>
            </DataTable>
            <template #footer>
                <Button
                    type="button"
                    :label="t('client.cancel')"
                    icon="pi pi-times"
                    severity="secondary"
                    @click="visible = false"
                />
                <div v-if="props.NPP">
                    <div v-if="props.feeData?.confirmStatus === STATUS.NOT_CONFIRMED">
                        <Button
                            v-if="
                                [STATUS.NOT_CONFIRMED].includes(
                                    props.feeData?.confirmStatus
                                )
                            "
                            class="mr-2"
                            icon="pi pi-times"
                            :label="t('body.status.reject')"
                            severity="danger"
                            @click="onConfirmReject(props.feeData?.id)"
                        />
                        <Button
                            icon="pi pi-check"
                           :label="t('client.confirm')"
                            @click="onConfirmFeeDataDetail(props.feeData?.id)"
                        />
                    </div>
                </div>
                <div v-else class="flex gap-2">
                    <Button
                        severity="help"
                        :label="t('client.export_files')"
                        icon="pi pi-file-export"
                        @click="onExportFileDetail(props.feeData.id)"
                    />
                    <Button
                        :label="t('Login.buttons.send')"
                        icon="pi pi-send"
                        severity="info"
                        v-if="props.feeData?.status === STATUS.NOT_SENT"
                        @click="onSendFeeDataDetail(props.feeData.id)"
                    />
                    <Button
                        :loading="loadingSaveBtn"
                        @click="onClickSaveNote"
                        :label="t('client.save')"
                        icon="pi pi-save"
                    />
                </div>
            </template>
            <template #empty>
                <div class="text-center pt-3">
                    <h5>Không có dữ liệu</h5>
                </div>
            </template>
        </Dialog>
        <!-- Loading -->
        <Loading v-if="loading"></Loading>
        <Dialog
            v-model:visible="visibleRejectConfirm"
            header="Bạn có muốn từ chối biên bản này?"
            class="w-5"
            modal
        >
            <div class="flex flex-column field gap-1">
                <label for="reason" class="font-semibold text-gray-500"
                    >Lý do từ chối<sup class="text-red-500 ml-2">*</sup></label
                >
                <Editor id="reason" editorStyle="height: 220px"></Editor>
            </div>
            <template #footer>
                <Button
                    label="Đóng"
                    severity="secondary"
                    @click="visibleRejectConfirm = false"
                />
                <Button label="Từ chối" severity="danger" @click="nhincaichoagi" />
            </template>
        </Dialog>
    </div>
</template>

<script setup>
// Import
import { ref, computed, onMounted, watch } from "vue";
import format from "@/helpers/format.helper";
import { useRouter, useRoute } from "vue-router";
import API from "@/api/api-main";
import { URL } from "read-excel-file";
import { useToast } from "primevue/usetoast";
import { useI18n } from 'vue-i18n';
const { t } = useI18n();
const toast = useToast();

const visibleRejectConfirm = ref(false);
const onConfirmReject = (id) => {
    visibleRejectConfirm.value = true;
};

const loadingSaveBtn = ref(false);
const onClickSaveNote = () => {
    loadingSaveBtn.value = true;
    const encodeNote = encodeURIComponent(note.value);
    API.update(`Fee/feeByCus/${props.feeData.id}/${encodeNote}`)
        .then((res) => {
            if (res.status == 200) {
                toast.add({
                    summary: t('common.success'),
                    detail: t('common.msg_save_success'),
                    severity: "success",
                    life: 3000,
                });
            }
            loadingSaveBtn.value = false;
            visible.value = false;
        })
        .catch((error) => {
            toast.add({
                summary: t('common.error'),
                detail: t('common.msg_error_occurred'),
                severity: "success",
                life: 3000,
            });
            loadingSaveBtn.value = false;
        });
    note.value = "";
};

const { router, route } = { router: useRouter(), route: useRoute() };
// Enum
const STATUS = {
    SENT: "SD",
    NOT_SENT: "NS",
    CONFIRMED: "CF",
    NOT_CONFIRMED: "NC",
};
const STATUS_MESSAGE = {
    CONFIRMED: "Đã xác nhận",
    NOT_CONFIRMED: "Chưa xác nhận",
};

// Emit
const emit = defineEmits([
    "onSendFeeDataDetail",
    "onConfirmFeeDataDetail",
    "onExportFileDetail",
]);

// Models
const visible = defineModel("visible", {
    type: Boolean,
    default: true,
});

const onHideDlg = () => {
    router.replace({
        name: route.name,
        query: null,
    });
    note.value = "";
    props.feeData.id = null;
};

// Props
const props = defineProps({
    feeData: {
        type: Object,
        default: {},
    },
    NPP: {
        type: Boolean,
        default: false,
    },
    note: null,
});

const note = ref("");

// Global States

// Internal States
const loading = ref(false);

// Computed States

// Normal Var
const totalQuantity = computed(() =>
    props.feeData?.feebyCustomerLine?.reduce((total, item) => total + item.quantity, 0)
);
const totalDayToFee = computed(() =>
    props.feeData?.feebyCustomerLine?.reduce(
        (total, item) => total + (item.dayToFee?.reduce((sum, num) => sum + num, 0) || 0),
        0
    )
);
const totalLineTotal = computed(() =>
    props.feeData?.feebyCustomerLine?.reduce(
        (total, item) =>
            total +
            (item.lineTotal.reduce((a, b) => a + b, 0) -
                item.lineVAT.reduce((a, b) => a + b, 0)),
        0
    )
);

// Lifecycle Functions
// API Functions

// Event Functions
const onChangeNote = (e) => {
    //Ghi chú chưa làm 
};

const onSendFeeDataDetail = () => {
    emit("onSendFeeDataDetail");
};

const onConfirmFeeDataDetail = () => {
    emit("onConfirmFeeDataDetail");
};

const onExportFileDetail = () => {
    emit("onExportFileDetail");
};

watch(
    () => props.feeData?.id,
    () => {
        note.value = props.feeData?.note;
    }
);

// Funtions
</script>
<style scoped>
.with-lines > div:not(:first-child) {
    border-top: 1px solid #e2e8f0;
}
</style>
