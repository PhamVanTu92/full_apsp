<template>
    <div>
        <div class="grid">
            <div class="col-2">
                <div>
                    <h3 class="font-bold m-0 mb-4">{{ t('body.promotion.promotion_title') }}</h3>
                </div>
                <div style="max-height: 77vh" class="overflow-auto">
                    <div class="card p-0 mb-5 shadow-1">
                        <div class="px-3 py-2 ">
                            <div class="flex justify-content-between align-items-center p-2">
                                <div><span class="m-0 font-bold text-base">{{ t('body.PurchaseRequestList.status') }}</span></div>
                                <div>
                                    <Button text icon="pi pi-angle-right" @click="createDate = !createDate" />
                                </div>
                            </div>
                        </div>
                        <transition name="fade">
                            <div class="p-3" v-if="createDate">
                                <div class="flex align-items-center mb-3">
                                    <RadioButton></RadioButton>
                                    <label class="ml-2">{{ t('body.sampleRequest.customer.all_status_option') }}</label>
                                </div>
                                <div class="flex align-items-center mb-3">
                                    <RadioButton></RadioButton>
                                    <label class="ml-2">{{ t('body.sampleRequest.customer.active_status_option') }}</label>
                                </div>
                                <div class="flex align-items-center mb-3">
                                    <RadioButton></RadioButton>
                                    <label class="ml-2">{{ t('body.sampleRequest.customer.inactive_status_option') }}</label>
                                </div>
                            </div>
                        </transition>
                    </div>
                </div>
            </div>
            <!-- Table -->
            <div class="col-10" :style="[styleSticky]">
                <div class="flex justify-content-between m-0 mb-3">
                    <div class="w-6">
                        <IconField iconPosition="left">
                            <InputText type="text" :placeholder="t('body.sampleRequest.customer.voucher_search_placeholder')"
                                @keydown.enter="searchProduct()" class="w-full" v-model="keySearchProduct" />
                            <InputIcon class="pi pi-search" />
                        </IconField>
                    </div>
                    <div class="flex gap-2">
                        <Button @click="openUpdCouponDialog()" icon="pi pi-plus"
                            :label="t('body.promotion.create_coupon_dialog_title')"/>
                        <Button icon="pi pi-align-justify" @click="op.toggle($event)" />
                    </div>
                </div>
                <div style="max-height: 77vh" class="card p-2 shadow-1">
                    <DataTable ref="dataDetailCoupon" paginator :rows="rows" :page="page" :totalRecords="totalRecords"
                        @page="onPageChange($event)" lazy
                        currentPageReportTemplate="Hiển thị {first} - {last} trên tổng {totalRecords} sản phẩm"
                        scrollable scrollHeight="70vh" stripedRows showGridlines v-model:expandedRows="expandedRows"
                        @row-click="DetailCoupon" dataKey="id"
                        paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
                        :rowClass="rowClass" :rowsPerPageOptions="[10, 20, 50]" :value="Coupon"
                        tableStyle="min-width: 50rem">
                        <template #empty>
                            <div class="p-4 text-center">
                                <!-- <span class="p-column-title">Không tìm thấy kết quả nào phù hợp với từ khoá {{ keySearchProduct ? '"' + keySearchProduct + '"' : '' }}</span> -->
                            </div>
                        </template>
                        <Column v-for="col of selectedColumn" :key="col.field" :field="col.field" :header="col.header">
                            <template #body="slotProps">
                                <div>
                                    <span v-if="col.type == 'text'">{{ slotProps.data[col.field] }}</span>
                                    <span v-if="col.type == 'number'">{{ slotProps.data[col.field] }}</span>
                                    <span v-if="col.type == 'time'">{{
                                        format(slotProps.data[col.field], "dd/MM/yyyy")
                                        }}</span>
                                    <span v-if="col.type == 'doubleField'">
                                        {{ slotProps.data[col.doubleField[0]] }}% ({{
                                            new Intl.NumberFormat().format(slotProps.data[col.doubleField[1]])
                                        }} )
                                    </span>
                                    <span v-if="col.type == 'statusField'">
                                        {{ formatStatusCoupon(slotProps.data[col.field]) }}
                                    </span>
                                </div>
                            </template>
                        </Column>
                        <template #expansion="slotProps">
                            <div>
                                <TabView>
                                    <TabPanel :header="t('body.promotion.coupon_tab_info') || 'Thông tin'">
                                        <div class="p-2">
                                            <div class="grid">
                                                <div class="col">
                                                    <div
                                                        class="flex gap-2 pb-2 my-4 border-bottom-1 border-gray-200 align-items-center">
                                                        <span class="text-gray-700" for="">{{ t('body.promotion.coupon_code_label') }} </span>
                                                        <span class="text-base text-gray-700 font-semibold">{{
                                                            slotProps.data.couponCode
                                                            }}</span>
                                                    </div>
                                                    <div
                                                        class="flex gap-2 pb-2 my-4 border-bottom-1 border-gray-200 align-items-center">
                                                        <span class="text-gray-700" for="">{{ t('body.report.time_label_3') }} </span>
                                                        <span class="text-base text-gray-700 font-semibold">{{
                                                            format(slotProps.data.fromDate,
                                                            "dd/MM/yyyy") }}-{{
                                                                format(slotProps.data.toDate, "dd/MM/yyyy")
                                                            }}</span>
                                                    </div>
                                                    <div
                                                        class="flex gap-2 pb-2 my-4 border-bottom-1 border-gray-200 align-items-center">
                                                        <span class="text-gray-700" for="">{{ t('body.promotion.coupon_product_label') }} </span>
                                                        <span class="text-base text-gray-700 font-semibold">
                                                            <div class="flex gap-1 flex-wrap">
                                                                <div v-for="(item, index) in slotProps.data.couponItem"
                                                                    :key="index"
                                                                    class="p-1 bg-gray-300 border-round mb-2">
                                                                    {{
                                                                        item.itemGroupId
                                                                            ? `${t('body.promotion.group_prefix') || 'Nhóm'} ${item.itmsGrpName}`
                                                                    : item.itemName
                                                                    }}
                                                                </div>
                                                            </div>
                                                        </span>
                                                    </div>
                                                </div>
                                                <div class="col">
                                                    <div
                                                        class="flex gap-2 pb-2 my-4 border-bottom-1 border-gray-200 align-items-center">
                                                        <span class="text-gray-700" for="">{{ t('body.promotion.coupon_name_label') }} </span>
                                                        <span class="text-base text-gray-700 font-semibold">{{
                                                            slotProps.data.couponName
                                                            }}</span>
                                                    </div>
                                                    <div
                                                        class="flex gap-2 pb-2 my-4 border-bottom-1 border-gray-200 align-items-center">
                                                        <span class="text-gray-700" for="">{{ t('body.promotion.coupon_value_label') }} </span>
                                                        <span class="text-base text-gray-700 font-semibold">{{
                                                            slotProps.data.discount }}% {{ t('body.promotion.max_value_label') }}:
                                                            {{
                                                                new Intl.NumberFormat().format(
                                                                    slotProps.data.maxDiscountAmount
                                                            )
                                                            }}</span>
                                                    </div>
                                                    <div
                                                        class="flex gap-2 pb-2 my-4 border-bottom-1 border-gray-200 align-items-center">
                                                        <span class="text-gray-700" for="">{{ t('body.promotion.coupon_total_amount_label') }}
                                                        </span>
                                                        <span class="text-base text-gray-700 font-semibold">{{
                                                            new Intl.NumberFormat().format(slotProps.data.amount)
                                                            }}</span>
                                                    </div>
                                                </div>
                                                <div class="col">
                                                    <Textarea autoResize :placeholder="t('body.PurchaseRequestList.note_placeholder')" rows="5" cols="30"
                                                        v-model="slotProps.data.couponDescription" />
                                                </div>
                                            </div>
                                            <div class="flex align-items-center justify-content-end">
                                                <div class="flex gap-2">
                                                    <Button @click="openUpdCouponDialog(slotProps.data)"
                                                        icon="pi pi-check" :label="t('body.promotion.update_button')"/>
                                                    <Button class="bg-green-600 border-green-600" icon="pi pi-file"
                                                        :label="t('body.promotion.copy_button')"/>
                                                    <Button @click="openRemoveDialog(slotProps.data)"
                                                        class="bg-red-600 border-red-600" icon="pi pi-trash"
                                                        :label="t('body.OrderList.delete')"/>
                                                </div>
                                            </div>
                                        </div>
                                    </TabPanel>
                                    <TabPanel :header="t('body.promotion.coupon_tab_list') || 'Danh sách coupon'">
                                        <div class="p-2">
                                            <div class="flex my-2 align-items-center justify-content-between">
                                                <IconField iconPosition="left">
                                                    <InputIcon class="pi pi-search"> </InputIcon>
                                                    <InputText v-model="dataEdit.searchByCouponCode"
                                                        @keydown.enter="fetchAllCouponLine(dataEdit.id)"
                                                        placeholder="Tìm kiếm mã Coupon" />
                                                </IconField>
                                                <Dropdown @change="fetchAllCouponLine(dataEdit.id)"
                                                    v-model="dataEdit.searchByCouponStatus" optionLabel="label"
                                                    :options="filterStatusOptions" class="w-17rem"></Dropdown>
                                            </div>
                                            <DataTable scrollable scrollHeight="400px" paginator :rows="dataEdit.rows"
                                                :page="dataEdit.page" :totalRecords="dataEdit.totalRecords" lazy
                                                @page="onCouponPageChange($event)"
                                                v-model:selection="dataEdit.selectedCouponToExport"
                                                :value="dataEdit.listCouponLineData" :rowsPerPageOptions="[10, 20, 50]">
                                                <template #empty>
                                                    <div class="p-4 text-center">
                                                        <span class="p-column-title">{{ t('body.promotion.no_matching_result_message') }}</span>
                                                    </div>
                                                </template>
                                                <Column selectionMode="multiple" headerStyle="width: 3rem"></Column>
                                                <Column field="couponCode" :header="t('body.promotion.coupon_code_label')"></Column>
                                                <Column field="releaseDate" :header="t('body.promotion.release_date_column')">
                                                    <template #body="sp">
                                                        <div v-if="sp.data.releaseDate">
                                                            {{ convertTime(sp.data.releaseDate) }}
                                                        </div>
                                                    </template>
                                                </Column>
                                                <Column field="usingDate" :header="t('body.promotion.voucher_using_date_label')"></Column>
                                                <Column field="status" :header="t('body.promotion.coupon_status_label')">
                                                    <template #body="sp">
                                                        <div>
                                                            {{ formatCouponStt(sp.data.status) }}
                                                        </div>
                                                    </template>
                                                </Column>
                                            </DataTable>
                                            <div class="flex align-items-center justify-content-end mt-2">
                                                <Button icon="pi pi-file-export" :label="t('body.report.export_excel_button_1')"/>
                                            </div>
                                        </div>
                                    </TabPanel>
                                    <TabPanel :header="t('body.promotion.voucher_tab_history')">
                                        <div class="p-2">
                                            <DataTable :value="[]">
                                                <template #empty>
                                                    <div class="p-4 text-center">
                                                        <span class="p-column-title">{{ t('body.promotion.no_matching_result_message') }}</span>
                                                    </div>
                                                </template>
                                                <Column field="code" :header="t('body.promotion.history_code')"></Column>
                                                <Column field="name" :header="t('body.promotion.history_time')"></Column>
                                                <Column field="name" :header="t('body.promotion.history_seller')"></Column>
                                                <Column field="category" :header="t('body.promotion.history_user')"></Column>
                                                <Column field="quantity" :header="t('body.promotion.history_revenue')"></Column>
                                                <Column field="discount" :header="t('body.promotion.history_voucher_value')"></Column>
                                            </DataTable>
                                            <div class="flex align-items-center justify-content-end mt-2">
                                                <Button icon="pi pi-file-export" :label="t('body.report.export_excel_button_1')"/>
                                            </div>
                                        </div>
                                    </TabPanel>
                                </TabView>
                            </div>
                        </template>
                    </DataTable>
                </div>
            </div>
        </div>
    </div>
    <OverlayPanel ref="op">
        <div class="flex flex-column flex-wrap gap-4">
            <div v-for="(item, index) in column" :key="index" class="flex align-items-center">
                <Checkbox v-model="selectedColumn" :value="item" @change="chooseColumn()"></Checkbox>
                <label class="ml-2"> {{ item.header }} {{ item.value }} </label>
            </div>
        </div>
    </OverlayPanel>
    <!-- Thêm mới khách hàng -->
    <Dialog position="top" :draggable="false" v-model:visible="releaseModal" modal :header="dataEdit.status == 'fix'
        ? `${t('body.promotion.update_button')} ${dataEdit.couponName}`
        : t('body.promotion.create_coupon_dialog_title')
        " :style="{ width: '1020px' }">
        <TabView>
            <TabPanel :header="t('body.promotion.coupon_tab_info') || 'Thông tin'">
                <div class="grid pt-3">
                    <div class="col">
                        <div class="flex flex-column my-2 gap-2">
                            <label class="text-base font-medium text-gray-700" for="">{{t('body.promotion.coupon_code_label')}}</label>
                            <InputText :placeholder="t('body.promotion.coupon_code_value')" class="w-full" v-model="payload.couponCode"></InputText>
                        </div>
                        <div class="flex flex-column my-2 gap-2">
                            <label class="text-base font-medium text-gray-700" for="">{{t('body.promotion.coupon_name_label')}}</label>
                            <InputText class="w-full" v-model="payload.couponName"></InputText>
                            <small v-if="submited && validate(payload.couponName)" class="text-red-500">{{ t('body.promotion.validation_name_required') || 'Vui lòng nhập tên đợt phát hành' }}</small>
                        </div>
                        <div class="flex flex-column my-2 gap-2">
                            <label class="text-base font-medium text-gray-700" for="">{{t('body.promotion.coupon_value_label')}}</label>
                            <div class="flex gap-2 align-items-center">
                                <InputNumber class="w-full" v-model="payload.discount" suffix=" %" :min="0" :max="100">
                                </InputNumber>
                                <span class="text-base white-space-nowrap">{{ t('body.promotion.max_value_label') }}</span>
                                <InputNumber class="w-full" v-model="payload.maxDiscountAmount" suffix=" VND" :min="0">
                                </InputNumber>
                            </div>
                            <small v-if="submited && !payload.discount" class="text-red-500">{{ t('body.promotion.validation_denomination_required') || 'Vui lòng nhập giá trị coupon' }}</small>
                        </div>
                        <div class="flex flex-column my-2 gap-2">
                            <label class="text-base font-medium text-gray-700" for="">{{t('body.promotion.effective_from_label')}}</label>
                            <div class="flex gap-2 align-items-center">
                                <Calendar showIcon iconDisplay="input" v-model="payload.fromDate" />
                                <span class="text-base white-space-nowrap">{{ t('body.promotion.to_date_label') }}</span>
                                <Calendar showIcon iconDisplay="input" v-model="payload.toDate" />
                            </div>
                        </div>
                        <div class="flex white-space-nowrap my-2 gap-2">
                            <label class="text-base font-medium text-gray-700" for="">{{t('body.promotion.coupon_status_label')}}</label>
                            <div class="flex gap-4">
                                <div class="flex align-items-center">
                                    <RadioButton v-model="payload.couponStatus" value="Y" />
                                    <label for="ingredient1" class="ml-2">{{ t('body.promotion.coupon_status_active') }}</label>
                                </div>
                                <div class="flex align-items-center">
                                    <RadioButton v-model="payload.couponStatus" value="N" />
                                    <label for="ingredient1" class="ml-2">{{ t('body.promotion.coupon_status_inactive') }}</label>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="col">
                        <div class="flex flex-column my-2 gap-2">
                            <label class="text-base font-medium text-gray-700" for="">{{t('body.promotion.coupon_product_label')}}</label>
                            <NodeGItem :data_req="dataGroupItem" @change="couponItem = $event" />
                        </div>
                        <div class="flex flex-column my-2 gap-2">
                            <label class="text-base font-medium text-gray-700" for="">{{t('body.promotion.coupon_total_amount_label')}}</label>
                            <InputNumber class="w-full" v-model="payload.amount"></InputNumber>
                        </div>
                        <div class="flex flex-column my-2 gap-2">
                            <label class="text-base font-medium text-gray-700" for="">{{t('body.promotion.coupon_notes_label') || t('body.PurchaseRequestList.note_label')}}</label>
                            <Textarea autoResize :placeholder="t('body.PurchaseRequestList.note_placeholder')" rows="5" cols="30" v-model="payload.couponDescription" />
                        </div>
                    </div>
                </div>
            </TabPanel>
            <TabPanel :header="t('body.promotion.coupon_tab_list') || 'Danh sách coupon'">
                <div v-if="dataEdit.status == 'fix'">
                    <div class="text-right mb-3 flex gap-2 justify-content-end">
                        <Button icon="pi pi-ellipsis-v" :label="t('body.OrderList.actions')" @click="handleCouponLine" severity="info" />
                        <Button icon="pi pi-plus" :label="t('body.promotion.dialog_list_voucher_title')" @click="dialogListCoupon = true" />
                    </div>
                    <DataTable scrollable scrollHeight="500px" v-model:selection="dataEdit.selectedCoupon" paginator
                        :rows="dataEdit.rows" :page="dataEdit.page" :totalRecords="dataEdit.totalRecords" lazy
                        @page="onCouponPageChange($event)" dataKey="id" :value="dataEdit.listCouponLineData"
                        :rowsPerPageOptions="[10, 20, 50]" tableStyle="min-width: 50rem" showGridlines>
                        <template #empty>
                            <div class="p-4 text-center">
                                <span class="p-column-title">{{ t('body.promotion.no_matching_result_message') }}</span>
                            </div>
                        </template>
                        <Column selectionMode="multiple" headerStyle="width: 3rem"></Column>
                        <Column field="couponCode" :header="t('body.promotion.coupon_code_label')"></Column>
                        <Column field="releaseDate" :header="t('body.promotion.release_date_column')">
                            <template #body="sp">
                                <div v-if="sp.data.releaseDate">
                                    {{ convertTime(sp.data.releaseDate) }}
                                </div>
                            </template>
                        </Column>
                        <Column field="usingDate" :header="t('body.promotion.voucher_using_date_label')"></Column>
                        <Column field="status" :header="t('body.promotion.coupon_status_label')">
                            <template #body="sp">
                                {{ formatCouponStt(sp.data.status) }}
                            </template>
                        </Column>
                    </DataTable>
                </div>
                <div v-else>
                    <div class="card text-center">
                        <span>{{ t('body.promotion.voucher_dialog_before_save_message') }}</span>
                    </div>
                </div>
            </TabPanel>
        </TabView>
        <template #footer>
            <div class="flex align-items-center gap-2 justify-content-end">
                <Button icon="pi pi-save" :label="t('body.promotion.save_button')" @click="SaveCoupon"/>
                <Button @click="releaseModal = false" class="bg-gray-500 text-white border-gray-500" icon="pi pi-times"
                    :label="t('body.home.cancel_button')"/>
            </div>
        </template>
    </Dialog>
    <Dialog position="top" :draggable="false" v-model:visible="dialogListCoupon" modal :header="t('body.promotion.dialog_list_voucher_title')"
        :style="{ width: '500px' }">
        <div class="mt-3 p-2">
            <FloatLabel class="mb-5">
                <InputNumber id="quantitycoupon" v-model="listCouponLine.quantitycoupon" class="w-full" />
                <label for="quantitycoupon">{{ t('body.promotion.dialog_list_quantity_label') }}</label>
            </FloatLabel>
            <FloatLabel class="mb-5">
                <InputNumber :min="1" :max="20" id="lengthcoupon" v-model="listCouponLine.lengthcoupon"
                    class="w-full" />
                <label for="lengthcoupon">{{ t('body.promotion.dialog_list_length_label') }}</label>
            </FloatLabel>
            <FloatLabel class="mb-5">
                <InputText id="startwithcoupon" v-model="listCouponLine.startwithcoupon" class="w-full" />
                <label for="startwithcoupon">{{ t('body.promotion.dialog_list_start_char_label') }}</label>
            </FloatLabel>
            <FloatLabel class="mb-5">
                <InputText id="endwithcoupon" v-model="listCouponLine.endwithcoupon" class="w-full" />
                <label for="endwithcoupon">{{ t('body.promotion.dialog_list_end_char_label') }}</label>
            </FloatLabel>
        </div>
        <template #footer>
            <div>
                <Button icon="pi pi-save" :label="t('body.home.confirm_button')" @click="SaveListCoupon" />
                <Button icon="pi pi-ban" :label="t('body.home.cancel_button')" @click="dialogListCoupon = !dialogListCoupon"
                    severity="secondary" />
            </div>
        </template>
    </Dialog>
    <Dialog position="top" :draggable="false" v-model:visible="removeModal" :style="{ width: '450px' }"
        :header="t('body.home.confirm_button')" :modal="true">
        <div class="flex align-items-center justify-content-center">
            <i class="pi pi-exclamation-triangle mr-3" style="font-size: 2rem" />
            <span v-if="dataRemove.couponName">{{ t('body.promotion.confirm_remove_prefix') || 'Bạn chắc chắn muốn xóa' }} <b>{{ dataRemove.couponName }}</b> ?</span>
        </div>
        <template #footer>
            <Button :label="t('body.home.cancel_button')" icon="pi pi-times" outlined @click="removeModal = false" />
            <Button :label="t('body.home.confirm_button')" icon="pi pi-check" @click="confirmRemove(dataRemove)" />
        </template>
    </Dialog>
    <OverlayPanel ref="opHandle">
        <div class="flex flex-column gap-2">
            <Button @click="releaseCouponLineDialog('R')" class="w-full" :label="t('body.promotion.publish_voucher')" icon="pi pi-check"
                severity="secondary"/>
            <Button class="w-full" :label="t('body.promotion.print_voucher_code')" icon="pi pi-qrcode" severity="secondary"/>
            <Button @click="releaseCouponLineDialog('C')" class="w-full" :label="t('body.promotion.cancel_voucher')" icon="pi pi-trash"
                severity="secondary"/>
        </div>
    </OverlayPanel>
    <Dialog position="top" :draggable="false" v-model:visible="releaseCouponLineModal" :style="{ width: '500px' }"
        :header="t('body.home.confirm_button')" :modal="true">
        <div>
            <h6 v-if="dataEdit.typeSubmit == 'R'" class="text-center">
                {{ t('body.promotion.confirm_release_selected_coupons') || 'Bạn chắc chắn phát hành danh sách coupon đã chọn?' }}
            </h6>
            <h6 v-if="dataEdit.typeSubmit == 'C'" class="text-center">
                {{ t('body.promotion.confirm_cancel_selected_coupons') || 'Bạn chắc chắn hủy phát hành danh sách coupon đã chọn?' }}
            </h6>
        </div>
        <template #footer>
            <Button :label="t('body.home.cancel_button')" icon="pi pi-times" outlined @click="releaseCouponLineModal = false" />
            <Button :label="t('body.home.confirm_button')" icon="pi pi-check" @click="confirmReleaseCouponLine()" />
        </template>
    </Dialog>
    <Loadding v-if="isLoading"></Loadding>
