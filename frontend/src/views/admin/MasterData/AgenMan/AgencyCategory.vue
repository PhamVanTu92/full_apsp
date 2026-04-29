<template>
    <!-- Header -->
    <template v-if="!infoWindows">
        <div class="flex justify-content-between align-items-center mb-3">
            <h4 class="font-bold m-0">{{ t('body.systemSetting.customers') }}</h4>
            <Button class="mr-3" icon="pi pi-plus" @click="openAddDialog"
                :label="t('body.productManagement.add_new_button')" v-if="false" />
        </div>
        <!-- End Header -->
        <!-- Data -->
        <template v-if="true">
            <!-- !errorsuppliersData -->
            <template v-if="true">
                <div class="card">
                    <DataTable :value="suppliersData.items" showGridlines tableStyle="min-width: 50rem" lazy
                        :totalRecords="pagable.total"
                        :rowsPerPageOptions="Array.from({ length: 10 }, (_, i) => (i + 1) * 10)" :rows="pagable.rows"
                        :page="pagable.page" :first="pagable.page * pagable.rows" dataKey="id" paginator
                        @page="onPageChange($event)" filterDisplay="menu" v-model:filters="filterStore.filters"
                        :filterLocale="'vi'" @filter="onFilter" scrollable scrollHeight="70vh">
                        <template #empty>
                            <div class="p-4 text-center">{{ t('body.systemSetting.no_data_to_display') }}</div>
                        </template>
                        <template #loading>
                            <Loading />
                        </template>
                        <template #header>
                            <div class="flex justify-content-between">
                                <IconField iconPosition="left">
                                    <InputText class="w-20rem"
                                        :placeholder="t('body.sampleRequest.customer.search_placeholder')"
                                        v-model="filterStore.filters['global'].value" @input="debounceF" />
                                    <InputIcon>
                                        <i class="pi pi-search" @click="onFilter()" />
                                    </InputIcon>
                                </IconField>
                                <div>
                                    <Button icon="pi pi-filter-slash" v-tooltip.bottom="t('body.OrderApproval.clear')"
                                        severity="danger" outlined @click="clearFilter()" />
                                    <Button icon="pi pi-sync" class="ml-2"
                                        :label="t('body.productManagement.sync_button')" @click="onClickSync"
                                        :loading="loadingSync" />
                                </div>
                            </div>
                            <!-- <AUserSelector
                                v-model="user"
                                optionValue="id"
                            ></AUserSelector>
                            <div>
                                {{ user }}
                            </div> -->
                        </template>
                        <Column header="#">
                            <template #body="sp">
                                <div>
                                    {{ sp.index + 1 }}
                                </div>
                            </template>
                        </Column>

                        <Column field="cardCode" :header="t('body.sampleRequest.customer.customer_code_label')">
                            <template #body="slotProp">
                                <router-link :to="{
                                    name: 'agencyCategory-detail',
                                    params: { id: slotProp.data.id },
                                }" class="text-primary hover:underline font-semibold">
                                    <span>{{ slotProp.data.cardCode }} </span>
                                </router-link>

                                <!-- <span
                                    @click="directToDetail(slotProp.data.id)"
                                    class="cursor-pointer text-primary hover:underline hover:font-bold"
                                >
                                    <i class="pi pi-arrow-right mr-2"></i>
                                    <span>{{ slotProp.data.cardName }} </span>
                                </span> -->
                            </template>
                        </Column>
                        <Column field="cardName" :header="t('body.sampleRequest.customer.customer_name_label')"
                            style="min-width: 25rem" />
                        <Column field="email" :header="t('body.sampleRequest.customer.email_label')"></Column>
                        <Column :header="t('body.sampleRequest.customer.phone_label')" field="phone"
                            :showFilterMatchModes="false">
                            <template #filter="{ filterModel }">
                                <InputText :placeholder="t('body.sampleRequest.customer.phone_label')"
                                    v-model="filterModel.value"></InputText>
                            </template>
                        </Column>
                        <Column :header="t('body.sampleRequest.customer.tax_code_label')" field="licTradNum"
                            :showFilterMatchModes="false">
                            <template #filter="{ filterModel }">
                                <InputText :placeholder="t('body.sampleRequest.customer.tax_code_label')"
                                    v-model="filterModel.value"></InputText>
                            </template>
                        </Column>

                        <Column :header="t('body.sampleRequest.customer.owner_label')" field="saleStaffId"
                            :showFilterMatchModes="false">
                            <template #filter="{ filterModel }">
                                <!-- <InputText
                                    placeholder="Tìm kiếm người phụ trách"
                                    v-model="filterModel.value"
                                ></InputText> -->
                                <!-- <UserSelector></UserSelector> -->
                                <div class="flex flex-column gap-2">
                                    <AUserSelector v-model="filterModel.value" optionValue="id"
                                        :placeholder="t('body.OrderList.select_customer_placeholder')"
                                        :disabled="filterModel.value == 'null'"></AUserSelector>
                                    <div>
                                        <Checkbox v-model="filterModel.value" inputId="kcpt" :trueValue="'null'" binary>
                                        </Checkbox>
                                        <label for="kcpt" class="ml-2">
                                            <!-- keep original text if not available in en.json -->
                                            {{ filterModel.value == 'null' ? t('body.systemSetting.no_data_customer') :
                                                t('body.systemSetting.yes_data_customer') }}
                                        </label>
                                    </div>
                                </div>
                            </template>
                            <template #body="{ data }">
                                {{ data.saleStaff?.fullName }}
                            </template>
                        </Column>

                        <Column :header="t('body.sampleRequest.customer.status_label')" style="width: 11rem"
                            field="status" :showFilterMatchModes="false">
                            <template #filter="{ filterModel }">
                                <MultiSelect v-model="filterModel.value" :options="[
                                    { name: t('body.sampleRequest.customer.inactive_status'), code: 'D' },
                                    { name: t('body.sampleRequest.customer.active_status'), code: 'A' }
                                ]" optionLabel="name" optionValue="code" :placeholder="t('body.OrderApproval.clear')"
                                    class="p-column-filter" showClear>
                                </MultiSelect>
                            </template>
                            <template #body="slotProps">
                                <Tag :severity="getStatusLabel(slotProps.data.status)['severity']"
                                    :value="getStatusLabel(slotProps.data.status)['label']" />
                            </template>
                        </Column>
                        <Column style="min-width: 7rem; width: 8rem" v-if="false">
                            <template #body="dataProps">
                                <div class="flex justify-content-center gap-3">
                                    <Button text severity="danger" icon="pi pi-trash"
                                        @click="showConfirmDelete(dataProps.data.id)" />
                                </div>
                            </template>
                        </Column>
                        <Column :header="t('client.scopeCustomerGroup')">
                            <template #body="{ data }">
                                <Button outlined size="small" @click="handleShowApplicableGroups($event, data.groups)"
                                    v-if="getScopeGroupInfo(data).message">
                                    {{ getScopeGroupInfo(data).message }}
                                </Button>
                            </template>
                        </Column>
                    </DataTable>
                </div>
            </template>
            <template v-else>
                <div class="flex justify-center align-items-center p-error"></div>
            </template>
        </template>
        <template v-else>
            <div v-if="0" class="flex" style="
                    justify-content: center;
                    align-items: center;
                    background-color: #dbe2e8;
                ">
                <ProgressSpinner style="min-height: 50rem" animationDuration=".5s"
                    aria-label="Custom ProgressSpinner" />
            </div>
        </template>
        <!-- End Data -->
    </template>
    <template v-else>
        <div class="mx-3">
            <div class="fixed-header flex">
                <div class="flex align-items-center justify-between">
                    <h5 style="color: var(--primary-color)" class="font-semibold m-0">
                        {{ t('body.sampleRequest.customer.customer_details_title') }}
                    </h5>
                </div>
            </div>
            <div class="grid container">
                <div class="fixed-panel">
                    <div class="card pb-3">
                        <div class="flex justify-content-center">
                            <div class="image-company">
                                <Image :src="avatar.Link" preview style="
                                        height: 100px;
                                        width: 100px;
                                        overflow: hidden;
                                        border-radius: 50%;
                                        background-color: #f2f2f2;
                                    " class="justify-self-center" :pt="{}" />
                            </div>
                        </div>
                        <h5 style="color: var(--primary-color)" class="font-semibold">
                            {{ t('body.sampleRequest.customer.general_info_title') }}
                        </h5>
                        <hr />
                        <div class="col-12 py-2 pl-0 flex flex-column">
                            <a class="p-button-label p-button-text mb-3 hover:text-green-400 text-color block"
                                @click="scrollToSection1" style="cursor: pointer">
                                {{ t('body.sampleRequest.customer.general_info_title') }}
                            </a>
                            <a class="p-button-label p-button-text mb-3 hover:text-green-400 text-color block"
                                @click="scrollToSection2" style="cursor: pointer">
                                {{ t('body.sampleRequest.customer.contact_info_title') }}
                            </a>
                            <a class="p-button-label p-button-text mb-3 hover:text-green-400 text-color block"
                                @click="scrollToSection3" style="cursor: pointer">
                                {{ t('body.sampleRequest.customer.credit_limit_title') }}
                            </a>
                            <a class="p-button-label p-button-text mb-3 hover:text-green-400 text-color block"
                                @click="scrollToSection4" style="cursor: pointer">
                                {{ t('body.sampleRequest.customer.product_group_title') }}
                            </a>
                            <a class="p-button-label p-button-text mb-3 hover:text-green-400 text-color block"
                                @click="scrollToSection5" style="cursor: pointer">
                                {{ t('body.sampleRequest.customer.special_product_list_title') }}
                            </a>
                            <a class="p-button-label p-button-text mb-3 hover:text-green-400 text-color block"
                                @click="scrollToSection6" style="cursor: pointer">
                                {{ t('body.sampleRequest.customer.document_title') }}
                            </a>
                        </div>
                    </div>
                </div>

                <div class="scroll-area">
                    <div class="close-button flex justify-content-end">
                        <Button outlined severity="contrast" icon="pi pi-times" :label="t('body.OderList.close')"
                            @click="closeDetailWindown" />
                    </div>
                    <!-- Thông tin chung -->
                    <div id="section1" class="card pt-3">
                        <div class="grid">
                            <div class="col-12">
                                <div class="flex align-items-center justify-content-between">
                                    <h5 style="color: var(--primary-color)" class="py-2 m-0 font-bold">
                                        {{ t('body.sampleRequest.customer.general_info_title') }}
                                    </h5>
                                    <Button icon="pi pi-pencil" severity="success" outlined
                                        @click="changeDialogDetail('info')" aria-label="Search" />
                                </div>
                            </div>
                            <span class="col-6">{{ t('body.sampleRequest.customer.customer_name_label') }}:</span>
                            <span class="col-6">{{ dataView.cardName }}</span>
                            <span class="col-6">{{ t('body.sampleRequest.customer.contact_name_label') }}</span>
                            <span class="col-6">{{ dataView.frgnName || "-" }}</span>
                            <span class="col-6">{{ t('body.sampleRequest.customer.tax_code_label') }}</span>
                            <span class="col-6">{{ dataView.licTradNum }}</span>
                            <span class="col-6">{{ t('body.sampleRequest.customer.email_label') }}</span>
                            <span class="col-6">{{ dataView.email }}</span>
                            <span class="col-6">{{ t('body.sampleRequest.customer.phone_label') }}</span>
                            <span class="col-6">{{ dataView.phone || "-" }}</span>
                            <span class="col-6">{{ t('body.sampleRequest.customer.address_label') }}</span>
                            <span class="col-6">{{ dataView.FullAddress || "-" }}</span>
                        </div>
                    </div>
                    <!-- Thông tin liên hệ -->
                    <div id="section2" class="card py-3">
                        <div class="grid">
                            <div class="col-12">
                                <div class="flex align-items-center justify-content-between">
                                    <h5 style="color: var(--primary-color)" class="py-2 m-0 font-bold">
                                        {{ t('body.sampleRequest.customer.contact_info_title') }}
                                    </h5>
                                    <Button icon="pi pi-pencil" severity="success" outlined aria-label="Search" />
                                </div>
                            </div>
                            {{ dataView.CRD1 }}
                            <DataTable class="w-full" :value="dataView.CRD1" showGridlines>
                                <Column header="#" style="width: 3rem">
                                    <template #body="slotProps">
                                        {{ slotProps.index + 1 }}
                                    </template>
                                </Column>
                                <Column :header="t('body.sampleRequest.customer.contact_person_name_column')"
                                    field="person" />
                                <Column :header="t('body.sampleRequest.customer.position') || 'Chức vụ'"
                                    field="LastName" />
                                <Column :header="t('body.sampleRequest.customer.phone_label')" field="phone" />
                                <Column :header="t('body.sampleRequest.customer.phone_label')" field="address" />
                                <Column :header="t('body.sampleRequest.customer.email_label')" field="email" />
                                <template #empty>
                                    <div class="flex justify-content-center align-items-center"></div>
                                </template>
                            </DataTable>
                            <Button outlined class="mt-4" severity="success" icon="pi pi-plus-circle"
                                :label="t('body.sampleRequest.customer.add_new_button')" @click="closeDetailWindown" />
                        </div>
                    </div>

                    <!-- Điều khoản thanh toán -->

                    <div id="section3" class="card py-3">
                        <div class="grid">
                            <div class="col-12">
                                <div class="flex align-items-center justify-content-between">
                                    <h5 style="color: var(--primary-color)" class="py-2 m-0 font-bold">
                                        {{ t('body.sampleRequest.customer.credit_limit_title') }}
                                    </h5>
                                </div>
                            </div>

                            <DataTable class="w-full" showGridlines>
                                <template #empty>
                                    <div class="flex justify-content-center align-items-center">
                                        {{ t('body.systemSetting.no_data_to_display') }}
                                    </div>
                                </template>
                                <Column header="#"></Column>
                                <Column :header="t('body.sampleRequest.customer.debt_form_column')" field=""></Column>
                            </DataTable>

                            <Button outlined class="mt-4" severity="success" icon="pi pi-plus-circle"
                                :label="t('body.sampleRequest.customer.add_new_button')" @click="closeDetailWindown" />
                        </div>
                    </div>

                    <!-- Danh muc san pham phan phoi -->
                    <div id="section4" class="card py-3">
                        <div class="grid">
                            <div class="col-12">
                                <div class="flex align-items-center justify-content-between">
                                    <h5 style="color: var(--primary-color)" class="py-2 m-0 font-bold">
                                        {{ t('body.sampleRequest.customer.product_group_title') }}
                                    </h5>
                                </div>
                            </div>

                            <div class="col-3">
                                <Dropdown :placeholder="t('body.productManagement.brand')" class="w-full"></Dropdown>
                            </div>
                            <div class="col-3">
                                <Dropdown :placeholder="t('body.productManagement.category')" class="w-full"></Dropdown>
                            </div>
                            <div class="col-3">
                                <Dropdown :placeholder="t('body.productManagement.product_type')" class="w-full">
                                </Dropdown>
                            </div>
                            <div class="col-3">
                                <Dropdown :placeholder="t('body.productManagement.packaging')" class="w-full">
                                </Dropdown>
                            </div>
                        </div>
                    </div>
                    <!-- Danh muc san pham dac thu  -->
                    <div id="section5" class="card py-3">
                        <div class="grid">
                            <div class="col-12">
                                <div class="flex align-items-center justify-content-between">
                                    <h5 class="py-2 m-0 font-bold" style="color: var(--primary-color)">
                                        {{ t('body.sampleRequest.customer.special_product_list_title') }}
                                    </h5>
                                </div>
                            </div>

                            <div class="col-8">
                                <span class="mb-3 block">{{ t('body.sampleRequest.customer.choose_product_button')
                                }}</span>
                                <InputGroup>
                                    <InputText :placeholder="t('body.sampleRequest.customer.search_placeholder')" />
                                    <Button icon="pi pi-search" severity="success" />
                                </InputGroup>
                            </div>

                            <div class="col-12">
                                <DataTable class="w-full" showGridlines>
                                    <template #empty>
                                        <div class="flex justify-content-center align-items-center">
                                            {{ t('body.systemSetting.no_data_to_display') }}
                                        </div>
                                    </template>
                                    <Column header="#"></Column>
                                    <Column :header="t('body.productManagement.product_name_column')" field=""></Column>
                                    <Column>
                                        <template #body="">
                                            <i icon="pi pi-downloads"></i>
                                        </template>
                                    </Column>
                                </DataTable>
                            </div>
                        </div>
                    </div>
                    <!-- Tài liệu -->
                    <div id="section6" class="card py-3">
                        <div class="grid">
                            <div class="col-12">
                                <div class="flex align-items-center justify-content-between">
                                    <h5 style="color: var(--primary-color)" class="py-2 m-0 font-bold">
                                        {{ t('body.sampleRequest.customer.document_title') }}
                                    </h5>
                                </div>
                            </div>

                            <DataTable class="w-full" showGridlines>
                                <template #empty>
                                    <div class="flex justify-center align-items-center">
                                        {{ t('body.systemSetting.no_data_to_display') }}
                                    </div>
                                </template>
                                <Column header="#"></Column>
                                <Column :header="t('body.sampleRequest.customer.document_name_column')" field="">
                                </Column>
                                <Column :header="t('body.sampleRequest.customer.note_column')" field=""></Column>
                                <Column>
                                    <template #body="">
                                        <i icon="pi pi-downloads"></i>
                                    </template>
                                </Column>
                            </DataTable>
                            <Button outlined class="mt-4" severity="success" icon="pi pi-plus-circle"
                                :label="t('body.sampleRequest.customer.add_new_button')" @click="closeDetailWindown" />
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <Dialog v-model:visible="dialogDetail.dialogInfor" modal
            :header="t('body.sampleRequest.customer.general_info_title')" :style="{ width: '60%' }">
            <div class="grid">
                <div class="col-2 field">
                    <label>{{ t('body.sampleRequest.customer.tax_code_label') }} </label>
                    <InputText :placeholder="dataView.licTradNum" v-model="dataView.licTradNum" class="w-full" disabled>
                    </InputText>
                </div>

                <div class="col-5 field">
                    <label>{{ t('body.sampleRequest.customer.customer_name_label') }} </label>
                    <InputText :placeholder="dataView.cardName" v-model="dataView.cardName" class="w-full"></InputText>
                </div>
                <div class="col-5 field">
                    <label>{{ t('body.sampleRequest.customer.contact_name_label') }}</label>
                    <InputText :placeholder="dataView.cardFName" v-model="dataView.cardName" class="w-full"></InputText>
                </div>
                <div class="col-4 field">
                    <label>{{ t('body.sampleRequest.customer.representative_label') }}</label>
                    <InputText class="w-full" :placeholder="dataView.person" v-model="dataView.person"></InputText>
                </div>

                <div class="col-2 field">
                    <label>{{ t('body.sampleRequest.customer.phone_label') }} </label>
                    <InputText class="w-full" :placeholder="dataView.phone" v-model="dataView.phone"></InputText>
                </div>

                <div class="col-6 field">
                    <label>{{ t('body.sampleRequest.customer.email_label') }}</label>
                    <InputText class="w-full" :placeholder="dataView.email" v-model="dataView.email" disabled>
                    </InputText>
                </div>
                <div class="col-4 field">
                    <label>{{ t('body.sampleRequest.customer.area_column') }}</label>
                    <AutoComplete :pt:input:class="'w-full'" v-model="selectedLocation" class="w-full"
                        :suggestions="location" @complete="updateChangeLocation" optionLabel="name" />
                </div>
                <div class="col-4 field">
                    <label>{{ t('body.sampleRequest.customer.province_column') }}</label>
                    <AutoComplete :pt:input:class="'w-full'" class="w-full" v-model="selectedProvince"
                        :suggestions="province" @complete="updateChangeProvince" optionLabel="name" />
                </div>
                <div class="col-4 field">
                    <label>{{ t('body.sampleRequest.customer.address_label') }}</label>
                    <InputText class="w-full" :placeholder="dataView.address" v-model="dataView.address"></InputText>
                </div>

                <div class="col-12" style="text-align: right">
                    <Button outlined severity="success" icon="pi pi-plus" @click="updateSupplier(dataView.id)"
                        :label="t('body.systemSetting.save_button')" />
                </div>
            </div>
        </Dialog>

        <!-- Begin dialog detail -->
    </template>
    <!-- Begin Dialog -->
    <Dialog v-model:visible="visiableDialog" modal :header="dialogMode.header" :style="{ width: '40rem' }" :pt="{
        header: { style: 'border-bottom: 1px solid #e5e7eb;' },
    }">
        <div class="flex flex-column mt-2">
            <div class="flex gap-3">
                <div class="flex flex-column field">
                    <label class="font-semibold">{{ t('body.sampleRequest.customer.tax_code_label') }}
                        <sup class="text-red-500">*</sup>
                    </label>
                    <InputMask v-model="payloadCreate.licTradNum" :class="{
                        'p-invalid': errorMessages.licTradNum,
                    }" autofocus class="w-13rem" mask="9999999999?-999" />
                    <small v-if="errorMessages.licTradNum" class="p-error">{{
                        errorMessages.licTradNum
                    }}</small>
                </div>
                <div class="flex flex-column flex-grow-1 field">
                    <label class="font-semibold">{{ t('body.sampleRequest.customer.customer_code_label') }}
                        <sup class="text-red-500">*</sup>
                    </label>
                    <InputText v-model="payloadCreate.cardCode" :class="{
                        'p-invalid': errorMessages.cardCode,
                    }" class="w-full" />
                    <small v-if="errorMessages.cardCode" class="p-error">{{
                        errorMessages.cardCode
                    }}</small>
                </div>
            </div>
            <div class="flex flex-column field">
                <label class="font-semibold" for="">{{ t('body.sampleRequest.customer.customer_name_label') }}<sup
                        class="text-red-500">*</sup></label>
                <InputText :class="{ 'p-invalid': errorMessages.cardCode }" v-model="payloadCreate.cardName" />
                <small v-if="errorMessages.cardName" class="p-error">{{
                    errorMessages.cardName
                }}</small>
            </div>
            <div class="flex flex-column field">
                <label class="font-semibold">{{ t('body.sampleRequest.customer.phone_label') }} <sup
                        class="text-red-500">*</sup></label>
                <InputText :invalid="errorMessages.phone" v-model="payloadCreate.phone"></InputText>
                <small v-if="errorMessages.phone" class="p-error">{{
                    errorMessages.phone
                }}</small>
            </div>
            <div class="flex flex-column field mb-0">
                <label class="font-semibold">{{ t('body.sampleRequest.customer.email_label') }}<sup
                        class="text-red-500 ml-1">*</sup>
                </label>
                <InputText :class="{ 'p-invalid': errorMessages.email }" v-model="payloadCreate.email" />
                <small v-if="errorMessages.email" class="p-error">{{
                    errorMessages.email
                }}</small>
            </div>
        </div>
        <template #footer>
            <Button :label="t('body.OderList.close')" icon="pi pi-times" @click="closeAddDialog" severity="secondary" />
            <Button :loading="loadingButton" :label="t('body.systemSetting.save_button')" icon="pi pi-save "
                @click="submit" autofocus />
        </template>
    </Dialog>

    <Dialog v-model:visible="visibleDialog1" modal
        :header="t('body.report.update_status_button') || 'Cập nhật trạng thái Khách hàng'" :style="{ width: '45rem' }"
        :pt="{
            header: { style: 'border-bottom: 1px solid #e5e7eb;' },
        }">
        <div class="m-0 mt-2 w-full">
            <div class="grid">
                <div class="col-12">
                    <div class="flex flex-column gap-2">
                        <label class="font-bold">{{ t('body.sampleRequest.customer.customer_name_label') }}</label>
                        <InputText :value="supplierName" />
                        <small v-if="errorMessages.cardCode" class="p-error">{{
                            errorMessages.cardCode
                        }}</small>
                    </div>
                </div>

                <div class="col-12">
                    <div class="flex flex-column gap-2">
                        <label class="font-bold">{{ t('body.sampleRequest.customer.email_label') }}</label>
                        <InputText :class="{ 'p-invalid': errorMessages.email }" :value="supplierEmail" variant="filled"
                            disabled />
                    </div>
                </div>
                <div class="col-12 flex align-items-center gap-3">
                    <label class="font-bold">{{ t('body.sampleRequest.customer.status_label') }}</label>
                    <InputSwitch v-model="supplierStatus" />
                </div>
            </div>
        </div>
        <template #footer>
            <Button :label="t('body.OderList.close')" icon="pi pi-times" @click="visibleDialog1 = false"
                severity="secondary" />
            <Button :loading="loadingUpdateStatus" :label="t('body.systemSetting.save_button')" icon="pi pi-save"
                @click="submit" autofocus />
        </template>
    </Dialog>
    <!-- End Dialog -->

    <!-- Thông tin chi tiết dialog -->

    <!-- Toast -->
    <ConfirmDialog></ConfirmDialog>
    <!-- <Toast /> -->
    <Loading v-if="loading" />
    <OverlayPanel ref="overlayPanelRef" class="w-20rem surface-100">
        <DataTable :value="applicableGroupsData" showGridlines scrollable scrollHeight="200px" size="small">
            <template #header>
                <div class="text-center font-bold">
                    {{ t('client.scopeCustomerGroup') }}
                </div>
            </template>
            <Column>
                <template #body="{ data }">
                    <span>{{ data.groupName }} </span>
                </template>
            </Column>
        </DataTable>
    </OverlayPanel>
