<script setup>
import { ref, onBeforeMount, onMounted, computed } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import API from '@/api/api-main';
import format from '../../../../../helpers/format.helper';
import StepHistory from '@/components/StepHistory.vue';
import { useAuthStore } from '@/Pinia/auth';
import { useI18n } from 'vue-i18n';
import DanhGiaDonHang from '@/views/common/Order/components/DanhGiaDonHang.vue';

const ratingDialogVisible = ref(false);
const { t } = useI18n();
const authStore = useAuthStore();
const TimelineData = ref([
    {
        time: '2:10 21/10/2024',
        body: {
            title: t('body.OrderList.createOder'),
            description: 'Công ty cổ phần công nghệ FOXAI đã tạo đơn đặt hàng'
        }
    },
    {
        time: '2:06 21/10/2024',
        body: {
            title: t('body.status.DXL') || t('body.status.pending'),
            description: t('body.status.DXL') ? 'Đơn hàng đang được APSP xử lý' : 'Processing'
        }
    }
]);
const Route = useRoute();
const DataProduct = ref({});
const isLoading = ref(false);
const discardModal = ref(false);
const Customer = ref({});
const editCTGH = ref(false);
onBeforeMount(() => {
    fetchOrderReqDetail();
});
onMounted(() => {
    if (authStore.userType !== 'APSP') GetMe();
});

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
const fetchOrderReqDetail = async () => {
    isLoading.value = true;
    try {
        const res = await API.get(`PurchaseRequest/${Route.params.id}`);
        if (res.data) {
            DataProduct.value = res.data.item;
            isLoading.value = false;
        }
    } catch (error) {
        console.error(error);
    }
};
const formatNumber = (num) => {
    if (Intl.NumberFormat().format(num) == 'NaN') return 0;
    return Intl.NumberFormat().format(num);
};
const openDiscardDialog = () => {
    discardModal.value = true;
};
let stt = {
    DXL: {
        class: 'text-yellow-500',
        label: t('body.status.DXL')
    },
    DXN: {
        class: 'text-green-500',
        label: t('body.status.DXN')
    },
    HUY: {
        class: 'text-red-500',
        label: t('body.status.HUY')
    },
    DGH: {
        class: 'text-blue-500',
        label: t('body.status.DGH')
    },
    DHT: {
        class: 'text-green-500',
        label: t('body.status.DHT')
    }
};
const formatStatus = (data) => {
    return (
        stt[data] || {
            class: 'text-yellow-500',
            label: t('body.status.DXL')
        }
    );
};
const Address = computed(() => {
    return Customer.value.crD1?.find((el) => el.type === 'S' && el.default === 'Y');
});

