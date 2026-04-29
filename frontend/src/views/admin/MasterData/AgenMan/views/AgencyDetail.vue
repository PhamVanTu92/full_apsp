<template>
    <Loading v-if="loading.scrn" />
    <div class="relative">
        <div class="flex justify-content-between mb-5" style="height: 33px">
            <h4 class="font-bold my-auto">
                {{ t('body.sampleRequest.customer.customer_details_title') }}
            </h4>
            <div class="flex _fixed justify-content-between">
                <ButtonGoBack />
            </div>
        </div>
        <template v-if="errorMessage.system">
            <div class="flex justify-content-center align-items-center" style="height: 50vh">
                <div class="card w-30rem">
                    <div class="flex flex-column align-items-center justify-content-center">
                        <i class="text-6xl fa-solid fa-circle-xmark mb-5 text-red-500"></i>
                        <div class="text-2xl font-bold text-center text-red-500 mb-2">
                            {{ errorMessage.system }}
                        </div>
                        <div v-if="errorMessage.detail" class="text-center font-italic card p-3 text-500">
                            {{ errorMessage.detail }}
                        </div>
                    </div>
                </div>
            </div>
        </template>
        <template v-else>
            <div class="grid">
                <div class="col-12">
                    <div id="section1" class="card">
                        <div class="flex justify-content-between">
                            <div class="py-2 text-green-700 text-xl font-semibold">
                                {{ t('body.sampleRequest.customer.general_info_title') }}
                            </div>
                            <Button :disabled="editMode.info == 'EDIT'" @click="onClickEditGeneralInfo" icon="pi pi-pencil" :label="t('body.sampleRequest.customer.edit_button')" text />
                        </div>
                        <hr class="m-0" />
                        <div class="mb-5">
                            <div class="grid mt-0">
                                <div class="col-6">
                                    <div class="grid align-items-center">
                                        <label class="col-4 font-semibold">{{ t('body.sampleRequest.customer.customer_code_label') }}</label>
                                        <span v-if="editMode.info == 'EDIT'" class="col-8">
                                            <InputText disabled class="w-full" v-model="generalInfoModel.cardCode"> </InputText>
                                        </span>
                                        <span v-else class="col-8">
                                            {{ modelStates.cardCode }}
                                        </span>
                                    </div>
                                    <div class="grid align-items-center">
                                        <label class="col-4 font-semibold">{{ t('body.sampleRequest.customer.customer_name_label') }}</label>
                                        <span v-if="editMode.info == 'EDIT'" class="col-8">
                                            <InputText disabled class="w-full" v-model="generalInfoModel.cardName"> </InputText>
                                        </span>
                                        <span v-else class="col-8">
                                            {{ modelStates.cardName }}
                                        </span>
                                    </div>
                                    <div class="grid align-items-center">
                                        <label class="col-4 font-semibold">{{ t('body.sampleRequest.customer.tax_code_label') }}</label>
                                        <span v-if="editMode.info == 'EDIT'" class="col-8">
                                            <InputMask disabled mask="9999999999?-999" class="w-full" v-model="generalInfoModel.licTradNum"> </InputMask>
                                        </span>
                                        <span v-else class="col-8">
                                            {{ modelStates.licTradNum }}
                                        </span>
                                    </div>
                                    <div class="grid align-items-center">
                                        <label class="col-4 font-semibold">{{ t('body.sampleRequest.customer.email_label') }}</label>
                                        <span v-if="editMode.info == 'EDIT'" class="col-8">
                                            <InputText disabled class="w-full" v-model="generalInfoModel.email"> </InputText>
                                        </span>
                                        <span v-else class="col-8">
                                            {{ modelStates.email }}
                                        </span>
                                    </div>
                                    <div class="grid align-items-center">
                                        <label class="col-4 font-semibold">{{ t('body.sampleRequest.customer.phone_label') }}</label>
                                        <span v-if="editMode.info == 'EDIT'" class="col-8">
                                            <InputText disabled class="w-full" v-model="generalInfoModel.phone"> </InputText>
                                        </span>
                                        <span v-else class="col-8">
                                            {{ modelStates.phone }}
                                        </span>
                                    </div>
                                    <div class="grid align-items-center">
                                        <label class="col-4 font-semibold">{{ t('body.sampleRequest.customer.status_label') }}</label>
                                        <span class="col-8">
                                            <div class="flex gap-3">
                                                {{ modelStates.status == 'A' ? t('body.sampleRequest.customer.active_status') : t('body.sampleRequest.customer.inactive_status') }}
                                                <i v-if="modelStates.status == 'A'" class="pi pi-check-circle text-green-500"></i>
                                                <i v-else class="pi pi-times-circle text-red-500"></i>
                                            </div>
                                        </span>
                                    </div>
                                    <div class="grid align-items-center">
                                        <label class="col-4 font-semibold">{{ t('body.sampleRequest.customer.person_in_charge_label') }}</label>
                                        <span v-if="editMode.info != 'EDIT'" class="col-4">
                                            {{ modelStates.saleStaff?.fullName }}
                                        </span>
                                        <span v-else class="col-8">
                                            <Dropdown v-model="generalInfoModel.saleId" :placeholder="t('body.sampleRequest.customer.owner_label')" class="w-full" :options="users" optionLabel="fullName" optionValue="id" filter></Dropdown>
                                        </span>
                                    </div>
                                </div>
                                <div class="col-6">
                                    <div class="grid align-items-center">
                                        <label class="col-4 font-semibold">{{ t('body.sampleRequest.customer.contact_name_label') }}</label>
                                        <span v-if="editMode.info == 'EDIT'" class="col-8">
                                            <InputText disabled class="w-full" v-model="generalInfoModel.frgnName"> </InputText>
                                        </span>
                                        <span v-else class="col-8">
                                            {{ modelStates.frgnName }}
                                        </span>
                                    </div>
                                    <div class="grid align-items-center">
                                        <label class="col-4 font-semibold">{{ t('body.sampleRequest.customer.representative_label') }}</label>
                                        <span v-if="editMode.info == 'EDIT'" class="col-8">
                                            <InputText disabled class="w-full" v-model="generalInfoModel.person"> </InputText>
                                        </span>
                                        <span v-else class="col-8">
                                            {{ modelStates.person }}
                                        </span>
                                    </div>
                                    <div class="grid align-items-center">
                                        <label class="col-4 font-semibold">{{ t('body.sampleRequest.customer.date_of_birth_label') }}</label>
                                        <span v-if="editMode.info == 'EDIT'" class="col-8">
                                            <Calendar disabled class="w-full" dateFormat="dd/mm/yy" v-model="generalInfoModel.dateOfBirth" :maxDate="new Date()"></Calendar>
                                        </span>
                                        <span v-else class="col-8">
                                            {{ modelStates.dateOfBirth ? format(modelStates.dateOfBirth, 'dd/MM/yyyy') : null }}
                                        </span>
                                    </div>
                                    <div class="grid align-items-center">
                                        <label class="col-4 font-semibold">{{ t('body.sampleRequest.customer.address_label') }}</label>
                                        <span v-if="editMode.info == 'EDIT'" class="col-8">
                                            <SelectAddress disabled @select="onChangeAddress" :modelValue="generalInfoModel" />
                                        </span>
                                        <span v-else class="col-8">
                                            {{ modelStates._addressLabel }}
                                        </span>
                                    </div>
                                    <div class="grid">
                                        <label class="col-4 font-semibold">{{ t('body.sampleRequest.customer.note_label') }}</label>
                                        <span v-if="editMode.info == 'EDIT'" class="col-8">
                                            <Textarea class="w-full" rows="2" v-model="generalInfoModel.note"></Textarea>
                                        </span>
                                        <span v-else class="col-8">
                                            {{ modelStates.note }}
                                        </span>
                                    </div>
                                    <div class="grid">
                                        <div class="col-4"></div>
                                        <div v-if="editMode.info !== 'EDIT'" class="col-8 flex gap-3">
                                            <div>
                                                <Checkbox v-model="modelStates.isBusinessHouse" inputId="hkd" class="mr-2" binary disabled></Checkbox>
                                                <label for="hkd" class="poiter-cursor">{{ t('body.sampleRequest.customer.household_business_checkbox') }}</label>
                                            </div>
                                            <div>
                                                <Checkbox v-model="modelStates.isInterCom" inputId="Incoterm" class="mr-2" binary disabled></Checkbox>
                                                <label for="Incoterm" class="poiter-cursor">{{ t('body.sampleRequest.customer.incoterm') }}</label>
                                            </div>
                                        </div>
                                        <div v-else class="col-8 flex gap-3">
                                            <div>
                                                <Checkbox v-model="generalInfoModel.isBusinessHouse" inputId="hkd" class="mr-2" binary></Checkbox>
                                                <label for="hkd" class="poiter-cursor">{{ t('body.sampleRequest.customer.household_business_checkbox') }}</label>
                                            </div>
                                            <div>
                                                <Checkbox v-model="generalInfoModel.isInterCom" inputId="Incoterm" class="mr-2" binary></Checkbox>
                                                <label for="Incoterm" class="poiter-cursor">{{ t('body.sampleRequest.customer.incoterm') }}</label>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <hr v-if="editMode.info == 'EDIT'" />
                            <div v-if="editMode.info == 'EDIT'" class="flex justify-content-end gap-3">
                                <Button @click="editMode.info = 'READ'" icon="pi pi-times" :label="t('body.status.HUY2')" severity="secondary" />
                                <Button :loading="loading.btn1" @click="onClickSaveGeneralInfo" icon="pi pi-save" :label="t('client.save')" />
                            </div>
                        </div>
                        <div class="flex justify-content-between">
                            <div class="py-2 text-green-700 text-xl font-semibold">
                                {{ t('body.sampleRequest.customer.contact_info_title') }}
                            </div>
                            <Button :disabled="editMode.crd5 == 'EDIT'" @click="onClickEdit('crd5')" icon="pi pi-pencil" :label="t('body.sampleRequest.customer.edit_button')" text />
                        </div>
                        <hr class="m-0" />
                        <div class="py-3 mb-5">
                            <DataTable dataKey="id" v-if="editMode.crd5 == 'READ'" :value="modelStates.crD5" showGridlines>
                                <template #empty>
                                    <div class="p-5 flex justify-content-center align-items-center">
                                        {{ t('body.sampleRequest.customer.no_data_message') }}
                                    </div>
                                </template>
                                <Column field="person" :header="t('body.sampleRequest.customer.contact_person_name_column')" />
                                <Column field="phone" :header="t('body.sampleRequest.customer.phone_number_1_column')" />
                                <Column field="email" :header="t('body.sampleRequest.customer.email_column')" />
                                <Column :header="t('body.sampleRequest.customer.default_column')" class="w-5rem text-center">
                                    <template #body="sp">
                                        <RadioButton v-if="sp.data.default == 'Y'" readonly v-model="sp.data.default" :value="'Y'" />
                                    </template>
                                </Column>
                            </DataTable>
                            <DataTable v-else-if="editMode.crd5 == 'EDIT'" :value="dataEditModel.crd5.filter((el) => el.status != 'D')" showGridlines>
                                <template #empty>
                                    <div class="p-5 flex justify-content-center align-items-center"></div>
                                </template>
                                <Column :header="t('body.sampleRequest.customer.contact_person_name_column')">
                                    <template #body="slotProps">
                                        <InputText v-model="slotProps.data.person" :placeholder="t('body.sampleRequest.customer.contact_person_name_column')" class="w-full" :invalid="slotProps.data.error?.person" />
                                        <small class="text-red-500">{{ slotProps.data.error?.person }}</small>
                                    </template>
                                </Column>
                                <Column :header="t('client.phone_1')">
                                    <template #body="slotProps">
                                        <InputText v-model="slotProps.data.phone" :placeholder="t('body.sampleRequest.customer.phone_number_label')" class="w-full input_number" :invalid="slotProps.data.error?.phone" />
                                        <small class="text-red-500">{{ slotProps.data.error?.phone }}</small>
                                    </template>
                                </Column>
                                <Column header="Email">
                                    <template #body="slotProps">
                                        <InputText v-model="slotProps.data.email" placeholder="example@example.com" class="w-full" :invalid="slotProps.data.error?.email" />
                                        <small class="text-red-500">{{ slotProps.data.error?.email }}</small>
                                    </template>
                                </Column>
                                <Column :header="t('body.sampleRequest.customer.default_column')" class="w-3rem text-center">
                                    <template #body="sp">
                                        <RadioButton @click="onChangeDefault('crd5', null, sp.index)" v-model="sp.data.default" value="Y"> </RadioButton>
                                    </template>
                                </Column>
                                <Column class="w-5rem text-center">
                                    <template #body="slotProps">
                                        <div class="flex">
                                            <Button text icon="pi pi-trash" severity="danger" @click="RemoveRow(slotProps, 'crd5')" />
                                        </div>
                                    </template>
                                </Column>
                                <template #footer>
                                    <div class="flex justify-content-between">
                                        <Button icon="pi pi-plus" @click="AddNewRow('crd5')" :label="t('body.sampleRequest.customer.add_new_button')" outlined />

                                        <div class="flex gap-3">
                                            <Button @click="editMode.crd5 = 'READ'" icon="pi pi-times" severity="secondary" :label="t('body.PurchaseRequestList.cancel_button')" />
                                            <Button :loading="loading.btn1" @click="onClickSaveContact" icon="pi pi-save" :label="t('body.PurchaseRequestList.confirm_button')" />
                                        </div>
                                    </div>
                                </template>
                            </DataTable>
                        </div>
                        <div class="flex justify-content-between">
                            <div class="py-2 text-green-700 text-xl font-semibold">
                                {{ t('body.sampleRequest.customer.shipping_address_title') }}
                            </div>
                            <Button :disabled="editMode.crd1 == 'EDIT'" @click="onClickEdit('crd1', 'S')" icon="pi pi-pencil" :label="t('body.sampleRequest.customer.edit_button')" text />
                        </div>
                        <hr class="m-0" />
                        <div class="py-3 mb-5">
                            <DataTable dataKey="id" v-if="editMode.crd1 == 'READ'" :value="modelStates.crD1.filter((el) => el.type == 'S')" showGridlines>
                                <template #empty>
                                    <div class="p-5 flex justify-content-center align-items-center">
                                        {{ t('body.sampleRequest.customer.no_data_message') }}
                                    </div>
                                </template>
                                <Column field="person" :header="t('body.sampleRequest.customer.contact_person_name_column')" class="w-12rem"> </Column>
                                <Column field="phone" :header="t('body.sampleRequest.customer.phone_number_label')" />
                                <Column field="vehiclePlate" :header="t('body.sampleRequest.customer.license_plate_column')" class="w-9rem"> </Column>
                                <Column field="cccd" :header="t('body.sampleRequest.customer.id_card_column')" />

                                <Column field="email" :header="t('body.sampleRequest.customer.email_column')" />
                                <Column field="" :header="t('body.sampleRequest.customer.address_column')">
                                    <template #body="{ data }">
                                        {{ [data.address, data.locationName, data.areaName].filter((x) => x).join(', ') }}
                                    </template>
                                </Column>
                                <Column :header="t('body.sampleRequest.customer.default_column')" class="w-5rem text-center">
                                    <template #body="sp">
                                        <RadioButton v-if="sp.data.default == 'Y'" readonly v-model="sp.data.default" :value="'Y'" />
                                    </template>
                                </Column>
                            </DataTable>
                            <!-- Địa chỉ giao hàng | current-area -->
                            <DataTable v-else-if="editMode.crd1 == 'EDIT'" :value="dataEditModel.crd1" showGridlines>
                                <template #empty>
                                    <div class="p-5 flex justify-content-center align-items-center"></div>
                                </template>
                                <Column :header="t('body.sampleRequest.customer.contact_person_column')" class="w-15rem">
                                    <template #body="slotProps">
                                        <InputText v-model="slotProps.data.person" :placeholder="t('body.sampleRequest.customer.contact_person_name_column')" class="w-full" :invalid="slotProps.data.error?.person" />
                                        <small class="text-red-500">{{ slotProps.data.error?.person }}</small>
                                    </template>
                                </Column>
                                <Column :header="t('body.sampleRequest.customer.phone_number_label')" class="w-10rem">
                                    <template #body="slotProps">
                                        <InputText v-model="slotProps.data.phone" :placeholder="t('body.sampleRequest.customer.phone_number_label')" class="w-full input_number" :invalid="slotProps.data.error?.phone" />
                                        <small class="text-red-500">{{ slotProps.data.error?.phone }}</small>
                                    </template>
                                </Column>
                                <Column :header="t('body.sampleRequest.customer.license_plate_column')" class="w-10rem">
                                    <template #body="slotProps">
                                        <InputText v-model="slotProps.data.vehiclePlate" placeholder="30A1-123.45" class="w-full input_number" :invalid="slotProps.data.error?.vehiclePlate ? true : false" />
                                        <small class="text-red-500">{{ slotProps.data.error?.vehiclePlate }}</small>
                                    </template>
                                </Column>
                                <Column :header="t('body.sampleRequest.customer.id_card_column')" class="w-11rem">
                                    <template #body="slotProps">
                                        <InputText v-model="slotProps.data.cccd" :placeholder="t('body.sampleRequest.customer.id_card_column')" class="w-full input_number" :invalid="slotProps.data.error?.cccd ? true : false" />
                                        <small class="text-red-500">{{ slotProps.data.error?.cccd }}</small>
                                    </template>
                                </Column>
                                <Column :header="t('body.sampleRequest.customer.email_column')" class="w-11rem">
                                    <template #body="slotProps">
                                        <InputText v-model="slotProps.data.email" placeholder="example@email.com" class="w-full input_number" :invalid="slotProps.data.error?.email ? true : false" />
                                        <small class="text-red-500">{{ slotProps.data.error?.email }}</small>
                                    </template>
                                </Column>
                                <Column :header="t('body.sampleRequest.customer.address_column')" class="w-15rem">
                                    <template #body="slotProps">
                                        <InputGroup>
                                            <InputText readonly :modelValue="slotProps.data._addressLabel" :placeholder="t('body.sampleRequest.customer.shipping_address_title')" />
                                            <Button @click="onClickChangeAddress(slotProps)" severity="secondary" icon="pi pi-pencil" />
                                        </InputGroup>
                                        <small class="text-red-500">{{ slotProps.data.error?._addressLabel }}</small>
                                    </template>
                                </Column>
                                <Column :header="t('body.sampleRequest.customer.default_column')" class="w-3rem text-center">
                                    <template #body="sp">
                                        <RadioButton :disabled="!sp.data.id" @click="onChangeDefault('crd1', 'S', sp.index)" v-model="sp.data.default" value="Y"> </RadioButton>
                                    </template>
                                </Column>
                                <Column class="w-5rem text-center">
                                    <template #body="slotProps">
                                        <div class="flex">
                                            <Button text icon="pi pi-trash" severity="danger" @click="RemoveRow(slotProps, 'crd1', 'S')" />
                                        </div>
                                    </template>
                                </Column>
                                <template #footer>
                                    <div class="flex justify-content-between">
                                        <Button icon="pi pi-plus" @click="AddNewRow('crd1', 'S')" :label="t('body.sampleRequest.customer.add_new_button')" outlined />

                                        <div class="flex gap-3">
                                            <Button @click="editMode.crd1 = 'READ'" icon="pi pi-times" severity="secondary" :label="t('body.PurchaseRequestList.cancel_button')" />
                                            <Button :loading="loading.btn1" @click="onClickSaveDeliveryAddress" icon="pi pi-save" :label="t('body.PurchaseRequestList.confirm_button')" />
                                        </div>
                                    </div>
                                </template>
                            </DataTable>
                        </div>
                        <div class="flex justify-content-between">
                            <div class="py-2 text-green-700 text-xl font-semibold">
                                {{ t('body.sampleRequest.customer.invoice_info_title') }}
                            </div>
                            <Button :disabled="editMode.crd1_1 == 'EDIT'" @click="onClickEdit('crd1', 'B')" icon="pi pi-pencil" :label="t('body.sampleRequest.customer.edit_button')" text />
                        </div>
                        <hr class="m-0" />
                        <div class="py-3">
                            <DataTable dataKey="id" v-if="editMode.crd1_1 == 'READ'" :value="modelStates.crD1.filter((el) => el.type == 'B')" showGridlines>
                                <template #empty>
                                    <div class="p-5 flex justify-content-center align-items-center">
                                        {{ t('body.sampleRequest.customer.no_data_message') }}
                                    </div>
                                </template>
                                <Column field="person" :header="t('body.sampleRequest.customer.contact_person_name_column')" />
                                <Column field="email" :header="t('body.sampleRequest.customer.email_column')" />
                                <Column field="" :header="t('body.sampleRequest.customer.address_label')">
                                    <template #body="{ data }">
                                        {{ `${data.address}, ${data.locationName || ''}, ${data.areaName || ''}` }}
                                    </template>
                                </Column>
                                <Column :header="t('body.sampleRequest.customer.default_column')" class="w-5rem">
                                    <template #body="sp">
                                        <RadioButton v-if="sp.data.default == 'Y'" readonly v-model="sp.data.default" :value="'Y'" />
                                    </template>
                                </Column>
                            </DataTable>
                            <DataTable v-else-if="editMode.crd1_1 == 'EDIT'" :value="dataEditModel.crd1_1" showGridlines>
                                <template #empty>
                                    <div class="p-5 flex justify-content-center align-items-center"></div>
                                </template>
                                <Column :header="t('body.sampleRequest.customer.contact_person_name_column')" class="w-15rem">
                                    <template #body="slotProps">
                                        <InputText v-model="slotProps.data.person" :placeholder="t('body.sampleRequest.customer.contact_person_name_column')" class="w-full" :invalid="slotProps.data.error?.person ? true : false" />
                                        <small class="text-red-500">{{ slotProps.data.error?.person }}</small>
                                    </template>
                                </Column>
                                <Column header="Email" class="w-20rem">
                                    <template #body="slotProps">
                                        <InputText type="email" v-model="slotProps.data.email" placeholder="example@example.com" class="w-full input_number" :invalid="slotProps.data.error?.email ? true : false" />
                                        <small class="text-red-500">{{ slotProps.data.error?.email }}</small>
                                    </template>
                                </Column>
                                <Column :header="t('body.sampleRequest.customer.address_column')">
                                    <template #body="slotProps">
                                        <InputGroup>
                                            <InputText readonly :modelValue="slotProps.data._addressLabel" :placeholder="t('body.sampleRequest.customer.invoice_info_title')" :invalid="slotProps.data.error?._addressLabel ? true : false" />
                                            <Button @click="onClickChangeAddress(slotProps)" severity="secondary" icon="pi pi-pencil" />
                                        </InputGroup>
                                        <small class="text-red-500">{{ slotProps.data.error?._addressLabel }}</small>
                                    </template>
                                </Column>
                                <Column :header="t('body.sampleRequest.customer.default_column')" class="w-3rem text-center">
                                    <template #body="sp">
                                        <RadioButton :disabled="!sp.data.id" @click="onChangeDefault('crd1', 'B', sp.index)" v-model="sp.data.default" value="Y"> </RadioButton>
                                    </template>
                                </Column>
                                <Column class="w-5rem text-center">
                                    <template #body="slotProps">
                                        <div class="flex">
                                            <Button text icon="pi pi-trash" severity="danger" @click="RemoveRow(slotProps, 'crd1', 'B')" />
                                        </div>
                                    </template>
                                </Column>
                                <template #footer>
                                    <div class="flex justify-content-between">
                                        <Button icon="pi pi-plus" @click="AddNewRow('crd1', 'B')" :label="t('body.sampleRequest.customer.add_new_button')" outlined />

                                        <div class="flex gap-3">
                                            <Button @click="editMode.crd1_1 = 'READ'" icon="pi pi-times" severity="secondary" :label="t('body.PurchaseRequestList.cancel_button')" />
                                            <Button :loading="loading.btn1" @click="onClickSaveBillInfo" icon="pi pi-save" :label="t('body.PurchaseRequestList.confirm_button')" />
                                        </div>
                                    </div>
                                </template>
                            </DataTable>
                        </div>

                        <div class="flex justify-content-between mt-5">
                            <div class="py-2 text-green-700 text-xl font-semibold">
                                {{ t('body.sampleRequest.customer.group_info_title') }}
                            </div>
                        </div>
                        <div class="py-3" v-if="modelStates.groups">
                            <DataTable dataKey="id" :value="modelStates.groups" showGridlines>
                                <template #empty>
                                    <div class="p-5 flex justify-content-center align-items-center">
                                        {{ t('body.sampleRequest.customer.no_data_message') }}
                                    </div>
                                </template>
                                <Column field="groupName" :header="t('body.sampleRequest.customerGroup.group_name_label')" />
                                <Column field="description" :header="t('body.sampleRequest.customer.description_column')" />
                                <Column field="isActive" :header="t('client.status')">
                                    <template #body="slotProps">
                                        <Tag :severity="slotProps.data.isActive ? 'success' : 'danger'">
                                        {{ slotProps.data.isActive ? t('body.sampleRequest.customer.active_status') : t('body.sampleRequest.customer.inactive_status') }}
                                        </Tag>
                                    </template>
                                </Column>
                            </DataTable>
                        </div>
                    </div>
                    <!-- Thông tin chung - General Information -->

                    <ClassifyDistributed v-if="!loading.scrn" :setup="{ API, modelStates, toast }" />
                    <DistributedProducts v-if="!loading.scrn" @on-save="ResponseDP" :setup="{ API, modelStates, toast }" />
                    <PaymentTerms v-if="!loading.scrn" :setup="{ API, modelStates, toast }" />
                    <Documents v-if="!loading.scrn" :setup="{ API, modelStates, toast }" />
                </div>
            </div>
        </template>
    </div>

    <AddressDialog v-model:visible="visible" :editMode="editMode" :generalInfoModel="generalInfoModel" :addressModel="addressModel" @confirm="onConfirmAddress" />