</template>
<script setup>
import { onMounted, ref } from "vue";
import API from '@/api/api-main-1'
import { format } from "date-fns";
import merge from "lodash/merge";
import { getCurrentInstance } from "vue";
import { useToast } from "primevue/usetoast";
import { useRouter } from "vue-router";
import { useI18n } from 'vue-i18n';

const { t } = useI18n();
const { proxy } = getCurrentInstance();
const toast = useToast();
const API_URL = ref("Coupon");
const router = useRouter();
const op = ref();
const opHandle = ref();
const releaseCouponLineModal = ref(false);
const submited = ref(false);
const dataEdit = ref({
    searchByCouponStatus: {
        label: t('body.sampleRequest.customer.all_status_option'),
        value: "",
    },
    rows: 10,
    page: 0,
    totalRecords: 0,
});
const filterStatusOptions = ref([
    {
        label: t('body.sampleRequest.customer.all_status_option'),
        value: "",
    },
    {
        label: t('body.promotion.status_not_used'),
        value: "NU",
    },
    {
        label: t('body.promotion.status_issued'),
        value: "R",
    },
    {
        label: t('body.promotion.status_used'),
        value: "U",
    },
    {
        label: t('body.promotion.status_cancelled'),
        value: "C",
    },
]);
const removeModal = ref(false);
const dataDetailCoupon = ref("");
const releaseModal = ref(false);
const dialogListCoupon = ref(false);
const isLoading = ref(true);
const expandedRows = ref({});
const Coupon = ref([]);
const keySearchProduct = ref("");
const selectedColumn = ref([]);
const createDate = ref(true);
const listCouponLine = ref({});
const dataSelected = ref({});
const rows = ref(10);
const page = ref(0);
const totalRecords = ref();
const dataRemove = ref({});
const defaultColumns = ref([
    {
        field: "couponCode",
        header: t('body.sampleRequest.customer.release_code_column') || "Mã đợt phát hành",
        style: "text-align: start;",
        headerStyle: "min-width: 10rem;",
        type: "text",
    },
    {
        field: "couponName",
        header: t('body.sampleRequest.customer.release_name_column') || "Tên đợt phát hành",
        style: "text-align: start;",
        headerStyle: "min-width: 10rem;",
        type: "text",
    },
    {
        field: "fromDate",
        header: t('body.sampleRequest.customer.from_date_column') || "Từ ngày",
        style: "text-align: start;",
        headerStyle: "min-width: 10rem;",
        type: "time",
    },
    {
        field: "toDate",
        header: t('body.sampleRequest.customer.to_date_column') || "Đến ngày",
        style: "text-align: start;",
        headerStyle: "min-width: 10rem;",
        type: "time",
    },
    {
        field: "quantity",
        header: t('body.sampleRequest.customer.quantity_column') || "Số lượng",
        style: "text-align: start;",
        headerStyle: "min-width: 10rem;",
        type: "number",
    },
]);
const column = ref([
    {
        field: "couponCode",
        header: t('body.sampleRequest.customer.release_code_column') || "Mã đợt phát hành",
        style: "text-align: start;",
        headerStyle: "min-width: 10rem;",
        type: "text",
    },
    {
        field: "couponName",
        header: t('body.sampleRequest.customer.release_name_column') || "Tên đợt phát hành",
        style: "text-align: start;",
        headerStyle: "min-width: 10rem;",
        type: "text",
    },
    {
        field: "fromDate",
        header: t('body.sampleRequest.customer.from_date_column') || "Từ ngày",
        style: "text-align: start;",
        headerStyle: "min-width: 10rem;",
        type: "time",
    },
    {
        field: "toDate",
        header: t('body.sampleRequest.customer.to_date_column') || "Đến ngày",
        style: "text-align: start;",
        headerStyle: "min-width: 10rem;",
        type: "time",
    },
    {
        field: "quantity",
        header: t('body.sampleRequest.customer.quantity_column') || "Số lượng",
        style: "text-align: start;",
        headerStyle: "min-width: 10rem;",
        type: "number",
    },
    {
        field: "discount",
        header: t('body.promotion.coupon_value_label') || "Giá trị coupon",
        style: "text-align: start;",
        headerStyle: "min-width: 10rem;",
        type: "doubleField",
        doubleField: ["discount", "maxDiscountAmount"],
    },
    {
        field: "creator",
        header: t('body.OrderApproval.creator') || "Người tạo",
        style: "text-align: start;",
        headerStyle: "min-width: 10rem;",
        type: "text",
    },
    {
        field: "couponDescription",
        header: t('body.promotion.coupon_notes_label') || "Ghi chú",
        style: "text-align: start;",
        headerStyle: "min-width: 10rem;",
        type: "text",
    },
    {
        field: "couponStatus",
        header: t('body.promotion.coupon_status_label') || "Trạng thái",
        style: "text-align: start;",
        headerStyle: "min-width: 10rem;",
        type: "statusField",
    },
]);
let futureDate = new Date();
futureDate.setMonth(futureDate.getMonth() + 6);
const payload = ref({
    // id: 0,
    couponCode: "",
    couponName: "",
    couponDescription: "",
    amount: 0,
    discount: 0,
    maxDiscountAmount: 0,
    fromDate: new Date(),
    toDate: futureDate,
    couponStatus: "Y",
    creator: "",
    updator: "",
    couponItem: [],
    couponLine: [],
});
const couponItem = ref([]);
const dataGroupItem = ref([]);
const styleSticky = ref(null);
onMounted(() => {
    fetchAllCoupon();
    if (localStorage.getItem("selectedColumnCoupons")) {
        selectedColumn.value = JSON.parse(localStorage.getItem("selectedColumnCoupons"));
    } else {
        selectedColumn.value = defaultColumns.value;
    }
    window.addEventListener("scroll", handleScroll);
});
const rowClass = (data) => {
    if (dataSelected.value.check == false) return [{ "": dataSelected.value === data }];

    return [{ "bg-primary": dataSelected.value === data }];
};
const openRemoveDialog = (data) => {
    removeModal.value = true;
    dataRemove.value = { ...data };
};
const DetailCoupon = async (event) => {
    if (expandedRows.value[event.data.id] != undefined) {
        expandedRows.value = {};
        dataSelected.value.check = false;
        rowClass(event.data);
    } else {
        dataSelected.value.check = true;
        try {
            const res = await API.get(`${API_URL.value}/${event.data.id}`);
            expandedRows.value = [res.data].reduce((acc, p) => (acc[p.id] = true) && acc, {});
            Coupon.value[findIndexById(event.data.id)] = merge(
                {},
                Coupon.value[findIndexById(event.data.id)],
                res.data
            );
            dataSelected.value = Coupon.value[findIndexById(event.data.id)];
            rowClass(Coupon.value[findIndexById(event.data.id)]);
            dataEdit.value.id = event.data.id;
            fetchAllCouponLine(dataEdit.value.id);
        } catch (error) {
            console.error(error);
        }
    }
};
const fetchAllCoupon = async () => {
    try {
        const res = await API.get(`${API_URL.value}?skip=${page.value}&limit=${rows.value}`);
        Coupon.value = res.data.items;
        totalRecords.value = res.data.total;
        router.push(`?skip=${page.value}&limit=${rows.value}`);
    } catch (err) {
        console.error(error);
    } finally {
        isLoading.value = false;
    }
};
const onPageChange = (event) => {
    rows.value = event.rows;
    page.value = event.page;
    fetchAllCoupon();
};
const openUpdCouponDialog = (data = null) => {
    if (data != null) {
        dataEdit.value = merge({}, dataEdit.value, data);
        payload.value = merge({}, payload.value, data);
        payload.value.fromDate = new Date(payload.value.fromDate);
        payload.value.toDate = new Date(payload.value.toDate);
        couponItem.value = payload.value.couponItem;
        couponItem.value.forEach((item) => {
            item.name = item.itmsGrpName ? `${t('body.promotion.group_prefix') || 'Nhóm'} ${item.itmsGrpName}` : item.itemName;
        });
        dataGroupItem.value = couponItem.value;

        dataEdit.value.status = "fix";
        fetchAllCouponLine(dataEdit.value.id);
    } else {
        dataEdit.value.status = "new";
        resetData();
    }
    releaseModal.value = true;
};
const confirmRemove = async (data) => {
    try {
        const res = await API.delete(`${API_URL.value}/${data.id}`);

    } catch (error) {
        console.error(error);
    } finally {
        fetchAllCoupon();
        proxy.$notify("S", `${t('body.promotion.delete_success_message')} ${data.couponName}!`, toast);
        removeModal.value = false;
    }
};
const setNullValues = (obj) => {
    for (let key in obj) {
        if (obj.hasOwnProperty(key)) {
            if (typeof obj[key] === "object" && obj[key] !== null) {
                setNullValues(obj[key]);
            } else {
                obj[key] = null;
            }
        }
    }
};
const searchProduct = async () => {
    if (keySearchProduct.value === "") {
        fetchAllCoupon();
        return;
    }
    try {
        isLoading.value = true;
        constres = await API.get(`Coupon/search/${keySearchProduct.value}`);
        Coupon.value = res.data.Coupon;
    } catch (error) {
        proxy.$notify("E", error, toast);
    } finally {
        isLoading.value = false;
    }
};
const chooseColumn = () => {
    localStorage.setItem("selectedColumnCoupons", JSON.stringify(selectedColumn.value));
};
const convertTime = (originalDate) => {
    const timeZone = "Asia/Bangkok"; // Múi giờ +7
    return format(originalDate, "dd/MM/yyyy", { timeZone });
};
const SaveCoupon = async () => {
    const datacouponItem = [];
    couponItem.value.forEach((element) => {
        datacouponItem.push({
            id: 0,
            couponId: 0,
            type: element.type == "G" ? element.type : "I",
            itemId: element.type != "G" ? element.id : 0,
            itemGroupId: element.type == "G" ? element.id : 0,
            itemCode: element.type != "G" ? element.itemCode : "",
            itemName: element.type != "G" ? element.itemName : "",
            itmsGrpName: element.type == "G" ? element.itmsGrpName : "",
        });
    });
    payload.value.couponItem = datacouponItem;
    submited.value = true;
    if (!validateData()) return;
    try {
        const API_END_POINT = payload.value.id
            ? `${API_URL.value}/${payload.value.id}`
            : `${API_URL.value}/add`;
        const FUN_API = payload.value.id
            ? API.update(API_END_POINT, payload.value)
            : API.add(API_END_POINT, payload.value);
        const res = await FUN_API;
        const message = payload.value.id ? t('body.promotion.update_success_message')  : t('body.promotion.add_success_message') ;
        if (res.data) {
            proxy.$notify("S", message, toast);
        }
    } catch (error) {
        proxy.$notify("E", error, toast);
    } finally {
        resetData();
    }
};
const validateData = () => {
    let status = true;
    if (payload.value.couponName == "") status = false;
    return status;
};
const findIndexById = (id) => {
    let index = -1;
    for (let i = 0; i < Coupon.value.length; i++) {
        if (Coupon.value[i].id === id) {
            index = i;
            break;
        }
    }
    return index;
};

