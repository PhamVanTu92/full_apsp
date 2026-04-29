<template>
    <div>
        <Button @click="openAddNewReqDialog()" icon="pi pi-plus-circle"
            :label="t('body.PurchaseRequestList.createPickupRequest')"/>

        <Dialog v-model:visible="AddNewReqModal" modal :header="t('body.PurchaseRequestList.createPickupRequest')"
            :style="{ width: '1400px' }" @keydown.esc.stop>
            <div class="card flex flex-column gap-3">
                <div class="flex justify-content-between align-items-center">
                    <div class="flex gap-2">
                        <div class="flex flex-column gap-2">
                            <label for="">{{ t('body.PurchaseRequestList.requestCode') }}</label>
                            <InputText v-model="payload.invoiceCode" disabled></InputText>
                        </div>
                        <div class="flex flex-column gap-2" v-if="authStore.userType === 'APSP'">
                            <label for="">{{ t('body.PurchaseRequestList.customer') }}</label>
                            <InputGroup>
                                <AutoComplete
                                    :placeholder="t('body.PurchaseRequestList.enter_customer_name_placeholder')"
                                    :pt:input:class="'w-30rem'" v-model="Customer" :suggestions="DataCustomer"
                                    optionLabel="cardName" @complete="GetCustomer(Customer)">
                                    <template #option="data">
                                        <div class="flex gap-2 align-items-center">
                                            <Avatar icon="pi pi-user" class="mr-2" size="large" style="
                                                    background-color: #ece9fc;
                                                    color: #2a1261;
                                                " shape="circle" />
                                            <div class="flex flex-column gap-1">
                                                <span class="font-bold">{{
                                                    data.option.cardName
                                                    }}</span>
                                                <span class="text-gray-500">{{
                                                    data.option.email
                                                    }}</span>
                                            </div>
                                        </div>
                                    </template>
                                </AutoComplete>
                                <InputGroupAddon>
                                    <i class="pi pi-search"></i>
                                </InputGroupAddon>
                            </InputGroup>
                        </div>
                        <div class="flex flex-column gap-2">
                            <label for="">{{ t('body.PurchaseRequestList.pickup_time_label') }}</label>
                            <Calendar showTime showTimeInput hourFormat="12" v-model="payload.deliveryTime"
                                :minDate="new Date()" showIcon iconDisplay="input" dateFormat="dd/mm/yy"
                                :placeholder="t('body.PurchaseRequestList.pickup_time_label')">
                            </Calendar>
                        </div>
                    </div>
                    <!-- <DebtCheck
                        :DebtData="Customer?.crD4"
                        :TotalDebtData="Customer?.crD3"
                        type="icon"
                        :paymentMethod="'PayNow'"
                        :TotalPayment="0"
                        :ItemDebt="null"
                    >
                    </DebtCheck> -->
                    <KiemTraCongNo :bpId="Customer?.id" />
                </div>

                <TabView>
                    <TabPanel :header="t('body.PurchaseRequestList.products_tab')">
                        <div class="flex flex-column gap-3">
                            <DataTable :value="selectedProduct" showGridlines stripedRows>
                                <template #empty>
                                    <div class="py-5 my-5 text-center">
                                        {{ t('body.PurchaseRequestList.no_matching_product_message') }}
                                    </div>
                                </template>
                                <Column header="#">
                                    <template #body="slotProps">
                                        {{ slotProps.index + 1 }}
                                    </template>
                                </Column>
                                <Column field="itemName" :header="t('body.PurchaseRequestList.product_name_column')">
                                </Column>
                                <Column field="quantity" :header="t('body.PurchaseRequestList.quantity_column')">
                                    <template #body="slotProps">
                                        <div class="flex justify-content-center">
                                            <InputNumber v-model="slotProps.data.quantity" :pt="{
                                                input: {
                                                    root: { style: 'width: 6rem' },
                                                },
                                            }" :min="1" :max="slotProps.data.onHand -
                                                slotProps.data.onOrder
                                                " inputId="horizontal-buttons" showButtons>
                                            </InputNumber>
                                        </div>
                                    </template>
                                </Column>
                                <Column field="ougp.ouom.uomName" :header="t('body.PurchaseRequestList.unit_column')">
                                </Column>
                                <Column field="onHand"
                                    :header="t('body.PurchaseRequestList.quantity_in_warehouse_column')"></Column>
                                <Column :header="t('body.PurchaseRequestList.actions_column')">
                                    <template #body="slotProps">
                                        <div class="flex justify-content-center">
                                            <Button @click="removeItemSelected(slotProps.index)" severity="danger" text
                                                icon="pi pi-trash"/>
                                        </div>
                                    </template>
                                </Column>
                            </DataTable>
                            <div class="flex justify-content-start">
                                <ProductSelector :typeModal="'YCLHG'" :customer="Customer" btnClass="w-12rem"
                                    icon="pi pi-plus-circle" :label="t('body.PurchaseRequestList.find_product_button')"
                                    outlined @confirm="updateSelectedProduct" type="PurchaseRequest">
                                </ProductSelector>
                            </div>
                        </div>
                    </TabPanel>
                    <TabPanel :header="t('body.PurchaseRequestList.shipping_info_tab')">
                        <div class="card bg-gray-100 flex flex-column gap-3">
                            <strong>{{ t('body.PurchaseRequestList.customer') }}</strong>
                            <div class="flex gap-2">
                                <div class="flex flex-column gap-2 w-full">
                                    <label for="">{{ t('body.PurchaseRequestList.customer') }} (Optional)</label>
                                    <InputText class="w-full" readonly :value="typeof Customer === 'object'
                                        ? Customer.cardName
                                        : ''
                                        ">
                                    </InputText>
                                </div>
                                <div class="flex flex-column gap-2 w-full">
                                    <label for="">{{ t('body.sampleRequest.customer.phone_label') }}</label>
                                    <InputText class="w-full" readonly :value="typeof Customer === 'object'
                                        ? Customer.phone
                                        : ''
                                        ">
                                    </InputText>
                                </div>
                            </div>
                            <div class="flex flex-column gap-3">
                                <div class="flex flex-column gap-2">
                                    <label for="">{{ t('body.sampleRequest.customer.email_label') }}</label>
                                    <InputText readonly :value="typeof Customer === 'object'
                                        ? Customer.email
                                        : ''
                                        "></InputText>
                                </div>
                                <SearchAddress :type="'YCLHG'" :customer="Customer"
                                    :data="Customer ? Customer.crD1 : []">
                                </SearchAddress>

                                <!-- <SearchAddress @onClick="GetDataAddress($event)"></SearchAddress> -->
                            </div>
                        </div>
                    </TabPanel>
                </TabView>
                <div class="flex flex-column gap-2">
                    <label for="">{{ t('body.PurchaseRequestList.note_label') }}</label>
                    <Textarea v-model="payload.note" :placeholder="t('body.PurchaseRequestList.note_placeholder')"
                        class="w-30rem"></Textarea>
                </div>
            </div>
            <template #footer>
                <div class="flex gap-2">
                    <Button @click="AddNewReqModal = false" :label="t('body.PurchaseRequestList.cancel_button')"
                        icon="pi pi-times" outlined/>
                    <Button @click="confrimContinue()" :label="t('body.PurchaseRequestList.confirm_button')"
                        icon="pi pi-check"/>
                </div>
            </template>
        </Dialog>

        <Dialog v-model:visible="addNewItemModal" modal :header="t('body.PurchaseRequestList.find_product_button')"
            :style="{ width: '1500px' }">
            <div class="flex flex-column gap-3">
                <FiltersProducts @getQueryString="getQueryString"></FiltersProducts>
                <DataTable v-model:selection="selectedProduct" :value="ProductsInTable" showGridlines stripedRows>
                    <template #empty>
                        <div class="text-center p-2">
                            {{ t('body.PurchaseRequestList.no_matching_product_message') }}
                        </div>
                    </template>
                    <Column selectionMode="multiple" headerStyle="width: 3rem"> </Column>
                    <Column header="STT">
                        <template #body="slotProps">
                            <div class="text-right">
                                {{ slotProps.index + 1 }}
                            </div>
                        </template>
                    </Column>
                    <Column field="itemName" :header="t('body.PurchaseRequestList.product_name_column')"></Column>
                    <Column field="ougp.ouom.uomName" :header="t('body.PurchaseRequestList.unit_column')"></Column>

                    <Column :header="t('body.PurchaseRequestList.quantity_column')">
                        <template #body="slotProps">
                            <div class="flex justify-content-center">
                                <InputNumber @input="onQuantityChange(slotProps.data)" v-model="slotProps.data.quantity"
                                    :pt="{ input: { root: { style: 'width: 6rem' } } }" :min="0"
                                    :max="slotProps.data.onHand" inputId="horizontal-buttons" showButtons>
                                </InputNumber>
                            </div>
                        </template>
                    </Column>
                    <Column field="onHand" :header="t('body.PurchaseRequestList.quantity_in_warehouse_column')">
                        <template #body="slotProps">
                            <div class="text-right">
                                {{ slotProps.data.onHand }}
                            </div>
                        </template>
                    </Column>
                </DataTable>
            </div>
            <template #footer>
                <div class="flex gap-2">
                    <Button @click="addNewItemModal = false" :label="t('body.PurchaseRequestList.cancel_button')"
                        outlined/>
                    <Button :disabled="selectedProduct.find((val) => val.quantity <= 0) || selectedProduct.length <= 0" @click="confirmSelectedProd" :label="t('body.PurchaseRequestList.confirm_button')"/>
                </div>
            </template>
        </Dialog>
    </div>
    <loading v-if="isLoading"></loading>