</template>

<script setup>
import { ref, shallowRef, onBeforeMount, onUnmounted, reactive, watch } from 'vue';
import { useRoute } from 'vue-router';
import { format } from 'date-fns';
import API from '@/api/api-main';
import { useToast } from 'primevue/usetoast';
import ClassifyDistributed from '../components/ClassifyDistributed.vue';
import DistributedProducts from '../components/DistributedProducts.vue';
import PaymentTerms from '../components/PaymentTerms.vue';
import Documents from '../components/Documents.vue';
import AddressDialog from '../components/AddressDialog.vue';
import { useI18n } from 'vue-i18n';
import { phoneRegex, vehiclePlateRegex, emailRegex, cccdRegex } from '@/helpers/regex';

const { t } = useI18n();
const route = useRoute();
const toast = useToast();
let visible = reactive({
    address: false
});

const loading = reactive({
    scrn: false,
    btn1: false
});

const editMode = reactive({
    info: 'READ',
    crd5: 'READ',
    crd1: 'READ',
    crd1_1: 'READ'
});

const errorMessage = reactive({
    system: null,
    detail: null
});

const generalInfoModel = reactive({
    id: 0,
    cardCode: null,
    cardName: null,
    frgnName: null,
    licTradNum: null,
    email: null,
    phone: null,
    address: null,
    areaId: null,
    areaName: null,
    locationId: null,
    locationName: null,
    note: null,
    person: null,
    status: null,
    dateOfBirth: null,
    _addressLabel: null,
    saleId: null,
    isInterCom: false,
    isBusinessHouse: false
});

