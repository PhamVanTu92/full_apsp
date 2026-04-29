<template>
    <div>
        <div class="card flex flex-column gap-3">
            <div class="font-bold text-xl">{{ t('body.PurchaseRequestList.create_pickup_request_title') }}</div>
            <div class="card">
                <div class="r-p1 flex justify-content-between align-items-center">
                    <div class="flex flex-wrap gap-2">
                        <div class="r-p2 flex flex-column gap-2">
                            <label for="">{{ t('body.PurchaseRequestList.request_code_label') }}</label>
                            <InputText class="w-12" v-model="payload.invoiceCode" disabled></InputText>
                        </div>
                        <div class="r-p2 flex flex-column gap-2">
                            <label for="">{{ t('body.PurchaseRequestList.pickup_time_label') }} <span class="text-red-500">*</span></label>
                            <Calendar
                                showTime
                                showTimeInput
                                hourFormat="12"
                                v-model="payload.deliveryTime"
                                :minDate="new Date(new Date().getTime())"
                                showIcon
                                iconDisplay="input"
                                dateFormat="dd/mm/yy"
                                :placeholder="t('body.report.from_date_placeholder')"
                            ></Calendar>
                        </div>
                    </div>
                    <div class="mt-4">
                        <!-- <DebtCheck
                            :DebtData="Customer?.crD4"
                            :TotalDebtData="Customer?.crD3"
                            type="icon"
                            :paymentMethod="'PayNow'"
                            :TotalPayment="0"
                            :ItemDebt="null"
                        >
                        </DebtCheck> -->
                        <KiemTraCongNo :bpId="Customer.id"></KiemTraCongNo>
                    </div>
                </div>
                <div class="grid flex flex-column product_table mt-3">
                    <div class="flex flex-column gap-3 card mb-0">
                        <TabView>
                            <TabPanel :header="t('body.PurchaseRequestList.products_tab')">
                                <div class="flex flex-column gap-3">
                                    <DataTable :value="selectedProduct" showGridlines stripedRows>
                                        <template #empty>
                                            <div class="p-2 text-center">
                                                {{ t('body.PurchaseRequestList.no_matching_product_message') }}
                                            </div>
                                        </template>
                                        <Column :header="t('client.serial_number')">
                                            <template #body="slotProps">
                                                {{ slotProps.index + 1 }}
                                            </template>
                                        </Column>
                                        <Column field="itemName" :header="t('body.PurchaseRequestList.product_name_column')"></Column>
                                        <Column field="quantity" :header="t('body.PurchaseRequestList.quantity_column')">
                                            <template #body="slotProps">
                                                <div class="flex justify-content-center">
                                                    <InputNumber
                                                        v-model="slotProps.data.quantity"
                                                        :pt="{
                                                            input: {
                                                                root: {
                                                                    style: 'width: 6rem'
                                                                }
                                                            }
                                                        }"
                                                        :min="1"
                                                        :max="slotProps.data.onHand"
                                                        inputId="horizontal-buttons"
                                                        showButtons
                                                    >
                                                    </InputNumber>
                                                </div>
                                            </template>
                                        </Column>
                                        <Column field="ougp?.packing?.name" :header="t('body.PurchaseRequestList.unit_column')"></Column>
                                        <Column field="onHand" :header="t('body.PurchaseRequestList.quantity_in_warehouse_column')"> </Column>

                                        <Column :header="t('body.PurchaseRequestList.actions_column')">
                                            <template #body="slotProps">
                                                <div class="flex justify-content-center">
                                                    <Button @click="removeItemSelected(slotProps.index)" text icon="pi pi-trash" />
                                                </div>
                                            </template>
                                        </Column>
                                    </DataTable>
                                    <div class="flex justify-content-between md:align-items-center flex-column md:flex-row gap-2">
                                        <ProductSelector :typeModal="'YCLHG'" :customer="Customer" icon="pi pi-plus-circle" :label="t('body.PurchaseRequestList.find_product_button')" outlined @confirm="updateSelectedProduct" type="PurchaseRequest">
                                        </ProductSelector>
                                    </div>
                                </div>
                            </TabPanel>
                            <TabPanel :header="t('body.PurchaseRequestList.shipping_info_tab')">
                                <div class="card bg-gray-100 flex flex-column gap-3">
                                    <strong>{{ t('client.customer_info') }}</strong>
                                    <div class="flex gap-2">
                                        <div class="flex flex-column gap-2 w-full">
                                            <label for="">{{ t('client.business_name') }} <span>(Optional)</span></label>
                                            <InputText class="w-full" readonly :value="typeof Customer === 'object' ? Customer.cardName : ''"> </InputText>
                                        </div>
                                        <div class="flex flex-column gap-2 w-full">
                                            <label for="">{{ t('client.phone_number') }}</label>
                                            <InputText class="w-full" readonly :value="typeof Customer === 'object' ? Customer.phone : ''"> </InputText>
                                        </div>
                                    </div>
                                    <div class="flex flex-column gap-3">
                                        <div class="flex flex-column gap-2">
                                            <label for="">{{ t('client.email') }}</label>
                                            <InputText readonly :value="typeof Customer === 'object' ? Customer.email : ''"></InputText>
                                        </div>
                                        <SearchAddress :type="'YCLHG'" :customer="Customer" :data="Customer ? Customer.crD1 : []"> </SearchAddress>
                                    </div>
                                </div>
                            </TabPanel>
                        </TabView>
                    </div>
                    <div class="flex flex-column gap-2 my-4">
                        <label for="" class="text-lg font-medium text-gray-700">{{ t('body.PurchaseRequestList.note_label') }}</label>
                        <Textarea class="w-full" v-model="payload.note" :placeholder="t('body.PurchaseRequestList.note_placeholder')" cols="24" rows="5"></Textarea>
                    </div>
                </div>
                <div class="r-p3 flex justify-content-end gap-2">
                    <Button @click="router.back()" :label="t('body.PurchaseRequestList.cancel_button')" icon="pi pi-times" outlined />
                    <Button @click="confrimContinue()" :label="t('body.PurchaseRequestList.confirm_button')" icon="pi pi-check" />
                </div>
            </div>
        </div>
    </div>
    <loading v-if="isLoading"></loading>
