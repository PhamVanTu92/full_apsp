<template>
    <div>
        <div class="grid">
            <div class="col-2">
                <div>
                    <h3 class="font-bold m-0 mb-4">{{ t('body.sampleRequest.customer.voucher_title') }}</h3>
                </div>
                <div style="max-height: 77vh" class="overflow-auto">
                    <div class="card shadow-1 p-0 mb-5">
                        <div class="px-3 py-2">
                            <div class="flex justify-content-between align-items-center py-2 px-3">
                                <div><span class="m-0 font-bold text-base">{{ t('body.PurchaseRequestList.status') }}</span></div>
                                <div>
                                    <Button text icon="pi pi-angle-right" @click="createDate = !createDate" />
                                </div>
                            </div>
                        </div>
                        <transition name="fade">
                            <div class="px-3 py-2" v-if="createDate">
                                <div class="flex align-items-center mb-3">
                                    <RadioButton></RadioButton>
                                    <label class="ml-2">{{ t('body.sampleRequest.customer.all_status_option') }}</label>
                                </div>
                                <div class="flex align-items-center mb-3">
                                    <RadioButton></RadioButton>
                                    <label class="ml-2"> {{ t('body.sampleRequest.customer.active_status_option') }} </label>
                                </div>
                                <div class="flex align-items-center mb-3">
                                    <RadioButton></RadioButton>
                                    <label class="ml-2"> {{ t('body.sampleRequest.customer.inactive_status_option') }}</label>
                                </div>
                            </div>
                        </transition>
                    </div>
                </div>
            </div>
            <!-- Table -->
            <div class="col-10" :style="[styleSticky]">
                <div class="flex justify-content-between align-items-center m-0 mb-3">
                    <div class="w-6">
                        <IconField iconPosition="left">
                            <InputText type="text" :placeholder="t('body.sampleRequest.customer.voucher_search_placeholder')"
                                @keydown.enter="searchProduct()" class="w-full" v-model="keySearchProduct" />
                            <InputIcon class="pi pi-search" />
                        </IconField>
                    </div>
                    <div class="flex gap-2">
                        <Button @click="openReleaseDialog()" icon="pi pi-plus" :label="t('body.sampleRequest.customer.add_new_release_button')"/>
                        <Button icon="pi pi-align-justify" @click="op.toggle($event)" />
                    </div>
                </div>
                <div style="max-height: 77vh" class="card p-2 shadow-1">
                    <DataTable rowHover paginator :rows="rows" :page="page" lazy :totalRecords="totalRecords"
                        @page="onPageChange($event)"
                         :currentPageReportTemplate="`${t('body.productManagement.display')} {first} - {last} ${t('body.productManagement.total_of')} {totalRecords} ${t('body.systemSetting.orders')}`"
                        scrollable scrollHeight="70vh" stripedRows showGridlines v-model:expandedRows="expandedRows"
                        @row-click="DetailVoucher" dataKey="id"
                        paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
                        :rowClass="rowClass" :rowsPerPageOptions="[10, 20, 50]" :value="Vouchers"
                        tableStyle="min-width: 50rem">
                        <template #empty>
                            <div class="py-5 my-5 text-center">
                                {{ t('body.sampleRequest.customer.no_data_message') }}
                            </div>
                        </template>
                        <template #expansion="slotProps">
                            <div>
                                <TabView>
                                    <TabPanel :header="t('body.promotion.voucher_tab_info')">
                                        <div class="p-2">
                                            <div class="grid">
                                                <div class="col">
                                                    <div
                                                        class="flex gap-2 pb-2 my-4 border-bottom-1 border-gray-200 align-items-center">
                                                        <span class="text-gray-700" for="">{{ t('body.promotion.voucher_code_label') }} </span>
                                                        <span class="text-base text-gray-700 font-semibold">{{
                                                            slotProps.data.voucherCode
                                                            }}</span>
                                                    </div>
                                                    <div
                                                        class="flex gap-2 pb-2 my-4 border-bottom-1 border-gray-200 align-items-center">
                                                        <span class="text-gray-700" for="">{{ t('body.report.time_label_3') }} </span>
                                                        <span class="text-base text-gray-700 font-semibold">{{
                                                            convertTime(slotProps.data.fromDate)
                                                            }} -
                                                            {{ convertTime(slotProps.data.toDate) }}</span>
                                                    </div>
                                                    <div
                                                        class="flex gap-2 pb-2 my-4 border-bottom-1 border-gray-200 align-items-center">
                                                        <span class="text-gray-700" for="">{{ t('body.promotion.coupon_product_label') }} </span>
                                                        <div class="flex gap-1 flex-wrap">
                                                            <div v-for="(item, index) in slotProps.data.voucherItem"
                                                                :key="index"
                                                                class="p-1 font-semibold bg-gray-200 border-round mb-2">
                                                                {{
                                                                    item.itemGroupId
                                                                        ? `${t('body.promotion.group_prefix') || 'Nhóm'} ${item.itmsGrpName}`
                                                                : item.itemName
                                                                }}
                                                            </div>
                                                        </div>
                                                    </div>
                                                    <div
                                                        class="flex gap-2 pb-2 my-4 border-bottom-1 border-gray-200 align-items-center">
                                                        <span class="text-gray-700" for="">{{ t('body.PurchaseRequestList.status') }} </span>
                                                        <span class="text-base text-gray-700 font-semibold">{{
                                                            formatStatusVoucher(slotProps.data.voucherStatus)
                                                            }}</span>
                                                    </div>
                                                </div>
                                                <div class="col">
                                                    <div
                                                        class="flex gap-2 pb-2 my-4 border-bottom-1 border-gray-200 align-items-center">
                                                        <span class="text-gray-700" for="">{{ t('body.sampleRequest.customer.release_name_column') }} </span>
                                                        <span class="text-base text-gray-700 font-semibold">{{
                                                            slotProps.data.voucherName
                                                            }}</span>
                                                    </div>
                                                    <div
                                                        class="flex gap-2 pb-2 my-4 border-bottom-1 border-gray-200 align-items-center">
                                                        <span class="text-gray-700" for="">{{ t('body.promotion.denomination_label') }} </span>
                                                        <span class="text-base text-gray-700 font-semibold">{{
                                                            convertNumber(slotProps.data.denomination)
                                                            }}</span>
                                                    </div>
                                                    <div
                                                        class="flex gap-2 pb-2 my-4 border-bottom-1 border-gray-200 align-items-center">
                                                        <span class="text-gray-700" for="">{{ t('body.promotion.min_total_amount_label') }}
                                                        </span>
                                                        <span class="text-base text-gray-700 font-semibold">{{
                                                            convertNumber(slotProps.data.amount)
                                                            }}</span>
                                                    </div>
                                                </div>
                                                <div class="col">
                                                    <Textarea disabled v-model="slotProps.data.voucherDescription"
                                                        autoResize :placeholder="t('body.PurchaseRequestList.note_placeholder')" rows="5" cols="30" />
                                                </div>
                                            </div>
                                            <div class="flex align-items-center justify-content-end">
                                                <div class="flex gap-2">
                                                    <Button @click="openUpdVoucherDialog(slotProps.data)"
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
                                    <TabPanel :header="t('body.promotion.voucher_tab_list')">
                                        <div class="p-2">
                                            <div class="flex my-2 align-items-center justify-content-between">
                                                <IconField iconPosition="left">
                                                    <InputIcon class="pi pi-search"> </InputIcon>
                                                    <InputText v-model="dataEdit.searchByVoucherCode"
                                                        @keyup.enter="fetchAllVoucherLine(dataEdit.id)"
                                                        :placeholder="t('body.promotion.voucher_search_placeholder')" />
                                                </IconField>
                                                <Dropdown @change="fetchAllVoucherLine(dataEdit.id)" optionLabel="label"
                                                    v-model="dataEdit.searchByVoucherStatus"
                                                    :options="filterStatusOptions" class="w-17rem">
                                                </Dropdown>
                                            </div>
                                            <DataTable scrollable scrollHeight="500px" dataKey="id"
                                                v-model:selection="dataEdit.selectedVouchersToExport" showGridlines
                                                paginator :rows="dataEdit.rows" :page="dataEdit.page"
                                                :totalRecords="dataEdit.totalRecords" lazy
                                                :value="dataEdit.listVoucherLine"
                                                @page="onPageVoucherLineChange($event)"
                                                :rowsPerPageOptions="[10, 20, 50]">
                                                <template #empty>
                                                    <div class="p-4 text-center">
                                                        <span class="p-column-title">{{ t('body.promotion.no_matching_result_message') }}</span>
                                                    </div>
                                                </template>
                                                <Column selectionMode="multiple" headerStyle="width: 3rem"></Column>

                                                <Column field="voucherCode" :header="t('body.promotion.voucher_code_column')"></Column>
                                                <Column field="name" :header="t('body.promotion.buyer_column')"></Column>
                                                <Column field="releaseDate" :header="t('body.promotion.release_date_column')">
                                                    <template #body="slotProps">
                                                        <div v-if="slotProps.data.releaseDate">
                                                            {{ convertTime(slotProps.data.releaseDate) }}
                                                        </div>
                                                    </template>
                                                </Column>
                                                <Column field="expDate" :header="t('body.promotion.voucher_expiry_date_label')">
                                                    <template #body="slotProps">
                                                        <div v-if="slotProps.data.expDate">
                                                            {{ convertTime(slotProps.data.expDate) }}
                                                        </div>
                                                    </template>
                                                </Column>
                                                <Column field="usingDate" :header="t('body.promotion.voucher_using_date_label')">
                                                    <template #body="slotProps">
                                                        <div v-if="slotProps.data.usingDate">
                                                            {{ convertTime(slotProps.data.usingDate) }}
                                                        </div>
                                                    </template>
                                                </Column>
                                                <Column field="amount" :header="t('body.promotion.voucher_value_column')"> </Column>
                                                <Column field="status" :header="t('body.promotion.voucher_status_label')">
                                                    <template #body="slotProps">
                                                        <div>
                                                            {{ formatVoucherStt(slotProps.data.status) }}
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
                                                <Column field="quantity" :header="t('body.promotion.history_voucher_value')"></Column>
                                            </DataTable>
                                            <div class="flex align-items-center justify-content-end mt-2">
                                                <Button icon="pi pi-file-export" :label="t('body.report.export_excel_button_1')"/>
                                            </div>
                                        </div>
                                    </TabPanel>
                                </TabView>
                            </div>
                        </template>
                        <Column v-for="col of selectedColumn" :key="col.field" :field="col.field" :header="col.header">
                            <template #body="slotProps">
                                <div v-if="col.type == 'text'">
                                    {{ getNestedValue(slotProps.data, col.field) }}
                                </div>
                                <div v-if="col.type == 'date'">
                                    {{ convertTime(getNestedValue(slotProps.data, col.field)) }}
                                </div>
                                <div v-if="col.field == 'voucherStatus' && col.type == 'bool'">
                                    {{ formatStatusVoucher(slotProps.data.voucherStatus) }}
                                </div>
                                <div v-if="col.field == 'denomination' && col.type == 'double'">
                                    {{ convertNumber(slotProps.data.denomination) }}
                                </div>
                                <div v-if="col.field == 'amount' && (col.header == t('body.PurchaseRequestList.quantity_column') || col.header == 'Số lượng')">
                                    {{ convertNumber(slotProps.data.amount) }}
                                </div>
                            </template>
                        </Column>
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
    <Dialog position="top" :draggable="false" v-model:visible="releaseModal" modal
        :header="dataEdit.update == 'update' ? `${t('body.promotion.update_promotion_page_title')} ${dataEdit.voucherName}` : t('body.promotion.voucher_dialog_title')"
        :style="{ width: '1020px' }">
        <TabView>
            <TabPanel :header="t('body.promotion.voucher_tab_info')">
                <div class="grid pt-3">
                    <div class="col">
                        <div class="flex flex-column my-2 gap-2">
                            <label class="text-base font-medium text-gray-700" for="">{{t('body.promotion.voucher_code_label')}}</label>
                            <InputText v-model="voucherData.voucherCode" class="w-full" :placeholder="t('body.promotion.voucher_code_value')">
                            </InputText>
                        </div>
                        <div class="flex flex-column my-2 gap-2">
                            <label class="text-base font-medium text-gray-700" for="">{{t('body.promotion.voucher_name_label')}}</label>
                            <InputText v-model="voucherData.voucherName" class="w-full"></InputText>
                            <small v-if="submited && !voucherData.voucherName" class="text-red-500">
                                {{ t('body.promotion.validation_name_required') }}</small>
                        </div>
                        <div class="flex flex-column my-2 gap-2">
                            <label class="text-base font-medium text-gray-700" for="">{{t('body.promotion.denomination_label')}}</label>
                            <InputNumber v-model="voucherData.denomination" class="w-full"></InputNumber>
                            <small class="text-red-500" v-if="submited && !voucherData.denomination">{{ t('body.promotion.validation_denomination_required') }}</small>
                        </div>
                        <div class="flex white-space-nowrap my-2 gap-2">
                            <label class="text-base font-medium text-gray-700" for="">{{t('body.promotion.voucher_status_label')}}
                            </label>
                            <div class="flex gap-4">
                                <div class="flex align-items-center">
                                    <RadioButton value="Y" v-model="voucherData.voucherStatus" />
                                    <label for="voucherStatus" class="ml-2">{{ t('body.promotion.voucher_status_active') }}</label>
                                </div>
                                <div class="flex align-items-center">
                                    <RadioButton value="N" v-model="voucherData.voucherStatus" />
                                    <label for="voucherStatus" class="ml-2">{{ t('body.promotion.voucher_status_inactive') }}</label>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="col">
                        <div class="flex flex-column my-2 gap-2">
                            <label class="text-base font-medium text-gray-700" for="">{{t('body.promotion.product_label')}}</label>
                            <NodeGItem :data_req="dataGroupItem" @change="SetData($event)" />
                        </div>
                        <div class="flex flex-column my-2 gap-2">
                            <label class="text-base font-medium text-gray-700" for="">{{t('body.promotion.total_amount_label')}}
                            </label>
                            <InputNumber v-model="voucherData.amount" class="w-full"></InputNumber>
                        </div>
                        <div class="flex flex-column my-2 gap-2">
                            <label class="text-base font-medium text-gray-700" for="">{{t('body.promotion.voucher_notes_label')}}</label>
                            <Textarea v-model="voucherData.voucherDescription" autoResize rows="5" cols="30" />
                        </div>
                    </div>
                </div>
            </TabPanel>
            <TabPanel :header="t('body.promotion.voucher_tab_application')">
                <div class="pt-4 border-bottom-1 border-gray-200 pb-4">
                    <h3 class="text-lg font-semibold">{{ t('body.promotion.voucher_tab_application') }}</h3>
                    <div class="flex gap-3">
                        <div :class="{ 'text-gray-400': voucherData.isExp == 'N' }"
                            class="flex align-items-center gap-4">
                            <div class="flex gap-2">
                                <RadioButton value="Y" v-model="voucherData.isExp"></RadioButton>
                                <label class="text-base font-semibold" for="">{{ t('body.report.time_label_3') }}</label>
                            </div>
                            <div class="flex gap-2 align-items-center">
                                <Calendar v-model="voucherData.fromDate" class="w-10rem" showIcon iconDisplay="input"
                                    dateFormat="dd/mm/yy" :disabled="voucherData.isExp == 'N'" />
                                <span>{{ t('body.promotion.to_date_column') }}</span>
                                <Calendar v-model="voucherData.toDate" class="w-10rem" showIcon iconDisplay="input"
                                    dateFormat="dd/mm/yy" :disabled="voucherData.isExp == 'N'" />
                            </div>
                        </div>
                        <div :class="{ 'text-gray-400': voucherData.isExp == 'Y' }"
                            class="flex align-items-center gap-4">
                            <div class="flex gap-2">
                                <RadioButton value="N" v-model="voucherData.isExp"></RadioButton>
                                <label class="text-base font-semibold" for="">{{ t('body.promotion.effective_from_label') }}</label>
                            </div>
                            <div class="flex gap-2 align-items-center">
                                <InputText v-model="voucherData.expDays" :disabled="voucherData.isExp == 'Y'"
                                    class="w-5rem">
                                </InputText>
                                <Dropdown v-model="voucherData.isDate" :options="dateOptions" optionValue="value"
                                    optionLabel="label" :disabled="voucherData.isExp == 'Y'" class="w-5rem" />
                                <span>{{ t('body.promotion.effective_from_label') }}</span>
                            </div>
                        </div>
                    </div>
                </div>
                <div>
                    <h3 class="text-lg font-semibold pt-4">{{ t('client.scopeTitle') }}</h3>
                    <div class="grid pt-4">
                        <div class="col">
                            <div class="flex align-items-center mb-4">
                                <RadioButton value="Y" v-model="voucherData.isAllSystem" />
                                <label class="ml-2">{{ t('client.scopeSystemWide') }}</label>
                            </div>
                            <div class="flex align-items-center">
                                <RadioButton value="N" v-model="voucherData.isAllSystem" />
                                <div class="align-items-center gap-2 flex white-space-nowrap">
                                    <label class="ml-2">{{ t('body.promotion.branch_label') }}</label>
                                    <AutoComplete v-model="branchValue" :suggestions="Branch" @complete="SearchBranch"
                                        optionLabel="branchName" multiple :disabled="voucherData.isAllSystem == 'Y'"
                                        :placeholder="t('body.promotion.select_branch_placeholder')">
                                    </AutoComplete>
                                </div>
                            </div>
                            <div class="pt-4">
                                <div class="flex align-items-center mb-4">
                                    <RadioButton value="Y" v-model="voucherData.isAllSeller" />
                                    <label class="ml-2">{{ t('client.scopeAllCreators') }}</label>
                                </div>
                            </div>
                        </div>
                        <div class="col">
                            <div class="flex align-items-center mb-4">
                                <RadioButton value="Y" v-model="voucherData.isAllCustomer" />
                                <label for="voucherStatus" class="ml-2">{{ t('client.scopeAllCustomers') }}</label>
                            </div>
                            <div class="flex align-items-center">
                                <RadioButton value="N" v-model="voucherData.isAllCustomer" />
                                <div class="align-items-center gap-2 flex white-space-nowrap">
                                    <label class="ml-2">{{ t('client.scopeCustomerGroup') }}</label>
                                    <AutoComplete v-model="customerGValue" :disabled="voucherData.isAllCustomer == 'Y'"
                                        multiple :suggestions="CustomerGroup" optionLabel="groupName"
                                        @complete="SearchCustomerGroup" :placeholder="t('body.OrderList.select_customer_placeholder')">
                                    </AutoComplete>
                                </div>
                            </div>
                            <small class="text-red-500" v-if="
                                submited &&
                                customerGValue.length < 1 &&
                                voucherData.isAllCustomer == 'N'
                            ">{{ t('body.promotion.validation_select_customer_group') }}</small>
                        </div>
                    </div>
                </div>
            </TabPanel>
            <TabPanel :header="t('body.promotion.voucher_tab_list')">
                <div v-if="dataEdit.update !== 'update'">
                    <div class="card text-center">
                        <span>{{ t('body.promotion.voucher_dialog_before_save_message') }}</span>
                    </div>
                </div>
                <div v-else>
                    <div class="flex align-items-center justify-content-end gap-2">
                        <Button v-if="dataEdit.checked == 'Y'" icon="pi pi-ellipsis-v" :label="t('body.OrderList.actions')" @click="handleVoucherLine" severity="help" />

                        <Button @click="openAddNewVoucherLine()" icon="pi pi-plus" :label="t('body.promotion.add_vouchers_dialog_title')"/>
                        <Button @click="openAddNewListVoucherLine()" icon="pi pi-plus" :label="t('body.promotion.dialog_list_voucher_title')"/>
                        <Button icon="pi pi-file-import" :label="t('body.promotion.import_label')" severity="info"/>
                    </div>
                    <div>
                        <div class="flex my-2 align-items-center justify-content-between">
                            <IconField iconPosition="left">
                                <InputIcon class="pi pi-search"> </InputIcon>
                                <InputText v-model="dataEdit.searchByVoucherCode"
                                    @keyup.enter="fetchAllVoucherLine(dataEdit.id)" :placeholder="t('body.promotion.voucher_search_placeholder')" />
                            </IconField>
                            <Dropdown @change="fetchAllVoucherLine(dataEdit.id)" optionLabel="label"
                                v-model="dataEdit.searchByVoucherStatus" :options="filterStatusOptions" class="w-17rem">
                            </Dropdown>
                        </div>
                        <DataTable scrollable scrollHeight="500px" dataKey="id"
                            v-model:selection="dataEdit.selectedVouchers" @change="onSelectedVouchersChange"
                            :value="dataEdit.listVoucherLine" showGridlines paginator lazy :rows="dataEdit.rows"
                            :page="dataEdit.page" :totalRecords="dataEdit.totalRecords"
                            :rowsPerPageOptions="[10, 20, 50]" @page="onPageVoucherLineChange($event)">
                            <template #empty>
                                <div class="p-4 text-center">
                                    <span class="p-column-title">{{ t('body.promotion.no_matching_result_message') }}</span>
                                </div>
                            </template>
                            <Column selectionMode="multiple" headerStyle="width: 3rem"></Column>
                            <Column field="voucherCode" :header="t('body.promotion.voucher_code_column')"></Column>
                            <Column field="name" :header="t('body.promotion.buyer_column')"></Column>
                            <Column field="releaseDate" :header="t('body.promotion.release_date_column')">
                                <template #body="slotProps">
                                    <div v-if="slotProps.data.releaseDate">
                                        {{ convertTime(slotProps.data.releaseDate) }}
                                    </div>
                                </template>
                            </Column>
                            <Column field="expDate" :header="t('body.promotion.voucher_expiry_date_label')">
                                <template #body="slotProps">
                                    <div v-if="slotProps.data.expDate">
                                        {{ convertTime(slotProps.data.expDate) }}
                                    </div>
                                </template>
                            </Column>
                            <Column field="usingDate" :header="t('body.promotion.voucher_using_date_label')">
                                <template #body="slotProps">
                                    <div v-if="slotProps.data.usingDate">
                                        {{ convertTime(slotProps.data.usingDate) }}
                                    </div>
                                </template>
                            </Column>
                            <Column field="amount" :header="t('body.promotion.voucher_value_column')"> </Column>
                            <Column field="status" :header="t('body.promotion.voucher_status_label')">
                                <template #body="slotProps">
                                    <div>
                                        {{ formatVoucherStt(slotProps.data.status) }}
                                    </div>
                                </template>
                            </Column>
                        </DataTable>
                    </div>
                </div>
            </TabPanel>
        </TabView>
        <template #footer>
            <div class="flex align-items-center gap-2 justify-content-end">
                <Button @click="addVouchers()" icon="pi pi-save" :label="t('body.promotion.save_button')"/>
                <Button @click="releaseModal = false" class="bg-gray-500 text-white border-gray-500" icon="pi pi-times"
                    :label="t('body.promotion.skip_button')"/>
            </div>
        </template>
    </Dialog>

    <Dialog position="top" :draggable="false" v-model:visible="removeModal" :style="{ width: '450px' }"
        header="{{ t('body.OrderList.delete') }}" :modal="true">
        <div class="flex align-items-center justify-content-center">
            <i class="pi pi-exclamation-triangle mr-3" style="font-size: 2rem" />
            <span v-if="dataRemove.voucherName">{{ t('body.promotion.confirm_remove_prefix') || 'Bạn chắc chắn muốn xóa' }} <b>{{ dataRemove.voucherName }}</b> ?</span>
        </div>
        <template #footer>
            <Button :label="t('body.home.cancel_button')" icon="pi pi-times" outlined @click="removeModal = false" />
            <Button :label="t('body.home.confirm_button')" icon="pi pi-check" @click="confirmRemove(dataRemove)" />
        </template>
    </Dialog>

    <Dialog position="top" :draggable="false" v-model:visible="addNewVoucherLineModal" :style="{ width: '700px' }"
        :header="t('body.promotion.add_vouchers_dialog_title')" :modal="true">
        <div v-for="(item, index) in vouchersLineData" :key="index" class="flex gap-2 align-items-center w-full mb-2">
            <label class="text-base font-semibold white-space-nowrap" for="">{{ t('body.promotion.voucher_line_code_label') }}</label>
            <InputText v-model="item.voucherCode" class="w-full"></InputText>
            <Button @click="removeVCodeFromList(item)" text icon="pi pi-times-circle"/>
        </div>
        <div class="flex justify-content-start pt-3">
            <Button :disabled="vouchersLineData[0].voucherCode == ''" @click="addOneVoucherCode()" severity="secondary"
                icon="pi pi-plus-circle" :label="t('body.promotion.add_vouchers_dialog_title')"/>
        </div>
        <template #footer>
            <Button :label="t('body.home.cancel_button')" icon="pi pi-times" outlined @click="addNewVoucherLineModal = false" />
            <Button @click="saveVoucherCode()" :label="t('body.home.confirm_button')" icon="pi pi-check" />
        </template>
    </Dialog>

    <Dialog position="top" :draggable="false" v-model:visible="dialogListVoucher" modal :header="t('body.promotion.dialog_list_voucher_title')"
        :style="{ width: '500px' }">
        <div class="mt-3 p-2">
            <FloatLabel class="mb-5">
                <InputNumber id="quantityVoucher" v-model="dataEdit.quantityVoucher" class="w-full" />
                <label for="quantityVoucher">{{ t('body.promotion.dialog_list_quantity_label') }}</label>
            </FloatLabel>
            <FloatLabel class="mb-5">
                <InputNumber id="lengthVoucher" v-model="dataEdit.lengthVoucher" class="w-full" />
                <label for="lengthVoucher">{{ t('body.promotion.dialog_list_length_label') }}</label>
            </FloatLabel>
            <FloatLabel class="mb-5">
                <InputText id="startWithVoucher" v-model="dataEdit.startWithVoucher" class="w-full" />
                <label for="startWithVoucher">{{ t('body.promotion.dialog_list_start_char_label') }}</label>
            </FloatLabel>
            <FloatLabel class="mb-5">
                <InputText id="endWithVoucher" v-model="dataEdit.endWithVoucher" class="w-full" />
                <label for="endWithVoucher">{{ t('body.promotion.dialog_list_end_char_label') }}</label>
            </FloatLabel>
        </div>
        <template #footer>
            <div>
                <Button icon="pi pi-save" :label="t('body.home.confirm_button')" @click="SaveListVoucher" />
                <Button icon="pi pi-ban" :label="t('body.home.cancel_button')" @click="dialogListVoucher = !dialogListVoucher"
                    severity="secondary" />
            </div>
        </template>
    </Dialog>
    <OverlayPanel ref="opHandle">
        <div class="flex flex-column gap-2">
            <Button @click="openConfigReleaseVoucher()" class="w-full" :label="t('body.promotion.publish_voucher')" icon="pi pi-check"
                severity="secondary"/>
            <Button class="w-full" :label="t('body.promotion.print_voucher_code')" icon="pi pi-qrcode" severity="secondary"/>
            <Button @click="releaseVoucherLineDialog('C')" class="w-full" :label="t('body.promotion.cancel_voucher')" icon="pi pi-trash"
                severity="secondary"/>
        </div>
    </OverlayPanel>
    <Dialog position="top" :draggable="false" v-model:visible="releaseVoucherLineModal" :style="{ width: '500px' }"
        :header="t('body.home.confirm_button')" :modal="true">
        <div>
            <h6 v-if="dataEdit.typeSubmit == 'C'" class="text-center">
                {{ t('body.promotion.confirm_cancel_selected_vouchers') }}
            </h6>
        </div>
        <template #footer>
            <Button :label="t('body.home.cancel_button')" icon="pi pi-times" outlined @click="releaseVoucherLineModal = false" />
            <Button :label="t('body.home.confirm_button')" icon="pi pi-check" @click="confirmDiscardVoucherLine()" />
        </template>
    </Dialog>

    <Dialog position="top" :draggable="false" v-model:visible="configReleaseVoucherModal" :style="{ width: '1000px' }"
        :header="t('body.promotion.release_voucher_title')" :modal="true">
        <div class="grid">
            <div class="col-6 flex flex-column gap-3">
                <div class="flex justify-content-between align-items-center text-center">
                    <h6 class="m-0">{{ t('body.promotion.dialog_list_quantity_label') }}:</h6>
                    <h6 class="m-0">{{ dataEdit.selectedVouchers ? dataEdit.selectedVouchers.length : 0 }}</h6>
                </div>
                <div class="flex gap-2 justify-content-between">
                    <h6>{{ t('body.promotion.transaction_type_label') || 'Hình thức' }}</h6>
                    <div class="flex gap-2">
                        <div class="flex items-center">
                            <RadioButton @change="onTypeReleaseVoucherChange" value="S" name="Sale"
                                v-model="releaseVoucherData.type" />
                            <label class="ml-2">{{ t('body.promotion.buy_label') || 'Bán' }}</label>
                        </div>
                        <div class="flex items-center">
                            <RadioButton @change="onTypeReleaseVoucherChange" value="D" name="Donate"
                                v-model="releaseVoucherData.type" />
                            <label class="ml-2">{{ t('body.promotion.give_label') || 'Tặng' }}</label>
                        </div>
                    </div>
                </div>
                <div class="flex flex-wrap gap-2">
                    <h6 class="m-0">{{ t('body.promotion.release_date_label') || 'Ngày phát hành' }}</h6>
                    <Calendar showIcon iconDisplay="input" class="w-full" v-model="releaseVoucherData.releaseDate">
                    </Calendar>
                </div>
                <div class="flex flex-wrap gap-2">
                    <h6 class="m-0">{{ t('body.promotion.buyer_group_label') || 'Nhóm người mua/nhận' }}</h6>
                    <Dropdown v-model="releaseVoucherData.selectedBuyerGrp" @change="onBuyerGrpChange($event)"
                        :options="voucherLineDataConfig.buyerGroupOpt" optionLabel="label" class="w-full"></Dropdown>
                </div>
                <div class="flex flex-column gap-2">
                    <h6 class="m-0">{{ t('body.promotion.buyer_name_label') || 'Tên người mua/nhận' }}</h6>
                    <AutoComplete v-model="keySearchNameBuyer" :delay="300" :suggestions="nameOfBuyer"
                        optionLabel="cardName" @complete="fetchAllBuyers">
                    </AutoComplete>
                </div>
            </div>
            <div class="col-6 flex flex-column gap-3">
                <div class="flex flex-column gap-2">
                    <h6 class="m-0">{{ t('body.promotion.voucher_amount_label') || 'Giá trị Voucher' }}</h6>
                    <InputNumber v-model="releaseVoucherData.amount" :disabled="releaseVoucherData.type == 'D'"
                        class="w-full">
                    </InputNumber>
                </div>
                <div class="flex align-items-center justify-content-between gap-2">
                    <h6 class="m-0">{{ t('body.promotion.payment_customer_label') || 'Khách thanh toán' }}</h6>
                    <strong>{{ convertNumber((dataEdit.selectedVouchers || []).length * releaseVoucherData.amount) }}</strong>
                </div>
                <div class="flex flex-wrap gap-2">
                    <h6 class="m-0">{{ t('body.promotion.payment_method_label') || 'Phương thức' }}</h6>
                    <Dropdown @change="onChangeTransMed($event)" v-model="releaseVoucherData.paymentMethod"
                        optionLabel="label" :options="voucherLineDataConfig.transactionMed" class="w-full"></Dropdown>
                </div>
                <div v-if="voucherLineDataConfig.selectedTransMed == 'Credit'" class="flex flex-wrap gap-2 mb-2">
                    <h6 class="m-0">{{ t('body.promotion.account_number_label') || 'Số tài khoản' }}</h6>
                    <InputNumber class="w-full"></InputNumber>
                </div>
                <Textarea v-model="releaseVoucherData.note" class="w-full mt-2" :placeholder="t('body.PurchaseRequestList.note_placeholder')" autoResize
                    rows="5" cols="30"></Textarea>
            </div>
        </div>
        <template #footer>
            <Button :label="t('body.home.cancel_button')" icon="pi pi-times" outlined @click="releaseVoucherLineModal = false" />
            <Button :label="t('body.home.confirm_button')" icon="pi pi-check" @click="confirmReleaseVouchers()" />
        </template>
    </Dialog>
    <loadding v-if="isLoading"></loadding>