const defaultModel = {
    id: null,
    series: null,
    cardCode: null,
    cardName: null,
    frgnName: null,
    cardType: 'C',
    licTradNum: null,
    avatar: null,
    gender: null,
    dateOfBirth: null,
    locationId: null,
    locationName: null,
    areaId: null,
    areaName: null,
    address: null,
    email: null,
    phone: null,
    person: null,
    note: null,
    status: null,
    rStatus: null,
    createdDate: null,
    creator: null,
    updatedDate: null,
    updator: null,
    crD1: [],
    crD2: [],
    crD3: [],
    crD4: [],
    crD5: [],
    isAllArea: null,
    area: null,
    isAllBrand: null,
    brand: null,
    isAllItemType: null,
    itemType: null,
    isAllIndustry: null,
    industry: null,
    isAllPacking: null,
    packing: null,
    isBusinessHouse: false,
    isInterCom: false
};
const modelStates = reactive({ ...defaultModel });
// shallowRef: users chỉ dùng làm options cho Dropdown — không cần deep reactivity
const users = shallowRef([]);
const dataEditModel = reactive({
    crd5: [],
    crd1: [],
    crd1_1: []
});

const addressModel = reactive({
    area: {},
    location: {},
    address: null,
    slotProps: null
});

const addressType = ref(true);