</template>
<script setup>
import { computed, onMounted, ref } from 'vue';
import API from '@/api/api-main';
import { useGlobal } from '@/services/useGlobal';
const { toast, FunctionGlobal } = useGlobal();
import { useRouter } from 'vue-router';
import { format } from 'date-fns';
import { useI18n } from 'vue-i18n';

const { t } = useI18n();

const router = useRouter();
const emits = defineEmits(['fetchData']);
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
    total: 0,
    docDate: '',
    deliveryTime: format(new Date(), 'dd/MM/yyyy hh:mm a'),
    totalPayment: 0,
    payingAmount: 0,
    note: '',
    userId: 0,
    status: '',
    itemDetail: [],
    address: [],
    paymentMethod: [
        {
            method: 'PayNow',
            methodStr: 'PayNow'
        }
    ]
});
const AddNewReqModal = ref(false);
const addNewItemModal = ref(false);
const DataCustomer = ref(null);
const Customer = ref({});
const QueryString = ref([]);
const ProductsInTable = ref([]);
const isSuccessResponse = (res) => res?.status >= 200 && res?.status < 300;
const dateNow = new Date(new Date().setMinutes(new Date().getMinutes()));
onMounted(() => {
    GetMe();
});

const updateSelectedProduct = (e) => {
    e.forEach((data) => {
        if (selectedProduct.value.length == 0) {
            selectedProduct.value.push({ ...data });
        } else {
            const existingItem = selectedProduct.value.find((el) => {
                return el.itemCode === data.itemCode;
            });
            if (existingItem) {
                existingItem.quantity += data.quantity;
            } else {
                selectedProduct.value.push({ ...data });
            }
        }
    });
};