</template>

<script setup>
import { onMounted, reactive, ref, onBeforeMount, onUnmounted } from "vue";
import API from "@/api/api-main";
import { useToast } from "primevue/usetoast";
import { useConfirm } from "primevue/useconfirm";
import { useRouter, useRoute } from "vue-router";
import { FilterStore } from "@/Pinia/Filter/FilterStoreAgencyCategory.js";
import { inject } from "vue";
import { debounce } from "lodash";
import { useI18n } from 'vue-i18n';
const { t } = useI18n();

const overlayPanelRef = ref();
const applicableGroupsData = ref([]);

const getScopeGroupInfo = (data) => {
    const count = data.groups.length;
    return {
        isSingle: count === 1,
        count: count,
        message: count === 0 ? '' : `${count} nhóm`,
        severity: 'info'
    };
};

const handleShowApplicableGroups = (event, groups) => {
    if (typeof groups === 'string') {
        applicableGroupsData.value = groups.split(',').map(s => s.trim()).filter(s => s !== "");
    } else if (Array.isArray(groups)) {
        applicableGroupsData.value = groups;
    } else {
        applicableGroupsData.value = [];
    }
    overlayPanelRef.value.toggle(event);
};

const toast = useToast();

const loadingSync = ref(false);
const onClickSync = () => {
    loadingSync.value = true;
    API.add("Customer/syncbp")
        .then((res) => {
            toast.add({
                severity: "success",
                summary: t('body.systemSetting.success_label'),
                detail: t('body.sampleRequest.customer.sync_button'),
                life: 3000,
            });
        })
        .catch((error) => {
            toast.add({
                severity: "error",
                summary: t('body.report.error_occurred_message'),
                detail: t('body.report.error_occurred_message'),
                life: 3000,
            });
        })
        .finally(() => {
            loadingSync.value = false;
        });
};