// Validation columns for contacts
const cols = [
    {
        field: 'person',
        header: t('body.sampleRequest.customer.contact_person_column'),
        required: true,
        regex: null
    },
    {
        field: 'phone',
        header: t('client.phone_1'),
        required: true,
        regex: /^(0|\+[0-9]{1,})(\s|\.)?(3[2-9]|5[689]|7[06-9]|8[1-9]|9[0-9])(\d{7})$/g
    },
    {
        field: 'email',
        header: t('client.email_contact'),
        required: true,
        regex: /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}$/g
    }
];

// Validation columns for delivery addresses
const deliveryAddressCols = [
    {
        field: 'person',
        header: t('client.invoice_recipient'),
        regex: null,
        required: true,
        placeholder: 'Nguyễn Văn A'
    },
    {
        field: 'phone',
        header: t('client.invoice_phone'),
        regex: phoneRegex,
        placeholder: '0123456789',
        required: true
    },
    {
        field: 'vehiclePlate',
        header: t('client.license_plate'),
        regex: vehiclePlateRegex,
        placeholder: '30A-12345',
        required: true
    },
    {
        field: 'cccd',
        header: t('client.cccd_number'),
        regex: cccdRegex,
        placeholder: '012345678912',
        required: true
    },
    {
        field: 'email',
        header: t('client.email_delivery'),
        regex: emailRegex,
        placeholder: 'example@email.com',
        required: false
    },
    {
        field: '_addressLabel',
        header: t('client.delivery_address_label'),
        required: true,
        regex: null
    }
];