const onClickDownloadFile = (data) => {
    let a = document.createElement('a');
    a.href = data.filePath;
    a.target = '_blank';
    a.download = data.fileName;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
};
</script>
<template>
    <div v-if="DataProduct">
        <div class="grid align-items-center">
            <div class="col-12">
                <div class="flex gap-2 justify-content-between">
                    <h4 class="m-0 font-bold">{{ t('client.purchase_request') }} - {{ DataProduct.invoiceCode }}</h4>
                    <div class="flex gap-2">
                        <Button v-if="DataProduct.status == '' || DataProduct.status == 'DXL'" @click="openDiscardDialog()" :tooltip="t('body.PurchaseRequestList.cancel_button')" icon="pi pi-trash" outlined raised />
                        <Button v-if="DataProduct.status == '' || DataProduct.status == 'DXL'" :tooltip="t('client.edit')" icon="pi pi-pencil" outlined raised />
                        <Button :tooltip="t('promotion.copy_button')" icon="pi pi-copy" outlined raised />
                        <router-link to="/client/setup/purchase-request-list">
                            <Button :tooltip="t('body.home.back_button')" outlined icon="pi pi-replay" iconPos="left" raised />
                        </router-link>
                        <Button icon="pi pi-star" outlined raised @click="ratingDialogVisible = true" v-if="DataProduct.status == 'DHT' && DataProduct.ratings?.length == 0" />
                    </div>
                </div>
            </div>
        </div>
        <div v-if="DataProduct.status === 'HUY'" class="cancel-notification mb-2">
            <div class="cancel-card bg-red-100">
                <div class="cancel-header">
                    <h3 class="cancel-title">{{ t('CancelOrder.title') }}</h3>
                </div>
                <div class="cancel-content">
                    <p class="reason-text">
                        <strong>{{ t('ChangePoint.reason') }}:</strong>
                        <i>{{ DataProduct.reasonForCancellation }}</i>
                    </p>
                </div>
            </div>
        </div>
        <div class="grid">
            <div class="col-12">
                <div class="card">
                    <div class="grid justify-content-between">
                        <div class="col-8">
                            <div class="flex flex-column gap-3">
                                <div class="flex align-items-center gap-3">
                                    <span>{{ t('client.request_code') }}:</span><strong>{{ DataProduct.invoiceCode }}</strong>
                                </div>
                                <div class="flex align-items-center gap-3">
                                    <span>{{ t('body.OrderList.customer_label') }}:</span><strong>{{ DataProduct.cardCode ? DataProduct.cardCode : '--' }}</strong>
                                </div>
                                <div class="flex align-items-center gap-3">
                                    <span>{{ t('client.customer_name') }}:</span><span class="font-bold">{{ DataProduct.cardName ? DataProduct.cardName : '--' }}</span>
                                    <KiemTraCongNo :bpId="Customer?.id" :bpName="Customer?.cardName" :payCredit="0" :payGuarantee="0" />
                                </div>
                            </div>
                        </div>
                        <div class="col-4">
                            <div class="flex flex-column gap-3">
                                <div class="flex gap-3">
                                    <span>{{ t('client.pickup_time') }}:</span><strong>{{ DataProduct.deliveryTime ? format.DateTime(DataProduct.deliveryTime).time + ' ' + format.DateTime(DataProduct.deliveryTime).date : '--' }} </strong>
                                </div>
                                <div class="flex gap-3">
                                    <span>{{ t('client.created_date') }}:</span><strong>{{ format.DateTime(DataProduct.docDate).time + ' ' + format.DateTime(DataProduct.docDate).date }}</strong>
                                </div>
                                <div class="flex gap-3">
                                    <span>{{ t('client.status') }}:</span><span :class="formatStatus(DataProduct.status).class">{{ formatStatus(DataProduct.status).label }}</span>
                                </div>

                                <div class="flex gap-3"></div>
                            </div>
                        </div>

                        <div class="col-12">
                            <DataTable showGridlines :value="DataProduct.itemDetail" tableStyle="min-width: 50rem">
                                <Column header="#">
                                    <template #body="sp">
                                        <span>{{ sp.index + 1 }}</span>
                                    </template>
                                </Column>
                                <Column field="itemCode" :header="t('body.report.table_header_product_code_1')"> </Column>
                                <Column field="itemName" :header="t('body.report.table_header_product_name_1')"> </Column>
                                <Column field="uomName" :header="t('client.unit')"></Column>
                                <Column style="min-width: 100px" :header="t('client.quantity')">
                                    <template #body="sp">
                                        <span>{{ formatNumber(sp.data.quantity) }}</span>
                                    </template>
                                </Column>
                                <!-- <Column style="min-width: 170px" :header="t('body.PurchaseRequestList.quantity_in_warehouse_column')">
                </Column> -->
                            </DataTable>
                        </div>
                        <div class="col-6">
                            <h6>{{ t('client.note') }}</h6>
                            <Textarea v-model="DataProduct.note" class="w-full" disabled></Textarea>
                        </div>
                    </div>
                </div>
            </div>
            <div class="col-12">
                <DocumentsComp :data="DataProduct" />
            </div>

            <div class="col-12">
                <div class="card p-3 border-noround">
                    <DataTable :value="DataProduct.attDocuments || []" showGridlines>
                        <Column header="#" class="w-3rem">
                            <template #body="sp">
                                <span>{{ sp.index + 1 }}</span>
                            </template>
                        </Column>
                        <Column header="Tệp đính kèm" field="fileName" />
                        <Column field="authorName" header="Người tạo" class="w-20rem"></Column>
                        <Column header="Ngày tạo" class="w-10rem">
                            <template #body="sp">
                                {{ format.DateTime(sp.data.uploadFileAt)?.date }}
                            </template>
                        </Column>
                        <Column class="w-3rem">
                            <template #body="sp">
                                <div class="flex gap-2">
                                    <Button :disabled="!sp.data.filePath" @click="onClickDownloadFile(sp.data)" icon="pi pi-download" text />
                                </div>
                            </template>
                        </Column>
                        <template #empty>
                            <div class="p-5 my-5 text-center">
                                {{ t('client.delivery_documents_doc') }}
                            </div>
                        </template>
                        <template #header>
                            <div class="flex justify-content-between align-items-center">
                                <span class="my-2 text-lg font-bold">
                                    {{ t('client.delivery_documents') }}
                                </span>
                            </div>
                        </template>
                    </DataTable>
                </div>
            </div>

            <div class="col-12">
                <div class="card p-3 border-noround">
                    <DanhGiaDonHang v-if="DataProduct.ratings?.length > 0" type="preview" :orderId="DataProduct.id" :initialData="DataProduct.ratings" docType="YCLHG" />
                </div>
            </div>
            <div class="col-12">
                <div class="grid">
                    <div class="col-6">
                        <div class="card m-0 flex flex-column gap-3 h-full">
                            <strong
                                >{{ t('client.customer_info') }}
                                <hr />
                            </strong>
                            <div class="flex flex-column gap-3">
                                <div class="flex gap-3">
                                    <span>{{ t('client.customer_name') }}:</span><strong>{{ Customer.cardName ? Customer.cardName : '--' }}</strong>
                                </div>
                                <div class="flex gap-3">
                                    <span>{{ t('client.tax_code') }}:</span><strong>{{ Customer.licTradNum ? Customer.licTradNum : '--' }} </strong>
                                </div>
                                <div class="flex gap-3">
                                    <span>{{ t('client.phoneNumber') || t('client.phone_number') }}:</span><strong>{{ Customer.phone ? Customer.phone : '--' }} </strong>
                                </div>
                                <div class="flex gap-3">
                                    <span>{{ t('client.email') }}:</span><strong> {{ Customer.email ? Customer.email : '--' }} </strong>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="col-6">
                        <div class="card flex flex-column gap-3 h-full">
                            <strong
                                >{{ t('client.delivery_info') }}
                                <hr />
                            </strong>
                            <div class="flex flex-column gap-3">
                                <div class="flex gap-3">
                                    <span class="w-7rem">{{ t('client.contact_person') }}:</span
                                    ><strong>
                                        {{ Customer.crD1 ? Customer.crD1?.find((el) => el.default === 'Y' && el.type === 'S')?.person : '--' }}
                                    </strong>
                                </div>
                                <div class="flex gap-3">
                                    <span class="w-7rem">{{ t('client.phone_number') }}:</span
                                    ><strong>
                                        {{ Customer.crD1 ? Customer.crD1?.find((el) => el.default === 'Y' && el.type === 'S')?.phone : '--' }}
                                    </strong>
                                </div>
                                <!--  -->
                                <div class="flex gap-3">
                                    <span class="w-7rem">{{ t('body.sampleRequest.customer.id_card_column') }}: </span
                                    ><strong>
                                        {{ Customer.crD1 ? Customer.crD1?.find((el) => el.default === 'Y' && el.type === 'S')?.cccd : '--' }}
                                    </strong>
                                </div>
                                <div class="flex gap-3">
                                    <span class="w-7rem">{{ t('body.sampleRequest.customer.license_plate_column') }}:</span
                                    ><strong>
                                        {{ Customer.crD1 ? Customer.crD1?.find((el) => el.default === 'Y' && el.type === 'S')?.vehiclePlate : '--' }}
                                    </strong>
                                </div>
                                <div class="flex gap-3">
                                    <span class="w-14rem">{{ t('client.delivery_address') }}:</span
                                    ><strong>
                                        {{
                                            Customer.crD1
                                                ? `${Address.address} -
                                        ${Address.locationName} - ${Address.areaName}`
                                                : '--'
                                        }}
                                    </strong>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
    <Dialog v-model:visible="discardModal" modal :header="t('client.confirm')" :style="{ width: '40rem' }">
        <div class="flex align-items-center p-2">
            <strong>{{ t('body.status.confirm_pickup_request') }}</strong>
        </div>
        <template #footer>
            <div class="flex gap-2">
                <Button :label="t('client.cancel')" severity="secondary" />
                <Button :label="t('client.confirm')" severity="danger" />
            </div>
        </template>
    </Dialog>
    <Dialog v-model:visible="ratingDialogVisible" modal :header="t('Evaluate.evaluateOrder')" :style="{ width: '50rem' }">
        <DanhGiaDonHang :type="'vote'" :orderId="DataProduct.id" docType="YCLHG" />
    </Dialog>
    <loading v-if="isLoading"></loading>
</template>

<style scoped>
.cancel-notification {
    margin: 12px 0;
}

.cancel-card {
    background: #fecaca;
    border-radius: 8px;
    padding: 12px;
    border-left: 4px solid #ef4444;
}

.cancel-header {
    display: flex;
    align-items: center;
    gap: 12px;
    margin-bottom: 12px;
}

.cancel-title {
    margin: 0;
    font-size: 16px;
    font-weight: 600;
    color: #7f1d1d;
}
 
</style>