// Biến function
const conditionHandler = inject("conditionHandler");
const filterStore = FilterStore();
const confirm = useConfirm();
const router = useRouter();
const directToDetail = (id) => {
    router.push({ name: "agencyCategory-detail", params: { id: id } });
};

const pagable = ref({
    rows: 10,
    page: 0,
    total: 0,
});
const selectedLocation = ref(null);
const selectedProvince = ref(null);
const loading = ref(false);
const searchKey = ref("");

// -------------------- Data ------------------------
const suppliersData = ref([]);
const loadingSuppliersData = ref(true);
const errorsuppliersData = ref(true);

const dataView = ref({});
// -------------------- Models ----------------------
const avatar = reactive({
    File: null,
    LocalFileLink: null,
    Link: null,
    FileName: null,
});
const supplierName = ref(null);
const supplierEmail = ref(null);
const supplierStatus = ref("A");

const payloadCreate = reactive({
    licTradNum: null,
    cardCode: null,
    cardName: null,
    email: null,
    phone: null,
});
const errorMessages = reactive({
    licTradNum: null,
    cardCode: null,
    cardName: null,
    email: null,
    phone: null,
});

// -------------------- Variables -------------------
// Xem thêm --------------------

const showDetailSupplier = ref(false);
const showDetailContact = ref(false);

