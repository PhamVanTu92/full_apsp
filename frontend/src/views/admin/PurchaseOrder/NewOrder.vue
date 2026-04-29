<script setup>
import { ref, computed, onBeforeUnmount, watch } from "vue";
import { useRouter } from "vue-router";
import API from "@/api/api-main";
import AddProducts from "../../../components/order/AddProducts.vue";
import PaymentOrder from "../../../components/order/PaymentOrder.vue";
import { usePurchaseOrderStore } from "../../../Pinia/PurchaseOrder";
import { useHierarchyStore } from "../../../Pinia/hierarchyStore";

const purchaseStore = usePurchaseOrderStore();
const filterStore = useHierarchyStore();
onBeforeUnmount(() => {
    filterStore.resetStore();
    purchaseStore.resetStats();
});

const router = useRouter();
// Global services và emit
const emit = defineEmits(["complete"]);
// Các biến trạng thái và dữ liệu
const visibleCustomer = ref(false);
const loading = ref(false);
const DataCustomer = ref(null);
const Customer = ref("");
const selectedProduct = ref([]);
const payload = ref();
const CustomerTem = ref();
const AddProductAction = ref();
const keySreachCus = ref();
const totalCustomer = ref(0);

// Khởi tạo dữ liệu ban đầu
const InitData = () => {
    payload.value = {
        id: 0,
        invoiceCode: "",
        cardId: 0,
        cardCode: "",
        cardName: "",
        isIncoterm: false,
        incotermType: "",
        docDate: new Date(),
        deliveryTime: new Date(),
        discount: 0,
        distcountAmount: 0,
        vatAmount: 0,
        total: 0,
        note: "",
        userId: 1,
        status: "",
        itemDetail: [],
        address: [],
        paymentMethod: [
            {
                paymentMethodID: 1,
                paymentMethodCode: "PayNow",
                paymentMethodName: "Thanh toán ngay",
            },
        ],
        currency: "VND",
    };
};
InitData();

// Nhóm các hàm xử lý khách hàng
const GetCustomer = async (key) => {
    try {
        const res = await API.get(
            `Customer?skip=0&limit=10000&filter=(status=A)&search=${key}`
        );
        if (res.data) {
            DataCustomer.value = res.data.items;
            totalCustomer.value = res.data.total;
        }
    } catch (error) {}
};

const getAllCustomer = async () => {
    visibleCustomer.value = true;
    CustomerTem.value = { ...Customer.value };
    loading.value = true;
    try {
        const res = await API.get("Customer?skip=0&limit=10000&filter=(status=A)");
        if (res.data) {
            DataCustomer.value = res.data.items;
            totalCustomer.value = res.data.total;
        }
    } catch (error) {
    } finally {
        loading.value = false;
    }
};

const CancelCustomer = () => {
    visibleCustomer.value = false;
    Customer.value = { ...CustomerTem.value };
};

const resetData = () => {
    Customer.value = "";
    selectedProduct.value = [];
    AddProductAction.value.Reset();
    InitData();
};

const attachFile = ref([]);
const onSelectAttachFile = (data) => {
    // Add code here to attach file
    attachFile.value = data;
};
// Incoterm 2020
const incotermOption = ref([
    {
        code: "EXW",
        name: "EXW (Ex Works)",
    },
    {
        code: "FCA",
        name: "FCA (Free Carrier)",
    },
    {
        code: "CPT",
        name: "CPT (Carriage Paid To)",
    },
    {
        code: "CIP",
        name: "CIP (Carriage and Insurance Paid To)",
    },
    {
        code: "DAP",
        name: "DAP (Delivered At Place)",
    },
    {
        code: "DPU",
        name: "DPU (Delivered at Place Unloaded)",
    },
    {
        code: "DDP",
        name: "DDP (Delivered Duty Paid)",
    },
    {
        code: "FAS",
        name: "FAS (Free Alongside Ship)",
    },
    {
        code: "FOB",
        name: "FOB (Free On Board)",
    },
    {
        code: "CFR",
        name: "CFR (Cost and Freight",
    },
    {
        code: "CIF",
        name: "CIF (Cost, Insurance, and Freight)",
    },
]);
const onIncotermChange = (e) => {
    if (payload.value.isIncoterm) {
        payload.value.incotermType = incotermOption.value[0].code;
    } else {
        payload.value.incotermType = null;
    }
};

const outputCheckComp = ref();
const onChoseProduct = (items) => {
    outputCheckComp.value?.onChoseProduct(items);
};

const paymentOrderComp = ref();

watch(
    () => Customer.value,
    (value) => {
        if (typeof value !== "object") {
            selectedProduct.value = [];
        }
    }
);

const onChangeCustomer = (e) => {

    purchaseStore.resetStats();
    payload.value.isIncoterm = false;
    payload.value.currency = "VND";
    payload.incotermType = null;
};