</template>

<script setup>
import { computed, onMounted, ref, watch } from "vue";
import API from "../api/api-main";
import { useGlobal } from "@/services/useGlobal";
import { useAuthStore } from "@/Pinia/auth";
import { format } from "date-fns";
import { useI18n } from "vue-i18n";

const { toast, FunctionGlobal } = useGlobal();

const { t } = useI18n();
const authStore = useAuthStore();
const emits = defineEmits(["fetchData"]);
const isLoading = ref(false);
const submited = ref(false);
const selectedProduct = ref([]);
const payload = ref({
    id: 0,
    discount: 0,
    distcountAmount: 0,
    discountByPromotion: 0,
    distcountAmountPromotion: 0,
    vatAmount: 0,
    docDate: "",
    deliveryTime: format(new Date(), "dd/MM/yyyy hh:mm a"),
    total: 0,
    totalPayment: 0,
    payingAmount: 0,
    note: "",
    userId: 0,
    status: "",
    itemDetail: [],
    address: [],
    paymentMethod: [
        {
            method: "PayNow",
            methodStr: "PayNow",
        },
    ],
});
const clearPayload = JSON.stringify(payload.value);
const AddNewReqModal = ref(false);
const addNewItemModal = ref(false);
const DataCustomer = ref(null);
const Customer = ref("");
const QueryString = ref([]);
const ProductsInTable = ref([]);
const user = JSON.parse(localStorage.getItem("user")).appUser;
const isSuccessResponse = (res) => res?.status >= 200 && res?.status < 300;