const billAddressCols = [
    {
        field: 'person',
        header: 'Tên người nhận',
        regex: null,
        required: true,
        placeholder: 'Nguyễn Văn A'
    },
    {
        field: 'email',
        header: 'Email',
        regex: emailRegex,
        placeholder: 'example@email.com',
        required: true
    },
    {
        field: '_addressLabel',
        header: t('body.sampleRequest.customer.address_column'),
        required: true,
        regex: null
    }
];

const validateCRD = (array, cols) => {
    let errorCount = 0;
    array
        .filter((el) => el.status != 'D')
        .forEach((row) => {
            row.error = {};
            for (const col of cols) {
                if (col.required) {
                    if (!row[col.field] || !row[col.field]?.trim()) {
                        row.error[col.field] = 'Không được để trống';
                        errorCount++;
                    } else {
                        if (col.regex && !row[col.field].trim().match(col.regex)) {
                            row.error[col.field] = `${col.header} không đúng định dạng`;
                            errorCount++;
                        }
                    }
                } else {
                    if (row[col.field]?.trim()) {
                        if (col.regex && !row[col.field].trim().match(col.regex)) {
                            row.error[col.field] = `${col.header} không đúng định dạng`;
                            errorCount++;
                        }
                    }
                }
            }
        });
    return errorCount < 1;
};