</template>
<script setup>
import { onMounted, ref } from "vue";
import API from '@/api/api-main-1'
import { format } from "date-fns";
import merge from "lodash/merge";
import { useToast } from "primevue/usetoast";
import { getCurrentInstance } from "vue";
const { proxy } = getCurrentInstance();
import { useRouter } from "vue-router";
import { debounce } from "lodash";
import { useI18n } from 'vue-i18n';

const { t } = useI18n();
const API_URL = ref("Voucher");
const toast = useToast();
const configReleaseVoucherModal = ref(false);
const dataRemove = ref({});
const removeModal = ref(false);
const dialogListVoucher = ref(false);
const nameOfBuyer = ref([])
const releaseVoucherData = ref({
    type: "S",
    amount: 0,
    selectedBuyerGrp: {
        label: t('body.promotion.buyer_group_customer') || 'Khách hàng',
        value: 'C',
        url: 'Customer'
    },
    paymentMethod: {
        label: t('body.promotion.transaction_cash') || "Tiền mặt",
        value: "Cash",
    },
})
const releaseVoucherDataToClear = JSON.stringify(releaseVoucherData.value)
const voucherLineDataConfig = ref({

    transactionMed: [
        {
            label: t('body.promotion.transaction_cash') || "Tiền mặt",
            value: "Cash",
        },
        {
            label: t('body.promotion.transaction_card') || "Thẻ",
            value: "Credit",
        },
        {
            label: t('body.promotion.transaction_bank') || "Chuyển khoản",
            value: "Bank",
        },
        {
            label: t('body.promotion.transaction_wallet') || "Ví",
            value: "Wallet",
        },
    ],
    buyerGroupOpt: [
        {
            label: t('body.promotion.buyer_group_customer') || 'Khách hàng',
            value: 'C',
            url: 'Customer'
        },
        {
            label: t('body.promotion.buyer_group_vendor') || 'Nhà cung cấp',
            value: 'S',
            url: 'Vendor'
        },

        {
            label: t('body.promotion.buyer_group_staff') || "Nhân viên",
        },
        {
            label: t('body.promotion.buyer_group_delivery') || "Đối tác giao hàng",
        },
        {
            label: t('body.promotion.buyer_group_other') || 'Khác',
            value: 'O',
            url: ''
        },
    ],
}); //Phát hành voucher(trong thao tác)
const URL_BASE = ref("")
const keySearchNameBuyer = ref("")
const router = useRouter();
const rows = ref(10);
const page = ref(0);
const totalRecords = ref();
const releaseVoucherLineModal = ref(false);
const op = ref();
const releaseModal = ref(false);
const addNewVoucherLineModal = ref(false);
const isLoading = ref(false)
const Branch = ref([])
const expandedRows = ref({});
const Vouchers = ref([]);
const keySearchProduct = ref("");
const selectedColumn = ref([]);
const createDate = ref(true);
const dataSelected = ref({});
const opHandle = ref();
const defaultColumns = ref([
    {
        field: "voucherCode",
        header: t('body.sampleRequest.customer.release_code_column') || "Mã đợt phát hành",
        style: "text-align: start;",
        headerStyle: "min-width: 10rem;",
        type: "text",
    },
    {
        field: "voucherName",
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
        type: "date",
    },
    {
        field: "toDate",
        header: t('body.sampleRequest.customer.to_date_column') || "Đến ngày",
        style: "text-align: start;",
        headerStyle: "min-width: 10rem;",
        type: "date",
    },
    {
        field: "amount",
        header: t('body.sampleRequest.customer.quantity_column') || "Số lượng",
        style: "text-align: start;",
        headerStyle: "min-width: 10rem;",
        type: "number",
    },
]);
const column = ref([
    {
        field: "voucherCode",
        header: t('body.sampleRequest.customer.release_code_column') || "Mã đợt phát hành",
        style: "text-align: start;",
        headerStyle: "min-width: 10rem;",
        type: "text",
    },
    {
        field: "voucherName",
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
        type: "date",
    },
    {
        field: "toDate",
        header: t('body.sampleRequest.customer.to_date_column') || "Đến ngày",
        style: "text-align: start;",
        headerStyle: "min-width: 10rem;",
        type: "date",
    },
    {
        field: "amount",
        header: t('body.sampleRequest.customer.quantity_column') || "Số lượng",
        style: "text-align: start;",
        headerStyle: "min-width: 10rem;",
        type: "number",
    },
    {
        field: "denomination",
        header: t('body.promotion.denomination_label') || "Mệnh giá",
        style: "text-align: start;",
        headerStyle: "min-width: 10rem;",
        type: "double",
    },
    {
        field: "creator",
        header: t('body.OrderApproval.creator') || "Người tạo",
        style: "text-align: start;",
        headerStyle: "min-width: 10rem;",
        type: "text",
    },
    {
        field: "voucherDescription",
        header: t('body.promotion.voucher_notes_label') || "Ghi chú",
        style: "text-align: start;",
        headerStyle: "min-width: 10rem;",
        type: "text",
    },
    {
        field: "voucherStatus",
        header: t('body.promotion.voucher_status_label') || "Trạng thái",
        style: "text-align: start;",
        headerStyle: "min-width: 10rem;",
        type: "bool",
    },
]);
const filterStatusOptions = ref([
    {
        label: t('body.sampleRequest.customer.all_status_option') || "Tất cả",
        value: "",
    },
    {
        label: t('body.promotion.status_not_used') || "Chưa sử dụng",
        value: "NU",
    },
    {
        label: t('body.promotion.status_issued') || "Đã phát hành",
        value: "R",
    },
    {
        label: t('body.promotion.status_used') || "Đã sử dụng",
        value: "U",
    },
    {
        label: t('body.promotion.status_cancelled') || "Đã hủy",
        value: "C",
    },
]);
const dataGroupItem = ref([]);
const voucherItem = ref([]);
const dataEdit = ref({
    searchByVoucherStatus: {
        label: t('body.sampleRequest.customer.all_status_option') || "Tất cả",
        value: "",
    },
    rows: 10,
    page: 0,
    totalRecords: 0,
});
const selectionKeysVoucherItem = ref("");
const CustomerGroup = ref([]);
const dateOptions = ref([
    {
        label: t('body.promotion.date_option_day'),
        value: "Y",
    },
    {
        label: t('body.promotion.date_option_month'),
        value: "N",
    },
]);
const customerGValue = ref([]);
const branchValue = ref([])
let futureDate = new Date();
futureDate.setMonth(futureDate.getMonth() + 6);
const vouchersLineData = ref([
    {
        voucherId: dataEdit.value ? dataEdit.value.id : 0,
        voucherCode: "",
    },
]);
const voucherData = ref({
    voucherCode: "",
    voucherName: "",
    voucherDescription: "",
    amount: 0,
    denomination: 0,
    fromDate: new Date(),
    toDate: futureDate,
    expDays: 0,
    isMerger: "N",
    isAllSystem: "Y",
    isAllCustomer: "N",
    isAllSeller: "N",
    voucherStatus: "Y",
    creator: "",
    updator: "",
    voucherCustomer: [],
    voucherItem: [],
    voucherLine: [],
});
const submited = ref(false);
const styleSticky = ref(null);
const User = ref({})
onMounted(() => {
    if (localStorage.getItem("user")) {

        let data = JSON.parse(localStorage.getItem("user"))
        User.value = data.appUser
    }
    fetchAllVouchers();
    if (localStorage.getItem("selectedColumnVouchers")) {
        selectedColumn.value = JSON.parse(localStorage.getItem("selectedColumnVouchers"));
    } else {
        selectedColumn.value = defaultColumns.value;
    }

    window.addEventListener("scroll", handleScroll);
});
const rowClass = (data) => {
    if (dataSelected.value.check == false) return [{ "": dataSelected.value === data }];

    return [{ "bg-primary": dataSelected.value === data }];
};