const loadingButton = ref(false);

const infoWindows = ref(false);
const visiableDialog = ref(false);
const location = ref([]);
const province = ref([]);

const dialogMode = reactive({
    mode: "",
    header: "",
    primaryButton: "",
    secondaryButton: "",
});

const dialogDetail = reactive({
    dialogInfor: false,
    dialogContact: false,
    dialogPayment: false,
    dialogDocument: false,
});

// -------------------- Methods ---------------------

const updateChangeLocation = async (event) => {
    const res = await API.get(
        "Area/search/" + encodeURIComponent(selectedLocation.value)
    );
    location.value = res.data;
};

const updateChangeProvince = async (event) => {
    const res = await API.get(
        "Area/search/" +
        selectedLocation.value.id +
        "/" +
        encodeURIComponent(selectedProvince.value)
    );
    province.value = res.data;
};

const changeDialogDetail = (action) => {
    if (action == "info") {
        dialogDetail.dialogInfor = !dialogDetail.dialogInfor;
    }
    if (action == "contact") dialogDetail.dialogContact = !dialogDetail.dialogContact;
    if (action == "payment") dialogDetail.dialogPayment = !dialogDetail.dialogPayment;
    if (action == "document") dialogDetail.dialogDocument = !dialogDetail.dialogDocument;
};
const scrollToSection1 = () => {
    const el = document.getElementById("section1");
    window.scrollTo(0, el.offsetTop - 126);
};