const onSelectCustomer = (e) => {

    // purchaseStore.resetStats();
    if (e.value.isInterCom) {
        payload.value.isIncoterm = e.value.isInterCom;
        onIncotermChange();
        payload.value.currency = "USD";
    }
};
</script>
<template>
    <div class="flex justify-content-between align-items-center mb-4">
        <h4 class="font-bold m-0">Tạo mới đơn hàng</h4>
        <ButtonGoBack/>
    </div>
    <div class="grid align-items-start">
        <div class="col-12">
            <div class="flex flex-wrap gap-2">
                <div
                    class="flex flex-column flex-order-0 lg:flex-order-0 gap-2 lg:flex-grow-0 flex-grow-1"
                >
                    <label>Mã đơn hàng</label>
                    <InputText disabled placeholder="Tự động sinh"></InputText>
                </div>
                <div
                    class="flex flex-column flex-order-2 lg:flex-order-1 gap-2 lg:flex-grow-0 flex-grow-1"
                >
                    <label>Khách hàng</label>
                    <InputGroup class="w-30rem">
                        <AutoComplete
                            v-model="Customer"
                            :suggestions="DataCustomer"
                            optionLabel="cardName"
                            @complete="GetCustomer(Customer)"
                            @change="onChangeCustomer"
                            @item-select="onSelectCustomer"
                        >
                            <template #option="data">
                                <div class="flex gap-2 align-items-center">
                                    <Avatar
                                        icon="pi pi-user"
                                        class="mr-2"
                                        size="large"
                                        style="background-color: #ece9fc; color: #2a1261"
                                        shape="circle"
                                    />
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
                        <InputGroupAddon
                            @click="getAllCustomer()"
                            class="cursor-pointer hover:bg-primary-100"
                        >
                            <i class="pi pi-list"></i>
                        </InputGroupAddon>
                    </InputGroup>
                </div>
                <div
                    class="flex flex-column flex-order-1 lg:flex-order-2 gap-2 lg:flex-grow-0 flex-grow-1"
                >
                    <label for="">Thời gian lấy hàng</label>
                    <Calendar
                        showTime
                        hourFormat="12"
                        :minDate="new Date()"
                        v-model="payload.deliveryTime"
                        showIcon
                        iconDisplay="input"
                        placeholder="Chọn thời gian"
                        class="w-14rem"
                    >
                    </Calendar>
                </div>
                <div class="flex-order-5 flex gap-3 ml-3">
                    <div class="flex flex-column gap-2">
                        <label for="">&nbsp;</label>
                        <div class="h-full flex align-items-center">
                            <Checkbox
                                v-model="payload.isIncoterm"
                                @change="onIncotermChange"
                                inputId="incoterm"
                                binary
                                readonly
                            ></Checkbox>
                            <label class="ml-2 cursor-pointer" for="incoterm"
                                >Incoterm 2020</label
                            >
                        </div>
                    </div>
                    <div v-if="payload.isIncoterm" class="flex flex-column gap-2">
                        <label for="">&nbsp;</label>
                        <Dropdown
                            v-model="payload.incotermType"
                            :options="incotermOption"
                            optionLabel="name"
                            optionValue="code"
                        ></Dropdown>
                    </div>
                    <template>{{
                        (payload.currency = !payload.currency ? "VNĐ" : payload.currency)
                    }}</template>
                    <div class="flex flex-column gap-2">
                        <label for="">&nbsp;</label>
                        <Dropdown
                            v-model="payload.currency"
                            :options="['VND', 'USD']"
                        ></Dropdown>
                    </div>
                </div>
            </div>
        </div>
        <div class="col-12 lg:col-9 flex flex-column gap-3 relative">
            <div v-if="0" class="absolute top-0 z-5 mt-3 mr-3 right-0 p-2 flex gap-2">
                <Button label="Đổi hình thức thanh toán" size="small"/>
                <Button  :label="t('body.OrderList.delete')" size="small"/>
            </div>
            <TabView class="card p-0 overflow-hidden"> 
                <TabPanel header="Sản phẩm"> 
                    <AddProducts
                        ref="AddProductAction"
                        v-model:selectedProduct="selectedProduct"
                        :Customer="Customer"
                        :isIncoterm="payload.isIncoterm"
                        :paymentMethodCode="null"
                        :payload="payload"
                        :currency="payload.currency"
                        @attachFile="onSelectAttachFile"
                        @onProductListChange="onChoseProduct"
                    ></AddProducts>
                </TabPanel>
                <TabPanel header="Khách hàng">
                    <div class="card bg-gray-100">
                        <h6 class="text-lg font-semibold">{{t('client.customer_info')}}</h6>
                        <div class="p-fluid">
                            <div class="grid mb-3">
                                <div class="col-12 lg:col-6 pb-0">
                                    <div class="field">
                                        <label>Công ty(Optional)</label>
                                        <InputText
                                            readonly
                                            :value="Customer?.cardName || ''"
                                        />
                                    </div>
                                </div>
                                <div class="col-12 lg:col-6 pb-0">
                                    <div class="field">
                                        <label>{{t('body.sampleRequest.customer.contact_name_label')}}</label>
                                        <InputText
                                            readonly
                                            :value="Customer?.frgnName || ''"
                                        />
                                    </div>
                                </div>
                            </div>
                            <div class="grid">
                                <div class="col-12 lg:col-6 pb-0">
                                    <div class="field">
                                        <label>{{t('body.sampleRequest.customer.email_column')}}</label>
                                        <InputText
                                            readonly
                                            :value="Customer?.email || ''"
                                        />
                                    </div>
                                </div>
                                <div class="col-12 lg:col-6 pb-0">
                                    <div class="field">
                                        <label>{{t('body.systemSetting.phone_label')}}</label>
                                        <InputText
                                            readonly
                                            :value="Customer?.phone || ''"
                                        />
                                    </div>
                                </div>
                            </div>
                            <div class="grid align-items-end">
                                <div class="col-12">
                                    <SearchAddress
                                        :customer="Customer"
                                        :data="Customer?.crD1 || null"
                                    ></SearchAddress>
                                </div>
                            </div>
                        </div>
                    </div>
                </TabPanel>
            </TabView>
        </div>
        <div class="col-12 lg:col-3 flex flex-column gap-3 justify-content-center">
            <!-- {{ Customer }} -->
            <!-- <KiemTraCongNo :bpId="Customer?.id" :bpName="Customer?.cardName" /> -->
            <PaymentOrder
                type="type1"
                v-model:selectedProduct="selectedProduct"
                :payload="payload"
                :Customer="Customer"
                @AddOrder="resetData()"
                :attachFile="attachFile"
                :isClient="false"
                :currency="payload.currency"
                :tienGiamSanLuong="outputCheckComp?.tiemGiam || null"
                ref="paymentOrderComp"
            ></PaymentOrder>
            <OutputCheck
                ref="outputCheckComp"
                :productsSelected="selectedProduct"
                :customer="Customer"
                :payload="payload"
            ></OutputCheck>
        </div>
    </div>
    <Dialog
        v-model:visible="visibleCustomer"
        modal
        position="top"
        :draggable="false"
        header="Danh sách khách hàng"
        :style="{ width: '100rem' }"
        scrollable
        scrollHeight="100%"
        selectionMode="single"
    >
        <DataTable
            v-model:selection="Customer"
            :value="DataCustomer"
            tableStyle="min-width: 50rem"
            selectionMode="single"
            paginator
            :rows="15"
            :page="1"
            scrollable
            scrollHeight="500px"
            :totalRecords="totalCustomer"
            paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
            currentPageReportTemplate="Hiển thị {first} - {last} trên tổng {totalRecords} khách hàng"
        >
            <template #header>
                <div class="flex justify-content-end">
                    <IconField iconPosition="left">
                        <InputText
                            placeholder="Tìm kiếm khách hàng"
                            v-model="keySreachCus"
                            @keyup.enter="GetCustomer(keySreachCus)"
                        />
                        <InputIcon>
                            <i class="pi pi-search" @click="GetCustomer(keySreachCus)" />
                        </InputIcon>
                    </IconField>
                </div>
            </template>
            <template #empty>
                <div class="text-center">
                    <span>{{t('body.promotion.no_matching_result_message')}}</span>
                </div>
            </template>
            <Column selectionMode="single" headerStyle="width: 3rem"></Column>
            <Column field="cardCode" header="Mã khách hàng"></Column>
            <Column field="cardName" header="Tên khách hàng"></Column>
            <Column field="email" header="Email"></Column>
            <Column field="areaName" header="Địa chỉ"></Column>
        </DataTable>
        <template #footer>
            <div class="flex justify-content-end gap-2">
                <Button
                    type="button"
                    :label="t('body.status.HUY2')"
                    severity="secondary"
                    @click="CancelCustomer"
                />
                <Button
                    type="button"
                   :label="t('body.status.XN')"
                    @click="
                        () => {
                            visibleCustomer = false;
                        }
                    "
                />
            </div>
        </template>
    </Dialog>
    <Loading v-if="loading"></Loading>
</template>
<style scoped>
.field {
    margin-bottom: 0;
}

:deep(.p-dropdown.p-component.p-inputwrapper) {
    width: 100%;
}

:deep(.p-autocomplete-input.p-inputtext.p-component) {
    width: 100%;
}

:deep(.p-inputtext.p-component.p-inputnumber-input) {
    width: 100%;
}
</style>