const GetCustomer = async (key) => {
    isLoading.value = true;
    try {
        const res = await API.get(`Customer?search=${key}`);
        if (res.data)
            DataCustomer.value = res.data.items.filter((cus) => cus.status != "D");
    } catch (error) {
    } finally {
        isLoading.value = false;
    }
};

const openAddNewReqDialog = () => {
    payload.value = JSON.parse(clearPayload);
    if (authStore.userType === "APSP") Customer.value = "";
    submited.value = false;
    AddNewReqModal.value = true;
};

const confrimContinue = async () => {
    const mapAddress = (addresses) => addresses?.filter(el => el.default === "Y")
        .map(el => ({
            cccd: el.cccd,
            address: el.address,
            vehiclePlate: el.vehiclePlate,
            locationId: el.locationId,
            locationName: el.locationName,
            areaId: el.areaId,
            areaName: el.areaName,
            email: el.email,
            phone: el.phone,
            person: el.person,
            note: el.note,
            type: el.type,
        })) || [];
    const parseDeliveryTime = (deliveryTime) => {
        if (typeof deliveryTime !== "string" || !deliveryTime.includes("/"))
            return deliveryTime;

        const [datePart, timePart] = deliveryTime.split(" ");
        const [day, month, year] = datePart.split("/").map(Number);
        if (!timePart)
            return new Date(Date.UTC(year, month - 1, day, 0, 0)).toISOString();
        const [time, period] = timePart.split(" ");
        const [hours, minutes] = time.split(":").map(Number);
        let hour24 = hours;
        if (period?.toLowerCase() === "pm" && hour24 < 12) hour24 += 12;
        if (period?.toLowerCase() === "am" && hour24 === 12) hour24 = 0;

        return new Date(Date.UTC(year, month - 1, day, hour24, minutes)).toISOString();
    };
    const mapItemDetails = (products) => products.map(val => ({
        type: "",
        itemId: val.id,
        itemCode: val.itemCode,
        onHand: val.onHand,
        itemName: val.itemName,
        quantity: val.quantity,
        price: 0,
        priceAfterDist: 0,
        discount: val.discount,
        distcountAmount: val.distcountAmount,
        vatCode: val.vatCode,
        vatAmount: val.vatAmount,
        lineTotal: val.lineTotal,
        note: val.note,
        ouomId: 0,
        uomCode: val.ougp?.ouom.uomCode,
        uomName: val.ougp?.ouom.uomName,
        numInSale: 0,
    }));
    payload.value.address = mapAddress(Customer.value.crD1);
    payload.value.deliveryTime = parseDeliveryTime(payload.value.deliveryTime);
    const data = {
        ...payload.value,
        docDate: new Date(),
        cardId: Customer.value.id,
        cardCode: Customer.value.cardCode,
        cardName: Customer.value.cardName,
        total: 0,
        itemDetail: mapItemDetails(selectedProduct.value),
    };
    if (!validateData(data)) return;
    isLoading.value = true;
    submited.value = true;
    try {
        const res = await API.add(`PurchaseRequest/add`, data);
        if (isSuccessResponse(res)) {
            FunctionGlobal.$notify("S", t('Custom.addnewSuccess'), toast);
            AddNewReqModal.value = false;
        } else {
            throw res;
        }
    } catch (error) {
        if (isSuccessResponse(error?.response)) {
            FunctionGlobal.$notify("S", t('Custom.addnewSuccess'), toast);
            AddNewReqModal.value = false;
            return;
        }
        console.error(error);
    } finally {
        isLoading.value = false;
        emits("fetchData");
    }
};