const scrollToSection2 = () => {
    const el = document.getElementById("section2");
    window.scrollTo(0, el.offsetTop - 126);
};

const scrollToSection3 = () => {
    const el = document.getElementById("section3");
    window.scrollTo(0, el.offsetTop - 126);
};

const scrollToSection4 = () => {
    const el = document.getElementById("section4");
    window.scrollTo(0, el.offsetTop - 126);
};
const scrollToSection5 = () => {
    const el = document.getElementById("section5");
    window.scrollTo(0, el.offsetTop - 126);
};

const scrollToSection6 = () => {
    const el = document.getElementById("section6");
    window.scrollTo(0, el.offsetTop - 126);
};

const submit = () => {
    if (!checkValidate()) return;
    switch (dialogMode.mode) {
        case "A":
            addSupplier();
            break;
        case "U":
            updateSupplier();
            break;
        default:
            break;
    }
};

const onPageChange = (event) => {
    pagable.value.rows = event.rows;
    pagable.value.page = event.page;
    fetchSuppliersData();
};

const emailRegExp = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
const phoneRegex = /^(0[0-9]{9}|[1-9][0-9]{9,14})$/;
const checkValidate = () => {
    resetErrorMessage();
    if (!payloadCreate.licTradNum || !payloadCreate.licTradNum.trim()) {
        errorMessages.licTradNum = "Mã số thuế không được để trống";
    }

    if (!payloadCreate.cardName) {
        errorMessages.cardName = "Tên Khách hàng không được để trống";
    }
    if (!payloadCreate.cardCode) {
        errorMessages.cardCode = "Mã Khách hàng không được để trống";
    }
    if (!payloadCreate.email || !payloadCreate.email.trim()) {
        errorMessages.email = "Email không được để trống";
    } else {
        if (!payloadCreate.email.trim().match(emailRegExp)) {
            errorMessages.email = "Định dạng email không hợp lệ";
        }
        if (
            payloadCreate.email &&
            payloadCreate.email.trim() &&
            payloadCreate.email.trim().length > 100
        ) {
            errorMessages.email = "Độ dài email không được vượt quá 100 ký tự";
        }
    }
    if (!payloadCreate.phone) {
        errorMessages.phone = "Số điện thoại không được để trống";
    } else if (!phoneRegex.test(payloadCreate.phone)) {
        errorMessages.phone = "Số điện thoại không đúng định dạng";
    }

    for (let key of Object.keys(errorMessages)) {
        if (errorMessages[key]) {
            return false;
        }
    }
    return true;
};