const resetData = () => {
    releaseModal.value = false;
    dialogListCoupon.value = false;
    expandedRows.value = {};
    dataSelected.value.check = false;
    dataGroupItem.value = [];
    clearData();
    submited.value = false;
    fetchAllCoupon();
};

const SaveListCoupon = async () => {
    const data = {
        couponId: dataEdit.value.id,
        quantity: listCouponLine.value.quantitycoupon,
        length: listCouponLine.value.lengthcoupon,
        startChar: listCouponLine.value.startwithcoupon,
        endChar: listCouponLine.value.endwithcoupon,
    };
    try {
        const res = await API.add(`${API_URL.value}/addCoupon`, data);
        proxy.$notify("S", t('body.promotion.add_list_vouchers_success', { count: res.data.length }) || `Thêm thành công ${res.data.length} mã coupon!`, toast);
    } catch (e) {

    } finally {
        fetchAllCouponLine(dataEdit.value.id);

        dialogListCoupon.value = false;
    }
};

const validate = (field) => {
    if (!field) return true;
    if (field.replace(/\s+/g, " ") == "") return true;
    if (/\s/.test(field.replace(/\s+/g, " ")) && field.replace(/\s+/g, " ").length == 1)
        return true;
    return false;
};
const clearData = () => {
    payload.value = {
        // id: 0,
        couponCode: "",
        couponName: "",
        couponDescription: "",
        amount: 0,
        discount: 0,
        maxDiscountAmount: 0,
        fromDate: new Date(),
        toDate: futureDate,
        couponStatus: "Y",
        creator: "",
        updator: "",
        couponItem: [],
        couponLine: [],
    };
};
const fetchAllCouponLine = async (id) => {
    const searchByCouponStatusKey = dataEdit.value.searchByCouponStatus;
    try {
        const res = await API.get(
            `${API_URL.value}/getCouponLine?skip=${dataEdit.value.page}&limit=${dataEdit.value.rows
            }&id=${id}&status=${searchByCouponStatusKey.value ? searchByCouponStatusKey.value : ""
            }&couponCode=${dataEdit.value.searchByCouponCode ? dataEdit.value.searchByCouponCode : ""
            }`
        );
        dataEdit.value.listCouponLineData = res.data.items;
        dataEdit.value.totalRecords = res.data.total;
    } catch (error) {
        console.error(error);
    }
};
const onCouponPageChange = (event) => {
    dataEdit.value.rows = event.rows;
    dataEdit.value.page = event.page;
    fetchAllCouponLine(dataEdit.value.id);
};
const handleCouponLine = (event) => {
    opHandle.value.toggle(event);
};
const formatCouponStt = (stt) => {
    switch (stt) {
        case "Y":
            return t('body.promotion.coupon_status_active') ;
        case "N":
            return t('body.promotion.coupon_status_inactive') ;
    }
};
const releaseCouponLineDialog = (type) => {
    releaseCouponLineModal.value = true;
    dataEdit.value.typeSubmit = type;
};
const confirmReleaseCouponLine = async () => {
    const data = dataEdit.value.selectedCoupon;
    const listCouponId = [];
    data.map((item) => {
        listCouponId.push({
            id: item.id,
            couponId: item.couponId,
        });
    });
    try {
        let URL_ENDPOINT = `status=C`;
        if (dataEdit.value.typeSubmit == "R") URL_ENDPOINT = `status=R`;
        const res = await API.update(
            `${API_URL.value}/updateCoupon?id=${dataEdit.value.id}&${URL_ENDPOINT}`,
            listCouponId
        );
        proxy.$notify("S", t('body.promotion.publish_voucher_success') , toast);
    } catch (error) {
        proxy.$notify("E", `${error.response?.data?.message || error}`, toast);
        console.error(error);
    } finally {
        releaseCouponLineModal.value = false;
        fetchAllCouponLine(dataEdit.value.id);
    }
};
const formatStatusCoupon = (stt) => {
    switch (stt) {
        case "Y":
            return t('body.promotion.coupon_status_active');
        case "N":
            return t('body.promotion.coupon_status_inactive') ;
    }
};
const handleScroll = () => {
    if (window.scrollY >= 109) {
        styleSticky.value = {
            position: "fixed",
            top: "0px",
            bottom: "auto",
            width: "82%",
            right: 0,
        };
    } else {
        styleSticky.value = {};
    }
};
</script>
<style scoped></style>