const validateContact = () => {
    return validateCRD(dataEditModel.crd5, cols);
};

const f = (n) => {
    return n < 10 ? '0' + n : n;
};

const ResponseDP = (res) => {
    Object.assign(modelStates, res);
};

const onChangeAddress = (address) => {
    Object.assign(generalInfoModel, address);
};

const onClickSaveGeneralInfo = async () => {
    loading.btn1 = true;
    try {
        let { _addressLabel, ..._payload } = generalInfoModel;
        const payload = JSON.parse(JSON.stringify(_payload));
        const dob = new Date(_payload.dateOfBirth);
        payload.dateOfBirth = `${dob.getFullYear()}-${f(dob.getMonth() + 1)}-${f(dob.getDate())}T00:00:00.000Z`;
        let formData = new FormData();
        formData.append('item', JSON.stringify(payload));

        await API.update('customer/' + payload.id, formData);
        toast.add({
            severity: 'success',
            summary: 'Thành công',
            detail: 'Cập nhật thông tin chung thành công',
            life: 3000
        });
        refreshCustomerData();
        editMode.info = 'READ';
    } catch (error) {
        toast.add({
            severity: 'error',
            summary: 'Thông báo',
            detail: error.message,
            life: 3000
        });
    }
    loading.btn1 = false;
};