const addSupplier = () => {
    loadingButton.value = true;
    const dataSupplier = {
        licTradNum: payloadCreate.licTradNum,
        email: payloadCreate.email,
        cardName: payloadCreate.cardName,
        cardCode: payloadCreate.cardCode,
        phone: payloadCreate.phone,
        isAllArea: true,
        isAllBrand: true,
        isAllItemType: true,
        isAllIndustry: true,
        isAllPacking: true,
        IsAllBPArea: true,
        IsAllBPSize: true,
    };

    let formData = new FormData();
    formData.append("item", JSON.stringify(dataSupplier));
    API.add("customer/add", formData)
        .then((res) => {
            loadingButton.value = false;
            closeAddDialog();
            toast.add({
                severity: "success",
                summary: t('body.systemSetting.success_label'),
                detail: t('body.sampleRequest.customer.add_new_button') || "Thêm mới Khách hàng thành công",
                life: 3000,
            });
            fetchSuppliersData();
        })
        .catch((error) => {
            loadingButton.value = false;
            toast.add({
                severity: "error",
                summary: t('body.report.error_occurred_message'),
                detail: t('body.report.error_occurred_message'),
                life: 3000,
            });
        });
};

const closeDetailWindown = () => {
    infoWindows.value = false;
    dataView.value = {};
    showDetailSupplier.value = false;
    showDetailContact.value = false;
};