const validateData = (data) => {
    if (typeof Customer.value != "object") {
        FunctionGlobal.$notify("E", t('Custom.pleaseSelectCustomer'), toast);
        return false;
    }
    if (!data.deliveryTime) {
        FunctionGlobal.$notify("E", t('Custom.pleaseSelectPickupTime'), toast);
        return false;
    }
    if (data.itemDetail.length < 1) {
        FunctionGlobal.$notify("E", t('Custom.pleaseAddGoods'), toast);
        return false;
    }

    return true;
};

const GetProducts = async () => {
    isLoading.value = true;
    try {
        const res = await API.get(`Item?${QueryString.value}`);
        ProductsInTable.value = res.data.items;
    } catch (error) {
        console.error(error);
    } finally {
        isLoading.value = false;
    }
};

const confirmSelectedProd = () => {
    selectedProduct.value.forEach((selectedProd) => {
        const existingItem = payload.value.itemDetail.find(
            (item) => item.id === selectedProd.id
        );
        if (existingItem) {
            existingItem.quantity += selectedProd.quantity;
        } else {
            payload.value.itemDetail.push({ ...selectedProd });
        }
    });
    selectedProduct.value = [];
    addNewItemModal.value = false;
};

const getQueryString = (e) => {
    QueryString.value = e;
    GetProducts();
};

const removeItemSelected = (data) => {
    selectedProduct.value.splice(data, 1);
};
// const GetDataAddress = (event) => {
//   payload.value.address.push({ ...event })
// };
const onQuantityChange = (data) => {
    const productIndex = selectedProduct.value.findIndex((el) => el === data);
    if (data.quantity === 0 && productIndex !== -1) {
        selectedProduct.value.splice(productIndex, 1);
        return;
    }
    if (productIndex === -1 && data.quantity != 0) {
        selectedProduct.value.push(data);
    }
};

import { useMeStore } from "../Pinia/me";
const meData = useMeStore();
const GetMe = async () => {
    const me = await meData.getMe();
    Customer.value = me?.user?.bpInfo;
    // API.get(`Account/me`).then((res) => {
    //     if (res.data) {
    //         Customer.value = res.data.user.bpInfo;
    //     }
    // });
};
const updateSelectedProduct = (e) => {
    e.forEach((data) => {
        if (selectedProduct.value.length == 0) {
            selectedProduct.value.push({ ...data });
        } else {
            const existingItem = selectedProduct.value.find((el) => {
                return el.itemCode === data.itemCode;
            });
            if (existingItem) {
                const newQuantity = existingItem.quantity + data.quantity;
                if (newQuantity <= data.onHand) {
                    existingItem.quantity = newQuantity;
                } else {
                    existingItem.quantity = data.onHand;
                }
            } else {
                selectedProduct.value.push({ ...data });
            }
        }
    });
};
onMounted(async () => {
    if (store?.state?.auth?.user?.appUser?.userType !== "APSP") {
        await GetMe();
    }
});
watch(AddNewReqModal, (newVal) => {
    if (newVal === false) {
        payload.value = JSON.parse(clearPayload);
        selectedProduct.value = [];
    }
});
watch(Customer, () => {
    selectedProduct.value = [];
});
</script>
<style scoped></style>