const onClickEditGeneralInfo = () => {
    Object.keys(generalInfoModel).forEach((key) => {
        generalInfoModel[key] = modelStates[key];
        if (key == 'dateOfBirth' && modelStates.dateOfBirth) {
            generalInfoModel.dateOfBirth = new Date(modelStates.dateOfBirth);
        }
    });
    let address = [modelStates.address, modelStates.locationName, modelStates.areaName].filter((el) => el).join(', ');
    generalInfoModel._addressLabel = address;
    editMode.info = 'EDIT';
};

const onConfirmAddress = (newAddress) => { 
    if (editMode.info == 'EDIT') {
        Object.assign(generalInfoModel, newAddress);
    } else {
        Object.assign(addressModel.slotProps.data, newAddress);
    }
    visible.address = false;
};

const onClickChangeAddress = (sp) => {
    visible.address = true;
    if (sp.data.id) {
        if (!sp.data.locationId || !sp.data.areaId) {
            addressType.value = false;
        }
        addressModel.location = {
            id: sp.data.locationId,
            name: sp.data.locationName
        };
        addressModel.area = {
            id: sp.data.areaId,
            name: sp.data.areaName
        };
        addressModel.address = sp.data.address;
        addressModel.slotProps = sp;
    } else {
        addressModel.slotProps = sp;
    }
    
};

const onClickSaveContact = async () => {
    if (!validateContact()) return;
    loading.btn1 = true;
    let crd5 = dataEditModel.crd5;
    let payload = {
        id: modelStates.id,
        crD5: crd5
    };
    let fromData = new FormData();
    fromData.append('item', JSON.stringify(payload));
    try {
        await API.update('customer/' + modelStates.id, fromData);
        toast.add({
            severity: 'success',
            summary: 'Thông báo',
            detail: 'Cập nhật dữ liệu thành công!',
            life: 3000
        });
        editMode.crd5 = 'READ';
        refreshCustomerData();
    } catch (e) {
        toast.add({
            severity: 'error',
            summary: 'Thông báo',
            detail: 'Cập nhật dữ liệu thất bại!',
            life: 3000
        });
    } finally {
        loading.btn1 = false;
    }
};

const onClickSaveDeliveryAddress = async () => {
    if (!validateCRD(dataEditModel.crd1, deliveryAddressCols)) return;
    loading.btn1 = true;
    let rowCount = dataEditModel.crd1.filter((el) => el.type == 'S').length;
    let crd1 = dataEditModel.crd1
        .filter((x) => x.address && x.person && x.phone)
        .map((el) => {
            let { _addressLabel, ...z } = el;
            z.type = 'S';
            return z;
        });
    if (crd1.length < rowCount) {
        alert('Vui lòng nhập đầy đủ thông tin');
        loading.btn1 = false;
        return;
    }
    let payload = {
        id: modelStates.id,
        crD1: crd1
    };
    let fromData = new FormData();
    fromData.append('item', JSON.stringify(payload));
    try {
        await API.update('customer/' + modelStates.id, fromData);
        toast.add({
            severity: 'success',
            summary: 'Thông báo',
            detail: 'Cập nhật dữ liệu thành công!',
            life: 3000
        });
        editMode.crd1 = 'READ';
        refreshCustomerData();
    } catch (e) {
        toast.add({
            severity: 'error',
            summary: 'Thông báo',
            detail: 'Cập nhật dữ liệu thất bại!',
            life: 3000
        });
    } finally {
        loading.btn1 = false;
    }
};

const onClickSaveBillInfo = async () => {
    if (!validateCRD(dataEditModel.crd1_1, billAddressCols)) return;
    loading.btn1 = true;
    const rowCount = dataEditModel.crd1_1.length;
    let crd1_1 = dataEditModel.crd1_1.map((el) => {
        let { _addressLabel, ...z } = el;
        z.type = 'B';
        return z;
    });
    const rowOkCount = crd1_1.filter((el) => el.person && el.email && el.address).length;
    if (rowOkCount < rowCount) {
        alert('Vui lòng nhập đầy đủ thông tin');
        loading.btn1 = false;
        return;
    }
    let payload = {
        id: modelStates.id,
        crD1: crd1_1
    };
    let fromData = new FormData();
    fromData.append('item', JSON.stringify(payload));
    try {
        await API.update('customer/' + modelStates.id, fromData);
        toast.add({
            severity: 'success',
            summary: 'Thông báo',
            detail: 'Cập nhật dữ liệu thành công!',
            life: 3000
        });
        editMode.crd1_1 = 'READ';
        refreshCustomerData();
    } catch (e) {
        toast.add({
            severity: 'error',
            summary: 'Thông báo',
            detail: 'Cập nhật dữ liệu thất bại!',
            life: 3000
        });
    } finally {
        loading.btn1 = false;
    }
};