const resetErrorMessage = () => {
    Object.keys(errorMessages).forEach((key) => {
        errorMessages[key] = null;
    });
};

const openAddDialog = () => {
    resetErrorMessage();
    Object.keys(payloadCreate).forEach((key) => {
        payloadCreate[key] = null;
    });
    dialogMode.mode = "A";
    loadingButton.value = false;
    dialogMode.header = "Thêm mới Khách hàng";
    visiableDialog.value = true;
};

const visibleDialog1 = ref(false);

const modeUpdate = ref(2);
const loadingUpdateStatus = ref(false);
const updateSupplier = async (id) => {
    loadingUpdateStatus.value = true;
    if (modeUpdate.value === 1) {
        alert("Tính năng đang phát triển, :P");
    } else if (modeUpdate.value === 2) {
        const formData = new FormData();
        formData.append("item", JSON.stringify(dataView.value));
        try {
            const res = await API.update("Customer/" + id, formData);
            if (res.status === 200) {
                visibleDialog1.value = false;
                showToast("success", "Thành công", "Cập nhật trạng thái thành công");
                fetchSuppliersData();
            }
        } catch (e) {

        }
    } else {
        showToast("error", "Exception", "Unhandler exception!");
    }
    loadingUpdateStatus.value = false;
};

const showConfirmDelete = (id2d) => {
    confirm.require({
        message: t('body.OrderApproval.confirm') || "Bạn có muốn xoá khách hàng này",
        header: t('body.OrderApproval.confirm') || "Xác nhận xoá khách hàng",
        icon: "pi pi-info-circle",
        rejectLabel: t('body.OderList.close') || "Huỷ",
        acceptLabel: t('body.OderList.delete') || "Xoá",
        rejectClass: "p-button-secondary",
        acceptClass: "p-button-danger",
        accept: () => {
            API.delete("Customer/" + id2d)
                .then((res) => {
                    if (res.status >= 200) {
                        showToast("success", t('body.systemSetting.success_label'), t('body.sampleRequest.customer.delete_success') || "Đã xoá thành công");
                        fetchSuppliersData();
                    }
                })
                .catch((error) => {
                    showToast("error", t('body.report.error_occurred_message'), error.message);
                });
        },
        reject: () => { },
    });
};