const DetailVoucher = async (event) => {
    if (expandedRows.value[event.data.id] != undefined) {
        expandedRows.value = {};
        dataSelected.value.check = false;

        rowClass(event.data);
    } else {
        dataSelected.value.check = true;

        try {
            const res = await API.get(`${API_URL.value}/${event.data.id}`);
            expandedRows.value = [res.data].reduce((acc, p) => (acc[p.id] = true) && acc, {});
            Vouchers.value[findIndexById(event.data.id)] = res.data;
            dataSelected.value = res.data;
            dataEdit.value.id = res.data.id;
            fetchAllVoucherLine(dataEdit.value.id);

            rowClass(res.data);
        } catch (e) {

        }
    }
};
const fetchAllVouchers = async () => {
    isLoading.value = true;
    try {
        const res = await API.get(`${API_URL.value}?skip=${page.value}&limit=${rows.value}`)
        Vouchers.value = res.data.items;
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
    fetchAllVouchers();
};
const openAddNewVoucherLine = () => {
    vouchersLineData.value[0].voucherId = dataEdit.value.id;
    addNewVoucherLineModal.value = true;
};
const addVouchers = async () => {
    const dataVoucherItem = [];
    const dataVoucherCustomer = [];
    const dataVoucherBranch = []
    branchValue.value.forEach((item) => {
        dataVoucherBranch.push({
            id: 0,
            voucherId: 0,
            systemId: item.id,
            systemCode: "",
            systemName: item.branchName
        })
    })
    customerGValue.value.forEach((item) => {
        dataVoucherCustomer.push({
            id: 0,
            voucherId: 0,
            groupId: item.id || 0,
            groupName: item.groupName,
        });
    });
    voucherItem.value.forEach((item) => {


        dataVoucherItem.push({
            id: item.id ? item.id : 0,
            voucherId: 0,
            type: item.type == "G" ? item.type : "I",
            itemId: item.type != "G" ? item.itemId : 0,
            itemCode: item.type != "G" ? item.itemCode : "",
            itemName: item.type != "G" ? item.itemName : "",
            itemGroupId: item.type == "G" ? item.id : 0,
            status: item.status ? item.status : 'A',
            itmsGrpName: item.type == "G" ? item.itmsGrpName : "",
        });
    });


    // return;
    voucherData.value.voucherCustomer = dataVoucherCustomer;
    voucherData.value.voucherItem = dataVoucherItem;
    voucherData.value.voucherSystem = dataVoucherBranch
    voucherData.value.creator = User.value.fullName
    submited.value = true;
    if (!validateData()) return;
    isLoading.value = true
    try {
        const API_END_POINT = voucherData.value.id
            ? `${API_URL.value}/${voucherData.value.id}`
            : `${API_URL.value}/add`;
        const FUN_API = voucherData.value.id
            ? API.update(API_END_POINT, voucherData.value)
            : API.add(`${API_END_POINT}`, voucherData.value);
        const res = await FUN_API;
        const message = voucherData.value.id
            ? `${t('body.promotion.update_success_message') || 'Cập nhật thành công'} ${res.data.voucherName}`
            : t('body.promotion.add_success_message') || `Thêm mới thành công!`;
        if (res.data) proxy.$notify("S", message, toast);
    } catch (e) {
        proxy.$notify("E", e, toast);
    } finally {
        isLoading.value = false
        resetData();
    }
};
const validateData = () => {
    let status = true;
    switch (status) {
        case voucherData.value.voucherName == "":
            return false;
        case voucherData.value.denomination == "":
            return false;
        case voucherData.value.isAllCustomer == "N" &&
            voucherData.value.voucherCustomer == []:
            return false;
        default:
            status = true;
    }
    return status;
};
const findIndexById = (id) => {
    let index = -1;
    for (let i = 0; i < Vouchers.value.length; i++) {
        if (Vouchers.value[i].id === id) {
            index = i;
            break;
        }
    }
    return index;
};
const convertTime = (originalDate) => {
    const timeZone = "Asia/Bangkok"; // Múi giờ +7
    return format(originalDate, "dd/MM/yyyy", { timeZone });
};
const convertNumber = (num) => {
    let money = new Intl.NumberFormat("vi-VN");
    return money.format(num);
};
const removeVCodeFromList = (data) => {
    if (vouchersLineData.value.length > 1) {
        vouchersLineData.value = vouchersLineData.value.filter((item) => {
            return item != data;
        });
        return;
    }
    vouchersLineData.value[0].voucherCode = "";
};
const openRemoveDialog = (data) => {
    removeModal.value = !removeModal.value;
    dataRemove.value = { ...data };
};
const openAddNewListVoucherLine = () => {
    dialogListVoucher.value = true;
};
const openUpdVoucherDialog = (data = null) => {
    if (data != null) {
        voucherData.value = merge({}, voucherData.value, data);
        dataEdit.value = merge({}, dataEdit.value, data);
        voucherItem.value = voucherData.value.voucherItem;
        voucherData.value.fromDate = new Date(voucherData.value.fromDate);
        voucherData.value.toDate = new Date(voucherData.value.toDate);
        voucherData.value.voucherSystem.forEach(item => {
            item.branchName = item.systemName
        })
        branchValue.value = voucherData.value.voucherSystem
        voucherItem.value.forEach((item) => {
            item.name = item.itmsGrpName ? `${t('body.promotion.group_prefix') || 'Nhóm'} ${item.itmsGrpName}` : item.itemName;
        });
        customerGValue.value = voucherData.value.voucherCustomer
        dataGroupItem.value = voucherItem.value;
        dataEdit.value.update = "update";
    } else {
        resetData();
    }
    releaseModal.value = true;
};
const saveVoucherCode = async () => {
    dataEdit.value.voucherLine = vouchersLineData.value;
    try {
        const res = await API.add(
            `${API_URL.value}/addAVoucher?id=${dataEdit.value.id}`,
            dataEdit.value.voucherLine
        );
        proxy.$notify("S", t('body.promotion.add_vouchers_success') || `Thêm thành công vouchers!`, toast);
    } catch (error) {
        proxy.$notify("E", `${error}`, toast);
    } finally {
        addNewVoucherLineModal.value = false;
        fetchAllVoucherLine(dataEdit.value.id);
        vouchersLineData.value = [{
            voucherId: dataEdit.value.id,
            voucherCode: ""
        }]
    }
};
const addOneVoucherCode = () => {
    vouchersLineData.value.push({
        voucherId: dataEdit.value.id,
        voucherCode: "",
    });
};
const getNestedValue = (obj, path) => {
    return path.split(".").reduce((o, i) => (o ? o[i] : null), obj);
};
const confirmRemove = async (data) => {
    isLoading.value = true
    try {
        const res = await API.delete(`${API_URL.value}/${data.id}`)
        fetchAllVouchers();
        proxy.$notify("S", res.data, toast);

    } catch (e) {
        proxy.$notify("E", e, toast);
    } finally {
        removeModal.value = false;
        isLoading.value = false

    }
};
const openReleaseDialog = () => {
    clearData();
    voucherItem.value = [];
    dataEdit.value.update = 'add'
    releaseModal.value = !releaseModal.value;
};
const getDataGroupItem = (data, key) => {
    data.forEach((item) => {
        if (key.toString() == item.key.toString()) {
            item.name = `${t('body.promotion.group_prefix') || 'Nhóm'} ${item.itmsGrpName}`;
            item.type = "G";
            voucherItem.value.push(item);
        } else {
            if (item.children) {
                getDataGroupItem(item.children, key);
            }
        }
    });
};
const chooseColumn = () => {
    localStorage.setItem("selectedColumnVouchers", JSON.stringify(selectedColumn.value));
};
const formatStatusVoucher = (stt) => {
    switch (stt) {
        case "Y":
            return t('body.sampleRequest.customer.active_status_option') || "Kích hoạt";
        case "N":
            return t('body.sampleRequest.customer.inactive_status_option') || "Chưa áp dụng";
    }
};
const resetData = () => {
    fetchAllVouchers();
    releaseModal.value = false;
    selectionKeysVoucherItem.value = {};
    dataGroupItem.value = [];
    customerGValue.value = [];
    branchValue.value = []
    submited.value = false;
};
const clearData = () => {
    expandedRows.value = {};
    dataSelected.value.check = false;

    branchValue.value = []
    customerGValue.value = []
    voucherData.value = {
        voucherCode: "",
        voucherName: "",
        voucherDescription: "",
        amount: 0,
        denomination: 0,
        isDate: "N",
        fromDate: new Date(),
        toDate: futureDate,
        isExp: "Y",
        expDays: 0,
        isMerger: "N",
        isAllSystem: "Y",
        isAllCustomer: "Y",
        isAllSeller: "Y",
        voucherStatus: "Y",
        voucherSystem: [],
        voucherCustomer: [],
        voucherSeller: [],
        voucherItem: [],
        voucherLine: [],
    };
    dataGroupItem.value = [];
    submited.value = false;
};
const SaveListVoucher = async () => {
    if (dataEdit.value.quantityVoucher < 0 || dataEdit.value.quantityVoucher == "") {
        proxy.$notify("E", t('body.promotion.validation_quantity_required') || "Vui lòng nhập số lượng Voucher", toast);
        return;
    }
    if (dataEdit.value.lengthVoucher < 8 || dataEdit.value.lengthVoucher > 15) {
        proxy.$notify("E", t('body.promotion.validation_length_range') || "Độ dài mã phải lớn hơn 8 và nhỏ hơn 15", toast);
        return;
    }
    const lengthCodeRandom =
        dataEdit.value.lengthVoucher -
        (dataEdit.value.startWithVoucher.length + dataEdit.value.endWithVoucher.length);
    if (lengthCodeRandom < 5) {
        proxy.$notify("E", t('body.promotion.validation_length_min_random') || "Độ dài từ kí tự bắt đầu và kí tự kết thúc phải lớn hơn 5", toast);
        return;
    }
    try {
        const data = {
            voucherId: dataEdit.value.id,
            quantity: dataEdit.value.lengthVoucher,
            length: dataEdit.value.lengthVoucher,
            startChar: dataEdit.value.startWithVoucher,
            endChar: dataEdit.value.endWithVoucher,
        };
        const res = await API.add(`${API_URL.value}/addVoucher`, data);
        proxy.$notify("S", t('body.promotion.add_list_vouchers_success', { count: res.data.length }) || `Thêm thành công ${res.data.length} vouchers!`, toast);
    } catch (e) {

    } finally {
        fetchAllVoucherLine(dataEdit.value.id);
        dialogListVoucher.value = false;
        dataEdit.value.quantityVoucher = 0
        dataEdit.value.lengthVoucher = 0
        dataEdit.value.startWithVoucher = ""
        dataEdit.value.endWithVoucher = ""
    }
};
const fetchAllCustomerGroup = async (key) => {
    try {
        const res = await API.get(`CustomerGroup/search/${key ? key : ""}`);
        CustomerGroup.value = res.data;
    } catch (e) {

    }
};
const debouncedFetchCustomer = debounce(fetchAllCustomerGroup, 300);
const SearchCustomerGroup = (event) => {
    debouncedFetchCustomer(event.query);
};
const fetchAllVoucherLine = async (id) => {
    isLoading.value = true
    try {
        const res = await API.get(
            `${API_URL.value}/getVoucherLine?skip=${dataEdit.value.page}&limit=${dataEdit.value.rows
            }&id=${id}&voucherCode=${dataEdit.value.searchByVoucherCode ? dataEdit.value.searchByVoucherCode : ""
            }&status=${dataEdit.value.searchByVoucherStatus
                ? dataEdit.value.searchByVoucherStatus.value
                : ""
            }`
        );
        dataEdit.value.listVoucherLine = res.data.items;
        dataEdit.value.totalRecords = res.data.total;
    } catch (error) {
        console.error(error);
    } finally {
        isLoading.value = false
    }
};
const formatVoucherStt = (stt) => {
    switch (stt) {
        case "NU":
            return t('body.promotion.status_not_used') || "Chưa sử dụng";
        case "R":
            return t('body.promotion.status_issued') || "Đã phát hành";
        case "U":
            return t('body.promotion.status_used') || "Đã sử dụng";
        case "C":
            return t('body.promotion.status_cancelled') || "Đã hủy";
    }
};
const onPageVoucherLineChange = (evt) => {
    dataEdit.value.rows = evt.rows;
    dataEdit.value.page = evt.page;
    fetchAllVoucherLine(dataEdit.value.id);
};
const handleVoucherLine = (evt) => {
    opHandle.value.toggle(evt);
};
const releaseVoucherLineDialog = (type) => {
    releaseVoucherLineModal.value = true;
    dataEdit.value.typeSubmit = type;
};
const confirmDiscardVoucherLine = async () => {
    isLoading.value = true
    const data = dataEdit.value.selectedVouchers;

    if (!data || data.length < 1) {
        proxy.$notify("E", t('body.promotion.validation_select_voucher_to_cancel') || "Bạn cần chọn Voucher để hủy!", toast);
        return;
    }
    let listVoucherCancel = []
    listVoucherCancel = data.map((val) => {
        return {
            voucherId: val.id
        }
    })
    let payload = {
        id: dataEdit.value.id,
        voucherLine: listVoucherCancel
    }
    try {
        const res = await API.update(`${API_URL.value}/cancelVoucher?id=${dataEdit.value.id}&status=C`, payload);
        if (res.data) proxy.$notify("S", t('body.promotion.cancel_success') || "Hủy thành công!", toast);
    } catch (error) {
        proxy.$notify("E", error, toast);
        console.error(error);
    } finally {
        releaseVoucherLineModal.value = false;
        fetchAllVoucherLine(dataEdit.value.id);
        isLoading.value = false
    }
};
const openConfigReleaseVoucher = () => {
    if (!dataEdit.value.selectedVouchers || dataEdit.value.selectedVouchers.length === 0) {
        proxy.$notify("E", t('body.promotion.validation_select_voucher_to_release') || `Bạn cần chọn Voucher để phát hành!`, toast);
        return;
    }
    releaseVoucherData.value = JSON.parse(releaseVoucherDataToClear)
    configReleaseVoucherModal.value = true;
};
const onChangeTransMed = (event) => {
    voucherLineDataConfig.value.selectedTransMed = event.value.value;
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
const fetchBranch = async (key) => {
    try {
        const res = await API.get(`Branch/search/${key ? key : ""}`)
        Branch.value = res.data
    } catch (error) {
        console.error(error);
    }
}
const debounceSearchBranch = debounce(fetchBranch, 300)
const SearchBranch = (event) => {
    debounceSearchBranch(event.query)

}

const confirmReleaseVouchers = async () => {
    const payload = {
        ...releaseVoucherData.value,
        partnerType: "string",
        partnerId: "string",
        partnerName: "string",
        paymentMethod: "",
        paymentMethodName: "",
        paymentAccountId: 0,
        paymentAccountName: "string",
        paymentAccountUser: "string",
        voucherLine: (dataEdit.value.selectedVouchers || []).map((val) => {
            return {
                voucherId: val.id
            }
        })
    }
    try {
        const res = await API.update(`Voucher/updateVoucher?id=${voucherData.value.id}&status=R`, payload)
        if (res.data) proxy.$notify("S", t('body.promotion.publish_voucher_success') || `Phát hành Voucher thành công!`, toast)
    } catch (error) {
        proxy.$notify("E", error, toast)
    } finally {
        fetchAllVoucherLine(dataEdit.value.id);
        configReleaseVoucherModal.value = false
    }
}
const onBuyerGrpChange = (e) => {

    URL_BASE.value = e.value
    keySearchNameBuyer.value = ""
}

const fetchAllBuyers = async () => {
    try {
        const res = await API.get(`${URL_BASE.value.url || releaseVoucherData.value.selectedBuyerGrp.url}/search/${keySearchNameBuyer.value}`)
        nameOfBuyer.value = res.data
    } catch (error) {

    }
}
const onSelectedVouchersChange = (e) => {
    if (dataEdit.value.selectedVouchers && dataEdit.value.selectedVouchers.length > 0) {
        return dataEdit.value.checked = "Y"
    }
    return dataEdit.value.checked = "N"
}
const onTypeReleaseVoucherChange = () => {
    releaseVoucherData.value.amount = 0
}
const SetData = (event) => {
    voucherItem.value.forEach(item => {
        if (event.length) {
            const check = event.filter((val) => {
                return val.id == item.id;
            })
            if (check.length == 0) return item.status = 'D';

        }
    })

    return voucherItem.value = event
}
</script>
<style scoped></style>