const convertDate = (input) => {
    let date;

    // Case 1: DD/MM/YYYY hh:mm AM/PM (local time)
    if (typeof input === 'string' && input.includes('/')) {
        const match = input.match(/(\d{2})\/(\d{2})\/(\d{4}) (\d{1,2}):(\d{2}) (AM|PM)/i);

        if (!match) return null;

        let [, dd, mm, yyyy, hh, min, period] = match;

        let hour = parseInt(hh, 10);

        if (period.toUpperCase() === 'PM' && hour !== 12) hour += 12;
        if (period.toUpperCase() === 'AM' && hour === 12) hour = 0;

        // Date được tạo theo LOCAL timezone (GMT+7)
        date = new Date(Number(yyyy), Number(mm) - 1, Number(dd), hour, Number(min), 0);
    }
    // Case 2: Date string chuẩn
    else {
        date = new Date(input);
    }

    if (isNaN(date.getTime())) return null;

    return date.toISOString();
};
const confrimContinue = async () => {
    const mapAddress =
        Customer.value.crD1
            ?.filter((el) => el.default === 'Y')
            .map((el) => ({
                address: el.address || '',
                locationId: el.locationId || 0,
                locationName: el.locationName || '',
                areaId: el.areaId || 0,
                areaName: el.areaName || '',
                email: el.email || '',
                phone: el.phone || '',
                person: el.person || '',
                note: el.note || '',
                type: el.type || '',
                cccd: el.cccd || '',
                vehiclePlate: el.vehiclePlate || ''
            })) || [];
    const { value: products } = selectedProduct;
    const data = {
        ...payload.value,
        address: mapAddress,
        docDate: new Date(new Date().setDate(new Date().getDate())),
        deliveryTime: convertDate(payload.value.deliveryTime),
        cardId: Customer.value.id,
        cardCode: Customer.value.cardCode,
        cardName: Customer.value.cardName,
        total: 0,
        itemDetail: products.map((val) => ({
            type: '',
            itemId: val.id,
            itemCode: val.itemCode,
            itemName: val.itemName,
            quantity: val.quantity,
            onHand: val.onHand,
            price: val.price,
            priceAfterDist: 0,
            discount: val.discount,
            distcountAmount: val.distcountAmount,
            vatCode: val.vatCode,
            vatAmount: val.vatAmount,
            lineTotal: val.lineTotal,
            note: val.note,
            ouomId: 0,
            uomCode: val.ougp?.ouom?.uomCode ?? val.packing?.code ?? '',
            uomName: val.ougp?.ouom?.uomName ?? val.packing?.name ?? '',
            numInSale: 0
        }))
    };

    if (!validateData(data)) return;
    submited.value = true;
    isLoading.value = true;
    try {
        const res = await API.add(`PurchaseRequest/add`, data);
        if (isSuccessResponse(res)) {
            FunctionGlobal.$notify('S', t('Custom.addnewSuccess'), toast);
            AddNewReqModal.value = false;
            if (res.data?.id) {
                router.push(`success?type=request&id=${res.data.id}`);
            } else {
                router.push({ name: 'purchase-request-list' });
            }
        } else {
            throw res;
        }
    } catch (error) {
        if (isSuccessResponse(error?.response)) {
            FunctionGlobal.$notify('S', t('Custom.addnewSuccess'), toast);
            AddNewReqModal.value = false;
            router.push({ name: 'purchase-request-list' });
            return;
        }
        console.error(error);
    } finally {
        isLoading.value = false;
        emits('fetchData');
    }
};
const validateData = (data) => {
    if (data.itemDetail.length < 1) {
        FunctionGlobal.$notify('E', t('Custom.addnewErrorGoods'), toast);
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

const getQueryString = (e) => {
    QueryString.value = e;
    GetProducts();
};
const removeItemSelected = (data) => {
    selectedProduct.value.splice(data, 1);
};

const GetMe = async () => {
    try {
        const res = await API.get(`Account/me`);
        if (res.data) {
            Customer.value = res.data.user.bpInfo;
        }
    } catch (error) {
        console.error(error);
    }
};
</script>

<style scoped>
@media only screen and (max-width: 640px) {
    .r-p1 {
        display: flex;
        flex-wrap: wrap;
        gap: 1rem;
    }

    .r-p2 {
        display: flex;
        flex-wrap: wrap;
    }

    .r-p3 {
        margin-top: 1.5rem;
    }

    .product_table {
        display: flex;
        flex-direction: column;
        gap: 2rem;
    }
}

/* Màn hình di động lớn / máy tính bảng nhỏ (641px - 1023px) */
@media only screen and (min-width: 641px) and (max-width: 1023px) {
    .r-p1 {
        display: flex;
        flex-wrap: wrap;
        gap: 1rem;
    }

    .r-p2 {
        display: flex;
        flex-wrap: wrap;
    }

    .r-p3 {
        margin-top: 1.5rem;
    }

    .product_table {
        display: flex;
        flex-direction: column;
        gap: 2rem;
    }

    .col-9,
    .col-3 {
        width: 100%;
    }

    .col-3 {
        order: 2;
    }
}

/* Máy tính bảng hoặc màn hình nhỏ (≥ 1024px) */
@media only screen and (min-width: 1024px) and (max-width: 1279px) {
    .col-9 {
        width: 65%;
        /* Điều chỉnh kích thước cột */
    }

    .col-3 {
        width: 35%;
    }
}

/* Máy tính bảng hoặc màn hình nhỏ (≥ 1280px) */
@media only screen and (min-width: 1280px) and (max-width: 1535px) {
}

/* Màn hình lớn (≥ 1536px) */
@media only screen and (min-width: 1536px) and (max-width: 1920px) {
}

/* Màn hình rất lớn (≥ 1921px) */
@media only screen and (min-width: 1921px) and (max-width: 2560px) {
}

/* Màn hình 4K (≥ 2561px) */
@media only screen and (min-width: 2561px) {
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

.sticky-table {
    position: sticky;
    top: 5px;
    z-index: 10;
    padding: 10px;
}
</style>