const onClickEdit = (table, _type) => {
    if (table == 'crd1') {
        if (_type == 'S') {
            dataEditModel.crd1 = JSON.parse(JSON.stringify(modelStates.crD1))
                .filter((el) => el.type == _type)
                .map((el) => {
                    return {
                        ...el,
                        _addressLabel: [el.address, el.locationName, el.areaName].filter((x) => x).join(', '),
                        status: 'U'
                    };
                });
            editMode.crd1 = 'EDIT';
            if (dataEditModel.crd1.length < 1) {
                AddNewRow('crd1', 'S');
            }
        }
        if (_type == 'B') {
            dataEditModel.crd1_1 = JSON.parse(JSON.stringify(modelStates.crD1))
                .filter((el) => el.type == _type)
                .map((el) => {
                    return {
                        ...el,
                        _addressLabel: `${el.address}, ${el.locationName}, ${el.areaName}`,
                        status: 'U'
                    };
                });
            editMode.crd1_1 = 'EDIT';
            if (dataEditModel.crd1_1.length < 1) {
                AddNewRow('crd1', 'B');
            }
        }
    } else {
        dataEditModel[table] = JSON.parse(JSON.stringify(modelStates.crD5)).map((el) => ({
            ...el,
            status: 'U'
        }));
        if (dataEditModel[table].length < 1) {
            AddNewRow(table);
        }
        editMode[table] = 'EDIT';
    }
};

const onChangeDefault = (table, _type, index) => {
    if (_type) {
        if (_type == 'S') {
            dataEditModel.crd1.forEach((row, i) => {
                if (i != index) {
                    row.default = 'N';
                }
            });
        }
        if (_type == 'B') {
            dataEditModel.crd1_1.forEach((row, i) => {
                if (i != index) {
                    row.default = 'N';
                }
            });
        }
    } else {
        dataEditModel[table].forEach((row, i) => {
            if (i != index) {
                row.default = 'N';
            }
        });
    }
};

const AddNewRow = (table, _type = null) => {
    if (table == 'crd1' && _type) {
        if (_type == 'S') {
            dataEditModel.crd1.push({ id: 0, status: 'A', default: 'N', type: _type });
        }
        if (_type == 'B') {
            dataEditModel.crd1_1.push({ id: 0, status: 'A', default: 'N', type: _type });
        }
    } else {
        dataEditModel[table].push({ id: 0, status: 'A', default: 'N' });
    }
};

const RemoveRow = (sp, table, _type) => {
    if (_type) {
        if (_type == 'S') {
            if (sp.data.id) {
                sp.data.status = 'D';
            } else {
                dataEditModel.crd1.splice(sp.index, 1);
            }
        }
        if (_type == 'B') {
            if (sp.data.id) {
                sp.data.status = 'D';
            } else {
                dataEditModel.crd1_1.splice(sp.index, 1);
            }
        }
    } else {
        if (sp.data.id) {
            sp.data.status = 'D';
        } else {
            dataEditModel.crd5.splice(sp.index, 1);
        }
    }
};

// AbortController để hủy request khi component unmount (tránh ghost requests)
let _abortCtrl = new AbortController();
onUnmounted(() => {
    _abortCtrl.abort();
});

const GetAllUser = async () => {
    try {
        const response = await API.get('Account/getall?skip=0&limit=50&userType=APSP&Filter=(status=A)');
        const data = response.data;
        // Tương thích cả 3 format: { item: [] } / { items: [] } / []
        users.value = data?.item ?? data?.items ?? (Array.isArray(data) ? data : []);
    } catch (err) {
        console.error('[GetAllUser] error:', err?.message ?? err);
        users.value = [];
    }
};

// Chỉ reload customer data — KHÔNG fetch lại user list
const refreshCustomerData = async () => {
    if (!route.params.id) return;
    try {
        const res = await API.get('Customer/' + route.params.id);
        const data = res.data;
        if (!data || (Object.hasOwn(data, 'items') && data.items == null)) {
            errorMessage.system = 'Không tìm thấy Khách hàng này';
        } else {
            Object.assign(modelStates, data);
            modelStates._addressLabel = [data.address, data.locationName, data.areaName].filter((el) => el).join(', ');
            if (data.dateOfBirth) {
                const dateDob = new Date(data.dateOfBirth);
                const dobStr = `${dateDob.getFullYear()}-${f(dateDob.getMonth() + 1)}-${f(dateDob.getDate())}T00:00:00.000Z`;
                modelStates.dateOfBirth = dobStr;
            }
        }
    } catch (error) {
        errorMessage.detail = error?.message ?? String(error);
        const status = error?.statusCode ?? error?.response?.status;
        const isNetworkError = !status && (error?.code === 'ERR_EMPTY_RESPONSE' || error?.code === 'ERR_NETWORK' || error?.code === 'ECONNABORTED');
        if (status === 400) {
            errorMessage.system = 'Không tìm thấy Khách hàng này';
        } else if (isNetworkError) {
            errorMessage.system = 'Không thể kết nối đến máy chủ. Vui lòng thử lại.';
        } else {
            errorMessage.system = 'Lỗi hệ thống';
        }
    }
};

const initialComponent = async () => {
    _abortCtrl = new AbortController();
    // Chạy song song: user list và customer data độc lập nhau
    await Promise.allSettled([GetAllUser(), refreshCustomerData()]);
};

onBeforeMount(async () => {
    loading.scrn = true;
    await initialComponent();
    loading.scrn = false;
});

// Watch route params: nếu navigate giữa 2 trang detail khác id (vd: từ thông báo),
// tải lại customer data mà không cần unmount/remount toàn bộ component
watch(
    () => route.params.id,
    (newId, oldId) => {
        if (newId && newId !== oldId) refreshCustomerData();
    }
);
</script>

<style scoped>
.input_number::-webkit-outer-spin-button,
.input_number::-webkit-inner-spin-button {
    -webkit-appearance: none;
    margin: 0;
}

/* Firefox */
.input_number[type='number'] {
    -moz-appearance: textfield;
}

.sticky_class {
    position: sticky;
    top: 160px;
}
</style>