const closeAddDialog = () => {
    visiableDialog.value = false;
};

// Guard: ngăn callback async cập nhật state / gọi router.replace sau khi component unmount
let _isMounted = false;
onMounted(() => {
    _isMounted = true;
    if (hasListQuery()) {
        pagable.value.page = route.query.skip ? Number(route.query.skip) : 0;
        pagable.value.rows = route.query.limit ? Number(route.query.limit) : 10;
    } else {
        filterStore.resetFilters();
        pagable.value.page = 0;
        pagable.value.rows = 10;
    }
    fetchSuppliersData();
});
onUnmounted(() => {
    _isMounted = false;
});

const hasListQuery = () => {
    return route.query.limit != undefined || route.query.skip != undefined || route.query.filter != undefined;
};

const fetchSuppliersData = () => {
    const filters = conditionHandler.getQuery(filterStore.filters);
    const updateFilters = filters.replace("saleStaffId", "saleId");
    const queryParams = `?skip=${pagable.value.page}&limit=${pagable.value.rows}&OrderBy=(desc id)${updateFilters}`;
    loading.value = true;
    API.get(`Customer${queryParams}`)
        .then((res) => {
            // Guard kép: kiểm tra _isMounted VÀ route.name để tuyệt đối không
            // gọi router.replace khi đã navigate sang trang khác (detail, create, v.v.)
            if (!_isMounted || route.name !== 'agencyCategory') return;
            suppliersData.value = res.data || [];
            pagable.value.total = res.data.total;
            loadingSuppliersData.value = false;
            errorsuppliersData.value = false;
            loading.value = false;
            // Dùng object format thay vì bare query string (Vue Router 4 yêu cầu)
            router.replace({ query: { skip: pagable.value.page, limit: pagable.value.rows } });
        })
        .catch((error) => {
            if (!_isMounted) return;
            loadingSuppliersData.value = false;
            errorsuppliersData.value = true;
            const summary = t('body.report.error_occurred_message') || "Lỗi";
            showToast("error", summary, error.response?.data?.message ?? error.message);
        })
        .finally(() => {
            if (!_isMounted) return;
            loading.value = false;
        });
};

const showToast = (severity, summary, msg) => {
    toast.add({
        severity: severity,
        summary: summary,
        group: "br",
        detail: msg,
        life: 3000,
    });
};
const onFilter = () => {
    pagable.value.page = 0;
    fetchSuppliersData();
};

const clearFilter = () => {
    filterStore.resetFilters();
    pagable.value.page = 0;
    fetchSuppliersData();
};
const labels = {
    D: {
        severity: "secondary",
        label: t('body.sampleRequest.customer.inactive_status') || "Không hoạt động",
    },
    A: {
        severity: "success",
        label: t('body.sampleRequest.customer.active_status') || "Hoạt động",
    },
};

const getStatusLabel = (str) => {
    return labels[str] || { severity: "secondary", label: t('body.systemSetting.no_data_to_display') || "Không hoạt động" };
};

const debounceF = debounce(onFilter, 1000);

const route = useRoute();
onBeforeMount(() => {


});
</script>

<style scoped>
.link-text:hover {
    color: rgb(5, 65, 108);
    text-decoration: underline;
}

.link-text {
    font-weight: bold;
    color: var(--primary-color);
    cursor: pointer;
}

.dash {
    margin: auto 0 auto 0;
    align-items: center;
    width: 7px;
    display: flex;
    background: #6a6a6a;
    height: 1px;
}

.image-company {
    display: flex;
    position: relative;
    justify-content: center;
}

.fixed-header {
    margin: 1rem 0 0 1rem;
    top: 5.5rem;
    position: fixed;
    z-index: 1000;
}

.fixed-panel {
    position: fixed;
    margin-top: 4.5rem;
    width: 20rem;
    z-index: 1;
}

.scroll-area {
    /* margin-top: 4.5rem; */
    margin-left: 21rem;
    width: 100% !important;
}

.close-button {
    margin: 0 0 2rem 0;
    width: 100%;
    padding-right: 1rem;
}

.btn-more {
    color: var(--primary-color);
    padding: 0.7rem;
    cursor: pointer;
}

.btn-more:hover {
    color: var(--blue-800);
    text-decoration: underline;
}

.mt-14px {
    margin-top: 14px;
}

/*
.field {
  margin-bottom: 0;
  line-height: 23px;
}

.field:hover button {
  visibility: visible;
  transition: visibility 0s;
  transition-delay: 0s;
  opacity: 0s linear;
} */
</style>
<style>
html {
    scroll-behavior: smooth;
}
</style>
